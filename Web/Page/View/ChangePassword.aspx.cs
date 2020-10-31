using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);

                bool isAuthenticated = paramsContainer.IsAuthenticated;

                if (!isAuthenticated)
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                string reason = string.Empty;
                bool passwordChangeNeeded = RaaiVanUtil.password_change_needed(HttpContext.Current, ref reason);

                initialJson.Value = "{\"AccessToken\":\"" + AccessTokenList.new_token(HttpContext.Current) + "\"" +
                    ",\"IsAuthenticated\":" + isAuthenticated.ToString().ToLower() +
                    ",\"SystemVersion\":\"" + PublicMethods.SystemVersion + "\"" +
                    ",\"ShowSystemVersion\":" + RaaiVanSettings.ShowSystemVersion(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"UserSignUp\":" + RaaiVanSettings.UserSignUp(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"Themes\":" + (RaaiVanSettings.EnableThemes(paramsContainer.ApplicationID) ? PublicMethods.get_themes() : false.ToString().ToLower()) +
                    ",\"SystemName\":\"" + Base64.encode(RaaiVanSettings.SystemName(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SystemTitle\":\"" + Base64.encode(RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"Modules\":" + ConfigUtilities.get_modules_json(paramsContainer.ApplicationID) +
                    ",\"PasswordChangeNeeded\":" + passwordChangeNeeded.ToString().ToLower() +
                    ",\"PasswordChangeReason\":\"" + reason + "\"" +
                    ",\"Notifications\":{\"SeenTimeout\":" + RaaiVanSettings.Notifications.SeenTimeout(paramsContainer.ApplicationID).ToString() +
                        ",\"UpdateInterval\":" + RaaiVanSettings.Notifications.UpdateInterval(paramsContainer.ApplicationID).ToString() + "}" +
                    "}";
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "ChangePasswordPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}