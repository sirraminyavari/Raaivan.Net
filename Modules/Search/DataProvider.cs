using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Search
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[SRCH_" + name + "]"; //'[dbo].' is database owner and 'SRCH_' is module qualifier
        }

        private static void _parse_search_docs(Guid applicationId,
            ref IDataReader reader, ref List<SearchDoc> _list, string itemType)
        {
            while (reader.Read())
            {
                try
                {
                    SearchDoc sd = new SearchDoc();
                    switch (itemType)
                    {
                        case "Node":
                            if (!string.IsNullOrEmpty(reader["ID"].ToString())) sd.ID = (Guid)reader["ID"];
                            if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) sd.Deleted = (bool)reader["Deleted"];
                            if (!string.IsNullOrEmpty(reader["TypeID"].ToString())) sd.TypeID = (Guid)reader["TypeID"];
                            if (!string.IsNullOrEmpty(reader["Type"].ToString())) sd.Type = (string)reader["Type"];
                            if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString())) sd.AdditionalID = (string)reader["AdditionalID"];
                            if (!string.IsNullOrEmpty(reader["Title"].ToString())) sd.Title = (string)reader["Title"];
                            if (!string.IsNullOrEmpty(reader["Description"].ToString())) sd.Description = (string)reader["Description"];
                            if (!string.IsNullOrEmpty(reader["Tags"].ToString())) sd.Tags = (string)reader["Tags"];
                            if (!string.IsNullOrEmpty(reader["Content"].ToString())) sd.Content = (string)reader["Content"];
                            if (!string.IsNullOrEmpty(reader["FileContent"].ToString())) sd.FileContent = (string)reader["FileContent"];
                            sd.SearchDocType = SearchDocType.Node;
                            break;
                        case "NodeType":
                            if (!string.IsNullOrEmpty(reader["ID"].ToString())) sd.ID = (Guid)reader["ID"];
                            if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) sd.Deleted = (bool)reader["Deleted"];
                            if (!string.IsNullOrEmpty(reader["Title"].ToString())) sd.Title = (string)reader["Title"];
                            if (!string.IsNullOrEmpty(reader["Description"].ToString())) sd.Description = (string)reader["Description"];
                            sd.SearchDocType = SearchDocType.NodeType;
                            break;
                        case "Question":
                            if (!string.IsNullOrEmpty(reader["ID"].ToString())) sd.ID = (Guid)reader["ID"];
                            if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) sd.Deleted = (bool)reader["Deleted"];
                            if (!string.IsNullOrEmpty(reader["Title"].ToString())) sd.Title = (string)reader["Title"];
                            if (!string.IsNullOrEmpty(reader["Description"].ToString())) sd.Description = (string)reader["Description"];
                            if (!string.IsNullOrEmpty(reader["Content"].ToString())) sd.Content = (string)reader["Content"];
                            sd.SearchDocType = SearchDocType.Question;
                            break;
                        case "File":
                            if (!string.IsNullOrEmpty(reader["ID"].ToString())) sd.ID = (Guid)reader["ID"];
                            if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) sd.Deleted = (bool)reader["Deleted"];
                            if (!string.IsNullOrEmpty(reader["Type"].ToString())) sd.Type = (string)reader["Type"];
                            if (!string.IsNullOrEmpty(reader["Title"].ToString())) sd.Title = (string)reader["Title"];
                            if (!string.IsNullOrEmpty(reader["FileContent"].ToString())) sd.FileContent = (string)reader["FileContent"];
                            sd.SearchDocType = SearchDocType.File;
                            break;
                        case "User":
                            if (!string.IsNullOrEmpty(reader["ID"].ToString())) sd.ID = (Guid)reader["ID"];
                            if (!string.IsNullOrEmpty(reader["Deleted"].ToString())) sd.Deleted = (bool)reader["Deleted"];
                            if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString())) sd.AdditionalID = (string)reader["AdditionalID"];
                            if (!string.IsNullOrEmpty(reader["Title"].ToString())) sd.Title = (string)reader["Title"];
                            sd.SearchDocType = SearchDocType.User;
                            break;
                    }

                    if (!string.IsNullOrEmpty(sd.Description)) sd.Description = 
                            PublicMethods.markup2plaintext(applicationId,
                        Expressions.replace(sd.Description, Expressions.Patterns.HTMLTag, " "));
                    if (!string.IsNullOrEmpty(sd.Content)) sd.Content = 
                            PublicMethods.markup2plaintext(applicationId,
                        Expressions.replace(sd.Content, Expressions.Patterns.HTMLTag, " "));

                    _list.Add(sd);
                }
                catch { };
            }
            if (!reader.IsClosed) reader.Close();
        }

        public static void SearchUsers(Guid applicationId, ref List<User> retUsers, string searchText,
            ref List<Guid> departmentIds, ref List<Guid> expertiseKDIds, ref List<Guid> projectIds, 
            ref List<Guid> processIds, ref List<Guid> communityIds, ref List<Guid> knowledgeKds, 
            int? count, Guid? minId)
        {
            string spName = GetFullyQualifiedName("SearchUsers");

            try
            {
                List<Guid> _userIds = new List<Guid>();

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, count, minId, 
                    ProviderUtil.get_search_text(searchText), ProviderUtil.list_to_string<Guid>(ref departmentIds),
                    ProviderUtil.list_to_string<Guid>(ref expertiseKDIds), ProviderUtil.list_to_string<Guid>(ref projectIds),
                    ProviderUtil.list_to_string<Guid>(ref processIds), ProviderUtil.list_to_string<Guid>(ref communityIds),
                    ProviderUtil.list_to_string<Guid>(ref knowledgeKds), ',');

                ProviderUtil.parse_guids(ref reader, ref _userIds);
                retUsers = UsersController.get_users(applicationId, _userIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SRCH);
            }
        }

        public static void GetIndexQueueItems(Guid applicationId, 
            ref List<SearchDoc> _list, int count, string itemType)
        {
            string spName = GetFullyQualifiedName("GetIndexQueueItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, count, itemType);
                _parse_search_docs(applicationId, ref reader, ref _list, itemType);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SRCH);
            }
        }

        public static bool SetIndexLastUpdateDate(Guid applicationId, SearchDocType itemType, List<Guid> IDs)
        {
            string spName = GetFullyQualifiedName("SetIndexLastUpdateDate");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    itemType.ToString(), ProviderUtil.list_to_string<Guid>(ref IDs), ',', DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.SRCH);
                return false;
            }
        }
    }
}
