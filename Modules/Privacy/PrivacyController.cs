using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Privacy
{
    public static class PrivacyController
    {
        public static bool initialize_confidentiality_levels(Guid applicationId)
        {
            return DataProvider.InitializeConfidentialityLevels(applicationId);
        }

        public static bool refine_access_roles(Guid applicationId)
        {
            return DataProvider.RefineAccessRoles(applicationId);
        }

        public static bool set_audience(Guid applicationId, List<Privacy> items, Guid currentUserId)
        {
            return DataProvider.SetAudience(applicationId, items, currentUserId);
        }

        public static Dictionary<Guid, List<PermissionType>> check_access(Guid applicationId,
            Guid? userId, List<Guid> objectIds, PrivacyObjectType objectType, List<PermissionType> permissions)
        {
            Dictionary<Guid, List<PermissionType>> ret = new Dictionary<Guid, List<PermissionType>>();

            PublicMethods.split_list<Guid>(objectIds, 200, ids =>
            {
                DataProvider.CheckAccess(applicationId, userId, ids, objectType, permissions)
                    .ToList().ForEach(x => ret.Add(x.Key, x.Value));
            });

            return ret;
        }

        public static Dictionary<Guid, List<PermissionType>> check_access(Guid applicationId,
            Guid? userId, List<Guid> objectIds, PrivacyObjectType objectType)
        {
            return check_access(applicationId, userId, objectIds, objectType, new List<PermissionType>());
        }

        public static List<Guid> check_access(Guid applicationId,
            Guid? userId, List<Guid> objectIds, PrivacyObjectType objectType, PermissionType permission)
        {
            return check_access(applicationId, userId, objectIds, objectType, new List<PermissionType>() { permission })
                .Keys.ToList();
        }

        public static List<PermissionType> check_access(Guid applicationId,
            Guid? userId, Guid objectId, PrivacyObjectType objectType, List<PermissionType> permissions)
        {
            Dictionary<Guid, List<PermissionType>> dic =
                check_access(applicationId, userId, new List<Guid>() { objectId }, objectType, permissions);
            return dic.ContainsKey(objectId) ? dic[objectId] : new List<PermissionType>();
        }

        public static List<PermissionType> check_access(Guid applicationId,
            Guid? userId, Guid objectId, PrivacyObjectType objectType)
        {
            Dictionary<Guid, List<PermissionType>> dic =
                check_access(applicationId, userId, new List<Guid>() { objectId }, objectType);
            return dic.ContainsKey(objectId) ? dic[objectId] : new List<PermissionType>();
        }

        public static bool check_access(Guid applicationId, Guid? userId, Guid objectId, 
            PrivacyObjectType objectType, PermissionType permission)
        {
            List<PermissionType> lst =
                check_access(applicationId, userId, objectId, objectType, new List<PermissionType>() { permission });
            return lst != null && lst.Count > 0;
        }

        public static Dictionary<Guid, List<Audience>> get_audience(Guid applicationId, List<Guid> objectIds)
        {
            return DataProvider.GetAudience(applicationId, objectIds);
        }

        public static List<Audience> get_audience(Guid applicationId, Guid objectId)
        {
            Dictionary<Guid, List<Audience>> dic = DataProvider.GetAudience(applicationId, new List<Guid>() { objectId });
            return dic.ContainsKey(objectId) ? dic[objectId] : new List<Audience>();
        }

        public static Dictionary<Guid, List<DefaultPermission>> get_default_permissions(Guid applicationId, List<Guid> objectIds)
        {
            return DataProvider.GetDefaultPermissions(applicationId, objectIds);
        }

        public static List<DefaultPermission> get_default_permissions(Guid applicationId, Guid objectId)
        {
            Dictionary<Guid, List<DefaultPermission>> dic =
                DataProvider.GetDefaultPermissions(applicationId, new List<Guid>() { objectId });
            return dic.ContainsKey(objectId) ? dic[objectId] : new List<DefaultPermission>();
        }

        public static List<Privacy> get_settings(Guid applicationId, List<Guid> objectIds)
        {
            return DataProvider.GetSettings(applicationId, objectIds);
        }

        public static Privacy get_settings(Guid applicationId, Guid objectId)
        {
            return DataProvider.GetSettings(applicationId, new List<Guid>() { objectId }).FirstOrDefault();
        }

        public static bool add_confidentiality_level(Guid applicationId, 
            Guid id, int levelId, string title, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.AddConfidentialityLevel(applicationId, 
                id, levelId, title, currentUserId, ref errorMessage);
        }

        public static bool modify_confidentiality_level(Guid applicationId, 
            Guid id, int newLevelId, string newTitle, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.ModifyConfidentialityLevel(applicationId,
                id, newLevelId, newTitle, currentUserId, ref errorMessage);
        }

        public static bool remove_confidentiality_level(Guid applicationId, Guid id, Guid currentUserId)
        {
            return DataProvider.RemoveConfidentialityLevel(applicationId, id, currentUserId);
        }

        public static List<ConfidentialityLevel> get_confidentiality_levels(Guid applicationId)
        {
            List<ConfidentialityLevel> retList = new List<ConfidentialityLevel>();
            DataProvider.GetConfidentialityLevels(applicationId, ref retList);
            return retList;
        }

        public static bool set_confidentiality_level(Guid applicationId, Guid itemId, Guid levelId, Guid currentUserId)
        {
            return DataProvider.SetConfidentialityLevel(applicationId, itemId, levelId, currentUserId);
        }

        public static bool unset_confidentiality_level(Guid applicationId, Guid itemId, Guid currentUserId)
        {
            return DataProvider.UnsetConfidentialityLevel(applicationId, itemId, currentUserId);
        }

        public static List<Guid> get_confidentiality_level_user_ids(Guid applicationId, 
            Guid confidentialityId, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetConfidentialityLevelUserIDs(applicationId,
                ref retList, confidentialityId, searchText, count, lowerBoundary, ref totalCount);
            return retList;
        }

        public static List<Guid> get_confidentiality_user_ids(Guid applicationId, 
            Guid confidentialityId, string searchText = null, int? count = null, long? lowerBoundary = null)
        {
            long totalCount = 0;
            return get_confidentiality_level_user_ids(applicationId,
                confidentialityId, searchText, count, lowerBoundary, ref totalCount);
        }
    }
}
