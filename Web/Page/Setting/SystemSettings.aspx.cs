using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.Setting
{
    public partial class SystemSettings : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            if (!RaaiVanUtil.is_authenticated(paramsContainer.ApplicationID, HttpContext.Current) || 
                !paramsContainer.CurrentUserID.HasValue ||
                !PublicMethods.is_system_admin(paramsContainer.ApplicationID, paramsContainer.CurrentUserID.Value))
                Response.Redirect(PublicConsts.NoAccessPage);

            initialJson.Value = "{\"Reauthenticate\":" + RaaiVanSettings
                .ReautheticationForSensitivePages.SettingsAdmin(paramsContainer.Tenant.Id).ToString().ToLower() + "}";
        }
    }
}