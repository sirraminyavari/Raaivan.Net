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

            if (!paramsContainer.IsAuthenticated ||
                UsersController.get_user(paramsContainer.ApplicationID, paramsContainer.CurrentUserID.Value) == null)
            {
                paramsContainer.redirect_to_login_page();
                return;
            }
            else if (!RaaiVanSettings.SAASBasedMultiTenancy)
                Response.Redirect(PublicConsts.HomePage);
        }
    }
}