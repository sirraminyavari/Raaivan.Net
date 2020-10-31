using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Wiki
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[WK_" + name + "]"; //'[dbo].' is database owner and 'WK_' is module qualifier
        }
        
        private static void _parse_titles(ref IDataReader reader, ref List<WikiTitle> lstTitles)
        {
            while (reader.Read())
            {
                try
                {
                    WikiTitle title = new WikiTitle();

                    if (!string.IsNullOrEmpty(reader["TitleID"].ToString())) title.TitleID = (Guid)reader["TitleID"];
                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) title.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) title.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["SequenceNumber"].ToString())) title.SequenceNumber = (int)reader["SequenceNumber"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) title.CreatorUserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString())) title.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["LastModificationDate"].ToString()))
                        title.LastModificationDate = (DateTime)reader["LastModificationDate"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) title.Status = (string)reader["Status"];

                    lstTitles.Add(title);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_paragraphs(ref IDataReader reader, ref List<Paragraph> lstParagraphs)
        {
            while (reader.Read())
            {
                try
                {
                    Paragraph paragraph = new Paragraph();

                    if (!string.IsNullOrEmpty(reader["ParagraphID"].ToString())) paragraph.ParagraphID = (Guid)reader["ParagraphID"];
                    if (!string.IsNullOrEmpty(reader["TitleID"].ToString())) paragraph.TitleID = (Guid)reader["TitleID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) paragraph.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["BodyText"].ToString())) paragraph.BodyText = (string)reader["BodyText"];
                    if (!string.IsNullOrEmpty(reader["SequenceNumber"].ToString())) paragraph.SequenceNumber = (int)reader["SequenceNumber"];
                    if (!string.IsNullOrEmpty(reader["IsRichText"].ToString())) paragraph.IsRichText = (bool)reader["IsRichText"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) paragraph.CreatorUserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString())) paragraph.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["LastModificationDate"].ToString())) 
                        paragraph.LastModificationDate = (DateTime)reader["LastModificationDate"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) paragraph.Status = (string)reader["Status"];

                    lstParagraphs.Add(paragraph);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_changes(ref IDataReader reader, ref List<Change> lstChanges)
        {
            while (reader.Read())
            {
                try
                {
                    Change change = new Change();

                    if (!string.IsNullOrEmpty(reader["ChangeID"].ToString())) change.ChangeID = (Guid)reader["ChangeID"];
                    if (!string.IsNullOrEmpty(reader["ParagraphID"].ToString())) change.ParagraphID = (Guid)reader["ParagraphID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) change.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["BodyText"].ToString())) change.BodyText = (string)reader["BodyText"];
                    if (!string.IsNullOrEmpty(reader["Status"].ToString())) change.Status = (string)reader["Status"];
                    if (!string.IsNullOrEmpty(reader["Applied"].ToString())) change.Applied = (bool)reader["Applied"];
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) change.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["SenderUserID"].ToString())) change.Sender.UserID = (Guid)reader["SenderUserID"];
                    if (!string.IsNullOrEmpty(reader["SenderUserName"].ToString())) change.Sender.UserName = (string)reader["SenderUserName"];
                    if (!string.IsNullOrEmpty(reader["SenderFirstName"].ToString())) change.Sender.FirstName = (string)reader["SenderFirstName"];
                    if (!string.IsNullOrEmpty(reader["SenderLastName"].ToString())) change.Sender.LastName = (string)reader["SenderLastName"];

                    lstChanges.Add(change);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_wiki_owner(ref IDataReader reader, ref Guid ownerId, ref WikiOwnerType ownerType)
        {
                try
                {
                    reader.Read();

                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) ownerId = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["OwnerType"].ToString()))
                    {
                        try { ownerType = (WikiOwnerType)Enum.Parse(typeof(WikiOwnerType), (string)reader["OwnerType"]); }
                        catch { ownerType = WikiOwnerType.NotSet; }
                    }

                }
                catch { }

            if (!reader.IsClosed) reader.Close();
        }

        public static bool AddTitle(Guid applicationId, WikiTitle Info, bool? accept)
        {
            string spName = GetFullyQualifiedName("AddTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.TitleID, Info.OwnerID, Info.Title, Info.SequenceNumber, Info.CreatorUserID, Info.CreationDate,
                    Info.OwnerType, accept));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool ModifyTitle(Guid applicationId, WikiTitle Info, bool? accept)
        {
            string spName = GetFullyQualifiedName("ModifyTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.TitleID, Info.Title, Info.LastModifierUserID, Info.LastModificationDate, accept));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool ArithmeticDeleteTitle(Guid applicationId, Guid titleId, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    titleId, lastModifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool RecycleTitle(Guid applicationId, Guid titleId, Guid lastModifierUserId)
        {
            string spName = GetFullyQualifiedName("RecycleTitle");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    titleId, lastModifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool SetTitlesOrder(Guid applicationId, List<Guid> titleIds)
        {
            string spName = GetFullyQualifiedName("SetTitlesOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(titleIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static void GetTitles(Guid applicationId, ref List<WikiTitle> retTitles, 
            Guid ownerId, bool? isAdmin, Guid? currentUserId, bool deleted)
        {
            string spName = GetFullyQualifiedName("GetTitles");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, isAdmin, currentUserId, deleted);
                _parse_titles(ref reader, ref retTitles);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static void GetTitles(Guid applicationId, 
            ref List<WikiTitle> retTitles, ref List<Guid> titleIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("GetTitlesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref titleIds), ',', currentUserId);
                _parse_titles(ref reader, ref retTitles);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static bool HasTitle(Guid applicationId, Guid ownerId, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("HasTitle");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, ownerId, currentUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool AddParagraph(Guid applicationId, Paragraph Info, bool? sendToAdmins, bool? hasAdmin, 
            List<Guid> adminUserIds, ref List<Dashboard> dashboards)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            if (string.IsNullOrEmpty(Info.BodyText)) Info.BodyText = string.Empty;

            //Add Users
            DataTable usersTable = new DataTable();
            usersTable.Columns.Add("Value", typeof(Guid));

            adminUserIds = adminUserIds.Distinct().ToList();
            foreach (Guid uid in adminUserIds)usersTable.Rows.Add(uid);

            SqlParameter usersParam = new SqlParameter("@AdminUserIDs", SqlDbType.Structured);
            usersParam.TypeName = "[dbo].[GuidTableType]";
            usersParam.Value = usersTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@ParagraphID", Info.ParagraphID);
            cmd.Parameters.AddWithValue("@TitleID", Info.TitleID);
            if(!string.IsNullOrEmpty(Info.Title)) cmd.Parameters.AddWithValue("@Title", Info.Title);
            cmd.Parameters.AddWithValue("@BodyText", Info.BodyText);
            if (Info.SequenceNumber.HasValue) cmd.Parameters.AddWithValue("@SequenceNumber", Info.SequenceNumber);
            if (Info.CreatorUserID.HasValue) cmd.Parameters.AddWithValue("@CreatorUserID", Info.CreatorUserID);
            if (Info.CreationDate.HasValue) cmd.Parameters.AddWithValue("@CreationDate", Info.CreationDate);
            if (Info.IsRichText.HasValue) cmd.Parameters.AddWithValue("@IsRichText", Info.IsRichText);
            if (sendToAdmins.HasValue) cmd.Parameters.AddWithValue("@SendToAdmins", sendToAdmins);
            if (hasAdmin.HasValue) cmd.Parameters.AddWithValue("@HasAdmin", hasAdmin);
            cmd.Parameters.Add(usersParam);

            string spName = GetFullyQualifiedName("AddParagraph");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@ParagraphID" + sep + "@TitleID" + sep +
                (string.IsNullOrEmpty(Info.Title) ? "null" : "@Title") + sep +
                "@BodyText" + sep +
                (!Info.SequenceNumber.HasValue ? "null" : "@SequenceNumber") + sep +
                (!Info.CreatorUserID.HasValue ? "null" : "@CreatorUserID") + sep +
                (!Info.CreationDate.HasValue ? "null" : "@CreationDate") + sep +
                (!Info.IsRichText.HasValue ? "null" : "@IsRichText") + sep +
                (!sendToAdmins.HasValue ? "null" : "@SendToAdmins") + sep +
                (!hasAdmin.HasValue ? "null" : "@HasAdmin") + sep +
                "@AdminUserIDs";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return ProviderUtil.parse_dashboards(ref reader, ref dashboards) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool ModifyParagraph(Guid applicationId, Paragraph Info, Guid? changeId2Accept, bool? hasAdmin, 
            List<Guid> adminUserIds, ref List<Dashboard> dashboards, bool? citationNeeded, bool? apply, bool? accept)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            if (string.IsNullOrEmpty(Info.BodyText)) Info.BodyText = string.Empty;
            if (changeId2Accept == Guid.Empty) changeId2Accept = null;

            //Add Users
            DataTable usersTable = new DataTable();
            usersTable.Columns.Add("Value", typeof(Guid));

            adminUserIds = adminUserIds.Distinct().ToList();
            foreach (Guid uid in adminUserIds) usersTable.Rows.Add(uid);

            SqlParameter usersParam = new SqlParameter("@AdminUserIDs", SqlDbType.Structured);
            usersParam.TypeName = "[dbo].[GuidTableType]";
            usersParam.Value = usersTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@ParagraphID", Info.ParagraphID);
            if (changeId2Accept.HasValue) cmd.Parameters.AddWithValue("@ChangeID2Accept", changeId2Accept);
            if(!string.IsNullOrEmpty(Info.Title)) cmd.Parameters.AddWithValue("@Title", Info.Title);
            cmd.Parameters.AddWithValue("@BodyText", Info.BodyText);
            if(Info.LastModifierUserID.HasValue) cmd.Parameters.AddWithValue("@LastModifierUserID", Info.LastModifierUserID);
            if(Info.LastModificationDate.HasValue) cmd.Parameters.AddWithValue("@LastModificationDate", Info.LastModificationDate);
            if(citationNeeded.HasValue) cmd.Parameters.AddWithValue("@CitationNeeded", citationNeeded);
            if(apply.HasValue) cmd.Parameters.AddWithValue("@Apply", apply);
            if(accept.HasValue) cmd.Parameters.AddWithValue("@Accept", accept);
            if(hasAdmin.HasValue) cmd.Parameters.AddWithValue("@HasAdmin", hasAdmin);
            cmd.Parameters.Add(usersParam);

            string spName = GetFullyQualifiedName("ModifyParagraph");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@ParagraphID" + sep +
                (!changeId2Accept.HasValue ? "null" : "@ChangeID2Accept") + sep + 
                (string.IsNullOrEmpty(Info.Title) ? "null" : "@Title") + sep + 
                "@BodyText" + sep +
                (!Info.LastModifierUserID.HasValue ? "null" : "@LastModifierUserID") + sep +
                (!Info.LastModificationDate.HasValue ? "null" : "@LastModificationDate") + sep +
                (!citationNeeded.HasValue ? "null" : "@CitationNeeded") + sep +
                (!apply.HasValue ? "null" : "@Apply") + sep +
                (!accept.HasValue ? "null" : "@Accept") + sep +
                (!hasAdmin.HasValue ? "null" : "@HasAdmin") + sep +
                "@AdminUserIDs";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return ProviderUtil.parse_dashboards(ref reader, ref dashboards) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool ArithmeticDeleteParagraph(Guid applicationId, Guid paragraphId, Guid modifierUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteParagraph");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    paragraphId, modifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool RecycleParagraph(Guid applicationId, Guid paragraphId, Guid modifierUserId)
        {
            string spName = GetFullyQualifiedName("RecycleParagraph");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    paragraphId, modifierUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool SetParagraphsOrder(Guid applicationId, List<Guid> paragraphIds)
        {
            string spName = GetFullyQualifiedName("SetParagraphsOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(paragraphIds), ','));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static void GetParagraphs(Guid applicationId, 
            ref List<Paragraph> retParagraphs, ref List<Guid> paragraphIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("GetParagraphsByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref paragraphIds), ',', currentUserId);
                _parse_paragraphs(ref reader, ref retParagraphs);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }
        
        public static void GetTitleParagraphs(Guid applicationId, ref List<Paragraph> retSubjects, 
            ref List<Guid> titleIds, bool? isAdmin, Guid? currentUserId, bool removed)
        {
            string spName = GetFullyQualifiedName("GetParagraphs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref titleIds), ',', isAdmin, currentUserId, removed);
                _parse_paragraphs(ref reader, ref retSubjects);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static bool HasParagraph(Guid applicationId, Guid titleOrOwnerId, Guid? currentUserId)
        {
            string spName = GetFullyQualifiedName("HasParagraph");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    titleOrOwnerId, currentUserId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static void GetDashboardParagraphs(Guid applicationId, ref List<Paragraph> retSubjects, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetDashboardParagraphs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId);
                _parse_paragraphs(ref reader, ref retSubjects);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static void GetParagraphRelatedUsers(Guid applicationId, ref List<User> retUsers, Guid paragraphId)
        {
            string spName = GetFullyQualifiedName("GetParagraphRelatedUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, paragraphId);

                List<Guid> lstUserIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref lstUserIds);
                retUsers = UsersController.get_users(applicationId, lstUserIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static bool RejectChange(Guid applicationId, Guid changeId, Guid evaluatorUserId)
        {
            string spName = GetFullyQualifiedName("RejectChange");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    changeId, evaluatorUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool AcceptChange(Guid applicationId, Guid changeId, Guid evaluatorUserId)
        {
            string spName = GetFullyQualifiedName("AcceptChange");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    changeId, evaluatorUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static bool ArithmeticDeleteChange(Guid applicationId, Guid changeId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteChange");

            try { return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, changeId)); }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return false;
            }
        }

        public static void GetChanges(Guid applicationId, ref List<Change> retChanges, ref List<Guid> ChangeIDs)
        {
            string spName = GetFullyQualifiedName("GetChangesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref ChangeIDs), ',');
                _parse_changes(ref reader, ref retChanges);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static void GetChanges(Guid applicationId, ref List<Change> retChanges, ref List<Guid> paragraphIds, 
            Guid? creatorUserId, WikiStatuses? status, bool? applied)
        {
            string spName = GetFullyQualifiedName("GetParagraphChanges");

            try
            {
                string strStatus = null;
                if (status.HasValue) strStatus = status.ToString();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref paragraphIds), ',', creatorUserId, strStatus, applied);
                _parse_changes(ref reader, ref retChanges);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static Change GetLastPendingChange(Guid applicationId, Guid paragraphId, Guid userId)
        {
            string spName = GetFullyQualifiedName("GetLastPendingChange");

            try
            {
                List<Change> lstChanges = new List<Change>();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, paragraphId, userId);
                _parse_changes(ref reader, ref lstChanges);

                return lstChanges.FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return null;
            }
        }

        public static void GetChangedWikiOwnerIDs(Guid applicationId, ref List<Guid> retIds, ref List<Guid> ownerIds)
        {
            string spName = GetFullyQualifiedName("GetChangedWikiOwnerIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref ownerIds), ',');
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static void GetWikiOwner(Guid applicationId, ref Guid ownerId, ref WikiOwnerType ownerType, Guid id)
        {
            string spName = GetFullyQualifiedName("GetWikiOwner");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, id);
                _parse_wiki_owner(ref reader, ref ownerId, ref ownerType);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
            }
        }

        public static string GetWikiContent(Guid applicationId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetWikiContent");

            try { return ProviderUtil.succeed_string(ProviderUtil.execute_reader(spName, applicationId, ownerId)); }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return null;
            }
        }

        public static int GetTitlesCount(Guid applicationId, 
            Guid ownerId, bool? isAdmin, Guid? currentUserId, bool? removed)
        {
            string spName = GetFullyQualifiedName("GetTitlesCount");

            try
            {
                return ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId,
                    ownerId, isAdmin, currentUserId, removed));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return 0;
            }
        }

        public static Dictionary<Guid, int> GetParagraphsCount(Guid applicationId, List<Guid> titleIds, 
            bool? isAdmin, Guid? currentUserId, bool? removed)
        {
            string spName = GetFullyQualifiedName("GetParagraphsCount");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(titleIds), ',', isAdmin, currentUserId, removed);
                return ProviderUtil.parse_items_count(ref reader);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return new Dictionary<Guid, int>();
            }
        }

        public static Dictionary<Guid, int> GetChangesCount(Guid applicationId, 
            List<Guid> paragraphIds, bool? applied)
        {
            string spName = GetFullyQualifiedName("GetChangesCount");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(paragraphIds), ',', applied);
                return ProviderUtil.parse_items_count(ref reader);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return new Dictionary<Guid, int>();
            }
        }

        public static DateTime? LastModificationDate(Guid applicationId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("LastModificationDate");

            try { return ProviderUtil.succeed_datetime(ProviderUtil.execute_reader(spName, applicationId, ownerId)); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return null;
            }
        }

        public static List<KeyValuePair<Guid, int>> WikiAuthors(Guid applicationId, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("WikiAuthors");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                return ProviderUtil.parse_items_count_list(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.WK);
                return new List<KeyValuePair<Guid, int>>();
            }
        }
    }
}
