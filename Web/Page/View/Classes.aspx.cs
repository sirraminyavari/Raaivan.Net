using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Web.API;
using RaaiVan.Modules.Users;

namespace RaaiVan.Web.Page.View
{
    public partial class Classes : System.Web.UI.Page
    {
        ParamsContainer paramsContainer = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            paramsContainer = new ParamsContainer(HttpContext.Current);

            if (!paramsContainer.IsAuthenticated) {
                paramsContainer.redirect_to_login_page();
                return;
            }

            if (!paramsContainer.ApplicationID.HasValue) return;

            Guid? id = PublicMethods.parse_guid(Request.Params["ID"]);
            List<Guid> ids = ListMaker.get_guid_items(Request.Params["IDs"], ',');

            if (id.HasValue && !ids.Any(u => u == id)) ids.Add(id.Value);

            List<NodeType> nodeTypes = ids.Count == 0 ? new List<NodeType>() : 
                CNController.get_node_types(paramsContainer.Tenant.Id, ids);

            Guid? relatedId = PublicMethods.parse_guid(Request.Params["RelatedID"]);

            string relatedItem = "null";

            if (relatedId.HasValue) {
                Modules.CoreNetwork.Node relatedNode = CNController.get_node(paramsContainer.Tenant.Id, relatedId.Value);

                User relatedUser = relatedNode != null && relatedNode.NodeID.HasValue ? null :
                    UsersController.get_user(paramsContainer.Tenant.Id, relatedId.Value);

                if (relatedNode != null && relatedNode.NodeID.HasValue) relatedItem = relatedNode.toJson(simple: true);
                else if (relatedUser != null && relatedUser.UserID.HasValue) relatedItem = relatedUser.toJson();
            }

            initialJson.Value = "{\"RelatedItem\":" + relatedItem + 
                ",\"NodeTypes\":[" +
                    string.Join(",", nodeTypes.Select(u => "{\"NodeTypeID\":\"" + u.NodeTypeID.ToString() + "\"" +
                        ",\"TypeName\":\"" + Base64.encode(u.Name) + "\"" + "}")) + "]" + 
                "}";
        }
    }
}