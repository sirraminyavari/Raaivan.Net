using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.Users;

namespace RaaiVan.Modules.Search
{
    public static class SearchController
    {
        public static List<User> search_users(Guid applicationId, string searchText, ref List<Guid> departmentIds, 
            ref List<Guid> expertiseKDIds, ref List<Guid> projectIds, ref List<Guid> processIds,
            ref List<Guid> communityIds, ref List<Guid> knowledgeKds, int? count = 20, Guid? minId = null)
        {
            List<User> retList = new List<User>();
            DataProvider.SearchUsers(applicationId, ref retList, searchText, ref departmentIds, ref expertiseKDIds, 
                ref projectIds, ref processIds, ref communityIds, ref knowledgeKds, count, minId);
            return retList;
        }

        public static List<SearchDoc> get_index_queue_items(Guid applicationId, int count, SearchDocType type)
        {
            List<SearchDoc> retList = new List<SearchDoc>();
            DataProvider.GetIndexQueueItems(applicationId, ref retList, count, type.ToString());
            return retList;
        }

        public static bool set_index_last_update_date(Guid applicationId, SearchDocType itemType, List<Guid> IDs)
        {
            return DataProvider.SetIndexLastUpdateDate(applicationId, itemType, IDs);
        }
    }
}
