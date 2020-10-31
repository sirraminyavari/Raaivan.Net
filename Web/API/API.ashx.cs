using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.Jobs;
using RaaiVan.Modules.Documents;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.Log;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for API
    /// </summary>
    public class API : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        ParamsContainer paramsContainer = null;

        public void ProcessRequest(HttpContext context)
        {
            paramsContainer = new ParamsContainer(context, nullTenantResponse: true);
            if (!paramsContainer.ApplicationID.HasValue) return;

            string command = PublicMethods.parse_string(context.Request.Params["command"], false);
            if (!string.IsNullOrEmpty(command)) command = command.ToLower();
            
            string responseText = string.Empty;
            
            Guid userId = Guid.Empty;

            if (command != "authenticate") check_ticket(ref userId);
            
            switch (command)
            {
                case "authenticate":
                    authenticate(PublicMethods.parse_string(context.Request.Params["username"], false),
                        PublicMethods.parse_string(context.Request.Params["password"], false), ref responseText);
                    return_response(responseText);
                    break;
                case "getdeletedstates":
                    get_deleted_states(PublicMethods.parse_int(context.Request.Params["count"]),
                        PublicMethods.parse_long(context.Request.Params["startfrom"]), ref responseText);
                    return_response(responseText);
                    break;
                case "getform":
                case "get_form":
                    get_form(PublicMethods.parse_guid(context.Request.Params["FormID"]), ref responseText);
                    return_response(responseText);
                    break;
                case "searchforminstances":
                case "search_form_instances":
                    search_form_instances(PublicMethods.parse_guid(context.Request.Params["FormID"]),
                        FGUtilities.get_filters_from_json(context.Request.Params["Filters"]), 
                        PublicMethods.parse_int(context.Request.Params["LowerBoundary"]),
                        PublicMethods.parse_int(context.Request.Params["Count"]), ref responseText);
                    return_response(responseText);
                    break;
                case "saveform":
                case "save_form":
                    save_form(PublicMethods.parse_guid(context.Request.Params["FormID"]),
                        context.Request.Params["InputData"], userId, ref responseText);
                    return_response(responseText);
                    break;
                case "getforminstance":
                case "get_form_instance":
                    get_form_instance(PublicMethods.parse_guid(context.Request.Params["InstanceID"]), ref responseText);
                    return_response(responseText);
                    break;
                case "new_node":
                    new_node(PublicMethods.parse_string(context.Request.Params["TypeID"], false),
                        PublicMethods.parse_string(context.Request.Params["Title"]),
                        PublicMethods.parse_string(context.Request.Params["Abstract"]),
                        ListMaker.get_string_items(context.Request.Params["Keywords"], ',').Select(u => Base64.decode(u)).ToList(),
                        FGUtilities.get_form_elements(context.Request.Params["FormDetails"]),
                        ref responseText);
                    return_response(responseText);
                    break;
                case "get_nodes":
                    get_nodes(ListMaker.get_string_items(context.Request.Params["ID"], ','), 
                        ListMaker.get_string_items(context.Request.Params["TypeID"], ','),
                        PublicMethods.parse_string(context.Request.Params["RelatedToTypeID"], false),
                        PublicMethods.parse_string(context.Request.Params["RelatedToID"], false),
                        PublicMethods.parse_string(context.Request.Params["SearchText"]),
                        PublicMethods.parse_date(context.Request.Params["CreationDateFrom"]),
                        PublicMethods.parse_date(context.Request.Params["CreationDateTo"], 1),
                        PublicMethods.parse_string(context.Request.Params["FormFilters"]),
                        PublicMethods.parse_bool(context.Request.Params["MatchAllFilters"]),
                        PublicMethods.parse_bool(context.Request.Params["FormDetails"]),
                        PublicMethods.parse_int(context.Request.Params["Count"]),
                        PublicMethods.parse_int(context.Request.Params["LowerBoundary"]), ref responseText);
                    return_response(responseText);
                    break;
                default:
                    return_response(CustomAPI.handle_request(paramsContainer.Tenant.Id, userId, command, context.Request));
                    break;
            }
        }

        protected void return_response(string responseText)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.BufferOutput = true;
            HttpContext.Current.Response.Write(responseText);
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Close();
        }

        protected void check_ticket(ref Guid userId)
        {
            string tkt = PublicMethods.parse_string(HttpContext.Current.Request.Params["ticket"], false);
            Guid? uId = RestAPI.get_user_id(tkt);

            if (!uId.HasValue) return_response(PublicConsts.InvalidTicketResponse);
            else userId = uId.Value;
        }

        protected void authenticate(string username, string password, ref string responseText)
        {
            string failureText = string.Empty;
            int remainingLockoutTime = 0;

            bool loggedInWithActiveDirectory = false;

            bool success = RaaiVanUtil.authenticate_user(paramsContainer.ApplicationID, username, password, string.Empty, false,
                ref loggedInWithActiveDirectory, ref failureText, ref remainingLockoutTime);
            
            if (success)
            {
                Guid? userId = UsersController.get_user_id(paramsContainer.Tenant.Id, username);
                string ticket = !userId.HasValue ? null : RestAPI.get_ticket(userId.Value);

                if (string.IsNullOrEmpty(ticket) && userId.HasValue)
                {
                    ticket = UserUtilities.generate_password(20).Replace('/', '_').Replace('+', '~').Replace('=', '|');
                    RestAPI.new_ticket(ticket, userId.Value);
                }
                
                responseText = "{\"Result\":\"ok\"" + 
                    ",\"Ticket\":\"" + ticket + "\"" + 
                    ",\"AccessToken\":\"" + AccessTokenList.new_token(ticket) + "\"" +
                    "}";
            }
            else
                responseText = "{\"Result\":\"nok\"}";
        }

        protected string _get_deleted_state_json(string id, string type, DateTime? time, bool? deleted, bool? isRelation, 
            Guid? relSourceId, string relSourceType, Guid? relDestinationId, string relDestinationType, 
            bool bidirectional, bool hasReverse)
        {
            if (!isRelation.HasValue || !relSourceId.HasValue || !relDestinationId.HasValue) isRelation = false;
            if (!time.HasValue) time = new DateTime(2011, 6, 15);

            string json = "{\"ID\":\"" + id + "\"" +
                ",\"Type\":\"" + type + "\"" +
                ",\"Time\":\"" + (!time.HasValue ? string.Empty : time.ToString())  + "\"" +
                ",\"Deleted\":" + (deleted.HasValue && deleted.Value).ToString().ToLower() +
                ",\"IsRelation\":" + isRelation.Value.ToString().ToLower() +
                ",\"RelSourceID\":\"" + (isRelation.Value ? relSourceId.ToString() : string.Empty) + "\"" +
                ",\"RelDestinationID\":\"" + (isRelation.Value ? relDestinationId.ToString() : string.Empty) + "\"" +
                ",\"RelSourceType\":\"" + (isRelation.Value ? relSourceType : string.Empty) + "\"" +
                ",\"RelDestinationType\":\"" + (isRelation.Value ? relDestinationType : string.Empty) + "\"" +
                ",\"Bidirectional\":" + (isRelation.Value ? bidirectional : false).ToString().ToLower() +
                ",\"HasReverse\":" + (isRelation.Value ? hasReverse : false).ToString().ToLower() +
                "}";
            
            return json;
        }

        protected void get_deleted_states(int? count, long? lowerBoundary, ref string responseText)
        {
            List<DeletedState> deletedStates = 
                GlobalController.get_deleted_states(paramsContainer.Tenant.Id, count, lowerBoundary);
            
            responseText = "{\"FirstID\":" + (deletedStates.Count > 0 ? deletedStates.First().ID.Value : 0).ToString() +
                ",\"LastID\":" + (deletedStates.Count > 0 ? deletedStates.Last().ID.Value : 0).ToString() +
                ",\"Items\":[";

            List<Guid> nodeIds = new List<Guid>();

            bool isFirst = true;
            foreach(DeletedState ds in deletedStates)
            {
                if (ds.ObjectType == "EmailAddress") continue;

                if (ds.ObjectType == "Node") nodeIds.Add(ds.ObjectID.Value);

                bool isRelation = ds.RelSourceID.HasValue && ds.RelDestinationID.HasValue;

                responseText += (isFirst ? string.Empty : ",") +
                    _get_deleted_state_json(ds.ObjectID.Value.ToString(), ds.ObjectType, ds.Date, ds.Deleted, isRelation,
                    ds.RelSourceID, ds.RelSourceType, ds.RelDestinationID, ds.RelDestinationType, 
                    ds.Bidirectional.HasValue && ds.Bidirectional.Value, ds.HasReverse.HasValue && ds.HasReverse.Value);

                if(ds.ObjectType == "TaggedItem" && isRelation && ds.RelCreatorID.HasValue)
                {
                    _get_deleted_state_json(Guid.NewGuid().ToString(), "UsedAsTag", ds.Date, false, true,
                    ds.RelCreatorID, "User", ds.RelDestinationID, ds.RelDestinationType, false, true);
                }
                
                isFirst = false;
            }

            if(nodeIds.Count > 0)
            {
                List<Modules.CoreNetwork.Node> nodes = 
                    CNController.get_nodes(paramsContainer.Tenant.Id, nodeIds, full: null, currentUserId: null);

                foreach(Modules.CoreNetwork.Node nd in nodes)
                {
                    responseText += (isFirst ? string.Empty : ",") +
                        _get_deleted_state_json(Guid.NewGuid().ToString(), "IsA", nd.CreationDate, false, true,
                        nd.NodeID.Value, "Node", nd.NodeTypeID.Value, "NodeType", false, true);

                    isFirst = false;
                }
            } //end of 'if(nodeIds.Count > 0)'
            
            responseText += "]}";
        }

        protected void get_form(Guid? formId, ref string responseText)
        {
            if (!formId.HasValue)
            {
                responseText = "{\"Result\":\"nok\"}";
                return;
            }
            
            List<FormElement> elements = FGController.get_form_elements(paramsContainer.Tenant.Id, formId.Value)
                .OrderBy(v => v.SequenceNumber).ToList();

            responseText = "{\"Elements\":[" + string.Join(",", elements.Select(u => u.toJson(paramsContainer.Tenant.Id))) + "]}";
        }

        protected void get_form_instance(Guid? instanceId, ref string responseText)
        {
            if (!instanceId.HasValue)
            {
                responseText = "{\"Result\":\"nok\"}";
                return;
            }
            
            List<FormElement> elements = FGController.get_form_instance_elements(
                paramsContainer.Tenant.Id, instanceId.Value).OrderBy(v => v.SequenceNumber).ToList();

            responseText = "{\"Elements\":[" + string.Join(",", elements.Select(u => u.toJson(paramsContainer.Tenant.Id))) + "]}";
        }

        protected void search_form_instances(Guid? formId, List<FormFilter> filters, 
            int? lowerBoundary, int? count, ref string responseText)
        {
            if (!formId.HasValue) return;

            List<FormRecord> records =
                FGController.get_form_records(paramsContainer.Tenant.Id, formId.Value, filters, lowerBoundary, count);

            responseText = "{\"Records\":[" +
                ProviderUtil.list_to_string<string>(records.Select(
                    u => "{\"InstanceID\":\"" + u.InstanceID.ToString() + "\"" +
                        ",\"Cells\":[" + ProviderUtil.list_to_string<string>(u.Cells.Select(
                            c => "{\"ElementID\":\"" + c.ElementID.ToString() + "\"" +
                                ",\"Value\":\"" + Base64.encode(c.Value) + "\"}"
                            ).ToList()) + 
                        "]}"
                    ).ToList()) + 
                "]}";
            
        }
        
        protected void save_form(Guid? formId, string formData, Guid currentUserId, ref string responseText)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            List<FormElement> formElements = new List<FormElement>();

            if(!formId.HasValue || 
                (formElements = FGController.get_form_elements(paramsContainer.Tenant.Id, formId.Value)).Count == 0)
            {
                res["Result"] = "nok";
                res["Message"] = "no form element found";

                responseText = PublicMethods.toJSON(res);
                return;
            }

            Dictionary<string, object> dataDic = PublicMethods.fromJSON(formData);

            ArrayList elements = PublicMethods.get_value<ArrayList>(dataDic, "Elements");
            Guid? instanceId = PublicMethods.get_value<Guid?>(dataDic, "InstanceID", typeof(Guid));

            if (instanceId == Guid.Empty) instanceId = null;

            if (instanceId.HasValue)
            {
                FormType ft = FGController.get_form_instance(paramsContainer.Tenant.Id, instanceId.Value);

                if(ft != null && ft.Filled.HasValue && ft.Filled.Value)
                {
                    res["Result"] = "nok";
                    res["Message"] = "this form has already been filled";

                    responseText = PublicMethods.toJSON(res);
                    return;
                }
            }

            List<FormElement> toBeSaved = new List<FormElement>();

            for (int i = 0; elements != null && i < elements.Count; ++i)
            {
                Dictionary<string, object> elem = (Dictionary<string, object>)elements[i];

                Guid? elementId = PublicMethods.get_value<Guid?>(elem, "ElementID", typeof(Guid));
                string value = PublicMethods.get_value<string>(elem, "Value", typeof(string));
                
                FormElement refElement = formElements.Where(u => u.ElementID == elementId).FirstOrDefault();

                if (refElement == null) continue;
                
                refElement.RefElementID = refElement.ElementID;

                if (refElement.CreationDate.HasValue) refElement.CreationDate = DateTime.Now;
                if (refElement.Creator.UserID.HasValue) refElement.Creator.UserID = currentUserId;

                if (instanceId.HasValue)
                {
                    refElement.FormInstanceID = instanceId;
                    
                    refElement.ElementID = Guid.NewGuid();

                    refElement.LastModificationDate = DateTime.Now;
                    refElement.LastModifierUserID = currentUserId;
                }
                else { refElement.ElementID = Guid.NewGuid(); }

                if (!FGUtilities.set_element_value(value, ref refElement)) continue;
                
                toBeSaved.Add(refElement);
            }

            if(toBeSaved.Count == 0)
            {
                res["Result"] = "nok";
                res["Message"] = "no element found";

                responseText = PublicMethods.toJSON(res);
                return;
            }

            if (!instanceId.HasValue)
            {
                instanceId = Guid.NewGuid();

                if (!FGController.create_form_instance(paramsContainer.Tenant.Id, new FormType()
                {
                    FormID = formId,
                    InstanceID = instanceId,
                    OwnerID = currentUserId,
                    Creator = new User() { UserID = currentUserId },
                    CreationDate = DateTime.Now
                }))
                {
                    res["Result"] = "nok";
                    res["Message"] = "form instance creation failed";

                    responseText = PublicMethods.toJSON(res);
                    return;
                }

                for (int i = 0, lnt = toBeSaved.Count; i < lnt; ++i)
                    toBeSaved[i].FormInstanceID = instanceId;
            }

            bool result = FGController.save_form_instance_elements(paramsContainer.Tenant.Id, 
                ref toBeSaved, new List<Guid>(), currentUserId);

            res["Result"] = result ? "ok" : "nok";
            res["Message"] = result ? "form saved successfully" : "saving form failed";

            if (result) res["InstanceID"] = instanceId.ToString();

            responseText = PublicMethods.toJSON(res);
        }

        protected void new_node(string typeId, string name, string description, List<string> tags,
            List<FormElement> elements, ref string responseText)
        {
            Guid? nodeTypeId = PublicMethods.parse_guid(typeId);
            if (!nodeTypeId.HasValue) nodeTypeId = CNController.get_node_type_id(paramsContainer.Tenant.Id, typeId);

            if (!nodeTypeId.HasValue || string.IsNullOrEmpty(name))
            {
                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                return;
            }
            else if (!PrivacyController.check_access(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID,
                nodeTypeId.Value, PrivacyObjectType.NodeType, PermissionType.Create)) {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Guid nodeId = Guid.NewGuid();
            Guid? formInstanceId = null;

            try
            {
                FormType form = CNController.has_extension(paramsContainer.Tenant.Id, nodeTypeId.Value, ExtensionType.Form) ?
                    FGController.get_owner_form(paramsContainer.Tenant.Id, nodeTypeId.Value) : null;

                if (form != null && form.FormID.HasValue)
                {
                    FGAPI fgHandler = new FGAPI() { paramsContainer = this.paramsContainer };

                    if (!fgHandler.create_form_instance(form.FormID.Value, nodeId, null, null, null, ref responseText))
                    {
                        responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                        return;
                    }

                    Dictionary<string, object> formResponse = PublicMethods.fromJSON(responseText);

                    Dictionary<string, object> instanceDic = formResponse.ContainsKey("Instance") ?
                        (Dictionary<string, object>)formResponse["Instance"] : new Dictionary<string, object>();

                    if (instanceDic != null && instanceDic.ContainsKey("InstanceID"))
                        formInstanceId = PublicMethods.parse_guid((string)instanceDic["InstanceID"]);

                    if (!formInstanceId.HasValue)
                    {
                        responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                        return;
                    }

                    List<FormElement> formElements = 
                        FGController.get_form_elements(paramsContainer.Tenant.Id, form.FormID.Value);

                    List<FormElement> elemsToBeSaved = new List<FormElement>();

                    elements.ForEach(elem =>
                    {
                        FormElement refElem = formElements.Where(u => u.ElementID == elem.ElementID ||
                            u.ElementID == elem.RefElementID || (!string.IsNullOrEmpty(u.Name) &&
                            !string.IsNullOrEmpty(elem.Name) && u.Name.ToLower() == elem.Name.ToLower())).FirstOrDefault();

                        if (refElem == null) return;
                        
                        elem.FormInstanceID = formInstanceId;
                        elem.Type = refElem.Type;

                        if (!elem.ElementID.HasValue) elem.ElementID = Guid.NewGuid();
                        elem.RefElementID = refElem.ElementID;

                        elemsToBeSaved.Add(elem);
                    });
                    
                    if (!fgHandler.save_form_instance_elements(elemsToBeSaved,
                        new List<Guid>(), null, PollOwnerType.None, ref responseText))
                    {
                        responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                        return;
                    }
                }

                responseText = string.Empty;
                
                (new CNAPI() { paramsContainer = this.paramsContainer })
                    .register_new_node(nodeId, nodeTypeId, null, null, null, name, description, tags,
                        new List<NodeCreator>(), null, null, null, formInstanceId, null, null, null, null, ref responseText);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(paramsContainer.Tenant.Id, null, "API_NewNode", ex, ModuleIdentifier.RV);
                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
            }
        }

        protected void get_nodes(List<string> strNodeIds, List<string> typeIds, string relatedToTypeId, string relatedToId, 
            string searchText, DateTime? creationDateFrom,  DateTime? creationDateTo, string formFilters, bool? matchAllFilters, 
            bool? formDetails, int? count, int? lowerBoundary, ref string responseText)
        {
            if (!count.HasValue) count = 50;
            if (count > 200) count = 200;

            strNodeIds = strNodeIds.Where(u => !string.IsNullOrEmpty(u)).ToList();
            typeIds = typeIds.Where(u => !string.IsNullOrEmpty(u)).ToList();

            List<string> typeAdditionalIds = new List<string>();
            List<Guid> nodeTypeIds = new List<Guid>();

            List<string> nodeAdditionalIds = new List<string>();
            List<Guid> nodeIds = new List<Guid>();

            //Extract NodeTypeIDs
            typeIds.Where(u => !string.IsNullOrEmpty(u)).ToList().ForEach(id => {
                Guid? ntId = PublicMethods.parse_guid(id);

                if (ntId.HasValue) nodeTypeIds.Add(ntId.Value);
                else if (!string.IsNullOrEmpty(id.Trim())) typeAdditionalIds.Add(id);
            });
            
            if (typeAdditionalIds.Count > 0)
                nodeTypeIds.AddRange(CNController.get_node_type_ids(paramsContainer.Tenant.Id, typeAdditionalIds));
            //end of Extract NodeTypeIDs
            
            if (nodeTypeIds.Count == 0) {
                responseText = "{\"Result\":\"nok\",\"Message\":\"InvalidTypeID\"}";
                return;
            }

            //Extract NodeIDs
            if (nodeTypeIds.Count == 1)
            {
                strNodeIds.Where(u => !string.IsNullOrEmpty(u)).ToList().ForEach(id => {
                    Guid? nId = PublicMethods.parse_guid(id);

                    if (nId.HasValue) nodeIds.Add(nId.Value);
                    else if (!string.IsNullOrEmpty(id.Trim())) nodeAdditionalIds.Add(id);
                });

                if (nodeAdditionalIds.Count > 0) nodeIds.AddRange(CNController.get_node_ids(paramsContainer.Tenant.Id,
                    nodeTypeIds[0], nodeAdditionalIds).Values.ToList());
            }
            //end of Extract NodeIDs

            if (strNodeIds.Count > 0 && nodeIds.Count == 0)
            {
                responseText = "{\"Result\":\"nok\",\"Message\":\"InvalidID\"}";
                return;
            }

            Guid? relatedToNodeTypeId = string.IsNullOrEmpty(relatedToTypeId) || string.IsNullOrEmpty(relatedToId) ? null :
                CNController.get_node_type_id(paramsContainer.Tenant.Id, relatedToTypeId);

            Guid? relatedToNodeId = !relatedToNodeTypeId.HasValue ? null :
                (Guid?)CNController.get_node_id(paramsContainer.Tenant.Id, relatedToId, relatedToNodeTypeId.Value);
            if (relatedToNodeId == Guid.Empty) relatedToNodeId = null;

            long totalCount = 0;
            List<FormFilter> filters = new List<FormFilter>();

            if (!string.IsNullOrEmpty(formFilters) && nodeTypeIds.Count == 1) {
                FormType frm = FGController.get_owner_form(paramsContainer.Tenant.Id, nodeTypeIds[0]);
                if (frm != null && frm.FormID.HasValue) filters = FGUtilities.get_filters_from_json(formFilters,
                    formId: frm.FormID.Value, applicationId: paramsContainer.Tenant.Id);
            }

            List<Node> nodes = nodeIds.Count > 0 ? CNController.get_nodes(paramsContainer.Tenant.Id, nodeIds) :
                CNController.get_nodes(paramsContainer.Tenant.Id, nodeTypeIds.Distinct().ToList(),
                    ref totalCount, relatedToNodeId: relatedToNodeId, searchText: searchText, lowerCreationDateLimit: creationDateFrom,
                    upperCreationDateLimit: creationDateTo, filters: filters, matchAllFilters: matchAllFilters,
                    count: count.Value, lowerBoundary: lowerBoundary, checkAccess: true);
            
            List<Guid> downloadAccess = PrivacyController.check_access(paramsContainer.Tenant.Id, null, 
                nodes.Select(u => u.NodeID.Value).ToList(), PrivacyObjectType.Node, PermissionType.Download);

            List<DocFileInfo> files = DocumentsController.get_owner_files(paramsContainer.Tenant.Id,
                nodes.Select(u => u.NodeID.Value).ToList());

            List<FormType> formInstances = FGController.get_owner_form_instances(paramsContainer.Tenant.Id, 
                nodes.Select(u => u.NodeID.Value).ToList());

            List<FormElement> instanceElements = new List<FormElement>();

            if (formDetails.HasValue && formDetails.Value) {
                instanceElements = FGController.get_form_instance_elements(paramsContainer.Tenant.Id,
                    formInstances.Select(u => u.InstanceID.Value).ToList());

                Dictionary<Guid, List<SelectedGuidItem>> guidItemsDic =
                    FGController.get_selected_guids(paramsContainer.Tenant.Id, instanceElements.Select(e => e.ElementID.Value).ToList());

                instanceElements
                    .Where(e => (e.Type == FormElementTypes.Node || e.Type == FormElementTypes.User) &&
                        guidItemsDic.ContainsKey(e.ElementID.Value)).ToList()
                    .ForEach(e => e.GuidItems = guidItemsDic[e.ElementID.Value]);

                List<Guid> selectedNodeIds = new List<Guid>();
                List<Guid> selectedUserIds = new List<Guid>();

                instanceElements
                    .Where(e => (e.Type == FormElementTypes.Node || e.Type == FormElementTypes.User) && e.GuidItems.Count > 0)
                    .ToList().ForEach(e => {
                        if (e.Type == FormElementTypes.Node) selectedNodeIds.AddRange(e.GuidItems.Select(i => i.ID.Value));
                        else if (e.Type == FormElementTypes.User) selectedUserIds.AddRange(e.GuidItems.Select(i => i.ID.Value));
                    });

                List<Node> selectedNodes = selectedNodeIds.Count == 0 ? new List<Node>() :
                    CNController.get_nodes(paramsContainer.Tenant.Id, selectedNodeIds.Distinct().ToList());
                List<User> selectedUsers = selectedUserIds.Count == 0 ? new List<User>() :
                    UsersController.get_users(paramsContainer.Tenant.Id, selectedUserIds.Distinct().ToList());

                instanceElements.Where(e => e.Type == FormElementTypes.Node && e.GuidItems.Count > 0).ToList()
                    .ForEach(e =>
                    {
                        e.GuidItems.ForEach(g => {
                            Node n = selectedNodes.Where(x => x.NodeID == g.ID).FirstOrDefault();

                            g.Type = SelectedGuidItemType.Node;

                            if (n != null) {
                                g.Name = n.Name;
                                g.Code = n.AdditionalID;
                            }
                        });
                    });

                instanceElements.Where(e => e.Type == FormElementTypes.User && e.GuidItems.Count > 0).ToList()
                    .ForEach(e =>
                    {
                        e.GuidItems.ForEach(g => {
                            User u = selectedUsers.Where(x => x.UserID == g.ID).FirstOrDefault();

                            g.Type = SelectedGuidItemType.User;

                            if (u != null)
                            {
                                g.Name = u.FullName;
                                g.Code = u.UserName;
                            }
                        });
                    });
            }

            List<NodeInfo> info = CNController.get_node_info(paramsContainer.Tenant.Id, nodes.Select(n => n.NodeID.Value).ToList(),
                currentUserId: null, description: true, tags: true, creator: false, contributorsCount: false, likesCount: false,
                visitsCount: false, expertsCount: false, membersCount: false, childsCount: false, relatedNodesCount: false, likeStatus: false);

            nodes.ForEach(nd => {
                NodeInfo i = info.Where(x => x.NodeID == nd.NodeID).FirstOrDefault();
                if (i != null)
                {
                    nd.Description = i.Description;
                    nd.Tags = i.Tags;
                }
            });

            
            
            responseText = "{" + 
                (nodeIds.Count > 0 ? string.Empty : "\"TotalCount\":" + totalCount.ToString() + ",") + 
                "\"Nodes\":[" + string.Join(",", nodes.Select(
                    n => "{\"NodeID\":\"" + n.NodeID.ToString() + "\"" +
                        ",\"AdditionalID\":\"" + Base64.encode(n.AdditionalID) + "\"" +
                        ",\"NodeTypeID\":\"" + n.NodeTypeID.ToString() + "\"" +
                        ",\"Name\":\"" + Base64.encode(n.Name) + "\"" +
                        ",\"NodeType\":\"" + Base64.encode(n.NodeType) + "\"" +
                        ",\"Abstract\":\"" + Base64.encode(n.Description) + "\"" +
                        ",\"Keywords\":[" + string.Join(",", n.Tags.Select(t => "\"" + Base64.encode(t) + "\"")) + "]" +
                        ",\"CreationDate\":\"" + (!n.CreationDate.HasValue ? string.Empty : 
                            PublicMethods.get_local_date(n.CreationDate.Value)) + "\"" +
                        ",\"CreationDate_Gregorian\":\"" + (!n.CreationDate.HasValue ? string.Empty :
                            n.CreationDate.Value.ToString()) + "\"" +
                        ",\"Status\":\"" + (n.Status == Status.NotSet ? string.Empty : n.Status.ToString()) + "\"" +
                        ",\"URL\":\"" + n.url(paramsContainer.Tenant.Id) + "\"" +
                        ",\"FormInstanceID\":\"" + (!formInstances.Any(u => u.OwnerID == n.NodeID) ? string.Empty :
                            formInstances.Where(u => u.OwnerID == n.NodeID).Select(i => i.InstanceID).First().ToString()) + "\"" +
                        ",\"Files\":[" + string.Join(",", files.Where(x => x.OwnerID == n.NodeID).Select(
                            f => "{\"Name\":\"" + Base64.encode(f.FileName) + "\"" +
                                ",\"Extension\":\"" + Base64.encode(f.Extension) + "\"" +
                                ",\"Size\":" + (f.Size.HasValue ? f.Size.Value : 0).ToString() +
                                ",\"Downloadable\":" + downloadAccess.Any(d => d == f.OwnerID).ToString().ToLower() +
                                ",\"URL\":\"" + PublicConsts.get_complete_url(paramsContainer.Tenant.Id, 
                                    PublicConsts.FileDownload) + "/" + f.FileID.ToString() + "\"" +
                                "}")) + 
                            "]" +
                        ",\"FormDetails\":{" + string.Join(",", instanceElements
                            .Where(u => formInstances.Any(x => x.OwnerID == n.NodeID) && 
                                u.FormInstanceID == formInstances.Where(y => y.OwnerID == n.NodeID)
                                .FirstOrDefault().InstanceID).Select(
                                a => "\"" + (string.IsNullOrEmpty(a.Name) ? a.ElementID.ToString().ToLower() : 
                                a.Name.ToLower()) + "\":" + a.toJson(paramsContainer.Tenant.Id))) + "}" +
                        "}"
                    )) + "]" + 
                "}";
        }
        
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}