using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Documents;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.CoreNetwork;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for PDFAPI
    /// </summary>
    public class PDFAPI : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        ParamsContainer paramsContainer = null;

        public void ProcessRequest(HttpContext context)
        {
            //Privacy Check: OK
            paramsContainer = new ParamsContainer(context, nullTenantResponse: true);
            if (!paramsContainer.ApplicationID.HasValue) return;

            string responseText = string.Empty;

            string command = PublicMethods.parse_string(context.Request.Params["Command"], false);
            if (string.IsNullOrEmpty(command)) return;

            Guid currentUserId = paramsContainer.CurrentUserID.HasValue ? paramsContainer.CurrentUserID.Value : Guid.Empty;

            Guid fileId = string.IsNullOrEmpty(context.Request.Params["FileID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["FileID"]);
            DocFileInfo file = DocumentsController.get_file(paramsContainer.Tenant.Id, fileId);
            if (file == null)
            {
                paramsContainer.return_response(PublicConsts.BadRequestResponse);
                return;
            }

            bool isTemporary =
                PublicMethods.parse_string(HttpContext.Current.Request.Params["Category"], false).ToLower() == "temporary";

            bool hasAccess = isTemporary || PublicMethods.is_system_admin(paramsContainer.Tenant.Id, currentUserId);

            PrivacyObjectType pot = file.OwnerType == FileOwnerTypes.Node ? PrivacyObjectType.Node : PrivacyObjectType.None;

            hasAccess = hasAccess || PrivacyController.check_access(paramsContainer.Tenant.Id,
                paramsContainer.CurrentUserID, file.OwnerID.Value, pot, PermissionType.View);

            if (!hasAccess && currentUserId != Guid.Empty &&
                CNController.is_node(paramsContainer.Tenant.Id, file.OwnerID.Value))
            {
                bool isCreator = false, isContributor = false, isExpert = false, isMember = false, isAdminMember = false,
                    isServiceAdmin = false, isAreaAdmin = false, perCreatorLevel = false;

                CNController.get_user2node_status(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value,
                    file.OwnerID.Value, ref isCreator, ref isContributor, ref isExpert, ref isMember, ref isAdminMember,
                    ref isServiceAdmin, ref isAreaAdmin, ref perCreatorLevel);

                hasAccess = isServiceAdmin || isAreaAdmin || isCreator || isContributor || isExpert || isMember;
            }

            if (!hasAccess)
                paramsContainer.return_response("{\"ErrorText\":\"" + Messages.AccessDenied.ToString() + "\"}");

            FolderNames folderName = isTemporary ? FolderNames.TemporaryFiles :
                DocumentUtilities.get_folder_name(file.OwnerType);
            file.set_folder_name(paramsContainer.Tenant.Id, folderName);

            string fileAddress = file.get_real_address(paramsContainer.Tenant.Id);

            if (string.IsNullOrEmpty(fileAddress)) _return_response(ref responseText);

            string destFolder = DocumentUtilities.map_path(paramsContainer.Tenant.Id, FolderNames.PDFImages) +
                "\\" + DocumentUtilities.get_sub_folder(file.FileID.ToString()) + "\\" + file.FileID.ToString();

            switch (command)
            {
                case "Convert2Image":
                    convert2image(file,
                        PublicMethods.parse_string(context.Request.Params["PS"]),
                        destFolder, 
                        PublicMethods.parse_bool(context.Request.Params["Repair"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetPagesCount":
                    get_pages_count(file,
                        PublicMethods.parse_string(context.Request.Params["PS"]),
                        destFolder, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetPage":
                    get_page(PublicMethods.parse_int(context.Request.Params["Page"], 1), destFolder);
                    return;
            }

            paramsContainer.return_response(PublicConsts.BadRequestResponse);
        }

        protected void _return_response(ref string responseText)
        {
            paramsContainer.return_response(responseText);
        }

        protected void convert2image(DocFileInfo file, string password, string destFolder, bool? repair, ref string responseText)
        {
            if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

            bool? status = PDFUtil.pdf2image_isprocessing(file.FileID.Value);

            if ((!status.HasValue || !status.Value) && repair.HasValue && repair.Value)
            {
                PublicMethods.set_timeout(() => {
                    PDFUtil.pdf2image(paramsContainer.Tenant.Id, file, password, destFolder, ImageFormat.Png, true);
                }, 0);

                responseText = "{\"Status\":\"" + "Processing" + "\"}";

                //PDFUtil.pdf2image(paramsContainer.Tenant.Id, file.FileID.Value, fileAddress, destFolder, ImageFormat.Png, true);
                //responseText = "{\"Status\":\"" + "Ready" + "\"}";
            }
            else if (status.HasValue)
                responseText = "{\"Status\":\"" + (status.Value ? "Processing" : "Ready") + "\"}";
            else
            {
                PublicMethods.set_timeout(() => {
                    PDFUtil.pdf2image(paramsContainer.Tenant.Id, file, password, destFolder, ImageFormat.Png, false);
                }, 0);

                responseText = "{\"Status\":\"" + "Processing" + "\"}";

                //PDFUtil.pdf2image(paramsContainer.Tenant.Id, file.FileID.Value, fileAddress, destFolder, ImageFormat.Png, false);
                //responseText = "{\"Status\":\"" + "Ready" + "\"}";
            }
        }

        protected void get_pages_count(DocFileInfo file, string password, string destFolder, ref string responseText)
        {
            bool invalidPassword = false;

            int count = PDFUtil.get_pages_count(paramsContainer.Tenant.Id, file, password, ref invalidPassword);
            int convertedCount = count == 0 ? 0 : PDFUtil.get_converted_pages_count(paramsContainer.Tenant.Id, destFolder);

            responseText = "{\"Count\":" + count.ToString() +
                ",\"ConvertedCount\":" + convertedCount.ToString() +
                (!invalidPassword ? string.Empty : ",\"InvalidPassword\":" + invalidPassword.ToString().ToLower()) +
                "}";
        }

        protected void get_page(int? pageNumber, string folderAddress)
        {
            if (!pageNumber.HasValue) pageNumber = 1;

            try
            {
                ImageFormat imageFormat = ImageFormat.Png;

                string fileAddr = folderAddress + "\\" + pageNumber.ToString() + "." + imageFormat.ToString().ToLower();

                if (!File.Exists(fileAddr)) fileAddr = PublicMethods.map_path(PublicConsts.NoPDFPage);

                byte[] FileBytes = File.ReadAllBytes(fileAddr);

                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition",
                    string.Format("attachment;filename=\"{0}\"",
                    pageNumber.ToString() + "." + imageFormat.ToString().ToLower()));
                HttpContext.Current.Response.AddHeader("Content-Length", FileBytes.Length.ToString());
                HttpContext.Current.Response.WriteFile(fileAddr);
                HttpContext.Current.Response.End();
            }
            catch { }
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