using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using RaaiVan.Web.API;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;

namespace RaaiVan.Web.Page.Master
{
    public partial class TopMaster : System.Web.UI.MasterPage
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                RaaiVanUtil.initialize(paramsContainer.ApplicationID);

                if (paramsContainer.Tenant == null)
                {
                    if (RaaiVanSettings.SAASBasedMultiTenancy)
                        RaaiVanUtil.redirect_to_teams_page(Page);
                    else
                    {
                        PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);
                        PublicMethods.set_rv_global(Page, PublicConsts.NullTenantResponse);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(Request.Params["iamadmin"]) && RaaiVanSettings.ServiceUnavailable)
                    Response.Redirect(PublicConsts.ServiceUnavailablePage);
                
                bool isAuthenticated = RaaiVanUtil.is_authenticated(paramsContainer.ApplicationID, HttpContext.Current);

                string reason = string.Empty;
                if (isAuthenticated && RaaiVanUtil.password_change_needed(HttpContext.Current, ref reason))
                    Response.Redirect(PublicConsts.ChangePasswordPage);

                if (!isAuthenticated && !RaaiVanSettings.AllowNotAuthenticatedUsers(paramsContainer.ApplicationID))
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                Guid currentUserId = isAuthenticated ? paramsContainer.CurrentUserID.Value : Guid.Empty;
                User _user = new User();

                if (isAuthenticated)
                {
                    _user = UsersController.get_user(paramsContainer.ApplicationID, currentUserId);

                    if (_user == null && currentUserId != Guid.Empty)
                    {
                        UsersController.set_first_and_last_name(paramsContainer.ApplicationID,
                            currentUserId, string.Empty, string.Empty);

                        _user = new User() { UserID = currentUserId, FirstName = string.Empty, LastName = string.Empty };
                    }

                    if (paramsContainer.ApplicationID.HasValue)
                        RaaiVanUtil.redirect_if_profile_not_completed(_user, Page);
                }

                string theme = isAuthenticated && RaaiVanSettings.EnableThemes(paramsContainer.ApplicationID) ?
                    UsersController.get_theme(paramsContainer.ApplicationID, currentUserId) : string.Empty;

                PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, isAuthenticated, theme);

                Guid? userId = PublicMethods.parse_guid(Request.Params["UserID"]);
                if (!userId.HasValue) userId = PublicMethods.parse_guid(Request.Params["uid"]);

                string imageUrl = DocumentUtilities.get_personal_image_address(paramsContainer.ApplicationID, currentUserId);

                string strCurrentUser = currentUserId == Guid.Empty ? "{\"ImageURL\":\"" + imageUrl + "\"}" :
                    "{\"UserID\":\"" + currentUserId.ToString() + "\"" +
                    ",\"FirstName\":\"" + Base64.encode(_user.FirstName) + "\"" +
                    ",\"LastName\":\"" + Base64.encode(_user.LastName) + "\"" +
                    ",\"UserName\":\"" + Base64.encode(_user.UserName) + "\"" +
                    ",\"ImageURL\":\"" + imageUrl + "\"" +
                    "}";

                string lastVersionSeen = !isAuthenticated || !paramsContainer.ApplicationID.HasValue ? string.Empty :
                    GlobalController.get_variable(paramsContainer.ApplicationID.Value, currentUserId.ToString() + "_LastVersionSeen");

                string rvGlobal = "{\"ApplicationID\":" + (!paramsContainer.ApplicationID.HasValue ? "null" : "\"" + paramsContainer.ApplicationID.ToString() + "\"") + 
                    ",\"UserID\":\"" + (userId.HasValue ? userId.ToString() : (isAuthenticated ? currentUserId.ToString() : string.Empty)) + "\"" +
                    ",\"CurrentUserID\":\"" + (isAuthenticated ? currentUserId.ToString() : string.Empty) + "\"" +
                    ",\"CurrentUser\":" + strCurrentUser +
                    ",\"AccessToken\":\"" + AccessTokenList.new_token(HttpContext.Current) + "\"" +
                    ",\"IsSystemAdmin\":" + (isAuthenticated &&
                        PublicMethods.is_system_admin(paramsContainer.ApplicationID, currentUserId)).ToString().ToLower() +
                    ",\"IsAuthenticated\":" + isAuthenticated.ToString().ToLower() +
                    ",\"SystemVersion\":\"" + PublicMethods.SystemVersion + "\"" +
                    (!isAuthenticated ? string.Empty :
                        ",\"LastVersionSeen\":" + (string.IsNullOrEmpty(lastVersionSeen) ? "{}" : lastVersionSeen)
                    ) +
                    ",\"ShowSystemVersion\":" + RaaiVanSettings.ShowSystemVersion(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"UserSignUp\":" + RaaiVanSettings.UserSignUp(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"EnableThemes\":" + RaaiVanSettings.EnableThemes(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"Theme\":\"" + theme + "\"" +
                    ",\"BackgroundColor\":\"" + RaaiVanSettings.BackgroundColor(paramsContainer.ApplicationID) + "\"" +
                    ",\"ColorfulBubbles\":" + RaaiVanSettings.ColorfulBubbles(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"SystemName\":\"" + Base64.encode(RaaiVanSettings.SystemName(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SystemTitle\":\"" + Base64.encode(RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"Modules\":" + ConfigUtilities.get_modules_json(paramsContainer.ApplicationID) +
                    //",\"OnlineUsersCount\":" + Membership.GetNumberOfUsersOnline().ToString() +
                    ",\"Notifications\":{\"SeenTimeout\":" + RaaiVanSettings.Notifications.SeenTimeout(paramsContainer.ApplicationID).ToString() +
                        ",\"UpdateInterval\":" + RaaiVanSettings.Notifications.UpdateInterval(paramsContainer.ApplicationID).ToString() + "}" +
                    ",\"SSOLoginURL\":\"" + (!RaaiVanSettings.SSO.Enabled(paramsContainer.ApplicationID) ? string.Empty :
                        RaaiVanSettings.SSO.LoginURL(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SSOLoginTitle\":\"" + Base64.encode(RaaiVanSettings.SSO.LoginTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SAASBasedMultiTenancy\":" + RaaiVanSettings.SAASBasedMultiTenancy.ToString().ToLower() +
                    "}";

                PublicMethods.set_rv_global(Page, rvGlobal);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "MasterPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}