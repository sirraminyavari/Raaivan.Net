using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Drawing;
using System.Collections;

namespace RaaiVan.Modules.GlobalUtilities
{
    public enum FileOwnerTypes
    {
        None,
        Node,
        FormElement,
        Message,
        Wiki,
        WikiContent,
        WorkFlow,
        PDFCover
    }

    public enum FolderNames
    {
        TemporaryFiles,
        Attachments,
        ProfileImages,
        PDFImages,
        PDFCovers,
        HighQualityProfileImage,
        CoverPhoto,
        HighQualityCoverPhoto,
        Icons,
        HighQualityIcon,
        ApplicationIcons,
        HighQualityApplicationIcon,
        Index,
        Themes,
        WikiContent,
        Pictures,
        EmailTemplates
    }
    
    public enum DefaultIconTypes
    {
        None,
        Node,
        Document,
        Extension
    }

    public enum IconType {
        None,
        ProfileImage,
        CoverPhoto,
        Icon,
        ApplicationIcon
    }

    public class DocumentUtilities
    {
        protected static DocFileInfo _get_file_info(Dictionary<string, object> dic)
        {
            if (dic == null) return null;

            Guid? fileId = !dic.ContainsKey("FileID") ? null : PublicMethods.parse_guid(dic["FileID"].ToString());
            string extension = !dic.ContainsKey("Extension") ? null : PublicMethods.parse_string(dic["Extension"].ToString());
            string fileName = !dic.ContainsKey("FileName") ? null : PublicMethods.parse_string(dic["FileName"].ToString());
            long? size = !dic.ContainsKey("Size") ? null : PublicMethods.parse_long(dic["Size"].ToString());
            Guid? ownerId = !dic.ContainsKey("OwnerID") ? null : PublicMethods.parse_guid(dic["OwnerID"].ToString());

            FileOwnerTypes ownerType = FileOwnerTypes.None;
            if (dic.ContainsKey("OwnerType")) Enum.TryParse<FileOwnerTypes>(dic["OwnerType"].ToString(), true, out ownerType);

            DocFileInfo fi = new DocFileInfo()
            {
                FileID = fileId,
                FileName = fileName,
                Extension = extension,
                Size = size
            };

            if (ownerId.HasValue && ownerId != Guid.Empty) fi.OwnerID = ownerId;
            if (ownerType != FileOwnerTypes.None) fi.OwnerType = ownerType;

            return !fileId.HasValue ? null : fi;
        }

        public static List<DocFileInfo> get_files_info(string strFiles)
        {
            List<DocFileInfo> retList = new List<DocFileInfo>();

            Dictionary<string, object> dic = PublicMethods.fromJSON("{\"Items\":" + Base64.decode(strFiles) + "}");

            if (!dic.ContainsKey("Items")) return retList;

            if (dic["Items"].GetType() == typeof(Dictionary<string, object>)) {
                DocFileInfo fi = _get_file_info((Dictionary<string, object>)dic["Items"]);
                if (fi != null) retList.Add(fi);
            }
            else if (dic["Items"].GetType() == typeof(ArrayList))
            {
                foreach (object obj in (ArrayList)dic["Items"])
                {
                    Dictionary<string, object> f = obj.GetType() == typeof(string) ? PublicMethods.fromJSON((string)obj) :
                        (obj.GetType() == typeof(Dictionary<string, object>) ? (Dictionary<string, object>)obj : null);

                    DocFileInfo fi = f == null ? null : _get_file_info(f);
                    if (fi != null) retList.Add(fi);
                }
            }

            return retList;
        }
        
        public static string get_files_json(Guid applicationId, List<DocFileInfo> attachedFiles, bool icon = false)
        {
            return attachedFiles == null || attachedFiles.Count == 0 ? "[]" : 
                "[" + string.Join(",", attachedFiles.Select(u => u.toJson(applicationId, icon))) + "]";
        }

        private static bool _store_file(Guid applicationId, byte[] fileContent, DocFileInfo file, FolderNames folderName)
        {
            try
            {
                using (FileStream fs = new FileStream(file.get_address(applicationId, folderName), FileMode.CreateNew))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(fileContent);
                    }
                }

                encrypt_file_if_needed(applicationId, file, folderName);

                return true;
            }
            catch { return false; }
        }
        
        public static DocFileInfo decode_base64_file_content(Guid applicationId, Guid? ownerId,
            string base64FileContent, FileOwnerTypes ownerType)
        {
            if (string.IsNullOrEmpty(base64FileContent)) return null;

            byte[] theData = null;

            try { theData = Convert.FromBase64String(base64FileContent); }
            catch { return null; }

            int FIXED_HEADER = 16;

            DocFileInfo ret = new DocFileInfo()
            {
                FileID = Guid.NewGuid(),
                OwnerID = ownerId,
                OwnerType = ownerType
            };

            try
            {
                using (MemoryStream ms = new MemoryStream(theData))
                {
                    using (BinaryReader theReader = new BinaryReader(ms))
                    {
                        //Position the reader to get the file size.
                        byte[] headerData = new byte[FIXED_HEADER];
                        headerData = theReader.ReadBytes(headerData.Length);

                        ret.Size = (int)theReader.ReadUInt32();
                        int fileNameLength = (int)theReader.ReadUInt32() * 2;

                        if (fileNameLength <= 0 || fileNameLength > 255) throw new Exception("what the fuzz!!");

                        byte[] fileNameBytes = theReader.ReadBytes(fileNameLength);
                        //InfoPath uses UTF8 encoding.
                        Encoding enc = Encoding.Unicode;
                        string fullFileName = enc.GetString(fileNameBytes, 0, fileNameLength - 2);

                        int dotIndex = fullFileName.LastIndexOf(".");
                        if (dotIndex > 0 && dotIndex < (fullFileName.Length - 1))
                            ret.Extension = fullFileName.Substring(dotIndex + 1);

                        ret.FileName = string.IsNullOrEmpty(ret.Extension) ?
                            fullFileName : fullFileName.Substring(0, dotIndex);

                        byte[] fileBytes = theReader.ReadBytes((int)ret.Size.Value);

                        if (!_store_file(applicationId, fileBytes, ret, FolderNames.TemporaryFiles)) return null;
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                //maybe the file is a base64 image!!
                try
                {
                    Image img = PublicMethods.image_from_byte_array(theData);
                    if (img == null) return null;
                    byte[] imageBytes = PublicMethods.image_to_byte_array(img, System.Drawing.Imaging.ImageFormat.Jpeg);
                    if (imageBytes == null || imageBytes.Length == 0) return null;

                    ret.Size = imageBytes.Length;
                    ret.FileName = "img";
                    ret.Extension = "jpg";

                    if (!_store_file(applicationId, imageBytes, ret, FolderNames.TemporaryFiles)) return null;

                    return ret;
                }
                catch { return null; }
            }
        }

        public static FolderNames get_folder_name(FileOwnerTypes ownerType)
        {
            switch (ownerType)
            {
                case FileOwnerTypes.Node:
                case FileOwnerTypes.Wiki:
                case FileOwnerTypes.Message:
                case FileOwnerTypes.WorkFlow:
                case FileOwnerTypes.FormElement:
                    return FolderNames.Attachments;
                case FileOwnerTypes.WikiContent:
                    return FolderNames.WikiContent;
                case FileOwnerTypes.PDFCover:
                    return FolderNames.PDFCovers;
                default:
                    return FolderNames.TemporaryFiles;
            }
        }

        private static string _get_folder_path(Guid? applicationId, FolderNames folderName)
        {
            bool isAppLogo = folderName == FolderNames.ApplicationIcons || folderName == FolderNames.HighQualityApplicationIcon;
            bool isProfileImage = folderName == FolderNames.ProfileImages || folderName == FolderNames.HighQualityProfileImage;

            if (isAppLogo || (RaaiVanSettings.SAASBasedMultiTenancy && isProfileImage)) applicationId = null;

            string applicationPart = !applicationId.HasValue ? string.Empty : applicationId.Value.ToString() + "/";

            switch (folderName)
            {
                case FolderNames.Attachments:
                case FolderNames.WikiContent:
                case FolderNames.Index:
                case FolderNames.TemporaryFiles:
                case FolderNames.Pictures:
                case FolderNames.PDFImages:
                case FolderNames.PDFCovers:
                    return "App_Data/" + applicationPart + "Documents/" + folderName.ToString();
                case FolderNames.Icons:
                case FolderNames.ApplicationIcons:
                case FolderNames.ProfileImages:
                case FolderNames.CoverPhoto:
                    return "Global_Documents/" + applicationPart + folderName.ToString();
                case FolderNames.HighQualityIcon:
                    return "Global_Documents/" + applicationPart + FolderNames.Icons.ToString() + "/" + "HighQuality";
                case FolderNames.HighQualityApplicationIcon:
                    return "Global_Documents/" + applicationPart + FolderNames.ApplicationIcons.ToString() + "/" + "HighQuality";
                case FolderNames.HighQualityProfileImage:
                    return "Global_Documents/" + applicationPart + FolderNames.ProfileImages.ToString() + "/" + "HighQuality";
                case FolderNames.HighQualityCoverPhoto:
                    return "Global_Documents/" + applicationPart + FolderNames.CoverPhoto.ToString() + "/" + "HighQuality";
                case FolderNames.Themes:
                    return "CSS/" + folderName.ToString();
                case FolderNames.EmailTemplates:
                    return "App_Data/" + applicationPart + folderName.ToString();
                default:
                    return string.Empty;
            }
        }

        public static string get_sub_folder(string guidName, bool clientPath = false)
        {
            return guidName[0].ToString() + guidName[1].ToString() + (clientPath ? "/" : "\\") + guidName[2].ToString();
        }
        
        public static string get_sub_folder(Guid fileId, bool clientPath = false)
        {
            return get_sub_folder(fileId.ToString(), clientPath);
        }

        public static bool has_sub_folder(FolderNames folderName)
        {
            return !(folderName == FolderNames.EmailTemplates || folderName == FolderNames.Index ||
                folderName == FolderNames.TemporaryFiles || folderName == FolderNames.Themes);
        }

        public static string map_path(Guid? applicationId, FolderNames folderName, string dest = null)
        {
            return PublicMethods.map_path("~/" + _get_folder_path(applicationId, folderName)) +
                (string.IsNullOrEmpty(dest) ? string.Empty : (dest[0] == '\\' ? string.Empty : "\\") + dest);
        }
        
        public static string map_path(string path)
        {
            return PublicMethods.map_path("~" + (path[0] == '/' ? string.Empty : "/") + path);
        }
        
        public static string get_client_path(Guid? applicationId, FolderNames folderName, string dest = null)
        {
            return "../../" + _get_folder_path(applicationId, folderName) +
                (string.IsNullOrEmpty(dest) ? string.Empty : (dest[0] == '/' ? string.Empty : "/") + dest);
        }

        public static string get_personal_image_address(Guid? applicationId, 
            Guid userId, bool networkAddress = false, bool highQuality = false)
        {
            if (RaaiVanSettings.SAASBasedMultiTenancy) applicationId = null;

            if (userId == Guid.Empty)
            {
                string addr = "../../Images/unknown.jpg";

                return highQuality ? string.Empty :
                    (networkAddress ? addr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : addr);
            }

            FolderNames folderName = highQuality ? FolderNames.HighQualityProfileImage : FolderNames.ProfileImages;

            string serverSubFolder = get_sub_folder(userId);
            string clientSubFolder = get_sub_folder(userId, true);

            string serverAddress = map_path(applicationId, folderName) +
                "\\" + serverSubFolder + "\\" + userId.ToString() + ".jpg";

            string address = !File.Exists(serverAddress) ?
                (highQuality ? string.Empty : "../../Images/unknown.jpg") :
                get_client_path(applicationId, folderName) + "/" + clientSubFolder + "/" + userId.ToString() + ".jpg";

            return networkAddress ? address.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : address;
        }
        
        public static bool picture_exists(Guid applicationId, Guid pictureId)
        {
            if (pictureId == Guid.Empty) return false;

            return File.Exists(map_path(applicationId, FolderNames.Pictures) +
                "\\" + get_sub_folder(pictureId, false) + "\\" + pictureId.ToString() + ".jpg");
        }

        public static bool get_icon_parameters(IconType iconType, ref int width, ref int height, 
            ref int highQualityWidth, ref int highQualityHeight, ref FolderNames folder, ref FolderNames highQualityFolder) {
            switch (iconType)
            {
                case IconType.ProfileImage:
                    width = height = 100;
                    highQualityWidth = highQualityHeight = 600;
                    folder = FolderNames.ProfileImages;
                    highQualityFolder = FolderNames.HighQualityProfileImage;
                    break;
                case IconType.CoverPhoto:
                    width = 900;
                    height = 220;
                    highQualityWidth = 1800;
                    highQualityHeight = 600;
                    folder = FolderNames.CoverPhoto;
                    highQualityFolder = FolderNames.HighQualityCoverPhoto;
                    break;
                case IconType.Icon:
                    width = height = 100;
                    highQualityWidth = highQualityHeight = 600;
                    folder = FolderNames.Icons;
                    highQualityFolder = FolderNames.HighQualityIcon;
                    break;
                case IconType.ApplicationIcon:
                    width = height = 100;
                    highQualityWidth = highQualityHeight = 600;
                    folder = FolderNames.ApplicationIcons;
                    highQualityFolder = FolderNames.HighQualityApplicationIcon;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static bool get_icon_parameters(IconType iconType, ref int width, ref int height,
            ref FolderNames folder, ref FolderNames highQualityFolder)
        {
            int highQualityWidth = 0, highQualityHeight = 0;
            return get_icon_parameters(iconType, ref width, ref height,
                ref highQualityWidth, ref highQualityHeight, ref folder, ref highQualityFolder);
        }

        public static string get_icon_url(Guid applicationId, DefaultIconTypes defaultIcon, 
            string extension = "", bool networkAddress = false)
        {
            string adr = string.Empty;

            switch (defaultIcon)
            {
                case DefaultIconTypes.Document:
                    adr = "../../images/archive.png";
                    break;
                case DefaultIconTypes.Extension:
                    adr = "../../images/extensions/" + extension + ".png";
                    string path = PublicMethods.map_path("~/images/extensions") + "\\" + extension + ".png";
                    adr = File.Exists(path) ? adr : "../../images/archive.png";
                    break;
                default:
                    adr = "../../images/Preview.png";
                    break;
            }

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }
        
        public static string get_icon_url(Guid applicationId, Guid ownerId, 
            DefaultIconTypes defaultIcon = DefaultIconTypes.Node, Guid? alternateOwnerId = null, bool networkAddress = false)
        {
            string adr = string.Empty;

            if (ownerId == Guid.Empty) return string.Empty;

            FolderNames folderName = FolderNames.Icons;
            string subFolder = get_sub_folder(ownerId);
            string alternateSubFolder = alternateOwnerId.HasValue ? get_sub_folder(alternateOwnerId.Value) : string.Empty;

            string serverAddress = map_path(applicationId, folderName) + 
                "\\" + subFolder + "\\" + ownerId.ToString() + ".jpg";

            string alternatePath = !alternateOwnerId.HasValue ? string.Empty : map_path(applicationId, folderName) + 
                "\\" + alternateSubFolder + "\\" + alternateOwnerId.ToString() + ".jpg";

            subFolder = get_sub_folder(ownerId, true);
            alternateSubFolder = alternateOwnerId.HasValue ? get_sub_folder(alternateOwnerId.Value, true) : string.Empty;

            adr = File.Exists(serverAddress) ? 
                get_client_path(applicationId, folderName) + "/" + subFolder + "/" + ownerId.ToString() + ".jpg" :
                (!string.IsNullOrEmpty(alternatePath) && File.Exists(alternatePath) ?
                get_client_path(applicationId, folderName) + "/" + alternateSubFolder + 
                "/" + alternateOwnerId.Value.ToString() + ".jpg" :
                (defaultIcon == DefaultIconTypes.None ? string.Empty : get_icon_url(applicationId, defaultIcon)));

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }
        
        public static string get_icon_url(Guid applicationId, Guid ownerId, string extension,
            bool highQuality = false, bool networkAddress = false)
        {
            string adr = string.Empty;

            if (ownerId == Guid.Empty) return string.Empty;

            FolderNames folderName = highQuality ? FolderNames.HighQualityIcon : FolderNames.Icons;

            string serverSubFolder = get_sub_folder(ownerId);
            string clientSubFolder = get_sub_folder(ownerId, true);

            string serverAddress = map_path(applicationId, folderName) + 
                "\\" + serverSubFolder + "\\" + ownerId.ToString() + ".jpg";

            adr = File.Exists(serverAddress) ? 
                get_client_path(applicationId, folderName) + "/" + clientSubFolder + "/" + ownerId.ToString() + ".jpg" :
                (highQuality ? string.Empty : get_icon_url(applicationId, DefaultIconTypes.Extension, extension));

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }
        
        public static string get_icon_url(Guid applicationId, string fileExtention, bool networkAddress = false)
        {
            string url = "images/extensions/" + fileExtention + ".png";
            string adr = File.Exists(map_path(url)) ? "../../" + url : string.Empty;

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }
        
        public static bool icon_exists(Guid applicationId, Guid ownerId)
        {
            return File.Exists(map_path(applicationId, FolderNames.Icons) +
                "\\" + get_sub_folder(ownerId) + "\\" + ownerId.ToString() + ".jpg");
        }
        
        public static string get_icon_json(Guid applicationId, Guid ownerId)
        {
            return new DocFileInfo()
            {
                FileID = ownerId,
                FileName = "آیکون",
                Extension = "jpg",
                OwnerID = ownerId
            }.toJson(applicationId);
        }

        public static string get_application_icon_url(Guid applicationId, bool highQuality = false, bool networkAddress = false)
        {
            string adr = string.Empty;

            FolderNames folderName = highQuality ? FolderNames.HighQualityApplicationIcon : FolderNames.ApplicationIcons;

            string serverSubFolder = get_sub_folder(applicationId);
            string clientSubFolder = get_sub_folder(applicationId, true);

            string serverAddress = map_path(applicationId, folderName) +
                "\\" + serverSubFolder + "\\" + applicationId.ToString() + ".jpg";

            adr = !File.Exists(serverAddress) ? string.Empty :
                get_client_path(applicationId, folderName) + "/" + clientSubFolder + "/" + applicationId.ToString() + ".jpg";

            if (!highQuality && string.IsNullOrEmpty(adr)) adr = "../../Images/RaaiVanLogo.png";

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }

        public static string get_cover_photo_url(Guid? applicationId, 
            Guid ownerId, bool networkAddress = false, bool highQuality = false)
        {
            if (RaaiVanSettings.SAASBasedMultiTenancy) applicationId = null;

            if (ownerId == Guid.Empty) return string.Empty;

            FolderNames folderName = highQuality ? FolderNames.HighQualityCoverPhoto : FolderNames.CoverPhoto;

            string serverSubFolder = get_sub_folder(ownerId);
            string clientSubFolder = get_sub_folder(ownerId, true);

            string serverAddress = map_path(applicationId, folderName) +
                "\\" + serverSubFolder + "\\" + ownerId.ToString() + ".jpg";

            string address = !File.Exists(serverAddress) ? string.Empty :
                get_client_path(applicationId, folderName) + "/" + clientSubFolder + "/" + ownerId.ToString() + ".jpg";

            return networkAddress ? address.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : address;
        }

        public static string get_download_url(Guid applicationId, Guid fileId)
        {
            return PublicConsts.get_complete_url(applicationId, PublicConsts.FileDownload) +
                "?timeStamp=" + DateTime.Now.Ticks.ToString() + "&FileID=" + fileId.ToString();
        }

        public static void encrypt_file_if_needed(Guid? applicationId, DocFileInfo file, FolderNames? folderName = null)
        {
            FolderNames? fn = folderName.HasValue ? folderName : file.get_folder_name();
            if (!fn.HasValue || (fn != FolderNames.TemporaryFiles && fn != FolderNames.Attachments &&
                fn != FolderNames.WikiContent)) return;

            if (!RaaiVanSettings.FileEncryption(applicationId) ||
                ((file.Size.HasValue ? file.Size.Value : 0) / (1024 * 1024)) > 10) return;

            string folderPath = !folderName.HasValue ? file.get_folder_address(applicationId) :
                file.get_folder_address(applicationId, folderName.Value);
            string fileName = file.get_file_name_with_extension();

            string sourcePath = folderPath + "\\" + fileName;
            string destPath = folderPath + "\\" + PublicConsts.EncryptedFileNamePrefix + fileName;

            if (DocumentUtilities.encrypt_file_aes(sourcePath, destPath)) File.Delete(sourcePath);
        }

        private static byte[] _aes_encryption(byte[] input, bool decrypt, bool useTokenKey)
        {
            //static key & salt
            byte[] AES_KEY = useTokenKey ? USBToken.read_encryption_key() : new byte[] {
                111, 14, 160, 236, 16, 107, 182, 80, 12, 58, 227, 77, 4, 127, 67, 27,
                212, 21, 173, 27, 254, 16, 130, 6, 198, 112, 21, 71, 144, 48, 170, 183
            };

            byte[] AES_SALT = new byte[] { 198, 254, 21, 67, 107, 14, 183, 80 };
            //end of static key & salt

            byte[] retBytes = new byte[0];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(AES_KEY, AES_SALT, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(memoryStream,
                        decrypt ? aes.CreateDecryptor() : aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        try
                        {
                            cs.Write(input, 0, input.Length);
                            cs.Dispose();
                        }
                        catch { }
                    }

                    retBytes = memoryStream.ToArray();
                }
            }

            return retBytes;
        }

        private static byte[] _aes_encryption(byte[] input, bool decrypt) {
            return _aes_encryption(input, decrypt, RaaiVanSettings.USBToken);
        }

        private static bool _aes_encryption_file(string input, string output, bool decrypt)
        {
            try
            {
                if (File.Exists(output)) return true;
                else if (string.IsNullOrEmpty(input) || !File.Exists(input)) return false;

                byte[] result = _aes_encryption(File.ReadAllBytes(input), decrypt);

                if (result.Length > 0) File.WriteAllBytes(output, result);

                return File.Exists(output);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool encrypt_file_aes(string input, string output)
        {
            return _aes_encryption_file(input, output, false);
        }

        public static byte[] encrypt_bytes_aes(byte[] input)
        {
            return _aes_encryption(input, decrypt: false);
        }

        public static byte[] encrypt_bytes_aes_native(byte[] input)
        {
            return _aes_encryption(input, decrypt: false, useTokenKey: false);
        }

        public static bool decrypt_file_aes(string input, string output)
        {
            return _aes_encryption_file(input, output, true);
        }

        public static byte[] decrypt_file_aes(string input)
        {
            return string.IsNullOrEmpty(input) || !File.Exists(input) ? new byte[0] :
                _aes_encryption(File.ReadAllBytes(input), decrypt: true);
        }

        public static byte[] decrypt_bytes_aes(byte[] input)
        {
            return _aes_encryption(input, decrypt: true);
        }

        public static byte[] decrypt_bytes_aes_native(byte[] input)
        {
            return _aes_encryption(input, decrypt: true, useTokenKey: false);
        }
    }
    
    public class DocFileInfo : ICloneable
    {
        public Guid? OwnerID;
        public FileOwnerTypes OwnerType;
        public Guid? FileID;
        public string FileName;
        public string Extension;
        public long? Size;
        public Guid? OwnerNodeID;
        public string OwnerNodeName;
        public string OwnerNodeType;
        private FolderNames? _FolderName;
        private bool? _Encrypted;

        public DocFileInfo() {
            _FolderName = null;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        public string MIME() {
            return PublicMethods.get_mime_type_by_extension(Extension);
        }

        public string stored_file_name {
            get {
                return !FileID.HasValue ? string.Empty : FileID.ToString() + (string.IsNullOrEmpty(Extension) ? "" : "." + Extension);
            }
        }

        public void move(Guid? applicationId, FolderNames source, FolderNames destination, Guid? newGuidName = null)
        {
            try
            {
                if (!FileID.HasValue) return;

                set_folder_name(applicationId, source);

                string sourceAddress = get_real_address(applicationId);

                if (string.IsNullOrEmpty(sourceAddress)) return;

                if (newGuidName.HasValue && newGuidName.Value != Guid.Empty) FileID = newGuidName;
                string destFolder = get_folder_address(applicationId, destination);
                string destinationAddress = get_address(applicationId, destination);

                if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
                File.Move(sourceAddress, destinationAddress);
            }
            catch { }
        }

        public bool exists(Guid? applicationId) {
            return RaaiVanSettings.CephStorage.Enabled ? CephStorage.file_exists(stored_file_name) :
                !string.IsNullOrEmpty(get_real_address(applicationId));
        }

        public void delete(Guid? applicationId) {
            try
            {
                string address = get_real_address(applicationId);
                if (File.Exists(address)) File.Delete(address);
            }
            catch { }
        }

        public void set_folder_name(Guid? applicationId, FolderNames name) {
            _FolderName = name;
            get_real_address(applicationId); //to set the property _Encrypted
        }

        public FolderNames? get_folder_name() {
            return _FolderName;
        }

        public bool is_encrypted(Guid? applicationId)
        {
            if (!_Encrypted.HasValue) get_real_address(applicationId);
            return _Encrypted.HasValue && _Encrypted.Value;
        }

        public string get_file_name_with_extension() {
            return !FileID.HasValue ? string.Empty :
                FileID.ToString() + (string.IsNullOrEmpty(Extension) ? string.Empty : "." + Extension);
        }

        public string get_folder_address(Guid? applicationId, FolderNames folderName)
        {
            if (!FileID.HasValue) return string.Empty;

            string sub = !DocumentUtilities.has_sub_folder(folderName) ? string.Empty :
                "\\" + DocumentUtilities.get_sub_folder(FileID.Value);
            return DocumentUtilities.map_path(applicationId, folderName) + sub;
        }

        public string get_folder_address(Guid? applicationId)
        {
            return !FileID.HasValue || !_FolderName.HasValue ? string.Empty :
                get_folder_address(applicationId, _FolderName.Value);
        }

        private string get_real_address(Guid? applicationId)
        {
            _Encrypted = false;

            string folderPath = get_folder_address(applicationId);

            if (!FileID.HasValue || string.IsNullOrEmpty(folderPath)) return string.Empty;

            string address = folderPath + "\\" + FileID.ToString() + 
                (string.IsNullOrEmpty(Extension) ? string.Empty : "." + Extension);
            string encryptedAddress = folderPath + "\\" + PublicConsts.EncryptedFileNamePrefix + FileID.ToString() +
                (string.IsNullOrEmpty(Extension) ? string.Empty : "." + Extension);
            string addressExtLess = folderPath + "\\" + FileID.ToString();

            if (File.Exists(address)) return address;
            else if (File.Exists(encryptedAddress))
            {
                _Encrypted = true;
                return encryptedAddress;
            }
            else if (!string.IsNullOrEmpty(Extension) && File.Exists(addressExtLess)) return addressExtLess;
            else return string.Empty;
        }

        public string get_address(Guid? applicationId, FolderNames folderName, bool? encrypted = null) {
            return !FileID.HasValue ? string.Empty : get_folder_address(applicationId, folderName) + "\\" + 
                ((!encrypted.HasValue ? is_encrypted(applicationId) : encrypted.Value) ? 
                PublicConsts.EncryptedFileNamePrefix : "") + get_file_name_with_extension();
        }

        public string get_address(Guid? applicationId, bool? encrypted = null)
        {
            return !_FolderName.HasValue ? string.Empty : get_address(applicationId, _FolderName.Value, encrypted);
        }

        public string get_client_address(Guid? applicationId) {
            if (!FileID.HasValue || !_FolderName.HasValue) return string.Empty;

            string subForlder = !DocumentUtilities.has_sub_folder(_FolderName.Value) ? string.Empty :
                "/" + DocumentUtilities.get_sub_folder(FileID.Value, true);

            return DocumentUtilities.get_client_path(applicationId, _FolderName.Value) + 
                subForlder + "/" + get_file_name_with_extension();
        }

        public bool readable(Guid applicationId)
        {
            return !string.IsNullOrEmpty(Extension) && ("jpg,png,jpeg,gif,bmp,mp4,mp3,wav,ogg,webm" +
                (RaaiVanConfig.Modules.PDFViewer(applicationId) ? ",pdf" : "")).Split(',').Any(x => x == Extension.ToLower());
        }

        public bool downloadable(Guid? applicationId)
        {
            return false;
            //string.IsNullOrEmpty(Extension) || !RaaiVanConfig.Modules.PDFViewer || Extension.ToLower() != "pdf";
        }

        public byte[] toByteArray(Guid? applicationId, ref string fileAddress)
        {
            try
            {
                if (RaaiVanSettings.CephStorage.Enabled) return CephStorage.get_file(stored_file_name);

                fileAddress = get_real_address(applicationId);

                if (string.IsNullOrEmpty(fileAddress)) return new byte[0];
                else return is_encrypted(applicationId) ? DocumentUtilities.decrypt_file_aes(fileAddress) : File.ReadAllBytes(fileAddress);
            }
            catch {
                return new byte[0];
            }
        }

        public byte[] toByteArray(Guid? applicationId) {
            string fileAddress = string.Empty;
            return toByteArray(applicationId, ref fileAddress);
        }

        public string url() {
            return RaaiVanSettings.CephStorage.Enabled ? CephStorage.get_download_url(stored_file_name) :
                "../../download/" + FileID.ToString() + (_FolderName.HasValue ? "?Category=" + _FolderName.ToString() : string.Empty);
        }

        public string toJson(Guid? applicationId, bool icon = false) {
            if (string.IsNullOrEmpty(FileName)) FileName = string.Empty;
            
            string iconName = string.IsNullOrEmpty(Extension) ? "dkgadjkghdkjghkfdjh" : Extension;

            string _path = PublicMethods.map_path("~/images/extensions/" + iconName + ".png");
            string _clPath = "../../images/extensions/" + iconName + ".png";

            return "{\"FileID\":\"" + FileID.Value.ToString() + "\"" +
                ",\"FileName\":\"" + Base64.encode(FileName) + "\"" +
                ",\"OwnerID\":\"" + (OwnerID.HasValue && OwnerID != Guid.Empty ? OwnerID.Value.ToString() : string.Empty) + "\"" +
                ",\"Extension\":\"" + Base64.encode(Extension) + "\",\"MIME\":\"" + MIME() + "\"" +
                ",\"Size\":" + (Size.HasValue ? Size.Value : 0).ToString() +
                ",\"Downloadable\":" + downloadable(applicationId).ToString().ToLower() +
                (!icon ? string.Empty :
                    ",\"IconURL\":\"" + (File.Exists(_path) ? _clPath : _clPath.Replace(iconName, "default")) + "\"") +
                "}";
        }
    }
}
