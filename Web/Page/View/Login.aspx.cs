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
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Users;

namespace RaaiVan.Web.Page.View
{
    public partial class Login : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current, nullTenantResponse: !RaaiVanSettings.SAASBasedMultiTenancy);

            Page.Title = RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID);
            
            try
            {
                if (!RaaiVanSettings.SAASBasedMultiTenancy && !paramsContainer.ApplicationID.HasValue)
                {
                    PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);
                    PublicMethods.set_rv_global(Page, PublicMethods.fromJSON(PublicConsts.NullTenantResponse));
                    return;
                }

                RaaiVanUtil.initialize(paramsContainer.ApplicationID);

                if (string.IsNullOrEmpty(Request.Params["iamadmin"]) && RaaiVanSettings.ServiceUnavailable)
                    Response.Redirect(PublicConsts.ServiceUnavailablePage);

                string returnUrl = Request.Params["ReturnUrl"];

                if (RaaiVanSettings.IgnoreReturnURLOnLogin(paramsContainer.ApplicationID) && !string.IsNullOrEmpty(returnUrl))
                    Response.Redirect(PublicConsts.LoginPage);

                bool isValid = PublicMethods.check_sys_id();

                bool disableLogin = false;
                string loginErrorMessage = string.Empty;
                bool loggedIn = false;

                bool? local = PublicMethods.parse_bool(Request.Params["Local"]);
                Guid? userId = null;

                string authCookie = string.Empty;
                
                if (RaaiVanUtil.is_authenticated(paramsContainer.ApplicationID, HttpContext.Current))
                {
                    Guid? invitationId = PublicMethods.parse_guid(Request.Params["inv"]);
                    RaaiVanUtil.init_user_application(invitationId, paramsContainer.CurrentUserID.Value);

                    Response.Redirect(PublicConsts.HomePage);
                }
                else if ((!local.HasValue || !local.Value) && RaaiVanSettings.SSO.Enabled(paramsContainer.ApplicationID) &&
                    !(loggedIn = sso_login(ref loginErrorMessage, ref userId, ref authCookie))) disableLogin = true;
                
                PublicMethods.set_page_headers(paramsContainer.ApplicationID, Page, false);

                string[] info = null;
                if (!string.IsNullOrEmpty(RaaiVanSettings.LoginPageInfo(paramsContainer.ApplicationID)))
                    info = RaaiVanSettings.LoginPageInfo(paramsContainer.ApplicationID).Split('|');

                string strLoginMessage = !loggedIn ? string.Empty : RVAPI.get_login_message(paramsContainer.ApplicationID, userId);
                string strLastLogins = !loggedIn ? string.Empty : RVAPI.get_last_logins(paramsContainer.ApplicationID, userId);
                
                string ssoLoginUrl = !RaaiVanSettings.SSO.Enabled(paramsContainer.ApplicationID) ? string.Empty :
                    Modules.Jobs.SSO.get_login_url(paramsContainer.ApplicationID);

                string rvGlobal = "{\"SysID\":\"" + (isValid ? string.Empty : PublicMethods.get_sys_id()) + "\"" +
                    ",\"SystemTitle\":\"" + Base64.encode(RaaiVanSettings.SystemTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SystemName\":\"" + Base64.encode(RaaiVanSettings.SystemName(paramsContainer.ApplicationID)) + "\"" +
                    ",\"SSOLoginURL\":\"" + Base64.encode(ssoLoginUrl) + "\"" +
                    ",\"SSOLoginTitle\":\"" + Base64.encode(RaaiVanSettings.SSO.LoginTitle(paramsContainer.ApplicationID)) + "\"" +
                    ",\"ReturnURL\":\"" + Base64.encode(returnUrl) + "\"" +
                    ",\"SystemVersion\":\"" + PublicMethods.SystemVersion + "\"" +
                    ",\"ShowSystemVersion\":" + RaaiVanSettings.ShowSystemVersion(paramsContainer.ApplicationID).ToString().ToLower() +
                    ",\"LoginPageModel\":\"" + RaaiVanSettings.LoginPageModel(paramsContainer.ApplicationID) + "\"" +
                    ",\"Modules\":" + ConfigUtilities.get_modules_json(paramsContainer.ApplicationID) +
                    ",\"LoggedIn\":" + loggedIn.ToString().ToLower() +
                    ",\"DisableLogin\":" + disableLogin.ToString().ToLower() +
                    ",\"LoginErrorMessage\":\"" + loginErrorMessage + "\"" +
                    ",\"AuthCookie\":" + (string.IsNullOrEmpty(authCookie) ? "null" : authCookie) +
                    ",\"LoginMessage\":\"" + Base64.encode(strLoginMessage) + "\"" +
                    (string.IsNullOrEmpty(strLastLogins) ? string.Empty : ",\"LastLogins\":" + strLastLogins) +
                    ",\"UserSignUp\":" + RaaiVanSettings.UserSignUp(paramsContainer.ApplicationID).ToString().ToLower();

                if (info != null)
                {
                    foreach (string str in info)
                    {
                        string result = get_info_json(str);
                        if (!string.IsNullOrEmpty(result)) rvGlobal += ", " + result;
                    }
                }

                rvGlobal += "}";

                PublicMethods.set_rv_global(Page, PublicMethods.fromJSON(rvGlobal));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "LoginPage_Load", ex, ModuleIdentifier.RV);

                PublicMethods.set_rv_global(Page,
                    PublicMethods.fromJSON("{\"Error\":\"" + Base64.encode(PublicMethods.get_exception(ex)) + "\"}"));
            }
        }

        protected bool sso_auto_redirect(string loginUrl)
        {
            if (RaaiVanSettings.SSO.AutoRedirect(paramsContainer.ApplicationID))
            {
                Response.Redirect(loginUrl);
                return true;
            }
            else return false;
        }

        protected bool sso_login(ref string errorMessage, ref Guid? userId, ref string authCookie)
        {
            if (paramsContainer.Tenant != null && 
                !RaaiVanUtil.pre_login_check(paramsContainer.ApplicationID.Value, ref errorMessage)) return false;

            string loginUrl = Modules.Jobs.SSO.get_login_url(paramsContainer.ApplicationID);
            
            if (string.IsNullOrEmpty(loginUrl)) return false;

            string ticket = Modules.Jobs.SSO.get_ticket(paramsContainer.ApplicationID, HttpContext.Current);

            if (string.IsNullOrEmpty(ticket)) return sso_auto_redirect(loginUrl);

            string username = string.Empty;

            if (!Modules.Jobs.SSO.validate_ticket(paramsContainer.ApplicationID, HttpContext.Current, ticket, ref username))
                return sso_auto_redirect(Modules.Jobs.SSO.get_login_url(paramsContainer.ApplicationID));

            if (!string.IsNullOrEmpty(username))
            {
                userId = UsersController.get_user_id(paramsContainer.ApplicationID, username);

                if (!userId.HasValue && !RaaiVanUtil.new_user(paramsContainer.ApplicationID, username, username, false))
                {
                    errorMessage = Modules.GlobalUtilities.Messages.UserCreationFailed.ToString();
                    return false;
                }
                else if (!userId.HasValue)
                    userId = UsersController.get_user_id(paramsContainer.ApplicationID, username);

                if (userId.HasValue)
                {
                    RaaiVanUtil.logged_in(paramsContainer.ApplicationID, HttpContext.Current, userId.Value, false, false, ref authCookie);

                    //Save Log
                    try
                    {
                        LogController.save_log(paramsContainer.ApplicationID, new Log()
                        {
                            UserID = userId,
                            Date = DateTime.Now,
                            HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                            HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                            Action = Modules.Log.Action.Login,
                            SubjectID = userId,
                            ModuleIdentifier = ModuleIdentifier.USR
                        });
                    }
                    catch { }
                    //end of Save Log

                    return true;
                }
                else
                {
                    errorMessage = Modules.GlobalUtilities.Messages.RetrievingUserFailed.ToString();
                    return false;
                }
            }

            errorMessage = Modules.GlobalUtilities.Messages.LoginFailed.ToString();

            return false;
        }

        protected string get_info_json(string info)
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
                return "\"" + strs[0] + "\":" + get_service_abstract(workflowId, nodeTypeId);
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

        protected string get_service_abstract(Guid workflowId, Guid nodeTypeId)
        {
            string result = string.Empty;

            new WFAPI() { paramsContainer = this.paramsContainer }
                .get_service_abstract(nodeTypeId, workflowId, null, "NoTag", true, ref result);

            return result;
        }
    }
}