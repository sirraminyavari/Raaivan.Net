using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Web.Page.View
{
    public partial class Graph : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);
            
            try
            {
                if (paramsContainer.Tenant == null)
                {
                    if (RaaiVanSettings.SAASBasedMultiTenancy)
                        Response.Redirect(PublicConsts.ApplicationsPage);
                    else
                    {
                        PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);
                        PublicMethods.set_rv_global(Page, PublicConsts.NullTenantResponse);
                    }

                    return;
                }

                if (string.IsNullOrEmpty(Request.Params["iamadmin"]) && RaaiVanSettings.ServiceUnavailable)
                    Response.Redirect(PublicConsts.ServiceUnavailablePage);

                bool isAuthenticated = RaaiVanUtil.is_authenticated(paramsContainer.ApplicationID, HttpContext.Current);

                string reason = string.Empty;
                if (isAuthenticated && RaaiVanUtil.password_change_needed(HttpContext.Current, ref reason))
                    Response.Redirect(PublicConsts.ChangePasswordPage);

                if (!isAuthenticated && !RaaiVanSettings.AllowNotAuthenticatedUsers(paramsContainer.Tenant.Id))
                    FormsAuthentication.RedirectToLoginPage("ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.AbsolutePath));

                RaaiVanUtil.redirect_if_profile_not_completed(paramsContainer.Tenant.Id, Page);

                Guid currentUserId = isAuthenticated ? paramsContainer.CurrentUserID.Value : Guid.Empty;

                string theme = isAuthenticated && RaaiVanSettings.EnableThemes(paramsContainer.Tenant.Id) ?
                    UsersController.get_theme(paramsContainer.Tenant.Id, currentUserId) : string.Empty;
                PublicMethods.set_page_headers(paramsContainer.Tenant.Id, Page, isAuthenticated, theme);

                if (string.IsNullOrEmpty(Request.Params["iamadmin"]) && RaaiVanSettings.ServiceUnavailable)
                    Response.Redirect(PublicConsts.ServiceUnavailablePage);


                User _user = new User();

                if (isAuthenticated)
                {
                    _user = UsersController.get_user(paramsContainer.Tenant.Id, currentUserId);
                    if (_user == null && currentUserId != Guid.Empty)
                        UsersController.set_first_and_last_name(paramsContainer.Tenant.Id,
                            currentUserId, string.Empty, string.Empty);
                    RaaiVanUtil.redirect_if_profile_not_completed(_user, Page);
                }

                Guid? userId = PublicMethods.parse_guid(Request.Params["UserID"]);
                if (!userId.HasValue) userId = PublicMethods.parse_guid(Request.Params["uid"]);

                string imageUrl =
                    DocumentUtilities.get_personal_image_address(paramsContainer.ApplicationID, currentUserId);

                string strCurrentUser = currentUserId == Guid.Empty ? "{\"ImageURL\":\"" + imageUrl + "\"}" :
                    "{\"UserID\":\"" + currentUserId.ToString() + "\"" +
                    ",\"FirstName\":\"" + Base64.encode(_user.FirstName) + "\"" +
                    ",\"LastName\":\"" + Base64.encode(_user.LastName) + "\"" +
                    ",\"UserName\":\"" + Base64.encode(_user.UserName) + "\"" +
                    ",\"ImageURL\":\"" + imageUrl + "\"" +
                    "}";

                string rvGlobal = "{\"UserID\":\"" + (userId.HasValue ? userId.ToString() : (isAuthenticated ? currentUserId.ToString() : string.Empty)) + "\"" +
                    ",\"CurrentUserID\":\"" + (isAuthenticated ? currentUserId.ToString() : string.Empty) + "\"" +
                    ",\"CurrentUser\":" + strCurrentUser +
                    ",\"AccessToken\":\"" + AccessTokenList.new_token(HttpContext.Current) + "\"" +
                    ",\"IsSystemAdmin\":" + (isAuthenticated &&
                        PublicMethods.is_system_admin(paramsContainer.Tenant.Id, currentUserId)).ToString().ToLower() +
                    ",\"IsAuthenticated\":" + isAuthenticated.ToString().ToLower() +
                    ",\"SystemVersion\":\"" + PublicMethods.SystemVersion + "\"" +
                    ",\"ShowSystemVersion\":" + RaaiVanSettings.ShowSystemVersion(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"UserSignUp\":" + RaaiVanSettings.UserSignUp(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"EnableThemes\":" + RaaiVanSettings.EnableThemes(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"Theme\":\"" + theme + "\"" +
                    ",\"SystemName\":\"" + Base64.encode(RaaiVanSettings.SystemName(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SystemTitle\":\"" + Base64.encode(RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"Modules\":" + ConfigUtilities.get_modules_json(paramsContainer.ApplicationID) +
                    ",\"SSOLoginURL\":\"" + (!RaaiVanSettings.SSO.Enabled(paramsContainer.ApplicationID) ? string.Empty :
                        RaaiVanSettings.SSO.LoginURL(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SSOLoginTitle\":\"" + Base64.encode(RaaiVanSettings.SSO.LoginTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"Notifications\":{\"SeenTimeout\":" + RaaiVanSettings.Notifications.SeenTimeout(paramsContainer.ApplicationID).ToString() +
                    ",\"UpdateInterval\":" + RaaiVanSettings.Notifications.UpdateInterval(paramsContainer.ApplicationID).ToString() + "}}";

                PublicMethods.set_rv_global(Page, rvGlobal);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "GraphPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}