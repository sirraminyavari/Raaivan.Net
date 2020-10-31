using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Messaging
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[MSG_" + name + "]";
        }
        
        private static void _parse_threads(ref IDataReader reader, ref List<ThreadInfo> retThreadInfo)
        {
            while (reader.Read())
            {
                try
                {
                    ThreadInfo th = new ThreadInfo();

                    if (!string.IsNullOrEmpty(reader["ThreadID"].ToString())) th.ThreadID = (Guid)reader["ThreadID"];
                    if (!string.IsNullOrEmpty(reader["IsGroup"].ToString())) th.IsGroup = (bool)reader["IsGroup"];
                    if (!string.IsNullOrEmpty(reader["SentCount"].ToString())) th.SentCount = (int)reader["SentCount"];
                    if (!string.IsNullOrEmpty(reader["NotSeenCount"].ToString())) th.NotSeenCount = (int)reader["NotSeenCount"];
                    if (!string.IsNullOrEmpty(reader["RowNumber"].ToString())) th.ID = (long)reader["RowNumber"];
                    if (!string.IsNullOrEmpty(reader["MessagesCount"].ToString())) th.MessagesCount = (int)reader["MessagesCount"];

                    if (!th.IsGroup.HasValue || !th.IsGroup.Value)
                    {
                        User u = new User();

                        if (!string.IsNullOrEmpty(reader["ThreadID"].ToString())) u.UserID = (Guid)reader["ThreadID"];
                        if (!string.IsNullOrEmpty(reader["UserName"].ToString())) u.UserName = (string)reader["UserName"];
                        if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) u.FirstName = (string)reader["FirstName"];
                        if (!string.IsNullOrEmpty(reader["LastName"].ToString())) u.LastName = (string)reader["LastName"];

                        th.ThreadUsers.Add(u);
                    }

                    retThreadInfo.Add(th);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_thread_info(ref IDataReader reader, 
            ref int messagesCount, ref int sentCount, ref int notSeenCount)
        {
            reader.Read();

            if (!string.IsNullOrEmpty(reader["MessagesCount"].ToString())) messagesCount = (int)reader["MessagesCount"];
            if (!string.IsNullOrEmpty(reader["SentCount"].ToString())) sentCount = (int)reader["SentCount"];
            if (!string.IsNullOrEmpty(reader["NotSeenCount"].ToString())) notSeenCount = (int)reader["NotSeenCount"];

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_messages(ref IDataReader reader, ref List<Message> retMessages)
        {
            while (reader.Read())
            {
                try
                {
                    Message msg = new Message();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) msg.ID = (long)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["ThreadID"].ToString())) msg.ThreadID = (Guid)reader["ThreadID"];
                    if (!string.IsNullOrEmpty(reader["MessageID"].ToString())) msg.MessageID = (Guid)reader["MessageID"];
                    if (!string.IsNullOrEmpty(reader["ForwardedFrom"].ToString())) msg.ForwardedFrom = (Guid)reader["ForwardedFrom"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) msg.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["MessageText"].ToString())) msg.MessageText = (string)reader["MessageText"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) msg.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["IsGroup"].ToString())) msg.IsGroup = (bool)reader["IsGroup"];
                    if (!string.IsNullOrEmpty(reader["IsSender"].ToString())) msg.IsSender = (bool)reader["IsSender"];
                    if (!string.IsNullOrEmpty(reader["Seen"].ToString())) msg.Seen = (bool)reader["Seen"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) msg.SenderUserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) msg.SenderUserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) msg.SenderFirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) msg.SenderLastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["HasAttachment"].ToString())) msg.HasAttachment = (bool)reader["HasAttachment"];

                    retMessages.Add(msg);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_forwarded_messages(ref IDataReader reader, ref List<Message> retFwdMessages)
        {
            while (reader.Read())
            {
                try
                {
                    Message fwd = new Message();

                    if (!string.IsNullOrEmpty(reader["MessageID"].ToString())) fwd.MessageID = (Guid)reader["MessageID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) fwd.SenderUserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) fwd.SenderUserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) fwd.SenderFirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) fwd.SenderLastName = (string)reader["SenderLastName"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) fwd.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["MessageText"].ToString())) fwd.MessageText = (string)reader["MessageText"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) fwd.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["IsGroup"].ToString())) fwd.IsGroup = (bool)reader["IsGroup"];
                    if (!string.IsNullOrEmpty(reader["ForwardedFrom"].ToString())) fwd.ForwardedFrom = (Guid)reader["ForwardedFrom"];
                    if (!string.IsNullOrEmpty(reader["Level"].ToString())) fwd.Level = (int)reader["Level"];
                    if (!string.IsNullOrEmpty(reader["HasAttachment"].ToString())) fwd.HasAttachment = (bool)reader["HasAttachment"];
                    
                    retFwdMessages.Add(fwd);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_thread_users(ref IDataReader reader, ref List<ThreadInfo> retThreadInfo)
        {
            while (reader.Read())
            {
                try
                {
                    Guid threadId = (Guid)reader["ThreadID"];

                    ThreadInfo th = retThreadInfo.Where(u => u.ThreadID == threadId).FirstOrDefault();

                    if (th == null)
                    {
                        th = new ThreadInfo() { ThreadID = threadId };
                        retThreadInfo.Add(th);
                    }

                    User usr = new User();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) usr.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) usr.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) usr.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) usr.LastName = (string)reader["LastName"];

                    int usersCount = 0;
                    if (!string.IsNullOrEmpty(reader["RevRowNumber"].ToString())) usersCount = (int)(long)reader["RevRowNumber"];
                    if (!th.UsersCount.HasValue || usersCount > th.UsersCount.Value) th.UsersCount = usersCount;

                    th.ThreadUsers.Add(usr);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_message_receivers(ref IDataReader reader, ref List<Message> retMessages)
        {
            while (reader.Read())
            {
                try
                {
                    Guid messageId = (Guid)reader["MessageID"];

                    Message msg = retMessages.Where(u => u.MessageID == messageId).FirstOrDefault();

                    if (msg == null)
                    {
                        msg = new Message() { MessageID = messageId };
                        retMessages.Add(msg);
                    }

                    User usr = new User();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) usr.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) usr.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) usr.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) usr.LastName = (string)reader["LastName"];

                    int receiversCount = 0;
                    if (!string.IsNullOrEmpty(reader["RevRowNumber"].ToString())) receiversCount = (int)(long)reader["RevRowNumber"];
                    if (!msg.ReceiversCount.HasValue || receiversCount > msg.ReceiversCount.Value) msg.ReceiversCount = receiversCount;

                    msg.ReceiverUsers.Add(usr);
                }
                catch (Exception e) { string s = e.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static void GetThreads(Guid applicationId, ref List<ThreadInfo> threads, Guid userId, int? count, int? lastId)
        {
            string spName = GetFullyQualifiedName("GetThreads");

            try
            {
                if (lastId <= 0) lastId = null;
                if (count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, count, lastId);
                _parse_threads(ref reader, ref threads);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
            }
        }

        public static void GetThreadInfo(Guid applicationId, Guid userId, Guid threadId, 
            ref int messagesCount, ref int sentCount, ref int notSeenCount)
        {
            string spName = GetFullyQualifiedName("GetThreadInfo");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, threadId);
                _parse_thread_info(ref reader, ref messagesCount, ref sentCount, ref notSeenCount);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
            }
        }

        public static void GetThreadUsers(Guid applicationId, 
            ref List<ThreadInfo> threads, List<Guid> threadIds, Guid userId, int? count, int? lastId)
        {
            string spName = GetFullyQualifiedName("GetThreadUsers");

            try
            {
                if (lastId <= 0) lastId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, ProviderUtil.list_to_string<Guid>(ref threadIds), ',', count, lastId);
                _parse_thread_users(ref reader, ref threads);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
            }
        }

        public static void GetMessages(Guid applicationId, ref List<Message> retMessages, Guid userId, Guid? threadId, 
            bool? sent, long? minId, int? count)
        {
            string spName = GetFullyQualifiedName("GetMessages");

            try
            {
                if (threadId == Guid.Empty) threadId = null;
                if (minId <= 0) minId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, threadId, sent, count, minId);
                _parse_messages(ref reader, ref retMessages);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
            }
        }

        public static bool HasMessage(Guid applicationId, long? id, Guid userId, Guid? threadId, Guid? messageId)
        {
            string spName = GetFullyQualifiedName("HasMessage");

            try
            {
                if (threadId == Guid.Empty) threadId = null;
                if (messageId == Guid.Empty) messageId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, userId, threadId, messageId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
                return false;
            }
        }

        public static long SendMessage(Guid applicationId, Guid messageId, Guid? forwardedFrom, Guid userId, 
            string title, string messageText, bool isGroup, List<Guid> receiverUserIds, Guid? threadId, 
            List<DocFileInfo> attachedFiles)
        {
            if(forwardedFrom == Guid.Empty) forwardedFrom = null;
            if(threadId == Guid.Empty) threadId = null;

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Receivers
            DataTable receiversTable = new DataTable();
            receiversTable.Columns.Add("Value", typeof(Guid));

            foreach (Guid uId in receiverUserIds)
                receiversTable.Rows.Add(uId);

            SqlParameter receiversParam = new SqlParameter("@Receivers", SqlDbType.Structured);
            receiversParam.TypeName = "[dbo].[GuidTableType]";
            receiversParam.Value = receiversTable;
            //end of Add Receivers

            //Add Attachments
            DataTable attachmentsTable = new DataTable();
            attachmentsTable.Columns.Add("FileID", typeof(Guid));
            attachmentsTable.Columns.Add("FileName", typeof(string));
            attachmentsTable.Columns.Add("Extension", typeof(string));
            attachmentsTable.Columns.Add("MIME", typeof(string));
            attachmentsTable.Columns.Add("Size", typeof(long));
            attachmentsTable.Columns.Add("OwnerID", typeof(Guid));
            attachmentsTable.Columns.Add("OwnerType", typeof(string));

            foreach (DocFileInfo _att in attachedFiles)
            {
                attachmentsTable.Rows.Add(_att.FileID, _att.FileName,
                    _att.Extension, _att.MIME(), _att.Size, _att.OwnerID, _att.OwnerType);
            }

            SqlParameter attachmentsParam = new SqlParameter("@Attachments", SqlDbType.Structured);
            attachmentsParam.TypeName = "[dbo].[DocFileInfoTableType]";
            attachmentsParam.Value = attachmentsTable;
            //end of Add Attachments

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            if (threadId.HasValue) cmd.Parameters.AddWithValue("@ThreadID", threadId.Value);
            cmd.Parameters.AddWithValue("@MessageID", messageId);
            if (forwardedFrom.HasValue) cmd.Parameters.AddWithValue("@ForwardedFrom", forwardedFrom.Value);
            if (!string.IsNullOrEmpty(title)) cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@MessageText", messageText);
            cmd.Parameters.AddWithValue("@IsGroup", isGroup);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);
            cmd.Parameters.Add(receiversParam);
            cmd.Parameters.Add(attachmentsParam);

            string spName = GetFullyQualifiedName("SendNewMessage");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@UserID" + sep + 
                (threadId.HasValue ? "@ThreadID" : "null") + sep + "@MessageID" + sep +
                (forwardedFrom.HasValue ? "@ForwardedFrom" : "null") + sep + 
                (!string.IsNullOrEmpty(title) ? "@Title" : "null") + sep +
                "@MessageText" + sep + "@IsGroup" + sep + "@Now" + sep + "@Receivers" + sep + "@Attachments";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed_long((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
                return 0;
            }
            finally { con.Close(); }
        }

        public static bool BulkSendMessage(Guid applicationId, List<Message> messages)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Messages & Recievers
            DataTable messagesTable = new DataTable();
            messagesTable.Columns.Add("MessageID", typeof(Guid));
            messagesTable.Columns.Add("SenderUserID", typeof(Guid));
            messagesTable.Columns.Add("Title", typeof(string));
            messagesTable.Columns.Add("MessageText", typeof(string));
            
            DataTable receiversTable = new DataTable();
            receiversTable.Columns.Add("FirstValue", typeof(Guid));
            receiversTable.Columns.Add("SecondValue", typeof(Guid));

            foreach (Message msg in messages)
            {
                if(msg.ReceiverUsers.Count > 0) messagesTable.Rows.Add(msg.MessageID, msg.SenderUserID.Value, msg.Title, msg.MessageText);

                foreach (User u in msg.ReceiverUsers) receiversTable.Rows.Add(msg.MessageID, u.UserID);
            }

            SqlParameter messagesParam = new SqlParameter("@Messages", SqlDbType.Structured);
            messagesParam.TypeName = "[dbo].[MessageTableType]";
            messagesParam.Value = messagesTable;

            SqlParameter receiversParam = new SqlParameter("@Receivers", SqlDbType.Structured);
            receiversParam.TypeName = "[dbo].[GuidPairTableType]";
            receiversParam.Value = receiversTable;
            //end of Add Messages & Receivers

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(messagesParam);
            cmd.Parameters.Add(receiversParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("BulkSendMessage");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Messages" + sep + "@Receivers" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool RemoveMessages(Guid applicationId, Guid? userId, Guid? threadId, long? id)
        {
            string spName = GetFullyQualifiedName("RemoveMessages");

            try
            {
                if (userId == Guid.Empty) userId = null;
                if (threadId == Guid.Empty) threadId = null;
                if (id <= 0) id = null;

                if (!id.HasValue && (!userId.HasValue || !threadId.HasValue)) return false;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, userId, threadId, id));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
                return false;
            }
        }

        public static void GetForwardedMessages(Guid applicationId, ref List<Message> fwdMessages, Guid messageId)
        {
            string spName = GetFullyQualifiedName("GetForwardedMessages");

            try
            {
                if (messageId == null || messageId == Guid.Empty) throw new Exception("messageID is null or empty!");

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, messageId);
                _parse_forwarded_messages(ref reader, ref fwdMessages);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
            }
        }

        public static bool SetMessagesAsSeen(Guid applicationId, Guid userId, Guid threadId)
        {
            string spName = GetFullyQualifiedName("SetMessagesAsSeen");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    userId, threadId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
                return false;
            }
        }

        public static int GetNotSeenMessagesCount(Guid applicationId, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetNotSeenMessagesCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
                return 0;
            }
        }

        public static void GetMessageReceivers(Guid applicationId, 
            ref List<Message> refMsg, List<Guid> MessagesIds, int? count, int? lastId)
        {
            string spName = GetFullyQualifiedName("GetMessageReceivers");

            try
            {
                if (lastId <= 0) lastId = null;
                if (count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref MessagesIds), ',', count, lastId);
                _parse_message_receivers(ref reader, ref refMsg);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.MSG);
            }
        }
    }
}
