using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.NotificationCenter
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[NTFN_" + name + "]"; //'[dbo].' is database owner and 'NTFN_' is module qualifier
        }

        private static void _parse_notifications(ref IDataReader reader, ref List<Notification> lstNotifications)
        {
            while (reader.Read())
            {
                try
                {
                    Notification not = new Notification();

                    if (!string.IsNullOrEmpty(reader["NotificationID"].ToString())) not.NotificationID = (long)reader["NotificationID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) not.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["SubjectID"].ToString())) not.SubjectID = (Guid)reader["SubjectID"];
                    if (!string.IsNullOrEmpty(reader["RefItemID"].ToString())) not.RefItemID = (Guid)reader["RefItemID"];
                    if (!string.IsNullOrEmpty(reader["SubjectName"].ToString())) not.SubjectName = (string)reader["SubjectName"];
                    if (!string.IsNullOrEmpty(reader["SubjectType"].ToString()))
                    {
                        try { not.SubjectType = (SubjectType)Enum.Parse(typeof(SubjectType), (string)reader["SubjectType"]); }
                        catch { not.SubjectType = SubjectType.None; }
                    }
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) not.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) not.Sender.UserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) not.Sender.FirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) not.Sender.LastName = (string)reader["SenderLastName"];
                    if (!string.IsNullOrEmpty(reader["Action"].ToString()))
                    {
                        try { not.Action = (ActionType)Enum.Parse(typeof(ActionType), (string)reader["Action"]); }
                        catch { not.Action = ActionType.None; }
                    }
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) not.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["Info"].ToString())) not.Info = (string)reader["Info"];
                    if (!string.IsNullOrEmpty(reader["UserStatus"].ToString()))
                    {
                        try { not.UserStatus = (UserStatus)Enum.Parse(typeof(UserStatus), (string)reader["UserStatus"]); }
                        catch { not.UserStatus = UserStatus.None; }
                    }
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) not.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["Seen"].ToString())) not.Seen = (bool)reader["Seen"];
                    if (!string.IsNullOrEmpty(reader["ViewDate"].ToString())) not.ViewDate = (DateTime)reader["ViewDate"];

                    lstNotifications.Add(not);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_message_templates(ref IDataReader reader, ref List<MessageTemplate> lstTemplates)
        {
            while (reader.Read())
            {
                try
                {
                    MessageTemplate tmp = new MessageTemplate();

                    if (!string.IsNullOrEmpty(reader["TemplateID"].ToString())) tmp.TemplateID = (Guid)reader["TemplateID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) tmp.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["BodyText"].ToString())) tmp.BodyText = (string)reader["BodyText"];
                    if (!string.IsNullOrEmpty(reader["AudienceType"].ToString()))
                    {
                        try { tmp.AudienceType = (AudienceType)Enum.Parse(typeof(AudienceType), (string)reader["AudienceType"]); }
                        catch { tmp.AudienceType = AudienceType.NotSet; }
                    }
                    if (!string.IsNullOrEmpty(reader["AudienceRefOwnerID"].ToString())) 
                        tmp.AudienceRefOwnerID = (Guid)reader["AudienceRefOwnerID"];
                    if (!string.IsNullOrEmpty(reader["AudienceNodeID"].ToString())) tmp.AudienceNodeID = (Guid)reader["AudienceNodeID"];
                    if (!string.IsNullOrEmpty(reader["AudienceNodeName"].ToString())) 
                        tmp.AudienceNodeName = (string)reader["AudienceNodeName"];
                    if (!string.IsNullOrEmpty(reader["AudienceNodeTypeID"].ToString()))
                        tmp.AudienceNodeTypeID = (Guid)reader["AudienceNodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["AudienceNodeType"].ToString()))
                        tmp.AudienceNodeType = (string)reader["AudienceNodeType"];
                    if (!string.IsNullOrEmpty(reader["AudienceNodeAdmin"].ToString()))
                        tmp.AudienceNodeAdmin = (bool)reader["AudienceNodeAdmin"];

                    lstTemplates.Add(tmp);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_notification_messages_info(Guid? applicationId, 
            ref IDataReader reader, ref List<NotificationMessage> retMessagesInfo)
        {
            while (reader.Read())
            {
                try
                {
                    NotificationMessage message = new NotificationMessage();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) message.ReceiverUserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Lang"].ToString())) message.Lang = (string)reader["Lang"];
                    if (!string.IsNullOrEmpty(reader["Subject"].ToString())) message.Subject = (string)reader["Subject"];
                    if (!string.IsNullOrEmpty(reader["Text"].ToString())) 
                        message.Text = EmailTemplates.inject_into_master(applicationId, (string)reader["Text"]);

                    string mediaStr = string.IsNullOrEmpty(reader["Media"].ToString()) ? string.Empty : (string)reader["Media"];
                    Media m = Media.None;
                    message.Media = Enum.TryParse<Media>(mediaStr, true, out m) ? m : Media.None;

                    retMessagesInfo.Add(message);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_notification_message_template(ref IDataReader reader, ref List<NotificationMessageTemplate> retMessagetemplates)
        {
            while (reader.Read())
            {
                try
                {
                    NotificationMessageTemplate messageTemplate = new NotificationMessageTemplate();

                    if (!string.IsNullOrEmpty(reader["TemplateID"].ToString())) messageTemplate.TemplateId = (Guid)reader["TemplateID"];
                    if (!string.IsNullOrEmpty(reader["Enable"].ToString())) messageTemplate.Enable = (bool)reader["Enable"];
                    if (!string.IsNullOrEmpty(reader["IsDefault"].ToString())) messageTemplate.IsDefault = (bool)reader["IsDefault"];
                    if (!string.IsNullOrEmpty(reader["Lang"].ToString())) messageTemplate.Lang = (string)reader["Lang"];
                    if (!string.IsNullOrEmpty(reader["Subject"].ToString())) messageTemplate.Subject = (string)reader["Subject"];
                    if (!string.IsNullOrEmpty(reader["Text"].ToString())) messageTemplate.Text = (string)reader["Text"];

                    string subjectTypeStr = string.IsNullOrEmpty(reader["SubjectType"].ToString()) ? string.Empty : (string)reader["SubjectType"];
                    try { messageTemplate.SubjectType = (SubjectType)Enum.Parse(typeof(SubjectType), subjectTypeStr); }
                    catch { messageTemplate.SubjectType = SubjectType.None; }

                    string actionStr = string.IsNullOrEmpty(reader["Action"].ToString()) ? string.Empty : (string)reader["Action"];
                    try { messageTemplate.Action = (ActionType)Enum.Parse(typeof(ActionType), actionStr); }
                    catch { messageTemplate.Action = ActionType.None; }

                    string mediaStr = string.IsNullOrEmpty(reader["Media"].ToString()) ? string.Empty : (string)reader["Media"];
                    Media m = Media.None;
                    messageTemplate.Media = Enum.TryParse<Media>(mediaStr, true, out m) ? m : Media.None;

                    string userStatusStr = string.IsNullOrEmpty(reader["UserStatus"].ToString()) ? string.Empty : (string)reader["UserStatus"];
                    try { messageTemplate.UserStatus = (UserStatus)Enum.Parse(typeof(UserStatus), userStatusStr); }
                    catch { messageTemplate.UserStatus = UserStatus.None; }

                    retMessagetemplates.Add(messageTemplate);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_messaging_activation_option(ref IDataReader reader, ref List<MessagingActivationOption> lstSendOption)
        {
            while (reader.Read())
            {
                try
                {
                    MessagingActivationOption options = new MessagingActivationOption();

                    if (!string.IsNullOrEmpty(reader["OptionID"].ToString())) options.OptionID = (Guid)reader["OptionID"];
                    if (!string.IsNullOrEmpty(reader["Lang"].ToString())) options.Lang = (string)reader["Lang"];
                    if (!string.IsNullOrEmpty(reader["Enable"].ToString())) options.Enable = (bool)reader["Enable"];
                    if (!string.IsNullOrEmpty(reader["AdminEnable"].ToString())) options.AdminEnable = (bool)reader["AdminEnable"];

                    string subjectTypeStr = string.IsNullOrEmpty(reader["SubjectType"].ToString()) ? string.Empty : (string)reader["SubjectType"];
                    try { options.SubjectType = (SubjectType)Enum.Parse(typeof(SubjectType), subjectTypeStr); }
                    catch { options.SubjectType = SubjectType.None; }

                    string userStatusStr = string.IsNullOrEmpty(reader["UserStatus"].ToString()) ? string.Empty : (string)reader["UserStatus"];
                    try { options.UserStatus = (UserStatus)Enum.Parse(typeof(UserStatus), userStatusStr); }
                    catch { options.UserStatus = UserStatus.None; }

                    string actionStr = string.IsNullOrEmpty(reader["Action"].ToString()) ? string.Empty : (string)reader["Action"];
                    try { options.Action = (ActionType)Enum.Parse(typeof(ActionType), actionStr); }
                    catch { options.Action = ActionType.None; }

                    string mediaStr = string.IsNullOrEmpty(reader["Media"].ToString()) ? string.Empty : (string)reader["Media"];
                    Media m = Media.None;
                    options.Media = Enum.TryParse<Media>(mediaStr, true, out m) ? m : Media.None;

                    lstSendOption.Add(options);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static bool SendNotification(Guid applicationId, ref List<Pair> users, Notification info)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable usersTable = new DataTable();
            usersTable.Columns.Add("FirstValue", typeof(Guid));
            usersTable.Columns.Add("SecondValue", typeof(string));

            List<Guid> userIds = new List<Guid>();
            foreach (Pair _usr in users)
            {
                if (!userIds.Exists(u => u == (Guid)_usr.First))
                {
                    userIds.Add((Guid)_usr.First);
                    usersTable.Rows.Add((Guid)_usr.First, _usr.Second.ToString());
                }
            }

            SqlParameter usersParam = new SqlParameter("@Users", SqlDbType.Structured);
            usersParam.TypeName = "[dbo].[GuidStringTableType]";
            usersParam.Value = usersTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(usersParam);
            cmd.Parameters.AddWithValue("@SubjectID", info.SubjectID);
            cmd.Parameters.AddWithValue("@RefItemID", info.RefItemID);
            cmd.Parameters.AddWithValue("@SubjectType", info.SubjectType.ToString());
            if(!string.IsNullOrEmpty(info.SubjectName)) cmd.Parameters.AddWithValue("@SubjectName", info.SubjectName);
            cmd.Parameters.AddWithValue("@Action", info.Action.ToString());
            cmd.Parameters.AddWithValue("@SenderUserID", info.Sender.UserID);
            cmd.Parameters.AddWithValue("@SendDate", info.SendDate.HasValue ? info.SendDate : DateTime.Now);
            if(!string.IsNullOrEmpty(info.Description)) cmd.Parameters.AddWithValue("@Description", info.Description);
            if(!string.IsNullOrEmpty(info.Info)) cmd.Parameters.AddWithValue("@Info", info.Info);

            string spName = GetFullyQualifiedName("SendNotification");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Users" + sep + "@SubjectID" + sep + "@RefItemID" + sep + 
                "@SubjectType" + sep + (string.IsNullOrEmpty(info.SubjectName) ? "null" : "@SubjectName") + sep + 
                "@Action" + sep + "@SenderUserID" + sep + "@SendDate" + sep +
                (string.IsNullOrEmpty(info.Description) ? "null" : "@Description") + sep +
                (string.IsNullOrEmpty(info.Info) ? "null" : "@Info");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool SetNotificationsAsSeen(Guid applicationId, Guid userId, ref List<long> notificationIds)
        {
            string spName = GetFullyQualifiedName("SetNotificationsAsSeen");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, ProviderUtil.list_to_string<long>(ref notificationIds), ',', DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool SetUserNotificationsAsSeen(Guid applicationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("SetUserNotificationsAsSeen");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool ArithmeticDeleteNotification(Guid applicationId, long notificationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteNotification");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, notificationId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool ArithmeticDeleteNotifications(Guid applicationId, Notification info, List<string> actions)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteNotifications");

            try
            {
                if (!string.IsNullOrEmpty(info.Action.ToString())) actions.Add(info.Action.ToString());
                actions = actions.Distinct().ToList();

                if (info.SubjectID.HasValue) info.SubjectIDs.Add(info.SubjectID.Value);
                if (info.RefItemID.HasValue) info.RefItemIDs.Add(info.RefItemID.Value);

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(info.SubjectIDs), ProviderUtil.list_to_string<Guid>(info.RefItemIDs),
                    info.Sender.UserID, ProviderUtil.list_to_string<string>(ref actions), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static int GetUserNotificationsCount(Guid applicationId, Guid userId, bool? seen)
        {
            string spName = GetFullyQualifiedName("GetUserNotificationsCount");

            try
            {
                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, userId, seen));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return 0;
            }
        }

        public static void GetUserNotifications(Guid applicationId, ref List<Notification> retNotifications, 
            Guid userId, bool? seen, long? lastNotSeenId, long? lastSeenId, 
            DateTime? lastViewDate, DateTime? lowerDateLimit, DateTime? upperDateLimit, int? count)
        {
            string spName = GetFullyQualifiedName("GetUserNotifications");

            try
            {
                if (lastSeenId == 0) lastSeenId = null;
                if (lastNotSeenId == 0) lastNotSeenId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, seen, lastNotSeenId, lastSeenId, lastViewDate, lowerDateLimit, upperDateLimit, count);
                _parse_notifications(ref reader, ref retNotifications);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
            }
        }

        public static bool SetDashboardsAsSeen(Guid applicationId, Guid userId, List<long> dashboardIds)
        {
            string spName = GetFullyQualifiedName("SetDashboardsAsSeen");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, ProviderUtil.list_to_string<long>(dashboardIds), ',', DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool ArithmeticDeleteDashboards(Guid applicationId, Guid userId, List<long> dashboardIds)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteDashboards");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, ProviderUtil.list_to_string<long>(dashboardIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static void GetDashboardsCount(Guid applicationId, ref List<DashboardCount> counts, Guid userId, 
            Guid? nodeTypeId, Guid? nodeId, string nodeAdditionalId, DashboardType type)
        {
            string spName = GetFullyQualifiedName("GetDashboardsCount");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;

                string strType = type == DashboardType.NotSet ? null : type.ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    userId, nodeTypeId, nodeId, nodeAdditionalId, strType);
                ProviderUtil.parse_dashboards_count(ref reader, ref counts);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
            }
        }

        public static List<Guid> GetDashboards(Guid applicationId, ref List<Dashboard> retDashboards, Guid? userId, 
            Guid? nodeTypeId, Guid? nodeId, string nodeAdditionalId, DashboardType dashboardType, DashboardSubType subType,
            string subTypeTitle, bool? done, DateTime? dateFrom, DateTime? dateTo, string searchText, bool? getDistinctItems, 
            bool? inWorkFlowState, int? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetDashboards");

            List<Guid> retList = new List<Guid>();

            try
            {
                if (userId == Guid.Empty) userId = null;
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (nodeId == Guid.Empty) nodeId = null;
                if (lowerBoundary == 0) lowerBoundary = null;
                if (!count.HasValue || count <= 0) count = 50;

                if (!string.IsNullOrEmpty(nodeAdditionalId)) nodeAdditionalId = nodeAdditionalId.Trim();
                if (string.IsNullOrEmpty(nodeAdditionalId)) nodeAdditionalId = null;

                string strDashboardType = dashboardType == DashboardType.NotSet ? null : dashboardType.ToString();
                string strSubType = subType == DashboardSubType.NotSet ?
                    (string.IsNullOrEmpty(subTypeTitle) ? null : subTypeTitle) : subType.ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, nodeTypeId, nodeId, nodeAdditionalId, strDashboardType, strSubType, done,
                    dateFrom, dateTo, ProviderUtil.get_search_text(searchText), getDistinctItems, inWorkFlowState, lowerBoundary, count);

                if (!getDistinctItems.HasValue || !getDistinctItems.Value)
                    ProviderUtil.parse_dashboards(ref reader, ref retDashboards, ref totalCount);
                else ProviderUtil.parse_guids(ref reader, ref retList, ref totalCount);

                return retList;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return new List<Guid>();
            }
        }

        public static bool DashboardExists(Guid applicationId, Guid? userId, Guid? nodeId, DashboardType? type, 
            DashboardSubType? subType, bool? seen, bool? done, DateTime? lowerDataLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("DashboardExists");

            try
            {
                if (userId == Guid.Empty) userId = null;
                if (nodeId == Guid.Empty) nodeId = null;

                string strType = null;
                if (type.HasValue && type.Value != DashboardType.NotSet) strType = type.Value.ToString();

                string strSubType = null;
                if (subType.HasValue && subType.Value != DashboardSubType.NotSet) strSubType = subType.Value.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, nodeId, strType, strSubType, seen, done, lowerDataLimit, upperDateLimit));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool SetMessageTemplate(Guid applicationId, MessageTemplate info)
        {
            string spName = GetFullyQualifiedName("SetMessageTemplate");

            try
            {
                if (info.AudienceType == AudienceType.NotSet) return false;
                if (info.AudienceRefOwnerID == Guid.Empty) info.AudienceRefOwnerID = null;
                if (info.AudienceNodeID == Guid.Empty) info.AudienceNodeID = null;
                if (!info.CreationDate.HasValue) info.CreationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    info.TemplateID, info.OwnerID, info.BodyText, info.AudienceType.ToString(), info.AudienceRefOwnerID,
                    info.AudienceNodeID, info.AudienceNodeAdmin, info.CreatorUserID, info.CreationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool ArithmeticDeleteMessageTemplate(Guid applicationId, Guid templateId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteMessageTemplate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    templateId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static void GetOwnerMessageTemplates(Guid applicationId, 
            ref List<MessageTemplate> retTemplates, ref List<Guid> ownerIds)
        {
            string spName = GetFullyQualifiedName("GetOwnerMessageTemplates");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref ownerIds), ',');
                _parse_message_templates(ref reader, ref retTemplates);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
            }
        }

        //Notification Messages

        public static void GetNotificationMessagesInfo(Guid applicationId, List<Pair> userStatusPairList, 
            SubjectType subjectType, ActionType action, ref List<NotificationMessage> retMessageList)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            DataTable userStatusPairTable = new DataTable();
            userStatusPairTable.Columns.Add("FirstValue", typeof(Guid));
            userStatusPairTable.Columns.Add("SecondValue", typeof(string));
            foreach (Pair _pair in userStatusPairList)
                userStatusPairTable.Rows.Add((Guid)_pair.First, ((UserStatus)_pair.Second).ToString());

            SqlParameter userStatusPairParam = new SqlParameter("@UserStatusPair", SqlDbType.Structured);
            userStatusPairParam.TypeName = "[dbo].[GuidStringTableType]";
            userStatusPairParam.Value = userStatusPairTable;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(userStatusPairParam);
            cmd.Parameters.AddWithValue("@SubjectType", subjectType.ToString());
            cmd.Parameters.AddWithValue("@Action", action.ToString());

            string spName = GetFullyQualifiedName("GetNotificationMessagesInfo");

            string sep = ",";
            string arguments = "@ApplicationID" + sep + "@UserStatusPair" + sep + "@SubjectType" + sep + "@Action";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                _parse_notification_messages_info(applicationId, ref reader, ref retMessageList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
            }
            finally { con.Close(); }
        }

        public static bool SetAdminMessagingActivation(Guid applicationId, Guid templateId, Guid currentUserId, 
            SubjectType subjectType, ActionType action, Media media, UserStatus userStatus, string lang, bool enable)
        {
            string spName = GetFullyQualifiedName("SetAdminMessagingActivation");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    templateId, currentUserId, DateTime.Now, subjectType, action, media, userStatus, lang, enable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool IsDefaultMessageTemplate(Guid applicationId, Guid templateId, bool? isDefault)
        {
            string spName = GetFullyQualifiedName("IsDefaultMessageTemplate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, templateId, isDefault));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static bool SetNotificationMessageTemplateText(Guid applicationId, 
            Guid templateId, Guid currentUserId, string subject, string text)
        {
            string spName = GetFullyQualifiedName("SetNotificationMessageTemplateText");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    templateId, currentUserId, DateTime.Now, subject, text));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static void GetNotificationMessageTemplatesInfo(Guid applicationId, 
            ref List<NotificationMessageTemplate> retMessagetemplates)
        {
            string spName = GetFullyQualifiedName("GetNotificationMessageTemplatesInfo");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                _parse_notification_message_template(ref reader, ref retMessagetemplates);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
            }
        }

        public static bool SetUserMessagingActivation(Guid applicationId, Guid optionId, Guid userId, Guid currentUserId, 
            SubjectType subjectType, UserStatus userStatus, ActionType action, Media media, string lang, bool enable)
        {
            string spName = GetFullyQualifiedName("SetUserMessagingActivation");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    optionId, userId, currentUserId, DateTime.Now, subjectType, userStatus, action, media, lang, enable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
                return false;
            }
        }

        public static void GetUserMessagingActivation(Guid applicationId, 
            Guid userId, ref List<MessagingActivationOption> retMessagingActivationOpt)
        {
            string spName = GetFullyQualifiedName("GetUserMessagingActivation");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_messaging_activation_option(ref reader, ref retMessagingActivationOpt);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.NTFN);
            }
        }

        //end of Notification Messages
    }
}
