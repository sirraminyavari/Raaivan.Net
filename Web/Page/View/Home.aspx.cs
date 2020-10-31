using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class Home : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                if (!paramsContainer.IsAuthenticated)
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                if (!paramsContainer.ApplicationID.HasValue) return;

                Guid currentUserId = paramsContainer.CurrentUserID.HasValue ? paramsContainer.CurrentUserID.Value : Guid.Empty;
                Guid userId = Guid.Empty;

                if (!Guid.TryParse(Request.Params["uid"], out userId) && !Guid.TryParse(Request.Params["UserID"], out userId)) userId = currentUserId;
                else Response.Redirect(PublicConsts.ProfilePage + "?" + Request.QueryString);

                User _profile = null;
                if (userId == Guid.Empty || currentUserId == Guid.Empty ||
                    (_profile = UsersController.get_user(paramsContainer.Tenant.Id, userId)) == null)
                    Response.Redirect(PublicConsts.NoAccessPage);

                Guid senderUserId = Guid.Empty;

                string personalPagePriorities = "{\"Left\":[\"" + string.Join("\",\"", RaaiVanSettings.PersonalPagePriorities.Left(paramsContainer.Tenant.Id)) +
                    "\"],\"Center\":[\"" + string.Join("\",\"", RaaiVanSettings.PersonalPagePriorities.Center(paramsContainer.Tenant.Id)) +
                    "\"],\"Right\":[\"" + string.Join("\",\"", RaaiVanSettings.PersonalPagePriorities.Right(paramsContainer.Tenant.Id)) + "\"]}";

                Friend fs = UsersController.get_friendship_status(paramsContainer.Tenant.Id, userId, currentUserId);
                if (fs != null && fs.AreFriends.HasValue)
                    senderUserId = fs.IsSender.HasValue && fs.IsSender.Value ? currentUserId : userId;

                NodeMember nm = paramsContainer.IsAuthenticated ?
                    CNController.get_user_department(paramsContainer.Tenant.Id, userId) : null;

                initialJson.Value = "{\"PersonalPagePriorities\":" + personalPagePriorities +
                    ",\"User\":{\"UserID\":\"" + userId.ToString() + "\"" +
                        ",\"FirstName\":\"" + Base64.encode(_profile.FirstName) + "\"" +
                        ",\"LastName\":\"" + Base64.encode(_profile.LastName) + "\"" +
                        ",\"JobTitle\":\"" + Base64.encode(_profile.JobTitle) + "\"" +
                        ",\"ProfileImageURL\":\"" +
                            DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, userId) + "\"" +
                        ",\"CoverPhotoURL\":\"" +
                            DocumentUtilities.get_cover_photo_url(paramsContainer.Tenant.Id, userId, false, false) + "\"" +
                        ",\"HighQualityCoverPhotoURL\":\"" +
                            DocumentUtilities.get_cover_photo_url(paramsContainer.Tenant.Id, userId, false, true) + "\"" +
                        ",\"GroupID\":" + (nm != null && nm.Node.NodeID.HasValue ? "\"" + nm.Node.NodeID.ToString() + "\"" : "null") +
                        ",\"GroupName\":" + (nm != null && nm.Node.NodeID.HasValue ? "\"" + Base64.encode(nm.Node.Name) + "\"" : "null") +
                    "}" +
                    "}";
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "UserHomePage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}