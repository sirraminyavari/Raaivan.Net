using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Documents
{
    public class DocumentsController
    {
        public static bool create_tree(Guid applicationId, Tree Info)
        {
            return DataProvider.CreateTree(applicationId, Info);
        }

        public static bool change_tree(Guid applicationId, Tree Info)
        {
            return DataProvider.ChangeTree(applicationId, Info);
        }

        public static bool remove_trees(Guid applicationId, List<Guid> treeIds, Guid? ownerId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteTree(applicationId, treeIds, ownerId, currentUserId);
        }

        public static bool remove_tree(Guid applicationId, Guid treeId, Guid? ownerId, Guid currentUserId)
        {
            return remove_trees(applicationId, new List<Guid>() { treeId }, ownerId, currentUserId);
        }

        public static bool recycle_tree(Guid applicationId, Guid treeId, Guid currentUserId)
        {
            return DataProvider.RecycleTree(applicationId, treeId, currentUserId);
        }

        public static List<Tree> get_trees(Guid applicationId, ref List<Guid> treeIds)
        {
            List<Tree> retList = new List<Tree>();
            DataProvider.GetTrees(applicationId, ref retList, ref treeIds);
            return retList;
        }

        public static Tree get_tree(Guid applicationId, Guid treeIdOrTreeNodeId)
        {
            List<Guid> _tIds = new List<Guid>();
            _tIds.Add(treeIdOrTreeNodeId);
            return get_trees(applicationId, ref _tIds).FirstOrDefault();
        }

        public static List<Tree> get_trees(Guid applicationId, Guid? ownerId = null, bool? archive = null)
        {
            List<Tree> retList = new List<Tree>();
            DataProvider.GetTrees(applicationId, ref retList, ownerId, archive);
            return retList;
        }

        public static bool add_tree_node(Guid applicationId, TreeNode Info)
        {
            return DataProvider.AddTreeNode(applicationId, Info);
        }

        public static bool change_tree_node(Guid applicationId, TreeNode Info)
        {
            return DataProvider.ChangeTreeNode(applicationId, Info);
        }

        public static bool copy_trees_or_tree_nodes(Guid applicationId, 
            Guid treeIdOrTreeNodeId, List<Guid> copiedIds, Guid currentUserId, ref List<Guid> createdIds)
        {
            return DataProvider.CopyTreesOrTreeNodes(applicationId, 
                treeIdOrTreeNodeId, copiedIds, currentUserId, ref createdIds);
        }

        public static bool move_trees_or_tree_nodes(Guid applicationId, Guid treeIdOrTreeNodeId,
            List<Guid> movedIds, Guid currentUserId, ref List<Guid> rootIds, ref string errorMessage)
        {
            return DataProvider.MoveTreesOrTreeNodes(applicationId, 
                treeIdOrTreeNodeId, movedIds, currentUserId, ref rootIds, ref errorMessage);
        }

        public static bool move_tree_node(Guid applicationId, List<Guid> treeNodeIds, Guid? parentTreeNodeId, 
            Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.MoveTreeNode(applicationId,
                treeNodeIds, parentTreeNodeId, currentUserId, ref errorMessage);
        }

        public static bool remove_tree_node(Guid applicationId, 
            List<Guid> treeNodeIds, Guid? treeOwnerId, bool? removeHierarchy, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteTreeNode(applicationId, treeNodeIds, 
                treeOwnerId, removeHierarchy, currentUserId);
        }

        public static List<TreeNode> get_tree_nodes(Guid applicationId, Guid treeId)
        {
            List<TreeNode> retList = new List<TreeNode>();
            DataProvider.GetTreeNodes(applicationId, ref retList, treeId);
            return retList;
        }

        public static List<TreeNode> get_tree_nodes(Guid applicationId, List<Guid> treeNodeIds)
        {
            List<TreeNode> retList = new List<TreeNode>();
            DataProvider.GetTreeNodes(applicationId, ref retList, ref treeNodeIds);
            return retList;
        }

        public static TreeNode get_tree_node(Guid applicationId, Guid treeNodeId)
        {
            List<Guid> _ids = new List<Guid>();
            _ids.Add(treeNodeId);
            return get_tree_nodes(applicationId, _ids).FirstOrDefault();
        }

        public static List<TreeNode> get_root_nodes(Guid applicationId, Guid treeId)
        {
            List<TreeNode> retList = new List<TreeNode>();
            DataProvider.GetRootNodes(applicationId, ref retList, treeId);
            return retList;
        }

        public static List<TreeNode> get_child_nodes(Guid applicationId, Guid? parentNodeId, 
            Guid? treeId = null, string searchText = null)
        {
            List<TreeNode> retList = new List<TreeNode>();
            DataProvider.GetChildNodes(applicationId, ref retList, parentNodeId, treeId, searchText);
            return retList;
        }

        public static TreeNode get_parent_node(Guid applicationId, Guid treeNodeId)
        {
            return DataProvider.GetParentNode(applicationId, treeNodeId);
        }

        public static bool add_files(Guid applicationId, 
            Guid ownerId, FileOwnerTypes ownerType, ref List<DocFileInfo> attachments, Guid currentUserId)
        {
            return DataProvider.AddFiles(applicationId, ownerId, ownerType, ref attachments, currentUserId);
        }

        public static bool add_file(Guid applicationId, Guid ownerId, 
            FileOwnerTypes ownerType, DocFileInfo attachment, Guid currentUserId)
        {
            List<DocFileInfo> atts = new List<DocFileInfo>();
            atts.Add(attachment);
            return add_files(applicationId, ownerId, ownerType, ref atts, currentUserId);
        }

        public static List<DocFileInfo> get_owner_files(Guid applicationId, 
            ref List<Guid> ownerIds, FileOwnerTypes ownerType = FileOwnerTypes.None)
        {
            List<DocFileInfo> retList = new List<DocFileInfo>();
            DataProvider.GetOwnerFiles(applicationId, ref retList, ref ownerIds, ownerType);
            return retList;
        }

        public static List<DocFileInfo> get_owner_files(Guid applicationId, 
            List<Guid> ownerIds, FileOwnerTypes ownerType = FileOwnerTypes.None)
        {
            return get_owner_files(applicationId, ref ownerIds, ownerType);
        }

        public static List<DocFileInfo> get_owner_files(Guid applicationId, 
            Guid ownerId, FileOwnerTypes ownerType = FileOwnerTypes.None)
        {
            List<Guid> _oIds = new List<Guid>();
            _oIds.Add(ownerId);
            return get_owner_files(applicationId, ref _oIds, ownerType);
        }

        public static List<DocFileInfo> get_files(Guid applicationId, ref List<Guid> fileIds)
        {
            List<DocFileInfo> retList = new List<DocFileInfo>();
            DataProvider.GetFiles(applicationId, ref retList, ref fileIds);
            return retList;
        }

        public static DocFileInfo get_file(Guid applicationId, Guid fileId)
        {
            List<Guid> _oIds = new List<Guid>();
            _oIds.Add(fileId);
            return get_files(applicationId, ref _oIds).FirstOrDefault();
        }

        public static List<DocFileInfo> get_file_owner_nodes(Guid applicationId, List<Guid> fileIds)
        {
            List<DocFileInfo> retList = new List<DocFileInfo>();
            DataProvider.GetFileOwnerNodes(applicationId, ref retList, ref fileIds);
            return retList;
        }

        public static DocFileInfo get_file_owner_node(Guid applicationId, Guid fileId)
        {
            return get_file_owner_nodes(applicationId, new List<Guid> { fileId }).FirstOrDefault();
        }

        public static bool rename_file(Guid applicationId, Guid fileId, string name)
        {
            return DataProvider.RenameFile(applicationId, fileId, name);
        }

        protected static bool _remove_files(Guid applicationId, Guid? ownerId, ref List<Guid> fileIds)
        {
            return DataProvider.ArithmeticDeleteFiles(applicationId, ownerId, ref fileIds);
        }

        public static bool remove_files(Guid applicationId, Guid ownerId, ref List<Guid> fileIds)
        {
            return _remove_files(applicationId, ownerId, ref fileIds);
        }

        public static bool remove_files(Guid applicationId, ref List<Guid> fileIds)
        {
            return _remove_files(applicationId, null, ref fileIds);
        }

        public static bool remove_file(Guid applicationId, Guid ownerId, Guid fileId)
        {
            List<Guid> _fIds = new List<Guid>();
            _fIds.Add(fileId);
            return _remove_files(applicationId, ownerId, ref _fIds);
        }

        public static bool remove_file(Guid applicationId, Guid fileId)
        {
            List<Guid> _fIds = new List<Guid>();
            _fIds.Add(fileId);
            return _remove_files(applicationId, null, ref _fIds);
        }

        public static bool copy_file(Guid applicationId, Guid ownerId, Guid fileId, 
            FileOwnerTypes ownerType, Guid currentUserId)
        {
            return DataProvider.CopyFile(applicationId, ownerId, fileId, ownerType, currentUserId);
        }

        public static List<Hierarchy> get_tree_node_hierarchy(Guid applicationId, Guid treeNodeId)
        {
            List<Hierarchy> retList = new List<Hierarchy>();
            DataProvider.GetTreeNodeHierarchy(applicationId, ref retList, treeNodeId);
            return retList;
        }

        //get not extracted file from DB
        public static List<DocFileInfo> get_not_extracted_files(Guid applicationId, 
            string allowedExtractions, char delimiter, int? count)
        {
            List<DocFileInfo> lst = new List<DocFileInfo>();
            DataProvider.GetNotExtractedFiles(applicationId, ref lst, allowedExtractions, delimiter, count);
            return lst;
        }

        //save extracted file content in DB
        public static bool save_file_content(Guid applicationId, Guid FileID, string Content, 
            bool NotExtractable, bool fileNotFound, double duration, string errorText)
        {
            return DataProvider.SaveFileContent(applicationId,
                FileID, Content, NotExtractable, fileNotFound, duration, errorText);
        }

        public static bool set_tree_nodes_order(Guid applicationId, List<Guid> treeNodeIds)
        {
            return DataProvider.SetTreeNodesOrder(applicationId, treeNodeIds);
        }

        public static bool is_private_tree(Guid applicationId, Guid treeIdOrTreeNodeId)
        {
            return DataProvider.IsPrivateTree(applicationId, treeIdOrTreeNodeId);
        }

        public static bool add_owner_tree(Guid applicationId, Guid ownerId, Guid treeId, Guid currentUserId)
        {
            return DataProvider.AddOwnerTree(applicationId, ownerId, treeId, currentUserId);
        }

        public static bool remove_owner_tree(Guid applicationId, Guid ownerId, Guid treeId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteOwnerTree(applicationId, ownerId, treeId, currentUserId);
        }

        public static Guid? get_tree_owner_id(Guid applicationId, Guid treeIdOrTreeNodeId)
        {
            return DataProvider.GetTreeOwnerID(applicationId, treeIdOrTreeNodeId);
        }

        public static List<Tree> get_owner_trees(Guid applicationId, Guid ownerId)
        {
            List<Tree> retList = new List<Tree>();
            DataProvider.GetOwnerTrees(applicationId, ref retList, ownerId);
            return retList;
        }

        public static List<Tree> clone_trees(Guid applicationId, 
            List<Guid> treeIds, Guid? ownerId, bool? allowMultiple, Guid currentUserId)
        {
            List<Tree> retList = new List<Tree>();
            DataProvider.CloneTrees(applicationId, ref retList, treeIds, ownerId, allowMultiple, currentUserId);
            return retList;
        }

        public static bool add_tree_node_contents(Guid applicationId,
            Guid treeNodeId, List<Guid> nodeIds, Guid? removeFrom, Guid currentUserId)
        {
            return DataProvider.AddTreeNodeContents(applicationId, treeNodeId, nodeIds, removeFrom, currentUserId);
        }

        public static bool remove_tree_node_contents(Guid applicationId,
            Guid treeNodeId, List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.RemoveTreeNodeContents(applicationId, treeNodeId, nodeIds, currentUserId);
        }
    }
}
