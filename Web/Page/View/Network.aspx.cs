using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class Network : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            var isAuthenticated = paramsContainer.IsAuthenticated;

            if (!isAuthenticated)
            {
                paramsContainer.redirect_to_login_page();
                return;
            }
            else if (!Modules.RaaiVanConfig.Modules.SocialNetwork(paramsContainer.ApplicationID)) Response.Redirect(PublicConsts.HomePage);
        }
    }
}