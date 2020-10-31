using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web.Page.View
{
    public partial class Search : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string searchText = string.IsNullOrEmpty(Request.Params["SearchText"]) ? string.Empty :
                Request.Params["SearchText"].Replace('_', '/').Replace('~', '+');
            
            initialJson.Value = "{\"SearchText\":\"" + searchText + "\"}";
        }
    }
}