using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Wiki;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class Profile : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                bool isAuthenticated = paramsContainer.IsAuthenticated;
                if (!isAuthenticated && !RaaiVanSettings.AllowNotAuthenticatedUsers(paramsContainer.ApplicationID))
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                if (!paramsContainer.ApplicationID.HasValue && !RaaiVanSettings.SAASBasedMultiTenancy) return;

                Guid currentUserId = paramsContainer.CurrentUserID.HasValue ? paramsContainer.CurrentUserID.Value : Guid.Empty;
                Guid userId = !string.IsNullOrEmpty(Request.Params["UserID"]) ? Guid.Parse(Request.Params["UserID"]) :
                    (!string.IsNullOrEmpty(Request.Params["uid"]) ? Guid.Parse(Request.Params["uid"]) : currentUserId);

                User _profile = null;
                if (userId == Guid.Empty ||
                    (_profile = UsersController.get_user(paramsContainer.ApplicationID, userId)) == null)
                {
                    Response.Write("پروفایل شما کامل نیست. لطفا به مدیر سیستم مراجعه فرمایید :)");
                    Response.End();
                    //Response.Redirect(PublicConsts.NoAccessPage);
                }

                Guid senderUserId = Guid.Empty;
                Friend fs = isAuthenticated && paramsContainer.ApplicationID.HasValue ?
                    UsersController.get_friendship_status(paramsContainer.ApplicationID.Value, userId, currentUserId) : null;
                if (fs != null && fs.AreFriends.HasValue)
                    senderUserId = fs.IsSender.HasValue && fs.IsSender.Value ? currentUserId : userId;

                NodeMember nm = isAuthenticated && paramsContainer.ApplicationID.HasValue ?
                    CNController.get_user_department(paramsContainer.ApplicationID.Value, userId) : null;

                initialJson.Value =
                    "{\"User\":" + (userId == Guid.Empty ? "null" :
                            "{\"UserID\":\"" + userId.ToString() + "\"" +
                            ",\"FirstName\":\"" + Base64.encode(_profile.FirstName) + "\"" +
                            ",\"LastName\":\"" + Base64.encode(_profile.LastName) + "\"" +
                            ",\"JobTitle\":\"" + Base64.encode(_profile.JobTitle) + "\"" +
                            ",\"ProfileImageURL\":\"" +
                                DocumentUtilities.get_personal_image_address(paramsContainer.ApplicationID, userId) + "\"" +
                            ",\"HighQualityImageURL\":\"" +
                                DocumentUtilities.get_personal_image_address(paramsContainer.ApplicationID, userId, false, true) + "\"" +
                            ",\"CoverPhotoURL\":\"" +
                                DocumentUtilities.get_cover_photo_url(paramsContainer.ApplicationID, userId, false, false) + "\"" +
                            ",\"HighQualityCoverPhotoURL\":\"" +
                                DocumentUtilities.get_cover_photo_url(paramsContainer.ApplicationID, userId, false, true) + "\"" +
                            ",\"GroupID\":" + (nm != null && nm.Node.NodeID.HasValue ? "\"" + nm.Node.NodeID.ToString() + "\"" : "null") +
                            ",\"GroupName\":" + (nm != null && nm.Node.NodeID.HasValue ? "\"" + Base64.encode(nm.Node.Name) + "\"" : "null") +
                        "}") +
                    ",\"ActiveTab\":" + (string.IsNullOrEmpty(Request.Params["Tab"]) ? "null" : "\"" + Request.Params["Tab"] + "\"") +
                    ",\"IsOwnPage\":" + (userId == currentUserId).ToString().ToLower() +
                    ",\"AreFriends\":" + (userId == currentUserId || fs == null ? false : fs.AreFriends.HasValue && fs.AreFriends.Value).ToString().ToLower() +
                    ",\"FriendRequestSenderUserID\":\"" + (senderUserId == Guid.Empty ? string.Empty : senderUserId.ToString()) + "\"" +
                    ",\"EmploymentTypes\":[" + string.Join(",", Enum.GetNames(typeof(EmploymentType)).Where(
                        u => u != EmploymentType.NotSet.ToString()).Select(v => "\"" + v + "\"")) + "]" +
                    ",\"PhoneNumberTypes\":[" + string.Join(",", Enum.GetNames(typeof(PhoneType)).Where(
                        u => u != PhoneType.NotSet.ToString()).Select(v => "\"" + v + "\"")) + "]" +
                    ",\"HasWikiTitle\":" + (paramsContainer.ApplicationID.HasValue && 
                        WikiController.has_title(paramsContainer.ApplicationID.Value, userId, currentUserId)).ToString().ToLower() +
                    ",\"HasWikiParagraph\":" + (paramsContainer.ApplicationID.HasValue && 
                        WikiController.has_paragraph(paramsContainer.ApplicationID.Value, userId, currentUserId)).ToString().ToLower() +
                    "}";

                //check if user do not visit self page
                if (paramsContainer.ApplicationID.HasValue && !IsPostBack && currentUserId != Guid.Empty && 
                    userId != currentUserId && Session[userId.ToString()] == null)
                {
                    ////register user page visitor
                    Session[userId.ToString()] = true;
                    UsersController.register_item_visit(paramsContainer.ApplicationID.Value,
                        userId, currentUserId, DateTime.Now, VisitItemTypes.User);
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "UserPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}