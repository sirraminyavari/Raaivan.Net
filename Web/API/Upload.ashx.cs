using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using RaaiVan.Modules.Documents;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for FileUpload
    /// </summary>
    public class Upload : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public ParamsContainer paramsContainer;

        public void ProcessRequest(HttpContext context)
        {
            paramsContainer = new ParamsContainer(context, nullTenantResponse: false);

            if (!paramsContainer.CurrentUserID.HasValue)
            {
                paramsContainer.return_response(PublicConsts.BadRequestResponse);
                return;
            }

            if (ProcessTenantIndependentRequest(context)) return;

            if (!paramsContainer.ApplicationID.HasValue)
            {
                paramsContainer.return_response(PublicConsts.NullTenantResponse);
                return;
            }

            string responseText = string.Empty;

            string command = !string.IsNullOrEmpty(context.Request.Params["cmd"]) ? context.Request.Params["cmd"].ToLower() :
                (string.IsNullOrEmpty(context.Request.Params["Command"]) ? "uploadfile" : context.Request.Params["Command"].ToLower());

            Guid userId = string.IsNullOrEmpty(context.Request.Params["UserID"]) ? PublicMethods.get_current_user_id() :
                Guid.Parse(context.Request.Params["UserID"]);

            Guid ownerId = string.IsNullOrEmpty(context.Request.Params["OwnerID"]) ? Guid.Empty :
                Guid.Parse(context.Request.Params["OwnerID"]);
            Guid fileId = string.IsNullOrEmpty(context.Request.Params["FileID"]) ? Guid.Empty :
                Guid.Parse(context.Request.Params["FileID"]);
            string fileName = string.IsNullOrEmpty(context.Request.Params["FileName"]) ? string.Empty :
                context.Request.Params["FileName"];

            FileOwnerTypes ownerType = FileOwnerTypes.None;
            if (!Enum.TryParse<FileOwnerTypes>(context.Request.Params["OwnerType"], out ownerType))
                ownerType = FileOwnerTypes.None;
            
            switch (command)
            {
                case "uploadfile":
                case "upload_file":
                    {
                        FolderNames folderName = ownerId == Guid.Empty ?
                            FolderNames.TemporaryFiles : DocumentUtilities.get_folder_name(ownerType);
                        
                        DocFileInfo file = new DocFileInfo()
                        {
                            FileID = fileId != Guid.Empty ? fileId : Guid.NewGuid(),
                            OwnerID = ownerId,
                            OwnerType = ownerType
                        };

                        file.set_folder_name(paramsContainer.Tenant.Id, folderName);
                        
                        _attach_file_command(paramsContainer.ApplicationID, file, ref responseText);
                        _return_response(ref responseText);
                        return;
                    }
                case "deletefile":
                    responseText = remove_file(fileId, ref responseText) ? "yes" : "no";
                    _return_response(ref responseText);
                    return;
                case "removefile":
                    remove_file(fileId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "deleteprofileimage":
                    {
                        DocFileInfo file = new DocFileInfo()
                        {
                            FileID = PublicMethods.parse_guid(fileName.Substring(0, fileName.IndexOf("."))),
                            Extension = "jpg"
                        };

                        file.set_folder_name(paramsContainer.Tenant.Id, FolderNames.ProfileImages);

                        remove_file(file, ref responseText);
                        _return_response(ref responseText);
                        return;
                    }
                case "uploadpicture":
                    {
                        int maxWidth = 100, maxHeight = 100;
                        FolderNames imageFolder = FolderNames.Pictures;

                        Guid? pictureId = PublicMethods.parse_guid(context.Request.Params["PictureID"]);
                        bool hasId = pictureId.HasValue;
                        if (!pictureId.HasValue) pictureId = Guid.NewGuid();

                        string imageType = string.IsNullOrEmpty(context.Request.Params["Type"]) ? "" :
                            context.Request.Params["Type"].ToLower();

                        string errorMessage = string.Empty;
                        
                        switch (imageType)
                        {
                            case "post":
                                maxWidth = maxHeight = 640;
                                imageFolder = FolderNames.Pictures;
                                break;
                            default:
                                _return_response(ref responseText);
                                return;
                        }
                        
                        DocFileInfo uploaded = new DocFileInfo() {
                            FileID = Guid.NewGuid(),
                            OwnerType = ownerType
                        };

                        if (ownerId != Guid.Empty) uploaded.OwnerID = ownerId;
                        uploaded.set_folder_name(paramsContainer.Tenant.Id, FolderNames.TemporaryFiles);

                        bool succeed = _attach_file_command(paramsContainer.ApplicationID, uploaded, ref responseText);

                        if (succeed)
                        {
                            DocFileInfo destFile = new DocFileInfo() {
                                FileID = hasId ? pictureId : uploaded.FileID,
                                OwnerType = ownerType,
                                Extension = "jpg"
                            };

                            if (ownerId != Guid.Empty) destFile.OwnerID = ownerId;
                            destFile.set_folder_name(paramsContainer.Tenant.Id,
                                hasId ? imageFolder : FolderNames.TemporaryFiles);
                            
                            RVGraphics.make_thumbnail(paramsContainer.Tenant.Id, uploaded, destFile,
                                maxWidth, maxHeight, 0, 0, ref errorMessage, "jpg");
                            
                            if (string.IsNullOrEmpty(errorMessage))
                                responseText = responseText.Replace(uploaded.FileID.ToString(), destFile.FileID.ToString());
                        }

                        responseText = responseText.Replace("\"~[[MSG]]\"",
                            string.IsNullOrEmpty(errorMessage) ? "\"\"" : errorMessage);

                        _return_response(ref responseText);
                        return;
                    }
                case "deleteicon":
                    {
                        DocFileInfo file = new DocFileInfo() {
                            FileID = PublicMethods.parse_guid(fileName.Substring(0, fileName.IndexOf("."))),
                            Extension = "jpg"
                        };

                        file.set_folder_name(paramsContainer.ApplicationID, FolderNames.Icons);
                        
                        remove_file(file, ref responseText);
                        _return_response(ref responseText);
                    }
                    return;
            }

            paramsContainer.return_response(PublicConsts.BadRequestResponse);
        }

        public bool ProcessTenantIndependentRequest(HttpContext context)
        {
            if (!RaaiVanSettings.SAASBasedMultiTenancy && !paramsContainer.ApplicationID.HasValue) {
                paramsContainer.return_response(PublicConsts.NullTenantResponse);
                return true;
            }

            string responseText = string.Empty;

            string command = !string.IsNullOrEmpty(context.Request.Params["cmd"]) ? context.Request.Params["cmd"].ToLower() :
                (string.IsNullOrEmpty(context.Request.Params["Command"]) ? "uploadfile" : context.Request.Params["Command"].ToLower());

            Guid userId = string.IsNullOrEmpty(context.Request.Params["UserID"]) ? PublicMethods.get_current_user_id() :
                Guid.Parse(context.Request.Params["UserID"]);

            Guid ownerId = string.IsNullOrEmpty(context.Request.Params["OwnerID"]) ? Guid.Empty :
                Guid.Parse(context.Request.Params["OwnerID"]);
            
            FileOwnerTypes ownerType = FileOwnerTypes.None;
            if (!Enum.TryParse<FileOwnerTypes>(context.Request.Params["OwnerType"], out ownerType))
                ownerType = FileOwnerTypes.None;

            Guid? applicationId = paramsContainer.ApplicationID;

            switch (command)
            {
                case "uploadicon":
                    {
                        Guid iconId = string.IsNullOrEmpty(context.Request.Params["IconID"]) ? Guid.Empty :
                            Guid.Parse(context.Request.Params["IconID"]);

                        IconType iconType = IconType.None;
                        if (!Enum.TryParse<IconType>(context.Request.Params["Type"], true, out iconType))
                            iconType = IconType.None;

                        if (iconType == IconType.ApplicationIcon || (RaaiVanSettings.SAASBasedMultiTenancy &&
                            (iconType == IconType.ProfileImage || iconType == IconType.CoverPhoto))) applicationId = null;
                        else if (!applicationId.HasValue)
                        {
                            responseText = PublicConsts.NullTenantResponse;
                            break;
                        }

                        DocFileInfo uploaded = new DocFileInfo()
                        {
                            FileID = iconId,
                            OwnerID = ownerId,
                            OwnerType = ownerType
                        };
                        
                        uploaded.set_folder_name(applicationId, FolderNames.TemporaryFiles);

                        bool succeed = _attach_file_command(applicationId, uploaded, ref responseText);

                        if (!succeed) break;

                        if (iconType == IconType.ProfileImage && iconId == Guid.Empty) iconId = userId;

                        string errorMessage = string.Empty;

                        succeed = RVGraphics.create_icon(applicationId, iconId, iconType, uploaded, ref errorMessage);

                        responseText = responseText.Replace("\"~[[MSG]]\"", errorMessage);

                        try
                        {
                            string tempRes = string.Empty;
                            if (succeed) remove_file(uploaded, ref tempRes);
                        }
                        catch { }

                        break;
                    }
                case "cropicon":
                    {
                        Guid iconId = string.IsNullOrEmpty(context.Request.Params["IconID"]) ? Guid.Empty :
                            Guid.Parse(context.Request.Params["IconID"]);

                        IconType iconType = IconType.None;
                        if (!Enum.TryParse<IconType>(context.Request.Params["Type"], true, out iconType))
                            iconType = IconType.None;

                        if (iconType == IconType.ApplicationIcon || (RaaiVanSettings.SAASBasedMultiTenancy &&
                            (iconType == IconType.ProfileImage || iconType == IconType.CoverPhoto))) applicationId = null;
                        else if (!applicationId.HasValue)
                        {
                            responseText = PublicConsts.NullTenantResponse;
                            break;
                        }

                        int iconWidth = 100, iconHeight = 100;
                        FolderNames imageFolder = FolderNames.ProfileImages,
                            highQualityImageFolder = FolderNames.HighQualityProfileImage;

                        if (iconType == IconType.ProfileImage && iconId == Guid.Empty) iconId = userId;

                        bool isValid = DocumentUtilities.get_icon_parameters(iconType,
                            ref iconWidth, ref iconHeight, ref imageFolder, ref highQualityImageFolder);

                        if (!isValid) break;

                        DocFileInfo highQualityFile = new DocFileInfo() { FileID = iconId, Extension = "jpg" };
                        highQualityFile.set_folder_name(applicationId, highQualityImageFolder);

                        DocFileInfo file = new DocFileInfo() { FileID = iconId, Extension = "jpg" };
                        file.set_folder_name(applicationId, imageFolder);

                        int x = string.IsNullOrEmpty(context.Request.Params["X"]) ? -1 : int.Parse(context.Request.Params["X"]);
                        int y = string.IsNullOrEmpty(context.Request.Params["Y"]) ? -1 : int.Parse(context.Request.Params["Y"]);
                        int width = string.IsNullOrEmpty(context.Request.Params["Width"]) ? -1 :
                            (int)double.Parse(context.Request.Params["Width"]);
                        int height = string.IsNullOrEmpty(context.Request.Params["Height"]) ? -1 :
                            (int)double.Parse(context.Request.Params["Height"]);

                        RVGraphics.extract_thumbnail(applicationId, highQualityFile, file,
                            x, y, width, height, iconWidth, iconHeight, ref responseText);

                        break;
                    }
            }

            if (!string.IsNullOrEmpty(responseText))
                paramsContainer.return_response(ref responseText);

            return !string.IsNullOrEmpty(responseText);
        }

        protected void _return_response(ref string responseText)
        {
            paramsContainer.return_response(ref responseText);
        }

        protected bool _attach_file_command(Guid? applicationId, DocFileInfo fileInfo, ref string responseText)
        {
            HttpContext context = HttpContext.Current;
            string filename = context.Request.Headers["X-File-Name"];
            filename = HttpUtility.UrlDecode(filename);
            
            if (string.IsNullOrEmpty(filename) && context.Request.Files.Count > 0)
                filename = context.Request.Files[0].FileName;

            if (filename.IndexOf("Encoded__") == 0) filename = Base64.decode(filename.Substring(("Encoded__").Length));

            if (string.IsNullOrEmpty(filename) && context.Request.Files.Count <= 0)
            {
                responseText = "{\"success\":false}";
                return false;
            }

            fileInfo.Size = context.Request.InputStream.Length;
            fileInfo.FileName = "";
            fileInfo.Extension = "";
            
            int indexOfLastDot = filename.LastIndexOf('.');
            if (indexOfLastDot >= 0 && indexOfLastDot < filename.Length - 1)
                fileInfo.Extension = filename.Substring(indexOfLastDot + 1).ToLower();

            fileInfo.FileName = string.IsNullOrEmpty(fileInfo.Extension) ? filename : filename.Substring(0, indexOfLastDot);

            FolderNames fn = fileInfo.get_folder_name().HasValue ?
                fileInfo.get_folder_name().Value : FolderNames.TemporaryFiles;
            
            bool result = filename == null ?
                __ie_response(applicationId, context.Request.Files[0], fileInfo, ref responseText) :
                __other_browsers_response(applicationId, context.Request.InputStream, fileInfo, ref responseText);

            if (applicationId.HasValue && paramsContainer.CurrentUserID.HasValue && 
                fileInfo.OwnerID.HasValue && fileInfo.OwnerID != Guid.Empty && fileInfo.FileID != null)
            {
                result = DocumentsController.add_file(applicationId.Value,
                    fileInfo.OwnerID.Value, fileInfo.OwnerType, fileInfo, paramsContainer.CurrentUserID.Value);

                if (!result)
                {
                    fileInfo.move(paramsContainer.ApplicationID, fn, FolderNames.TemporaryFiles);
                    responseText = "{\"success\":false}";
                    return false;
                }
            }

            return true;
        }

        protected bool __ie_response(Guid? applicationId, HttpPostedFile uploadedFile, DocFileInfo fi, ref string responseText)
        {
            string filename = HttpUtility.UrlDecode(uploadedFile.FileName);

            int indexOfLastDot = filename.LastIndexOf('.');
            if (indexOfLastDot >= 0 && indexOfLastDot < filename.Length - 1)
                fi.Extension = filename.Substring(indexOfLastDot + 1).ToLower();
            fi.FileName = string.IsNullOrEmpty(fi.Extension) ? filename : filename.Substring(0, indexOfLastDot);
            
            try
            {
                string folderPath = fi.get_folder_address(applicationId);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                uploadedFile.SaveAs(fi.get_address(applicationId));
                
                fi.Size = uploadedFile.ContentLength;
                
                responseText = "{\"success\":" + true.ToString().ToLower() +
                    ",\"AttachedFile\":" + fi.toJson(applicationId, true) +
                    ",\"name\":\"" + fi.get_file_name_with_extension() + "\"" +
                    ",\"url\":\"" + fi.url() + "\"" +
                    ",\"Message\":\"~[[MSG]]\"}";
            }
            catch (Exception)
            {
                fi.FileID = null;
                responseText = "{\"success\":\"false\"}";
                return false;
            }

            return false;
        }

        protected bool __other_browsers_response(Guid? applicationId, Stream inputStream, DocFileInfo fi, ref string responseText)
        {
            //This work for Firefox and Chrome.
            
            try
            {
                string folderPath = fi.get_folder_address(applicationId);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using (FileStream fileStream = new FileStream(fi.get_address(applicationId), FileMode.Create))
                {
                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        HttpContext.Current.Request.Files[0].InputStream.CopyTo(fileStream);
                        HttpContext.Current.Request.Files[0].InputStream.Close();
                        HttpContext.Current.Request.Files[0].InputStream.Dispose();
                    }
                    else inputStream.CopyTo(fileStream);
                }

                DocumentUtilities.encrypt_file_if_needed(applicationId, fi);
                
                responseText = "{\"success\":" + true.ToString().ToLower() + 
                    ",\"AttachedFile\":" + fi.toJson(applicationId, true) +
                    ",\"name\":\"" + fi.get_file_name_with_extension() + "\"" +
                    ",\"url\":\"" + fi.url() + "\"" +
                    ",\"Message\":\"~[[MSG]]\"}";

            }
            catch (Exception)
            {
                fi.FileID = null;
                responseText = "{\"success\":false}";
                return false;
            }
            finally
            {
                inputStream.Close();
                inputStream.Dispose();
            }

            return true;
        }

        protected bool remove_file(DocFileInfo file, ref string responseText)
        {
            try
            {
                if (file != null && file.get_folder_name().HasValue && file.get_folder_name().Value == FolderNames.TemporaryFiles)
                    file.delete(paramsContainer.Tenant.Id);
            }
            catch { }
            
            bool result = file != null && file.FileID.HasValue && 
                DocumentsController.remove_file(paramsContainer.Tenant.Id, file.FileID.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            return result;
        }

        protected bool remove_file(Guid fileId, ref string responseText)
        {
            return remove_file(DocumentsController.get_file(paramsContainer.Tenant.Id, fileId), ref responseText);
        }
        
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}