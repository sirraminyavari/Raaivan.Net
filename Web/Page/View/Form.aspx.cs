using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Web.API;
using RaaiVan.Modules.FormGenerator;

namespace RaaiVan.Web.Page.View
{
    public partial class Form : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                if (!paramsContainer.ApplicationID.HasValue) return;

                Guid? id = null;
                if (!string.IsNullOrEmpty(Request.Params["ID"])) id = Guid.Parse(Request.Params["ID"]);
                if (!id.HasValue && !string.IsNullOrEmpty(Request.Params["InstanceID"]))
                    id = Guid.Parse(Request.Params["InstanceID"]);

                Guid userId = paramsContainer.CurrentUserID.HasValue ? paramsContainer.CurrentUserID.Value : Guid.Empty;

                Poll poll = !id.HasValue ? null : FGController.get_poll(paramsContainer.Tenant.Id, id.Value);

                //if (!FGController.is_director(instanceId.HasValue ? instanceId.Value : Guid.Empty, userId)) 
                //    Response.Redirect(publicCode.NoAccessPage);

                Guid? refInstanceId = null;
                if (!string.IsNullOrEmpty(Request.Params["RefInstanceID"]))
                    refInstanceId = Guid.Parse(Request.Params["RefInstanceID"]);
                
                initialJson.Value = "{\"InstanceID\":\"" + (id.HasValue ? id.Value.ToString() : string.Empty) + "\"" +
                    ",\"RefInstanceID\":\"" + (refInstanceId.HasValue ? refInstanceId.Value.ToString() : string.Empty) + "\"" +
                    ",\"IsPoll\":" + (poll != null && poll.PollID.HasValue).ToString().ToLower() +
                    (poll == null || !poll.PollID.HasValue ? string.Empty : ",\"Poll\":" + poll.toJson()) +
                    "}";
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "FormViewPage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}