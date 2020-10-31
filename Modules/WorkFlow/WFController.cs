using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.WorkFlow
{
    public class WFController
    {
        public static bool create_state(Guid applicationId, State Info)
        {
            return DataProvider.CreateState(applicationId, Info);
        }

        public static bool modify_state(Guid applicationId, State Info)
        {
            return DataProvider.ModifyState(applicationId, Info);
        }

        public static bool remove_state(Guid applicationId, Guid stateId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteState(applicationId, stateId, currentUserId);
        }

        public static List<State> get_states(Guid applicationId, Guid? workflowId = null)
        {
            List<State> retList = new List<State>();
            DataProvider.GetStates(applicationId, ref retList, workflowId);
            return retList;
        }

        public static List<State> get_states(Guid applicationId, ref List<Guid> stateIds)
        {
            List<State> retList = new List<State>();
            DataProvider.GetStates(applicationId, ref retList, ref stateIds);
            return retList;
        }

        public static State get_state(Guid applicationId, Guid stateId)
        {
            List<Guid> _sIds = new List<Guid>();
            _sIds.Add(stateId);
            return get_states(applicationId, ref _sIds).FirstOrDefault();
        }

        public static bool create_workflow(Guid applicationId, WorkFlow Info)
        {
            return DataProvider.CreateWorkFlow(applicationId, Info);
        }

        public static bool modify_workflow(Guid applicationId, WorkFlow Info)
        {
            return DataProvider.ModifyWorkFlow(applicationId, Info);
        }

        public static bool remove_workflow(Guid applicationId, Guid workflowId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteWorkFlow(applicationId, workflowId, currentUserId);
        }

        public static List<WorkFlow> get_workflows(Guid applicationId)
        {
            List<WorkFlow> retList = new List<WorkFlow>();
            DataProvider.GetWorkFlows(applicationId, ref retList);
            return retList;
        }

        public static List<WorkFlow> get_workflows(Guid applicationId, ref List<Guid> workflowIds)
        {
            List<WorkFlow> retList = new List<WorkFlow>();
            DataProvider.GetWorkFlows(applicationId, ref retList, ref workflowIds);
            return retList;
        }

        public static WorkFlow get_workflow(Guid applicationId, Guid workflowId)
        {
            List<Guid> _wfIds = new List<Guid>();
            _wfIds.Add(workflowId);
            return get_workflows(applicationId, ref _wfIds).FirstOrDefault();
        }

        public static bool add_workflow_state(Guid applicationId, 
            Guid? id, Guid workflowId, Guid stateId, Guid currentUserId)
        {
            return DataProvider.AddWorkFlowState(applicationId, id, workflowId, stateId, currentUserId);
        }

        public static bool remove_workflow_state(Guid applicationId, Guid workflowId, Guid stateId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteWorkFlowState(applicationId, workflowId, stateId, currentUserId);
        }

        public static bool set_workflow_state_description(Guid applicationId, State info)
        {
            return DataProvider.SetWorkFlowStateDescription(applicationId, info);
        }

        public static bool set_workflow_state_tag(Guid applicationId, 
            Guid workflowId, Guid stateId, string tag, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowStateTag(applicationId, workflowId, stateId, tag, currentUserId);
        }

        public static bool remove_workflow_state_tag(Guid applicationId, Guid workflowId, Guid stateId)
        {
            return DataProvider.RemoveWorkFlowStateTag(applicationId, workflowId, stateId);
        }

        public static List<Tag> get_workflow_tags(Guid applicationId, Guid workflowId)
        {
            List<Tag> _tags = new List<Tag>();
            DataProvider.GetWorkFlowTags(applicationId, ref _tags, workflowId);
            return _tags;
        }

        public static bool set_state_director(Guid applicationId, State Info)
        {
            return DataProvider.SetStateDirector(applicationId, Info);
        }

        public static bool set_state_poll(Guid applicationId, 
            Guid workflowId, Guid stateId, Guid? pollId, Guid currentUserId)
        {
            return DataProvider.SetStatePoll(applicationId, workflowId, stateId, pollId, currentUserId);
        }

        public static bool set_state_data_needs_type(Guid applicationId, Guid workflowId, Guid stateId, 
            StateDataNeedsTypes? dataNeedsType, Guid refStateId, Guid currentUserId)
        {
            return DataProvider.SetStateDataNeedsType(applicationId,
                workflowId, stateId, dataNeedsType, refStateId, currentUserId);
        }

        public static bool set_state_data_needs_description(Guid applicationId, 
            Guid workflowId, Guid stateId, string description, Guid currentUserId)
        {
            return DataProvider.SetStateDataNeedsDescription(applicationId,
                workflowId, stateId, description, currentUserId);
        }

        public static bool set_state_description_needed(Guid applicationId, 
            Guid workflowId, Guid stateId, bool descriptionNeeded, Guid currentUserId)
        {
            return DataProvider.SetStateDescriptionNeeded(applicationId,
                workflowId, stateId, descriptionNeeded, currentUserId);
        }

        public static bool set_state_hide_owner_name(Guid applicationId, 
            Guid workflowId, Guid stateId, bool hideOwnerName, Guid currentUserId)
        {
            return DataProvider.SetStateHideOwnerName(applicationId, workflowId, stateId, hideOwnerName, currentUserId);
        }

        public static bool set_state_edit_permisison(Guid applicationId, 
            Guid workflowId, Guid stateId, bool editPermission, Guid currentUserId)
        {
            return DataProvider.SetStateEditPermisison(applicationId, workflowId, stateId, editPermission, currentUserId);
        }

        public static bool set_free_data_need_requests(Guid applicationId, 
            Guid workflowId, Guid stateId, bool freeDataNeedRequests, Guid currentUserId)
        {
            return DataProvider.SetFreeDataNeedRequests(applicationId,
                workflowId, stateId, freeDataNeedRequests, currentUserId);
        }

        public static bool set_state_data_need(Guid applicationId, StateDataNeed Info, Guid? previousNodeTypeId)
        {
            return DataProvider.SetStateDataNeed(applicationId, Info, previousNodeTypeId);
        }

        public static bool remove_state_data_need(Guid applicationId, StateDataNeed Info)
        {
            return DataProvider.ArithmeticDeleteStateDataNeed(applicationId, Info);
        }

        public static bool set_rejection_settings(Guid applicationId, State info)
        {
            return DataProvider.SetRejectionSettings(applicationId, info);
        }

        public static bool set_max_allowed_rejections(Guid applicationId, State info)
        {
            return DataProvider.SetMaxAllowedRejections(applicationId, info);
        }

        public static int get_rejections_count(Guid applicationId, Guid ownerId, Guid workflowId, Guid stateId)
        {
            return DataProvider.GetRejectionsCount(applicationId, ownerId, workflowId, stateId);
        }

        public static Guid? add_state_connection(Guid applicationId, StateConnection Info)
        {
            return DataProvider.AddStateConnection(applicationId, Info);
        }

        public static bool sort_state_connections(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.SortStateConnections(applicationId, ids);
        }

        public static bool move_state_connection(Guid applicationId, StateConnection Info, bool moveDown)
        {
            return DataProvider.MoveStateConnection(applicationId, Info, moveDown);
        }

        public static bool remove_state_connection(Guid applicationId, StateConnection Info)
        {
            return DataProvider.ArithmeticDeleteStateConnection(applicationId, Info);
        }

        public static bool set_state_connection_label(Guid applicationId, StateConnection Info)
        {
            return DataProvider.SetStateConnectionLabel(applicationId, Info);
        }

        public static bool set_state_connection_attachment_status(Guid applicationId, StateConnection Info)
        {
            return DataProvider.SetStateConnectionAttachmentStatus(applicationId, Info);
        }

        public static bool set_state_connection_director(Guid applicationId, StateConnection Info)
        {
            return DataProvider.SetStateConnectionDirector(applicationId, Info);
        }

        public static bool set_state_connection_form(Guid applicationId, StateConnectionForm Info)
        {
            return DataProvider.SetStateConnectionForm(applicationId, Info);
        }

        public static bool remove_state_connection_form(Guid applicationId, StateConnectionForm Info)
        {
            return DataProvider.ArithmeticDeleteStateConnectionForm(applicationId, Info);
        }

        public static bool add_auto_message(Guid applicationId, AutoMessage Info)
        {
            return DataProvider.AddAutoMessage(applicationId, Info);
        }

        public static bool modify_auto_message(Guid applicationId, AutoMessage Info)
        {
            return DataProvider.ModifyAutoMessage(applicationId, Info);
        }

        public static bool remove_auto_message(Guid applicationId, Guid automessageId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteAutoMessage(applicationId, automessageId, currentUserId);
        }

        public static List<AutoMessage> get_owner_auto_messages(Guid applicationId, ref List<Guid> ownerIds)
        {
            List<AutoMessage> retList = new List<AutoMessage>();
            DataProvider.GetOwnerAutoMessages(applicationId, ref retList, ref ownerIds);
            return retList;
        }

        public static List<AutoMessage> get_owner_auto_messages(Guid applicationId, List<Guid> ownerIds)
        {
            return get_owner_auto_messages(applicationId, ref ownerIds);
        }

        public static List<AutoMessage> get_owner_auto_messages(Guid applicationId, Guid ownerId)
        {
            List<Guid> _oIds = new List<Guid>();
            _oIds.Add(ownerId);
            return get_owner_auto_messages(applicationId, ref _oIds);
        }

        public static List<AutoMessage> get_workflow_auto_messages(Guid applicationId, Guid workflowId)
        {
            List<AutoMessage> retList = new List<AutoMessage>();
            DataProvider.GetWorkFlowAutoMessages(applicationId, ref retList, workflowId);
            return retList;
        }

        public static List<AutoMessage> get_connection_auto_messages(Guid applicationId, 
            Guid workflowId, Guid inStateId, Guid outStateId)
        {
            List<AutoMessage> retList = new List<AutoMessage>();
            DataProvider.GetConnectionAutoMessages(applicationId, ref retList, workflowId, inStateId, outStateId);
            return retList;
        }

        public static List<State> get_workflow_states(Guid applicationId, Guid workflowId)
        {
            List<State> retList = new List<State>();
            List<Guid> ids = new List<Guid>();
            DataProvider.GetWorkFlowStates(applicationId, ref retList, workflowId, ref ids, true);
            return retList;
        }

        public static List<State> get_workflow_states(Guid applicationId, Guid workflowId, ref List<Guid> stateIds)
        {
            List<State> retList = new List<State>();
            DataProvider.GetWorkFlowStates(applicationId, ref retList, workflowId, ref stateIds, false);
            return retList;
        }

        public static State get_workflow_state(Guid applicationId, Guid workflowId, Guid stateId)
        {
            List<Guid> _sIds = new List<Guid>();
            _sIds.Add(stateId);
            return get_workflow_states(applicationId, workflowId, ref _sIds).FirstOrDefault();
        }

        public static State get_first_workflow_state(Guid applicationId, Guid workflowId)
        {
            return DataProvider.GetFirstWorkFlowState(applicationId, workflowId);
        }

        public static List<StateDataNeed> get_state_data_needs(Guid applicationId, Guid workflowId)
        {
            List<StateDataNeed> retList = new List<StateDataNeed>();
            List<Guid> ids = new List<Guid>();
            DataProvider.GetStateDataNeeds(applicationId, ref retList, workflowId, ref ids, true);
            return retList;
        }

        public static List<StateDataNeed> get_state_data_needs(Guid applicationId, 
            Guid workflowId, ref List<Guid> stateIds)
        {
            List<StateDataNeed> retList = new List<StateDataNeed>();
            DataProvider.GetStateDataNeeds(applicationId, ref retList, workflowId, ref stateIds, false);
            return retList;
        }

        public static List<StateDataNeed> get_state_data_needs(Guid applicationId, Guid workflowId, Guid stateId)
        {
            List<Guid> _sIds = new List<Guid>();
            _sIds.Add(stateId);
            return get_state_data_needs(applicationId, workflowId, ref _sIds);
        }

        public static StateDataNeed get_state_data_need(Guid applicationId, 
            Guid workflowId, Guid stateId, Guid nodeTypeId)
        {
            return DataProvider.GetStateDataNeed(applicationId, workflowId, stateId, nodeTypeId);
        }

        public static List<StateDataNeed> get_current_state_data_needs(Guid applicationId, Guid workflowId, Guid stateId)
        {
            List<StateDataNeed> retList = new List<StateDataNeed>();
            DataProvider.GetCurrentStateDataNeeds(applicationId, ref retList, workflowId, stateId);
            return retList;
        }

        public static bool create_state_data_need_instance(Guid applicationId, StateDataNeedInstance instance, 
            ref List<Dashboard> dashboards, ref string errorMessage)
        {
            return DataProvider.CreateStateDataNeedInstance(applicationId, instance, ref dashboards, ref errorMessage);
        }

        public static bool set_state_data_need_instance_as_filled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return DataProvider.SetStateDataNeedInstanceAsFilled(applicationId, instanceId, currentUserId);
        }

        public static bool set_state_data_need_instance_as_not_filled(Guid applicationId, Guid instanceId, 
            Guid currentUserId, ref List<Dashboard> dashboards, ref string errorText)
        {
            return DataProvider.SetStateDataNeedInstanceAsNotFilled(applicationId,
                instanceId, currentUserId, ref dashboards, ref errorText);
        }

        public static bool remove_state_data_need_instance(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteStateDataNeedInstance(applicationId, instanceId, currentUserId);
        }

        public static List<StateDataNeedInstance> get_state_data_need_instances(Guid applicationId, 
            ref List<Guid> historyIds)
        {
            List<StateDataNeedInstance> retList = new List<StateDataNeedInstance>();
            DataProvider.GetStateDataNeedInstances(applicationId, ref retList, ref historyIds);
            return retList;
        }

        public static List<StateDataNeedInstance> get_state_data_need_instances(Guid applicationId, Guid historyId)
        {
            List<Guid> _hIds = new List<Guid>();
            _hIds.Add(historyId);
            return get_state_data_need_instances(applicationId, ref _hIds);
        }

        public static StateDataNeedInstance get_state_data_need_instance(Guid applicationId, Guid instanceId)
        {
            return DataProvider.GetStateDataNeedInstance(applicationId, instanceId);
        }

        public static List<StateConnection> get_workflow_connections(Guid applicationId, Guid workflowId)
        {
            List<StateConnection> retList = new List<StateConnection>();
            List<Guid> ids = new List<Guid>();
            DataProvider.GetWorkFlowConnections(applicationId, ref retList, workflowId, ref ids, true);
            return retList;
        }

        public static List<StateConnection> get_workflow_connections(Guid applicationId, 
            Guid workflowId, ref List<Guid> inStateIds)
        {
            List<StateConnection> retList = new List<StateConnection>();
            DataProvider.GetWorkFlowConnections(applicationId, ref retList, workflowId, ref inStateIds, false);
            return retList;
        }

        public static List<StateConnection> get_workflow_connections(Guid applicationId, Guid workflowId, Guid inStateId)
        {
            List<Guid> _sIds = new List<Guid>();
            _sIds.Add(inStateId);
            return get_workflow_connections(applicationId, workflowId, ref _sIds);
        }

        public static List<StateConnectionForm> get_workflow_connection_forms(Guid applicationId, Guid workflowId)
        {
            List<StateConnectionForm> retList = new List<StateConnectionForm>();
            List<Guid> ids = new List<Guid>();
            DataProvider.GetWorkFlowConnectionForms(applicationId, ref retList, workflowId, ref ids, true);
            return retList;
        }

        public static List<StateConnectionForm> get_workflow_connection_forms(Guid applicationId, 
            Guid workflowId, ref List<Guid> inStateIds)
        {
            List<StateConnectionForm> retList = new List<StateConnectionForm>();
            DataProvider.GetWorkFlowConnectionForms(applicationId, ref retList, workflowId, ref inStateIds, false);
            return retList;
        }

        public static List<StateConnectionForm> get_workflow_connection_forms(Guid applicationId, 
            Guid workflowId, Guid inStateId)
        {
            List<Guid> _sIds = new List<Guid>();
            _sIds.Add(inStateId);
            return get_workflow_connection_forms(applicationId, workflowId, ref _sIds);
        }

        public static List<History> get_owner_history(Guid applicationId, Guid ownerId)
        {
            List<History> retList = new List<History>();
            DataProvider.GetOwnerHistory(applicationId, ref retList, ownerId);
            return retList;
        }

        public static History get_last_history(Guid applicationId, Guid ownerId, Guid? stateId = null, bool done = false)
        {
            History retItem = new History();
            DataProvider.GetLastHistory(applicationId, ref retItem, ownerId, stateId, done);
            return retItem;
        }

        public static Guid? get_last_selected_state_id(Guid applicationId, Guid ownerId, Guid? inStateId = null)
        {
            return DataProvider.GetLastSelectedStateID(applicationId, ownerId, inStateId);
        }

        public static List<History> get_history(Guid applicationId, ref List<Guid> historyIds)
        {
            List<History> retList = new List<History>();
            DataProvider.GetHistory(applicationId, ref retList, ref historyIds);
            return retList;
        }

        public static History get_history(Guid applicationId, Guid historyId)
        {
            List<Guid> _hIds = new List<Guid>();
            _hIds.Add(historyId);
            return get_history(applicationId, ref _hIds).FirstOrDefault();
        }

        public static Guid get_history_owner_id(Guid applicationId, Guid historyId)
        {
            return DataProvider.GetHistoryOwnerID(applicationId, historyId);
        }

        public static Guid create_history_form_instance(Guid applicationId, 
            Guid historyId, Guid outStateId, Guid formId, Guid currentUserId)
        {
            return DataProvider.CreateHistoryFormInstance(applicationId, historyId, outStateId, formId, currentUserId);
        }

        public static List<HistoryFormInstance> get_history_form_instances(Guid applicationId, 
            ref List<Guid> historyIds, bool? selected)
        {
            List<HistoryFormInstance> retList = new List<HistoryFormInstance>();
            DataProvider.GetHistoryFormInstances(applicationId, ref retList, ref historyIds, selected);
            return retList;
        }

        public static List<HistoryFormInstance> get_history_form_instances(Guid applicationId, 
            Guid historyId, bool? selected)
        {
            List<Guid> _hIds = new List<Guid>();
            _hIds.Add(historyId);
            return get_history_form_instances(applicationId, ref _hIds, selected);
        }

        public static bool send_to_next_state(Guid applicationId, 
            History info, bool reject, ref string errorMessage, ref List<Dashboard> dashboards)
        {
            return DataProvider.SendToNextState(applicationId, info, reject, ref errorMessage, ref dashboards);
        }
        
        public static bool terminate_workFlow(Guid applicationId, 
            Guid historyId, string description, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.TerminateWorkFlow(applicationId, 
                historyId, description, currentUserId, ref errorMessage);
        }

        public static ViewerStatus get_viewer_status(Guid applicationId, Guid userId, Guid ownerId)
        {
            return DataProvider.GetViewerStatus(applicationId, userId, ownerId);
        }

        public static bool restart_workFlow(Guid applicationId, Guid ownerId, Guid? directorNodeId,
            Guid? directorUserId, Guid currentUserId, ref List<Dashboard> dashboards, ref string errorMessage)
        {
            return DataProvider.RestartWorkFlow(applicationId, ownerId, directorNodeId, directorUserId,
                currentUserId, ref dashboards, ref errorMessage);
        }

        public static bool has_workflow(Guid applicationId, Guid ownerId)
        {
            return DataProvider.HasWorkFlow(applicationId, ownerId);
        }

        public static bool is_terminated(Guid applicationId, Guid ownerId)
        {
            return DataProvider.IsTerminated(applicationId, ownerId);
        }

        public static List<KeyValuePair<string, int>> get_service_abstract(Guid applicationId, 
            Guid nodeTypeId, Guid? workflowId, Guid? userId, string nullTagLabel)
        {
            List<KeyValuePair<string, int>> retList = new List<KeyValuePair<string, int>>();
            DataProvider.GetServiceAbstract(applicationId, ref retList, nodeTypeId, workflowId, userId, nullTagLabel);
            return retList;
        }

        public static List<Guid> get_service_user_ids(Guid applicationId, Guid? nodeTypeId, Guid? workflowId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetServiceUserIDs(applicationId, ref retList, nodeTypeId, workflowId);
            return retList;
        }

        public static int get_workflow_items_count(Guid applicationId, Guid workflowId)
        {
            return DataProvider.GetWorkFlowItemsCount(applicationId, workflowId);
        }

        public static int get_workflow_state_items_count(Guid applicationId, Guid workflowId, Guid stateId)
        {
            return DataProvider.GetWorkFlowStateItemsCount(applicationId, workflowId, stateId);
        }

        public static List<NodesCount> get_user_workflow_items_count(Guid applicationId, Guid userId, 
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetUserWorkFlowItemsCount(applicationId,
                ref retList, userId, lowerCreationDateLimit, upperCreationDateLimit);
            return retList;
        }

        public static bool add_owner_workflow(Guid applicationId, Guid nodeTypeId, Guid workflowId, Guid currentUserId)
        {
            return DataProvider.AddOwnerWorkFlow(applicationId, nodeTypeId, workflowId, currentUserId);
        }

        public static bool remove_owner_workflow(Guid applicationId, 
            Guid nodeTypeId, Guid workflowId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteOwnerWorkFlow(applicationId, nodeTypeId, workflowId, currentUserId);
        }

        public static List<WorkFlow> get_owner_workflows(Guid applicationId, Guid nodeTypeId)
        {
            List<WorkFlow> retList = new List<WorkFlow>();
            DataProvider.GetOwnerWorkFlows(applicationId, ref retList, nodeTypeId);
            return retList;
        }

        public static Guid? get_owner_workflow_primary_key(Guid applicationId, Guid nodeTypeId, Guid workflowId)
        {
            return DataProvider.GetOwnerWorkFlowPrimaryKey(applicationId, nodeTypeId, workflowId);
        }

        public static List<NodeType> get_workflow_owners(Guid applicationId)
        {
            List<Guid> ids = DataProvider.GetWorkFlowOwnerIDs(applicationId);
            return CNController.get_node_types(applicationId, ids);
        }

        public static Guid? get_form_instance_workflow_owner_id(Guid applicationId, Guid formInstanceId)
        {
            return DataProvider.GetFormInstanceWorkFlowOwnerID(applicationId, formInstanceId);
        }
    }
}
