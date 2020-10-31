using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;

namespace RaaiVan.Web.Page.View
{
    public partial class QATag : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                if (!RaaiVanSettings.AllowNotAuthenticatedUsers(paramsContainer.ApplicationID) && !paramsContainer.IsAuthenticated)
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                if (!paramsContainer.ApplicationID.HasValue) return;

                if (!Modules.RaaiVanConfig.Modules.QA(paramsContainer.Tenant.Id)) Response.Redirect(PublicConsts.HomePage);

                Guid currentUserId = paramsContainer.CurrentUserID.HasValue ? paramsContainer.CurrentUserID.Value : Guid.Empty;

                Guid tagId = Guid.Empty;
                Guid.TryParse(Request.Params["id"], out tagId);
                if (tagId == Guid.Empty) Guid.TryParse(Request.Params["TagID"], out tagId);

                if (tagId != Guid.Empty)
                {
                    Modules.CoreNetwork.Node node = CNController.get_node(paramsContainer.Tenant.Id, tagId);

                    initialJson.Value = "{\"TagID\":\"" + tagId.ToString() + "\"" + 
                        ",\"Name\":\"" + Base64.encode(node.Name) + "\"" + "}";
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "QATagPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}