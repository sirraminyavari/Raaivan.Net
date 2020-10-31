using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Knowledge;
using RaaiVan.Modules.Documents;
using System.Web.Security;

namespace RaaiVan.Web.Page.View
{
    public partial class NewNode : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);
            
            try
            {
                bool isAuthenticated = paramsContainer.IsAuthenticated;
                if (!isAuthenticated)
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                if (!paramsContainer.ApplicationID.HasValue) return;

                Guid nodeTypeId = string.IsNullOrEmpty(Request.Params["ID"]) ? Guid.Empty : Guid.Parse(Request.Params["ID"]);
                if (nodeTypeId == Guid.Empty && !string.IsNullOrEmpty(Request.Params["NodeTypeID"]))
                    nodeTypeId = Guid.Parse(Request.Params["NodeTypeID"]);
                
                Service service = CNController.get_service(paramsContainer.Tenant.Id, nodeTypeId);

                if (nodeTypeId == Guid.Empty || service == null)
                    Response.Redirect(isAuthenticated ? PublicConsts.HomePage : PublicConsts.LoginPage);

                Guid? parentId = PublicMethods.parse_guid(Request.Params["ParentID"]);
                Guid? documentTreeNodeId = PublicMethods.parse_guid(Request.Params["DocumentTreeNodeID"]);
                Guid? previousVersionId = PublicMethods.parse_guid(Request.Params["PreviousVersionID"]);

                Modules.CoreNetwork.Node parentNode = !parentId.HasValue ? null :
                    CNController.get_node(paramsContainer.Tenant.Id, parentId.Value);
                Modules.CoreNetwork.Node previousVersion = !previousVersionId.HasValue ? null :
                    CNController.get_node(paramsContainer.Tenant.Id, previousVersionId.Value);
                if (previousVersion != null && previousVersion.NodeTypeID != nodeTypeId) previousVersion = null;

                Tree tree = !documentTreeNodeId.HasValue ? null :
                    DocumentsController.get_tree(paramsContainer.Tenant.Id, documentTreeNodeId.Value);
                List<Hierarchy> path = !documentTreeNodeId.HasValue ? new List<Hierarchy>() :
                    DocumentsController.get_tree_node_hierarchy(paramsContainer.Tenant.Id, documentTreeNodeId.Value);
                
                List<Extension> extensions = CNUtilities.extend_extensions(paramsContainer.Tenant.Id, 
                    CNController.get_extensions(paramsContainer.Tenant.Id, nodeTypeId));

                bool isServiceAdmin = CNController.is_service_admin(paramsContainer.Tenant.Id,
                    nodeTypeId, paramsContainer.CurrentUserID.Value);

                KnowledgeType kt = KnowledgeController.get_knowledge_type(paramsContainer.Tenant.Id, nodeTypeId);

                initialJson.Value = "{\"Service\":" + service.toJson(paramsContainer.Tenant.Id) +
                    ",\"Extensions\":{" + string.Join(",", extensions.Where(x => !x.Disabled.HasValue || !x.Disabled.Value)
                        .Select(u => "\"" + u.ExtensionType.ToString() + "\":" + u.toJson())) + "}" +
                    ",\"IsServiceAdmin\":" + isServiceAdmin.ToString().ToLower() +
                    (kt == null ? string.Empty : ",\"KnowledgeType\":" + kt.toJson()) +
                    ",\"ParentNode\":" + (parentNode == null ? "null" : parentNode.toJson()) +
                    ",\"PreviousVersion\":" + (previousVersion == null ? "null" : previousVersion.toJson()) +
                    (tree == null || path == null || path.Count == 0 ? string.Empty :
                        ",\"DocumentTreeNode\":{\"Tree\":" + tree.toJson() + 
                        ",\"Path\":[" + string.Join(",", path.Select(u => u.toJSON())) + "]}") +
                    "}";
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "NewNodePage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}