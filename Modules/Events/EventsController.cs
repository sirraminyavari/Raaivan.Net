using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.Knowledge;
using RaaiVan.Modules.CoreNetwork;

namespace RaaiVan.Modules.Events
{
    public static class EventsController
    {
        public static bool create_event(Guid applicationId, 
            Event Info, List<Guid> userIds, List<Guid> groupIds, List<Guid> nodeIds)
        {
            return DataProvider.CreateEvent(applicationId, Info, userIds, groupIds, nodeIds);
        }

        public static bool remove_event(Guid applicationId, Guid eventId)
        {
            return DataProvider.ArithmeticDeleteEvent(applicationId, eventId);
        }

        public static List<Event> get_events(Guid applicationId, ref List<Guid> eventIds, bool? full = null)
        {
            List<Event> retList = new List<Event>();
            DataProvider.GetEvents(applicationId, ref retList, ref eventIds, full);
            return retList;
        }

        public static Event get_event(Guid applicationId, Guid eventId, bool? full = null)
        {
            if (eventId == Guid.Empty) return null;
            List<Guid> _eIds = new List<Guid>();
            _eIds.Add(eventId);
            return get_events(applicationId, ref _eIds, full).FirstOrDefault();
        }

        public static int get_user_finished_events_count(Guid applicationId, Guid userId, bool? done = null)
        {
            return DataProvider.GetUserFinishedEventsCount(applicationId, userId, done);
        }

        public static List<Event> get_user_finished_events(Guid applicationId, Guid userId, bool? done = null)
        {
            List<Event> retList = new List<Event>();
            DataProvider.GetUserFinishedEvents(applicationId, ref retList, userId, done);
            return retList;
        }

        public static List<Guid> get_related_user_ids(Guid applicationId, Guid eventId)
        {
            List<Guid> retIDs = new List<Guid>();
            DataProvider.GetRelatedUserIDs(applicationId, ref retIDs, eventId);
            return retIDs;
        }

        public static List<RelatedUser> get_related_users(Guid applicationId, Guid eventId)
        {
            List<RelatedUser> retUsers = new List<RelatedUser>();
            DataProvider.GetRelatedUsers(applicationId, ref retUsers, eventId);
            return retUsers;
        }

        public static bool remove_related_user(Guid applicationId, Guid eventId, Guid userId, ref bool calenderDeleted)
        {
            return DataProvider.ArithmeticDeleteRelatedUser(applicationId, eventId, userId, ref calenderDeleted);
        }

        public static bool change_user_status(Guid applicationId, Guid eventId, Guid userId, string status)
        {
            return DataProvider.ChangeUserStatus(applicationId, eventId, userId, status);
        }

        public static List<Node> get_related_nodes(Guid applicationId, Guid eventId, NodeTypes? nodeType = null)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetRelatedNodes(applicationId, ref retList, eventId, nodeType);
            return retList;
        }

        public static List<Event> get_node_related_events(Guid applicationId, 
            Guid nodeId, DateTime? beginDate = null, bool? notFinished = null)
        {
            List<Event> retList = new List<Event>();
            DataProvider.GetNodeRelatedEvents(applicationId, ref retList, nodeId, beginDate, notFinished);
            return retList;
        }

        public static List<RelatedUser> get_user_related_events(Guid applicationId, Guid userId, 
            DateTime? beginDate = null, bool? notFinished = null, UserStatus? status = null, Guid? nodeId = null)
        {
            List<RelatedUser> retList = new List<RelatedUser>();
            DataProvider.GetUserRelatedEvents(applicationId, ref retList, userId, beginDate, notFinished, status, nodeId);
            return retList;
        }
    }
}
