using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.IO;
using System.Net;
using RaaiVan.Modules.Documents;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.Users;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for FileDownLoad
    /// </summary>
    public class DownLoad : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        ParamsContainer paramsContainer;

        public void ProcessRequest(HttpContext context)
        {
            //Privacy Check: OK
            paramsContainer = new ParamsContainer(context, nullTenantResponse: false);
            
            Guid fileId = string.IsNullOrEmpty(context.Request.Params["FileID"]) ?
                Guid.Empty : Guid.Parse(context.Request.Params["FileID"]);
            if (fileId == Guid.Empty && !Guid.TryParse(context.Request.Params["ATTFID"], out fileId)) fileId = Guid.Empty;

            string category = PublicMethods.parse_string(context.Request.Params["Category"], false);
            
            bool isTemporary = category.ToLower() == FolderNames.TemporaryFiles.ToString().ToLower();
            bool? addFooter = PublicMethods.parse_bool(context.Request.Params["Meta"]);
            Guid? coverId = PublicMethods.parse_guid(context.Request.Params["CoverID"]);
            string pdfPassword = PublicMethods.parse_string(context.Request.Params["PS"]);

            List<FolderNames> freeFolders = new[] {
                FolderNames.ProfileImages,
                FolderNames.HighQualityProfileImage,
                FolderNames.CoverPhoto,
                FolderNames.HighQualityCoverPhoto,
                FolderNames.Icons,
                FolderNames.HighQualityIcon,
                FolderNames.ApplicationIcons,
                FolderNames.HighQualityApplicationIcon,
                FolderNames.Pictures
            }.ToList();

            bool isFreeFolder = !string.IsNullOrEmpty(category) && freeFolders.Any(f => f.ToString().ToLower() == category.ToLower());

            if (isFreeFolder) {
                FolderNames fn = freeFolders.Where(u => u.ToString().ToLower() == category.ToLower()).FirstOrDefault();

                DocFileInfo pic =
                    new DocFileInfo() { FileID = fileId, Extension = "jpg", FileName = fileId.ToString(), FolderName = fn };

                send_file(pic, false);
            }

            if (!paramsContainer.ApplicationID.HasValue)
            {
                paramsContainer.return_response(PublicConsts.NullTenantResponse);
                return;
            }

            if (isTemporary)
            {
                string ext = PublicMethods.parse_string(context.Request.Params["Extension"]);

                DocFileInfo temp = new DocFileInfo()
                {
                    FileID = fileId,
                    Extension = ext,
                    FileName = fileId.ToString(),
                    FolderName = FolderNames.TemporaryFiles
                };

                send_file(temp, false);
            }
            else
            {
                DocFileInfo AttachFile = DocumentsController.get_file(paramsContainer.Tenant.Id, fileId);

                if (AttachFile == null) {
                    paramsContainer.return_response("{\"ErrorText\":\"" + Messages.AccessDenied + "\"}");
                    return;
                }

                PrivacyObjectType pot = AttachFile.OwnerType == FileOwnerTypes.Node ?
                    PrivacyObjectType.Node : PrivacyObjectType.None;

                DocFileInfo ownerNode = !AttachFile.FileID.HasValue ? null :
                    DocumentsController.get_file_owner_node(paramsContainer.Tenant.Id, AttachFile.FileID.Value);
                if (ownerNode != null) {
                    AttachFile.OwnerNodeID = ownerNode.OwnerNodeID;
                    AttachFile.OwnerNodeName = ownerNode.OwnerNodeName;
                    AttachFile.OwnerNodeType = ownerNode.OwnerNodeType;
                }

                bool accessDenied = 
                    !PrivacyController.check_access(paramsContainer.Tenant.Id,
                        paramsContainer.CurrentUserID, AttachFile.OwnerID.Value, pot, PermissionType.View) &&
                    !(
                        paramsContainer.CurrentUserID.HasValue &&
                        new CNAPI() { paramsContainer = this.paramsContainer }
                            ._is_admin(paramsContainer.Tenant.Id, AttachFile.OwnerID.Value,
                            paramsContainer.CurrentUserID.Value, CNAPI.AdminLevel.Creator, false)
                     );

                if (accessDenied)
                {
                    //Save Log
                    try
                    {
                        LogController.save_log(paramsContainer.Tenant.Id, new Log()
                        {
                            UserID = paramsContainer.CurrentUserID,
                            HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                            HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                            Action = Modules.Log.Action.Download_AccessDenied,
                            SubjectID = fileId,
                            Info = "{\"Error\":\"" + Base64.encode(Messages.AccessDenied.ToString()) + "\"}",
                            ModuleIdentifier = ModuleIdentifier.DCT
                        });
                    }
                    catch { }
                    //end of Save Log

                    paramsContainer.return_response("{\"ErrorText\":\"" + Messages.AccessDenied + "\"}");

                    return;
                }

                AttachFile.refresh_folder_name();

                string ext = AttachFile == null || string.IsNullOrEmpty(AttachFile.Extension) ? string.Empty :
                    AttachFile.Extension.ToLower();
                bool isImage = ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "gif" || ext == "bmp";

                DocFileInfo pdfCover = null;
                if (!string.IsNullOrEmpty(AttachFile.Extension) && AttachFile.Extension.ToLower() == "pdf" && coverId.HasValue)
                {
                    pdfCover = DocumentsController.get_file(paramsContainer.Tenant.Id, coverId.Value);
                    if (pdfCover == null || string.IsNullOrEmpty(pdfCover.Extension) || pdfCover.Extension.ToLower() != "pdf") pdfCover = null;
                }

                send_file(AttachFile, !isImage, addPDFCover: true,
                    addPDFFooter: addFooter.HasValue && addFooter.Value,
                    pdfCover: pdfCover == null ? null : pdfCover.toByteArray(paramsContainer.ApplicationID),
                    pdfPassword: pdfPassword);
            }
        }

        protected void send_file(DocFileInfo file, bool logNeeded, 
            bool addPDFCover = false, bool addPDFFooter = false, byte[] pdfCover = null, string pdfPassword = null)
        {
            byte[] fileContent = file.toByteArray(paramsContainer.ApplicationID);

            if (fileContent.Length == 0) {
                send_empty_response();
                return;
            }
            
            //Save Log
            if (logNeeded && paramsContainer.CurrentUserID.HasValue)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.Download,
                    SubjectID = file.FileID,
                    Info = file.toJson(paramsContainer.Tenant.Id),
                    ModuleIdentifier = ModuleIdentifier.DCT
                });
            }
            //end of Save Log

            if (file.Extension.ToLower() == "pdf")
            {
                addPDFCover = addPDFCover && file.OwnerNodeID.HasValue && pdfCover != null && pdfCover.Length > 0;

                if (addPDFFooter || addPDFCover)
                {
                    bool invalidPassword = false;

                    fileContent = PDFUtil.get_pdf_content(paramsContainer.Tenant.Id, fileContent, pdfPassword, ref invalidPassword);

                    if (invalidPassword)
                    {
                        string responseText = "{\"InvalidPassword\":" + true.ToString().ToLower() + "}";
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                }

                if (addPDFFooter) {
                    User currentUser = !paramsContainer.CurrentUserID.HasValue ? null :
                        UsersController.get_user(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);

                    if (currentUser == null)
                    {
                        currentUser = new User()
                        {
                            UserID = Guid.NewGuid(),
                            UserName = "[anonymous]",
                            FirstName = "[anonymous]",
                            LastName = "[anonymous]"
                        };
                    }

                    DownloadedFileMeta meta = new DownloadedFileMeta(PublicMethods.get_client_ip(HttpContext.Current),
                        currentUser.UserName, currentUser.FirstName, currentUser.LastName, null);

                    fileContent = PDFTemplates.append_footer(fileContent, meta.toString());
                }

                if (addPDFCover) {
                    Dictionary<string, string> dic = new CNAPI() { paramsContainer = this.paramsContainer }
                        .get_replacement_dictionary(file.OwnerNodeID.Value, true);

                    List<FormElement> tempElems = dic.Keys.ToList().Select(key => new FormElement()
                    {
                        Name = key,
                        Type = FormElementTypes.Text,
                        TextValue = dic[key]
                    }).ToList();

                    byte[] cover = PDFTemplates.fill_template(pdfCover, tempElems);

                    fileContent = PDFUtil.merge_documents(new List<object>() { cover, fileContent });
                }

                if (!string.IsNullOrEmpty(pdfPassword)) fileContent = PDFUtil.set_password(fileContent, pdfPassword);
            }

            HttpContext context = HttpContext.Current;

            //Stream the File from Server
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", string.Format("attachment;filename=\"{0}\"", file.FileName +
                (string.IsNullOrEmpty(file.Extension) ? string.Empty : "." + file.Extension)));
            context.Response.AddHeader("Content-Length", fileContent.Length.ToString());

            context.Response.BinaryWrite(fileContent);

            context.Response.End();
        }

        protected void send_empty_response()
        {
            HttpContext context = HttpContext.Current;

            context.Response.Clear();
            context.Response.BufferOutput = true;
            context.Response.Write("فایل یافت نشد");
            context.Response.End();
            context.Response.Close();
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