﻿
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.FormGenerator;
using System.Collections;

namespace RaaiVan.Modules.CoreNetwork
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[CN_" + name + "]"; //'[dbo].' is database owner and 'CN_' is module qualifier
        }

        private static long _parse_node_types(ref IDataReader reader, ref List<NodeType> lstNodeTypes)
        {
            while (reader.Read())
            {
                try
                {
                    NodeType nodeType = new NodeType();

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodeType.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["ParentID"].ToString())) nodeType.ParentID = (Guid)reader["ParentID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) nodeType.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString()))
                    {
                        nodeType.NodeTypeAdditionalID = (string)reader["AdditionalID"];

                        try { nodeType.AdditionalID = int.Parse((string)reader["AdditionalID"]); }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(reader["AdditionalIDPattern"].ToString()))
                    {
                        nodeType.AdditionalIDPattern = CNUtilities.DefaultAdditionalIDPattern;
                        nodeType.HasDefaultPattern = true;
                    }
                    else
                    {
                        nodeType.AdditionalIDPattern = (string)reader["AdditionalIDPattern"];
                        nodeType.HasDefaultPattern = false;
                    }
                    if (!string.IsNullOrEmpty(reader["Archive"].ToString())) nodeType.Archive = (bool)reader["Archive"];
                    if (!string.IsNullOrEmpty(reader["IsService"].ToString())) nodeType.IsService = (bool)reader["IsService"];

                    lstNodeTypes.Add(nodeType);
                }
                catch { }
            }

            long totalCount = (reader.NextResult()) ? ProviderUtil.succeed_long(reader) : 0;

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static void _parse_relation_types(ref IDataReader reader, ref List<RelationType> lstRelationTypes)
        {
            while (reader.Read())
            {
                try
                {
                    RelationType relationType = new RelationType();

                    if (!string.IsNullOrEmpty(reader["RelationTypeID"].ToString())) relationType.RelationTypeID = (Guid)reader["RelationTypeID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) relationType.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString())) relationType.AdditionalID = int.Parse((string)reader["AdditionalID"]);

                    lstRelationTypes.Add(relationType);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_nodes(ref IDataReader reader, ref List<Node> lstNodes, 
            ref List<NodesCount> lstCounts, bool? full, bool hasTotalCount = false, bool fetchCounts = false)
        {
            while (reader.Read())
            {
                try
                {
                    Node node = new Node();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeAdditionalID"].ToString()))
                        node.TypeAdditionalID = (string)reader["NodeTypeAdditionalID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeAdditionalID"].ToString()))
                    {
                        try { node.NodeTypeAdditionalID = (int.Parse((string)reader["NodeTypeAdditionalID"])); }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) node.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID_Main"].ToString()))
                        node.AdditionalID_Main = (string)reader["AdditionalID_Main"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString()))
                        node.AdditionalID = (string)reader["AdditionalID"];
                    if (!string.IsNullOrEmpty(reader["ParentNodeID"].ToString())) node.ParentNodeID = (Guid)reader["ParentNodeID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) node.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString())) node.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["AdminAreaID"].ToString())) node.AdminAreaID = (Guid)reader["AdminAreaID"];
                    if (!string.IsNullOrEmpty(reader["DocumentTreeNodeID"].ToString())) node.DocumentTreeNodeID = (Guid)reader["DocumentTreeNodeID"];

                    Status st = Status.NotSet;
                    if (Enum.TryParse<Status>(reader["Status"].ToString(), out st)) node.Status = st;

                    if (!string.IsNullOrEmpty(reader["WFState"].ToString())) node.WFState = (string)reader["WFState"];
                    if (!string.IsNullOrEmpty(reader["Searchable"].ToString()))
                        node.Searchable = (bool)reader["Searchable"];
                    if (!string.IsNullOrEmpty(reader["HideCreators"].ToString())) node.HideCreators = (bool)reader["HideCreators"];
                    if (!string.IsNullOrEmpty(reader["Archive"].ToString())) node.Archive = (bool)reader["Archive"];

                    if (full.HasValue && full.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString()))
                            node.Creator.UserName = (string)reader["CreatorUserName"];
                        if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString()))
                            node.Creator.FirstName = (string)reader["CreatorFirstName"];
                        if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString()))
                            node.Creator.LastName = (string)reader["CreatorLastName"];
                        if (!string.IsNullOrEmpty(reader["DocumentTreeID"].ToString()))
                            node.DocumentTreeID = (Guid)reader["DocumentTreeID"];
                        if (!string.IsNullOrEmpty(reader["DocumentTreeName"].ToString()))
                            node.DocumentTreeName = (string)reader["DocumentTreeName"];
                        if (!string.IsNullOrEmpty(reader["PreviousVersionID"].ToString()))
                            node.PreviousVersionID = (Guid)reader["PreviousVersionID"];
                        if (!string.IsNullOrEmpty(reader["PreviousVersionName"].ToString()))
                            node.PreviousVersionName = (string)reader["PreviousVersionName"];
                        if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                            node.Description = (string)reader["Description"];
                        if (!string.IsNullOrEmpty(reader["PublicDescription"].ToString()))
                            node.PublicDescription = (string)reader["PublicDescription"];
                        if (!string.IsNullOrEmpty(reader["Tags"].ToString()))
                        {
                            string strTags = strTags = (string)reader["Tags"];
                            node.Tags = ProviderUtil.get_tags_list(strTags);
                        }
                        if (!string.IsNullOrEmpty(reader["LikesCount"].ToString()))
                            node.LikesCount = (int)reader["LikesCount"];
                        if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString()))
                            node.LikeStatus = (bool)reader["LikeStatus"];
                        if (!string.IsNullOrEmpty(reader["MembershipStatus"].ToString()))
                            node.MembershipStatus = (string)reader["MembershipStatus"];
                        if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString()))
                            node.VisitsCount = (int)reader["VisitsCount"];
                        if (!string.IsNullOrEmpty(reader["AdminAreaName"].ToString()))
                            node.AdminAreaName = (string)reader["AdminAreaName"];
                        if (!string.IsNullOrEmpty(reader["AdminAreaType"].ToString()))
                            node.AdminAreaType = (string)reader["AdminAreaType"];
                        if (!string.IsNullOrEmpty(reader["ConfidentialityLevelID"].ToString()))
                            node.ConfidentialityLevel.ID = (Guid)reader["ConfidentialityLevelID"];
                        if (!string.IsNullOrEmpty(reader["ConfidentialityLevelNum"].ToString()))
                            node.ConfidentialityLevel.LevelID = (int)reader["ConfidentialityLevelNum"];
                        if (!string.IsNullOrEmpty(reader["ConfidentialityLevel"].ToString()))
                            node.ConfidentialityLevel.Title = (string)reader["ConfidentialityLevel"];
                        if (!string.IsNullOrEmpty(reader["OwnerID"].ToString()))
                            node.OwnerID = (Guid)reader["OwnerID"];
                        if (!string.IsNullOrEmpty(reader["OwnerName"].ToString()))
                            node.OwnerName = (string)reader["OwnerName"];

                        if (!string.IsNullOrEmpty(reader["PublicationDate"].ToString())) 
                            node.PublicationDate = (DateTime)reader["PublicationDate"];
                        if (!string.IsNullOrEmpty(reader["ExpirationDate"].ToString()))
                            node.ExpirationDate = (DateTime)reader["ExpirationDate"];
                        if (!string.IsNullOrEmpty(reader["Score"].ToString()))
                            node.Score = (double)reader["Score"];
                        if (!string.IsNullOrEmpty(reader["IsFreeUser"].ToString()))
                            node.IsFreeUser = (bool)reader["IsFreeUser"];
                        if (!string.IsNullOrEmpty(reader["HasWikiContent"].ToString()))
                            node.HasWikiContent = (bool)reader["HasWikiContent"];
                        if (!string.IsNullOrEmpty(reader["HasFormContent"].ToString()))
                            node.HasFormContent = (bool)reader["HasFormContent"];
                    }

                    lstNodes.Add(node);
                }
                catch { }
            }

            long totalCount = (hasTotalCount && reader.NextResult()) ? ProviderUtil.succeed_long(reader, dontClose: true) : 0;

            if (fetchCounts && reader.NextResult()) _parse_nodes_count(ref reader, ref lstCounts);

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static long _parse_nodes(ref IDataReader reader, ref List<Node> lstNodes, bool? full, bool hasTotalCount = false)
        {
            List<NodesCount> lst = new List<NodesCount>();
            return _parse_nodes(ref reader, ref lstNodes, ref lst, full, hasTotalCount, fetchCounts: false);
        }

        private static Dictionary<string, object> _parse_node_counts_grouped_by_element(ref IDataReader reader)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ArrayList items = new ArrayList();

            ret["Items"] = items;

            while (reader.Read())
            {
                try
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();

                    dic["TextValue"] = string.IsNullOrEmpty(reader["TextValue"].ToString()) ? null :
                        Base64.encode((string)reader["TextValue"]);
                    dic["BitValue"] = string.IsNullOrEmpty(reader["BitValue"].ToString()) ? null : reader["BitValue"];
                    dic["Type"] = string.IsNullOrEmpty(reader["Type"].ToString()) ? null : reader["Type"];
                    dic["Count"] = string.IsNullOrEmpty(reader["Count"].ToString()) ? null : reader["Count"];

                    items.Add(dic);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        private static long _parse_popular_nodes(ref IDataReader reader, ref List<Node> lstNodes)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    Node node = new Node();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) node.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString())) node.VisitsCount = (int)reader["VisitsCount"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) node.LikesCount = (int)reader["LikesCount"];

                    totalCount = !string.IsNullOrEmpty(reader["TotalCount"].ToString()) ? (long)reader["TotalCount"] : 0;

                    lstNodes.Add(node);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static void _parse_relations(ref IDataReader reader, ref List<Relation> lstRelations)
        {
            while (reader.Read())
            {
                try
                {
                    Relation relation = new Relation();

                    bool isInRelation = false;
                    Guid? nodeId = null;
                    Node relatedNode = new Node();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) nodeId = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodeID"].ToString())) relatedNode.NodeID = (Guid)reader["RelatedNodeID"];
                    if (!string.IsNullOrEmpty(reader["RelationTypeID"].ToString())) 
                        relation.RelationType.RelationTypeID = (Guid)reader["RelationTypeID"];
                    if (!string.IsNullOrEmpty(reader["IsInRelation"].ToString())) isInRelation = (bool)reader["IsInRelation"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodeTypeID"].ToString())) 
                        relatedNode.NodeTypeID = (Guid)reader["RelatedNodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodeName"].ToString())) relatedNode.Name = (string)reader["RelatedNodeName"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodeTypeName"].ToString())) relatedNode.NodeType = (string)reader["RelatedNodeTypeName"];

                    if (!string.IsNullOrEmpty(reader["RelatedNodeTypeAdditionalID"].ToString()))
                    {
                        try { relatedNode.NodeTypeAdditionalID = int.Parse((string)reader["RelatedNodeTypeAdditionalID"]); }
                        catch { }
                    }
                        
                    if (!string.IsNullOrEmpty(reader["RelationTypeAdditionalID"].ToString()))
                        relation.RelationType.AdditionalID = int.Parse((string)reader["RelationTypeAdditionalID"]);
                    if (!string.IsNullOrEmpty(reader["RelationType"].ToString())) relation.RelationType.Name = (string)reader["RelationType"];

                    if (isInRelation)
                    {
                        relation.Destination.NodeID = nodeId;
                        relation.Source = relatedNode;
                    }
                    else
                    {
                        relation.Source.NodeID = nodeId;
                        relation.Destination = relatedNode;
                    }

                    if (lstRelations.Exists(u => u.Source.NodeID == relation.Destination.NodeID && u.Destination.NodeID == relation.Source.NodeID &&
                        u.RelationType.RelationTypeID == relation.RelationType.RelationTypeID))
                        lstRelations.Where(u => u.Source.NodeID == relation.Destination.NodeID && u.Destination.NodeID == relation.Source.NodeID &&
                        u.RelationType.RelationTypeID == relation.RelationType.RelationTypeID).FirstOrDefault().Bidirectional = true;
                    else
                        lstRelations.Add(relation);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_nodes_count(ref IDataReader reader, ref List<NodesCount> lstNodesCount)
        {
            bool? hasOrder = null;

            while (reader.Read())
            {
                try
                {
                    NodesCount nodesCount = new NodesCount();

                    if (!hasOrder.HasValue || hasOrder.Value)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(reader["Order"].ToString())) nodesCount.Order = (int)reader["Order"];
                            if (!string.IsNullOrEmpty(reader["ReverseOrder"].ToString())) nodesCount.ReverseOrder = (int)reader["ReverseOrder"];

                            hasOrder = true;
                        }
                        catch { hasOrder = false; }
                    }

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodesCount.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeAdditionalID"].ToString()))
                        nodesCount.NodeTypeAdditionalID = (string)reader["NodeTypeAdditionalID"];
                    if (!string.IsNullOrEmpty(reader["TypeName"].ToString())) nodesCount.TypeName = (string)reader["TypeName"];
                    if (!string.IsNullOrEmpty(reader["NodesCount"].ToString())) nodesCount.Count = (int)reader["NodesCount"];

                    lstNodesCount.Add(nodesCount);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_node_members(ref IDataReader reader, 
            ref List<NodeMember> lstMembers, bool? parseNode, bool? parseUser)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    NodeMember nodeMember = new NodeMember();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) nodeMember.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) nodeMember.Member.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["MembershipDate"].ToString())) nodeMember.MembershipDate = (DateTime)reader["MembershipDate"];
                    if (!string.IsNullOrEmpty(reader["IsAdmin"].ToString())) nodeMember.IsAdmin = (bool)reader["IsAdmin"];
                    if (!string.IsNullOrEmpty(reader["IsPending"].ToString()))
                        nodeMember.IsPending = (bool)reader["IsPending"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) nodeMember.Status = (string)reader["Status"];
                    if (!string.IsNullOrEmpty(reader["AcceptionDate"].ToString())) nodeMember.AcceptionDate = (DateTime)reader["AcceptionDate"];
                    if (!string.IsNullOrEmpty(reader["Position"].ToString())) nodeMember.Position = (string)reader["Position"];

                    if (parseNode.HasValue && parseNode.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["NodeAdditionalID"].ToString()))
                            nodeMember.Node.AdditionalID = (string)reader["NodeAdditionalID"];
                        if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) nodeMember.Node.Name = (string)reader["NodeName"];
                        if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodeMember.Node.NodeTypeID = (Guid)reader["NodeTypeID"];
                        if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) nodeMember.Node.NodeType = (string)reader["NodeType"];
                        if (!string.IsNullOrEmpty(reader["AcceptionDate"].ToString())) nodeMember.Node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    }

                    if (parseUser.HasValue && parseUser.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["UserName"].ToString())) nodeMember.Member.UserName = (string)reader["UserName"];
                        if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) nodeMember.Member.FirstName = (string)reader["FirstName"];
                        if (!string.IsNullOrEmpty(reader["LastName"].ToString())) nodeMember.Member.LastName = (string)reader["LastName"];
                    }

                    lstMembers.Add(nodeMember);
                }
                catch { }
            }

            if (reader.NextResult()) totalCount = ProviderUtil.succeed_long(reader);

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static void _parse_lists(ref IDataReader reader, ref List<NodeList> lstLists)
        {
            while (reader.Read())
            {
                try
                {
                    NodeList nodeList = new NodeList();

                    if (!string.IsNullOrEmpty(reader["ListID"].ToString())) nodeList.ListID = (Guid)reader["ListID"];
                    if (!string.IsNullOrEmpty(reader["ListName"].ToString())) nodeList.Name = (string)reader["ListName"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString())) nodeList.AdditionalID = (string)reader["AdditionalID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodeList.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) nodeList.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) nodeList.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["OwnerType"].ToString())) nodeList.OwnerType = (string)reader["OwnerType"];

                    lstLists.Add(nodeList);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_hierarchy_admins(ref IDataReader reader, ref List<HierarchyAdmin> lstAdmins)
        {
            while (reader.Read())
            {
                try
                {
                    HierarchyAdmin hierarchyAdmin = new HierarchyAdmin();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) hierarchyAdmin.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) hierarchyAdmin.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Level"].ToString())) hierarchyAdmin.Level = (int)reader["Level"];

                    lstAdmins.Add(hierarchyAdmin);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_tags(ref IDataReader reader, ref List<Tag> lstTags)
        {
            while (reader.Read())
            {
                try
                {
                    Tag tag = new Tag();

                    if (!string.IsNullOrEmpty(reader["TagID"].ToString())) tag.TagID = (Guid)reader["TagID"];
                    if (!string.IsNullOrEmpty(reader["Tag"].ToString())) tag.Text = (string)reader["Tag"];
                    if (!string.IsNullOrEmpty(reader["IsApproved"].ToString())) tag.Approved = (bool)reader["IsApproved"];
                    if (!string.IsNullOrEmpty(reader["CallsCount"].ToString())) tag.CallsCount = (int)reader["CallsCount"];

                    lstTags.Add(tag);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_node_creators(ref IDataReader reader, ref List<NodeCreator> lstCreators, bool? full)
        {
            while (reader.Read())
            {
                try
                {
                    NodeCreator creator = new NodeCreator();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) creator.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) creator.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["CollaborationShare"].ToString())) 
                        creator.CollaborationShare = (double)reader["CollaborationShare"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) creator.Status = (string)reader["Status"];

                    if (full.HasValue && full.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["UserName"].ToString())) creator.User.UserName = (string)reader["UserName"];
                        if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) creator.User.FirstName = (string)reader["FirstName"];
                        if (!string.IsNullOrEmpty(reader["LastName"].ToString())) creator.User.LastName = (string)reader["LastName"];
                    }

                    lstCreators.Add(creator);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_fan_user_ids(ref IDataReader reader, ref List<Guid> retList)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) retList.Add((Guid)reader["UserID"]);
                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static long _parse_experts(ref IDataReader reader, ref List<Expert> lstExperts)
        {
            long? totalCount = null;

            while (reader.Read())
            {
                try
                {
                    Expert expert = new Expert();

                    if (!totalCount.HasValue)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];
                        }
                        catch { totalCount = 0; }
                    }

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) expert.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeAdditionalID"].ToString()))
                        expert.Node.AdditionalID = (string)reader["NodeAdditionalID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) expert.Node.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) expert.Node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) expert.Node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserID"].ToString())) expert.User.UserID = (Guid)reader["ExpertUserID"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserName"].ToString())) expert.User.UserName = (string)reader["ExpertUserName"];
                    if (!string.IsNullOrEmpty(reader["ExpertFirstName"].ToString())) expert.User.FirstName = (string)reader["ExpertFirstName"];
                    if (!string.IsNullOrEmpty(reader["ExpertLastName"].ToString())) expert.User.LastName = (string)reader["ExpertLastName"];

                    lstExperts.Add(expert);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount.HasValue ? totalCount.Value : 0;
        }

        private static void _parse_expertise_suggestions(ref IDataReader reader, ref List<Expert> lstExperts)
        {
            while (reader.Read())
            {
                try
                {
                    Expert expert = new Expert();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) expert.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) expert.Node.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) expert.Node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserID"].ToString())) expert.User.UserID = (Guid)reader["ExpertUserID"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserName"].ToString())) expert.User.UserName = (string)reader["ExpertUserName"];
                    if (!string.IsNullOrEmpty(reader["ExpertFirstName"].ToString())) expert.User.FirstName = (string)reader["ExpertFirstName"];
                    if (!string.IsNullOrEmpty(reader["ExpertLastName"].ToString())) expert.User.LastName = (string)reader["ExpertLastName"];

                    lstExperts.Add(expert);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_node_info(ref IDataReader reader, ref List<NodeInfo> retList)
        {
            while (reader.Read())
            {
                try
                {
                    NodeInfo info = new NodeInfo();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) info.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) info.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) info.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) info.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString())) info.Creator.UserName = (string)reader["CreatorUserName"];
                    if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString())) info.Creator.FirstName = (string)reader["CreatorFirstName"];
                    if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString())) info.Creator.LastName = (string)reader["CreatorLastName"];
                    if (!string.IsNullOrEmpty(reader["ContributorsCount"].ToString())) info.ContributorsCount = (int)reader["ContributorsCount"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) info.LikesCount = (int)reader["LikesCount"];
                    if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString())) info.VisitsCount = (int)reader["VisitsCount"];
                    if (!string.IsNullOrEmpty(reader["ExpertsCount"].ToString())) info.ExpertsCount = (int)reader["ExpertsCount"];
                    if (!string.IsNullOrEmpty(reader["MembersCount"].ToString())) info.MembersCount = (int)reader["MembersCount"];
                    if (!string.IsNullOrEmpty(reader["ChildsCount"].ToString())) info.ChildsCount = (int)reader["ChildsCount"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodesCount"].ToString())) info.RelatedNodesCount = (int)reader["RelatedNodesCount"];
                    if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString())) info.LikeStatus = (bool)reader["LikeStatus"];

                    if (!string.IsNullOrEmpty(reader["Tags"].ToString()))
                    {
                        string strTags = strTags = (string)reader["Tags"];
                        info.Tags = ProviderUtil.get_tags_list(strTags);
                    }

                    retList.Add(info);
                }
                catch (Exception ex) { string strEx = ex.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_extensions(ref IDataReader reader, ref List<Extension> extensions)
        {
            while (reader.Read())
            {
                try
                {
                    Extension extension = new Extension();

                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) extension.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) extension.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Disabled"].ToString())) extension.Disabled = (bool)reader["Disabled"];

                    string strExtension = string.IsNullOrEmpty(reader["Extension"].ToString()) ? null : (string)reader["Extension"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strExtension))
                            extension.ExtensionType = (ExtensionType)Enum.Parse(typeof(ExtensionType), strExtension);
                    }
                    catch { extension.ExtensionType = ExtensionType.NotSet; }

                    extension.Initialized = true;

                    if (extension.ExtensionType != ExtensionType.NotSet) extensions.Add(extension);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_services(ref IDataReader reader, ref List<Service> services)
        {
            while (reader.Read())
            {
                try
                {
                    Service service = new Service();

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString()))
                        service.NodeType.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString()))
                        service.NodeType.Name = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["ServiceTitle"].ToString()))
                        service.Title = (string)reader["ServiceTitle"];
                    if (!string.IsNullOrEmpty(reader["ServiceDescription"].ToString()))
                        service.Description = (string)reader["ServiceDescription"];
                    if (!string.IsNullOrEmpty(reader["EnableContribution"].ToString()))
                        service.EnableContribution = (bool)reader["EnableContribution"];
                    if (!string.IsNullOrEmpty(reader["NoContent"].ToString()))
                        service.NoContent = (bool)reader["NoContent"];
                    if (!string.IsNullOrEmpty(reader["IsKnowledge"].ToString()))
                        service.IsKnowledge = (bool)reader["IsKnowledge"];
                    if (!string.IsNullOrEmpty(reader["IsDocument"].ToString()))
                        service.IsDocument = (bool)reader["IsDocument"];
                    if (!string.IsNullOrEmpty(reader["EnablePreviousVersionSelect"].ToString()))
                        service.EnablePreviousVersionSelect = (bool)reader["EnablePreviousVersionSelect"];
                    if (!string.IsNullOrEmpty(reader["IsTree"].ToString()))
                        service.IsTree = (bool)reader["IsTree"];
                    if (!string.IsNullOrEmpty(reader["UniqueMembership"].ToString()))
                        service.UniqueMembership = (bool)reader["UniqueMembership"];
                    if (!string.IsNullOrEmpty(reader["UniqueAdminMember"].ToString()))
                        service.UniqueAdminMember = (bool)reader["UniqueAdminMember"];
                    if (!string.IsNullOrEmpty(reader["DisableAbstractAndKeywords"].ToString()))
                        service.DisableAbstractAndKeywords = (bool)reader["DisableAbstractAndKeywords"];
                    if (!string.IsNullOrEmpty(reader["DisableFileUpload"].ToString()))
                        service.DisableFileUpload = (bool)reader["DisableFileUpload"];
                    if (!string.IsNullOrEmpty(reader["DisableRelatedNodesSelect"].ToString()))
                        service.DisableRelatedNodesSelect = (bool)reader["DisableRelatedNodesSelect"];
                    if (!string.IsNullOrEmpty(reader["AdminNodeID"].ToString()))
                        service.AdminNode.NodeID = (Guid)reader["AdminNodeID"];
                    if (!string.IsNullOrEmpty(reader["MaxAcceptableAdminLevel"].ToString()))
                        service.MaxAcceptableAdminLevel = (int)reader["MaxAcceptableAdminLevel"];
                    if (!string.IsNullOrEmpty(reader["LimitAttachedFilesTo"].ToString()))
                        service.LimitAttachedFilesTo = ListMaker.get_string_items((string)reader["LimitAttachedFilesTo"], ',');
                    if (!string.IsNullOrEmpty(reader["MaxAttachedFileSize"].ToString()))
                        service.MaxAttachedFileSize = (int)reader["MaxAttachedFileSize"];
                    if (!string.IsNullOrEmpty(reader["MaxAttachedFilesCount"].ToString()))
                        service.MaxAttachedFilesCount = (int)reader["MaxAttachedFilesCount"];
                    if (!string.IsNullOrEmpty(reader["EditableForAdmin"].ToString()))
                        service.EditableForAdmin = (bool)reader["EditableForAdmin"];
                    if (!string.IsNullOrEmpty(reader["EditableForCreator"].ToString()))
                        service.EditableForCreator = (bool)reader["EditableForCreator"];
                    if (!string.IsNullOrEmpty(reader["EditableForOwners"].ToString()))
                        service.EditableForContributors = (bool)reader["EditableForOwners"];
                    if (!string.IsNullOrEmpty(reader["EditableForExperts"].ToString()))
                        service.EditableForExperts = (bool)reader["EditableForExperts"];
                    if (!string.IsNullOrEmpty(reader["EditableForMembers"].ToString()))
                        service.EditableForMembers = (bool)reader["EditableForMembers"];
                    if (!string.IsNullOrEmpty(reader["EditSuggestion"].ToString()))
                        service.EditSuggestion = (bool)reader["EditSuggestion"];

                    string strAdminType = string.IsNullOrEmpty(reader["AdminType"].ToString()) ? null : (string)reader["AdminType"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strAdminType))
                            service.AdminType = (ServiceAdminType)Enum.Parse(typeof(ServiceAdminType), strAdminType);
                    }
                    catch { service.AdminType = ServiceAdminType.NotSet; }

                    services.Add(service);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static bool _parse_user2node_status(ref IDataReader reader, ref Guid? nodeTypeId, 
            ref Guid? areaId, ref bool isCreator, ref bool isContributor, ref bool isExpert, 
            ref bool isMember, ref bool isAdminMember, ref bool isServiceAdmin)
        {
            bool result = true;

            reader.Read();

            try
            {
                if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodeTypeId = (Guid)reader["NodeTypeID"];
                if (!string.IsNullOrEmpty(reader["AreaID"].ToString())) areaId = (Guid)reader["AreaID"];
                if (!string.IsNullOrEmpty(reader["IsCreator"].ToString())) isCreator = (bool)reader["IsCreator"];
                if (!string.IsNullOrEmpty(reader["IsContributor"].ToString())) isContributor = (bool)reader["IsContributor"];
                if (!string.IsNullOrEmpty(reader["IsExpert"].ToString())) isExpert = (bool)reader["IsExpert"];
                if (!string.IsNullOrEmpty(reader["IsMember"].ToString())) isMember = (bool)reader["IsMember"];
                if (!string.IsNullOrEmpty(reader["IsAdminMember"].ToString())) isAdminMember = (bool)reader["IsAdminMember"];
                if (!string.IsNullOrEmpty(reader["IsServiceAdmin"].ToString())) isServiceAdmin = (bool)reader["IsServiceAdmin"];
            }
            catch { result = false; }

            if (!reader.IsClosed) reader.Close();

            return result;
        }

        private static long _parse_explore_items(ref IDataReader reader, ref List<ExploreItem> items)
        {
            long? totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    ExploreItem itm = new CoreNetwork.ExploreItem();

                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];

                    if (!string.IsNullOrEmpty(reader["BaseID"].ToString())) itm.BaseID = (Guid)reader["BaseID"];
                    if (!string.IsNullOrEmpty(reader["BaseTypeID"].ToString())) itm.BaseTypeID = (Guid)reader["BaseTypeID"];
                    if (!string.IsNullOrEmpty(reader["BaseName"].ToString())) itm.BaseName = (string)reader["BaseName"];
                    if (!string.IsNullOrEmpty(reader["BaseType"].ToString())) itm.BaseType = (string)reader["BaseType"];
                    if (!string.IsNullOrEmpty(reader["RelatedID"].ToString())) itm.RelatedID = (Guid)reader["RelatedID"];
                    if (!string.IsNullOrEmpty(reader["RelatedTypeID"].ToString())) itm.RelatedTypeID = (Guid)reader["RelatedTypeID"];
                    if (!string.IsNullOrEmpty(reader["RelatedName"].ToString())) itm.RelatedName = (string)reader["RelatedName"];
                    if (!string.IsNullOrEmpty(reader["RelatedType"].ToString())) itm.RelatedType = (string)reader["RelatedType"];
                    if (!string.IsNullOrEmpty(reader["RelatedCreationDate"].ToString()))
                        itm.RelatedCreationDate = (DateTime)reader["RelatedCreationDate"];
                    if (!string.IsNullOrEmpty(reader["IsTag"].ToString())) itm.IsTag = (bool)reader["IsTag"];
                    if (!string.IsNullOrEmpty(reader["IsRelation"].ToString())) itm.IsRelation = (bool)reader["IsRelation"];
                    if (!string.IsNullOrEmpty(reader["IsRegistrationArea"].ToString()))
                        itm.IsRegistrationArea = (bool)reader["IsRegistrationArea"];

                    items.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return !totalCount.HasValue ? 0 : totalCount.Value;
        }

        private static void _parse_similar_nodes(ref IDataReader reader, ref List<SimilarNode> items)
        {
            while (reader.Read())
            {
                try
                {
                    SimilarNode itm = new CoreNetwork.SimilarNode();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) itm.Suggested.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["Rank"].ToString())) itm.Rank = (double)reader["Rank"];
                    if (!string.IsNullOrEmpty(reader["Tags"].ToString())) itm.Tags = (bool)reader["Tags"];
                    if (!string.IsNullOrEmpty(reader["Favorites"].ToString())) itm.Favorites = (bool)reader["Favorites"];
                    if (!string.IsNullOrEmpty(reader["Relations"].ToString())) itm.Relations = (bool)reader["Relations"];
                    if (!string.IsNullOrEmpty(reader["Experts"].ToString())) itm.Experts = (bool)reader["Experts"];
                    
                    items.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_knowledgable_users(ref IDataReader reader, ref List<KnowledgableUser> items)
        {
            while (reader.Read())
            {
                try
                {
                    KnowledgableUser itm = new CoreNetwork.KnowledgableUser();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) itm.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Rank"].ToString())) itm.Rank = (double)reader["Rank"];
                    if (!string.IsNullOrEmpty(reader["Expert"].ToString())) itm.Expert = (bool)reader["Expert"];
                    if (!string.IsNullOrEmpty(reader["Contributor"].ToString())) itm.Contributor = (bool)reader["Contributor"];
                    if (!string.IsNullOrEmpty(reader["WikiEditor"].ToString())) itm.WikiEditor = (bool)reader["WikiEditor"];
                    if (!string.IsNullOrEmpty(reader["Member"].ToString())) itm.Member = (bool)reader["Member"];
                    if (!string.IsNullOrEmpty(reader["ExpertOfRelatedNode"].ToString()))
                        itm.ExpertOfRelatedNode = (bool)reader["ExpertOfRelatedNode"];
                    if (!string.IsNullOrEmpty(reader["ContributorOfRelatedNode"].ToString()))
                        itm.ContributorOfRelatedNode = (bool)reader["ContributorOfRelatedNode"];
                    if (!string.IsNullOrEmpty(reader["MemberOfRelatedNode"].ToString()))
                        itm.MemberOfRelatedNode = (bool)reader["MemberOfRelatedNode"];

                    items.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static Dictionary<string, Guid> _parse_node_ids(ref IDataReader reader)
        {
            Dictionary<string, Guid> ret = new Dictionary<string, Guid>();

            while (reader.Read())
            {
                try
                {
                    string key = !string.IsNullOrEmpty(reader["AdditionalID"].ToString()) ?
                        (string)reader["AdditionalID"] : null;
                    Guid? value = null;
                    if(!string.IsNullOrEmpty(reader["NodeID"].ToString())) value = (Guid)reader["NodeID"];

                    if (!string.IsNullOrEmpty(key) && value.HasValue) ret[key] = value.Value;
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }


        public static bool Initialize(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("Initialize");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddNodeType(Guid applicationId, NodeType Info, Guid? templateFormId)
        {
            string spName = GetFullyQualifiedName("AddNodeType");

            try
            {
                if (Info.AdditionalID.HasValue) Info.NodeTypeAdditionalID = Info.AdditionalID.Value.ToString();
                if (Info.ParentID == Guid.Empty) Info.ParentID = null;
                
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.NodeTypeID, 
                    Info.NodeTypeAdditionalID, Info.Name, Info.ParentID, Info.IsService, Info.TemplateTypeID,
                    templateFormId, Info.CreatorUserID, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool RenameNodeType(Guid applicationId, NodeType Info)
        {
            string spName = GetFullyQualifiedName("RenameNodeType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeTypeID, Info.Name, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetNodeTypeAdditionalID(Guid applicationId, Guid nodeTypeId, 
            string additionalId, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("SetNodeTypeAdditionalID");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, additionalId, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetAdditionalIDPattern(Guid applicationId, NodeType Info)
        {
            string spName = GetFullyQualifiedName("SetAdditionalIDPattern");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeTypeID, Info.AdditionalIDPattern, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool MoveNodeType(Guid applicationId, List<Guid> nodeTypeIDs, Guid? parentID, Guid currentUserID)
        {
            string spName = GetFullyQualifiedName("MoveNodeType");

            try
            {
                if (parentID == Guid.Empty) parentID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeIDs), ',', parentID, currentUserID, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetNodeTypes(Guid applicationId, ref List<NodeType> retNodeTypes, 
            List<Guid> nodeTypeIds, bool grabSubNodeTypes)
        {
            string spName = GetFullyQualifiedName("GetNodeTypesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeTypeIds), ',', grabSubNodeTypes);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeTypes(Guid applicationId,  ref List<NodeType> retNodeTypes, string searchText,
            bool? isKnowledge, bool? isDocument, bool? archive, List<ExtensionType> extensions, 
            int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetNodeTypes");

            try
            {
                if (extensions == null) extensions = new List<ExtensionType>();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.get_search_text(searchText), isKnowledge, isDocument, archive, 
                    string.Join(",", extensions), ',', count, lowerBoundary);
                totalCount = _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static NodeType GetNodeType(Guid applicationId, Guid? nodeTypeId, NodeTypes? nodeType, Guid? nodeId)
        {
            string spName = GetFullyQualifiedName("GetNodeType");

            try
            {
                string strNodeTypeAdditionalId = null;
                if (nodeType.HasValue) strNodeTypeAdditionalId = 
                        CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

                List<NodeType> lstTypes = new List<NodeType>();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    nodeTypeId, strNodeTypeAdditionalId, nodeId);
                _parse_node_types(ref reader, ref lstTypes);

                return lstTypes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return null;
            }
        }

        public static void HaveChildNodeTypes(Guid applicationId, ref List<Guid> retNodeTypeIDs, ref List<Guid> nodeIds)
        {
            string spName = GetFullyQualifiedName("HaveChildNodeTypes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retNodeTypeIDs);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetChildNodeTypes(Guid applicationId, ref List<NodeType> retNodeTypes, Guid? parentId, bool? archive)
        {
            string spName = GetFullyQualifiedName("GetChildNodeTypes");

            try
            {
                if (parentId == Guid.Empty) parentId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, parentId, archive);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool ArithmeticDeleteNodeTypes(Guid applicationId, 
            ref List<Guid> nodeTypeIds, bool? removeHierarchy, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteNodeTypes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeTypeIds), ',', removeHierarchy, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool RecoverNodeType(Guid applicationId, NodeType Info)
        {
            string spName = GetFullyQualifiedName("RecoverNodeType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeTypeID, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddRelationType(Guid applicationId, RelationType Info)
        {
            string spName = GetFullyQualifiedName("AddRelationType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.RelationTypeID, Info.Name, Info.Description, Info.CreatorUserID, Info.CreationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ModifyRelationType(Guid applicationId, RelationType Info)
        {
            string spName = GetFullyQualifiedName("ModifyRelationType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.RelationTypeID, 
                    Info.Name, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetRelationTypes(Guid applicationId, ref List<RelationType> retRelationTypes)
        {
            string spName = GetFullyQualifiedName("GetRelationTypes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                _parse_relation_types(ref reader, ref retRelationTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool ArithmeticDeleteRelationType(Guid applicationId, RelationType Info)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteRelationType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.RelationTypeID, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddRelations(Guid applicationId, ref List<Relation> relations, Guid creatorUserId)
        {
            string spName = GetFullyQualifiedName("AddRelation");

            try
            {
                List<KeyValuePair<KeyValuePair<Guid, Guid>, Guid>> _lstRelations = 
                    new List<KeyValuePair<KeyValuePair<Guid, Guid>, Guid>>();

                foreach (Relation _item in relations)
                {
                    Guid relTypeId = Guid.Empty;
                    if (_item.RelationType.RelationTypeID.HasValue) relTypeId = _item.RelationType.RelationTypeID.Value;

                    _lstRelations.Add(new KeyValuePair<KeyValuePair<Guid, Guid>, Guid>(
                        new KeyValuePair<Guid, Guid>(_item.Source.NodeID.Value, _item.Destination.NodeID.Value),
                        relTypeId));
                }

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.triple_list_to_string<Guid, Guid, Guid>(ref _lstRelations, '|', ','), '|', ',', 
                    creatorUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SaveRelations(Guid applicationId, 
            Guid nodeId, List<Guid> relatedNodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SaveRelations");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, ProviderUtil.list_to_string<Guid>(relatedNodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool MakeParent(Guid applicationId, ref List<Relation> relations, Guid creatorUserId)
        {
            string spName = GetFullyQualifiedName("MakeParent");
            
            try
            {
                List<KeyValuePair<Guid, Guid>> _lstRelations = new List<KeyValuePair<Guid, Guid>>();
                foreach (Relation _item in relations)
                {
                    _lstRelations.Add(
                        new KeyValuePair<Guid, Guid>(_item.Source.NodeID.Value, _item.Destination.NodeID.Value));
                }

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.pair_list_to_string<Guid, Guid>(ref _lstRelations, '|', ','), '|', ',', 
                    creatorUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool MakeCorrelation(Guid applicationId, ref List<Relation> relations, Guid creatorUserId)
        {
            string spName = GetFullyQualifiedName("MakeCorrelation");
            
            try
            {
                List<KeyValuePair<Guid, Guid>> _lstRelations = new List<KeyValuePair<Guid, Guid>>();
                foreach (Relation _item in relations)
                {
                    _lstRelations.Add(
                        new KeyValuePair<Guid, Guid>(_item.Source.NodeID.Value, _item.Destination.NodeID.Value));
                }

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.pair_list_to_string<Guid, Guid>(ref _lstRelations, '|', ','), '|', ',', 
                    creatorUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteRelations(Guid applicationId, 
            ref List<Relation> relations, Guid? relationTypeId, Guid lastModifierUserId, bool? reverseAlso)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteRelations");
            
            try
            {
                List<KeyValuePair<Guid, Guid>> _lstRelations = new List<KeyValuePair<Guid, Guid>>();
                foreach (Relation _item in relations)
                {
                    _lstRelations.Add(
                        new KeyValuePair<Guid, Guid>(_item.Source.NodeID.Value, _item.Destination.NodeID.Value));
                }

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.pair_list_to_string<Guid, Guid>(ref _lstRelations, '|', ','), '|', ',', relationTypeId,
                    lastModifierUserId, DateTime.Now, reverseAlso));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteCorrelation(Guid applicationId, 
            ref List<Relation> relations, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteCorrelations");
            
            try
            {
                List<KeyValuePair<Guid, Guid>> _lstRelations = new List<KeyValuePair<Guid, Guid>>();
                foreach (Relation _item in relations)
                {
                    _lstRelations.Add(
                        new KeyValuePair<Guid, Guid>(_item.Source.NodeID.Value, _item.Destination.NodeID.Value));
                }

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.pair_list_to_string<Guid, Guid>(ref _lstRelations, '|', ','), '|', ',', 
                    lastModifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool Unparent(Guid applicationId, ref List<Relation> relations, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("Unparent");
            
            try
            {
                List<KeyValuePair<Guid, Guid>> _lstRelations = new List<KeyValuePair<Guid, Guid>>();
                foreach (Relation _item in relations)
                {
                    _lstRelations.Add(
                        new KeyValuePair<Guid, Guid>(_item.Source.NodeID.Value, _item.Destination.NodeID.Value));
                }
                
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.pair_list_to_string<Guid, Guid>(ref _lstRelations, '|', ','), '|', ',', 
                    lastModifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddNode(Guid applicationId, 
            Node Info, NodeTypes? nodeType, string nodeTypeAdditionalId, bool? addMember)
        {
            string spName = GetFullyQualifiedName("AddNode");

            try
            {
                if (nodeType != null)
                    nodeTypeAdditionalId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

                if (Info.NodeTypeID == Guid.Empty) Info.NodeTypeID = null;
                if (Info.ParentNodeID == Guid.Empty) Info.ParentNodeID = null;
                if (Info.DocumentTreeNodeID == Guid.Empty) Info.DocumentTreeNodeID = null;

                if (string.IsNullOrEmpty(Info.AdditionalID)) Info.AdditionalID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeID, Info.AdditionalID_Main, Info.AdditionalID, Info.NodeTypeID, nodeTypeAdditionalId, 
                    Info.DocumentTreeNodeID, Info.Name, Info.Description, ProviderUtil.get_tags_text(Info.Tags), 
                    Info.Searchable, Info.Creator.UserID, Info.CreationDate, Info.ParentNodeID, addMember));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetAdditionalID(Guid applicationId,
            Guid id, string additionalId_main, string additionalId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetAdditionalID");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, additionalId_main, additionalId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ModifyNode(Guid applicationId, Node Info)
        {
            string spName = GetFullyQualifiedName("ModifyNode");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeID, Info.Name, Info.Description, ProviderUtil.get_tags_text(Info.Tags),
                    Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ChangeNodeType(Guid applicationId, Guid nodeId, Guid nodeTypeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ChangeNodeType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, nodeTypeId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetDocumentTreeNodeID(Guid applicationId, 
            List<Guid> nodeIds, Guid? documentTreeNodeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetDocumentTreeNodeID");

            try
            {
                if (documentTreeNodeId == Guid.Empty) documentTreeNodeId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', documentTreeNodeId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ModifyNodeName(Guid applicationId, Node Info)
        {
            string spName = GetFullyQualifiedName("ModifyNodeName");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeID, Info.Name, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ModifyNodeDescription(Guid applicationId, Node Info)
        {
            string spName = GetFullyQualifiedName("ModifyNodeDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeID, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ModifyNodePublicDescription(Guid applicationId, Guid nodeId, string description)
        {
            string spName = GetFullyQualifiedName("ModifyNodePublicDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, description));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetNodeExpirationDate(Guid applicationId, Guid nodeId, DateTime? expirationDate)
        {
            string spName = GetFullyQualifiedName("SetNodeExpirationDate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, expirationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetExpiredNodesAsNotSearchable(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("SetExpiredNodesAsNotSearchable");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static List<Guid> GetNodeIDsThatWillBeExpiredSoon(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("GetNodeIDsThatWillBeExpiredSoon");

            try
            {
                List<Guid> lst = new List<Guid>();

                ProviderUtil.parse_guids(ProviderUtil.execute_reader(spName, applicationId,
                    DateTime.Now.AddDays(RaaiVanSettings.Knowledge.AlertExpirationInDays(applicationId))), ref lst);

                return lst;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static bool NotifyNodeExpiration(Guid applicationId, Guid nodeId, Guid userId, 
            ref List<Dashboard> retDashboards)
        {
            string spName = GetFullyQualifiedName("NotifyNodeExpiration");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, userId, DateTime.Now);
                
                return ProviderUtil.parse_dashboards(ref reader, ref retDashboards) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetPreviousVersion(Guid applicationId, 
            Guid nodeId, Guid? previousVersionId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetPreviousVersion");

            try
            {
                if (previousVersionId == Guid.Empty) previousVersionId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, previousVersionId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetPreviousVersions(Guid applicationId, 
            ref List<Node> retList, Guid nodeId, Guid? currentUserId, bool? checkPrivacy)
        {
            string spName = GetFullyQualifiedName("GetPreviousVersions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, currentUserId, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                _parse_nodes(ref reader, ref retList, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNewVersions(Guid applicationId, ref List<Node> retList, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetNewVersions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref retList, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool ModifyNodeTags(Guid applicationId, Node Info)
        {
            string spName = GetFullyQualifiedName("ModifyNodeTags");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.NodeID, ProviderUtil.get_tags_text(Info.Tags), Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static string GetNodeDescription(Guid applicationId, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetNodeDescription");

            try
            {
                return ProviderUtil.succeed_string(ProviderUtil.execute_reader(spName, applicationId, nodeId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return string.Empty;
            }
        }

        public static bool SetNodesSearchability(Guid applicationId, 
            ref List<Guid> nodeIds, bool searchable, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetNodesSearchability");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', searchable, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteNodes(Guid applicationId, 
            ref List<Guid> nodeIds, bool? removeHierarchy, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', removeHierarchy, lastModifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool RecycleNodes(Guid applicationId, ref List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RecycleNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetNodeTypesOrder(Guid applicationId, List<Guid> nodeTypeIds)
        {
            string spName = GetFullyQualifiedName("SetNodeTypesOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeTypeIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetNodesOrder(Guid applicationId, List<Guid> nodeIds)
        {
            string spName = GetFullyQualifiedName("SetNodesOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetNodesCount(Guid applicationId, ref List<NodesCount> retNodesCount, 
            ref List<Guid> nodeTypeIds, NodeTypes? nodeType, DateTime? lowerCreationDateLimit, 
            DateTime? upperCreationDateLimit, bool? root, bool? archive)
        {
            string spName = GetFullyQualifiedName("GetNodesCount");

            try
            {
                string strAdditionalId = null;
                if (nodeType.HasValue)
                    strAdditionalId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeTypeIds), ',', strAdditionalId,
                    lowerCreationDateLimit, upperCreationDateLimit, root, archive);
                _parse_nodes_count(ref reader, ref retNodesCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetMostPopulatedNodeTypes(Guid applicationId, 
            ref List<NodesCount> retNodesCount, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetMostPopulatedNodeTypes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, count, lowerBoundary);
                _parse_nodes_count(ref reader, ref retNodesCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static int GetNodeRecordsCount(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("GetNodeRecordsCount");

            try
            {
                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return -1;
            }
        }

        public static List<Guid> GetNodeTypeIDs(Guid applicationId, List<string> nodeTypeAdditionalIds)
        {
            string spName = GetFullyQualifiedName("GetNodeTypeIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    string.Join(",", nodeTypeAdditionalIds), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static void GetNodeIDs(Guid applicationId, ref List<Guid> retNodeIDs, 
            List<string> nodeAdditionalIds, Guid? nodeTypeId, NodeTypes? nodeType, string nodeTypeAdditionalId)
        {
            if (!nodeTypeId.HasValue && !nodeType.HasValue && string.IsNullOrEmpty(nodeTypeAdditionalId)) return;

            if (nodeType.HasValue)
                nodeTypeAdditionalId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add AdditionalIDs
            DataTable additionalIdsTable = new DataTable();
            additionalIdsTable.Columns.Add("Value", typeof(string));
            foreach (string _adId in nodeAdditionalIds) additionalIdsTable.Rows.Add(_adId);

            SqlParameter additionalIdsParam = new SqlParameter("@NodeAdditionalIDs", SqlDbType.Structured);
            additionalIdsParam.TypeName = "[dbo].[StringTableType]";
            additionalIdsParam.Value = additionalIdsTable;
            //end of Add AdditionalIDs

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(additionalIdsParam);
            if (nodeTypeId.HasValue) cmd.Parameters.AddWithValue("@NodeTypeID", nodeTypeId);
            if(!string.IsNullOrEmpty(nodeTypeAdditionalId))
                cmd.Parameters.AddWithValue("@NodeTypeAddtionalID", nodeTypeAdditionalId);

            string spName = GetFullyQualifiedName("GetNodeIDs");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@NodeAdditionalIDs" + sep +
                (nodeTypeId.HasValue ? "@NodeTypeID" : "null") + sep +
                (!string.IsNullOrEmpty(nodeTypeAdditionalId) ? "@NodeTypeAddtionalID" : "null");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                ProviderUtil.parse_guids(ref reader, ref retNodeIDs);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
            finally { con.Close(); }
        }

        public static void GetNodeIDs(Guid applicationId, ref List<Guid> retNodeIDs, ref List<Node> nodes)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add AdditionalIDs
            DataTable nodesTable = new DataTable();
            nodesTable.Columns.Add("FirstValue", typeof(string));
            nodesTable.Columns.Add("SecondValue", typeof(string));
            foreach (Node _nd in nodes) nodesTable.Rows.Add(_nd.AdditionalID, _nd.TypeAdditionalID);

            SqlParameter nodesParam = new SqlParameter("@Nodes", SqlDbType.Structured);
            nodesParam.TypeName = "[dbo].[StringPairTableType]";
            nodesParam.Value = nodesTable;
            //end of Add AdditionalIDs

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(nodesParam);

            string spName = GetFullyQualifiedName("GetNodeIDsByAdditionalIDs");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Nodes";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                ProviderUtil.parse_guids(ref reader, ref retNodeIDs);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
            finally { con.Close(); }
        }

        public static Dictionary<string, Guid> GetNodeIDs(Guid applicationId, Guid nodeTypeId, List<string> additionalIds)
        {
            string spName = GetFullyQualifiedName("GetNodeIDsByAdditionalID");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, string.Join("~", additionalIds), '~');
                return _parse_node_ids(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new Dictionary<string, Guid>();
            }
        }

        public static void GetNodes(Guid applicationId, 
            ref List<Node> retNodes, ref List<Guid> nodeIds, bool? full, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("GetNodesByIDs");

            try
            {
                if (nodeIds.Count == 0) return;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', full, currentUserId);
                _parse_nodes(ref reader, ref retNodes, full);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        private static void GetNodes(Guid applicationId, ref List<Node> retNodes, ref List<NodesCount> retNodesCount, 
            List<Guid> nodeTypeIds, NodeTypes? nodeType, bool? useNodeTypeHierarchy, Guid? relatedToNodeId, string searchText, 
            bool? isDocument, bool? isKnowledge, DateTime? lowerCreationDateLimit, DateTime? upperCreationDateLimit,
            int count, long? lowerBoundary, bool? searchable, bool? archive, bool? grabNoContentServices, 
            List<FormFilter> filters, bool? matchAllFilters, bool? fetchCounts, Guid? currentUserId, Guid? creatorUserId, 
            bool checkAccess, ref long totalCount, Guid? groupByFormElementId, ref Dictionary<string, object> groupedResults)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            
            //Add Filters
            DataTable filtersTable = new DataTable();
            filtersTable.Columns.Add("ElementID", typeof(Guid));
            filtersTable.Columns.Add("OwnerID", typeof(Guid));
            filtersTable.Columns.Add("Text", typeof(string));
            filtersTable.Columns.Add("TextItems", typeof(string));
            filtersTable.Columns.Add("Or", typeof(bool));
            filtersTable.Columns.Add("Exact", typeof(bool));
            filtersTable.Columns.Add("DateFrom", typeof(DateTime));
            filtersTable.Columns.Add("DateTo", typeof(DateTime));
            filtersTable.Columns.Add("FloatFrom", typeof(double));
            filtersTable.Columns.Add("FloatTo", typeof(double));
            filtersTable.Columns.Add("Bit", typeof(bool));
            filtersTable.Columns.Add("Guid", typeof(Guid));
            filtersTable.Columns.Add("GuidItems", typeof(string));
            filtersTable.Columns.Add("Compulsory", typeof(bool));

            if (filters != null)
            {
                foreach (FormFilter f in filters)
                {
                    filtersTable.Rows.Add(f.ElementID, f.OwnerID, f.Text, ProviderUtil.list_to_string<string>(f.TextItems),
                        f.Or, f.Exact, f.DateFrom, f.DateTo, f.FloatFrom, f.FloatTo, f.Bit, f.Guid,
                        ProviderUtil.list_to_string<Guid>(f.GuidItems), f.Compulsory);
                }
            }

            SqlParameter filtersParam = new SqlParameter("@FormFilters", SqlDbType.Structured);
            filtersParam.TypeName = "[dbo].[FormFilterTableType]";
            filtersParam.Value = filtersTable;
            //end of Add Filters

            string strAddId = null;
            if (nodeType != null) strAddId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();
            nodeTypeIds = nodeTypeIds == null ? new List<Guid>() : nodeTypeIds;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            if (currentUserId.HasValue && currentUserId != Guid.Empty)
                cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId.Value);
            if(nodeTypeIds != null && nodeTypeIds.Count > 0)
                cmd.Parameters.AddWithValue("@strNodeTypeIDs", ProviderUtil.list_to_string<Guid>(nodeTypeIds));
            cmd.Parameters.AddWithValue("@delimiter", ',');
            if(!string.IsNullOrEmpty(strAddId)) cmd.Parameters.AddWithValue("@NodeTypeAdditionalID", strAddId);
            if (useNodeTypeHierarchy.HasValue)
                cmd.Parameters.AddWithValue("@UseNodeTypeHierarchy", useNodeTypeHierarchy.Value);
            if (relatedToNodeId.HasValue) cmd.Parameters.AddWithValue("@RelatedToNodeID", relatedToNodeId.Value);
            if (!string.IsNullOrEmpty(searchText))
                cmd.Parameters.AddWithValue("@SearchText", ProviderUtil.get_search_text(searchText));
            if (isDocument.HasValue) cmd.Parameters.AddWithValue("@IsDocument", isDocument.Value);
            if (isKnowledge.HasValue) cmd.Parameters.AddWithValue("@IsKnowledge", isKnowledge.Value);
            if (creatorUserId.HasValue) cmd.Parameters.AddWithValue("@CreatorUserID", creatorUserId);
            if (searchable.HasValue) cmd.Parameters.AddWithValue("@Searchable", searchable.Value);
            if (archive.HasValue) cmd.Parameters.AddWithValue("@Archive", archive.Value);
            if (grabNoContentServices.HasValue)
                cmd.Parameters.AddWithValue("@GrabNoContentServices", grabNoContentServices.Value);
            if (lowerCreationDateLimit.HasValue)
                cmd.Parameters.AddWithValue("@LowerCreationDateLimit", lowerCreationDateLimit.Value);
            if (upperCreationDateLimit.HasValue)
                cmd.Parameters.AddWithValue("@UpperCreationDateLimit", upperCreationDateLimit.Value);
            cmd.Parameters.AddWithValue("@Count", count);
            if(lowerBoundary.HasValue) cmd.Parameters.AddWithValue("@LowerBoundary", lowerBoundary.Value);
            cmd.Parameters.Add(filtersParam);
            if (matchAllFilters.HasValue) cmd.Parameters.AddWithValue("@MatchAllFilters", matchAllFilters.Value);
            if (fetchCounts.HasValue) cmd.Parameters.AddWithValue("@FetchCounts", fetchCounts.Value);
            cmd.Parameters.AddWithValue("@CheckAccess", checkAccess);
            cmd.Parameters.AddWithValue("@DefaultPrivacy", RaaiVanSettings.DefaultPrivacy(applicationId));
            if(groupByFormElementId.HasValue && groupByFormElementId != Guid.Empty)
                cmd.Parameters.AddWithValue("@GroupByFormElementID", groupByFormElementId.Value);

            string spName = GetFullyQualifiedName("GetNodes");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + 
                (currentUserId.HasValue && currentUserId != Guid.Empty ? "@CurrentUserID" : "null") + sep +
                (nodeTypeIds != null && nodeTypeIds.Count > 0 ? "@strNodeTypeIDs" : "null") + sep + 
                "@delimiter" + sep +
                (!string.IsNullOrEmpty(strAddId) ? "@NodeTypeAdditionalID" : "null") + sep +
                (useNodeTypeHierarchy.HasValue ? "@UseNodeTypeHierarchy" : "null") + sep +
                (relatedToNodeId.HasValue ? "@RelatedToNodeID" : "null") + sep +
                (!string.IsNullOrEmpty(searchText) ? "@SearchText" : "null") + sep +
                (isDocument.HasValue ? "@IsDocument" : "null") + sep +
                (isKnowledge.HasValue ? "@IsKnowledge" : "null") + sep +
                (creatorUserId.HasValue ? "@CreatorUserID" : "null") + sep +
                (searchable.HasValue ? "@Searchable" : "null") + sep +
                (archive.HasValue ? "@Archive" : "null") + sep +
                (grabNoContentServices.HasValue ? "@GrabNoContentServices" : "null") + sep +
                (lowerCreationDateLimit.HasValue ? "@LowerCreationDateLimit" : "null") + sep +
                (upperCreationDateLimit.HasValue ? "@UpperCreationDateLimit" : "null") + sep +
                "@Count" + sep +
                (lowerBoundary.HasValue ? "@LowerBoundary" : "null") + sep +
                "@FormFilters" + sep +
                (matchAllFilters.HasValue ? "@MatchAllFilters" : "null") + sep +
                (fetchCounts.HasValue ? "@FetchCounts" : "null") + sep +
                "@CheckAccess" + sep +
                "@DefaultPrivacy" + sep + 
                (!groupByFormElementId.HasValue || groupByFormElementId == Guid.Empty ? "null" : "@GroupByFormElementID");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();

                if (groupByFormElementId.HasValue && groupByFormElementId != Guid.Empty)
                    groupedResults = _parse_node_counts_grouped_by_element(ref reader);
                else
                {
                    totalCount = _parse_nodes(ref reader, ref retNodes, ref retNodesCount, false, true, 
                        fetchCounts: fetchCounts.HasValue && fetchCounts.Value);
                    if (totalCount < 0) totalCount = 0;
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
            finally { con.Close(); }
        }

        public static void GetNodes(Guid applicationId, ref List<Node> retNodes, ref List<NodesCount> retNodesCount, 
            List<Guid> nodeTypeIds, NodeTypes? nodeType, bool? useNodeTypeHierarchy, Guid? relatedToNodeId, string searchText,
            bool? isDocument, bool? isKnowledge, DateTime? lowerCreationDateLimit, DateTime? upperCreationDateLimit,
            int count, long? lowerBoundary, bool? searchable, bool? archive, bool? grabNoContentServices,
            List<FormFilter> filters, bool? matchAllFilters, bool? fetchCounts, Guid? currentUserId, Guid? creatorUserId,
            bool checkAccess, ref long totalCount)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            GetNodes(applicationId, ref retNodes, ref retNodesCount, nodeTypeIds, nodeType, useNodeTypeHierarchy, relatedToNodeId,
                searchText, isDocument, isKnowledge, lowerCreationDateLimit, upperCreationDateLimit, count, lowerBoundary,
                searchable, archive, grabNoContentServices, filters, matchAllFilters, fetchCounts, currentUserId, creatorUserId, 
                checkAccess, ref totalCount, groupByFormElementId: null, groupedResults: ref dic);
        }

        public static Dictionary<string, object> GetNodes(Guid applicationId, Guid nodeTypeId, Guid groupByFormElementId,
            Guid? relatedToNodeId, string searchText, DateTime? lowerCreationDateLimit, DateTime? upperCreationDateLimit,
            bool? searchable,  List<FormFilter> filters, bool? matchAllFilters, Guid? currentUserId, Guid? creatorUserId, bool checkAccess)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            List<Node> nds = new List<Node>();
            List<NodesCount> cnts = new List<NodesCount>();
            long totalCount = 0;

            GetNodes(applicationId, ref nds, ref cnts,
                nodeTypeIds: new List<Guid>() { nodeTypeId }, 
                nodeType: null, 
                useNodeTypeHierarchy: null, 
                relatedToNodeId: relatedToNodeId,
                searchText: searchText, 
                isDocument: null, 
                isKnowledge: null, 
                lowerCreationDateLimit, 
                upperCreationDateLimit, 
                count: 0, 
                lowerBoundary: null,
                searchable, 
                archive: false, 
                grabNoContentServices: null, 
                filters: filters, 
                matchAllFilters: matchAllFilters, 
                fetchCounts: false,
                currentUserId: currentUserId, 
                creatorUserId: creatorUserId, 
                checkAccess: checkAccess,
                totalCount: ref totalCount, 
                groupByFormElementId: groupByFormElementId, 
                groupedResults: ref dic);

            return dic;
        }

        public static void GetMostPopularNodes(Guid applicationId, ref List<Node> retNodes, List<Guid> nodeTypeIds, 
            Guid? parentNodeId, int count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetMostPopularNodes");

            try
            {
                if (parentNodeId == Guid.Empty) parentNodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ',', parentNodeId, count, lowerBoundary);
                totalCount = _parse_popular_nodes(ref reader, ref retNodes);
                if (totalCount < 0) totalCount = 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetParentNodes(Guid applicationId, ref List<Node> retNodes, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetParentNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetChildNodes(Guid applicationId, ref List<Node> retNodes, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetChildNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetDefaultRelatedNodes(Guid applicationId, ref List<Node> retNodes, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetDefaultRelatedNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetDefaultConnectedNodes(Guid applicationId, ref List<Node> retNodes, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetDefaultConnectedNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetBrotherNodes(Guid applicationId, ref List<Node> retNodes, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetBrotherNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }
        
        public static void GetDirectChilds(Guid applicationId, ref List<Node> retNodes, Guid? nodeId, Guid? nodeTypeId, 
            string nodeTypeAdditionalId, bool? searchable, double? lowerBoundary, int? count,
            string orderBy, bool? orderByDesc, string searchText, bool? checkAccess, Guid? currentUserId, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetDirectChilds");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, 
                    applicationId, nodeId, nodeTypeId, nodeTypeAdditionalId, searchable, lowerBoundary, count, 
                    orderBy, orderByDesc, ProviderUtil.get_search_text(searchText), checkAccess, 
                    currentUserId, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                totalCount = _parse_nodes(ref reader, ref retNodes, false, true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static Node GetDirectParent(Guid applicationId, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetDirectParent");

            try
            {
                List<Node> _lstNodes = new List<Node>();
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId);
                _parse_nodes(ref reader, ref _lstNodes, false);

                return _lstNodes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return null;
            }
        }

        public static bool SetDirectParent(Guid applicationId, 
            ref List<Guid> nodeIds, Guid? parentNodeId, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("SetDirectParent");

            try
            {
                if (parentNodeId == Guid.Empty) parentNodeId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', parentNodeId, 
                    currentUserId, DateTime.Now), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void HaveChilds(Guid applicationId, ref List<Guid> retNodeIDs, ref List<Guid> nodeIds)
        {
            string spName = GetFullyQualifiedName("HaveChilds");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retNodeIDs);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetRelatedNodeIDs(Guid applicationId, ref List<Guid> retNodeIds, Guid nodeId, 
            Guid? nodeTypeId, string searchText, bool? inRelations, bool? outRelations,
            bool? inTagRelations, bool? outTagRelations, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetRelatedNodeIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, nodeTypeId, ProviderUtil.get_search_text(searchText), inRelations, outRelations,
                    inTagRelations, outTagRelations, count, lowerBoundary);
                ProviderUtil.parse_guids(ref reader, ref retNodeIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetRelatedNodes(Guid applicationId, ref List<Node> retNodes, Guid nodeId, 
            Guid? nodeTypeId, string searchText, bool? inRelations, bool? outRelations,
            bool? inTagRelations, bool? outTagRelations, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetRelatedNodes");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (string.IsNullOrEmpty(searchText)) searchText = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, nodeTypeId, ProviderUtil.get_search_text(searchText), inRelations, outRelations,
                    inTagRelations, outTagRelations, count, lowerBoundary);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetRelatedNodesCount(Guid applicationId, ref List<NodesCount> retNodesCount, Guid nodeId,
            Guid? nodeTypeId, string searchText, bool? inRelations, bool? outRelations,
            bool? inTagRelations, bool? outTagRelations, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetRelatedNodesCount");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (string.IsNullOrEmpty(searchText)) searchText = null;
                
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, nodeTypeId, ProviderUtil.get_search_text(searchText), inRelations, outRelations,
                    inTagRelations, outTagRelations, count, lowerBoundary);
                _parse_nodes_count(ref reader, ref retNodesCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetRelatedNodesPartitioned(Guid applicationId, ref List<Node> retNodes, Guid nodeId,
            List<Guid> nodeTypeIds, bool? inRelations, bool? outRelations, 
            bool? inTagRelations, bool? outTagRelations, int? count)
        {
            string spName = GetFullyQualifiedName("GetRelatedNodesPartitioned");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, string.Join(",", nodeTypeIds), ',', inRelations, outRelations,
                    inTagRelations, outTagRelations, count);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool RelationExists(Guid applicationId, Guid sourceNodeId, Guid destinationNodeId, bool? reverseAlso)
        {
            string spName = GetFullyQualifiedName("RelationExists");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    sourceNodeId, destinationNodeId, reverseAlso));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddMember(Guid applicationId, NodeMember Info, ref List<Dashboard> retDashboards)
        {
            string spName = GetFullyQualifiedName("AddMember");
            
            try
            {
                bool isPending = Info.Status == NodeMemberStatuses.Pending.ToString() ? true : false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.Node.NodeID, Info.Member.UserID, Info.MembershipDate, Info.IsAdmin, isPending,
                    Info.AcceptionDate, Info.Position), ref retDashboards);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteMember(Guid applicationId, Guid nodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteMember");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AcceptMember(Guid applicationId, Guid nodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("AcceptMember");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName,
                    applicationId, nodeId, userId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetMemberPosition(Guid applicationId, Guid nodeId, Guid userId, string position)
        {
            string spName = GetFullyQualifiedName("SetMemberPosition");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId, userId, position));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool IsNodeCreator(Guid applicationId, 
            Guid? nodeId, Guid? nodeTypeId, string additionalId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsNodeCreator");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (string.IsNullOrEmpty(additionalId)) additionalId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, nodeTypeId, additionalId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool IsNodeMember(Guid applicationId, 
            Guid nodeId, Guid userId, bool? isAdmin, NodeMemberStatuses? status)
        {
            string spName = GetFullyQualifiedName("IsNodeMember");

            try
            {
                string strStatus = status.HasValue ? status.ToString() : null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    nodeId, userId, isAdmin, strStatus));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool HasAdmin(Guid applicationId, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("HasAdmin");

            try { return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId)); }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetUnsetNodeAdmin(Guid applicationId, Guid nodeId, Guid userId, bool admin, bool unique)
        {
            string spName = GetFullyQualifiedName("SetUnsetNodeAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    nodeId, userId, admin, unique));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool IsNodeAdmin(Guid applicationId, Guid nodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsNodeAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetComplexAdmins(Guid applicationId, ref List<Guid> retIds, Guid listIdOrNodeId)
        {
            string spName = GetFullyQualifiedName("GetComplexAdmins");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, listIdOrNodeId);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool IsComplexAdmin(Guid applicationId, Guid nodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsComplexAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static Guid? GetComplexTypeID(Guid applicationId, Guid listId)
        {
            string spName = GetFullyQualifiedName("GetComplexTypeID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, listId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return null;
            }
        }

        public static bool GetUser2NodeStatus(Guid applicationId, Guid userId, Guid nodeId, 
            ref Guid? nodeTypeId, ref Guid? areaId, ref bool isCreator, ref bool isContributor, 
            ref bool isExpert, ref bool isMember, ref bool isAdminMember, ref bool isServiceAdmin)
        {
            string spName = GetFullyQualifiedName("GetUser2NodeStatus");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeId);
                return _parse_user2node_status(ref reader, ref nodeTypeId, ref areaId, ref isCreator, 
                    ref isContributor, ref isExpert, ref isMember, ref isAdminMember, ref isServiceAdmin);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetNodeHierarchy(Guid applicationId, 
            ref List<Hierarchy> retNodes, Guid nodeId, bool? sameType)
        {
            string spName = GetFullyQualifiedName("GetNodeHierarchy");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, sameType);
                ProviderUtil.parse_hierarchy(ref reader, ref retNodes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeTypesHierarchy(Guid applicationId, 
            ref List<Hierarchy> retNodeTypes,  List<Guid> nodeTypeIds)
        {
            string spName = GetFullyQualifiedName("GetNodeTypesHierarchy");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ',');
                ProviderUtil.parse_hierarchy(ref reader, ref retNodeTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static int GetTreeDepth(Guid applicationId, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetTreeDepth");

            try { return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId)); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return 0;
            }
        }

        public static void GetHierarchyAdmins(Guid applicationId, 
            ref List<HierarchyAdmin> retAdmins, Guid nodeId, bool? sameType)
        {
            string spName = GetFullyQualifiedName("GetNodeHierarchyAdminIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, sameType);
                _parse_hierarchy_admins(ref reader, ref retAdmins);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetMembers(Guid applicationId, ref List<NodeMember> retNodeMembers, 
            List<Guid> nodeIds, bool? pending, bool? admin, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetMembers");

            if (nodeIds == null || nodeIds.Count == 0) return;

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', 
                    pending, admin, ProviderUtil.get_search_text(searchText), count, lowerBoundary);
                totalCount = _parse_node_members(ref reader, ref retNodeMembers, false, true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static NodeMember GetMember(Guid applicationId, Guid nodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetMember");

            try
            {
                List<NodeMember> members = new List<NodeMember>();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, userId);
                _parse_node_members(ref reader, ref members, false, true);

                return members.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return null;
            }
        }

        public static void GetMemberUserIDs(Guid applicationId, 
            ref List<Guid> retUserIds, ref List<Guid> nodeIds, NodeMemberStatuses? status, bool? admin)
        {
            string spName = GetFullyQualifiedName("GetMemberUserIDs");

            try
            {
                string strStatus = status.ToString();
                if (string.IsNullOrEmpty(strStatus)) strStatus = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', strStatus, admin);
                ProviderUtil.parse_guids(ref reader, ref retUserIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static int GetMembersCount(Guid applicationId, Guid nodeId, NodeMemberStatuses? status, bool? admin)
        {
            string spName = GetFullyQualifiedName("GetMembersCount");

            try
            {
                string strStatus = status.ToString();
                if (string.IsNullOrEmpty(strStatus)) strStatus = null;

                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, 
                    nodeId, strStatus, admin));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return 0;
            }
        }

        public static void GetMembershipRequestsDashboards(Guid applicationId, 
            ref List<NodeMember> retNodeMembers, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetMembershipRequestsDashboards");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_node_members(ref reader, ref retNodeMembers, true, true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static int GetMembershipRequestsDashboardsCount(Guid applicationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetMembershipRequestsDashboards");

            try
            {
                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return 0;
            }
        }

        public static void GetMembershipDomainsCount(Guid applicationId, ref List<NodesCount> retItems, Guid userId, 
            Guid? nodeTypeId, Guid? nodeId, string additionalId, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("GetMembershipDomainsCount");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (nodeId == Guid.Empty) nodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeTypeId,
                    nodeId, additionalId, lowerDateLimit, upperDateLimit);
                _parse_nodes_count(ref reader, ref retItems);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetMembershipDomains(Guid applicationId, ref List<Node> retItems, Guid userId, 
            List<Guid> nodeTypeIds, Guid? nodeId, string additionalId, string searchText,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetMembershipDomains");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, string.Join(",", nodeTypeIds), ',', 
                    nodeId, additionalId, ProviderUtil.get_search_text(searchText),
                    lowerDateLimit, upperDateLimit, lowerBoundary, count);
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetMemberNodes(Guid applicationId, ref List<NodeMember> retNodeMembers, List<Guid> userIds,
            ref List<Guid> nodeTypeIds, NodeTypes? nodeType, bool? admin)
        {
            string spName = GetFullyQualifiedName("GetMemberNodes");

            try
            {
                string additionalTypeId = null;
                if (nodeType != null) additionalTypeId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(userIds), 
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ',', additionalTypeId, admin);
                _parse_node_members(ref reader, ref retNodeMembers, true, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetChildHierarchyMemberIDs(Guid applicationId, ref List<Guid> retIds, 
            Guid nodeId, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetChildHierarchyMemberIDs");

            try
            {
                ProviderUtil.parse_guids(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, ProviderUtil.get_search_text(searchText), count, lowerBoundary), ref retIds, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetChildHierarchyExpertIDs(Guid applicationId, ref List<Guid> retIds,
            Guid nodeId, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetChildHierarchyExpertIDs");

            try
            {
                ProviderUtil.parse_guids(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, ProviderUtil.get_search_text(searchText), count, lowerBoundary), ref retIds, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetUsersDepartments(Guid applicationId, 
            ref List<NodeMember> retNodeMembers, ref List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("GetUsersDepartments");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref userIds), ',');
                _parse_node_members(ref reader, ref retNodeMembers, true, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool LikeNodes(Guid applicationId, ref List<Guid> nodeIds, Guid userId)
        {
            string spName = GetFullyQualifiedName("LikeNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', userId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool UnlikeNodes(Guid applicationId, ref List<Guid> nodeIds, Guid userId)
        {
            string spName = GetFullyQualifiedName("UnlikeNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void IsFan(Guid applicationId, ref List<Guid> retIds, ref List<Guid> nodeIds, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsFan");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', userId);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeFansUserIDs(Guid applicationId, 
            ref List<Guid> retIds, Guid nodeId, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetNodeFansUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, count, lowerBoundary);
                totalCount = _parse_fan_user_ids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetFavoriteNodesCount(Guid applicationId, ref List<NodesCount> retItems, 
            Guid userId, Guid? nodeTypeId, Guid? nodeId, string additionalId, bool? isDocument, 
            DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("GetFavoriteNodesCount");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (nodeId == Guid.Empty) nodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeTypeId,
                    nodeId, additionalId, isDocument, lowerDateLimit, upperDateLimit);
                _parse_nodes_count(ref reader, ref retItems);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetFavoriteNodes(Guid applicationId, ref List<Node> retItems, Guid userId,
            List<Guid> nodeTypeIds, bool? useNodeTypeHierarchy, Guid? nodeId, string additionalId, string searchText, 
            bool? isDocument, Guid? creatorUserId, Guid? relatedToNodeId, List<FormFilter> filters, bool? matchAllFilters,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            if (nodeId == Guid.Empty) nodeId = null;
            if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
            if (count.HasValue && count <= 0) count = null;

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Filters
            DataTable filtersTable = new DataTable();
            filtersTable.Columns.Add("ElementID", typeof(Guid));
            filtersTable.Columns.Add("OwnerID", typeof(Guid));
            filtersTable.Columns.Add("Text", typeof(string));
            filtersTable.Columns.Add("TextItems", typeof(string));
            filtersTable.Columns.Add("Or", typeof(bool));
            filtersTable.Columns.Add("Exact", typeof(bool));
            filtersTable.Columns.Add("DateFrom", typeof(DateTime));
            filtersTable.Columns.Add("DateTo", typeof(DateTime));
            filtersTable.Columns.Add("FloatFrom", typeof(double));
            filtersTable.Columns.Add("FloatTo", typeof(double));
            filtersTable.Columns.Add("Bit", typeof(bool));
            filtersTable.Columns.Add("Guid", typeof(Guid));
            filtersTable.Columns.Add("GuidItems", typeof(string));
            filtersTable.Columns.Add("Compulsory", typeof(bool));

            if (filters != null)
            {
                foreach (FormFilter f in filters)
                {
                    filtersTable.Rows.Add(f.ElementID, f.OwnerID, f.Text, ProviderUtil.list_to_string<string>(f.TextItems),
                        f.Or, f.Exact, f.DateFrom, f.DateTo, f.FloatFrom, f.FloatTo, f.Bit, f.Guid,
                        ProviderUtil.list_to_string<Guid>(f.GuidItems), f.Compulsory);
                }
            }

            SqlParameter filtersParam = new SqlParameter("@FormFilters", SqlDbType.Structured);
            filtersParam.TypeName = "[dbo].[FormFilterTableType]";
            filtersParam.Value = filtersTable;
            //end of Add Filters

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            if (nodeTypeIds != null && nodeTypeIds.Count > 0)
                cmd.Parameters.AddWithValue("@strNodeTypeIDs", ProviderUtil.list_to_string<Guid>(nodeTypeIds));
            cmd.Parameters.AddWithValue("@delimiter", ',');
            if (useNodeTypeHierarchy.HasValue)
                cmd.Parameters.AddWithValue("@UseNodeTypeHierarchy", useNodeTypeHierarchy.Value);
            if (nodeId.HasValue) cmd.Parameters.AddWithValue("@NodeID", nodeId.Value);
            if (!string.IsNullOrEmpty(additionalId))
                cmd.Parameters.AddWithValue("@AdditionalID", additionalId);
            if (!string.IsNullOrEmpty(searchText))
                cmd.Parameters.AddWithValue("@SearchText", ProviderUtil.get_search_text(searchText));
            if (isDocument.HasValue) cmd.Parameters.AddWithValue("@IsDocument", isDocument.Value);
            if (creatorUserId.HasValue) cmd.Parameters.AddWithValue("@CreatorUserID", creatorUserId);
            if (relatedToNodeId.HasValue)
                cmd.Parameters.AddWithValue("@RelatedToNodeID", relatedToNodeId.Value);
            cmd.Parameters.Add(filtersParam);
            if (matchAllFilters.HasValue) cmd.Parameters.AddWithValue("@MatchAllFilters", matchAllFilters.Value);
            if (lowerDateLimit.HasValue)
                cmd.Parameters.AddWithValue("@LowerDateLimit", lowerDateLimit.Value);
            if (upperDateLimit.HasValue)
                cmd.Parameters.AddWithValue("@UpperDateLimit", upperDateLimit.Value);
            if (lowerBoundary.HasValue) cmd.Parameters.AddWithValue("@LowerBoundary", lowerBoundary.Value);
            cmd.Parameters.AddWithValue("@Count", count);

            string spName = GetFullyQualifiedName("GetFavoriteNodes");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep +
                "@UserID" + sep +
                (nodeTypeIds != null && nodeTypeIds.Count > 0 ? "@strNodeTypeIDs" : "null") + sep +
                "@delimiter" + sep +
                (useNodeTypeHierarchy.HasValue ? "@UseNodeTypeHierarchy" : "null") + sep +
                (nodeId.HasValue ? "@NodeID" : "null") + sep +
                (!string.IsNullOrEmpty(additionalId) ? "@AdditionalID" : "null") + sep +
                (!string.IsNullOrEmpty(searchText) ? "@SearchText" : "null") + sep +
                (isDocument.HasValue ? "@IsDocument" : "null") + sep +
                (creatorUserId.HasValue ? "@CreatorUserID" : "null") + sep +
                (relatedToNodeId.HasValue ? "@RelatedToNodeID" : "null") + sep +
                "@FormFilters" + sep +
                (matchAllFilters.HasValue ? "@MatchAllFilters" : "null") + sep +
                (lowerDateLimit.HasValue ? "@LowerDateLimit" : "null") + sep +
                (upperDateLimit.HasValue ? "@UpperDateLimit" : "null") + sep +
                (lowerBoundary.HasValue ? "@LowerBoundary" : "null") + sep +
                "@Count";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
            finally { con.Close(); }
        }

        public static bool AddComplex(Guid applicationId, NodeList Info)
        {
            string spName = GetFullyQualifiedName("AddComplex");

            try
            {
                if (Info.ParentListID == Guid.Empty) Info.ParentListID = null;
                if (Info.OwnerID == Guid.Empty) Info.OwnerID = null;
                if (!Info.CreationDate.HasValue) Info.CreationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ListID, Info.NodeTypeID, Info.Name, Info.Description, Info.CreatorUserID, Info.CreationDate,
                    Info.ParentListID, Info.OwnerID, Info.OwnerType));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ModifyComplex(Guid applicationId, NodeList Info)
        {
            string spName = GetFullyQualifiedName("ModifyComplex");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ListID, Info.Name, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteComplexes(Guid applicationId, 
            ref List<Guid> listIds, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteComplexes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref listIds), ',', lastModifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddComplexAdmin(Guid applicationId, Guid listId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddComplexAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    listId, userId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool RemoveComplexAdmin(Guid applicationId, Guid listId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveComplexAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    listId, userId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetLists(Guid applicationId, ref List<NodeList> retLists, ref List<Guid> listIds)
        {
            string spName = GetFullyQualifiedName("GetListsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref listIds), ',');
                _parse_lists(ref reader, ref retLists);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetLists(Guid applicationId, ref List<NodeList> retLists, Guid? nodeTypeId, 
            NodeTypes? nodeType, string searchText, int? count, Guid? minId)
        {
            string spName = GetFullyQualifiedName("GetLists");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                string strNodeTypeAdditionalId = null;
                if (nodeType.HasValue)
                    strNodeTypeAdditionalId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();
                if (minId == Guid.Empty) minId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, strNodeTypeAdditionalId, ProviderUtil.get_search_text(searchText), count, minId);
                _parse_lists(ref reader, ref retLists);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool AddNodesToComplex(Guid applicationId, 
            Guid listId, ref List<Guid> nodeIds, Guid creatorUserId)
        {
            string spName = GetFullyQualifiedName("AddNodesToComplex");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    listId, ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', creatorUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteComplexNodes(Guid applicationId, 
            Guid listId, ref List<Guid> nodeIds, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteComplexNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    listId, ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', lastModifierUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetListNodes(Guid applicationId, 
            ref List<Node> retNodes, Guid listId, Guid? nodeTypeId, NodeTypes? nodeType)
        {
            string spName = GetFullyQualifiedName("GetListNodes");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;

                string strNodeTypeAdditionalId = null;
                if (nodeType.HasValue)
                    strNodeTypeAdditionalId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    listId, nodeTypeId, strNodeTypeAdditionalId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }
        
        public static Guid? AddTags(Guid applicationId, List<Tag> tags, Guid? currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Tags
            DataTable tagsTable = new DataTable();
            tagsTable.Columns.Add("Value", typeof(string));

            foreach (Tag tg in tags)
                tagsTable.Rows.Add(tg.Text);

            SqlParameter tagsParam = new SqlParameter("@Tags", SqlDbType.Structured);
            tagsParam.TypeName = "[dbo].[StringTableType]";
            tagsParam.Value = tagsTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(tagsParam);
            cmd.Parameters.AddWithValue("@CreatorUserID", currentUserId);
            cmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);

            string spName = GetFullyQualifiedName("AddTags");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Tags" + sep + "@CreatorUserID" + sep + "@CreationDate";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed_guid((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return null;
            }
            finally { con.Close(); }
        }

        public static void SearchTags(Guid applicationId, ref List<Tag> retList, 
            string searchText, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("SearchTags");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.get_search_text(searchText), count, lowerBoundary);
                _parse_tags(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeCreators(Guid applicationId, ref List<NodeCreator> retList, Guid nodeId, bool? full)
        {
            string spName = GetFullyQualifiedName("GetNodeCreators");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, full);
                _parse_node_creators(ref reader, ref retList, full);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetCreatorNodes(Guid applicationId, ref List<Node> retNodes, Guid userId, Guid? nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetCreatorNodes");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeTypeId);
                _parse_nodes(ref reader, ref retNodes, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool AddExperts(Guid applicationId, Guid nodeId, ref List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("AddExperts");

            try
            {
                if (userIds == null || userIds.Count == 0) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, ProviderUtil.list_to_string<Guid>(ref userIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteExperts(Guid applicationId, Guid nodeId, ref List<Guid> userIds)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteExperts");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, ProviderUtil.list_to_string<Guid>(ref userIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetExperts(Guid applicationId, ref List<Expert> retExperts, List<Guid> nodeIds, 
            string searchText, bool hierarchy, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetExperts");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', 
                    ProviderUtil.get_search_text(searchText), hierarchy, count, lowerBoundary);
                totalCount = _parse_experts(ref reader, ref retExperts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseDomainsCount(Guid applicationId, ref List<NodesCount> retItems, Guid userId, 
            Guid? nodeTypeId, Guid? nodeId, string additionalId, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("GetExpertiseDomainsCount");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (nodeId == Guid.Empty) nodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeTypeId,
                    nodeId, additionalId, lowerDateLimit, upperDateLimit);
                _parse_nodes_count(ref reader, ref retItems);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseDomains(Guid applicationId, ref List<Node> retItems, Guid userId,
            List<Guid> nodeTypeIds, Guid? nodeId, string additionalId, string searchText,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetExpertiseDomains");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, string.Join(",", nodeTypeIds), ',',
                    nodeId, additionalId, ProviderUtil.get_search_text(searchText),
                    lowerDateLimit, upperDateLimit, lowerBoundary, count);
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseDomains(Guid applicationId, ref List<Expert> retExperts, 
            ref List<Guid> userIds, Guid? nodeTypeId, bool? approved, bool? socialApproved, bool? all)
        {
            string spName = GetFullyQualifiedName("GetUsersExpertiseDomains");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref userIds), ',', nodeTypeId, approved, socialApproved, all);
                _parse_experts(ref reader, ref retExperts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseDomainIDs(Guid applicationId, 
            ref List<Guid> retIds, ref List<Guid> userIds, bool? approved, bool? socialApproved)
        {
            string spName = GetFullyQualifiedName("GetUsersExpertiseDomainIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref userIds), ',', approved, socialApproved);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool IsExpert(Guid applicationId, 
            Guid userId, Guid nodeId, bool? approved, bool? socialApproved)
        {
            string spName = GetFullyQualifiedName("IsExpert");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, nodeId, approved, socialApproved));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static int GetExpertsCount(Guid applicationId, 
            Guid? nodeId, bool? distinctUsers, bool? approved, bool? socialApproved)
        {
            string spName = GetFullyQualifiedName("GetExpertsCount");

            try
            {
                return (int)ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, distinctUsers, approved, socialApproved));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return 0;
            }
        }

        public static bool VoteExpertise(Guid applicationId, 
            Guid referrerUserId, Guid nodeId, Guid userId, bool? confirmStatus)
        {
            string spName = GetFullyQualifiedName("VoteExpertise");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    referrerUserId, nodeId, userId, confirmStatus, DateTime.Now,
                    RaaiVanSettings.CoreNetwork.MinAcceptableExpertiseReferralsCount(applicationId),
                    RaaiVanSettings.CoreNetwork.MinAcceptableExpertiseConfirmsPercentage(applicationId)));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static Guid? IAmExpert(Guid applicationId, Guid userId, string expertiseDomain)
        {
            string spName = GetFullyQualifiedName("IAmExpert");

            try
            {
                Guid? result = ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId,
                    userId, expertiseDomain, DateTime.Now, 
                    RaaiVanSettings.CoreNetwork.MinAcceptableExpertiseReferralsCount(applicationId),
                    RaaiVanSettings.CoreNetwork.MinAcceptableExpertiseConfirmsPercentage(applicationId)));
                if (result == Guid.Empty) result = null;
                return result;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return null;
            }
        }

        public static bool IAmNotExpert(Guid applicationId, Guid userId, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("IAmNotExpert");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, nodeId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static int GetReferralsCount(Guid applicationId, Guid userId, Guid nodeId)
        {
            string spName = GetFullyQualifiedName("GetReferralsCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, userId, nodeId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return 0;
            }
        }

        public static void GetExpertiseSuggestions(Guid applicationId, 
            ref List<Expert> retExperts, Guid userId, int count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetExpertiseSuggestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, count, lowerBoundary);
                _parse_expertise_suggestions(ref reader, ref retExperts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestNodeRelations(Guid applicationId, 
            ref List<Node> retNodes, Guid userId, Guid? relatedNodeTypeId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestNodeRelations");

            try
            {
                if (relatedNodeTypeId == Guid.Empty) relatedNodeTypeId = null;
                if (!count.HasValue) count = 20;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, null, relatedNodeTypeId, count, DateTime.Now);
                _parse_nodes(ref reader, ref retNodes, null);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestNodeTypesForRelations(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid userId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestNodeTypesForRelations");

            try
            {
                if (!count.HasValue) count = 10;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, null, count, DateTime.Now);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestSimilarNodes(Guid applicationId,
            ref List<SimilarNode> ret, Guid nodeId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestSimilarNodes");

            try
            {
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, count);
                _parse_similar_nodes(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestKnowledgableUsers(Guid applicationId,
            ref List<KnowledgableUser> ret, Guid nodeId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestKnowledgableUsers");

            try
            {
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, count);
                _parse_knowledgable_users(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExistingNodeIDs(Guid applicationId, 
            ref List<Guid> retIds, ref List<Guid> nodeIds, bool? searchable, bool? noContent)
        {
            string spName = GetFullyQualifiedName("GetExistingNodeIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string(ref nodeIds), ',', searchable, noContent);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExistingNodeTypeIDs(Guid applicationId,
            ref List<Guid> retIds, ref List<Guid> nodeTypeIds, bool? noContent)
        {
            string spName = GetFullyQualifiedName("GetExistingNodeTypeIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string(ref nodeTypeIds), ',', noContent);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeInfo(Guid applicationId, ref List<NodeInfo> retList, List<Guid> nodeIds, 
            Guid? currentUserId, bool? tags, bool? description, bool? creator, bool? contributorsCount, bool? likesCount, 
            bool? visitsCount, bool? expertsCount, bool? membersCount, bool? childsCount, 
            bool? relatedNodesCount, bool? likeStatus)
        {
            string spName = GetFullyQualifiedName("GetNodeInfo");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;
                if (!currentUserId.HasValue) likeStatus = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeIds), ',', currentUserId, tags, description, 
                    creator, contributorsCount, likesCount, visitsCount, expertsCount, membersCount, 
                    childsCount, relatedNodesCount, likeStatus);
                _parse_node_info(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool InitializeExtensions(Guid applicationId, Guid ownerId, Guid currentUserId, bool ignoreDefault)
        {
            string spName = GetFullyQualifiedName("InitializeExtensions");

            try
            {
                List<Extension> lst = CNUtilities.extend_extensions(applicationId, new List<Extension>(), ignoreDefault);

                List<ExtensionType> enabledExtensions = lst.Where(u => !u.Disabled.HasValue || u.Disabled == false).Select(
                    u => u.ExtensionType).ToList();

                List<ExtensionType> disabledExtensions = lst.Where(u => u.Disabled.HasValue && u.Disabled == true).Select(
                    u => u.ExtensionType).ToList();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, ProviderUtil.list_to_string<ExtensionType>(ref enabledExtensions),
                    ProviderUtil.list_to_string<ExtensionType>(ref disabledExtensions), ',', currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EnableDisableExtension(Guid applicationId, 
            Guid ownerId, ExtensionType extensionType, bool disable, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EnableDisableExtension");

            try
            {
                if (ownerId == Guid.Empty || extensionType == ExtensionType.NotSet) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, extensionType.ToString(), disable, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetExtensionTitle(Guid applicationId, 
            Guid ownerId, ExtensionType extensionType, string title, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetExtensionTitle");

            try
            {
                if (ownerId == Guid.Empty || extensionType == ExtensionType.NotSet) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, extensionType.ToString(), title, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool MoveExtension(Guid applicationId, Guid ownerId, ExtensionType extensionType, bool moveDown)
        {
            string spName = GetFullyQualifiedName("MoveExtension");

            try
            {
                if (ownerId == Guid.Empty || extensionType == ExtensionType.NotSet) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, extensionType.ToString(), moveDown));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SaveExtensions(Guid applicationId, Guid ownerId, List<Extension> extensions, Guid currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Extensions
            DataTable extensionsTable = new DataTable();
            extensionsTable.Columns.Add("OwnerID", typeof(Guid));
            extensionsTable.Columns.Add("Extension", typeof(string));
            extensionsTable.Columns.Add("Title", typeof(string));
            extensionsTable.Columns.Add("SequenceNumber", typeof(int));
            extensionsTable.Columns.Add("Disabled", typeof(bool));

            int seq = 1;

            extensions.ForEach(ex => extensionsTable.Rows.Add(null, ex.ExtensionType.ToString(), ex.Title, seq++, ex.Disabled));

            SqlParameter extensionsParam = new SqlParameter("@Extensions", SqlDbType.Structured);
            extensionsParam.TypeName = "[dbo].[CNExtensionTableType]";
            extensionsParam.Value = extensionsTable;
            //end of Add Extensions

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@OwnerID", ownerId);
            cmd.Parameters.Add(extensionsParam);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("SaveExtensions");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@OwnerID" + sep + 
                "@Extensions" + sep + "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
            finally { con.Close(); }
        }

        public static void GetExtensions(Guid applicationId, ref List<Extension> retExtensions, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetExtensions");

            try
            {
                if (ownerId == Guid.Empty) return;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                _parse_extensions(ref reader, ref retExtensions);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool HasExtension(Guid applicationId, Guid ownerId, ExtensionType extensionType)
        {
            string spName = GetFullyQualifiedName("HasExtension");
            
            try
            {
                if (ownerId == Guid.Empty || extensionType == ExtensionType.NotSet) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, extensionType.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetNodeTypesWithExtension(Guid applicationId, 
            ref List<NodeType> retItems, List< ExtensionType> exts)
        {
            string spName = GetFullyQualifiedName("GetNodeTypesWithExtension");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<ExtensionType>(exts), ',');
                _parse_node_types(ref reader, ref retItems);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetIntellectualPropertiesCount(Guid applicationId, ref List<NodesCount> retItems, 
            Guid userId, Guid? nodeTypeId, Guid? nodeId , string additionalId, Guid? currentUserId, bool? isDocument,
            DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("GetIntellectualPropertiesCount");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (nodeId == Guid.Empty) nodeId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeTypeId,
                    nodeId, additionalId, currentUserId, isDocument, lowerDateLimit, upperDateLimit);
                _parse_nodes_count(ref reader, ref retItems);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetIntellectualProperties(Guid applicationId, ref List<Node> retItems, Guid userId,
            List<Guid> nodeTypeIds, Guid? nodeId, string additionalId, Guid? currentUserId, string searchText, bool? isDocument,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetIntellectualProperties");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, string.Join(",", nodeTypeIds), ',', 
                    nodeId, additionalId, currentUserId, ProviderUtil.get_search_text(searchText),
                    isDocument, lowerDateLimit, upperDateLimit, lowerBoundary, count);
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetIntellectualPropertiesOfFriends(Guid applicationId, ref List<Node> retItems, 
            Guid userId, Guid? nodeTypeId, int? lowerBoundary, int? count)
        {
            string spName = GetFullyQualifiedName("GetIntellectualPropertiesOfFriends");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, nodeTypeId, lowerBoundary, count);
                _parse_nodes(ref reader, ref retItems, null);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetDocumentTreeNodeItems(Guid applicationId, ref List<Node> retItems, 
            Guid documentTreeNodeId, Guid? currenrUserId, bool? checkPrivacy, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetDocumentTreeNodeItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, documentTreeNodeId, 
                    currenrUserId, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId), count, lowerBoundary);
                _parse_nodes(ref reader, ref retItems, null);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetDocumentTreeNodeContents(Guid applicationId, ref List<Node> retItems, 
            Guid documentTreeNodeId, Guid? currenrUserId, bool? checkPrivacy, int? count, int? lowerBoundary, 
            string searchText, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetDocumentTreeNodeContents");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, documentTreeNodeId,
                    currenrUserId, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId), 
                    count, lowerBoundary, ProviderUtil.get_search_text(searchText));
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static List<Guid> IsNodeType(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsNodeType");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static List<Guid> IsNode(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsNode");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static void Explore(Guid applicationId, ref List<ExploreItem> retItems, Guid? baseId, Guid? relatedId, 
            List<Guid> baseTypeIds, List<Guid> relatedTypeIds, Guid? secondLevelNodeId, 
            bool? registrationArea, bool? tags, bool? relations, int? lowerBoundary, int? count, string orderBy, 
            bool? orderByDesc, string searchText, bool? checkAccess, Guid? currentUserId, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("Explore");

            try
            {
                if (baseId == Guid.Empty) baseId = null;
                if (relatedId == Guid.Empty) relatedId = null;
                if (secondLevelNodeId == Guid.Empty) secondLevelNodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, baseId, relatedId,
                    ProviderUtil.list_to_string<Guid>(baseTypeIds), ProviderUtil.list_to_string<Guid>(relatedTypeIds), ',',
                    secondLevelNodeId, registrationArea, tags, relations, lowerBoundary, count, orderBy, orderByDesc,
                    ProviderUtil.get_search_text(searchText), checkAccess, currentUserId,
                    DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                totalCount = _parse_explore_items(ref reader, ref retItems);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool UpdateFormAndWikiTags(Guid applicationId, List<Guid> nodeIds, Guid? creatorUserId, int? count)
        {
            string spName = GetFullyQualifiedName("UpdateFormAndWikiTags");

            try
            {
                if (creatorUserId == Guid.Empty) creatorUserId = null;
                bool form = RaaiVanConfig.Modules.FormGenerator(applicationId);
                bool wiki = true;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', creatorUserId, count, form, wiki));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        /* Service */

        public static bool InitializeService(Guid applicationId, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("InitializeService");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetServices(Guid applicationId, ref List<Service> retServices, List<Guid> nodeTypeIds)
        {
            string spName = GetFullyQualifiedName("GetServicesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ',');
                _parse_services(ref reader, ref retServices);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetServices(Guid applicationId, ref List<Service> retServices, Guid? nodeTypeIdOrNodeId,
            Guid? currentUserId, bool? isDocument, bool? isKnowledge, bool? checkPrivacy)
        {
            string spName = GetFullyQualifiedName("GetServices");

            try
            {
                if (nodeTypeIdOrNodeId == Guid.Empty) nodeTypeIdOrNodeId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrNodeId, 
                    currentUserId, isDocument, isKnowledge, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                _parse_services(ref reader, ref retServices);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool SetServiceTitle(Guid applicationId, Guid nodeTypeId, string title)
        {
            string spName = GetFullyQualifiedName("SetServiceTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, title));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetServiceDescription(Guid applicationId, Guid nodeTypeId, string description)
        {
            string spName = GetFullyQualifiedName("SetServiceDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, description));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetServiceSuccessMessage(Guid applicationId, Guid nodeTypeId, string message)
        {
            string spName = GetFullyQualifiedName("SetServiceSuccessMessage");

            try
            {
                if (string.IsNullOrEmpty(message)) message = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, message));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static string GetServiceSuccessMessage(Guid applicationId, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetServiceSuccessMessage");

            try
            {
                return ProviderUtil.succeed_string(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return string.Empty;
            }
        }

        public static bool SetServiceAdminType(Guid applicationId, Guid nodeTypeId, ServiceAdminType adminType,
            Guid? adminNodeId, ref List<Guid> limitNodeTypeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetServiceAdminType");

            try
            {
                if (adminNodeId == Guid.Empty) adminNodeId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    nodeTypeId, adminType.ToString(), adminNodeId, 
                    ProviderUtil.list_to_string<Guid>(ref limitNodeTypeIds), ',', currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetAdminAreaLimits(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid nodeTypeIdOrnodeId)
        {
            string spName = GetFullyQualifiedName("GetAdminAreaLimits");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrnodeId);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool SetMaxAcceptableAdminLevel(Guid applicationId, Guid nodeTypeId, int? maxAcceptableAdminLevel)
        {
            string spName = GetFullyQualifiedName("SetMaxAcceptableAdminLevel");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, maxAcceptableAdminLevel));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetContributionLimits(Guid applicationId, 
            Guid nodeTypeId, ref List<Guid> limitNodeTypeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetContributionLimits");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, ProviderUtil.list_to_string<Guid>(ref limitNodeTypeIds), ',', currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetContributionLimits(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid nodeTypeIdOrnodeId)
        {
            string spName = GetFullyQualifiedName("GetContributionLimits");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrnodeId);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool EnableContribution(Guid applicationId, Guid nodeTypeId, bool enable)
        {
            string spName = GetFullyQualifiedName("EnableContribution");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, enable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool NoContentService(Guid applicationId, Guid nodeTypeIdOrNodeId, bool? value)
        {
            string spName = GetFullyQualifiedName("NoContentService");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeIdOrNodeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool IsKnowledge(Guid applicationId, Guid nodeTypeIdOrNodeId, bool? isKnowledge)
        {
            string spName = GetFullyQualifiedName("IsKnowledge");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeIdOrNodeId, isKnowledge));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool IsDocument(Guid applicationId, Guid nodeTypeIdOrNodeId, bool? isDocument)
        {
            string spName = GetFullyQualifiedName("IsDocument");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeIdOrNodeId, isDocument));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EnablePreviousVersionSelect(Guid applicationId, Guid nodeTypeIdOrNodeId, bool? value)
        {
            string spName = GetFullyQualifiedName("EnablePreviousVersionSelect");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrNodeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool IsTree(Guid applicationId, 
            ref List<Guid> treeIds, List<Guid> nodeTypeOrNodeIds, bool? isTree)
        {
            string spName = GetFullyQualifiedName("IsTree");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeOrNodeIds), ',', isTree);

                if (isTree.HasValue)
                    return ProviderUtil.succeed(reader);
                else
                {
                    ProviderUtil.parse_guids(ref reader, ref treeIds);
                    return true;
                }
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool HasUniqueMembership(Guid applicationId,
            ref List<Guid> groupIds, List<Guid> nodeTypeOrNodeIds, bool? value)
        {
            string spName = GetFullyQualifiedName("HasUniqueMembership");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeOrNodeIds), ',', value);

                if (value.HasValue)
                    return ProviderUtil.succeed(reader);
                else
                {
                    ProviderUtil.parse_guids(ref reader, ref groupIds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool HasUniqueAdminMember(Guid applicationId,
            ref List<Guid> groupIds, List<Guid> nodeTypeOrNodeIds, bool? value)
        {
            string spName = GetFullyQualifiedName("HasUniqueAdminMember");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeOrNodeIds), ',', value);

                if (value.HasValue)
                    return ProviderUtil.succeed(reader);
                else
                {
                    ProviderUtil.parse_guids(ref reader, ref groupIds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AbstractAndKeywordsDisabled(Guid applicationId,
            ref List<Guid> groupIds, List<Guid> nodeTypeOrNodeIds, bool? value)
        {
            string spName = GetFullyQualifiedName("AbstractAndKeywordsDisabled");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeOrNodeIds), ',', value);

                if (value.HasValue)
                    return ProviderUtil.succeed(reader);
                else
                {
                    ProviderUtil.parse_guids(ref reader, ref groupIds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool FileUploadDisabled(Guid applicationId,
            ref List<Guid> groupIds, List<Guid> nodeTypeOrNodeIds, bool? value)
        {
            string spName = GetFullyQualifiedName("FileUploadDisabled");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeOrNodeIds), ',', value);

                if (value.HasValue)
                    return ProviderUtil.succeed(reader);
                else
                {
                    ProviderUtil.parse_guids(ref reader, ref groupIds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool RelatedNodesSelectDisabled(Guid applicationId,
            ref List<Guid> groupIds, List<Guid> nodeTypeOrNodeIds, bool? value)
        {
            string spName = GetFullyQualifiedName("RelatedNodesSelectDisabled");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeOrNodeIds), ',', value);

                if (value.HasValue)
                    return ProviderUtil.succeed(reader);
                else
                {
                    ProviderUtil.parse_guids(ref reader, ref groupIds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EditableForAdmin(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            string spName = GetFullyQualifiedName("EditableForAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, editable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EditableForCreator(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            string spName = GetFullyQualifiedName("EditableForCreator");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, editable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EditableForOwners(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            string spName = GetFullyQualifiedName("EditableForOwners");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, editable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EditableForExperts(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            string spName = GetFullyQualifiedName("EditableForExperts");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, editable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EditableForMembers(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            string spName = GetFullyQualifiedName("EditableForMembers");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, editable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool EditSuggestion(Guid applicationId, Guid nodeTypeId, bool enable)
        {
            string spName = GetFullyQualifiedName("EditSuggestion");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, enable));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddFreeUser(Guid applicationId, Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddFreeUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, userId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteFreeUser(Guid applicationId, 
            Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteFreeUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, userId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetFreeUsers(Guid applicationId, ref List<User> retUsers, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetFreeUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeId);
                List<Guid> userIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref userIds);
                retUsers = UsersController.get_users(applicationId, userIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool IsFreeUser(Guid applicationId, Guid nodeTypeIdOrNodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsFreeUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeIdOrNodeId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool AddServiceAdmin(Guid applicationId, Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddServiceAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, userId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool ArithmeticDeleteServiceAdmin(Guid applicationId, 
            Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteServiceAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, userId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static void GetServiceAdmins(Guid applicationId, ref List<User> retUsers, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetServiceAdminIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeId);
                List<Guid> userIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref userIds);
                retUsers = UsersController.get_users(applicationId, userIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static List<Guid> IsServiceAdmin(Guid applicationId, List<Guid> nodeTypeIdOrNodeIds, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsServiceAdmin");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeTypeIdOrNodeIds), ',', userId);

                List<Guid> lst = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref lst);
                return lst;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static bool RegisterNewNode(Guid applicationId, Node nodeObject, Guid? workflowId, Guid? formInstanceId,
            Guid? wfDirectorNodeId, Guid? wfDirectorUserId, ref List<Dashboard> dashboards, ref string message)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            if (nodeObject.ParentNodeID == Guid.Empty) nodeObject.ParentNodeID = null;
            if (nodeObject.DocumentTreeNodeID == Guid.Empty) nodeObject.DocumentTreeNodeID = null;
            if (nodeObject.PreviousVersionID == Guid.Empty) nodeObject.PreviousVersionID = null;

            //Add CreatorUsers
            DataTable contributorsTable = new DataTable();
            contributorsTable.Columns.Add("FirstValue", typeof(Guid));
            contributorsTable.Columns.Add("SecondValue", typeof(double));

            foreach (NodeCreator _cnt in nodeObject.Contributors)
                contributorsTable.Rows.Add(_cnt.User.UserID, _cnt.CollaborationShare);

            SqlParameter contributorsParam = new SqlParameter("@Contributors", SqlDbType.Structured);
            contributorsParam.TypeName = "[dbo].[GuidFloatTableType]";
            contributorsParam.Value = contributorsTable;
            //end of Add CreatorUsers

            if (!nodeObject.CreationDate.HasValue) nodeObject.CreationDate = DateTime.Now;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@NodeID", nodeObject.NodeID);
            cmd.Parameters.AddWithValue("@NodeTypeID", nodeObject.NodeTypeID);
            if (!string.IsNullOrEmpty(nodeObject.AdditionalID_Main))
                cmd.Parameters.AddWithValue("@AdditionalID_Main", nodeObject.AdditionalID_Main);
            if (!string.IsNullOrEmpty(nodeObject.AdditionalID))
                cmd.Parameters.AddWithValue("@AdditionalID", nodeObject.AdditionalID);
            if (nodeObject.ParentNodeID.HasValue)
                cmd.Parameters.AddWithValue("@ParentNodeID", nodeObject.ParentNodeID.Value);
            if (nodeObject.DocumentTreeNodeID.HasValue)
                cmd.Parameters.AddWithValue("@DocumentTreeNodeID", nodeObject.DocumentTreeNodeID.Value);
            if (nodeObject.PreviousVersionID.HasValue)
                cmd.Parameters.AddWithValue("@PreviousVersionID", nodeObject.PreviousVersionID.Value);
            cmd.Parameters.AddWithValue("@Name", nodeObject.Name);
            if(!string.IsNullOrEmpty(nodeObject.Description))
                cmd.Parameters.AddWithValue("@Description", nodeObject.Description);
            if (nodeObject.Tags.Count > 0) cmd.Parameters.AddWithValue("@Tags", ProviderUtil.get_tags_text(nodeObject.Tags));
            cmd.Parameters.AddWithValue("@CreatorUserID", nodeObject.Creator.UserID);
            cmd.Parameters.AddWithValue("@CreationDate", nodeObject.CreationDate);
            cmd.Parameters.Add(contributorsParam);
            if (nodeObject.OwnerID.HasValue) cmd.Parameters.AddWithValue("@OwnerID", nodeObject.OwnerID);
            if (workflowId.HasValue && workflowId != Guid.Empty)
                cmd.Parameters.AddWithValue("@WorkFlowID", workflowId.Value);
            if (nodeObject.AdminAreaID.HasValue && nodeObject.AdminAreaID != Guid.Empty)
                cmd.Parameters.AddWithValue("@AdminAreaID", nodeObject.AdminAreaID.Value);
            if (formInstanceId.HasValue && formInstanceId != Guid.Empty)
                cmd.Parameters.AddWithValue("@FormInstanceID", formInstanceId.Value);
            if (wfDirectorNodeId.HasValue && wfDirectorNodeId != Guid.Empty)
                cmd.Parameters.AddWithValue("@WFDirectorNodeID", wfDirectorNodeId.Value);
            if (wfDirectorUserId.HasValue && wfDirectorUserId != Guid.Empty)
                cmd.Parameters.AddWithValue("@WFDirectorUserID", wfDirectorUserId.Value);

            string spName = GetFullyQualifiedName("RegisterNewNode");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@NodeID" + sep + "@NodeTypeID" + sep +
                (string.IsNullOrEmpty(nodeObject.AdditionalID_Main) ? "null" : "@AdditionalID_Main") + sep +
                (string.IsNullOrEmpty(nodeObject.AdditionalID) ? "null" : "@AdditionalID") + sep +
                (!nodeObject.ParentNodeID.HasValue ? "null" : "@ParentNodeID") + sep +
                (!nodeObject.DocumentTreeNodeID.HasValue ? "null" : "@DocumentTreeNodeID") + sep +
                (!nodeObject.PreviousVersionID.HasValue ? "null" : "@PreviousVersionID") + sep +
                "@Name" + sep + 
                (string.IsNullOrEmpty(nodeObject.Description) ? "null" : "@Description") + sep +
                (nodeObject.Tags.Count == 0 ? "null" : "@Tags") + sep +
                "@CreatorUserID" + sep + "@CreationDate" + sep + "@Contributors" + sep +
                (nodeObject.OwnerID.HasValue ? "@OwnerID" : "null") + sep +
                (workflowId.HasValue && workflowId != Guid.Empty ? "@WorkFlowID" : "null") + sep +
                (nodeObject.AdminAreaID.HasValue && nodeObject.AdminAreaID != Guid.Empty ? "@AdminAreaID" : "null") + sep +
                (formInstanceId.HasValue && formInstanceId != Guid.Empty ? "@FormInstanceID" : "null") + sep +
                (wfDirectorNodeId.HasValue && wfDirectorNodeId != Guid.Empty ? "@WFDirectorNodeID" : "null") + sep +
                (wfDirectorUserId.HasValue && wfDirectorUserId != Guid.Empty ? "@WFDirectorUserID" : "null");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref message) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool SetAdminArea(Guid applicationId, Guid nodeId, Guid? areaId)
        {
            string spName = GetFullyQualifiedName("SetAdminArea");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId, areaId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool SetContributors(Guid applicationId, Node nodeObject, ref string errorMessage)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add CreatorUsers
            DataTable contributorsTable = new DataTable();
            contributorsTable.Columns.Add("FirstValue", typeof(Guid));
            contributorsTable.Columns.Add("SecondValue", typeof(double));

            foreach (NodeCreator _cnt in nodeObject.Contributors)
                contributorsTable.Rows.Add(_cnt.User.UserID, _cnt.CollaborationShare);

            SqlParameter contributorsParam = new SqlParameter("@Contributors", SqlDbType.Structured);
            contributorsParam.TypeName = "[dbo].[GuidFloatTableType]";
            contributorsParam.Value = contributorsTable;
            //end of Add CreatorUsers

            if (!nodeObject.LastModificationDate.HasValue) nodeObject.LastModificationDate = DateTime.Now;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@NodeID", nodeObject.NodeID);
            cmd.Parameters.Add(contributorsParam);
            if (nodeObject.OwnerID.HasValue) cmd.Parameters.AddWithValue("@OwnerID", nodeObject.OwnerID);
            cmd.Parameters.AddWithValue("@LastModifierUserID", nodeObject.LastModifierUserID);
            cmd.Parameters.AddWithValue("@LastModificationDate", nodeObject.LastModificationDate);

            string spName = GetFullyQualifiedName("SetContributors");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@NodeID" + sep + "@Contributors" + sep +
                (nodeObject.OwnerID.HasValue ? "@OwnerID" : "null") + sep +
                "@LastModifierUserID" + sep + "@LastModificationDate";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader(), ref errorMessage); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
            finally { con.Close(); }
        }

        /* end of Service */
    }
}
