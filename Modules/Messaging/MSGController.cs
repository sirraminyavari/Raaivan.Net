using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;

namespace RaaiVan.Modules.Messaging
{
    public static class MSGController
    {
        public static List<ThreadInfo> get_threads(Guid applicationId, 
            Guid userId, int? count = null, int? lastId = null)
        {
            List<ThreadInfo> recent_threads = new List<ThreadInfo>();
            DataProvider.GetThreads(applicationId, ref recent_threads, userId, count, lastId);
            return recent_threads;
        }

        public static void get_thread_info(Guid applicationId, 
            Guid userId, Guid threadId, ref int messagesCount, ref int sentCount, ref int notSeenCount)
        {
            DataProvider.GetThreadInfo(applicationId, 
                userId, threadId, ref messagesCount, ref sentCount, ref notSeenCount);
        }

        private static List<ThreadInfo> _get_thread_users(Guid applicationId, 
            List<Guid> threadIds, Guid userId, int? count = null, int? lastId = null)
        {
            List<ThreadInfo> recent_threads = new List<ThreadInfo>();
            DataProvider.GetThreadUsers(applicationId, ref recent_threads, threadIds, userId, count, lastId);
            return recent_threads;
        }

        public static List<ThreadInfo> get_thread_users(Guid applicationId, 
            List<Guid> threadIds, Guid userId, int? count = null)
        {
            return MSGController._get_thread_users(applicationId, threadIds, userId, count);
        }

        public static List<Users.User> get_thread_users(Guid applicationId, 
            Guid threadId, Guid userId, int? count = null, int? lastId = null)
        {
            List<Guid> tIds = new List<Guid>();
            tIds.Add(threadId);
            List<Users.User> users = new List<Users.User>();
            ThreadInfo ti = MSGController._get_thread_users(applicationId,
                tIds, userId, count, lastId).FirstOrDefault();

            return ti == null ? users : ti.ThreadUsers;
        }

        public static List<Message> get_messages(Guid applicationId, 
            Guid userId, Guid? threadId, bool? sent, long? minId, int? count = null)
        {
            List<Message> retList = new List<Message>();
            DataProvider.GetMessages(applicationId, ref retList, userId, threadId, sent, minId, count);
            return retList;
        }

        public static bool has_message(Guid applicationId, long? id, Guid userId, Guid? threadId, Guid? messageId)
        {
            return DataProvider.HasMessage(applicationId, id, userId, threadId, messageId);
        }

        public static List<Message> get_forwarded_messages(Guid applicationId, Guid messageId)
        {
            List<Message> retList = new List<Message>();
            DataProvider.GetForwardedMessages(applicationId, ref retList, messageId);
            return retList;
        }

        public static long send_message(Guid applicationId, Guid messageId, Guid? forwardedFrom, Guid userId, 
            string title, string messageText, bool isGroup, List<Guid> receiverUserIds, Guid? threadId, 
            List<DocFileInfo> attachedFiles)
        {
            return DataProvider.SendMessage(applicationId,
                messageId, forwardedFrom, userId, title, messageText, isGroup, receiverUserIds, threadId, attachedFiles);
        }

        public static bool send_message(Guid applicationId, 
            Guid senderUserId, Guid receiverUserId, string title, string messageText)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(receiverUserId);
            return MSGController.bulk_send_message(applicationId, senderUserId, lst, title, messageText);
        }

        public static bool bulk_send_message(Guid applicationId, List<Message> messages)
        {
            return DataProvider.BulkSendMessage(applicationId, messages);
        }

        public static bool bulk_send_message(Guid applicationId, Message message)
        {
            List<Message> lst = new List<Message>();
            lst.Add(message);
            return MSGController.bulk_send_message(applicationId, lst);
        }

        public static bool bulk_send_message(Guid applicationId, 
            Guid senderUserId, List<Guid> receiverUserIds, string title, string messageText)
        {
            List<User> _receiverUsers = new List<User>();
            foreach (Guid uId in receiverUserIds) _receiverUsers.Add(new User() { UserID = uId });

            return MSGController.bulk_send_message(applicationId, new Message()
            {
                MessageID = Guid.NewGuid(),
                SenderUserID = senderUserId,
                Title = title,
                MessageText = messageText,
                ReceiverUsers = _receiverUsers
            });
        }

        public static bool remove_thread(Guid applicationId, Guid userId, Guid threadId)
        {
            return DataProvider.RemoveMessages(applicationId, userId, threadId, null);
        }

        public static bool remove_message(Guid applicationId, long id)
        {
            return DataProvider.RemoveMessages(applicationId, null, null, id);
        }

        public static bool set_messages_as_seen(Guid applicationId, Guid userId, Guid threadId)
        {
            return DataProvider.SetMessagesAsSeen(applicationId, userId, threadId);
        }

        public static int get_not_seen_messages_count(Guid applicationId, Guid userId)
        {
            return DataProvider.GetNotSeenMessagesCount(applicationId, userId);
        }

        private static List<Message> _get_message_receivers(Guid applicationId, 
            List<Guid> messageIds, int? count = null, int? lastId = null)
        {
            List<Message> retMsg = new List<Message>();
            DataProvider.GetMessageReceivers(applicationId, ref retMsg, messageIds, count, lastId);
            return retMsg;
        }

        public static List<Message> get_message_receivers(Guid applicationId, List<Guid> messageIds, int? count = null)
        {
            return MSGController._get_message_receivers(applicationId, messageIds, count);
        }

        public static List<Users.User> get_message_receivers(Guid applicationId, 
            Guid messageId, int? count = null, int? lastId = null)
        {
            List<Guid> mIds = new List<Guid>();
            mIds.Add(messageId);
            List<Users.User> users = new List<Users.User>();
            Message msg = MSGController._get_message_receivers(applicationId, mIds, count, lastId).FirstOrDefault();
            return msg == null ? users : msg.ReceiverUsers;
        }
    }
}
