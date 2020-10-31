using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Web.API;
using RaaiVan.Modules.Users;

namespace RaaiVan.Web.Page.Setting
{
    public partial class Users : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            if (!paramsContainer.IsAuthenticated)
            {
                paramsContainer.redirect_to_login_page();
                return;
            }

            AuthorizationManager.redirect_if_no_access(AccessRoleName.UsersManagement,
                PublicMethods.get_current_user_id(), Page);

            initialJson.Value = "{\"Reauthenticate\":" + RaaiVanSettings
                .ReautheticationForSensitivePages.UsersAdmin(paramsContainer.Tenant.Id).ToString().ToLower() + "}";
        }
    }
}