using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaaiVan.Modules.DataExchange
{
    public static class DEController
    {
        public static bool update_nodes(Guid applicationId, List<ExchangeNode> nodes, 
            Guid? nodeTypeId, string nodeTypeAdditionalId, Guid currentUserId, ref List<Guid> newNodeIds)
        {
            return DataProvider.UpdateNodes(applicationId, nodes, 
                nodeTypeId, nodeTypeAdditionalId, currentUserId, ref newNodeIds);
        }

        public static bool update_node_ids(Guid applicationId,
            Guid currentUserId, Guid nodeTypeId, List<KeyValuePair<string, string>> items)
        {
            return DataProvider.UpdateNodeIDs(applicationId, currentUserId, nodeTypeId, items);
        }

        public static bool remove_nodes(Guid applicationId,
            Guid currentUserId, List<KeyValuePair<string, string>> items)
        {
            return DataProvider.RemoveNodes(applicationId, currentUserId, items);
        }

        public static bool update_users(Guid applicationId, List<ExchangeUser> users)
        {
            return DataProvider.UpdateUsers(applicationId, users);
        }

        public static bool update_members(Guid applicationId, List<ExchangeMember> members)
        {
            return DataProvider.UpdateMembers(applicationId, members);
        }

        public static bool update_experts(Guid applicationId, List<ExchangeMember> experts)
        {
            return DataProvider.UpdateExperts(applicationId, experts);
        }

        public static bool update_relations(Guid applicationId, Guid currentUserId, List<ExchangeRelation> relations)
        {
            return DataProvider.UpdateRelations(applicationId, currentUserId, relations);
        }

        public static bool update_authors(Guid applicationId, Guid currentUserId, List<ExchangeAuthor> authors)
        {
            return DataProvider.UpdateAuthors(applicationId, currentUserId, authors);
        }

        public static bool update_user_confidentialities(Guid applicationId, Guid currentUserId, List<KeyValuePair<string, int>> items)
        {
            return DataProvider.UpdateUserConfidentialities(applicationId, currentUserId, items);
        }

        public static bool update_permissions(Guid applicationId, Guid currentUserId, List<ExchangePermission> permissions)
        {
            return DataProvider.UpdatePermissions(applicationId, currentUserId, permissions);
        }
    }
}
