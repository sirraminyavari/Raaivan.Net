using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using RaaiVan.Web.API;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web.Page.Master
{
    public partial class TopMaster : System.Web.UI.MasterPage
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, paramsContainer.IsAuthenticated);

        }
    }
}