using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.Reports;

namespace RaaiVan.Web.Page.View
{
    public partial class Reports : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            if (!paramsContainer.ApplicationID.HasValue) return;

            AuthorizationManager.redirect_if_no_access(AccessRoleName.Reports, paramsContainer.CurrentUserID, Page);

            List<Guid> lst = ReportUtilities.ReportIDs.Select(u => u.Value).ToList();

            if (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value))
            {
                lst = PrivacyController.check_access(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, lst,
                    PrivacyObjectType.Report, PermissionType.View);
            }

            List<Privacy> privacy = PrivacyController.get_settings(paramsContainer.Tenant.Id, 
                ReportUtilities.ReportIDs.Select(u => u.Value).ToList());
            
            initialJson.Value = "{\"Reports\":{" + string.Join(",", lst.Select(
                u => "\"" + ReportUtilities.ReportIDs.Where(x => x.Value == u).Select(a => a.Key).First().ToLower() + "\":" +
                "{\"ID\":\"" + u.ToString() + "\"" + 
                ",\"Confidentiality\":" + (!privacy.Any(x => x.ObjectID == u) ? "null" :
                privacy.Where(x => x.ObjectID == u).FirstOrDefault().Confidentiality.toJson()) + "}")) + "}}";
        }
    }
}