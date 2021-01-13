﻿using System;
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
            initialJson.Value = PublicMethods.toJSON(RouteList.get_data_server_side(paramsContainer, RouteName.error));

            Page.Title = RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID);
            PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);
        }
    }
}