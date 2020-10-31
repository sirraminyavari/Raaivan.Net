using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using RaaiVan.Modules.Users;

namespace RaaiVan.Modules.Sharing
{
    public static class SharingController
    {
        public static bool add_post(Guid applicationId, Post postInfo)
        {
            return DataProvider.AddPost(applicationId, postInfo);
        }

        public static bool update_post(Guid applicationId, Post postInfo)
        {
            return DataProvider.UpdatePost(applicationId, postInfo);
        }

        public static bool arithmetic_delete_post(Guid applicationId, Guid? PostID)
        {
            return DataProvider.ArithmeticDeletePost(applicationId, PostID);
        }

        public static List<Post> get_posts(Guid applicationId, Guid? ownerId, Guid? userId, bool? news, 
            DateTime? maxDate, DateTime? minDate, int count = 10)
        {
            List<Post> lst = new List<Post>();
            DataProvider.GetPosts(applicationId, ref lst, ownerId, userId, news, maxDate, minDate, count);
            return lst;
        }

        public static List<Post> get_posts(Guid applicationId, List<Guid> postIds, Guid? userId)
        {
            List<Post> lst = new List<Post>();
            DataProvider.GetPosts(applicationId, ref lst, ref postIds, userId);
            return lst;
        }

        public static Post get_post(Guid applicationId, Guid postId, Guid? userId)
        {
            List<Guid> _postIds = new List<Guid>();
            _postIds.Add(postId);

            List<Post> lst = new List<Post>();
            DataProvider.GetPosts(applicationId, ref lst, ref _postIds, userId);
            return lst.FirstOrDefault();
        }

        public static Guid? get_post_owner_id(Guid applicationId, Guid postIdOrCommentId)
        {
            return DataProvider.GetPostOwnerID(applicationId, postIdOrCommentId);
        }

        public static Guid? get_post_sender_id(Guid applicationId, Guid postIdOrCommentId)
        {
            return DataProvider.GetPostSenderID(applicationId, postIdOrCommentId);
        }

        public static bool share(Guid applicationId, Post postInfo)
        {
            return DataProvider.Share(applicationId, postInfo);
        }

        public static bool add_comment(Guid applicationId, Comment commentInfo)
        {
            return DataProvider.AddComment(applicationId, commentInfo);
        }

        public static bool update_comment(Guid applicationId, Comment commentInfo)
        {
            return DataProvider.UpdateComment(applicationId, commentInfo);
        }

        public static bool arithmetic_delete_comment(Guid applicationId, Guid? commentId)
        {
            return DataProvider.ArithmeticDeleteComment(applicationId, commentId);
        }

        public static List<Comment> get_post_comments(Guid applicationId, ref List<Guid> postIds, Guid? userId)
        {
            List<Comment> lst = new List<Comment>();
            DataProvider.GetPostComments(applicationId, ref lst, ref postIds, userId);
            return lst;
        }

        public static List<Comment> get_post_comments(Guid applicationId, List<Guid> postIds, Guid? userId)
        {
            return get_post_comments(applicationId, ref postIds, userId);
        }

        public static List<Comment> get_post_comments(Guid applicationId, Guid postId, Guid? userId)
        {
            List<Guid> _pIds = new List<Guid>();
            _pIds.Add(postId);
            return get_post_comments(applicationId, ref _pIds, userId);
        }

        public static List<Comment> get_comments(Guid applicationId, ref List<Guid> commentIds, Guid? userId)
        {
            List<Comment> lst = new List<Comment>();
            DataProvider.GetComments(applicationId, ref lst, ref commentIds, userId);
            return lst;
        }

        public static List<Comment> get_comments(Guid applicationId, List<Guid> commentIds, Guid? userId)
        {
            return get_comments(applicationId, ref commentIds, userId);
        }

        public static Comment get_comment(Guid applicationId, Guid commentId, Guid? userId)
        {
            List<Guid> _pIds = new List<Guid>();
            _pIds.Add(commentId);
            return get_comments(applicationId, ref _pIds, userId).FirstOrDefault();
        }

        public static Guid? get_comment_sender_id(Guid applicationId, Guid commentId)
        {
            return DataProvider.GetCommentSenderID(applicationId, commentId);
        }

        public static List<Guid> get_comment_sender_ids(Guid applicationId, Guid postId)
        {
            List<Guid> retIds = new List<Guid>();
            DataProvider.GetCommentSenderIDs(applicationId, ref retIds, postId);
            return retIds;
        }

        public static bool like_dislike_post(Guid applicationId, LikeDislike info)
        {
            return DataProvider.LikeDislikePost(applicationId, info);
        }

        public static bool unlike_post(Guid applicationId, LikeDislike info)
        {
            return DataProvider.UnlikePost(applicationId, info);
        }

        public static List<Guid> get_post_fan_ids(Guid applicationId, 
            Guid postId, bool? likeStatus, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> ids = new List<Guid>();
            DataProvider.GetPostFanIDs(applicationId, ref ids, postId, likeStatus, count, lowerBoundary, ref totalCount);
            return ids;
        }

        public static List<Guid> get_post_fan_ids(Guid applicationId, Guid postId)
        {
            long totalCount = 0;
            return SharingController.get_post_fan_ids(applicationId, postId, true, null, null, ref totalCount);
        }

        public static List<User> get_post_fans(Guid applicationId, 
            Guid postId, bool? likeStatus, int? count, long? lowerBoundary, ref long totalCount)
        {
            return UsersController.get_users(applicationId, SharingController.get_post_fan_ids(
                applicationId, postId, likeStatus, count, lowerBoundary, ref totalCount));
        }

        public static List<Guid> get_comment_fan_ids(Guid applicationId, 
            Guid commentId, bool? likeStatus, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> ids = new List<Guid>();
            DataProvider.GetCommentFanIDs(applicationId,
                ref ids, commentId, likeStatus, count, lowerBoundary, ref totalCount);
            return ids;
        }

        public static List<Guid> get_comment_fan_ids(Guid applicationId, Guid commentId)
        {
            long totalCount = 0;
            return SharingController.get_comment_fan_ids(applicationId, commentId, true, null, null, ref totalCount);
        }

        public static List<User> get_comment_fans(Guid applicationId, 
            Guid commentId, bool? likeStatus, int? count, long? lowerBoundary, ref long totalCount)
        {
            return UsersController.get_users(applicationId, SharingController.get_comment_fan_ids(applicationId,
                commentId, likeStatus, count, lowerBoundary, ref totalCount));
        }

        public static bool like_dislike_comment(Guid applicationId, LikeDislike info)
        {
            return DataProvider.LikeDislikeComment(applicationId, info);
        }

        public static bool unlike_comment(Guid applicationId, LikeDislike info)
        {
            return DataProvider.UnlikeComment(applicationId, info);
        }

        public static long get_posts_count(Guid applicationId, Guid? ownerId, Guid? senderUserId = null)
        {
            return DataProvider.GetPostsCount(applicationId, ownerId, senderUserId);
        }

        public static long get_shares_count(Guid applicationId, Guid? postId)
        {
            return DataProvider.GetSharesCount(applicationId, postId);
        }

        public static long get_comments_count(Guid applicationId, Guid? postId, Guid? senderUserId = null)
        {
            return DataProvider.GetCommentsCount(applicationId, postId, senderUserId);
        }

        public static long get_user_posts_count(Guid applicationId, Guid? userId, int postTypeId = 0)
        {
            return DataProvider.GetUserPostsCount(applicationId, userId, postTypeId);
        }

        public static long get_post_likes_dislikes_count(Guid applicationId, Guid? postId, bool like = true)
        {
            return DataProvider.GetPostLikesDislikesCount(applicationId, postId, like);
        }

        public static long get_comment_likes_dislikes_count(Guid applicationId, Guid? commentId, bool like = true)
        {
            return DataProvider.GetCommentLikesDislikesCount(applicationId, commentId, like);
        }
    }
}