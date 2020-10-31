using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class Posts : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                if (!paramsContainer.ApplicationID.HasValue) return;

                string strOwnerId = string.IsNullOrEmpty(Request.Params["OwnerID"]) ? string.Empty : Request.Params["OwnerID"];
                string ownerType = string.IsNullOrEmpty(Request.Params["OwnerType"]) ? string.Empty : Request.Params["OwnerType"];
                string strPostId = string.IsNullOrEmpty(Request.Params["PostID"]) ? string.Empty : Request.Params["PostID"];

                initialJson.Value = "{\"OwnerID\":\"" + strOwnerId + "\",\"OwnerType\":\"" + ownerType +
                    "\",\"PostID\":\"" + strPostId + "\"}";
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "PostPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}