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
    public partial class DataImport : System.Web.UI.Page
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

            AuthorizationManager.redirect_if_no_access(AccessRoleName.DataImport,
                PublicMethods.get_current_user_id(), Page);
        }
    }
}