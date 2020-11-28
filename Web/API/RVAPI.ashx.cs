﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Collections.Specialized;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.NotificationCenter;
using RaaiVan.Modules.Messaging;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for RVAPI
    /// </summary>
    public class RVAPI : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        ParamsContainer paramsContainer = null;

        public void ProcessRequest(HttpContext context)
        {
            paramsContainer = new ParamsContainer(context, nullTenantResponse: false);

            if (ProcessTenantIndependentRequest(context)) return;

            if (!paramsContainer.ApplicationID.HasValue) {
                paramsContainer.return_response(PublicConsts.NullTenantResponse);
                return;
            }
            
            string responseText = string.Empty;
            string command = PublicMethods.parse_string(context.Request.Params["Command"], false);

            switch (command)
            {
                case "SetOwnerVariable":
                    set_owner_variable(PublicMethods.parse_long(context.Request.Params["ID"]),
                        PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                        PublicMethods.parse_string(context.Request.Params["Name"], false),
                        PublicMethods.parse_string(context.Request.Params["Value"]), ref responseText);
                    paramsContainer.return_response(ref responseText);
                    return;
                case "GetOwnerVariables":
                    get_owner_variables(PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                        PublicMethods.parse_string(context.Request.Params["Name"], false),
                        PublicMethods.parse_bool(context.Request.Params["CurrentUserOnly"]), ref responseText);
                    paramsContainer.return_response(ref responseText);
                    return;
                case "RemoveOwnerVariable":
                    remove_owner_variable(PublicMethods.parse_long(context.Request.Params["ID"]), ref responseText);
                    paramsContainer.return_response(ref responseText);
                    return;
                case "GetJob":
                    {
                        get_job(PublicMethods.parse_string(context.Request.Params["Type"], false), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetJobs":
                    get_jobs(ref responseText);
                    paramsContainer.return_response(ref responseText);
                    return;
                case "StartJob":
                    {
                        start_job(PublicMethods.parse_string(context.Request.Params["Type"], false), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "StopJob":
                    {
                        stop_job(PublicMethods.parse_string(context.Request.Params["Type"], false), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "SetJobTiming":
                    {
                        set_job_timing(PublicMethods.parse_string(context.Request.Params["Type"], false),
                            PublicMethods.parse_int(context.Request.Params["Interval"]),
                            PublicMethods.parse_int(context.Request.Params["StartHour"]),
                            PublicMethods.parse_int(context.Request.Params["StartMinute"]),
                            PublicMethods.parse_int(context.Request.Params["EndHour"]),
                            PublicMethods.parse_int(context.Request.Params["EndMinute"]),
                            ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetLogActions":
                    {
                        get_log_actions(ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "WebRequest":
                    {
                        NameValueCollection values = new NameValueCollection();
                        string[] vals = PublicMethods.parse_string(context.Request.Params["Data"], false).Split('|');
                        foreach (string itm in vals)
                        {
                            string[] parts = itm.Split(':');
                            if (parts.Length > 1) values[Base64.decode(parts[0])] = Base64.decode(parts[1]);
                        }

                        web_request(PublicMethods.parse_string(context.Request.Params["RequestURL"]), values, ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "SuggestTags":
                    {
                        string searchText = PublicMethods.parse_string(context.Request.Params["SearchText"], true,
                            PublicMethods.parse_string(context.Request.Params["text"]));

                        suggest_tags(searchText, PublicMethods.parse_int(context.Request.Params["Count"]),
                            ListMaker.get_guid_items(context.Request.Params["NodeTypeIDs"], '|'), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "Like":
                case "Dislike":
                case "Unlike":
                    {
                        bool? like = null;
                        if (command == "Like") like = true;
                        else if (command == "Dislike") like = false;

                        Liked type = Liked.None;
                        if (!Enum.TryParse<Liked>(PublicMethods.parse_string(
                            context.Request.Params["LikedType"], false), out type)) type = Liked.None;

                        like_dislike_unlike(PublicMethods.parse_guid(context.Request.Params["LikedID"]),
                            like, type, ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetFans":
                    get_fans(PublicMethods.parse_guid(context.Request.Params["LikedID"]), ref responseText);
                    paramsContainer.return_response(ref responseText);
                    return;
                case "Follow":
                case "Unfollow":
                    {
                        bool? follow = null;
                        if (command == "Follow") follow = true;

                        follow_unfollow(PublicMethods.parse_guid(context.Request.Params["FollowedID"]),
                            follow, ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetHelpIndex":
                    {
                        get_help_index(ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetHelpIndexEntry":
                    {
                        get_help_index_entry(PublicMethods.parse_string(context.Request.Params["Name"]), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetHelpMediaFiles":
                    {
                        get_help_media_files(ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "SaveHelpIndexEntry":
                    {
                        save_help_index_entry(PublicMethods.parse_string(context.Request.Params["Lang"]),
                            PublicMethods.parse_string(context.Request.Params["Path"]),
                            PublicMethods.parse_string(context.Request.Params["Content"]), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "SaveSystemSettingsValues":
                    {
                        save_system_setting_values(PublicMethods.fromJSON(context.Request.Params["Settings"]), ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetSystemSettingsValues":
                    {
                        List<string> names = string.IsNullOrEmpty(context.Request.Params["Names"]) ? new List<string>() :
                            context.Request.Params["Names"].Split('|').ToList();

                        List<RVSettingsItem> items = new List<RVSettingsItem>();
                        foreach (string n in names)
                        {
                            RVSettingsItem i;
                            if (Enum.TryParse<RVSettingsItem>(n, out i)) items.Add(i);
                        }

                        get_system_setting_values(items, ref responseText);
                        paramsContainer.return_response(ref responseText);
                        return;
                    }
                case "GetAllNotifications":
                    get_all_notifications(ref responseText);
                    paramsContainer.return_response(ref responseText);
                    return;
            }

            paramsContainer.return_response(PublicConsts.BadRequestResponse);
        }

        public bool ProcessTenantIndependentRequest(HttpContext context)
        {
            if (!RaaiVanSettings.SAASBasedMultiTenancy && !paramsContainer.ApplicationID.HasValue)
            {
                paramsContainer.return_response(PublicConsts.NullTenantResponse);
                return true;
            }

            string responseText = string.Empty;
            string command = PublicMethods.parse_string(context.Request.Params["Command"], false);

            switch (command)
            {
                case "GetApplications":
                    get_appliations(PublicMethods.parse_bool(context.Request.Params["Archive"]), ref responseText);
                    break;
                case "SelectApplication":
                    select_appliation(PublicMethods.parse_guid(context.Request.Params["ApplicationID"]), ref responseText);
                    break;
                case "CreateApplication":
                    create_application(PublicMethods.parse_string(context.Request.Params["Title"]), ref responseText);
                    break;
                case "ModifyApplication":
                    modify_application(PublicMethods.parse_guid(context.Request.Params["ApplicationID"]),
                        PublicMethods.parse_string(context.Request.Params["Title"]), ref responseText);
                    break;
                case "RemoveApplication":
                    remove_application(PublicMethods.parse_guid(context.Request.Params["ApplicationID"]), ref responseText);
                    break;
                case "RecycleApplication":
                    recycle_application(PublicMethods.parse_guid(context.Request.Params["ApplicationID"]), ref responseText);
                    break;
                case "RemoveUserFromApplication":
                    remove_user_from_application(PublicMethods.parse_guid(context.Request.Params["ApplicationID"]),
                        PublicMethods.parse_guid(context.Request.Params["UserID"]), ref responseText);
                    break;
                case "UnsubscribeFromApplication":
                    unsubscribe_from_application(PublicMethods.parse_guid(context.Request.Params["ApplicationID"]), ref responseText);
                    break;
                case "SetVariable":
                    set_variable(PublicMethods.parse_string(context.Request.Params["Name"], false),
                        PublicMethods.parse_string(context.Request.Params["Value"]),
                        PublicMethods.parse_bool(context.Request.Params["ApplicationIndependent"]),
                        ref responseText);
                    break;
                case "GetVariable":
                    get_variable(PublicMethods.parse_string(context.Request.Params["Name"], false),
                        PublicMethods.parse_bool(context.Request.Params["ApplicationIndependent"]),
                        ref responseText);
                    break;
                case "GetDomains":
                    get_domains(ref responseText);
                    break;
                case "Login":
                case "LoginStepTwo":
                    string failureText = string.Empty;

                    int remainingLockoutTime = 0;
                    Guid? userId = null;

                    string authCookie = string.Empty;
                    bool result = false, stepTwoNeeded = false, codeDisposed = false;

                    if (command == "Login")
                    {
                        string captcha = PublicMethods.parse_string(context.Request.Params["Captcha"]);
                        bool hasValidCaptcha = !string.IsNullOrEmpty(captcha) &&
                            Captcha.check(HttpContext.Current, captcha);

                        if (!string.IsNullOrEmpty(captcha) && !hasValidCaptcha)
                            failureText = Messages.CaptchaIsNotValid.ToString();

                        result = string.IsNullOrEmpty(failureText) && RaaiVanUtil.login(paramsContainer.ApplicationID,
                            PublicMethods.parse_string(context.Request.Params["UserName"]),
                            PublicMethods.parse_string(context.Request.Params["Password"]),
                            PublicMethods.parse_string(context.Request.Params["DomainName"]),
                            PublicMethods.parse_bool(context.Request.Params["RememberMe"]),
                            PublicMethods.parse_guid(context.Request.Params["InvitationID"]),
                            hasValidCaptcha,
                            ref failureText, ref remainingLockoutTime, ref stepTwoNeeded, ref userId, ref context, ref authCookie);
                    }
                    else if (command == "LoginStepTwo")
                    {
                        result = RaaiVanUtil.login_step_two(paramsContainer.ApplicationID,
                            PublicMethods.parse_string(context.Request.Params["TwoStepToken"], false),
                            PublicMethods.parse_long(context.Request.Params["Code"]),
                            PublicMethods.parse_bool(context.Request.Params["RememberMe"]),
                            PublicMethods.parse_guid(context.Request.Params["InvitationID"]),
                            ref failureText, ref codeDisposed, ref userId, ref context, ref authCookie);
                    }

                    if (!result)
                    {
                        //Save Log
                        if (!stepTwoNeeded)
                        {
                            try
                            {
                                LogController.save_log(paramsContainer.ApplicationID, new Log()
                                {
                                    UserID = userId,
                                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                                    Action = Modules.Log.Action.Login_Failed,
                                    SubjectID = userId,
                                    Info = "{\"UserName\":\"" + context.Request.Params["UserName"] + "\"" +
                                        ",\"Error\":\"" + Base64.encode(failureText) + "\"" +
                                        ",\"RemainingLockoutTime\":" + remainingLockoutTime.ToString() +
                                        "}",
                                    ModuleIdentifier = ModuleIdentifier.RV
                                });
                            }
                            catch { }
                        }
                        //end of Save Log
                    }
                    else if (userId.HasValue && paramsContainer.ApplicationID.HasValue)
                    {
                        Log lg = LogController.get_logs(paramsContainer.ApplicationID, new List<Guid>() { userId.Value },
                            new List<Modules.Log.Action>() { Modules.Log.Action.Login },
                            null, null, null, 1).FirstOrDefault();

                        if (lg == null && paramsContainer.CurrentUserID.HasValue)
                        {
                            GlobalController.set_variable(paramsContainer.ApplicationID,
                                userId.ToString() + "_LastVersionSeen",
                                "{\"Version\":\"" + PublicMethods.SystemVersion + "\",\"Tour\":\"Seen\"}",
                                paramsContainer.CurrentUserID.Value);
                        }
                    }

                    string loginMessage = !result ? string.Empty : get_login_message(paramsContainer.ApplicationID, userId);
                    string strLastLogins = !result ? string.Empty : get_last_logins(paramsContainer.ApplicationID, userId);

                    if (string.IsNullOrEmpty(failureText) || failureText[0] != '{')
                        failureText = "\"" + failureText + "\"";

                    responseText = result ?
                        "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"" +
                        ",\"LoginMessage\":\"" + Base64.encode(loginMessage) + "\"" +
                        ",\"AuthCookie\":" + (string.IsNullOrEmpty(authCookie) ? "null" : authCookie) +
                        (string.IsNullOrEmpty(strLastLogins) ? string.Empty : ",\"LastLogins\":" + strLastLogins) +
                        "}" :
                        "{\"ErrorText\":" + failureText +
                        ",\"CodeDisposed\":" + codeDisposed.ToString().ToLower() +
                        ",\"RemainingLockoutTime\":" + remainingLockoutTime.ToString() +
                        "}";

                    break;
                case "ResendVerificationCode":
                    responseText = VerificationCode.resend_code(
                        PublicMethods.parse_string(context.Request.Params["VerificationToken"], false)) ?
                        "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                        "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                    break;
                case "Logout":
                    RaaiVanUtil.logout(paramsContainer.ApplicationID);
                    paramsContainer.return_response(ref responseText);
                    return true;
                case "CaptchaImage":
                    int? width = PublicMethods.parse_int(context.Request.Params["Width"], 140);
                    int? height = PublicMethods.parse_int(context.Request.Params["Height"], 40);
                    Captcha.generate(context, width.Value, height.Value);
                    return true;
                case "SetSession":
                    HttpContext.Current.Session["MY_" +
                        PublicMethods.parse_string(context.Request.Params["SessionName"], false, string.Empty)] =
                        PublicMethods.parse_string(context.Request.Params["Value"], false, string.Empty);
                    paramsContainer.return_response(ref responseText);
                    return true;
                case "GetSession":
                    string sessionName = PublicMethods.parse_string(context.Request.Params["SessionName"],
                        false, string.Empty);

                    responseText = "{\"Value\":" + (HttpContext.Current.Session["MY_" + sessionName] == null ? "null" :
                            "\"" + HttpContext.Current.Session["MY_" + sessionName].ToString() + "\"") + "}";
                    break;
                case "GetThemes":
                    get_themes(ref responseText);
                    break;
            }

            if (!string.IsNullOrEmpty(responseText))
                paramsContainer.return_response(ref responseText);

            return !string.IsNullOrEmpty(responseText);
        }

        public static string get_login_message(Guid? applicationId, Guid? userId)
        {
            User usr = null;

            if (string.IsNullOrEmpty(RaaiVanSettings.LoginMessage(applicationId)) || !userId.HasValue || userId == Guid.Empty ||
                (usr = UsersController.get_user(applicationId, userId.Value)) == null) return string.Empty;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["firstname"] = string.IsNullOrEmpty(usr.FirstName) ? string.Empty : usr.FirstName;
            dic["lastname"] = string.IsNullOrEmpty(usr.LastName) ? string.Empty : usr.LastName;
            dic["username"] = string.IsNullOrEmpty(usr.UserName) ? string.Empty : usr.UserName;
            dic["fullname"] = (dic["firstname"] + " " + dic["lastname"]).Trim();
            dic["pdate"] = PublicMethods.get_local_date(DateTime.Now, true);

            return Expressions.replace(RaaiVanSettings.LoginMessage(applicationId), ref dic, Expressions.Patterns.AutoTag);
        }

        public static string get_last_logins(Guid? applicationId, Guid? userId)
        {
            if (RaaiVanSettings.InformLastLogins(applicationId) <= 0 || !userId.HasValue || userId == Guid.Empty)
                return string.Empty;

            List<Log> logs = LogController.get_logs(applicationId, new List<Guid>() { userId.Value },
                new List<Modules.Log.Action>() { Modules.Log.Action.Login, Modules.Log.Action.Login_Failed },
                null, null, null, RaaiVanSettings.InformLastLogins(applicationId) + 1);

            if (logs.Count > 0) logs.RemoveAt(0);

            return logs.Count <= 0 ? string.Empty :
                "[" + ProviderUtil.list_to_string<string>(logs.Select(
                    u => "{\"Action\":\"" + u.Action.ToString() + "\"" +
                    ",\"Date\":\"" + PublicMethods.get_local_date(u.Date.Value, true) + "\"" +
                    ",\"HostAddress\":\"" + u.HostAddress + "\"" +
                    ",\"HostName\":\"" + u.HostName + "\"" +
                    "}").ToList()) + "]";
        }

        public void get_appliations(bool? archive, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!archive.HasValue) archive = false;

            List<Application> apps = !paramsContainer.CurrentUserID.HasValue ? new List<Application>() :
                GlobalController.get_user_applications(paramsContainer.CurrentUserID.Value, isCreator: archive.Value, archive: archive.Value);

            List<ApplicationUsers> appUsers =
                UsersController.get_application_users_partitioned(apps.Select(a => a.ApplicationID.Value).ToList(), 6);

            responseText = "{\"Applications\":[" + string.Join(",", 
                    apps.Select(app => app.toJson(paramsContainer.CurrentUserID, icon: true, highQualityIcon: true))) + "]" +
                ",\"ApplicationUsers\":{" + 
                    string.Join(",", appUsers.Select(a => "\"" + a.ApplicationID.ToString() + "\":" + a.toJson())) + "}" +
                "}";
        }

        public void select_appliation(Guid? applicationId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<Application> apps = !applicationId.HasValue || !paramsContainer.CurrentUserID.HasValue ? new List<Application>() :
                GlobalController.get_user_applications(paramsContainer.CurrentUserID.Value);

            Application app = !applicationId.HasValue ? null : apps.Where(u => u.ApplicationID == applicationId).FirstOrDefault();

            if (app != null) PublicMethods.set_current_application(app);

            responseText = app == null ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}";
        }

        public void create_application(string title, ref string responseText) {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if ((!string.IsNullOrEmpty(title) && title.Length > 250))
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            Application app = new Application()
            {
                ApplicationID = Guid.NewGuid(),
                Name = PublicMethods.random_string(),
                Title = title,
                CreatorUserID = paramsContainer.CurrentUserID
            };
            
            bool result = !string.IsNullOrEmpty(title) && 
                GlobalController.add_or_modify_application(app.ApplicationID.Value, 
                app.Name, title, string.Empty, paramsContainer.CurrentUserID);

            ApplicationUsers au = !result ? null : new ApplicationUsers()
            {
                ApplicationID = app.ApplicationID,
                Count = 1,
                Users = new List<User>() { UsersController.get_user(null, paramsContainer.CurrentUserID.Value) }
            };

            if (result) GlobalController.add_system_admin(app.ApplicationID.Value, paramsContainer.CurrentUserID.Value);

            responseText = !result ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"" + 
                ",\"Application\":" + app.toJson(paramsContainer.CurrentUserID, icon: true, highQualityIcon: false) +
                ",\"ApplicationUsers\":" + au.toJson() +
                "}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.ApplicationID, new Log()
                {
                    UserID = paramsContainer.CurrentUserID.Value,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.CreateApplication,
                    SubjectID = app.ApplicationID.Value,
                    Info = "{\"Title\":\"" + Base64.encode(title) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.RV
                });
            }
            //end of Save Log
        }

        public void modify_application(Guid? applicationId, string title, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if ((!string.IsNullOrEmpty(title) && title.Length > 250))
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            bool result = applicationId.HasValue && !string.IsNullOrEmpty(title) &&
                GlobalController.add_or_modify_application(applicationId.Value, 
                string.Empty, title, string.Empty, paramsContainer.CurrentUserID);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.ApplicationID, new Log()
                {
                    UserID = paramsContainer.CurrentUserID.Value,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyApplication,
                    SubjectID = applicationId.Value,
                    Info = "{\"Title\":\"" + Base64.encode(title) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.RV
                });
            }
            //end of Save Log
        }

        public void remove_application(Guid? applicationId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            Application app = !applicationId.HasValue ? null :
                GlobalController.get_user_applications(paramsContainer.CurrentUserID.Value, isCreator: true)
                .Where(a => a.ApplicationID == applicationId.Value).FirstOrDefault();

            if (app == null) {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool result = GlobalController.remove_application(applicationId.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            if (result) {
                Application curApp = PublicMethods.get_current_application();
                if (curApp != null && curApp.ApplicationID == applicationId) PublicMethods.set_current_application(null);
            }

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.ApplicationID, new Log()
                {
                    UserID = paramsContainer.CurrentUserID.Value,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveApplication,
                    SubjectID = applicationId.Value,
                    ModuleIdentifier = ModuleIdentifier.RV
                });
            }
            //end of Save Log
        }

        public void recycle_application(Guid? applicationId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            Application app = !applicationId.HasValue ? null :
                GlobalController.get_user_applications(paramsContainer.CurrentUserID.Value, isCreator: true, archive: true)
                .Where(a => a.ApplicationID == applicationId.Value).FirstOrDefault();

            if (app == null)
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool result = GlobalController.recycle_application(applicationId.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.ApplicationID, new Log()
                {
                    UserID = paramsContainer.CurrentUserID.Value,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RecycleApplication,
                    SubjectID = applicationId.Value,
                    ModuleIdentifier = ModuleIdentifier.RV
                });
            }
            //end of Save Log
        }

        public void remove_user_from_application(Guid? applicationId, Guid? userId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            Application app = !applicationId.HasValue || !userId.HasValue ? null :
                GlobalController.get_user_applications(paramsContainer.CurrentUserID.Value, isCreator: true, archive: null)
                .Where(a => a.ApplicationID == applicationId.Value).FirstOrDefault();
            
            if (app == null)
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool result = GlobalController.remove_user_from_application(applicationId.Value, userId.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.ApplicationID, new Log()
                {
                    UserID = paramsContainer.CurrentUserID.Value,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveUserFromApplication,
                    SubjectID = applicationId.Value,
                    Info = "{\"UserID\":\"" + userId.Value.ToString() + "\"}",
                    ModuleIdentifier = ModuleIdentifier.RV
                });
            }
            //end of Save Log
        }

        public void unsubscribe_from_application(Guid? applicationId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            bool isCreator = !applicationId.HasValue ? false :
                GlobalController.get_user_applications(paramsContainer.CurrentUserID.Value, isCreator: true, archive: null)
                .Where(a => a.ApplicationID == applicationId.Value).FirstOrDefault() != null;

            bool result = !isCreator && applicationId.HasValue &&
                GlobalController.remove_user_from_application(applicationId.Value, paramsContainer.CurrentUserID.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.ApplicationID, new Log()
                {
                    UserID = paramsContainer.CurrentUserID.Value,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.UnsubscribeFromApplication,
                    SubjectID = applicationId.Value,
                    ModuleIdentifier = ModuleIdentifier.RV
                });
            }
            //end of Save Log
        }

        protected void get_domains(ref string responseText)
        {
            List<KeyValuePair<string, string>> domains = !paramsContainer.ApplicationID.HasValue ?
                new List<KeyValuePair<string, string>>() : RaaiVanSettings.Domains(paramsContainer.ApplicationID.Value);

            responseText = "{\"Domains\":[" + string.Join(",",
                domains.Select(
                    u => "{\"Value\":\"" + Base64.encode(u.Key) + "\"" + ",\"Text\":\"" + Base64.encode(u.Value) + "\"}"
                ).ToList()) + "]}";
        }

        protected void set_variable(string name, string value, bool? applicationIndependent, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(name) && name.Length > 99)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            Guid? appId = applicationIndependent.HasValue && applicationIndependent.Value ? null : paramsContainer.ApplicationID;

            bool result = GlobalController.set_variable(appId, name, value, paramsContainer.CurrentUserID.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
        }

        protected void get_variable(string name, bool? applicationIndependent, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit)
            {
                responseText = "{}";
                return;
            }

            Guid? appId = applicationIndependent.HasValue && applicationIndependent.Value ? null : paramsContainer.ApplicationID;

            string value = GlobalController.get_variable(appId, name);

            responseText = "{\"Value\":" + (string.IsNullOrEmpty(value) ? "null" : "\"" + Base64.encode(value) + "\"") + "}";
        }

        protected void set_owner_variable(long? id, Guid? ownerId, string name, string value, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(name) && name.Length > 99)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (id.HasValue)
            {
                Variable var = GlobalController.get_owner_variable(paramsContainer.Tenant.Id, id.Value);

                if (var == null || (var.CreatorUserID != paramsContainer.CurrentUserID.Value &&
                    !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value)))
                {
                    responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                    return;
                }
            }

            long? result = !id.HasValue && !ownerId.HasValue ? null : (id.HasValue ?
                GlobalController.set_owner_variable(paramsContainer.Tenant.Id,
                    id.Value, name, value, paramsContainer.CurrentUserID.Value) :
                GlobalController.set_owner_variable(paramsContainer.Tenant.Id,
                    ownerId.Value, name, value, paramsContainer.CurrentUserID.Value));

            responseText = !result.HasValue ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"" +
                ",\"Variable\":" + (new Variable()
                {
                    ID = result.Value,
                    OwnerID = !ownerId.HasValue ? null : ownerId,
                    Name = name,
                    Value = value,
                    CreatorUserID = paramsContainer.CurrentUserID,
                    CreationDate = DateTime.Now
                }).to_json(true) + "}";
        }

        protected void get_owner_variables(Guid? ownerId, string name, bool? currentUserOnly, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBView) return;

            Guid? userId = null;
            if (currentUserOnly.HasValue && currentUserOnly.Value)
            {
                if (!paramsContainer.CurrentUserID.HasValue)
                {
                    responseText = "{\"Variables\":[]}";
                    return;
                }
                else userId = paramsContainer.CurrentUserID.Value;
            }

            List<Variable> vars = !ownerId.HasValue ? new List<Variable>() :
                GlobalController.get_owner_variables(paramsContainer.Tenant.Id, ownerId.Value, name, userId);

            bool isSystemAdmin = PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);

            responseText = "{\"Variables\":[" + string.Join(",", vars
                .Select(u => u.to_json(isSystemAdmin || paramsContainer.CurrentUserID == u.CreatorUserID))) + "]}";
        }

        protected void remove_owner_variable(long? id, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (id.HasValue)
            {
                Variable var = GlobalController.get_owner_variable(paramsContainer.Tenant.Id, id.Value);

                if (var == null || (var.CreatorUserID != paramsContainer.CurrentUserID.Value &&
                    !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value)))
                {
                    responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                    return;
                }
            }

            bool result = id.HasValue && GlobalController.remove_owner_variable(paramsContainer.Tenant.Id,
                    id.Value, paramsContainer.CurrentUserID.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
        }

        protected string _get_job_json(string jobName, RVJob job)
        {
            return "{\"Name\":\"" + jobName + "\"" +
                    ",\"Interval\":" + (job.Interval.HasValue ? job.Interval : 0).ToString() +
                    ",\"StartTime\":" + (!job.StartTime.HasValue ? "null" :
                        "{\"Hour\":" + job.StartTime.Value.Hour.ToString() + ",\"Minute\":" + job.StartTime.Value.Minute.ToString()) + "}" +
                    ",\"EndTime\":" + (!job.EndTime.HasValue ? "null" :
                        "{\"Hour\":" + job.EndTime.Value.Hour.ToString() + ",\"Minute\":" + job.EndTime.Value.Minute.ToString()) + "}" +
                    ",\"LastActivityDate\":\"" + (!job.LastActivityDate.HasValue ? string.Empty :
                        PublicMethods.get_local_date(job.LastActivityDate.Value, true)) + "\"" +
                    ",\"LastActivityDuration\":" + (!job.LastActivityDuration.HasValue ? 0 : job.LastActivityDuration.Value).ToString() +
                    ",\"ErrorMessage\":\"" + Base64.encode(job.ErrorMessage) + "\"" +
                    "}";
        }

        protected void get_job(string jobName, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            RVJob job = RVScheduler.get_job(jobName);

            responseText = job == null ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" : _get_job_json(jobName.ToString(), job);
        }

        protected void get_jobs(ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }
            
            Dictionary<string, RVJob> threads = RVScheduler.get_list();

            responseText = "{\"Jobs\":[" +
                ProviderUtil.list_to_string<string>(threads.Where(x => x.Value.TenantID == paramsContainer.Tenant.Id)
                .Select(u => _get_job_json(u.Key, u.Value)).ToList()) + "]}";
        }

        protected void start_job(string jobName, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            RVScheduler.start(jobName);

            RVJob job = RVScheduler.get_job(jobName);

            responseText = job == null ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" : _get_job_json(jobName.ToString(), job);
        }

        protected void stop_job(string jobName, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            RVScheduler.stop(jobName);

            RVJob job = RVScheduler.get_job(jobName);

            responseText = job == null ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" : _get_job_json(jobName.ToString(), job);
        }

        protected void set_job_timing(string jobName, int? interval,
            int? startHour, int? startMinute, int? endHour, int? endMinute, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            if (RVScheduler.get_job(jobName) == null)
            {
                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                return;
            }
            else if (interval.HasValue && interval <= 0 ||
                    (startHour.HasValue && (startHour > 23 || startHour < 0)) ||
                    (startMinute.HasValue && (startMinute > 59 || startMinute < 0)) ||
                    (endHour.HasValue && (endHour > 23 || endHour < 0)) ||
                    (endMinute.HasValue && (endMinute > 59 || endMinute < 0)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.NumbersAreNotValid + "\"}";
                return;
            }

            DateTime? startTime = null;
            if (startHour.HasValue || startMinute.HasValue)
                startTime = new DateTime(2000, 1, 1, startHour.HasValue ? startHour.Value : 0, startMinute.HasValue ? startMinute.Value : 0, 0);

            DateTime? endTime = null;
            if (endHour.HasValue || endMinute.HasValue)
                endTime = new DateTime(2000, 1, 1, endHour.HasValue ? endHour.Value : 23, endMinute.HasValue ? endMinute.Value : 59, 59);

            RVScheduler.set_timing(jobName, HttpContext.Current, interval, startTime, endTime);

            responseText = _get_job_json(jobName.ToString(), RVScheduler.get_job(jobName));
        }

        public void get_log_actions(ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.Reports, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            responseText = "[" + ProviderUtil.list_to_string<string>(
                LogUtilities.get_actions().Select(u => "\"" + u.ToString() + "\"").ToList()) + "]";
        }

        protected void web_request(string url, NameValueCollection values, ref string responseText)
        {
            responseText = Base64.encode(PublicMethods.web_request(url, values));
        }

        protected void get_themes(ref string responseText)
        {
            responseText = "{\"Themes\":[" + string.Join(",", RaaiVanSettings.Themes.Select(u => u)) + "]}";
        }

        protected void suggest_tags(string searchText, int? count, List<Guid> nodeTypeIds, ref string responseText)
        {
            if (!count.HasValue) count = 10;

            List<User> users = UsersController.get_users(paramsContainer.Tenant.Id, searchText, null, count);

            long totalCount = 0;

            List<Node> nodes = CNController.get_nodes(paramsContainer.Tenant.Id, nodeTypeIds, ref totalCount, null,
                searchText, isDocument: null, isKnowledge: null, count: count.Value * 2, archive: false, searchable: true);

            responseText = "{\"Items\":[";

            bool isFirst = true;

            foreach (User User in users)
            {
                string firstName = string.IsNullOrEmpty(User.FirstName) ? string.Empty : User.FirstName;
                string lastName = string.IsNullOrEmpty(User.LastName) ? string.Empty : User.LastName;

                if (!isFirst) responseText += ",";
                isFirst = false;

                responseText += "{\"ItemID\":\"" + User.UserID.Value.ToString() + "\"" +
                    ",\"AdditionalID\":\"" + Base64.encode(User.UserName) + "\"" +
                    ",\"Name\":\"" + Base64.encode((firstName + " " + lastName).Trim()) + "\"" +
                    ",\"Type\":\"" + "User" + "\"" +
                    ",\"ImageURL\":\"" + DocumentUtilities.get_personal_image_address(
                        paramsContainer.Tenant.Id, User.UserID.Value) + "\"" +
                    "}";
            }

            foreach (Node _node in nodes)
            {
                if (!isFirst) responseText += ",";
                isFirst = false;

                responseText += "{\"ItemID\":\"" + _node.NodeID.Value.ToString() + "\"" +
                    ",\"AdditionalID\":\"" + Base64.encode(_node.AdditionalID) + "\"" +
                    ",\"Name\":\"" + Base64.encode(_node.Name) + "\"" +
                    ",\"Type\":\"" + "Node" + "\"" +
                    ",\"NodeType\":\"" + Base64.encode(_node.NodeType) + "\"" +
                    ",\"ImageURL\":\"" + DocumentUtilities.get_icon_url(paramsContainer.Tenant.Id,
                        _node.NodeID.Value, DefaultIconTypes.Node, _node.NodeTypeID) + "\"" +
                    "}";
            }

            responseText += "]}";
        }

        protected void like_dislike_unlike(Guid? likedId, bool? like, Liked likedType, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            bool result = likedId.HasValue && (!like.HasValue ?
                GlobalController.unlike(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, likedId.Value) :
                (like.Value ? GlobalController.like(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, likedId.Value) :
                GlobalController.dislike(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, likedId.Value)));

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Send Notification
            SubjectType st = SubjectType.None;
            if (!Enum.TryParse<SubjectType>(likedType.ToString(), out st)) st = SubjectType.None;

            if (result && like.HasValue && st != SubjectType.None)
            {
                Notification not = new Notification()
                {
                    SubjectID = likedId,
                    RefItemID = likedId,
                    SubjectType = st,
                    Action = like.Value ? Modules.NotificationCenter.ActionType.Like :
                        Modules.NotificationCenter.ActionType.Dislike,
                    Sender = new User() { UserID = paramsContainer.CurrentUserID }
                };
                NotificationController.send_notification(paramsContainer.Tenant.Id, not);

                not = new Notification()
                {
                    SubjectID = likedId,
                    Action = like.HasValue ? Modules.NotificationCenter.ActionType.Dislike :
                        Modules.NotificationCenter.ActionType.Like,
                    Sender = new User() { UserID = paramsContainer.CurrentUserID }
                };
                NotificationController.remove_notifications(paramsContainer.Tenant.Id, not);
            }
            //end of Send Notification
        }

        protected void get_fans(Guid? likedId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<User> users = !likedId.HasValue ? new List<User>() :
                UsersController.get_users(paramsContainer.Tenant.Id,
                GlobalController.get_fan_ids(paramsContainer.Tenant.Id, likedId.Value));

            responseText = "{\"Users\":[" + ProviderUtil.list_to_string<string>(users.Select(
                u => "{\"UserID\":\"" + u.UserID.ToString() + "\"" +
                ",\"UserName\":\"" + Base64.encode(u.UserName) + "\"" +
                ",\"FirstName\":\"" + Base64.encode(u.FirstName) + "\"" +
                ",\"LastName\":\"" + Base64.encode(u.LastName) + "\"" +
                ",\"ProfileImageURL\":\"" +
                    DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, u.UserID.Value) + "\"" +
                "}").ToList()) + "]}";
        }

        protected void follow_unfollow(Guid? followedId, bool? follow, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            bool result = followedId.HasValue && (!follow.HasValue || !follow.Value ?
                GlobalController.unfollow(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, followedId.Value) :
                GlobalController.follow(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, followedId.Value));

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
        }

        protected void get_help_index(ref string responseText)
        {
            //Privacy Check: OK

            List<HelpIndexEntry> index = RaaiVanHelp.help_index("fa");
            if (index == null) index = new List<HelpIndexEntry>();

            responseText = "{\"Index\":[" + string.Join(",", index.Select(u => u.toJson(false, true))) + "]}";
        }

        protected void get_help_index_entry(string name, ref string responseText)
        {
            //Privacy Check: OK

            HelpIndexEntry entry = RaaiVanHelp.index_entry("fa", name);

            responseText = "{\"Entry\":" + (entry == null ? "null" : entry.toJson(true, false)) + "}";
        }

        protected void get_help_media_files(ref string responseText)
        {
            string path = PublicMethods.map_path("~/Help/fa/media");

            if (!System.IO.Directory.Exists(path)) return;

            string[] images = System.IO.Directory.GetFiles(path);

            responseText = "{\"Files\":[" + ProviderUtil.list_to_string<string>(images.Select(
                u => "\"" + u.Substring(u.LastIndexOf("\\") + 1) + "\"").ToList()) + "]}";
        }

        protected void save_help_index_entry(string lang, string path, string content, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit ||
                !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value)) return;
            
            try
            {
                string filePath = PublicMethods.map_path("~/Help/" + lang + "/" + path);
                string folderPath = filePath.Substring(0, filePath.LastIndexOf("\\"));
                string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                fileName = fileName.Substring(0, fileName.LastIndexOf("."));

                if (!System.IO.Directory.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
                    file.Write(content);

                responseText = "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}";

                HelpIndexEntry entry = RaaiVanHelp.index_entry(lang, fileName);
                if (entry != null) entry.Content = content;
            }
            catch { responseText = "{\"Succeed\":\"" + Messages.OperationFailed + "\"}"; }
        }

        protected void save_system_setting_values(Dictionary<string, object> settings, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            Dictionary<RVSettingsItem, string> items = new Dictionary<RVSettingsItem, string>();
            foreach (string k in settings.Keys)
            {
                RVSettingsItem itm;
                if (Enum.TryParse<RVSettingsItem>(k, out itm)) items[itm] = Base64.decode(settings[k].ToString());
            }

            bool rslt = items.Keys.Count > 0 && paramsContainer.CurrentUserID.HasValue &&
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) &&
                RaaiVanSettings.set_value(paramsContainer.Tenant.Id, items, paramsContainer.CurrentUserID.Value);

            responseText = rslt ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
        }

        protected void get_system_setting_values(List<RVSettingsItem> names, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            Dictionary<RVSettingsItem, string> dic = !paramsContainer.CurrentUserID.HasValue ||
                !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) ?
                new Dictionary<RVSettingsItem, string>() : RaaiVanSettings.get_value(paramsContainer.Tenant.Id, names);

            Dictionary<string, object> retDic = new Dictionary<string, object>();

            foreach (RVSettingsItem n in dic.Keys)
                retDic[n.ToString()] = Base64.encode(dic[n]);

            responseText = PublicMethods.toJSON(retDic);
        }

        protected void get_all_notifications(ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;


            //Notifications
            int notifications = !Modules.RaaiVanConfig.Modules.Notifications(paramsContainer.Tenant.Id) ? 0 :
                NotificationController.get_user_notifications_count(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            //end of Notifications


            //Dashboards
            List<DashboardCount> dashboardsCount = NotificationController.get_dashboards_count(paramsContainer.Tenant.Id,
                paramsContainer.CurrentUserID.Value, null, null, null, DashboardType.NotSet);

            string dashboards = "{\"ToBeDone\":" + dashboardsCount.Sum(u => u.ToBeDone).ToString() +
                ",\"NotSeen\":" + dashboardsCount.Sum(u => u.NotSeen).ToString() +
                ",\"Done\":" + dashboardsCount.Sum(u => u.Done).ToString() +
                ",\"DoneAndInWorkFlow\":" + dashboardsCount.Sum(u => u.DoneAndInWorkFlow).ToString() +
                ",\"DoneAndNotInWorkFlow\":" + dashboardsCount.Sum(u => u.DoneAndNotInWorkFlow).ToString() +
                ",\"Items\":[" + string.Join(",", dashboardsCount.Select(u => u.toJson())) + "]" +
                "}";
            //end of Dashboards


            //Friend Requests
            int friendRequests = !Modules.RaaiVanConfig.Modules.SocialNetwork(paramsContainer.Tenant.Id) ? 0 :
                UsersController.get_friends_count(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, false, false, true);
            //end of Friend Requests


            //Messages
            int messages = !Modules.RaaiVanConfig.Modules.Messaging(paramsContainer.Tenant.Id) ? 0 :
                MSGController.get_not_seen_messages_count(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            //end of Messages


            responseText = "{\"Notifications\":" + notifications.ToString() +
                ",\"Dashboards\":" + dashboards +
                ",\"FriendRequests\":" + friendRequests.ToString() +
                ",\"Messages\":" + messages.ToString() +
                "}";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}