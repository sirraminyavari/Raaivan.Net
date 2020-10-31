using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web.Page.View
{
    public partial class Error : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            Page.Title = RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID);

            PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);

            int code = 0;
            if (!int.TryParse(Request.Params["code"], out code) || code <= 0) code = 0;

            initialJson.Value = "{\"Code\":" + (code <= 0 ? "null" : code.ToString()) + "}";
            
            //Response.TrySkipIisCustomErrors = true;
            if(code > 0) Response.StatusCode = code;
            if (code == 404) Response.StatusDescription = "Page not found";
        }
    }
}