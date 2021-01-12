using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Web.API;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Knowledge;
using QRCoder;
using System.Drawing.Imaging;

namespace RaaiVan.Web.Page.View
{
    public partial class Node : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            try
            {
                bool isAuthenticated = paramsContainer.IsAuthenticated;

                if (!isAuthenticated && !RaaiVanSettings.AllowNotAuthenticatedUsers(paramsContainer.ApplicationID))
                {
                    paramsContainer.redirect_to_login_page();
                    return;
                }

                if (!paramsContainer.ApplicationID.HasValue) return;

                Guid nodeId = string.IsNullOrEmpty(Request.Params["ID"]) ? Guid.Empty : Guid.Parse(Request.Params["ID"]);
                if (nodeId == Guid.Empty && !string.IsNullOrEmpty(Request.Params["NodeID"]))
                    nodeId = Guid.Parse(Request.Params["NodeID"]);

                if (nodeId == Guid.Empty) Response.Redirect(isAuthenticated ? PublicConsts.HomePage : PublicConsts.LoginPage);

                if (UsersController.get_user(paramsContainer.Tenant.Id, nodeId) != null)
                    Response.Redirect(PublicConsts.ProfilePage + "/" + nodeId.ToString());

                if (Request.Url.ToString().ToLower().Contains("_escaped_fragment_="))
                {
                    ParamsContainer paramsContainer = new ParamsContainer(HttpContext.Current);

                    Modules.CoreNetwork.Node _nd = CNController.get_node(paramsContainer.Tenant.Id, nodeId, true);

                    string htmlContent = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
                        "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title>" + _nd.Name + " - " + RaaiVanSettings.SystemTitle(paramsContainer.Tenant.Id) + "</title></head><body>" +
                        "<div>" + _nd.Name + "</div>" +
                        "<div>" + ProviderUtil.list_to_string<string>(_nd.Tags, ' ') + "</div>" +
                        "<div>" + PublicMethods.shuffle_text(PublicMethods.markup2plaintext(paramsContainer.Tenant.Id,
                            _nd.Description, true)) + "</div>" +
                        "<div>" + PublicMethods.markup2plaintext(paramsContainer.Tenant.Id,
                            Modules.Wiki.WikiController.get_wiki_content(paramsContainer.Tenant.Id, nodeId), true) + "</div>" +
                        "</body></html>";

                    paramsContainer.return_response(htmlContent);

                    return;
                }

                Guid currentUserId = isAuthenticated ? paramsContainer.CurrentUserID.Value : Guid.Empty;
                bool isSystemAdmin = isAuthenticated &&
                    PublicMethods.is_system_admin(paramsContainer.Tenant.Id, currentUserId);

                //Check User access to Node
                bool isServiceAdmin = false, isAreaAdmin = false, isCreator = false, isContributor = false,
                    isExpert = false, isMember = false, isAdminMember = false, editable = false;
                Service service = CNController.get_service(paramsContainer.Tenant.Id, nodeId);

                Modules.CoreNetwork.Node node = CNController.get_node(paramsContainer.Tenant.Id, nodeId, full: true);

                bool isKnowledge = service != null && service.IsKnowledge.HasValue && service.IsKnowledge.Value;
                
                bool result = true;
                if (isAuthenticated)
                {
                    result = CNController.get_user2node_status(paramsContainer.Tenant.Id, currentUserId, nodeId,
                        ref isCreator, ref isContributor, ref isExpert, ref isMember, ref isAdminMember,
                        ref isServiceAdmin, ref isAreaAdmin, ref editable, service);
                }

                bool hasKnowledgePermission = false, hasWorkFlowPermission = false, 
                    hasWFEditPermission = false, hideContributors = false;

                if (isAuthenticated)
                {
                    (new CNAPI() { paramsContainer = this.paramsContainer })
                        .check_node_workflow_permissions(node, isKnowledge, isSystemAdmin,
                        isServiceAdmin, isAreaAdmin, isCreator, ref hasKnowledgePermission, 
                        ref hasWorkFlowPermission, ref hasWFEditPermission, ref hideContributors);

                    hideContributors = hideContributors && !isSystemAdmin && !isServiceAdmin;
                }
                
                //isWorkflowDirector = 'some code to determine appropriate value

                List<PermissionType> lstPT = new List<PermissionType>() { PermissionType.View, PermissionType.ViewAbstract };

                bool hasAccess = result && (isSystemAdmin || isServiceAdmin || isAreaAdmin || isCreator || isContributor || isExpert ||
                    isMember || hasWorkFlowPermission || hasKnowledgePermission ||
                    PrivacyController.check_access(paramsContainer.Tenant.Id,
                        paramsContainer.CurrentUserID, nodeId, PrivacyObjectType.Node, lstPT).Count > 0);

                if (!hasAccess)
                {
                    bool membershipButton = paramsContainer.CurrentUserID.HasValue &&
                        CNController.has_extension(paramsContainer.Tenant.Id, nodeId, ExtensionType.Group) &&
                        !CNController.is_node_member(paramsContainer.Tenant.Id, nodeId,
                            paramsContainer.CurrentUserID.Value, null, NodeMemberStatuses.Accepted) &&
                        CNController.has_admin(paramsContainer.Tenant.Id, nodeId);
                    bool hasPendingRequest = membershipButton && paramsContainer.CurrentUserID.HasValue &&
                        CNController.is_node_member(paramsContainer.Tenant.Id, nodeId, paramsContainer.CurrentUserID.Value, null, NodeMemberStatuses.Pending);

                    initialJson.Value = "{\"NodeID\":\"" + nodeId.ToString() + "\"" +
                        ",\"AccessDenied\":" + true.ToString().ToLower() +
                        (!paramsContainer.IsAuthenticated ? string.Empty :
                            ",\"Name\":\"" + Base64.encode(node.Name) + "\"" +
                            ",\"PublicDescription\":\"" + Base64.encode(node.PublicDescription) + "\"" +
                            ",\"NodeType\":\"" + Base64.encode(node.NodeType) + "\"" +
                            ",\"MembershipButton\":" + membershipButton.ToString().ToLower() +
                            ",\"HasPendingRequest\":" + hasPendingRequest.ToString().ToLower()
                        ) +
                        "}";

                    return;
                }
                //end of Check User access to Node

                initialJson.Value = "{\"NodeID\":\"" + nodeId.ToString() + "\"" +
                    ",\"ShowWorkFlow\":" + hasWorkFlowPermission.ToString().ToLower() +
                    ",\"ShowKnowledgeOptions\":" + hasKnowledgePermission.ToString().ToLower() +
                    ",\"HideContributors\":" + hideContributors.ToString().ToLower() +
                    "}";

                //register node page visitor
                if (Session[nodeId.ToString()] == null)
                {
                    Session[nodeId.ToString()] = true;
                    UsersController.register_item_visit(paramsContainer.Tenant.Id,
                        nodeId, currentUserId, DateTime.Now, Modules.Users.VisitItemTypes.Node);
                }
                //end of node page visitor registration
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.ApplicationID, null, "NodePage_Load", ex, ModuleIdentifier.RV);
            }
        }
    }
}