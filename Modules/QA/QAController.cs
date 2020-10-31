using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.QA
{
    public class QAController
    {
        public static bool add_new_workflow(Guid applicationId, Guid workflowId, string name, Guid currentUserId)
        {
            return DataProvider.AddNewWorkFlow(applicationId, workflowId, name, currentUserId);
        }

        public static bool rename_workflow(Guid applicationId, Guid workflowId, string name, Guid currentUserId)
        {
            return DataProvider.RenameWorkFlow(applicationId, workflowId, name, currentUserId);
        }

        public static bool set_workflow_description(Guid applicationId,
            Guid workflowId, string description, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowDescription(applicationId, workflowId, description, currentUserId);
        }

        public static bool set_workflows_order(Guid applicationId, List<Guid> workflowIds)
        {
            return DataProvider.SetWorkFlowsOrder(applicationId, workflowIds);
        }

        public static bool set_workflow_initial_check_needed(Guid applicationId,
            Guid workflowId, bool value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowInitialCheckNeeded(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_final_confirmation_needed(Guid applicationId,
            Guid workflowId, bool value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowFinalConfirmationNeeded(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_action_deadline(Guid applicationId,
            Guid workflowId, int value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowActionDeadline(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_answer_by(Guid applicationId,
            Guid workflowId, AnswerBy value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowAnswerBy(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_publish_after(Guid applicationId,
            Guid workflowId, PublishAfter value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowPublishAfter(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_removable_after_confirmation(Guid applicationId,
            Guid workflowId, bool value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowRemovableAfterConfirmation(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_node_select_type(Guid applicationId,
            Guid workflowId, NodeSelectType value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowNodeSelectType(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_disable_comments(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowDisableComments(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_disable_question_likes(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowDisableQuestionLikes(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_disable_answer_likes(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowDisableAnswerLikes(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_disable_comment_likes(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowDisableCommentLikes(applicationId, workflowId, value, currentUserId);
        }

        public static bool set_workflow_disable_best_answer(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            return DataProvider.SetWorkFlowDisableBestAnswer(applicationId, workflowId, value, currentUserId);
        }

        public static bool remove_workflow(Guid applicationId, Guid workflowId, Guid currentUserId)
        {
            return DataProvider.RemoveWorkFlow(applicationId, workflowId, currentUserId);
        }

        public static bool recycle_workflow(Guid applicationId, Guid workflowId, Guid currentUserId)
        {
            return DataProvider.RecycleWorkFlow(applicationId, workflowId, currentUserId);
        }

        public static List<QAWorkFlow> get_workflows(Guid applicationId, bool archive = false)
        {
            List<QAWorkFlow> lst = new List<QA.QAWorkFlow>();
            DataProvider.GetWorkFlows(applicationId, ref lst, archive);
            return lst;
        }

        public static QAWorkFlow get_workflow(Guid applicationId, Guid workflowIdOrQuestionIdOrAnswerId)
        {
            List<QAWorkFlow> lst = new List<QA.QAWorkFlow>();
            DataProvider.GetWorkFlow(applicationId, ref lst, workflowIdOrQuestionIdOrAnswerId);
            return lst.FirstOrDefault();
        }

        public static List<Guid> is_workflow(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.IsWorkFlow(applicationId, ids);
        }

        public static bool is_workflow(Guid applicationId, Guid id)
        {
            return is_workflow(applicationId, new List<Guid>() { id }).Count == 1;
        }

        public static bool add_workflow_admin(Guid applicationId, Guid userId, Guid? workflowId, Guid currentUserId)
        {
            return DataProvider.AddWorkFlowAdmin(applicationId, userId, workflowId, currentUserId);
        }

        public static bool remove_workflow_admin(Guid applicationId, Guid userId, Guid? workflowId, Guid currentUserId)
        {
            return DataProvider.RemoveWorkFlowAdmin(applicationId, userId, workflowId, currentUserId);
        }

        public static bool is_workflow_admin(Guid applicationId, Guid userId, Guid? workflowIdOrQuestionIdOrAnswerId)
        {
            return DataProvider.IsWorkFlowAdmin(applicationId, userId, workflowIdOrQuestionIdOrAnswerId);
        }

        public static List<Guid> get_workflow_admin_ids(Guid applicationId, 
            Guid? workflowIdOrQuestionIdOrAnswerId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetWorkFlowAdminIDs(applicationId, ref retList, workflowIdOrQuestionIdOrAnswerId);
            return retList;
        }
        
        public static bool set_candidate_relations(Guid applicationId, Guid workflowId,
            ref List<Guid> nodeTypeIds, ref List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.SetCandidateRelations(applicationId,
                workflowId, ref nodeTypeIds, ref nodeIds, currentUserId);
        }

        public static List<Node> get_candidate_node_relations(Guid applicationId, 
            Guid workflowIdOrQuestionIdOrAnswerId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetCandidateNodeRelations(applicationId, ref retList, workflowIdOrQuestionIdOrAnswerId);
            return retList;
        }

        public static List<NodeType> get_candidate_node_type_relations(Guid applicationId,
            Guid workflowIdOrQuestionIdOrAnswerId)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetCandidateNodeTypeRelations(applicationId, ref retList,
                workflowIdOrQuestionIdOrAnswerId);
            return retList;
        }

        public static bool create_faq_category(Guid applicationId,
            Guid categoryId, Guid? parentId, string name, Guid currentUserId)
        {
            return DataProvider.CreateFAQCategory(applicationId, categoryId, parentId, name, currentUserId);
        }

        public static bool rename_faq_category(Guid applicationId, Guid categoryId, string name, Guid currentUserId)
        {
            return DataProvider.RenameFAQCategory(applicationId, categoryId, name, currentUserId);
        }

        public static bool move_faq_categories(Guid applicationId,
            List<Guid> categoryIds, Guid? newParentId, Guid currentUserId)
        {
            return DataProvider.MoveFAQCategories(applicationId, categoryIds, newParentId, currentUserId);
        }

        public static bool set_faq_categories_order(Guid applicationId, List<Guid> categoryIds)
        {
            return DataProvider.SetFAQCategoriesOrder(applicationId, categoryIds);
        }

        public static bool remove_faq_categories(Guid applicationId,
            List<Guid> categoryIds, bool? removeHierarchy, Guid currentUserId)
        {
            return DataProvider.RemoveFAQCategories(applicationId, categoryIds, removeHierarchy, currentUserId);
        }

        public static List<FAQCategory> get_child_faq_categories(Guid applicationId,
            Guid? parentId, Guid? currentUserId, bool? checkAccess)
        {
            List<FAQCategory> lst = new List<QA.FAQCategory>();
            DataProvider.GetChildFAQCategories(applicationId, ref lst, parentId, currentUserId, checkAccess);
            return lst;
        }

        public static List<Guid> is_faq_category(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.IsFAQCategory(applicationId, ids);
        }

        public static bool is_faq_category(Guid applicationId, Guid id)
        {
            return is_faq_category(applicationId, new List<Guid>() { id }).Count == 1;
        }

        public static bool add_faq_items(Guid applicationId, 
            Guid categoryId, List<Guid> questionIds, Guid currentUserId)
        {
            return DataProvider.AddFAQItems(applicationId, categoryId, questionIds, currentUserId);
        }

        public static bool add_question_to_faq_categories(Guid applicationId, 
            Guid questionId, List<Guid> categoryIds, Guid currentUserId)
        {
            return DataProvider.AddQuestionToFAQCategories(applicationId, questionId, categoryIds, currentUserId);
        }

        public static bool remove_faq_item(Guid applicationId, Guid categoryId, Guid questionId, Guid currentUserId)
        {
            return DataProvider.RemoveFAQItem(applicationId, categoryId, questionId, currentUserId);
        }

        public static bool set_faq_items_order(Guid applicationId, Guid categoryId, List<Guid> questionIds)
        {
            return DataProvider.SetFAQItemsOrder(applicationId, categoryId, questionIds);
        }

        public static bool add_question(Guid applicationId, Guid questionId, string title, string description,
            QuestionStatus status, DateTime? publicationDate, List<Guid> relatedNodeIds,
            Guid? workflowId, Guid? adminId, Guid currentUserId, ref List<Dashboard> dashboards)
        {
            return DataProvider.AddQuestion(applicationId, questionId, title, description, status,
                publicationDate, relatedNodeIds, workflowId, adminId, currentUserId, ref dashboards);
        }

        public static bool edit_question_title(Guid applicationId, Guid questionId, string title, Guid currentUserId)
        {
            return DataProvider.EditQuestionTitle(applicationId, questionId, title, currentUserId);
        }

        public static bool edit_question_description(Guid applicationId,
            Guid questionId, string description, Guid currentUserId)
        {
            return DataProvider.EditQuestionDescription(applicationId, questionId, description, currentUserId);
        }

        public static bool is_question(Guid applicationId, Guid id)
        {
            return DataProvider.IsQuestion(applicationId, id);
        }

        public static bool is_answer(Guid applicationId, Guid id)
        {
            return DataProvider.IsAnswer(applicationId, id);
        }

        public static bool confirm_question(Guid applicationId, Guid questionId, Guid currentUserId)
        {
            return DataProvider.ConfirmQuestion(applicationId, questionId, currentUserId);
        }
        
        public static bool set_the_best_answer(Guid applicationId, Guid questionId, Guid answerId,
            bool publish, Guid currentUserId)
        {
            return DataProvider.SetTheBestAnswer(applicationId, questionId, answerId, publish, currentUserId);
        }

        public static bool set_question_status(Guid applicationId, Guid questionId,
            QuestionStatus status, bool publish, Guid currentUserId)
        {
            return DataProvider.SetQuestionStatus(applicationId, questionId, status, publish, currentUserId);
        }

        public static bool remove_question(Guid applicationId, Guid questionId, Guid currentUserId)
        {
            return DataProvider.RemoveQuestion(applicationId, questionId, currentUserId);
        }

        public static List<Question> get_questions(Guid applicationId, List<Guid> questionIds, Guid? currentUserId)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.GetQuestions(applicationId, ref lst, questionIds, currentUserId);
            return lst;
        }

        public static Question get_question(Guid applicationId, Guid questionId, Guid? currentUserId)
        {
            return get_questions(applicationId, new List<Guid>() { questionId }, currentUserId).FirstOrDefault();
        }

        public static List<Question> get_questions(Guid applicationId, string searchText, bool startWithSearch,
            DateTime? dateFrom, DateTime? dateTo, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.GetQuestions(applicationId, ref lst, searchText, startWithSearch,
                dateFrom, dateTo, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> get_questions(Guid applicationId, Guid nodeId, 
            string searchText, bool startWithSearch, DateTime? dateFrom, DateTime? dateTo, 
            int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.GetQuestionsRelatedToNode(applicationId, ref lst, nodeId, searchText, startWithSearch,
                dateFrom, dateTo, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> find_related_questions(Guid applicationId, 
            Guid questionId, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.FindRelatedQuestions(applicationId, ref lst, 
                questionId, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> get_related_questions(Guid applicationId, Guid userId, bool? groups, 
            bool? expertiseDomains, bool? favorites, bool? properties, bool? fromFriends,
            int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.GetRelatedQuestions(applicationId, ref lst, userId, groups, expertiseDomains,
                favorites, properties, fromFriends, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> my_favorite_questions(Guid applicationId,
            Guid userId, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.MyFavoriteQuestions(applicationId, ref lst, userId, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> my_asked_questions(Guid applicationId,
            Guid userId, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.MyAskedQuestions(applicationId, ref lst, userId, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> questions_asked_of_me(Guid applicationId,
            Guid userId, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.QuestionsAskedOfMe(applicationId, ref lst, userId, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Question> get_faq_items(Guid applicationId,
            Guid categoryId, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<Question> lst = new List<QA.Question>();
            DataProvider.GetFAQItems(applicationId, ref lst, categoryId, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<RelatedNode> group_questions_by_related_nodes(Guid applicationId, Guid? currentUserId, 
            string searchText,  bool? checkAccess, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<RelatedNode> lst = new List<RelatedNode>();
            DataProvider.GroupQuestionsByRelatedNodes(applicationId, ref lst, 
                currentUserId, null, searchText, checkAccess, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<RelatedNode> get_related_nodes(Guid applicationId,
            Guid? currentUserId, Guid? questionId, string searchText, bool? checkAccess,
            int? count, double? lowerBoundary, ref long totalCount)
        {
            List<RelatedNode> lst = new List<RelatedNode>();
            DataProvider.GroupQuestionsByRelatedNodes(applicationId, ref lst,
                currentUserId, questionId, searchText, checkAccess, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<RelatedNode> find_related_tags(Guid applicationId,
            Guid nodeId, int? count, double? lowerBoundary, ref long totalCount)
        {
            List<RelatedNode> lst = new List<RelatedNode>();
            DataProvider.FindRelatedTags(applicationId, ref lst, nodeId, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<RelatedNode> check_nodes(Guid applicationId, List<Guid> nodeIds)
        {
            List<RelatedNode> lst = new List<RelatedNode>();
            DataProvider.CheckNodes(applicationId, ref lst, nodeIds);
            return lst;
        }

        public static List<RelatedNode> search_nodes(Guid applicationId,  string searchText, bool exactSearch,
            bool startWithSearch, bool orderByRank, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<RelatedNode> lst = new List<RelatedNode>();
            DataProvider.SearchNodes(applicationId, ref lst, searchText, exactSearch, startWithSearch, 
                orderByRank, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static bool save_related_nodes(Guid applicationId,
            Guid questionId, List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.SaveRelatedNodes(applicationId, questionId, nodeIds, currentUserId);
        }

        public static bool add_related_nodes(Guid applicationId,
            Guid questionId, List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.AddRelatedNodes(applicationId, questionId, nodeIds, currentUserId);
        }

        public static bool remove_related_nodes(Guid applicationId,
            Guid questionId, List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.RemoveRelatedNodes(applicationId, questionId, nodeIds, currentUserId);
        }

        public static bool is_question_owner(Guid applicationId, Guid questionIdOrAnswerId, Guid userId)
        {
            return DataProvider.IsQuestionOwner(applicationId, questionIdOrAnswerId, userId);
        }

        public static bool is_answer_owner(Guid applicationId, Guid answerId, Guid userId)
        {
            return DataProvider.IsAnswerOwner(applicationId, answerId, userId);
        }

        public static bool is_comment_owner(Guid applicationId, Guid commentId, Guid userId)
        {
            return DataProvider.IsCommentOwner(applicationId, commentId, userId);
        }

        public static bool is_related_user(Guid applicationId, Guid questionId, Guid userId)
        {
            return DataProvider.IsRelatedUser(applicationId, questionId, userId);
        }

        public static bool is_related_expert_or_member(Guid applicationId, 
            Guid questionId, Guid userId, bool experts, bool members, bool checkCandidates)
        {
            return DataProvider.IsRelatedExpertOrMember(applicationId, questionId,
                userId, experts, members, checkCandidates);
        }

        public static bool send_answer(Guid applicationId,
            Guid answerId, Guid questionId, string answerBody, Guid currentUserId)
        {
            return DataProvider.SendAnswer(applicationId, answerId, questionId, answerBody, currentUserId);
        }

        public static bool edit_answer(Guid applicationId, Guid answerId, string answerBody, Guid currentUserId)
        {
            return DataProvider.EditAnswer(applicationId, answerId, answerBody, currentUserId);
        }

        public static bool remove_answer(Guid applicationId, Guid answerId, Guid currentUserId)
        {
            return DataProvider.RemoveAnswer(applicationId, answerId, currentUserId);
        }

        public static List<Answer> get_answers(Guid applicationId, List<Guid> answerIds, Guid? currentUserId)
        {
            List<Answer> lst = new List<QA.Answer>();
            DataProvider.GetAnswers(applicationId, ref lst, answerIds, currentUserId);
            return lst;
        }

        public static Answer get_answer(Guid applicationId, Guid answerId, Guid? currentUserId)
        {
            return get_answers(applicationId, new List<Guid>() { answerId }, currentUserId).FirstOrDefault();
        }

        public static List<Answer> get_answers(Guid applicationId, Guid questionId, Guid? currentUserId)
        {
            List<Answer> lst = new List<QA.Answer>();
            DataProvider.GetAnswers(applicationId, ref lst, questionId, currentUserId);
            return lst;
        }

        public static bool send_comment(Guid applicationId,
            Guid commentId, Guid ownerId, Guid? replyToCommentId, string bodyText, Guid currentUserId)
        {
            return DataProvider.SendComment(applicationId, commentId, ownerId, replyToCommentId, bodyText, currentUserId);
        }

        public static bool edit_comment(Guid applicationId, Guid commentId, string bodyText, Guid currentUserId)
        {
            return DataProvider.EditComment(applicationId, commentId, bodyText, currentUserId);
        }

        public static bool remove_comment(Guid applicationId, Guid commentId, Guid currentUserId)
        {
            return DataProvider.RemoveComment(applicationId, commentId, currentUserId);
        }

        public static List<Comment> get_comments(Guid applicationId, Guid questionId, Guid? currentUserId)
        {
            List<Comment> lst = new List<QA.Comment>();
            DataProvider.GetComments(applicationId, ref lst, questionId, currentUserId);
            return lst;
        }

        public static Guid? get_comment_owner_id(Guid applicationId, Guid commentId)
        {
            return DataProvider.GetCommentOwnerID(applicationId, commentId);
        }

        public static bool add_knowledgable_user(Guid applicationId, Guid questionId, Guid userId,
            Guid currentUserId, ref List<Dashboard> dashboards)
        {
            return DataProvider.AddKnowledgableUser(applicationId, questionId, userId, currentUserId, ref dashboards);
        }

        public static bool remove_knowledgable_user(Guid applicationId, Guid questionId, Guid userId, Guid currentUserId)
        {
            return DataProvider.RemoveKnowledgableUser(applicationId, questionId, userId, currentUserId);
        }


        public static List<Guid> get_knowledgable_user_ids(Guid applicationId, Guid questionId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetKnowledgableUserIDs(applicationId, ref retList, questionId);
            return retList;
        }

        public static List<Guid> get_related_expert_and_member_ids(Guid applicationId, Guid questionId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetRelatedExpertAndMemberIDs(applicationId, ref retList, questionId);
            return retList;
        }

        public static List<Guid> find_knowledgeable_user_ids(Guid applicationId, 
            Guid questionId, int? count, long? lowerBoundary)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.FindKnowledgeableUserIDs(applicationId, ref retList, questionId, count, lowerBoundary);
            return retList;
        }

        public static Guid? get_question_asker_id(Guid applicationId, Guid questionId)
        {
            return DataProvider.GetQuestionAskerID(applicationId, questionId);
        }

        public static List<Question> search_questions(Guid applicationId,
            string searchText, Guid? userId, Guid? minId, int? count = 10)
        {
            List<Question> lst = new List<Question>();
            DataProvider.SearchQuestions(applicationId, ref lst, searchText, userId, count, minId);
            return lst;
        }

        public static long get_questions_count(Guid applicationId,
            bool published = false, DateTime? creationDateLowerLimit = null, DateTime? creationDateUpperLimit = null)
        {
            return DataProvider.GetQuestionsCount(applicationId, published, creationDateLowerLimit, creationDateUpperLimit);
        }

        public static List<Guid> get_answer_sender_ids(Guid applicationId, Guid questionId)
        {
            List<Guid> retIds = new List<Guid>();
            DataProvider.GetAnswerSenderIDs(applicationId, ref retIds, questionId);
            return retIds;
        }

        public static List<Guid> get_existing_question_ids(Guid applicationId, List<Guid> questionIds)
        {
            List<Guid> retIds = new List<Guid>();
            DataProvider.GetExistingQuestionIDs(applicationId, ref retIds, ref questionIds);
            return retIds;
        }
    }
}
