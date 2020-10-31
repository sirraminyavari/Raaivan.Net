using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class Explorer : System.Web.UI.Page
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
        }
    }
}