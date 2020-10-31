using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.WorkFlow
{
    class DataProvider
    {
        private static ModuleIdentifier GetModuleQualifier()
        {
            return ModuleIdentifier.WF;
        }

        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[" + GetModuleQualifier().ToString() + "_" + name + "]"; //'[dbo].' is database owner
        }

        private static void _parse_states(ref IDataReader reader, ref List<State> lstStates)
        {
            while (reader.Read())
            {
                try
                {
                    State state = new State();

                    if (!string.IsNullOrEmpty(reader["StateID"].ToString())) state.StateID = (Guid)reader["StateID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) state.Title = (string)reader["Title"];

                    lstStates.Add(state);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_workflows(ref IDataReader reader, ref List<WorkFlow> lstWorkFlows)
        {
            while (reader.Read())
            {
                try
                {
                    WorkFlow workflow = new WorkFlow();

                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) workflow.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) workflow.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) workflow.Description = (string)reader["Description"];

                    lstWorkFlows.Add(workflow);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_workflow_states(ref IDataReader reader, ref List<State> lstStates)
        {
            while (reader.Read())
            {
                try
                {
                    State state = new State();

                    string strDataNeedsType = string.Empty;
                    string strResponseType = string.Empty;

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) state.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["StateID"].ToString())) state.StateID = (Guid)reader["StateID"];
                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) state.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) state.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["Tag"].ToString())) state.Tag = (string)reader["Tag"];
                    if (!string.IsNullOrEmpty(reader["DataNeedsType"].ToString())) strDataNeedsType = (string)reader["DataNeedsType"];
                    if (!string.IsNullOrEmpty(reader["RefDataNeedsStateID"].ToString())) 
                        state.RefDataNeedsStateID = (Guid)reader["RefDataNeedsStateID"];
                    if (!string.IsNullOrEmpty(reader["DataNeedsDescription"].ToString()))
                        state.DataNeedsDescription = (string)reader["DataNeedsDescription"];
                    if (!string.IsNullOrEmpty(reader["DescriptionNeeded"].ToString()))
                        state.DescriptionNeeded = (bool)reader["DescriptionNeeded"];
                    if (!string.IsNullOrEmpty(reader["HideOwnerName"].ToString())) state.HideOwnerName = (bool)reader["HideOwnerName"];
                    if (!string.IsNullOrEmpty(reader["EditPermission"].ToString())) state.EditPermission = (bool)reader["EditPermission"];
                    if (!string.IsNullOrEmpty(reader["FreeDataNeedRequests"].ToString()))
                        state.FreeDataNeedRequests = (bool)reader["FreeDataNeedRequests"];
                    if (!string.IsNullOrEmpty(reader["ResponseType"].ToString())) strResponseType = (string)reader["ResponseType"];
                    if (!string.IsNullOrEmpty(reader["RefStateID"].ToString())) state.RefStateID = (Guid)reader["RefStateID"];
                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) state.DirectorNode.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) state.DirectorNode.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) state.DirectorNode.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) state.DirectorNode.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["Admin"].ToString())) state.DirectorIsAdmin = (bool)reader["Admin"];
                    if (!string.IsNullOrEmpty(reader["MaxAllowedRejections"].ToString()))
                        state.MaxAllowedRejections = (int)reader["MaxAllowedRejections"];
                    if (!string.IsNullOrEmpty(reader["RejectionTitle"].ToString())) state.RejectionTitle = (string)reader["RejectionTitle"];
                    if (!string.IsNullOrEmpty(reader["RejectionRefStateID"].ToString())) 
                        state.RejectionRefStateID = (Guid)reader["RejectionRefStateID"];
                    if (!string.IsNullOrEmpty(reader["RejectionRefStateTitle"].ToString()))
                        state.RejectionRefStateTitle = (string)reader["RejectionRefStateTitle"];
                    if (!string.IsNullOrEmpty(reader["PollID"].ToString())) state.PollID = (Guid)reader["PollID"];
                    if (!string.IsNullOrEmpty(reader["PollName"].ToString())) state.PollName = (string)reader["PollName"];

                    try { state.DataNeedsType = (StateDataNeedsTypes)Enum.Parse(typeof(StateDataNeedsTypes), strDataNeedsType); }
                    catch { state.DataNeedsType = null; }

                    try { state.ResponseType = (StateResponseTypes)Enum.Parse(typeof(StateResponseTypes), strResponseType); }
                    catch { state.ResponseType = null; }
                    
                    lstStates.Add(state);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_state_data_needs(ref IDataReader reader, ref List<StateDataNeed> lstNeeds)
        {
            while (reader.Read())
            {
                try
                {
                    StateDataNeed need = new StateDataNeed();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) need.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["StateID"].ToString())) need.StateID = (Guid)reader["StateID"];
                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) need.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) need.DirectorNodeType.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) need.DirectorNodeType.Name = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["FormID"].ToString())) need.FormID = (Guid)reader["FormID"];
                    if (!string.IsNullOrEmpty(reader["FormTitle"].ToString())) need.FormTitle = (string)reader["FormTitle"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) need.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["MultiSelect"].ToString())) need.MultiSelect = (bool)reader["MultiSelect"];
                    if (!string.IsNullOrEmpty(reader["Admin"].ToString())) need.Admin = (bool)reader["Admin"];
                    if (!string.IsNullOrEmpty(reader["Necessary"].ToString())) need.Necessary = (bool)reader["Necessary"];

                    lstNeeds.Add(need);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_state_data_need_instances(ref IDataReader reader, ref List<StateDataNeedInstance> lstNeeds)
        {
            while (reader.Read())
            {
                try
                {
                    StateDataNeedInstance need = new StateDataNeedInstance();

                    if (!string.IsNullOrEmpty(reader["InstanceID"].ToString())) need.InstanceID = (Guid)reader["InstanceID"];
                    if (!string.IsNullOrEmpty(reader["HistoryID"].ToString())) need.HistoryID = (Guid)reader["HistoryID"];
                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) need.DirectorNode.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) need.DirectorNode.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) need.DirectorNode.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["Filled"].ToString())) need.Filled = (bool)reader["Filled"];
                    if (!string.IsNullOrEmpty(reader["FillingDate"].ToString())) need.FillingDate = (DateTime)reader["FillingDate"];
                    if (!string.IsNullOrEmpty(reader["AttachmentID"].ToString())) need.AttachmentID = (Guid)reader["AttachmentID"];

                    lstNeeds.Add(need);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_state_connections(ref IDataReader reader, ref List<StateConnection> lstConnections)
        {
            while (reader.Read())
            {
                try
                {
                    StateConnection connection = new StateConnection();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) connection.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) connection.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["InStateID"].ToString())) connection.InState.StateID = (Guid)reader["InStateID"];
                    if (!string.IsNullOrEmpty(reader["OutStateID"].ToString())) connection.OutState.StateID = (Guid)reader["OutStateID"];
                    if (!string.IsNullOrEmpty(reader["SequenceNumber"].ToString())) connection.SequenceNumber = (int)reader["SequenceNumber"];
                    if (!string.IsNullOrEmpty(reader["ConnectionLabel"].ToString())) connection.Label = (string)reader["ConnectionLabel"];
                    if (!string.IsNullOrEmpty(reader["AttachmentRequired"].ToString())) 
                        connection.AttachmentRequired = (bool)reader["AttachmentRequired"];
                    if (!string.IsNullOrEmpty(reader["AttachmentTitle"].ToString()))
                        connection.AttachmentTitle = (string)reader["AttachmentTitle"];
                    if (!string.IsNullOrEmpty(reader["NodeRequired"].ToString())) connection.NodeRequired = (bool)reader["NodeRequired"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) 
                        connection.DirectorNodeType.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) connection.DirectorNodeType.Name = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeDescription"].ToString())) 
                        connection.NodeTypeDescription = (string)reader["NodeTypeDescription"];

                    lstConnections.Add(connection);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_connection_forms(ref IDataReader reader, ref List<StateConnectionForm> lstNeeds)
        {
            while (reader.Read())
            {
                try
                {
                    StateConnectionForm need = new StateConnectionForm();

                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) need.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["InStateID"].ToString())) need.InStateID = (Guid)reader["InStateID"];
                    if (!string.IsNullOrEmpty(reader["OutStateID"].ToString())) need.OutStateID = (Guid)reader["OutStateID"];
                    if (!string.IsNullOrEmpty(reader["FormID"].ToString())) need.Form.FormID = (Guid)reader["FormID"];
                    if (!string.IsNullOrEmpty(reader["FormTitle"].ToString())) need.Form.Title = (string)reader["FormTitle"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) need.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["Necessary"].ToString())) need.Necessary = (bool)reader["Necessary"];

                    lstNeeds.Add(need);
                }
                catch (Exception ex) { string strEx = ex.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_auto_messages(ref IDataReader reader, ref List<AutoMessage> lstAudience)
        {
            while (reader.Read())
            {
                try
                {
                    AutoMessage automessage = new AutoMessage();

                    string strAudienceType = string.Empty;

                    if (!string.IsNullOrEmpty(reader["AutoMessageID"].ToString())) automessage.AutoMessageID = (Guid)reader["AutoMessageID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) automessage.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["BodyText"].ToString())) automessage.BodyText = (string)reader["BodyText"];
                    if (!string.IsNullOrEmpty(reader["AudienceType"].ToString())) strAudienceType = (string)reader["AudienceType"];
                    if (!string.IsNullOrEmpty(reader["RefStateID"].ToString())) automessage.RefState.StateID = (Guid)reader["RefStateID"];
                    if (!string.IsNullOrEmpty(reader["RefStateTitle"].ToString())) automessage.RefState.Title = (string)reader["RefStateTitle"];
                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) automessage.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) automessage.Node.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) automessage.Node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) automessage.Node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["Admin"].ToString())) automessage.Admin = (bool)reader["Admin"];

                    try { automessage.AudienceType = (AudienceTypes)Enum.Parse(typeof(AudienceTypes), strAudienceType.ToString()); }
                    catch { automessage.AudienceType = null; }

                    lstAudience.Add(automessage);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_history(ref IDataReader reader, ref List<History> lstHistory)
        {
            while (reader.Read())
            {
                try
                {
                    History history = new History();

                    if (!string.IsNullOrEmpty(reader["HistoryID"].ToString())) history.HistoryID = (Guid)reader["HistoryID"];
                    if (!string.IsNullOrEmpty(reader["PreviousHistoryID"].ToString())) 
                        history.PreviousHistoryID = (Guid)reader["PreviousHistoryID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) history.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) history.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["DirectorNodeID"].ToString())) 
                        history.DirectorNode.NodeID = (Guid)reader["DirectorNodeID"];
                    if (!string.IsNullOrEmpty(reader["DirectorUserID"].ToString()))
                        history.DirectorUserID = (Guid)reader["DirectorUserID"];
                    if (!string.IsNullOrEmpty(reader["DirectorNodeName"].ToString()))
                        history.DirectorNode.Name = (string)reader["DirectorNodeName"];
                    if (!string.IsNullOrEmpty(reader["DirectorNodeType"].ToString()))
                        history.DirectorNode.NodeType = (string)reader["DirectorNodeType"];
                    if (!string.IsNullOrEmpty(reader["StateID"].ToString())) history.State.StateID = (Guid)reader["StateID"];
                    if (!string.IsNullOrEmpty(reader["StateTitle"].ToString())) history.State.Title = (string)reader["StateTitle"];
                    if (!string.IsNullOrEmpty(reader["SelectedOutStateID"].ToString())) history.SelectedOutStateID = (Guid)reader["SelectedOutStateID"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) history.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) history.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) history.Sender.UserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) history.Sender.FirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) history.Sender.LastName = (string)reader["SenderLastName"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) history.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["PollID"].ToString())) history.PollID = (Guid)reader["PollID"];
                    if (!string.IsNullOrEmpty(reader["PollName"].ToString())) history.PollName = (string)reader["PollName"];

                    lstHistory.Add(history);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_history_form_instances(ref IDataReader reader, ref List<HistoryFormInstance> lstInstances)
        {
            while (reader.Read())
            {
                try
                {
                    HistoryFormInstance instance = new HistoryFormInstance();

                    if (!string.IsNullOrEmpty(reader["HistoryID"].ToString())) instance.HistoryID = (Guid)reader["HistoryID"];
                    if (!string.IsNullOrEmpty(reader["OutStateID"].ToString())) instance.OutStateID = (Guid)reader["OutStateID"];
                    if (!string.IsNullOrEmpty(reader["FormsID"].ToString())) instance.FormsID = (Guid)reader["FormsID"];

                    lstInstances.Add(instance);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_workflow_nodes(ref IDataReader reader, ref List<WorkFlowNode> workflowNodes)
        {
            while (reader.Read())
            {
                try
                {
                    WorkFlowNode wfNode = new WorkFlowNode();

                    if (!string.IsNullOrEmpty(reader["StateID"].ToString())) wfNode.State.StateID = (Guid)reader["StateID"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) wfNode.State.Title = (string)reader["Status"];
                    if (!string.IsNullOrEmpty(reader["Tag"].ToString())) wfNode.State.Tag = (string)reader["Tag"];
                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) wfNode.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) wfNode.Node.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeCreatorUserID"].ToString()))
                        wfNode.Node.Creator.UserID = (Guid)reader["NodeCreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["NodeCreatorUserName"].ToString()))
                        wfNode.Node.Creator.UserName = (string)reader["NodeCreatorUserName"];
                    if (!string.IsNullOrEmpty(reader["NodeCreatorFirstName"].ToString()))
                        wfNode.Node.Creator.FirstName = (string)reader["NodeCreatorFirstName"];
                    if (!string.IsNullOrEmpty(reader["NodeCreatorLastName"].ToString()))
                        wfNode.Node.Creator.LastName = (string)reader["NodeCreatorLastName"];

                    workflowNodes.Add(wfNode);
                }
                catch { }
            }

            reader.NextResult();

            while (reader.Read())
            {
                try
                {
                    WorkFlowNode _wfNode = new WorkFlowNode();

                    if (!string.IsNullOrEmpty(reader["StateID"].ToString())) _wfNode.State.StateID = (Guid)reader["StateID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) _wfNode.State.Title = (string)reader["Title"];

                    workflowNodes.Add(_wfNode);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_workflow_dashboards(ref IDataReader reader, ref List<WFDashboard> dashboards)
        {
            while (reader.Read())
            {
                try
                {
                    WFDashboard _dash = new WFDashboard();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) _dash.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) _dash.NodeName = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) _dash.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["InstanceID"].ToString())) _dash.InstanceID = (Guid)reader["InstanceID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString())) _dash.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["StateTitle"].ToString())) _dash.StateTitle = (string)reader["StateTitle"];

                    dashboards.Add(_dash);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_items_count(ref IDataReader reader, ref List<NodesCount> items)
        {
            while (reader.Read())
            {
                try
                {
                    NodesCount _item = new NodesCount();

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) _item.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) _item.TypeName = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["Count"].ToString())) _item.Count = (int)reader["Count"];

                    items.Add(_item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_tags(ref IDataReader reader, ref List<Tag> tags)
        {
            while (reader.Read())
            {
                try
                {
                    Tag _item = new Tag();

                    if (!string.IsNullOrEmpty(reader["TagID"].ToString())) _item.TagID = (Guid)reader["TagID"];
                    if (!string.IsNullOrEmpty(reader["Tag"].ToString())) _item.Text = (string)reader["Tag"];

                    tags.Add(_item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }


        public static bool CreateState(Guid applicationId, State Info)
        {
            string spName = GetFullyQualifiedName("CreateState");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.StateID, Info.Title, Info.CreatorUserID, Info.CreationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ModifyState(Guid applicationId, State Info)
        {
            string spName = GetFullyQualifiedName("ModifyState");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.StateID, Info.Title, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteState(Guid applicationId, Guid stateId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteState");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    stateId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetStates(Guid applicationId, ref List<State> retStates, Guid? workflowId)
        {
            string spName = GetFullyQualifiedName("GetStates");

            try
            {
                if (workflowId == Guid.Empty) workflowId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId);
                _parse_states(ref reader, ref retStates);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetStates(Guid applicationId, ref List<State> retStates, ref List<Guid> stateIds)
        {
            string spName = GetFullyQualifiedName("GetStatesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref stateIds), ',');
                _parse_states(ref reader, ref retStates);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static bool CreateWorkFlow(Guid applicationId, WorkFlow Info)
        {
            string spName = GetFullyQualifiedName("CreateWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.Name, Info.Description, Info.CreatorUserID, Info.CreationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ModifyWorkFlow(Guid applicationId, WorkFlow Info)
        {
            string spName = GetFullyQualifiedName("ModifyWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.Name, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteWorkFlow(Guid applicationId, Guid workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetWorkFlows(Guid applicationId, ref List<WorkFlow> retWorkFlows)
        {
            string spName = GetFullyQualifiedName("GetWorkFlows");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                _parse_workflows(ref reader, ref retWorkFlows);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetWorkFlows(Guid applicationId, 
            ref List<WorkFlow> retWorkFlows, ref List<Guid> workflowIds)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref workflowIds), ',');
                _parse_workflows(ref reader, ref retWorkFlows);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static bool AddWorkFlowState(Guid applicationId, 
            Guid? id, Guid workflowId, Guid stateId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddWorkFlowState");

            try
            {
                if (id == Guid.Empty) id = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, workflowId, stateId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteWorkFlowState(Guid applicationId, 
            Guid workflowId, Guid stateId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteWorkFlowState");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetWorkFlowStateDescription(Guid applicationId, State info)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowStateDescription");

            try
            {
                if (!info.LastModificationDate.HasValue) info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    info.WorkFlowID, info.StateID, info.Description, info.LastModifierUserID, info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetWorkFlowStateTag(Guid applicationId, 
            Guid workflowId, Guid stateId, string tag, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowStateTag");

            try
            {
                if (string.IsNullOrEmpty(tag)) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, tag, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool RemoveWorkFlowStateTag(Guid applicationId, Guid workflowId, Guid stateId)
        {
            string spName = GetFullyQualifiedName("RemoveWorkFlowStateTag");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, workflowId, stateId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetWorkFlowTags(Guid applicationId, ref List<Tag> retTags, Guid workflowId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowTags");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId);
                _parse_tags(ref reader, ref retTags);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static bool SetStateDirector(Guid applicationId, State Info)
        {
            string spName = GetFullyQualifiedName("SetStateDirector");

            try
            {
                if (Info.RefStateID == Guid.Empty) Info.RefStateID = null;
                if (Info.DirectorNode.NodeID == Guid.Empty) Info.DirectorNode.NodeID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.StateID, Info.ResponseType, Info.RefStateID, Info.DirectorNode.NodeID, 
                    Info.DirectorIsAdmin, Info.CreatorUserID, Info.CreationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStatePoll(Guid applicationId, Guid workflowId, Guid stateId, Guid? pollId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStatePoll");

            try
            {
                if (pollId == Guid.Empty) pollId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, pollId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateDataNeedsType(Guid applicationId, Guid workflowId, Guid stateId, 
            StateDataNeedsTypes? dataNeedsType, Guid? refStateId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStateDataNeedsType");

            try
            {
                string strDataNeedsType = null;
                if (dataNeedsType.HasValue) strDataNeedsType = dataNeedsType.Value.ToString();

                if (refStateId == Guid.Empty) refStateId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, strDataNeedsType, refStateId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateDataNeedsDescription(Guid applicationId, 
            Guid workflowId, Guid stateId, string description, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStateDataNeedsDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, description, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateDescriptionNeeded(Guid applicationId, 
            Guid workflowId, Guid stateId, bool descriptionNeeded, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStateDescriptionNeeded");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, descriptionNeeded, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateHideOwnerName(Guid applicationId, 
            Guid workflowId, Guid stateId, bool hideOwnerName, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStateHideOwnerName");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, hideOwnerName, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateEditPermisison(Guid applicationId, 
            Guid workflowId, Guid stateId, bool editPermission, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStateEditPermission");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, editPermission, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetFreeDataNeedRequests(Guid applicationId, 
            Guid workflowId, Guid stateId, bool freeDataNeedRequests, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetFreeDataNeedRequests");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, stateId, freeDataNeedRequests, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateDataNeed(Guid applicationId, StateDataNeed Info, Guid? previousNodeTypeId)
        {
            string spName = GetFullyQualifiedName("SetStateDataNeed");

            try
            {
                if (previousNodeTypeId == Guid.Empty) previousNodeTypeId = null;
                if (Info.FormID == Guid.Empty) Info.FormID = null;
                if (!Info.CreationDate.HasValue) Info.CreationDate = DateTime.Now;

                if (Info.DirectorNodeType.NodeTypeID == Guid.Empty) Info.DirectorNodeType.NodeTypeID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.ID, 
                    Info.WorkFlowID, Info.StateID, Info.DirectorNodeType.NodeTypeID, previousNodeTypeId, 
                    Info.FormID, Info.Description, Info.MultiSelect, Info.Admin, Info.Necessary, 
                    Info.CreatorUserID, Info.CreationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteStateDataNeed(Guid applicationId, StateDataNeed Info)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteStateDataNeed");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.WorkFlowID, 
                    Info.StateID, Info.DirectorNodeType.NodeTypeID, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetRejectionSettings(Guid applicationId, State Info)
        {
            string spName = GetFullyQualifiedName("SetRejectionSettings");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;
                if (Info.RejectionRefStateID == Guid.Empty) Info.RejectionRefStateID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.StateID, Info.MaxAllowedRejections, Info.RejectionTitle,
                    Info.RejectionRefStateID, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetMaxAllowedRejections(Guid applicationId, State Info)
        {
            string spName = GetFullyQualifiedName("SetMaxAllowedRejections");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.WorkFlowID, 
                    Info.StateID, Info.MaxAllowedRejections, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static int GetRejectionsCount(Guid applicationId, Guid ownerId, Guid workflowId, Guid stateId)
        {
            string spName = GetFullyQualifiedName("GetRejectionsCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, workflowId, stateId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return 0;
            }
        }

        public static Guid? AddStateConnection(Guid applicationId, StateConnection Info)
        {
            string spName = GetFullyQualifiedName("AddStateConnection");

            try
            {
                if (!Info.CreationDate.HasValue) Info.CreationDate = DateTime.Now;

                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.InState.StateID, Info.OutState.StateID, Info.CreatorUserID, Info.CreationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return Guid.Empty;
            }
        }

        public static bool SortStateConnections(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("SortStateConnections");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool MoveStateConnection(Guid applicationId, StateConnection Info, bool moveDown)
        {
            string spName = GetFullyQualifiedName("MoveStateConnection");

            try
            {
                if (!Info.CreationDate.HasValue) Info.CreationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.InState.StateID, Info.OutState.StateID, moveDown));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteStateConnection(Guid applicationId, StateConnection Info)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteStateConnection");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.WorkFlowID, 
                    Info.InState.StateID, Info.OutState.StateID, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateConnectionLabel(Guid applicationId, StateConnection Info)
        {
            string spName = GetFullyQualifiedName("SetStateConnectionLabel");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.InState.StateID, Info.OutState.StateID, Info.Label,
                    Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateConnectionAttachmentStatus(Guid applicationId, StateConnection Info)
        {
            string spName = GetFullyQualifiedName("SetStateConnectionAttachmentStatus");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.WorkFlowID, 
                    Info.InState.StateID, Info.OutState.StateID, Info.AttachmentRequired, Info.AttachmentTitle, 
                    Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateConnectionDirector(Guid applicationId, StateConnection Info)
        {
            string spName = GetFullyQualifiedName("SetStateConnectionDirector");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                if (Info.DirectorNodeType.NodeTypeID == Guid.Empty) Info.DirectorNodeType.NodeTypeID = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.WorkFlowID, 
                    Info.InState.StateID, Info.OutState.StateID, Info.NodeRequired, Info.DirectorNodeType.NodeTypeID,
                    Info.NodeTypeDescription, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateConnectionForm(Guid applicationId, StateConnectionForm Info)
        {
            string spName = GetFullyQualifiedName("SetStateConnectionForm");

            try
            {
                if (!Info.CreationDate.HasValue) Info.CreationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.WorkFlowID, Info.InStateID, Info.OutStateID, Info.Form.FormID, Info.Description, Info.Necessary,
                    Info.CreatorUserID, Info.CreationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteStateConnectionForm(Guid applicationId, StateConnectionForm Info)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteStateConnectionForm");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.WorkFlowID, 
                    Info.InStateID, Info.OutStateID, Info.Form.FormID, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool AddAutoMessage(Guid applicationId, AutoMessage Info)
        {
            string spName = GetFullyQualifiedName("AddAutoMessage");

            try
            {
                if (!Info.CreationDate.HasValue) Info.CreationDate = DateTime.Now;

                if (Info.RefState.StateID == Guid.Empty) Info.RefState.StateID = null;
                if (Info.Node.NodeID == Guid.Empty) Info.Node.NodeID = null;

                string strAudienceType = null;
                if (Info.AudienceType.HasValue) strAudienceType = Info.AudienceType.Value.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.AutoMessageID, 
                    Info.OwnerID, Info.BodyText, strAudienceType, Info.RefState.StateID, Info.Node.NodeID, Info.Admin,
                    Info.CreatorUserID, Info.CreationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ModifyAutoMessage(Guid applicationId, AutoMessage Info)
        {
            string spName = GetFullyQualifiedName("ModifyAutoMessage");

            try
            {
                if (!Info.LastModificationDate.HasValue) Info.LastModificationDate = DateTime.Now;

                if (Info.RefState.StateID == Guid.Empty) Info.RefState.StateID = null;
                if (Info.Node.NodeID == Guid.Empty) Info.Node.NodeID = null;

                string strAudienceType = null;
                if (Info.AudienceType.HasValue) strAudienceType = Info.AudienceType.Value.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.AutoMessageID, 
                    Info.BodyText, strAudienceType, Info.RefState.StateID, Info.Node.NodeID, Info.Admin,
                    Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteAutoMessage(Guid applicationId, Guid automessageId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteAutoMessage");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    automessageId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetOwnerAutoMessages(Guid applicationId, 
            ref List<AutoMessage> retAudience, ref List<Guid> ownerIds)
        {
            string spName = GetFullyQualifiedName("GetOwnerAutoMessages");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref ownerIds), ',');
                _parse_auto_messages(ref reader, ref retAudience);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetWorkFlowAutoMessages(Guid applicationId, 
            ref List<AutoMessage> retAudience, Guid workflowId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowAutoMessages");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId);
                _parse_auto_messages(ref reader, ref retAudience);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetConnectionAutoMessages(Guid applicationId, 
            ref List<AutoMessage> retAudience, Guid workflowId, Guid inStateId, Guid outStateId)
        {
            string spName = GetFullyQualifiedName("GetConnectionAutoMessages");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, inStateId, outStateId);
                _parse_auto_messages(ref reader, ref retAudience);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetWorkFlowStates(Guid applicationId, 
            ref List<State> retStates, Guid workflowId, ref List<Guid> stateIds, bool? all)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowStates");

            try
            {
                string strStateIds = null;
                if (!all.HasValue || !all.Value) strStateIds = ProviderUtil.list_to_string<Guid>(ref stateIds);


                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, strStateIds, ',');
                _parse_workflow_states(ref reader, ref retStates);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static State GetFirstWorkFlowState(Guid applicationId, Guid workflowId)
        {
            string spName = GetFullyQualifiedName("GetFirstWorkFlowState");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId);
                List<State> lst = new List<State>();
                _parse_workflow_states(ref reader, ref lst);
                return lst == null || lst.Count != 1 ? null : lst.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }

        public static void GetStateDataNeeds(Guid applicationId, 
            ref List<StateDataNeed> retNeeds, Guid workflowId, ref List<Guid> stateIds, bool? all)
        {
            string spName = GetFullyQualifiedName("GetStateDataNeeds");

            try
            {
                string strStateIds = null;

                if (!all.HasValue || !all.Value) strStateIds = ProviderUtil.list_to_string<Guid>(ref stateIds);

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, strStateIds, ',');
                _parse_state_data_needs(ref reader, ref retNeeds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static StateDataNeed GetStateDataNeed(Guid applicationId, 
            Guid workflowId, Guid stateId, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetStateDataNeed");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, stateId, nodeTypeId);
                List<StateDataNeed> needs = new List<StateDataNeed>();
                _parse_state_data_needs(ref reader, ref needs);
                return needs.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }

        public static void GetCurrentStateDataNeeds(Guid applicationId, 
            ref List<StateDataNeed> retNeeds, Guid workflowId, Guid stateId)
        {
            string spName = GetFullyQualifiedName("GetCurrentStateDataNeeds");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, stateId);
                _parse_state_data_needs(ref reader, ref retNeeds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static bool CreateStateDataNeedInstance(Guid applicationId, StateDataNeedInstance instance, 
            ref List<Dashboard> dashboards, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("CreateStateDataNeedInstance");

            try
            {
                if (instance.FormID == Guid.Empty) instance.FormID = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, instance.InstanceID, 
                    instance.HistoryID, instance.DirectorNode.NodeID, instance.Admin, instance.FormID,
                    instance.CreatorUserID, instance.CreationDate);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref errorMessage) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateDataNeedInstanceAsFilled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetStateDataNeedInstanceAsFilled");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    instanceId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool SetStateDataNeedInstanceAsNotFilled(Guid applicationId, Guid instanceId, Guid currentUserId,
            ref List<Dashboard> dashboards, ref string errorText)
        {
            string spName = GetFullyQualifiedName("SetStateDataNeedInstanceAsNotFilled");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    instanceId, currentUserId, DateTime.Now);
                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref errorText) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteStateDataNeedInstance(Guid applicationId, 
            Guid instanceId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteStateDataNeedInstance");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    instanceId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetStateDataNeedInstances(Guid applicationId, 
            ref List<StateDataNeedInstance> retNeeds, ref List<Guid> historyIds)
        {
            string spName = GetFullyQualifiedName("GetStateDataNeedInstances");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref historyIds), ',');
                _parse_state_data_need_instances(ref reader, ref retNeeds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static StateDataNeedInstance GetStateDataNeedInstance(Guid applicationId, Guid instanceId)
        {
            string spName = GetFullyQualifiedName("GetStateDataNeedInstance");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, instanceId);
                List<StateDataNeedInstance> insts = new List<StateDataNeedInstance>();
                _parse_state_data_need_instances(ref reader, ref insts);
                return insts.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }

        public static void GetWorkFlowConnections(Guid applicationId, ref List<StateConnection> retConnections, 
            Guid workflowId, ref List<Guid> inStateIds, bool? all)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowConnections");

            try
            {
                string strStateIds = null;

                if (!all.HasValue || !all.Value) strStateIds = ProviderUtil.list_to_string<Guid>(ref inStateIds);

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, strStateIds, ',');
                _parse_state_connections(ref reader, ref retConnections);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetWorkFlowConnectionForms(Guid applicationId, ref List<StateConnectionForm> retNeeds, 
            Guid workflowId, ref List<Guid> inStateIds, bool? all)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowConnectionForms");

            try
            {
                string strStateIds = null;
                if (!all.HasValue || !all.Value)
                    strStateIds = ProviderUtil.list_to_string<Guid>(ref inStateIds);

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowId, strStateIds, ',');
                _parse_connection_forms(ref reader, ref retNeeds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetOwnerHistory(Guid applicationId, ref List<History> retHistory, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetHistory");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                _parse_history(ref reader, ref retHistory);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetLastHistory(Guid applicationId, ref History retHistory, Guid ownerId, Guid? stateId, bool done)
        {
            string spName = GetFullyQualifiedName("GetLastHistory");

            try
            {
                List<History> retList = new List<History>();
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId, stateId, done);
                _parse_history(ref reader, ref retList);
                retHistory = retList.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static Guid? GetLastSelectedStateID(Guid applicationId, Guid ownerId, Guid? inStateId)
        {
            string spName = GetFullyQualifiedName("GetLastSelectedStateID");

            try
            {
                List<History> retList = new List<History>();
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, ownerId, inStateId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }

        public static void GetHistory(Guid applicationId, ref List<History> retHistory, ref List<Guid> historyIds)
        {
            string spName = GetFullyQualifiedName("GetHistoryByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref historyIds), ',');
                _parse_history(ref reader, ref retHistory);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static Guid GetHistoryOwnerID(Guid applicationId, Guid historyId)
        {
            string spName = GetFullyQualifiedName("GetHistoryOwnerID");

            try
            {
                List<Guid> retList = new List<Guid>();
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, historyId);
                ProviderUtil.parse_guids(ref reader, ref retList);
                return retList.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return Guid.Empty;
            }
        }

        public static Guid CreateHistoryFormInstance(Guid applicationId, 
            Guid historyId, Guid outStateId, Guid formId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("CreateHistoryFormInstance");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    historyId, outStateId, formId, currentUserId, DateTime.Now);
                List<Guid> ids = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ids);
                return ids.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return Guid.Empty;
            }
        }

        public static void GetHistoryFormInstances(Guid applicationId, 
            ref List<HistoryFormInstance> retInstances, ref List<Guid> historyIds, bool? selected)
        {
            string spName = GetFullyQualifiedName("GetHistoryFormInstances");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref historyIds), ',', selected);
                _parse_history_form_instances(ref reader, ref retInstances);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetDirectorIDs(Guid applicationId, ref List<Guid> retIds, Guid ownerId)
        {

        }

        public static bool SendToNextState(Guid applicationId, 
            History Info, bool reject, ref string errorMessage, ref List<Dashboard> dashboards)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            DataTable attachmentsTable = new DataTable();
            attachmentsTable.Columns.Add("FileID", typeof(Guid));
            attachmentsTable.Columns.Add("FileName", typeof(string));
            attachmentsTable.Columns.Add("Extension", typeof(string));
            attachmentsTable.Columns.Add("MIME", typeof(string));
            attachmentsTable.Columns.Add("Size", typeof(long));
            attachmentsTable.Columns.Add("OwnerID", typeof(Guid));
            attachmentsTable.Columns.Add("OwnerType", typeof(string));

            foreach (DocFileInfo _att in Info.AttachedFiles)
            {
                attachmentsTable.Rows.Add(_att.FileID, _att.FileName, 
                    _att.Extension, _att.MIME(), _att.Size, _att.OwnerID, _att.OwnerType);
            }

            SqlParameter attachmentsParam = new SqlParameter("@AttachedFiles", SqlDbType.Structured);
            attachmentsParam.TypeName = "[dbo].[DocFileInfoTableType]";
            attachmentsParam.Value = attachmentsTable;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@HistoryID", Info.HistoryID);
            cmd.Parameters.AddWithValue("@StateID", Info.State.StateID);
            if (Info.DirectorNode.NodeID.HasValue) cmd.Parameters.AddWithValue("@DirectorNodeID", Info.DirectorNode.NodeID);
            if (Info.DirectorUserID.HasValue) cmd.Parameters.AddWithValue("@DirectorUserID", Info.DirectorUserID);
            cmd.Parameters.AddWithValue("@Description", Info.Description);
            cmd.Parameters.AddWithValue("@Reject", reject);
            cmd.Parameters.AddWithValue("@CreatorUserID", Info.Sender.UserID);
            cmd.Parameters.AddWithValue("@CreationDate", Info.SendDate);
            cmd.Parameters.Add(attachmentsParam);

            string spName = GetFullyQualifiedName("SendToNextState");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@HistoryID" + sep + "@StateID" + sep +
                (Info.DirectorNode.NodeID.HasValue ? "@DirectorNodeID" : "null") + sep +
                (Info.DirectorUserID.HasValue ? "@DirectorUserID" : "null") + sep + "@Description" + sep +
                "@Reject" + sep + "@CreatorUserID" + sep + "@CreationDate" + sep + "@AttachedFiles";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref errorMessage) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool TerminateWorkFlow(Guid applicationId, 
            Guid historyId, string description, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("TerminateWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    historyId, description, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static ViewerStatus GetViewerStatus(Guid applicationId, Guid userId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetViewerStatus");

            try
            {
                string status = ProviderUtil.succeed_string(ProviderUtil.execute_reader(spName, applicationId,
                    userId, ownerId));
                return (ViewerStatus)Enum.Parse(typeof(ViewerStatus), status);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return ViewerStatus.None;
            }
        }

        public static bool RestartWorkFlow(Guid applicationId, Guid ownerId, Guid? directorNodeId,
            Guid? directorUserId, Guid currentUserId, ref List<Dashboard> dashboards, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("RestartWorkFlow");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, directorNodeId, directorUserId, currentUserId, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref errorMessage) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool HasWorkFlow(Guid applicationId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("HasWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, ownerId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool IsTerminated(Guid applicationId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("IsTerminated");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, ownerId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetServiceAbstract(Guid applicationId, ref List<KeyValuePair<string, int>> retList, 
            Guid nodeTypeId, Guid? workflowId, Guid? userId, string nullTagLabel)
        {
            string spName = GetFullyQualifiedName("GetServiceAbstract");
            
            try
            {
                IDataReader reader = (ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, nodeTypeId, userId, nullTagLabel));
                
                while (reader.Read())
                {
                    try
                    {
                        string tag = string.Empty;
                        int count = 0;

                        if (!string.IsNullOrEmpty(reader["Tag"].ToString())) tag = (string)reader["Tag"];
                        if (!string.IsNullOrEmpty(reader["Count"].ToString())) count = (int)reader["Count"];

                        retList.Add(new KeyValuePair<string, int>(tag, count));
                    }
                    catch { }
                }

                if (!reader.IsClosed) reader.Close();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static void GetServiceUserIDs(Guid applicationId, 
            ref List<Guid> retList, Guid? nodeTypeId, Guid? workflowId)
        {
            string spName = GetFullyQualifiedName("GetServiceUserIDs");

            try
            {
                IDataReader reader = (ProviderUtil.execute_reader(spName, applicationId, workflowId, nodeTypeId));
                ProviderUtil.parse_guids(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static int GetWorkFlowItemsCount(Guid applicationId, Guid workflowId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowItemsCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, workflowId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return 0;
            }
        }

        public static int GetWorkFlowStateItemsCount(Guid applicationId, Guid workflowId, Guid stateId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowStateItemsCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, workflowId, stateId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return 0;
            }
        }

        public static void GetUserWorkFlowItemsCount(Guid applicationId, ref List<NodesCount> lstWFNodes, Guid userId,
            DateTime? lowerCreationDateLimit, DateTime? upperCreationDateLimit)
        {
            string spName = GetFullyQualifiedName("GetUserWorkFlowItemsCount");

            try
            {
                IDataReader reader = (ProviderUtil.execute_reader(spName, applicationId,
                    userId, lowerCreationDateLimit, upperCreationDateLimit));
                _parse_items_count(ref reader, ref lstWFNodes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static bool AddOwnerWorkFlow(Guid applicationId, 
            Guid nodeTypeId, Guid workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddOwnerWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static bool ArithmeticDeleteOwnerWorkFlow(Guid applicationId, 
            Guid nodeTypeId, Guid workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteOwnerWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return false;
            }
        }

        public static void GetOwnerWorkFlows(Guid applicationId, ref List<WorkFlow> lstWorkFlows, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetOwnerWorkFlows");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeId);
                _parse_workflows(ref reader, ref lstWorkFlows);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
            }
        }

        public static Guid? GetOwnerWorkFlowPrimaryKey(Guid applicationId, Guid nodeTypeId, Guid workflowId)
        {
            string spName = GetFullyQualifiedName("GetOwnerWorkFlowPrimaryKey");

            try
            {
                Guid? retVal = ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId,
                    nodeTypeId, workflowId));
                if (retVal == Guid.Empty) retVal = null;
                return retVal;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }

        public static List<Guid> GetWorkFlowOwnerIDs(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowOwnerIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                List<Guid> nodeTypeIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref nodeTypeIds);
                return nodeTypeIds;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return new List<Guid>();
            }
        }

        public static Guid? GetFormInstanceWorkFlowOwnerID(Guid applicationId, Guid formInstanceId)
        {
            string spName = GetFullyQualifiedName("GetFormInstanceWorkFlowOwnerID");

            try
            {
                Guid? ownerId = ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId,
                    formInstanceId));

                if (!ownerId.HasValue || ownerId == Guid.Empty) return null;
                else return ownerId.Value;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WF);
                return null;
            }
        }
    }
}
