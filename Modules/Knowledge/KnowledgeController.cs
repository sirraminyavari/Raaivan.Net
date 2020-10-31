using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Documents;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Knowledge
{
    public static class KnowledgeController
    {
        public static bool initialize(Guid applicationId)
        {
            return DataProvider.Initialize(applicationId);
        }

        public static bool add_knowledge_type(Guid applicationId, Guid knowledgeTypeId, Guid currentUserId)
        {
            return DataProvider.AddKnowledgeType(applicationId, knowledgeTypeId, currentUserId);
        }

        public static bool remove_knowledge_type(Guid applicationId, Guid knowledgeTypeId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteKnowledgeType(applicationId, knowledgeTypeId, currentUserId);
        }

        private static List<KnowledgeType> _get_knowledge_types(Guid applicationId, ref List<Guid> knowledgeTypeIds)
        {
            List<KnowledgeType> retList = new List<KnowledgeType>();
            DataProvider.GetKnowledgeTypes(applicationId, ref retList, ref knowledgeTypeIds);
            return retList;
        }

        public static List<KnowledgeType> get_knowledge_types(Guid applicationId)
        {
            List<Guid> knowledgeTypeIds = new List<Guid>();
            return _get_knowledge_types(applicationId, ref knowledgeTypeIds);
        }

        public static List<KnowledgeType> get_knowledge_types(Guid applicationId, List<Guid> knowledgeTypeIds)
        {
            return _get_knowledge_types(applicationId, ref knowledgeTypeIds);
        }

        public static KnowledgeType get_knowledge_type(Guid applicationId, Guid knowledgeTypeIdOrKnowledgeId)
        {
            List<Guid> knowledgeTypeIds = new List<Guid>();
            knowledgeTypeIds.Add(knowledgeTypeIdOrKnowledgeId);
            return _get_knowledge_types(applicationId, ref knowledgeTypeIds).FirstOrDefault();
        }

        public static bool set_evaluation_type(Guid applicationId, 
            Guid knowledgeTypeId, KnowledgeEvaluationType evaluationType)
        {
            return DataProvider.SetEvaluationType(applicationId, knowledgeTypeId, evaluationType);
        }

        public static bool set_evaluators(Guid applicationId, 
            Guid knowledgeTypeId, KnowledgeEvaluators evaluators, int? minEvaluationsCount)
        {
            return DataProvider.SetEvaluators(applicationId, knowledgeTypeId, evaluators, minEvaluationsCount);
        }

        public static bool set_pre_evaluate_by_owner(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetPreEvaluateByOwner(applicationId, knowledgeTypeId, value);
        }

        public static bool set_force_evaluators_describe(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetForceEvaluatorsDescribe(applicationId, knowledgeTypeId, value);
        }

        public static bool set_node_select_type(Guid applicationId, 
            Guid knowledgeTypeId, KnowledgeNodeSelectType nodeSelectType)
        {
            return DataProvider.SetNodeSelectType(applicationId, knowledgeTypeId, nodeSelectType);
        }

        public static bool set_submission_type(Guid applicationId, Guid knowledgeTypeId, SubmissionType submissionType)
        {
            return DataProvider.SetSubmissionType(applicationId, knowledgeTypeId, submissionType);
        }

        public static bool set_searchability_type(Guid applicationId, Guid knowledgeTypeId, SearchableAfter searchableAfter)
        {
            return DataProvider.SetSearchabilityType(applicationId, knowledgeTypeId, searchableAfter);
        }

        public static bool set_score_scale(Guid applicationId, Guid knowledgeTypeId, int? scoreScale)
        {
            return DataProvider.SetScoreScale(applicationId, knowledgeTypeId, scoreScale);
        }

        public static bool set_min_acceptable_score(Guid applicationId, Guid knowledgeTypeId, double? minAcceptableScore)
        {
            return DataProvider.SetMinAcceptableScore(applicationId, knowledgeTypeId, minAcceptableScore);
        }

        public static bool set_convert_evaluators_to_experts(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetConvertEvaluatorsToExperts(applicationId, knowledgeTypeId, value);
        }

        public static bool set_evaluations_editable(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetEvaluationsEditable(applicationId, knowledgeTypeId, value);
        }

        public static bool set_evaluations_editable_for_admin(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetEvaluationsEditableForAdmin(applicationId, knowledgeTypeId, value);
        }

        public static bool set_evaluations_removable(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetEvaluationsRemovable(applicationId, knowledgeTypeId, value);
        }

        public static bool set_unhide_evaluators(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetUnhideEvaluators(applicationId, knowledgeTypeId, value);
        }

        public static bool set_unhide_evaluations(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetUnhideEvaluations(applicationId, knowledgeTypeId, value);
        }

        public static bool set_unhide_node_creators(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            return DataProvider.SetUnhideNodeCreators(applicationId, knowledgeTypeId, value);
        }

        public static bool set_text_options(Guid applicationId, Guid knowledgeTypeId, string value)
        {
            return DataProvider.SetTextOptions(applicationId, knowledgeTypeId, value);
        }

        public static bool set_candidate_relations(Guid applicationId, Guid knowledgeTypeId, 
            ref List<Guid> nodeTypeIds, ref List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.SetCandidateRelations(applicationId,
                knowledgeTypeId, ref nodeTypeIds, ref nodeIds, currentUserId);
        }

        public static List<Node> get_candidate_node_relations(Guid applicationId, Guid knowledgeTypeIdOrKnowledgeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetCandidateNodeRelations(applicationId, ref retList, knowledgeTypeIdOrKnowledgeId);
            return retList;
        }

        public static List<NodeType> get_candidate_node_type_relations(Guid applicationId, 
            Guid knowledgeTypeIdOrKnowledgeId)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetCandidateNodeTypeRelations(applicationId, ref retList, knowledgeTypeIdOrKnowledgeId);
            return retList;
        }

        public static bool add_question(Guid applicationId, KnowledgeTypeQuestion question, ref string errorMessage)
        {
            return DataProvider.AddQuestion(applicationId, question, ref errorMessage);
        }

        public static bool modify_question(Guid applicationId, KnowledgeTypeQuestion question)
        {
            return DataProvider.ModifyQuestion(applicationId, question);
        }

        public static bool set_questions_order(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.SetQuestionsOrder(applicationId, ids);
        }
        
        public static bool set_question_weight(Guid applicationId, Guid id, double weight, ref string errorMessage)
        {
            return DataProvider.SetQuestionWeight(applicationId, id, weight, ref errorMessage);
        }

        public static bool remove_question(Guid applicationId, Guid id, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteQuestion(applicationId, id, currentUserId);
        }

        public static bool remove_related_node_questions(Guid applicationId, 
            Guid knowledgeTypeId, Guid nodeId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteRelatedNodeQuestions(applicationId,
                knowledgeTypeId, nodeId, currentUserId);
        }

        public static bool add_answer_option(Guid applicationId, Guid id,
            Guid typeQuestionId, string title, double value, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.AddAnswerOption(applicationId, id, typeQuestionId, 
                title, value, currentUserId, ref errorMessage);
        }

        public static bool modify_answer_option(Guid applicationId, Guid id,
            string title, double value, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.ModifyAnswerOption(applicationId, id, title, value, currentUserId, ref errorMessage);
        }

        public static bool set_answer_options_order(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.SetAnswerOptionsOrder(applicationId, ids);
        }
        
        public static bool remove_answer_option(Guid applicationId, Guid id, Guid currentUserId)
        {
            return DataProvider.RemoveAnswerOption(applicationId, id, currentUserId);
        }

        public static List<KnowledgeTypeQuestion> get_questions(Guid applicationId, Guid knowledgeTypeId)
        {
            List<KnowledgeTypeQuestion> retList = new List<KnowledgeTypeQuestion>();
            DataProvider.GetQuestions(applicationId, ref retList, knowledgeTypeId);
            return retList;
        }

        public static List<string> search_questions(Guid applicationId, string searchText, int? count = 20)
        {
            List<string> retList = new List<string>();
            DataProvider.SearchQuestions(applicationId, ref retList, searchText, count);
            return retList;
        }

        public static List<AnswerOption> get_answer_options(Guid applicationId, List<Guid> typeQuestionIds)
        {
            List<AnswerOption> retList = new List<AnswerOption>();
            DataProvider.GetAnswerOptions(applicationId, ref retList, typeQuestionIds);
            return retList;
        }

        public static List<EvaluationAnswer> get_filled_evaluation_form(Guid applicationId, 
            Guid knowledgeId, Guid userId, int? wfVersionId = null)
        {
            List<EvaluationAnswer> lst = new List<Knowledge.EvaluationAnswer>();
            DataProvider.GetFilledEvaluationForm(applicationId, ref lst, knowledgeId, userId, wfVersionId);
            return lst;
        }

        public static List<KnowledgeEvaluation> get_evaluations_done(Guid applicationId, Guid knowledgeId, int? wfVersionId)
        {
            List<KnowledgeEvaluation> retList = new List<KnowledgeEvaluation>();
            DataProvider.GetEvaluationsDone(applicationId, ref retList, knowledgeId, wfVersionId);
            return retList;
        }

        public static bool has_evaluated(Guid applicationId, Guid knowledgeId, Guid userId)
        {
            return DataProvider.HasEvaluated(applicationId, knowledgeId, userId);
        }

        public static bool accept_knowledge(Guid applicationId, 
            Guid nodeId, Guid currentUserId, List<string> textOptions, string description)
        {
            return DataProvider.AcceptRejectKnowledge(applicationId, nodeId, currentUserId, true, textOptions, description);
        }

        public static bool reject_knowledge(Guid applicationId, 
            Guid nodeId, Guid currentUserId, List<string> textOptions, string description)
        {
            return DataProvider.AcceptRejectKnowledge(applicationId, nodeId, currentUserId, false, textOptions, description);
        }

        public static bool send_to_admin(Guid applicationId, Guid nodeId, List<Guid> adminUserIds, 
            Guid currentUserId, string description, ref List<Dashboard> dashboards, ref string message)
        {
            return DataProvider.SendToAdmin(applicationId, nodeId, adminUserIds, 
                currentUserId, description, ref dashboards, ref message);
        }

        public static bool send_to_admin(Guid applicationId, Guid nodeId, Guid adminUserId, Guid currentUserId, 
            string description, ref List<Dashboard> dashboards, ref string message)
        {
            List<Guid> adminIds = new List<Guid>();
            adminIds.Add(adminUserId);
            return send_to_admin(applicationId, nodeId, adminIds, 
                currentUserId, description, ref dashboards, ref message);
        }

        public static bool send_back_for_revision(Guid applicationId, Guid nodeId, Guid currentUserId, 
            List<string> textOptions, string description, ref List<Dashboard> dashboards, ref string message)
        {
            return DataProvider.SendBackForRevision(applicationId,
                nodeId, currentUserId, textOptions, description, ref dashboards, ref message);
        }

        public static bool send_to_evaluators(Guid applicationId, Guid nodeId, List<Guid> evaluatorUserIds, 
            Guid currentUserId, string description, ref List<Dashboard> dashboards, ref string message)
        {
            return DataProvider.SendToEvaluators(applicationId, nodeId, evaluatorUserIds, 
                currentUserId, description, ref dashboards, ref message);
        }

        public static bool new_evaluators(Guid applicationId, Guid nodeId, 
            List<Guid> evaluatorUserIds, ref List<Dashboard> dashboards, ref string message)
        {
            return DataProvider.NewEvaluators(applicationId, nodeId, evaluatorUserIds, ref dashboards, ref message);
        }

        public static bool send_knowledge_comment(Guid applicationId, Guid nodeId, Guid userId, long? replyToHistoryId,
            List<Guid> adminUserIds, string description, ref List<Dashboard> retDashboards)
        {
            return DataProvider.SendKnowledgeComment(applicationId, 
                nodeId, userId, replyToHistoryId, adminUserIds, description, ref retDashboards);
        }

        public static bool save_evaluation_form(Guid applicationId, Guid nodeId, Guid userId, List<KeyValuePair<Guid, double>> answers, 
            Guid currentUserId, double score, List<Guid> adminUserIds, List<string> textOptions, string description, 
            ref List<Dashboard> retDashboards, ref string status, ref bool searchabilityActivated)
        {
            return DataProvider.SaveEvaluationForm(applicationId, nodeId, userId, answers, currentUserId, score,
                adminUserIds, textOptions, description, ref retDashboards, ref status, ref searchabilityActivated);
        }

        public static bool save_evaluation_form(Guid applicationId, Guid nodeId, Guid userId, List<KeyValuePair<Guid, double>> answers, 
            Guid currentUserId, double score, Guid adminUserId, List<string> textOptions, string description,
            ref List<Dashboard> retDashboards, ref string status, ref bool searchabilityActivated)
        {
            List<Guid> adminIds = new List<Guid>();
            adminIds.Add(adminUserId);

            return save_evaluation_form(applicationId, nodeId, userId, answers, currentUserId, score, 
                adminIds, textOptions, description, ref retDashboards, ref status, ref searchabilityActivated);
        }

        public static bool remove_evaluator(Guid applicationId, Guid nodeId, Guid userId)
        {
            return DataProvider.RemoveEvaluator(applicationId, nodeId, userId);
        }

        public static bool refuse_evaluation(Guid applicationId, Guid nodeId, Guid currentUserId, 
            List<Guid> adminUserIds, string description, ref List<Dashboard> retDashboards)
        {
            return DataProvider.RefuseEvaluation(applicationId,
                nodeId, currentUserId, adminUserIds, description, ref retDashboards);
        }

        public static bool refuse_evaluation(Guid applicationId, Guid nodeId, Guid currentUserId, 
            Guid adminUserId, string description, ref List<Dashboard> retDashboards)
        {
            List<Guid> adminIds = new List<Guid>();
            adminIds.Add(adminUserId);
            return refuse_evaluation(applicationId, nodeId, currentUserId, adminIds, description, ref retDashboards);
        }

        public static bool terminate_evaluation(Guid applicationId, Guid nodeId, Guid currentUserId,
            string description, ref bool accepted, ref bool searchabilityActivated)
        {
            return DataProvider.TerminateEvaluation(applicationId,
                nodeId, currentUserId, description, ref accepted, ref searchabilityActivated);
        }

        public static int? get_last_history_version_id(Guid applicationId, Guid knowledgeId)
        {
            return DataProvider.GetLastHistoryVersionID(applicationId, knowledgeId);
        }

        public static bool edit_history_description(Guid applicationId, long id, string description)
        {
            return DataProvider.EditHistoryDescription(applicationId, id, description);
        }

        public static List<KWFHistory> get_history(Guid applicationId, Guid knowledgeId, 
            Guid? userId = null, string action = null, int? wfVersionId = null)
        {
            List<KWFHistory> retList = new List<KWFHistory>();
            DataProvider.GetHistory(applicationId, ref retList, knowledgeId, userId, action, wfVersionId);
            return retList;
        }

        public static KWFHistory get_history(Guid applicationId, long id)
        {
            return DataProvider.GetHistory(applicationId, id);
        }

        public static long add_feedback(Guid applicationId, FeedBack info)
        {
            return DataProvider.AddFeedBack(applicationId, info);
        }

        public static bool modify_feedback(Guid applicationId, FeedBack Info)
        {
            return DataProvider.ModifyFeedBack(applicationId, Info);
        }

        public static bool remove_feedback(Guid applicationId, long feedbackId)
        {
            return DataProvider.ArithmeticDeleteFeedBack(applicationId, feedbackId);

        }

        public static List<FeedBack> get_knowledge_feedbacks(Guid applicationId, Guid knowledgeId, Guid userId,
            FeedBackTypes? feedbackType = null, DateTime? sendDateLowerThreshold = null, 
            DateTime? sendDateUpperThreshold = null)
        {
            List<FeedBack> retList = new List<FeedBack>();
            DataProvider.GetKnowledgeFeedBacks(applicationId,
                ref retList, knowledgeId, userId, feedbackType, sendDateLowerThreshold, sendDateUpperThreshold);
            return retList;
        }

        public static List<FeedBack> get_knowledge_feedbacks(Guid applicationId, Guid knowledgeId, 
            FeedBackTypes? feedbackType = null, DateTime? sendDateLowerThreshold = null, 
            DateTime? sendDateUpperThreshold = null)
        {
            List<FeedBack> retList = new List<FeedBack>();
            DataProvider.GetKnowledgeFeedBacks(applicationId,
                ref retList, knowledgeId, null, feedbackType, sendDateLowerThreshold, sendDateUpperThreshold);
            return retList;
        }

        public static List<FeedBack> get_knowledge_feedbacks(Guid applicationId, ref List<long> feedbackIds)
        {
            List<FeedBack> retList = new List<FeedBack>();
            DataProvider.GetKnowledgeFeedBacks(applicationId, ref retList, ref feedbackIds);
            return retList;
        }

        public static FeedBack get_knowledge_feedback(Guid applicationId, long feedbackId)
        {
            List<long> _fIds = new List<long>();
            _fIds.Add(feedbackId);
            return get_knowledge_feedbacks(applicationId, ref _fIds).FirstOrDefault();
        }

        public static void get_feedback_status(Guid applicationId, Guid knowledgeId, Guid? userId, 
            ref double totalFinancialFeedbacks, ref double totalTemporalFeedbacks, 
            ref double financialFeedbackStatus, ref double temporalFeedbackStatus)
        {
            DataProvider.GetFeedBackStatus(applicationId, knowledgeId, userId, ref totalFinancialFeedbacks, 
                ref totalTemporalFeedbacks, ref financialFeedbackStatus, ref temporalFeedbackStatus);
        }

        public static List<NecessaryItem> get_necessary_items(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            return DataProvider.GetNecessaryItems(applicationId, nodeTypeIdOrNodeId);
        }

        public static bool activate_necessary_item(Guid applicationId, Guid knowledgeTypeId,
            NecessaryItem itm, Guid currentUserId)
        {
            return DataProvider.ActivateNecessaryItem(applicationId, knowledgeTypeId, itm, currentUserId);
        }

        public static bool deactive_necessary_item(Guid applicationId, Guid knowledgeTypeId,
            NecessaryItem itm, Guid currentUserId)
        {
            return DataProvider.DeactiveNecessaryItem(applicationId, knowledgeTypeId, itm, currentUserId);
        }

        public static List<NecessaryItem> check_necessary_items(Guid applicationId, Guid knowledgeId)
        {
            return DataProvider.CheckNecessaryItems(applicationId, knowledgeId);
        }
    }
}
