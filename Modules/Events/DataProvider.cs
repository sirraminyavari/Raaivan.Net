using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Events
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name) => "[dbo]." + "[EVT_" + name + "]"; //'[dbo].' is database owner and 'EVT_' is module qualifier

        private static void _parse_events(ref IDataReader reader, ref List<Event> lstEvents, bool? full)
        {
            while (reader.Read())
            {
                try
                {
                    Event _event = new Event();

                    if (!string.IsNullOrEmpty(reader["EventID"].ToString())) _event.EventID = (Guid)reader["EventID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) _event.Title = (string)reader["Title"];

                    if (full.HasValue && full.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["EventType"].ToString())) _event.EventType = (string)reader["EventType"];
                        if (!string.IsNullOrEmpty(reader["Description"].ToString())) _event.Description = (string)reader["Description"];
                        if (!string.IsNullOrEmpty(reader["BeginDate"].ToString())) _event.BeginDate = (DateTime)reader["BeginDate"];
                        if (!string.IsNullOrEmpty(reader["FinishDate"].ToString())) _event.FinishDate = (DateTime)reader["FinishDate"];
                        if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) _event.CreatorUserID = (Guid)reader["CreatorUserID"];
                    }

                    lstEvents.Add(_event);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_related_users(ref IDataReader reader, ref List<RelatedUser> lstUsers, 
            bool? userInfo = null, bool? eventInfo = null)
        {
            while (reader.Read())
            {
                try
                {
                    RelatedUser _relatedUser = new RelatedUser();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) _relatedUser.UserInfo.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["EventID"].ToString())) _relatedUser.EventInfo.EventID = (Guid)reader["EventID"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) _relatedUser.Status = (string)reader["Status"];
                    if (!string.IsNullOrEmpty(reader["Done"].ToString())) _relatedUser.Done = (bool)reader["Done"];
                    if (!string.IsNullOrEmpty(reader["RealFinishDate"].ToString())) _relatedUser.RealFinishDate = (DateTime)reader["RealFinishDate"];

                    if (userInfo.HasValue && userInfo.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["UserName"].ToString())) _relatedUser.UserInfo.UserName = (string)reader["UserName"];
                        if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) _relatedUser.UserInfo.FirstName = (string)reader["FirstName"];
                        if (!string.IsNullOrEmpty(reader["LastName"].ToString())) _relatedUser.UserInfo.LastName = (string)reader["LastName"];
                    }

                    if (eventInfo.HasValue && eventInfo.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["EventType"].ToString())) _relatedUser.EventInfo.EventType = (string)reader["EventType"];
                        if (!string.IsNullOrEmpty(reader["Title"].ToString())) _relatedUser.EventInfo.Title = (string)reader["Title"];
                        if (!string.IsNullOrEmpty(reader["Description"].ToString())) _relatedUser.EventInfo.Description = (string)reader["Description"];
                        if (!string.IsNullOrEmpty(reader["BeginDate"].ToString())) _relatedUser.EventInfo.BeginDate = (DateTime)reader["BeginDate"];
                        if (!string.IsNullOrEmpty(reader["FinishDate"].ToString())) _relatedUser.EventInfo.FinishDate = (DateTime)reader["FinishDate"];
                        if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) _relatedUser.EventInfo.CreatorUserID = (Guid)reader["CreatorUserID"];
                    }

                    lstUsers.Add(_relatedUser);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static bool CreateEvent(Guid applicationId, 
            Event Info, List<Guid> userIds, List<Guid> groupIds, List<Guid> nodeIds)
        {
            string spName = GetFullyQualifiedName("CreateEvent");

            try
            {
                List<Guid> relatedNodeIds = new List<Guid>();

                relatedNodeIds.AddRange(groupIds);
                relatedNodeIds.AddRange(nodeIds);

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.EventID, 
                    Info.EventType, Info.OwnerID, Info.Title, Info.Description, Info.BeginDate, Info.FinishDate,
                    Info.CreatorUserID, Info.CreationDate, ProviderUtil.list_to_string<Guid>(ref relatedNodeIds),
                    ProviderUtil.list_to_string<Guid>(ref userIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
                return false;
            }
        }

        public static bool ArithmeticDeleteEvent(Guid applicationId, Guid eventId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteEvent");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, eventId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
                return false;
            }
        }

        public static void GetEvents(Guid applicationId, 
            ref List<Event> retEvents, ref List<Guid> eventIds, bool? full)
        {
            string spName = GetFullyQualifiedName("GetEventsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref eventIds), ',', full);
                _parse_events(ref reader, ref retEvents, full);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }

        public static int GetUserFinishedEventsCount(Guid applicationId, Guid userId, bool? done)
        {
            string spName = GetFullyQualifiedName("GetUserFinishedEventsCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId,
                    userId, DateTime.Now, done));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
                return 0;
            }
        }

        public static void GetUserFinishedEvents(Guid applicationId, 
            ref List<Event> retEvents, Guid userId, bool? done)
        {
            string spName = GetFullyQualifiedName("GetUserFinishedEvents");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, DateTime.Now, done);
                _parse_events(ref reader, ref retEvents, false);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }

        public static void GetRelatedUserIDs(Guid applicationId, ref List<Guid> retIDs, Guid eventId)
        {
            string spName = GetFullyQualifiedName("GetRelatedUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, eventId);
                ProviderUtil.parse_guids(ref reader, ref retIDs);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }

        public static void GetRelatedUsers(Guid applicationId, ref List<RelatedUser> retUsers, Guid eventId)
        {
            string spName = GetFullyQualifiedName("GetRelatedUsers");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, eventId);
                _parse_related_users(ref reader, ref retUsers, true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }

        public static bool ArithmeticDeleteRelatedUser(Guid applicationId, 
            Guid eventId, Guid userId, ref bool calenderDeleted)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteRelatedUser");

            try
            {
                int result = (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    eventId, userId));

                calenderDeleted = result == 2 ? true : false;
                return result > 0 ? true : false;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
                return false;
            }
        }

        public static bool ChangeUserStatus(Guid applicationId, Guid eventId, Guid userId, string status)
        {
            string spName = GetFullyQualifiedName("ChangeUserStatus");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, eventId, userId, status));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
                return false;
            }
        }

        public static void GetRelatedNodes(Guid applicationId, 
            ref List<Node> retNodes, Guid eventId, NodeTypes? nodeType)
        {
            string spName = GetFullyQualifiedName("GetRelatedNodeIDs");

            try
            {
                string strNodeTypeId = null;
                if (nodeType.HasValue) strNodeTypeId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

                List<Guid> nodeIds = new List<Guid>();
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, eventId, strNodeTypeId);
                ProviderUtil.parse_guids(ref reader, ref nodeIds);

                retNodes = CNController.get_nodes(applicationId, nodeIds, full: null, currentUserId: null);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }

        public static void GetNodeRelatedEvents(Guid applicationId, 
            ref List<Event> retEvents, Guid nodeId, DateTime? beginDate, bool? notFinished)
        {
            string spName = GetFullyQualifiedName("GetNodeRelatedEvents");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, beginDate, notFinished);
                _parse_events(ref reader, ref retEvents, false);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }

        public static void GetUserRelatedEvents(Guid applicationId, ref List<RelatedUser> retEvents, Guid userId, 
            DateTime? currentDate, bool? notFinished, UserStatus? status, Guid? nodeId)
        {
            string spName = GetFullyQualifiedName("GetUserRelatedEvents");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, currentDate, notFinished, status, nodeId);
                _parse_related_users(ref reader, ref retEvents, false, true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.EVT);
            }
        }
    }
}
