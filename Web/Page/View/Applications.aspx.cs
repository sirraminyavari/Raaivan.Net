using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;

namespace RaaiVan.Web.Page.View
{
    public partial class Applications : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            User user = null;

            if (!paramsContainer.IsAuthenticated ||
                (user = UsersController.get_user(paramsContainer.ApplicationID, paramsContainer.CurrentUserID.Value)) == null)
            {
                paramsContainer.redirect_to_login_page();
                return;
            }
            else if (!RaaiVanSettings.SAASBasedMultiTenancy)
                Response.Redirect(PublicConsts.HomePage);

            string theme = RaaiVanSettings.EnableThemes(paramsContainer.ApplicationID) ?
                    UsersController.get_theme(paramsContainer.ApplicationID, paramsContainer.CurrentUserID.Value) : string.Empty;

            PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false, theme);

            string imageUrl = DocumentUtilities.get_personal_image_address(paramsContainer.ApplicationID, user.UserID.Value);

            string strCurrentUser = "{\"UserID\":\"" + user.UserID.ToString() + "\"" +
                    ",\"FirstName\":\"" + Base64.encode(user.FirstName) + "\"" +
                    ",\"LastName\":\"" + Base64.encode(user.LastName) + "\"" +
                    ",\"UserName\":\"" + Base64.encode(user.UserName) + "\"" +
                    ",\"ImageURL\":\"" + imageUrl + "\"" +
                    "}";

            string rvGlobal = "{\"UserID\":\"" + user.UserID.ToString() + "\"" +
                ",\"CurrentUserID\":\"" + user.UserID.ToString() + "\"" +
                ",\"CurrentUser\":" + strCurrentUser +
                ",\"AccessToken\":\"" + AccessTokenList.new_token(HttpContext.Current) + "\"" +
                ",\"IsAuthenticated\":" + paramsContainer.IsAuthenticated.ToString().ToLower() +
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
    }
}