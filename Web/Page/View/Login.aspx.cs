using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using RaaiVan.Web.API;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web.Page.View
{
    public partial class Login : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);
            RouteList.get_data_server_side(paramsContainer, RouteName.login);

            Page.Title = RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID);
            PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);

            string returnUrl = Request.Params["ReturnUrl"];

            if (RaaiVanSettings.IgnoreReturnURLOnLogin(paramsContainer.ApplicationID) && !string.IsNullOrEmpty(returnUrl))
                Response.Redirect(PublicConsts.LoginPage);
        }

        public static string get_info_json(ParamsContainer paramsContainer, string info)
        {
            if (!paramsContainer.ApplicationID.HasValue) return string.Empty;

            if (info.ToLower().Contains("wfabstract"))
            {

                string[] strs = info.Split(':');
                if (strs.Length < 3) return string.Empty;
                Guid workflowId = Guid.Empty;
                Guid nodeTypeId = Guid.Empty;
                if (!Guid.TryParse(strs[1], out workflowId)) return string.Empty;
                if (!Guid.TryParse(strs[2], out nodeTypeId)) return string.Empty;
                return "\"" + strs[0] + "\":" + get_service_abstract(paramsContainer, workflowId, nodeTypeId);
            }
            else if (info.ToLower().Contains("modern_28_1"))
            {
                Dictionary<string, object> statistics = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id, null, null);
                Dictionary<string, object> lastMonth = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id,
                    DateTime.Now.AddDays(-30), null);

                if (lastMonth.ContainsKey("ActiveUsersCount")) statistics["ActiveUsersCount"] = lastMonth["ActiveUsersCount"];

                statistics["OnlineUsersCount"] = Membership.GetNumberOfUsersOnline();

                ArrayList lst = GlobalController.get_last_content_creators(paramsContainer.Tenant.Id, 10);

                for (int i = 0, lnt = lst.Count; i < lnt; ++i)
                {
                    Dictionary<string, object> item = (Dictionary<string, object>)lst[i];

                    if (item.ContainsKey("UserID")) item["ProfileImageURL"] =
                            DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, (Guid)item["UserID"]);
                    if (item.ContainsKey("Date"))
                    {
                        DateTime dt = (DateTime)item["Date"];
                        item["Date"] = PublicMethods.get_local_date(dt);
                        item["Date_Gregorian"] = dt.ToString();
                    }

                    lst[i] = item;
                }

                Dictionary<string, object> dic = new Dictionary<string, object>();

                dic["Users"] = lst;
                dic["Statistics"] = statistics;

                return "\"modern_28_1\":" + PublicMethods.toJSON(dic);
            }
            else if (info.ToLower().Contains("modern_29_1"))
            {
                Dictionary<string, object> statistics = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id, null, null);
                Dictionary<string, object> lastMonth = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id,
                    DateTime.Now.AddDays(-30), null);
                Dictionary<string, object> previousMonth = GlobalController.raaivan_statistics(paramsContainer.Tenant.Id,
                    DateTime.Now.AddDays(-60), DateTime.Now.AddDays(-30));

                Dictionary<string, object> dic = new Dictionary<string, object>();
                Dictionary<string, object> stats = new Dictionary<string, object>();

                stats["Total"] = statistics;
                stats["LastMonth"] = lastMonth;
                stats["PreviousMonth"] = previousMonth;

                dic["Statistics"] = stats;

                return "\"modern_29_1\":" + PublicMethods.toJSON(dic);
            }

            return string.Empty;
        }

        private static string get_service_abstract(ParamsContainer paramsContainer, Guid workflowId, Guid nodeTypeId)
        {
            string result = string.Empty;

            new WFAPI() { paramsContainer = paramsContainer }
                .get_service_abstract(nodeTypeId, workflowId, null, "NoTag", true, ref result);

            return result;
        }
    }
}