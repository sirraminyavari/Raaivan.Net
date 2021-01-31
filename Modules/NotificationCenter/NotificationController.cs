using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Threading;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.QA;
using RaaiVan.Modules.Sharing;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;

namespace RaaiVan.Modules.NotificationCenter
{
    public class NotificationController
    {
        private static List<Guid> __get_audience_user_ids(ref Notification notification, UserStatus status)
        {
            if (notification.Audience.ContainsKey(status)) return notification.Audience[status];
            else return null;
        }

        private static void _send_notification(Guid applicationId, Notification info)
        {
            if (!RaaiVanConfig.Modules.Notifications(applicationId)) return;

            if (!info.Action.HasValue || info.Action.Value == ActionType.None ||
                !info.SubjectType.HasValue || info.SubjectType.Value == SubjectType.None) return;
            
            List<Pair> users = new List<Pair>();
            if (info.UserID.HasValue && info.UserID != info.Sender.UserID) users.Add(new Pair(info.UserID.Value, UserStatus.Owner));

            List<Guid> userIds = new List<Guid>();

            List<Guid> mentionedUserIds = info.Action == ActionType.Post || info.Action == ActionType.Share || info.Action == ActionType.Comment ?
                Expressions.get_tagged_items(info.Description, "User").Where(u => u.ID.HasValue && u.ID != info.UserID)
                .Select(u => u.ID.Value).ToList() : new List<Guid>();

            info.Description = PublicMethods.markup2plaintext(applicationId,
                Expressions.replace(info.Description, Expressions.Patterns.HTMLTag, " "));

            switch (info.Action.Value)
            {
                case ActionType.Like:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Node:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                                userIds = CNController.get_node_creators(applicationId, info.RefItemID.Value).Select(
                                    u => u.User.UserID.Value).ToList();
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Member)) == null)
                                userIds = CNController.get_member_user_ids(applicationId, 
                                    info.RefItemID.Value, NodeMemberStatuses.Accepted);
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Member));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Expert)) == null)
                                userIds = CNController.get_experts(applicationId, info.RefItemID.Value).Select(
                                    u => u.User.UserID.Value).ToList();
                            foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Expert));

                            Node node = CNController.get_node(applicationId, info.RefItemID.Value, true);
                            if (node != null)
                            {
                                info.SubjectName = node.Name;
                                info.Description = node.Description;
                                info.Info = "{\"NodeType\":\"" + Base64.encode(node.NodeType) + "\"}";
                            }
                            break;
                        case SubjectType.Question:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>() { };
                                Guid? id = QAController.get_question_asker_id(applicationId, info.RefItemID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            Question question = QAController.get_question(applicationId, info.RefItemID.Value, null);
                            if (question != null)
                            {
                                info.SubjectName = question.Title;
                                info.Description = question.Description;
                            }
                            break;
                        case SubjectType.Post:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = SharingController.get_post_sender_id(applicationId, info.RefItemID.Value);
                                if(id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            Post post = SharingController.get_post(applicationId, info.RefItemID.Value, null);
                            info.Description = string.IsNullOrEmpty(post.Description) ? 
                                post.OriginalDescription : post.Description;
                            break;
                        case SubjectType.Comment:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = SharingController.get_comment_sender_id(applicationId, info.SubjectID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            Sharing.Comment comment = SharingController.get_comment(applicationId, info.SubjectID.Value, null);
                            info.RefItemID = comment.PostID;
                            info.Description = comment.Description;
                            break;
                    }
                    break;
                case ActionType.Dislike:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Post:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = SharingController.get_post_sender_id(applicationId, info.RefItemID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            Post post = SharingController.get_post(applicationId, info.RefItemID.Value, null);
                            info.Description = string.IsNullOrEmpty(post.Description) ? post.OriginalDescription : post.Description;
                            break;
                        case SubjectType.Comment:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = SharingController.get_comment_sender_id(applicationId, info.SubjectID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            Sharing.Comment comment = SharingController.get_comment(applicationId, info.SubjectID.Value, null);
                            info.RefItemID = comment.PostID;
                            info.Description = comment.Description;
                            break;
                    }
                    break;
                case ActionType.Question:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Question:
                            if(info.ReceiverUserIDs != null && info.ReceiverUserIDs.Count > 0)
                            {
                                userIds = info.ReceiverUserIDs;
                                foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Contributor));
                            }
                            break;
                    }
                    break;
                case ActionType.Answer:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Answer:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = QAController.get_question_asker_id(applicationId, info.RefItemID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Fan)) == null)
                                userIds = GlobalController.get_fan_ids(applicationId, info.RefItemID.Value).ToList();
                            foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Fan));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Contributor)) == null)
                                userIds = QAController.get_answer_sender_ids(applicationId, info.RefItemID.Value).ToList();
                            foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Contributor));

                            info.SubjectName = QAController.get_question(applicationId, info.RefItemID.Value, null).Title;
                            break;
                    }
                    break;
                case ActionType.Post:
                case ActionType.Share:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Post:
                            foreach (Guid _usr in mentionedUserIds) users.Add(new Pair(_usr, UserStatus.Mentioned));

                            Node node = null;

                            bool isNode = info.RefItemID.HasValue && 
                                CNController.is_node(applicationId, info.RefItemID.Value);

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Director)) != null)
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Owner));

                            if (isNode)
                            {
                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                                    userIds = CNController.get_node_creators(applicationId, info.RefItemID.Value).Select(
                                        u => u.User.UserID.Value).ToList();
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Owner));

                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Member)) == null)
                                    userIds = CNController.get_members(applicationId, info.RefItemID.Value,
                                        pending: false, admin: null).Select(u => u.Member.UserID.Value).ToList();
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Member));

                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Fan)) == null)
                                    userIds = CNController.get_node_fans_user_ids(applicationId, info.RefItemID.Value).ToList();
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Fan));

                                node = CNController.get_node(applicationId, info.RefItemID.Value);
                                if (node != null) info.SubjectName = node.Name;
                            }

                            if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                            {
                                User user = UsersController.get_user(applicationId, info.Sender.UserID.Value);
                                info.ReplacementDic["SenderProfileImageURL"] =
                                    DocumentUtilities.get_personal_image_address(applicationId, user.UserID.Value, true);
                                info.ReplacementDic["SenderFullName"] = user.FirstName + " " + user.LastName;
                                info.ReplacementDic["SenderPageURL"] = 
                                    PublicConsts.get_complete_url(applicationId, PublicConsts.HomePage) +
                                    "/" + user.UserID.Value.ToString();
                                info.ReplacementDic["PostURL"] = 
                                    PublicConsts.get_complete_url(applicationId, PublicConsts.PostPage) +
                                    "/" + info.SubjectID.Value.ToString();
                                info.ReplacementDic["Description"] = info.Description;

                                if (isNode)
                                {
                                    info.ReplacementDic["NodePageURL"] =
                                        PublicConsts.get_complete_url(applicationId, PublicConsts.NodePage) +
                                        "/" + info.RefItemID.Value.ToString();
                                    if (node != null) info.ReplacementDic["NodeName"] = node.Name;
                                }
                            }

                            break;
                    }
                    break;
                case ActionType.Comment:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Comment:
                            foreach (Guid _usr in mentionedUserIds) users.Add(new Pair(_usr, UserStatus.Mentioned));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = SharingController.get_post_sender_id(applicationId, info.RefItemID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Fan)) == null)
                                userIds = SharingController.get_post_fan_ids(applicationId, info.RefItemID.Value).ToList();
                            foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Fan));

                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Contributor)) == null)
                                userIds = SharingController.get_comment_sender_ids(applicationId, info.RefItemID.Value).ToList();
                            foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Contributor));

                            if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                            {
                                User user = UsersController.get_user(applicationId, info.Sender.UserID.Value);
                                info.ReplacementDic["SenderProfileImageURL"] =
                                    DocumentUtilities.get_personal_image_address(applicationId, user.UserID.Value, true);
                                info.ReplacementDic["SenderFullName"] = user.FirstName + " " + user.LastName;
                                info.ReplacementDic["SenderPageURL"] = 
                                    PublicConsts.get_complete_url(applicationId, PublicConsts.ProfilePage) + 
                                    "/" + user.UserID.Value.ToString();
                                info.ReplacementDic["PostURL"] = 
                                    PublicConsts.get_complete_url(applicationId, PublicConsts.PostPage) +
                                    "/" + info.RefItemID.Value.ToString();
                                info.ReplacementDic["Description"] = info.Description;
                            }

                            break;
                        case SubjectType.Question:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = QAController.get_question_asker_id(applicationId, info.RefItemID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));
                            
                            break;
                        case SubjectType.Answer:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? commentId = QAController.get_comment_owner_id(applicationId, info.SubjectID.Value);
                                if(commentId.HasValue) userIds.Add(commentId.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            break;
                    }
                    break;
                case ActionType.Modify:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.Wiki:
                            Node node = CNController.get_node(applicationId, info.RefItemID.Value, false);

                            if (node != null && node.NodeID.HasValue)
                            {
                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                                    userIds = CNController.get_node_creators(applicationId, info.RefItemID.Value).Select(
                                        u => u.User.UserID.Value).ToList();
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Owner));

                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Fan)) == null) userIds = 
                                        CNController.get_node_fans_user_ids(applicationId, info.RefItemID.Value).ToList();
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Fan));

                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Expert)) == null)
                                    userIds = CNController.get_experts(applicationId, info.RefItemID.Value).Select(
                                        u => u.User.UserID.Value).ToList();
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Expert));

                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Member)) == null)
                                    userIds = CNController.get_member_user_ids(applicationId, info.RefItemID.Value);
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Member));

                                if (!users.Exists(u => (UserStatus)u.Second == UserStatus.Owner))
                                    users.Add(new Pair(node.Creator.UserID.Value, UserStatus.Owner));

                                info.SubjectName = node.Name;
                                info.Info = "{\"NodeType\":\"" + Base64.encode(node.NodeType) + "\"}";

                                if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                                {
                                    User user = UsersController.get_user(applicationId, info.Sender.UserID.Value);
                                    info.ReplacementDic["SenderProfileImageURL"] =
                                        DocumentUtilities.get_personal_image_address(applicationId, user.UserID.Value, true);
                                    info.ReplacementDic["SenderFullName"] = user.FirstName + " " + user.LastName;
                                    info.ReplacementDic["SenderPageURL"] = 
                                        PublicConsts.get_complete_url(applicationId, PublicConsts.ProfilePage) +
                                        "/" + user.UserID.Value.ToString();
                                    info.ReplacementDic["NodeName"] = node.Name;
                                    info.ReplacementDic["NodeType"] = node.NodeType;
                                    info.ReplacementDic["NodePageURL"] = 
                                        PublicConsts.get_complete_url(applicationId, PublicConsts.NodePage) +
                                        "/" + info.RefItemID.Value.ToString();
                                    info.ReplacementDic["Description"] = info.Description;
                                }
                            }
                            break;
                    }
                    break;
                case ActionType.FriendRequest:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.User:
                            users.Clear();
                            users.Add(new Pair(info.UserID, UserStatus.Mentioned));

                            info.UserStatus = UserStatus.Mentioned;

                            if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                            {
                                User user = UsersController.get_user(applicationId, info.RefItemID.Value);
                                info.ReplacementDic["SenderProfileImageURL"] =
                                    DocumentUtilities.get_personal_image_address(applicationId, user.UserID.Value, true);
                                info.ReplacementDic["SenderFullName"] = user.FirstName + " " + user.LastName;
                                info.ReplacementDic["SenderPageURL"] = 
                                    PublicConsts.get_complete_url(applicationId, PublicConsts.ProfilePage) +
                                    "/" + user.UserID.Value.ToString();
                            }

                            break;
                    }
                    break;
                case ActionType.AcceptFriendRequest:
                    switch (info.SubjectType.Value)
                    {
                        case SubjectType.User:
                            users.Clear();
                            users.Add(new Pair(info.UserID, UserStatus.Mentioned));

                            info.UserStatus = UserStatus.Mentioned;

                            if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                            {
                                User user = UsersController.get_user(applicationId, info.RefItemID.Value);
                                info.ReplacementDic["SenderProfileImageURL"] =
                                    DocumentUtilities.get_personal_image_address(applicationId, user.UserID.Value, true);
                                info.ReplacementDic["SenderFullName"] = user.FirstName + " " + user.LastName;
                                info.ReplacementDic["SenderPageURL"] = 
                                    PublicConsts.get_complete_url(applicationId, PublicConsts.ProfilePage) +
                                    "/" + user.UserID.Value.ToString();
                            }
                            
                            break;
                    }
                    break;
                case ActionType.Accept:
                    switch (info.SubjectType)
                    {
                        case SubjectType.Node:
                            {
                                if ((userIds = __get_audience_user_ids(ref info, UserStatus.Member)) == null && 
                                    info.RefItemID.HasValue)
                                {
                                    List<Guid> nIds = CNController.get_related_node_ids(applicationId, info.RefItemID.Value,
                                        null, null, false, true);

                                    List<Guid> creatorIds = 
                                        CNController.get_node_creators(applicationId, info.RefItemID.Value)
                                        .Select(u => u.User.UserID.Value).ToList();

                                    userIds = CNController.get_members(applicationId, nIds,
                                        pending: false, admin: null).Select(u => u.Member.UserID.Value)
                                        .Distinct().Where(x => !creatorIds.Any(a => a == x)).ToList();
                                }
                                foreach (Guid _usr in userIds) users.Add(new Pair(_usr, UserStatus.Member));
                            }
                            break;
                    }
                    break;
                case ActionType.Publish:
                    switch (info.SubjectType)
                    {
                        case SubjectType.Question:
                            if ((userIds = __get_audience_user_ids(ref info, UserStatus.Owner)) == null)
                            {
                                userIds = new List<Guid>();
                                Guid? id = QAController.get_question_asker_id(applicationId, info.RefItemID.Value);
                                if (id.HasValue) userIds.Add(id.Value);
                            }
                            foreach (Guid _uid in userIds) users.Add(new Pair(_uid, UserStatus.Owner));

                            break;
                    }
                    break;
            }

            users = users.Except(users.Where(u => info.Sender.UserID.HasValue && (Guid)u.First == info.Sender.UserID)).ToList();

            DataProvider.SendNotification(applicationId, ref users, info);

            if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                NotificationController._send_notification_message(applicationId, users, info);
        }

        public static void send_notification(Guid applicationId, Notification info)
        {
            PublicMethods.set_timeout(() => _send_notification(applicationId, info), 0);
        }

        private static void _transfer_dashboards(object obj)
        {
            Guid applicationId = (Guid)((Pair)obj).First;
            List<Dashboard> dashboards = (List<Dashboard>)((Pair)obj).Second;
        }

        public static void transfer_dashboards(Guid applicationId, List<Dashboard> dashboards)
        {
            if (RaaiVanConfig.Modules.SMSEMailNotifier(applicationId))
                ThreadPool.QueueUserWorkItem(new WaitCallback(_transfer_dashboards), new Pair(applicationId, dashboards));
        }

        public static bool set_notifications_as_seen(Guid applicationId, Guid userId, ref List<long> notificationIds)
        {
            return DataProvider.SetNotificationsAsSeen(applicationId, userId, ref notificationIds);
        }

        public static bool set_notifications_as_seen(Guid applicationId, Guid userId, List<long> notificationIds)
        {
            return set_notifications_as_seen(applicationId, userId, ref notificationIds);
        }

        public static bool set_notification_as_seen(Guid applicationId, Guid userId, long notificationId)
        {
            List<long> _nIds = new List<long>() { notificationId };
            return set_notifications_as_seen(applicationId, userId, ref _nIds);
        }

        public static bool set_user_notifications_as_seen(Guid applicationId, Guid userId)
        {
            return DataProvider.SetUserNotificationsAsSeen(applicationId, userId);
        }

        public static bool remove_notification(Guid applicationId, long notificationId, Guid userId)
        {
            return DataProvider.ArithmeticDeleteNotification(applicationId, notificationId, userId);
        }

        public static void remove_notifications(Guid applicationId, Notification info, List<string> actions)
        {
            if (!RaaiVanConfig.Modules.Notifications(applicationId)) return;

            PublicMethods.set_timeout(() => DataProvider.ArithmeticDeleteNotifications(applicationId, info, actions), 0);
        }

        public static void remove_notifications(Guid applicationId, Notification info)
        {
            remove_notifications(applicationId, info, new List<string>());
        }

        public static int get_user_notifications_count(Guid applicationId, Guid userId, bool? seen = false)
        {
            return DataProvider.GetUserNotificationsCount(applicationId, userId, seen);
        }

        public static List<Notification> get_user_notifications(Guid applicationId, Guid userId, bool? seen = null, 
            long? lastNotSeenId = null, long? lastSeenId = null,DateTime? lastViewDate = null, 
            DateTime? lowerDateLimit = null, DateTime? upperDateLimit = null, int? count = null)
        {
            List<Notification> nots = new List<Notification>();
            DataProvider.GetUserNotifications(applicationId, ref nots, userId, seen, lastNotSeenId, lastSeenId, 
                lastViewDate, lowerDateLimit, upperDateLimit, count);
            return nots;
        }

        public static bool set_dashboards_as_seen(Guid applicationId, Guid userId, List<long> dashboardIds)
        {
            return DataProvider.SetDashboardsAsSeen(applicationId, userId, dashboardIds);
        }

        public static bool remove_dashboards(Guid applicationId, Guid userId, List<long> dashboardIds)
        {
            return DataProvider.ArithmeticDeleteDashboards(applicationId, userId, dashboardIds);
        }

        public static List<DashboardCount> get_dashboards_count(Guid applicationId, Guid userId, 
            Guid? nodeTypeId, Guid? nodeId, string nodeAdditionalId, DashboardType type)
        {
            List<DashboardCount> retList = new List<DashboardCount>();
            DataProvider.GetDashboardsCount(applicationId, ref retList, userId, nodeTypeId, nodeId, nodeAdditionalId, type);
            return retList;
        }

        public static List<Dashboard> get_dashboards(Guid applicationId, Guid? userId, Guid? nodeTypeId, Guid? nodeId, 
            string nodeAdditionalId, DashboardType dashboardType, DashboardSubType subType, string subTypeTitle, bool? done, 
            DateTime? dateFrom, DateTime? dateTo, string searchText, int? lowerBoundary, int? count, ref long totalCount)
        {
            List<Dashboard> retList = new List<Dashboard>();
            DataProvider.GetDashboards(applicationId, ref retList, userId, nodeTypeId, nodeId, nodeAdditionalId, dashboardType, 
                subType, subTypeTitle, done, dateFrom, dateTo, searchText, false, null, lowerBoundary, count, ref totalCount);
            return retList;
        }

        public static List<Dashboard> get_dashboards(Guid applicationId, Guid? userId, Guid? nodeTypeId, Guid? nodeId,
            string nodeAdditionalId, DashboardType dashboardType, DashboardSubType subType, string subTypeTitle, bool? done,
            DateTime? dateFrom, DateTime? dateTo, string searchText, int? lowerBoundary, int? count)
        {
            long totalCount = 0;
            return get_dashboards(applicationId, userId, nodeTypeId, nodeId, nodeAdditionalId, dashboardType, subType,
                subTypeTitle, done, dateFrom, dateTo, searchText, lowerBoundary, count, ref totalCount);
        }

        public static List<Guid> get_dashboards(Guid applicationId, Guid? userId, Guid? nodeTypeId, Guid? nodeId, 
            DashboardType dashboardType, DashboardSubType subType, string subTypeTitle, string searchText, bool? inWorkFlowState, 
            int? lowerBoundary, int? count, ref long totalCount)
        {
            List<Dashboard> retList = new List<Dashboard>();
            return DataProvider.GetDashboards(applicationId, ref retList, userId, nodeTypeId, nodeId, null,
                dashboardType, subType, subTypeTitle, null, null, null, searchText, true, inWorkFlowState, lowerBoundary, count, ref totalCount);
        }

        public static bool dashboard_exists(Guid applicationId, Guid? userId = null, Guid? nodeId = null, 
            DashboardType? type = null, DashboardSubType? subType = null, bool? seen = null, bool? done = null,
            DateTime? lowerDataLimit = null, DateTime? upperDateLimit = null)
        {
            return DataProvider.DashboardExists(applicationId,
                userId, nodeId, type, subType, seen, done, lowerDataLimit, upperDateLimit);
        }

        public static bool set_message_template(Guid applicationId, MessageTemplate info)
        {
            return DataProvider.SetMessageTemplate(applicationId, info);
        }

        public static bool remove_message_template(Guid applicationId, Guid templateId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteMessageTemplate(applicationId, templateId, currentUserId);
        }

        public static List<MessageTemplate> get_owner_message_templates(Guid applicationId, List<Guid> ownerIds)
        {
            List<MessageTemplate> retList = new List<MessageTemplate>();
            DataProvider.GetOwnerMessageTemplates(applicationId, ref retList, ref ownerIds);
            return retList;
        }

        public static List<MessageTemplate> get_owner_message_templates(Guid applicationId, Guid ownerId)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(ownerId);
            return get_owner_message_templates(applicationId, lst);
        }

        //Notification Messages

        private static void _send_notification_message(Guid applicationId, List<Pair> users, Notification not)
        {
            if (!not.SubjectType.HasValue || !not.Action.HasValue) return;

            EmailTemplates.Initialize(applicationId);

            List<NotificationMessage> messageLst = NotificationController._get_notification_messages_info(
                applicationId, users, not.SubjectType.Value, not.Action.Value);
            List<EmailAddress> emailList = UsersController.get_users_main_email(messageLst.Select(
                p => (Guid)p.ReceiverUserID).Distinct().ToList<Guid>());
            List<PhoneNumber> phoneList = UsersController.get_users_main_phone(messageLst.Select(
                p => (Guid)p.ReceiverUserID).Distinct().ToList<Guid>());
            List<NotificationMessage> sentMessages = new List<NotificationMessage>();

            SortedSet<Guid> emailSentTo = new SortedSet<Guid>();
            SortedSet<Guid> smsSentTo = new SortedSet<Guid>();

            foreach (NotificationMessage m in messageLst)
            {
                m.Subject = Expressions.replace(m.Subject, ref not.ReplacementDic, Expressions.Patterns.AutoTag);
                m.Text = Expressions.replace(m.Text, ref not.ReplacementDic, Expressions.Patterns.AutoTag);

                if (m.Media == Media.Email && 
                    !emailSentTo.Any(u => u == m.ReceiverUserID) && emailList.Any(e => e.UserID == m.ReceiverUserID))
                {
                    m.Action = not.Action.Value;
                    m.RefItemID = not.RefItemID.Value;
                    m.SubjectType = not.SubjectType.Value;
                    m.UserStatus = 
                        users.Where(u => (Guid)u.First == m.ReceiverUserID).Select(u => (UserStatus)u.Second).First();
                    m.EmailAddress = emailList.Where(e => e.UserID == m.ReceiverUserID).First();

                    ThreadPool.QueueUserWorkItem(new WaitCallback(m.send_email), applicationId);

                    emailSentTo.Add(m.ReceiverUserID.Value);
                }

                if (m.Media == Media.SMS && 
                    !smsSentTo.Any(u => u == m.ReceiverUserID) && phoneList.Any(p => p.UserID == m.ReceiverUserID))
                {
                    m.Action = not.Action.Value;
                    m.RefItemID = not.RefItemID.Value;
                    m.SubjectType = not.SubjectType.Value;
                    m.UserStatus = 
                        users.Where(u => (Guid)u.First == m.ReceiverUserID).Select(u => (UserStatus)u.Second).First();
                    m.PhoneNumber = phoneList.Where(p => p.UserID == m.ReceiverUserID).First();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(m.send_sms));

                    smsSentTo.Add(m.ReceiverUserID.Value);
                }
            }
        }

        private static List<NotificationMessage> _get_notification_messages_info(Guid applicationId, 
            List<Pair> userStatusPairList, SubjectType subjectType, ActionType action)
        {
            List<NotificationMessage> retMessageList = new List<NotificationMessage>();
            DataProvider.GetNotificationMessagesInfo(applicationId, 
                userStatusPairList, subjectType, action, ref retMessageList);
            return retMessageList;
        }

        public static bool set_admin_messaging_activation(Guid applicationId, Guid templateId, Guid currentUserId, 
            SubjectType subjectType, ActionType action, Media media, UserStatus userStatus, string lang, bool enable)
        {
            return DataProvider.SetAdminMessagingActivation(applicationId,
                templateId, currentUserId, subjectType, action, media, userStatus, lang, enable);
        }

        public static bool set_notification_message_template_text(Guid applicationId, 
            Guid templateId, Guid currentUserId, string subject, string text)
        {
            return DataProvider.SetNotificationMessageTemplateText(applicationId, 
                templateId, currentUserId, subject, text);
        }

        public static bool set_user_messaging_activation(Guid applicationId, Guid optionId, 
            Guid userId, Guid currentUserId, SubjectType subjectType, UserStatus userStatus, 
            ActionType action, Media media, string lang, bool enable)
        {
            return DataProvider.SetUserMessagingActivation(applicationId,
                optionId, userId, currentUserId, subjectType, userStatus, action, media, lang, enable);
        }

        public static List<NotificationMessageTemplate> get_notification_message_templates_info(Guid applicationId)
        {
            List<NotificationMessageTemplate> retMessagetemplates = new List<NotificationMessageTemplate>();
            DataProvider.GetNotificationMessageTemplatesInfo(applicationId, ref retMessagetemplates);
            return retMessagetemplates;
        }

        public static List<MessagingActivationOption> get_user_messaging_activation(Guid applicationId, Guid userId)
        {
            List<MessagingActivationOption> retSendOption = new List<MessagingActivationOption>();
            DataProvider.GetUserMessagingActivation(applicationId, userId, ref retSendOption);
            return retSendOption;
        }

        //end of Notification Messages
    }
}
