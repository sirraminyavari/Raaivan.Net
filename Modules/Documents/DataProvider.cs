using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Documents
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[DCT_" + name + "]"; //'[dbo].' is database owner and 'DCT_' is module qualifier
        }

        private static void _parse_trees(ref IDataReader reader, ref List<Tree> lstTrees)
        {
            while (reader.Read())
            {
                try
                {
                    Tree tree = new Tree();

                    if (!string.IsNullOrEmpty(reader["TreeID"].ToString())) tree.TreeID = (Guid)reader["TreeID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) tree.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) tree.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["IsTemplate"].ToString())) tree.IsTemplate = (bool)reader["IsTemplate"];

                    lstTrees.Add(tree);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_tree_nodes(ref IDataReader reader, ref List<TreeNode> lstTreeNodes)
        {
            while (reader.Read())
            {
                try
                {
                    TreeNode treeNode = new TreeNode();

                    if (!string.IsNullOrEmpty(reader["TreeNodeID"].ToString())) treeNode.TreeNodeID = (Guid)reader["TreeNodeID"];
                    if (!string.IsNullOrEmpty(reader["TreeID"].ToString())) treeNode.TreeID = (Guid)reader["TreeID"];
                    if (!string.IsNullOrEmpty(reader["ParentNodeID"].ToString())) treeNode.ParentNodeID = (Guid)reader["ParentNodeID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) treeNode.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["HasChild"].ToString())) treeNode.HasChild = (bool)reader["HasChild"];

                    lstTreeNodes.Add(treeNode);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_files(Guid applicationId, ref IDataReader reader, ref List<DocFileInfo> lstFiles)
        {
            while (reader.Read())
            {
                try
                {
                    DocFileInfo file = new DocFileInfo();
                    
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) file.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["FileID"].ToString())) file.FileID = (Guid)reader["FileID"];
                    if (!string.IsNullOrEmpty(reader["FileName"].ToString())) file.FileName = (string)reader["FileName"];
                    if (!string.IsNullOrEmpty(reader["Extension"].ToString())) file.Extension = (string)reader["Extension"];
                    if (!string.IsNullOrEmpty(reader["Size"].ToString())) file.Size = (long)reader["Size"];

                    FileOwnerTypes fot = FileOwnerTypes.None;
                    if (Enum.TryParse<FileOwnerTypes>(reader["OwnerType"].ToString(), out fot)) file.OwnerType = fot;

                    file.set_folder_name(applicationId, DocumentUtilities.get_folder_name(fot));

                    lstFiles.Add(file);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_file_owner_nodes(ref IDataReader reader, ref List<DocFileInfo> lstFiles)
        {
            while (reader.Read())
            {
                try
                {
                    DocFileInfo file = new DocFileInfo();

                    if (!string.IsNullOrEmpty(reader["FileID"].ToString())) file.FileID = (Guid)reader["FileID"];
                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) file.OwnerNodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) file.OwnerNodeName = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) file.OwnerNodeType = (string)reader["NodeType"];

                    lstFiles.Add(file);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static bool CreateTree(Guid applicationId, Tree Info)
        {
            string spName = GetFullyQualifiedName("CreateTree");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.TreeID, Info.IsPrivate, Info.OwnerID, Info.Name, Info.Description, 
                    Info.CreatorUserID, Info.CreationDate, Info.Privacy, Info.IsTemplate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool ChangeTree(Guid applicationId, Tree Info)
        {
            string spName = GetFullyQualifiedName("ChangeTree");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.TreeID, Info.Name, 
                    Info.Description, Info.LastModifierUserID, Info.LastModificationDate, Info.IsTemplate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool ArithmeticDeleteTree(Guid applicationId, 
            List<Guid> treeIds, Guid? ownerId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteTree");

            try
            {
                if (ownerId == Guid.Empty) ownerId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(treeIds), ',', ownerId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool RecycleTree(Guid applicationId, Guid treeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RecycleTree");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    treeId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static void GetTrees(Guid applicationId, ref List<Tree> retTrees, ref List<Guid> treeIds)
        {
            string spName = GetFullyQualifiedName("GetTreesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref treeIds), ',');
                _parse_trees(ref reader, ref retTrees);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static void GetTrees(Guid applicationId, ref List<Tree> retTrees, Guid? ownerId, bool? archive)
        {
            string spName = GetFullyQualifiedName("GetTrees");

            try
            {
                if (ownerId == Guid.Empty) ownerId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId, archive);
                _parse_trees(ref reader, ref retTrees);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static bool AddTreeNode(Guid applicationId, TreeNode Info)
        {
            string spName = GetFullyQualifiedName("AddTreeNode");

            try
            {
                if (Info.ParentNodeID == Guid.Empty) Info.ParentNodeID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.TreeNodeID, Info.TreeID, Info.ParentNodeID, Info.Name, Info.Description, Info.CreatorUserID,
                    Info.CreationDate, Info.Privacy));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool ChangeTreeNode(Guid applicationId, TreeNode Info)
        {
            string spName = GetFullyQualifiedName("ChangeTreeNode");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.TreeNodeID, Info.Name, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool CopyTreesOrTreeNodes(Guid applicationId, 
            Guid treeIdOrTreeNodeId, List<Guid> copiedIds, Guid currentUserId, ref List<Guid> createdIds)
        {
            string spName = GetFullyQualifiedName("CopyTreesOrTreeNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, treeIdOrTreeNodeId,
                    ProviderUtil.list_to_string<Guid>(ref copiedIds), ',', currentUserId, DateTime.Now);
                ProviderUtil.parse_guids(ref reader, ref createdIds);
                return createdIds.Count > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool MoveTreesOrTreeNodes(Guid applicationId, Guid treeIdOrTreeNodeId,
            List<Guid> movedIds, Guid currentUserId, ref List<Guid> rootIds, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("MoveTreesOrTreeNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, treeIdOrTreeNodeId,
                    ProviderUtil.list_to_string<Guid>(ref movedIds), ',', currentUserId, DateTime.Now);
                ProviderUtil.parse_guids(ref reader, ref rootIds, ref errorMessage);
                return rootIds.Count > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool MoveTreeNode(Guid applicationId, 
            List<Guid> treeNodeIds, Guid? parentTreeNodeId, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("MoveTreeNode");

            try
            {
                if (parentTreeNodeId == Guid.Empty) parentTreeNodeId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref treeNodeIds), ',', parentTreeNodeId,
                    currentUserId, DateTime.Now), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool ArithmeticDeleteTreeNode(Guid applicationId, 
            List<Guid> treeNodeIds, Guid? treeOwnerId, bool? removeHierarchy, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteTreeNode");

            try
            {
                if (treeOwnerId == Guid.Empty) treeOwnerId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref treeNodeIds), ',', 
                    treeOwnerId, removeHierarchy, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static void GetTreeNodes(Guid applicationId, ref List<TreeNode> retTreeNodes, Guid treeId)
        {
            string spName = GetFullyQualifiedName("GetTreeNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, treeId);
                _parse_tree_nodes(ref reader, ref retTreeNodes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static void GetTreeNodes(Guid applicationId, 
            ref List<TreeNode> retTreeNodes, ref List<Guid> treeNodeIds)
        {
            string spName = GetFullyQualifiedName("GetTreeNodesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref treeNodeIds), ',');
                _parse_tree_nodes(ref reader, ref retTreeNodes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static void GetRootNodes(Guid applicationId, ref List<TreeNode> retTreeNodes, Guid treeId)
        {
            string spName = GetFullyQualifiedName("GetRootNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, treeId);
                _parse_tree_nodes(ref reader, ref retTreeNodes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static void GetChildNodes(Guid applicationId, ref List<TreeNode> retTreeNodes, 
            Guid? parentNodeId, Guid? treeId, string searchText)
        {
            string spName = GetFullyQualifiedName("GetChildNodes");

            try
            {
                if (parentNodeId == Guid.Empty) parentNodeId = null;
                if (treeId == Guid.Empty) treeId = null;

                if (!parentNodeId.HasValue && !treeId.HasValue) return;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    parentNodeId, treeId, ProviderUtil.get_search_text(searchText));
                _parse_tree_nodes(ref reader, ref retTreeNodes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static TreeNode GetParentNode(Guid applicationId, Guid treeNodeId)
        {
            string spName = GetFullyQualifiedName("GetParentNode");

            try
            {
                List<TreeNode> retTreeNodes = new List<TreeNode>();
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, treeNodeId);
                _parse_tree_nodes(ref reader, ref retTreeNodes);
                return retTreeNodes.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return null;
            }
        }

        public static bool AddFiles(Guid applicationId, 
            Guid ownerId, FileOwnerTypes ownerType, ref List<DocFileInfo> attachments, Guid currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Attachments
            DataTable attachmentsTable = new DataTable();
            attachmentsTable.Columns.Add("FileID", typeof(Guid));
            attachmentsTable.Columns.Add("FileName", typeof(string));
            attachmentsTable.Columns.Add("Extension", typeof(string));
            attachmentsTable.Columns.Add("MIME", typeof(string));
            attachmentsTable.Columns.Add("Size", typeof(long));
            attachmentsTable.Columns.Add("OwnerID", typeof(Guid));
            attachmentsTable.Columns.Add("OwnerType", typeof(string));

            foreach (DocFileInfo _att in attachments)
            {
                attachmentsTable.Rows.Add(_att.FileID, _att.FileName,
                    _att.Extension, _att.MIME(), _att.Size, _att.OwnerID, _att.OwnerType);
            }

            SqlParameter attachmentsParam = new SqlParameter("@Attachments", SqlDbType.Structured);
            attachmentsParam.TypeName = "[dbo].[DocFileInfoTableType]";
            attachmentsParam.Value = attachmentsTable;
            //end of Add Attachments

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@OwnerID", ownerId);
            cmd.Parameters.AddWithValue("@OwnerType", ownerType.ToString());
            cmd.Parameters.Add(attachmentsParam);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("AddFiles");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@OwnerID" + sep + "@OwnerType" + sep +
                "@Attachments" + sep + "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
            finally { con.Close(); }
        }

        public static void GetOwnerFiles(Guid applicationId, 
            ref List<DocFileInfo> retFiles, ref List<Guid> ownerIds, FileOwnerTypes ownerType)
        {
            string spName = GetFullyQualifiedName("GetOwnerFiles");

            try
            {
                if (ownerIds.Count == 0) return;
                string strOwnerType = null;
                if (ownerType != FileOwnerTypes.None) strOwnerType = ownerType.ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref ownerIds), ',', strOwnerType);
                _parse_files(applicationId, ref reader, ref retFiles);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static void GetFiles(Guid applicationId, ref List<DocFileInfo> retFiles, ref List<Guid> fileIds)
        {
            string spName = GetFullyQualifiedName("GetFilesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref fileIds), ',');
                _parse_files(applicationId, ref reader, ref retFiles);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static void GetFileOwnerNodes(Guid applicationId, ref List<DocFileInfo> retFiles, ref List<Guid> fileIds)
        {
            string spName = GetFullyQualifiedName("GetFileOwnerNodes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref fileIds), ',');
                _parse_file_owner_nodes(ref reader, ref retFiles);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        public static bool RenameFile(Guid applicationId, Guid fileId, string name)
        {
            string spName = GetFullyQualifiedName("RenameFile");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, fileId, name));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool ArithmeticDeleteFiles(Guid applicationId, Guid? ownerId, ref List<Guid> fileIds)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteFiles");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, ProviderUtil.list_to_string<Guid>(ref fileIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool CopyFile(Guid applicationId, Guid ownerId, Guid fileId, 
            FileOwnerTypes ownerType, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("CopyFile");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, fileId, ownerType.ToString(), currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static void GetTreeNodeHierarchy(Guid applicationId, ref List<Hierarchy> retList, Guid treeNodeId)
        {
            string spName = GetFullyQualifiedName("GetTreeNodeHierarchy");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, treeNodeId);
                ProviderUtil.parse_hierarchy(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }


        //get not extracted file from DB
        public static void GetNotExtractedFiles(Guid applicationId, ref List<DocFileInfo> retFiles, 
            string allowedExtractions, char delimiter, int? count)
        {
            string spName = GetFullyQualifiedName("GetNotExtractedFiles");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    allowedExtractions, delimiter, count);
                _parse_files(applicationId, ref reader, ref retFiles);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
            }
        }

        //save extracted file content in DB
        public static bool SaveFileContent(Guid applicationId, Guid FileID, string Content, 
            bool NotExtractable, bool fileNotFound, double duration, string errorText)
        {
            string spName = GetFullyQualifiedName("SaveFileContent");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    FileID, Content, NotExtractable, fileNotFound, duration, DateTime.Now, errorText));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool SetTreeNodesOrder(Guid applicationId, List<Guid> treeNodeIds)
        {
            string spName = GetFullyQualifiedName("SetTreeNodesOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(treeNodeIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DCT);
                return false;
            }
        }

        public static bool IsPrivateTree(Guid applicationId, Guid treeIdOrTreeNodeId)
        {
            string spName = GetFullyQualifiedName("IsPrivateTree");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, treeIdOrTreeNodeId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool AddOwnerTree(Guid applicationId, Guid ownerId, Guid treeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddOwnerTree");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, treeId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteOwnerTree(Guid applicationId,
            Guid ownerId, Guid treeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteOwnerTree");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, treeId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static Guid? GetTreeOwnerID(Guid applicationId, Guid treeIdOrTreeNodeId)
        {
            string spName = GetFullyQualifiedName("GetTreeOwnerID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, treeIdOrTreeNodeId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }

        public static void GetOwnerTrees(Guid applicationId, ref List<Tree> lstTrees, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetOwnerTrees");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                _parse_trees(ref reader, ref lstTrees);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void CloneTrees(Guid applicationId, ref List<Tree> lstTrees, 
            List<Guid> treeIds, Guid? ownerId, bool? allowMultiple, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("CloneTrees");

            try
            {
                if (ownerId == Guid.Empty) ownerId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(treeIds), ',', ownerId, allowMultiple, currentUserId, DateTime.Now);
                _parse_trees(ref reader, ref lstTrees);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static bool AddTreeNodeContents(Guid applicationId, 
            Guid treeNodeId, List<Guid> nodeIds, Guid? removeFrom, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddTreeNodeContents");

            try
            {
                if (removeFrom == Guid.Empty) removeFrom = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    treeNodeId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', removeFrom, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool RemoveTreeNodeContents(Guid applicationId,
            Guid treeNodeId, List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveTreeNodeContents");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    treeNodeId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }
    }
}
