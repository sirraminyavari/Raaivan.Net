using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.CoreNetwork;

namespace RaaiVan.Modules.QA
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[QA_" + name + "]"; //'[dbo].' is database owner and 'QA_' is module qualifier
        }

        private static void _parse_workflows(ref IDataReader reader, ref List<QAWorkFlow> lst)
        {
            while (reader.Read())
            {
                try
                {
                    QAWorkFlow qa = new QA.QAWorkFlow();

                    if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString())) qa.WorkFlowID = (Guid)reader["WorkFlowID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) qa.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                        qa.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["InitialCheckNeeded"].ToString()))
                        qa.InitialCheckNeeded = (bool)reader["InitialCheckNeeded"];
                    if (!string.IsNullOrEmpty(reader["FinalConfirmationNeeded"].ToString()))
                        qa.FinalConfirmationNeeded = (bool)reader["FinalConfirmationNeeded"];
                    if (!string.IsNullOrEmpty(reader["ActionDeadline"].ToString()))
                        qa.ActionDeadline = (int)reader["ActionDeadline"];
                    if (!string.IsNullOrEmpty(reader["RemovableAfterConfirmation"].ToString()))
                        qa.RemovableAfterConfirmation = (bool)reader["RemovableAfterConfirmation"];
                    if (!string.IsNullOrEmpty(reader["DisableComments"].ToString()))
                        qa.DisableComments = (bool)reader["DisableComments"];
                    if (!string.IsNullOrEmpty(reader["DisableQuestionLikes"].ToString()))
                        qa.DisableQuestionLikes = (bool)reader["DisableQuestionLikes"];
                    if (!string.IsNullOrEmpty(reader["DisableAnswerLikes"].ToString()))
                        qa.DisableAnswerLikes = (bool)reader["DisableAnswerLikes"];
                    if (!string.IsNullOrEmpty(reader["DisableCommentLikes"].ToString()))
                        qa.DisableCommentLikes = (bool)reader["DisableCommentLikes"];
                    if (!string.IsNullOrEmpty(reader["DisableBestAnswer"].ToString()))
                        qa.DisableBestAnswer = (bool)reader["DisableBestAnswer"];


                    if (!string.IsNullOrEmpty(reader["AnswerBy"].ToString()))
                    {
                        AnswerBy ansBy = AnswerBy.None;
                        if (Enum.TryParse<AnswerBy>(reader["AnswerBy"].ToString(), out ansBy)) qa.AnswerBy = ansBy;
                    }

                    if (!string.IsNullOrEmpty(reader["PublishAfter"].ToString()))
                    {
                        PublishAfter pubAfter = PublishAfter.None;
                        if (Enum.TryParse<PublishAfter>(reader["PublishAfter"].ToString(), out pubAfter))
                            qa.PublishAfter = pubAfter;
                    }

                    if (!string.IsNullOrEmpty(reader["NodeSelectType"].ToString()))
                    {
                        NodeSelectType nsType = NodeSelectType.None;
                        if (Enum.TryParse<NodeSelectType>(reader["NodeSelectType"].ToString(), out nsType))
                            qa.NodeSelectType = nsType;
                    }

                    lst.Add(qa);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_faq_categories(ref IDataReader reader, ref List<FAQCategory> lst)
        {
            while (reader.Read())
            {
                try
                {
                    FAQCategory cat = new QA.FAQCategory();

                    if (!string.IsNullOrEmpty(reader["CategoryID"].ToString())) cat.CategoryID = (Guid)reader["CategoryID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) cat.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["HasChild"].ToString())) cat.HasChild = (bool)reader["HasChild"];

                    lst.Add(cat);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_questions(ref IDataReader reader, 
            ref List<Question> lstQuestions, ref long totalCount, bool full)
        {
            totalCount = 0;

            bool parseIsWhat = true;

            while (reader.Read())
            {
                try
                {
                    Question question = new Question();

                    if (!full)
                    {
                        if (!string.IsNullOrEmpty(reader["TotalCount"].ToString()))
                            totalCount = (long)reader["TotalCount"];
                        if (!string.IsNullOrEmpty(reader["HasBestAnswer"].ToString()))
                            question.HasBestAnswer = (bool)reader["HasBestAnswer"];
                        if (!string.IsNullOrEmpty(reader["RelatedNodesCount"].ToString()))
                            question.RelatedNodesCount = (int)reader["RelatedNodesCount"];
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(reader["WorkFlowID"].ToString()))
                            question.WorkFlowID = (Guid)reader["WorkFlowID"];
                        if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                            question.Description = (string)reader["Description"];
                        if (!string.IsNullOrEmpty(reader["BestAnswerID"].ToString()))
                            question.BestAnswerID = (Guid)reader["BestAnswerID"];
                        if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString()))
                            question.LikeStatus = (bool)reader["LikeStatus"];
                        if (!string.IsNullOrEmpty(reader["FollowStatus"].ToString()))
                            question.FollowStatus = (bool)reader["FollowStatus"];
                        //if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString())) question.VisitsCount = (int)reader["VisitsCount"];
                        if (!string.IsNullOrEmpty(reader["PublicationDate"].ToString()))
                            question.PublicationDate = (DateTime)reader["PublicationDate"];
                    }

                    if (!string.IsNullOrEmpty(reader["QuestionID"].ToString())) question.QuestionID = (Guid)reader["QuestionID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) question.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) question.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) question.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) question.Sender.UserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) question.Sender.FirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) question.Sender.LastName = (string)reader["SenderLastName"];
                    if (!string.IsNullOrEmpty(reader["AnswersCount"].ToString())) question.AnswersCount = (int)reader["AnswersCount"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) question.LikesCount = (int)reader["LikesCount"];
                    if (!string.IsNullOrEmpty(reader["DislikesCount"].ToString())) question.DislikesCount = (int)reader["DislikesCount"];

                    if (!full && parseIsWhat)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(reader["IsGroup"].ToString()))
                                question.IsGroup = (bool)reader["IsGroup"];
                            if (!string.IsNullOrEmpty(reader["IsExpertiseDomain"].ToString()))
                                question.IsExpertiseDomain = (bool)reader["IsExpertiseDomain"];
                            if (!string.IsNullOrEmpty(reader["IsFavorite"].ToString()))
                                question.IsFavorite = (bool)reader["IsFavorite"];
                            if (!string.IsNullOrEmpty(reader["IsProperty"].ToString()))
                                question.IsProperty = (bool)reader["IsProperty"];
                            if (!string.IsNullOrEmpty(reader["FromFriend"].ToString()))
                                question.FromFriend = (bool)reader["FromFriend"];
                        }
                        catch { parseIsWhat = false; }
                    }

                    string strStatus = string.Empty;
                    QuestionStatus st = QuestionStatus.None;
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) strStatus = (string)reader["Status"];
                    if (!Enum.TryParse<QuestionStatus>(strStatus, out st)) st = QuestionStatus.None;

                    question.Status = st;

                    lstQuestions.Add(question);
                }
                catch (Exception ex) { string strEx = ex.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_related_nodes(ref IDataReader reader, ref List<RelatedNode> retList, ref long totalCount)
        {
            totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    RelatedNode item = new RelatedNode();
                    

                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) item.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) item.NodeName = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) item.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["Count"].ToString())) item.Count = (int)reader["Count"];
                    if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) item.Deleted = (bool)reader["Deleted"];
                    
                    retList.Add(item);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_answers(ref IDataReader reader, ref List<Answer> lstAnswers)
        {
            while (reader.Read())
            {
                try
                {
                    Answer answer = new Answer();

                    if (!string.IsNullOrEmpty(reader["AnswerID"].ToString())) answer.AnswerID = (Guid)reader["AnswerID"];
                    if (!string.IsNullOrEmpty(reader["QuestionID"].ToString())) answer.QuestionID = (Guid)reader["QuestionID"];
                    if (!string.IsNullOrEmpty(reader["AnswerBody"].ToString())) answer.AnswerBody = (string)reader["AnswerBody"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) answer.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) answer.Sender.UserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) answer.Sender.FirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) answer.Sender.LastName = (string)reader["SenderLastName"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) answer.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) answer.LikesCount = (int)reader["LikesCount"];
                    if (!string.IsNullOrEmpty(reader["DislikesCount"].ToString())) answer.DislikesCount = (int)reader["DislikesCount"];
                    if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString())) answer.LikeStatus = (bool)reader["LikeStatus"];

                    lstAnswers.Add(answer);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_comments(ref IDataReader reader, ref List<Comment> lstComments)
        {
            while (reader.Read())
            {
                try
                {
                    Comment comment = new Comment();

                    if (!string.IsNullOrEmpty(reader["CommentID"].ToString())) comment.CommentID = (Guid)reader["CommentID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) comment.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["ReplyToCommentID"].ToString())) comment.ReplyToCommentID = (Guid)reader["ReplyToCommentID"];
                    if (!string.IsNullOrEmpty(reader["BodyText"].ToString())) comment.BodyText = (string)reader["BodyText"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) comment.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) comment.Sender.UserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) comment.Sender.FirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) comment.Sender.LastName = (string)reader["SenderLastName"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) comment.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) comment.LikesCount = (int)reader["LikesCount"];
                    if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString())) comment.LikeStatus = (bool)reader["LikeStatus"];

                    lstComments.Add(comment);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }
        
        public static bool AddNewWorkFlow(Guid applicationId, Guid workflowId, string name, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddNewWorkFlow");

            try
            {
                if (workflowId == Guid.Empty) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    workflowId, name, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RenameWorkFlow(Guid applicationId, Guid workflowId, string name, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RenameWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, name, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowDescription(Guid applicationId, 
            Guid workflowId, string description, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, description, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowsOrder(Guid applicationId, List<Guid> workflowIds)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowsOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(workflowIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowInitialCheckNeeded(Guid applicationId, 
            Guid workflowId, bool value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowInitialCheckNeeded");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowFinalConfirmationNeeded(Guid applicationId,
            Guid workflowId, bool value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowFinalConfirmationNeeded");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowActionDeadline(Guid applicationId,
            Guid workflowId, int value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowActionDeadline");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowAnswerBy(Guid applicationId,
            Guid workflowId, AnswerBy value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowAnswerBy");

            try
            {
                string strValue = null;
                if (value != AnswerBy.None) strValue = value.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, strValue, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowPublishAfter(Guid applicationId,
            Guid workflowId, PublishAfter value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowPublishAfter");

            try
            {
                string strValue = null;
                if (value != PublishAfter.None) strValue = value.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, strValue, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowRemovableAfterConfirmation(Guid applicationId,
            Guid workflowId, bool value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowRemovableAfterConfirmation");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowNodeSelectType(Guid applicationId,
            Guid workflowId, NodeSelectType value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowNodeSelectType");

            try
            {
                string strValue = null;
                if (value != NodeSelectType.None) strValue = value.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, strValue, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowDisableComments(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowDisableComments");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowDisableQuestionLikes(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowDisableQuestionLikes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowDisableAnswerLikes(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowDisableAnswerLikes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowDisableCommentLikes(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowDisableCommentLikes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetWorkFlowDisableBestAnswer(Guid applicationId,
            Guid workflowId, bool? value, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetWorkFlowDisableBestAnswer");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, value, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveWorkFlow(Guid applicationId, Guid workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RecycleWorkFlow(Guid applicationId, Guid workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RecycleWorkFlow");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static void GetWorkFlows(Guid applicationId, ref List<QAWorkFlow> retList, bool archive)
        {
            string spName = GetFullyQualifiedName("GetWorkFlows");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, archive);
                _parse_workflows(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetWorkFlow(Guid applicationId, ref List<QAWorkFlow> retList, 
            Guid workflowIdOrQuestionIdOrAnswerId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlow");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowIdOrQuestionIdOrAnswerId);
                _parse_workflows(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static List<Guid> IsWorkFlow(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsWorkFlow");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> lst = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref lst);
                return lst;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return new List<Guid>();
            }
        }

        public static bool AddWorkFlowAdmin(Guid applicationId, Guid userId, Guid? workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddWorkFlowAdmin");

            try
            {
                if (workflowId == Guid.Empty) workflowId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveWorkFlowAdmin(Guid applicationId, Guid userId, Guid? workflowId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveWorkFlowAdmin");

            try
            {
                if (workflowId == Guid.Empty) workflowId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, workflowId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsWorkFlowAdmin(Guid applicationId, Guid userId, Guid? workflowIdOrQuestionIdOrAnswerId)
        {
            string spName = GetFullyQualifiedName("IsWorkFlowAdmin");

            try
            {
                if (workflowIdOrQuestionIdOrAnswerId == Guid.Empty) workflowIdOrQuestionIdOrAnswerId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, workflowIdOrQuestionIdOrAnswerId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static void GetWorkFlowAdminIDs(Guid applicationId, ref List<Guid> retList,
            Guid? workflowIdOrQuestionIdOrAnswerId)
        {
            string spName = GetFullyQualifiedName("GetWorkFlowAdminIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, workflowIdOrQuestionIdOrAnswerId);
                ProviderUtil.parse_guids(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static bool SetCandidateRelations(Guid applicationId,
            Guid workflowId, ref List<Guid> nodeTypeIds, ref List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetCandidateRelations");
            
            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    workflowId, ProviderUtil.list_to_string<Guid>(ref nodeTypeIds),
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetCandidateNodeRelations(Guid applicationId,
            ref List<Node> retNodes, Guid workflowIdOrQuestionIdOrAnswerId)
        {
            string spName = GetFullyQualifiedName("GetCandidateNodeRelationIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    workflowIdOrQuestionIdOrAnswerId);
                List<Guid> nodeIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref nodeIds);
                retNodes = CNController.get_nodes(applicationId, nodeIds, full: null, currentUserId: null);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetCandidateNodeTypeRelations(Guid applicationId,
            ref List<NodeType> retNodeTypes, Guid workflowIdOrQuestionIdOrAnswerId)
        {
            string spName = GetFullyQualifiedName("GetCandidateNodeTypeRelationIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    workflowIdOrQuestionIdOrAnswerId);
                List<Guid> nodeTypeIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref nodeTypeIds);
                retNodeTypes = CNController.get_node_types(applicationId, nodeTypeIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static bool CreateFAQCategory(Guid applicationId, 
            Guid categoryId, Guid? parentId, string name, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("CreateFAQCategory");

            try
            {
                if (parentId == Guid.Empty) parentId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    categoryId, parentId, name, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RenameFAQCategory(Guid applicationId, Guid categoryId, string name, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RenameFAQCategory");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    categoryId, name, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool MoveFAQCategories(Guid applicationId, 
            List<Guid> categoryIds, Guid? newParentId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("MoveFAQCategories");

            try
            {
                if (newParentId == Guid.Empty) newParentId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(categoryIds), ',', newParentId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetFAQCategoriesOrder(Guid applicationId, List<Guid> categoryIds)
        {
            string spName = GetFullyQualifiedName("SetFAQCategoriesOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(categoryIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveFAQCategories(Guid applicationId,
            List<Guid> categoryIds, bool? removeHierarchy, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveFAQCategories");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(categoryIds), ',', removeHierarchy, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static void GetChildFAQCategories(Guid applicationId, ref List<FAQCategory> retList,
            Guid? parentId, Guid? currentUserId, bool? checkAccess)
        {
            string spName = GetFullyQualifiedName("GetChildFAQCategories");

            try
            {
                if (parentId == Guid.Empty) parentId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    parentId, currentUserId, checkAccess, RaaiVanSettings.DefaultPrivacy(applicationId), DateTime.Now);
                _parse_faq_categories(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static List<Guid> IsFAQCategory(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsFAQCategory");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> lst = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref lst);
                return lst;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return new List<Guid>();
            }
        }

        public static bool AddFAQItems(Guid applicationId,
            Guid categoryId, List<Guid> questionIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddFAQItems");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    categoryId, ProviderUtil.list_to_string<Guid>(questionIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool AddQuestionToFAQCategories(Guid applicationId,
            Guid questionId, List<Guid> categoryIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddQuestionToFAQCategories");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, ProviderUtil.list_to_string<Guid>(categoryIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveFAQItem(Guid applicationId,
            Guid categoryId, Guid questionId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveFAQItem");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    categoryId, questionId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetFAQItemsOrder(Guid applicationId, Guid categoryId, List<Guid> questionIds)
        {
            string spName = GetFullyQualifiedName("SetFAQItemsOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    categoryId, ProviderUtil.list_to_string<Guid>(questionIds), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool AddQuestion(Guid applicationId, Guid questionId, string title, string description,
            QuestionStatus status, DateTime? publicationDate, List<Guid> relatedNodeIds, 
            Guid? workflowId, Guid? adminId, Guid currentUserId, ref List<Dashboard> dashboards)
        {
            string spName = GetFullyQualifiedName("AddQuestion");

            try
            {
                if (workflowId == Guid.Empty) workflowId = null;
                if (adminId == Guid.Empty) adminId = null;
                string strStatus = null;
                if (status != QuestionStatus.None) strStatus = status.ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    questionId, title, description, strStatus, publicationDate, 
                    ProviderUtil.list_to_string<Guid>(relatedNodeIds), 
                    ',', workflowId, adminId, currentUserId, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool EditQuestionTitle(Guid applicationId, Guid questionId, string title, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EditQuestionTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, title, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool EditQuestionDescription(Guid applicationId, 
            Guid questionId, string description, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EditQuestionDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, description, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsQuestion(Guid applicationId, Guid id)
        {
            string spName = GetFullyQualifiedName("IsQuestion");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, id));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsAnswer(Guid applicationId, Guid id)
        {
            string spName = GetFullyQualifiedName("IsAnswer");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, id));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool ConfirmQuestion(Guid applicationId, Guid questionId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ConfirmQuestion");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }
        
        public static bool SetTheBestAnswer(Guid applicationId, Guid questionId, Guid answerId, 
            bool publish, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetTheBestAnswer");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, answerId, publish, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SetQuestionStatus(Guid applicationId, Guid questionId, 
            QuestionStatus status, bool publish, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetQuestionStatus");

            try
            {
                string strStatus = null;
                if (status != QuestionStatus.None) strStatus = status.ToString();

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, strStatus, publish, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveQuestion(Guid applicationId, Guid questionId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveQuestion");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static void GetQuestions(Guid applicationId, ref List<Question> retList, 
            List<Guid> questionIds, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("GetQuestionsByIDs");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;

                long totalCount = 0;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(questionIds), ',', currentUserId);
                _parse_questions(ref reader, ref retList, ref totalCount, true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetQuestions(Guid applicationId, ref List<Question> retList, 
            string searchText, bool startWithSearch, DateTime? dateFrom, DateTime? dateTo, 
            int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.get_search_text(searchText, startWithSearch), dateFrom, dateTo, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void FindRelatedQuestions(Guid applicationId, ref List<Question> retList, 
            Guid questionId, int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("FindRelatedQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    questionId, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetQuestionsRelatedToNode(Guid applicationId, ref List<Question> retList, Guid nodeId, 
            string searchText, bool startWithSearch, DateTime? dateFrom, DateTime? dateTo, 
            int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetQuestionsRelatedToNode");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId,
                    ProviderUtil.get_search_text(searchText, startWithSearch), dateFrom, dateTo, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetRelatedQuestions(Guid applicationId, ref List<Question> retList, 
            Guid userId, bool? groups, bool? expertiseDomains, bool? favorites, bool? properties, bool? fromFriends, 
            int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetRelatedQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, groups, expertiseDomains, favorites, properties, fromFriends, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void MyFavoriteQuestions(Guid applicationId, ref List<Question> retList,
            Guid userId, int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("MyFavoriteQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void MyAskedQuestions(Guid applicationId, ref List<Question> retList,
            Guid userId, int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("MyAskedQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void QuestionsAskedOfMe(Guid applicationId, ref List<Question> retList,
            Guid userId, int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("QuestionsAskedOfMe");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetFAQItems(Guid applicationId, ref List<Question> retList,
            Guid categoryId, int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetFAQItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    categoryId, count, lowerBoundary);
                _parse_questions(ref reader, ref retList, ref totalCount, false);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GroupQuestionsByRelatedNodes(Guid applicationId, ref List<RelatedNode> retList,
            Guid? currentUserId, Guid? questionId, string searchText, bool? checkAccess, 
            int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GroupQuestionsByRelatedNodes");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;
                if (questionId == Guid.Empty) questionId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    currentUserId, questionId, ProviderUtil.get_search_text(searchText),
                    RaaiVanSettings.DefaultPrivacy(applicationId), checkAccess, DateTime.Now, count, lowerBoundary);
                _parse_related_nodes(ref reader, ref retList, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void FindRelatedTags(Guid applicationId, ref List<RelatedNode> retList,
            Guid nodeId, int? count, double? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("FindRelatedTags");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, count, lowerBoundary);
                _parse_related_nodes(ref reader, ref retList, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void CheckNodes(Guid applicationId, ref List<RelatedNode> retList, List<Guid> nodeIds)
        {
            string spName = GetFullyQualifiedName("CheckNodes");

            try
            {
                long totalCount = 0;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeIds), ',');
                _parse_related_nodes(ref reader, ref retList, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void SearchNodes(Guid applicationId, ref List<RelatedNode> retList, 
            string searchText, bool exactSearch, bool startWithSearch, bool orderByRank, 
            int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("SearchNodes");

            try
            {
                if (!exactSearch) searchText = ProviderUtil.get_search_text(searchText, startWithSearch);

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    searchText, exactSearch, orderByRank, count, lowerBoundary);
                _parse_related_nodes(ref reader, ref retList, ref totalCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static bool SaveRelatedNodes(Guid applicationId, 
            Guid questionId, List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SaveRelatedNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool AddRelatedNodes(Guid applicationId,
            Guid questionId, List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddRelatedNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveRelatedNodes(Guid applicationId,
            Guid questionId, List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveRelatedNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, ProviderUtil.list_to_string<Guid>(nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsQuestionOwner(Guid applicationId, Guid questionIdOrAnswerId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsQuestionOwner");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionIdOrAnswerId, userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsAnswerOwner(Guid applicationId, Guid answerId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsAnswerOwner");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, answerId, userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsCommentOwner(Guid applicationId, Guid commentId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsCommentOwner");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, commentId, userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsRelatedUser(Guid applicationId, Guid questionId, Guid userId)
        {
            string spName = GetFullyQualifiedName("IsRelatedUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, questionId, userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool IsRelatedExpertOrMember(Guid applicationId, Guid questionId, Guid userId,
            bool experts, bool members, bool checkCandidates)
        {
            string spName = GetFullyQualifiedName("IsRelatedExpertOrMember");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    questionId, userId, experts, members, checkCandidates));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool SendAnswer(Guid applicationId, 
            Guid answerId, Guid questionId, string answerBody, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SendAnswer");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    answerId, questionId, answerBody, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool EditAnswer(Guid applicationId, Guid answerId, string answerBody, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EditAnswer");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    answerId, answerBody, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveAnswer(Guid applicationId, Guid answerId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveAnswer");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    answerId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static void GetAnswers(Guid applicationId, ref List<Answer> retList,
            List<Guid> answerIds, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("GetAnswersByIDs");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(answerIds), ',', currentUserId);
                _parse_answers(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetAnswers(Guid applicationId, ref List<Answer> retList, 
            Guid questionId, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("GetAnswers");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId, currentUserId);
                _parse_answers(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static bool SendComment(Guid applicationId,
            Guid commentId, Guid ownerId, Guid? replyToCommentId, string bodyText, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SendComment");

            try
            {
                if (replyToCommentId == Guid.Empty) replyToCommentId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    commentId, ownerId, replyToCommentId, bodyText, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool EditComment(Guid applicationId, Guid commentId, string bodyText, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("EditComment");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    commentId, bodyText, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static bool RemoveComment(Guid applicationId, Guid commentId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveComment");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    commentId, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return false;
            }
        }

        public static void GetComments(Guid applicationId, ref List<Comment> retList, 
            Guid questionId, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("GetComments");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId, currentUserId);
                _parse_comments(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static Guid? GetCommentOwnerID(Guid applicationId, Guid commentId)
        {
            string spName = GetFullyQualifiedName("GetCommentOwnerID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, commentId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return null;
            }
        }

        public static bool AddKnowledgableUser(Guid applicationId, Guid questionId, Guid userId,
            Guid currentUserId, ref List<Dashboard> dashboards)
        {
            string spName = GetFullyQualifiedName("AddKnowledgableUser");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId, userId, 
                    currentUserId, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool RemoveKnowledgableUser(Guid applicationId, Guid questionId, Guid userId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveKnowledgableUser");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, questionId, userId,
                    currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetKnowledgableUserIDs(Guid applicationId, ref List<Guid> retList, Guid questionId)
        {
            string spName = GetFullyQualifiedName("GetKnowledgableUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId);
                ProviderUtil.parse_guids(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetRelatedExpertAndMemberIDs(Guid applicationId, ref List<Guid> retList, Guid questionId)
        {
            string spName = GetFullyQualifiedName("GetRelatedExpertAndMemberIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId);
                ProviderUtil.parse_guids(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void FindKnowledgeableUserIDs(Guid applicationId, ref List<Guid> retList, 
            Guid questionId, int? count, long? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("FindKnowledgeableUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId, count, lowerBoundary);
                ProviderUtil.parse_guids(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static Guid? GetQuestionAskerID(Guid applicationId, Guid questionId)
        {
            string spName = GetFullyQualifiedName("GetQuestionAskerID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, questionId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return Guid.Empty;
            }
        }

        public static void SearchQuestions(Guid applicationId,
            ref List<Question> retQuestions, string searchText, Guid? userId, int? count, Guid? minId)
        {
            string spName = GetFullyQualifiedName("SearchQuestions");

            try
            {
                if (minId == Guid.Empty) minId = null;

                long totalCount = 0;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.get_search_text(searchText), userId, count, minId);
                _parse_questions(ref reader, ref retQuestions, ref totalCount, true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static long GetQuestionsCount(Guid applicationId,
            bool published, DateTime? creationDateLowerLimit, DateTime? creationDateUpperLimit)
        {
            string spName = GetFullyQualifiedName("GetQuestionsCount");

            try
            {
                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    published, creationDateLowerLimit, creationDateUpperLimit));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
                return 0;
            }
        }

        public static void GetAnswerSenderIDs(Guid applicationId, ref List<Guid> retIds, Guid questionId)
        {
            string spName = GetFullyQualifiedName("GetAnswerSenderIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, questionId);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }

        public static void GetExistingQuestionIDs(Guid applicationId,
            ref List<Guid> retIds, ref List<Guid> questionIds)
        {
            string spName = GetFullyQualifiedName("GetExistingQuestionIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string(ref questionIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.QA);
            }
        }
    }
}
