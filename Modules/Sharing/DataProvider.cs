using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Sharing
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[SH_" + name + "]"; //'[dbo].' is database owner and 'SH_' is module qualifier
        }

        private static void _parse_posts(ref IDataReader reader, ref List<Post> lstPosts)
        {
            while (reader.Read())
            {
                try
                {
                    Post post = new Post();

                    if (!string.IsNullOrEmpty(reader["PostID"].ToString())) post.PostID = (Guid)reader["PostID"];
                    if (!string.IsNullOrEmpty(reader["RefPostID"].ToString())) post.RefPostID = (Guid)reader["RefPostID"];
                    if (!string.IsNullOrEmpty(reader["PostTypeID"].ToString())) post.PostTypeID = (int)reader["PostTypeID"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) post.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["OriginalDescription"].ToString())) post.OriginalDescription = (string)reader["OriginalDescription"];
                    if (!string.IsNullOrEmpty(reader["SharedObjectID"].ToString())) post.SharedObjectID = (Guid)reader["SharedObjectID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) post.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) post.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) post.Sender.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) post.Sender.LastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["JobTitle"].ToString())) post.Sender.JobTitle = (string)reader["JobTitle"];
                    if (!string.IsNullOrEmpty(reader["OriginalSenderUserID"].ToString())) post.OriginalSender.UserID = (Guid)reader["OriginalSenderUserID"];
                    if (!string.IsNullOrEmpty(reader["OriginalSendDate"].ToString())) post.OriginalSendDate = (DateTime)reader["OriginalSendDate"];
                    if (!string.IsNullOrEmpty(reader["OriginalFirstName"].ToString())) post.OriginalSender.FirstName = (string)reader["OriginalFirstName"];
                    if (!string.IsNullOrEmpty(reader["OriginalLastName"].ToString())) post.OriginalSender.LastName = (string)reader["OriginalLastName"];
                    if (!string.IsNullOrEmpty(reader["OriginalJobTitle"].ToString())) post.OriginalSender.JobTitle = (string)reader["OriginalJobTitle"];
                    if (!string.IsNullOrEmpty(reader["LastModificationDate"].ToString())) post.LastModificationDate = (DateTime)reader["LastModificationDate"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) post.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["OwnerType"].ToString())) post.OwnerType = (string)reader["OwnerType"];
                    if (!string.IsNullOrEmpty(reader["Privacy"].ToString())) post.Privacy = (string)reader["Privacy"];
                    if (!string.IsNullOrEmpty(reader["CommentsCount"].ToString())) post.CommentsCount = Convert.ToInt64(reader["CommentsCount"]);
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) post.LikesCount = Convert.ToInt64(reader["LikesCount"]);
                    if (!string.IsNullOrEmpty(reader["DislikesCount"].ToString())) post.DislikesCount = Convert.ToInt64(reader["DislikesCount"]);
                    if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString())) post.LikeStatus = Convert.ToBoolean(reader["LikeStatus"]);
                    if (!string.IsNullOrEmpty(reader["HasPicture"].ToString())) post.HasPicture = Convert.ToBoolean(reader["HasPicture"]);

                    lstPosts.Add(post);
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
                    if (!string.IsNullOrEmpty(reader["PostID"].ToString())) comment.PostID = (Guid)reader["PostID"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) comment.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) comment.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) comment.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) comment.Sender.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) comment.Sender.LastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) comment.LikesCount = Convert.ToInt64(reader["LikesCount"]);
                    if (!string.IsNullOrEmpty(reader["DislikesCount"].ToString())) comment.DislikesCount = Convert.ToInt64(reader["DislikesCount"]);
                    if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString())) comment.LikeStatus = Convert.ToBoolean(reader["LikeStatus"]);

                    lstComments.Add(comment);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_fan_user_ids(ref IDataReader reader, ref List<Guid> retList)
        {
            long totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) retList.Add((Guid)reader["UserID"]);
                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        public static bool AddPost(Guid applicationId, Post Info)
        {
            string spName = GetFullyQualifiedName("AddPost");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.PostID, 
                    Info.PostTypeID, Info.OriginalDescription, Info.SharedObjectID, Info.OriginalSender.UserID, 
                    Info.OriginalSendDate, Info.OwnerID, Info.OwnerType, Info.HasPicture, Info.Privacy));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool UpdatePost(Guid applicationId, Post Info)
        {
            string spName = GetFullyQualifiedName("UpdatePost");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.PostID, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool ArithmeticDeletePost(Guid applicationId, Guid? PostID)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeletePost");

            try { return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, PostID)); }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static void GetPosts(Guid applicationId, ref List<Post> retPosts, Guid? OwnerID, Guid? userId, 
            bool? news, DateTime? maxDate, DateTime? minDate, int count)
        {
            string spName = GetFullyQualifiedName("GetPosts");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    OwnerID, userId, news, maxDate, minDate, count);
                _parse_posts(ref reader, ref retPosts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static void GetPosts(Guid applicationId, ref List<Post> retPosts, ref List<Guid> postIds, Guid? userId)
        {
            string spName = GetFullyQualifiedName("GetPostsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref postIds), ',', userId);
                _parse_posts(ref reader, ref retPosts);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static Guid? GetPostOwnerID(Guid applicationId, Guid postIdOrCommentId)
        {
            string spName = GetFullyQualifiedName("GetPostOwnerID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, postIdOrCommentId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return Guid.Empty;
            }
        }

        public static Guid? GetPostSenderID(Guid applicationId, Guid postIdOrCommentId)
        {
            string spName = GetFullyQualifiedName("GetPostSenderID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, postIdOrCommentId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return Guid.Empty;
            }
        }

        public static bool Share(Guid applicationId, Post Info)
        {
            string spName = GetFullyQualifiedName("Share");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, Info.PostID,
                    Info.RefPostID, Info.OwnerID, Info.Description, Info.Sender.UserID, Info.SendDate, 
                    Info.OwnerType, Info.Privacy));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool AddComment(Guid applicationId, Comment Info)
        {
            string spName = GetFullyQualifiedName("AddComment");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.CommentID, Info.PostID, Info.Description, Info.Sender.UserID, Info.SendDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool UpdateComment(Guid applicationId, Comment Info)
        {
            string spName = GetFullyQualifiedName("UpdateComment");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.CommentID, Info.Description, Info.LastModifierUserID, Info.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool ArithmeticDeleteComment(Guid applicationId, Guid? CommentID)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteComment");

            try { return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, CommentID)); }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static void GetPostComments(Guid applicationId, 
            ref List<Comment> retComments, ref List<Guid> PostIDs, Guid? userId)
        {
            string spName = GetFullyQualifiedName("GetComments");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref PostIDs), ',', userId);
                _parse_comments(ref reader, ref retComments);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static void GetComments(Guid applicationId, 
            ref List<Comment> retComments, ref List<Guid> CommentIDs, Guid? userId)
        {
            string spName = GetFullyQualifiedName("GetCommentsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref CommentIDs), ',', userId);
                _parse_comments(ref reader, ref retComments);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static Guid? GetCommentSenderID(Guid applicationId, Guid commentId)
        {
            string spName = GetFullyQualifiedName("GetCommentSenderID");

            try
            {
                return ProviderUtil.succeed_guid(ProviderUtil.execute_reader(spName, applicationId, commentId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return Guid.Empty;
            }
        }

        public static void GetCommentSenderIDs(Guid applicationId, ref List<Guid> retIds, Guid postId)
        {
            string spName = GetFullyQualifiedName("GetCommentSenderIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, postId);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static bool LikeDislikePost(Guid applicationId, LikeDislike Info)
        {
            string spName = GetFullyQualifiedName("LikeDislikePost");

            try
            {
                if (!Info.Score.HasValue) Info.Score = 0;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ObjectID, Info.UserID, Info.Like, Info.Score, Info.Date));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool UnlikePost(Guid applicationId, LikeDislike Info)
        {
            string spName = GetFullyQualifiedName("UnlikePost");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    Info.ObjectID, Info.UserID));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static void GetPostFanIDs(Guid applicationId, ref List<Guid> retIds, Guid postId, 
            bool? likeStatus, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetPostFanIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    postId, likeStatus, count, lowerBoundary);
                totalCount = _parse_fan_user_ids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static void GetCommentFanIDs(Guid applicationId, ref List<Guid> retIds, Guid commentId, 
            bool? likeStatus, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetCommentFanIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    commentId, likeStatus, count, lowerBoundary);
                totalCount = _parse_fan_user_ids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
            }
        }

        public static bool LikeDislikeComment(Guid applicationId, LikeDislike Info)
        {
            string spName = GetFullyQualifiedName("LikeDislikeComment"); ;

            try
            {
                if (!Info.Score.HasValue) Info.Score = 0;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ObjectID, Info.UserID, Info.Like, Info.Score, Info.Date));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static bool UnlikeComment(Guid applicationId, LikeDislike Info)
        {
            string spName = GetFullyQualifiedName("UnlikeComment");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.ObjectID, Info.UserID));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return false;
            }
        }

        public static long GetPostsCount(Guid applicationId, Guid? OwnerID, Guid? senderUserId)
        {
            string spName = GetFullyQualifiedName("GetPostsCount");

            try
            {
                if (OwnerID == Guid.Empty) OwnerID = null;
                if (senderUserId == Guid.Empty) senderUserId = null;

                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    OwnerID, senderUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return -1;
            }
        }

        public static long GetSharesCount(Guid applicationId, Guid? PostID)
        {
            string spName = GetFullyQualifiedName("GetSharesCount");

            try
            {
                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, PostID));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return -1;
            }
        }

        public static long GetCommentsCount(Guid applicationId, Guid? postId, Guid? senderUserId)
        {
            string spName = GetFullyQualifiedName("GetCommentsCount");

            try
            {
                if (postId == Guid.Empty) postId = null;
                if (senderUserId == Guid.Empty) senderUserId = null;

                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, postId, senderUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return -1;
            }
        }

        public static long GetUserPostsCount(Guid applicationId, Guid? UserID, int PostTypeID)
        {
            string spName = GetFullyQualifiedName("GetUserPostsCount");

            try
            {
                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, UserID, PostTypeID));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return -1;
            }
        }

        public static long GetPostLikesDislikesCount(Guid applicationId, Guid? PostID, bool Like)
        {
            string spName = GetFullyQualifiedName("GetPostLikesDislikesCount");

            try
            {
                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, PostID, Like));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return -1;
            }
        }

        public static long GetCommentLikesDislikesCount(Guid applicationId, Guid? CommentID, bool Like)
        {
            string spName = GetFullyQualifiedName("GetCommentLikesDislikesCount");

            try
            {
                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId, CommentID, Like));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SH);
                return -1;
            }
        }
    }
}