using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using Newtonsoft.Json;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.Users;

namespace RaaiVan.Web.API
{
    [Serializable]
    public class TemplateNodeType {
        [JsonProperty("id")]
        public string ID;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("is_service")]
        public bool? IsService;

        [JsonProperty("has_form")]
        public bool? HasForm;

        [JsonProperty("has_wiki")]
        public bool? HasWiki;

        [JsonProperty("form_id")]
        public string FormID;

        [JsonProperty("enable_contribution")]
        public bool? EnableContribution;

        [JsonProperty("disable_abstract_and_keywords")]
        public bool? DisableAbstractAndKeywords;

        [JsonProperty("disable_file_upload")]
        public bool? DisableFileUpload;

        [JsonProperty("disable_related_nodes_select")]
        public bool? DisableRelatedNodesSelect;
    }

    [Serializable]
    public class TemplateFormElement
    {
        [JsonProperty("id")]
        public string ID;

        [JsonProperty("code")]
        public string Code;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("weight")]
        public double? Weight;

        [JsonProperty("info")]
        public Dictionary<string, object> Info;
    }

    [Serializable]
    public class TemplateForm {
        [JsonProperty("id")]
        public string ID;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("elements")]
        public List<TemplateFormElement> Elements;

        public TemplateForm() {
            Elements = new List<TemplateFormElement>();
        }
    }

    [Serializable]
    public class TemplateIDs {
        [JsonProperty("dic")]
        private Dictionary<string, string> Dic; //Key: Guid, Value: string

        [JsonProperty("use_existing_objects")]
        private Dictionary<string, Guid> UseExistingObjects;

        [JsonIgnore]
        private Dictionary<string, Guid> NewIDs;

        public TemplateIDs() {
            Dic = new Dictionary<string, string>();
            UseExistingObjects = new Dictionary<string, Guid>();
            NewIDs = new Dictionary<string, Guid>();
        }

        public string resolve(Guid? id)
        {
            if (!id.HasValue) return string.Empty;
            else if (Dic.ContainsKey(id.Value.ToString())) return Dic[id.Value.ToString()];
            else
            {
                string key = string.Empty;

                while (string.IsNullOrEmpty(key) || Dic.Keys.Any(u => Dic[u] == key))
                    key = PublicMethods.random_string(10).ToLower();

                Dic[id.Value.ToString()] = key;

                return key;
            }
        }

        public Guid? get_existing_id(string id) {
            return string.IsNullOrEmpty(id) || UseExistingObjects == null || !UseExistingObjects.ContainsKey(id) ?
                null : (Guid?)UseExistingObjects[id];
        }

        public bool is_replaced(string id) {
            return get_existing_id(id).HasValue;
        }

        public Guid? new_id(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (!NewIDs.ContainsKey(id)) NewIDs[id] = Guid.NewGuid();

            return NewIDs[id];
        }
    }

    [Serializable]
    public class Template
    {
        [JsonProperty("ids")]
        private TemplateIDs IDs;

        [JsonProperty("root")]
        private List<string> RootIDs;

        [JsonProperty("node_types")]
        private Dictionary<string, TemplateNodeType> NodeTypes;

        [JsonProperty("forms")]
        private Dictionary<string, TemplateForm> Forms;

        [JsonProperty("dependencies")]
        private Dictionary<string, ArrayList> Dependencies;

        public Template(Guid? nodeTypeId) {
            //Initilization
            IDs = new TemplateIDs();
            NodeTypes = new Dictionary<string, TemplateNodeType>();
            Forms = new Dictionary<string, TemplateForm>();
            Dependencies = new Dictionary<string, ArrayList>();
            //end of Initialization


            //Calculation
            List<NodeType> children = !nodeTypeId.HasValue || !RefAppID.HasValue ? new List<NodeType>() :
                CNController.get_child_node_types(RefAppID.Value, nodeTypeId.Value);

            List<Guid> root = (children.Count > 0 ? children.Select(c => c.NodeTypeID).ToList() : new List<Guid?>() { nodeTypeId })
                .Where(id => id.HasValue).Select(id => id.Value).ToList();

            root.ForEach(r => add_node_type(r));

            RootIDs = root.Select(r => IDs.resolve(r)).ToList();
            //Calculation
        }

        [JsonIgnore]
        private Guid? RefAppID
        {
            get
            {
                return RaaiVanSettings.ReferenceTenantID;
            }
        }

        private void add_dependency(Guid? id, Guid? dependsOnId) {
            if (!id.HasValue || !dependsOnId.HasValue) return;

            string strId = IDs.resolve(id);
            string strDependsOnId = IDs.resolve(dependsOnId);

            if (!Dependencies.ContainsKey(strId)) Dependencies[strId] = new ArrayList();

            if (!Dependencies[strId].ToArray().Select(i => (string)i).Any(i => i == strDependsOnId))
                Dependencies[strId].Add(strDependsOnId);
        }

        private void add_node_type(Guid? nodeTypeId)
        {
            if (!RefAppID.HasValue || !nodeTypeId.HasValue || !RaaiVanSettings.SAASBasedMultiTenancy) return;

            //get new id
            string newNtId = IDs.resolve(nodeTypeId);
            if (NodeTypes.ContainsKey(newNtId)) return;
            //end of get new id

            Guid appId = RefAppID.Value;
            Guid ntId = nodeTypeId.Value;

            NodeType nodeType = CNController.get_node_type(appId, ntId);

            if (nodeType == null || string.IsNullOrEmpty(nodeType.Name)) return;

            Service service = CNController.get_service(appId, ntId);
            if (service == null) service = new Service();

            List<Extension> extensions = CNController.get_extensions(appId, ntId)
                .Where(ex => !ex.Disabled.HasValue || !ex.Disabled.Value).ToList();
            FormType form = FGController.get_owner_form(appId, ntId);
            List<FormElement> formElements = form == null || !form.FormID.HasValue ? new List<FormElement>() :
                FGController.get_form_elements(appId, form.FormID.Value);

            bool hasFormExt = extensions.Any(e => e.ExtensionType == ExtensionType.Form) && formElements.Count > 0;
            bool hasWikiExt = extensions.Any(e => e.ExtensionType == ExtensionType.Wiki);

            bool hasForm = formElements.Count > 0 && (hasFormExt || hasWikiExt);

            TemplateNodeType newNodeType = new TemplateNodeType() {
                ID = newNtId,
                Name = nodeType.Name,
                IsService = !string.IsNullOrEmpty(service.Title),
                HasForm = hasFormExt,
                HasWiki = hasWikiExt,
                FormID = !hasForm ? null : IDs.resolve(form.FormID),
                EnableContribution = service.EnableContribution,
                DisableAbstractAndKeywords = service.DisableAbstractAndKeywords,
                DisableFileUpload = service.DisableFileUpload,
                DisableRelatedNodesSelect = service.DisableRelatedNodesSelect
            };

            if (hasForm)
            {
                add_dependency(ntId, form.FormID);
                add_form(form, formElements);
            }

            NodeTypes[newNtId] = newNodeType;
        }

        private void add_form(FormType form, List<FormElement> elements)
        {
            //get new id
            string newFormId = IDs.resolve(form.FormID.Value);
            if (Forms.ContainsKey(newFormId)) return;
            //end of get new id

            TemplateForm newForm = new TemplateForm() {
                ID = newFormId,
                Title = form.Title
            };

            elements.Where(e => e.Type.HasValue).ToList().ForEach(elem => {
                TemplateFormElement newFormElement = new TemplateFormElement() {
                    ID = IDs.resolve(elem.ElementID),
                    Code = elem.Name,
                    Title = elem.Title,
                    Type = elem.Type.Value.ToString(),
                    Weight = !elem.Weight.HasValue ? 0 : elem.Weight.Value,
                    Info = PublicMethods.fromJSON(elem.Info)
                };

                newForm.Elements.Add(newFormElement);

                if (elem.Type == FormElementTypes.MultiLevel && newFormElement.Info.ContainsKey("NodeType")) {
                    Dictionary<string, object> infoNt = 
                        PublicMethods.get_dic_value<Dictionary<string, object>>(newFormElement.Info, "NodeType");

                    Guid? _id = infoNt == null || !infoNt.ContainsKey("ID") ? null : PublicMethods.parse_guid(infoNt["ID"].ToString());

                    if (_id.HasValue) {
                        string strId = IDs.resolve(_id);
                        ((Dictionary<string, object>)newFormElement.Info["NodeType"])["ID"] = strId.ToString();

                        add_dependency(elem.ElementID, _id);
                        add_node_type(_id);
                    }
                }
                else if (elem.Type == FormElementTypes.Form && newFormElement.Info.ContainsKey("FormID"))
                {
                    Guid? _id = PublicMethods.parse_guid(newFormElement.Info["FormID"].ToString());

                    if (_id.HasValue)
                    {
                        string strId = IDs.resolve(_id);
                        newFormElement.Info["FormID"] = strId;

                        add_dependency(elem.ElementID, _id);
                        add_form(_id.Value);
                    }
                }
                else if (elem.Type == FormElementTypes.Node && newFormElement.Info.ContainsKey("NodeTypes"))
                {
                    ArrayList infoNts = PublicMethods.get_dic_value<ArrayList>(newFormElement.Info, "NodeTypes");

                    List<Dictionary<string, object>> newNts = (infoNts == null ? new ArrayList() : infoNts).ToArray().ToList()
                        .Where(itm => itm.GetType() == typeof(Dictionary<string, object>))
                        .Select(itm => (Dictionary<string, object>)itm)
                        .Where(itm => itm.ContainsKey("NodeTypeID")).ToList()
                        .Select(itm =>
                        {
                            Guid? ntId = PublicMethods.parse_guid(itm["NodeTypeID"].ToString());

                            if (!ntId.HasValue) return null;

                            string strId = IDs.resolve(ntId);

                            itm["NodeTypeID"] = strId;

                            add_dependency(elem.ElementID, ntId);
                            add_node_type(ntId);

                            return itm;
                        }).Where(itm => itm != null).ToList();

                    newFormElement.Info["NodeTypes"] = newNts;
                }
            });

            Forms[newFormId] = newForm;
        }

        private void add_form(Guid formId) {
            FormType form = FGController.get_form(RefAppID.Value, formId);
            List<FormElement> elements = FGController.get_form_elements(RefAppID.Value, formId);
            add_form(form, elements);
        }

        public bool activate(Guid applicationId, Guid currentUserId)
        {
            return RootIDs != null && RootIDs.Where(id => !IDs.is_replaced(id)).ToList()
                .Select(id => activate_node_type(applicationId, currentUserId, id) != null).Count() > 0;
        }

        private NodeType activate_node_type(Guid applicationId, Guid currentUserId, string id)
        {
            TemplateNodeType tempNodeType = !NodeTypes.ContainsKey(id) ? null : NodeTypes[id];
            if (tempNodeType == null) return null;

            NodeType nodeType = new NodeType() {
                NodeTypeID = IDs.new_id(tempNodeType.ID),
                Name = tempNodeType.Name,
                CreatorUserID = currentUserId
            };

            if(!CNController.add_node_type(applicationId, nodeType)) return null;

            CNController.initialize_extensions(applicationId, nodeType.NodeTypeID.Value, currentUserId, ignoreDefault: true);
            CNController.initialize_service(applicationId, nodeType.NodeTypeID.Value);

            if (tempNodeType.HasForm.HasValue && tempNodeType.HasForm.Value)
                CNController.enable_extension(applicationId, nodeType.NodeTypeID.Value, ExtensionType.Form, currentUserId);

            if (tempNodeType.HasWiki.HasValue && tempNodeType.HasWiki.Value)
                CNController.enable_extension(applicationId, nodeType.NodeTypeID.Value, ExtensionType.Wiki, currentUserId);

            if (tempNodeType.IsService.HasValue && tempNodeType.IsService.Value)
                CNController.set_service_title(applicationId, nodeType.NodeTypeID.Value, nodeType.Name);

            if (tempNodeType.EnableContribution.HasValue && tempNodeType.EnableContribution.Value)
                CNController.enable_contribution(applicationId, nodeType.NodeTypeID.Value, true);

            if (tempNodeType.DisableAbstractAndKeywords.HasValue && tempNodeType.DisableAbstractAndKeywords.Value)
                CNController.abstract_and_keywords_disabled(applicationId, nodeType.NodeTypeID.Value, true);

            if (tempNodeType.DisableFileUpload.HasValue && tempNodeType.DisableFileUpload.Value)
                CNController.file_upload_disabled(applicationId, nodeType.NodeTypeID.Value, true);

            if (tempNodeType.DisableRelatedNodesSelect.HasValue && tempNodeType.DisableRelatedNodesSelect.Value)
                CNController.related_nodes_select_disabled(applicationId, nodeType.NodeTypeID.Value, true);

            if (tempNodeType.HasForm.HasValue && tempNodeType.HasForm.Value)
            {
                FormType form = activate_form(applicationId, currentUserId, tempNodeType.FormID);
                if (form != null) FGController.set_form_owner(applicationId, nodeType.NodeTypeID.Value, form.FormID.Value, currentUserId);
            }

            return nodeType;
        }

        private FormType activate_form(Guid applicationId, Guid currentUserId, string id)
        {
            TemplateForm tempForm = !Forms.ContainsKey(id) ? null : Forms[id];
            if (tempForm == null) return null;

            FormType form = new FormType() {
                FormID = IDs.new_id(id),
                Title = tempForm.Title + " " + PublicMethods.get_random_number().ToString(),
                Creator = new User() { UserID = currentUserId }
            };

            if (!FGController.create_form(applicationId, form)) return null;

            List<FormElement> elements = tempForm.Elements == null ? new List<FormElement>() : tempForm.Elements.Select(e =>
            {
                FormElement newElem = new FormElement() {
                    ElementID = IDs.new_id(e.ID),
                    Name = e.Code,
                    Title = e.Title,
                    Weight = e.Weight
                };

                FormElementTypes tp = FormElementTypes.Text;
                if (!Enum.TryParse<FormElementTypes>(e.Type, out tp)) return null;
                else newElem.Type = tp;

                if (tp == FormElementTypes.MultiLevel && e.Info.ContainsKey("NodeType"))
                {
                    Dictionary<string, object> infoNt = PublicMethods.get_dic_value<Dictionary<string, object>>(e.Info, "NodeType");

                    string _id = infoNt == null || !infoNt.ContainsKey("ID") ? null : infoNt["ID"].ToString();

                    NodeType nodeType = string.IsNullOrEmpty(_id) ? null : (IDs.is_replaced(_id) ? 
                        CNController.get_node_type(applicationId, IDs.get_existing_id(_id).Value) :
                        activate_node_type(applicationId, currentUserId, _id));

                    if (nodeType == null) return null;

                    ((Dictionary<string, object>)e.Info["NodeType"])["ID"] = nodeType.NodeTypeID.Value.ToString();
                    ((Dictionary<string, object>)e.Info["NodeType"])["Name"] = Base64.encode(nodeType.Name);
                }
                else if (tp == FormElementTypes.Form && e.Info.ContainsKey("FormID"))
                {
                    string _id = e.Info["FormID"].ToString();

                    FormType elmForm = string.IsNullOrEmpty(_id) ? null : (IDs.is_replaced(_id) ? 
                        FGController.get_form(applicationId, IDs.get_existing_id(_id).Value) :
                        activate_form(applicationId, currentUserId, _id));

                    if (elmForm == null) return null;

                    e.Info["FormID"] = elmForm.FormID.Value.ToString();
                    e.Info["FormName"] = Base64.encode(elmForm.Title);
                }
                else if (tp == FormElementTypes.Node && e.Info.ContainsKey("NodeTypes"))
                {
                    ArrayList infoNts = PublicMethods.get_dic_value<ArrayList>(e.Info, "NodeTypes");

                    List<Dictionary<string, object>> newNts = (infoNts == null ? new ArrayList() : infoNts).ToArray().ToList()
                        .Where(itm => itm.GetType() == typeof(Dictionary<string, object>))
                        .Select(itm => (Dictionary<string, object>)itm)
                        .Where(itm => itm.ContainsKey("NodeTypeID")).ToList()
                        .Select(itm =>
                        {
                            string _id = itm["NodeTypeID"].ToString();

                            NodeType nodeType = string.IsNullOrEmpty(_id) ? null : (IDs.is_replaced(_id) ?
                                CNController.get_node_type(applicationId, IDs.get_existing_id(_id).Value) :
                                activate_node_type(applicationId, currentUserId, _id));

                            if (nodeType == null) return null;

                            itm["NodeTypeID"] = nodeType.NodeTypeID.Value;
                            itm["NodeType"] = Base64.encode(nodeType.Name);

                            return itm;
                        }).Where(itm => itm != null).ToList();

                    e.Info["NodeTypes"] = newNts;
                }

                newElem.Info = PublicMethods.toJSON(e.Info);

                return newElem;
            }).Where(e => e != null).ToList();

            FGController.save_form_elements(applicationId, form.FormID.Value, null, null, null, elements, currentUserId);

            return form;
        }
    }
}