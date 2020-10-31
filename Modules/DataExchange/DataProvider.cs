using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.DataExchange
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[DE_" + name + "]"; //'[dbo].' is database owner and 'DE_' is module qualifier
        }

        private static bool _parse_update_nodes_results(ref IDataReader reader, ref List<Guid> nodeIds)
        {
            try
            {
                reader.Read();

                try { return (bool)reader[0]; }
                catch { }
                
                bool result = long.Parse(reader[0].ToString()) > 0 ? true : false;

                if (result && reader.NextResult())
                {
                    try
                    {
                        while (reader.Read())
                            if (!string.IsNullOrEmpty(reader["ID"].ToString())) nodeIds.Add((Guid)reader["ID"]);
                    }
                    catch { }
                }

                return result;
            }
            catch { return false; }
            finally { if (!reader.IsClosed) reader.Close(); }
        }


        public static bool UpdateNodes(Guid applicationId, List<ExchangeNode> nodes, 
            Guid? nodeTypeId, string nodeTypeAdditionalId, Guid currentUserId, ref List<Guid> newNodeIds)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Nodes
            DataTable nodesTable = new DataTable();
            nodesTable.Columns.Add("NodeID", typeof(Guid));
            nodesTable.Columns.Add("NodeAdditionalID", typeof(string));
            nodesTable.Columns.Add("Name", typeof(string));
            nodesTable.Columns.Add("ParentAdditionalID", typeof(string));
            nodesTable.Columns.Add("Abstract", typeof(string));
            nodesTable.Columns.Add("Tags", typeof(string));

            foreach (ExchangeNode _nd in nodes)
            {
                if (_nd.NodeID == Guid.Empty) _nd.NodeID = null;
                if (string.IsNullOrEmpty(_nd.AdditionalID)) _nd.AdditionalID = null;
                if (string.IsNullOrEmpty(_nd.Name)) _nd.Name = string.Empty;
                if (string.IsNullOrEmpty(_nd.ParentAdditionalID)) _nd.ParentAdditionalID = null;
                if (string.IsNullOrEmpty(_nd.Abstract)) _nd.Abstract = string.Empty;
                if (string.IsNullOrEmpty(_nd.Tags)) _nd.Tags = string.Empty;

                if (_nd.Tags.Length > 1900) _nd.Tags = _nd.Tags.Substring(0, 1900);

                nodesTable.Rows.Add(_nd.NodeID, _nd.AdditionalID,
                    _nd.Name.Substring(0, Math.Min(250, _nd.Name.Length)), _nd.ParentAdditionalID, _nd.Abstract, _nd.Tags);
            }

            SqlParameter nodesParam = new SqlParameter("@Nodes", SqlDbType.Structured);
            nodesParam.TypeName = "[dbo].[ExchangeNodeTableType]";
            nodesParam.Value = nodesTable;
            //end of Add Nodes

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            if(nodeTypeId.HasValue) cmd.Parameters.AddWithValue("@NodeTypeID", nodeTypeId);
            if(!string.IsNullOrEmpty(nodeTypeAdditionalId))
                cmd.Parameters.AddWithValue("@NodeTypeAdditionalID", nodeTypeAdditionalId);
            cmd.Parameters.Add(nodesParam);
            cmd.Parameters.AddWithValue("@CreatorUserID", currentUserId);
            cmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdateNodes");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + 
                (nodeTypeId.HasValue ? "@NodeTypeID" : "null") + sep +
                (!string.IsNullOrEmpty(nodeTypeAdditionalId) ? "@NodeTypeAdditionalID" : "null") + sep +
                "@Nodes" + sep + "@CreatorUserID" + sep + "@CreationDate";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return _parse_update_nodes_results(ref reader, ref newNodeIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool UpdateNodeIDs(Guid applicationId, 
            Guid currentUserId, Guid nodeTypeId, List<KeyValuePair<string, string>> items)
        {
            string spName = GetFullyQualifiedName("UpdateNodeIDs");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeTypeId, 
                    string.Join(",", items.Select(i => i.Key + "|" + i.Value)), '|', ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool RemoveNodes(Guid applicationId, Guid currentUserId, List<KeyValuePair<string, string>> items)
        {
            string spName = GetFullyQualifiedName("RemoveNodes");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    string.Join(",", items.Select(i => i.Key + "|" + i.Value)), '|', ',', currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool UpdateUsers(Guid applicationId, List<ExchangeUser> users)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable usersTable = new DataTable();
            usersTable.Columns.Add("UserID", typeof(Guid));
            usersTable.Columns.Add("UserName", typeof(string));
            usersTable.Columns.Add("NewUserName", typeof(string));
            usersTable.Columns.Add("FirstName", typeof(string));
            usersTable.Columns.Add("LastName", typeof(string));
            usersTable.Columns.Add("EmploymentType", typeof(string));
            usersTable.Columns.Add("DepartmentID", typeof(string));
            usersTable.Columns.Add("IsManager", typeof(bool));
            usersTable.Columns.Add("Email", typeof(string));
            usersTable.Columns.Add("PhoneNumber", typeof(string));
            usersTable.Columns.Add("ResetPassword", typeof(bool));
            usersTable.Columns.Add("Password", typeof(string));
            usersTable.Columns.Add("PasswordSalt", typeof(string));
            usersTable.Columns.Add("EncryptedPassword", typeof(string));

            foreach (ExchangeUser _usr in users)
            {
                string strEmploymentType = null;
                if (_usr.EmploymentType != Users.EmploymentType.NotSet) strEmploymentType = _usr.EmploymentType.ToString();

                usersTable.Rows.Add(null, _usr.UserName, _usr.NewUserName, _usr.FirstName, _usr.LastName,
                    strEmploymentType, _usr.DepartmentID, _usr.IsManager, _usr.Email, _usr.PhoneNumber,
                    _usr.ResetPassword, _usr.Password.Salted, _usr.Password.Salt, _usr.Password.Encrypted);
            }

            SqlParameter usersParam = new SqlParameter("@Users", SqlDbType.Structured);
            usersParam.TypeName = "[dbo].[ExchangeUserTableType]";
            usersParam.Value = usersTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(usersParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdateUsers");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Users" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool UpdateMembers(Guid applicationId, List<ExchangeMember> members)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable membersTable = new DataTable();
            membersTable.Columns.Add("NodeTypeAdditionalID", typeof(string));
            membersTable.Columns.Add("NodeAdditionalID", typeof(string));
            membersTable.Columns.Add("NodeID", typeof(Guid));
            membersTable.Columns.Add("UserName", typeof(string));
            membersTable.Columns.Add("IsAdmin", typeof(bool));

            foreach (ExchangeMember _mbr in members)
                membersTable.Rows.Add(_mbr.NodeTypeAdditionalID, _mbr.NodeAdditionalID, _mbr.NodeID, _mbr.UserName, _mbr.IsAdmin);

            SqlParameter membersParam = new SqlParameter("@Members", SqlDbType.Structured);
            membersParam.TypeName = "[dbo].[ExchangeMemberTableType]";
            membersParam.Value = membersTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(membersParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdateMembers");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Members" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool UpdateExperts(Guid applicationId, List<ExchangeMember> experts)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable expertsTable = new DataTable();
            expertsTable.Columns.Add("NodeTypeAdditionalID", typeof(string));
            expertsTable.Columns.Add("NodeAdditionalID", typeof(string));
            expertsTable.Columns.Add("NodeID", typeof(Guid));
            expertsTable.Columns.Add("UserName", typeof(string));
            expertsTable.Columns.Add("IsAdmin", typeof(bool));
            
            foreach (ExchangeMember _xprt in experts)
                expertsTable.Rows.Add(_xprt.NodeTypeAdditionalID, _xprt.NodeAdditionalID, _xprt.NodeID, _xprt.UserName, false);

            SqlParameter expertsParam = new SqlParameter("@Experts", SqlDbType.Structured);
            expertsParam.TypeName = "[dbo].[ExchangeMemberTableType]";
            expertsParam.Value = expertsTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(expertsParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdateExperts");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@Experts" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool UpdateRelations(Guid applicationId, Guid currentUserId, List<ExchangeRelation> relations)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Users
            DataTable relationsTable = new DataTable();
            relationsTable.Columns.Add("SourceTypeAdditionalID", typeof(string));
            relationsTable.Columns.Add("SourceAdditionalID", typeof(string));
            relationsTable.Columns.Add("SourceID", typeof(Guid));
            relationsTable.Columns.Add("DestinationTypeAdditionalID", typeof(string));
            relationsTable.Columns.Add("DestinationAdditionalID", typeof(string));
            relationsTable.Columns.Add("DestinationID", typeof(Guid));
            relationsTable.Columns.Add("Bidirectional", typeof(bool));

            foreach (ExchangeRelation r in relations)
                relationsTable.Rows.Add(r.SourceTypeAdditionalID, r.SourceAdditionalID, r.SourceID,
                    r.DestinationTypeAdditionalID, r.DestinationAdditionalID, r.DestinationID, r.Bidirectional);

            SqlParameter relationsParam = new SqlParameter("@Relations", SqlDbType.Structured);
            relationsParam.TypeName = "[dbo].[ExchangeRelationTableType]";
            relationsParam.Value = relationsTable;
            //end of Add Users

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.Add(relationsParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdateRelations");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@CurrentUserID" + sep + "@Relations" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool UpdateAuthors(Guid applicationId, Guid currentUserId, List<ExchangeAuthor> authors)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Authors
            DataTable authorsTable = new DataTable();
            authorsTable.Columns.Add("NodeTypeAdditionalID", typeof(string));
            authorsTable.Columns.Add("NodeAdditionalID", typeof(string));
            authorsTable.Columns.Add("UserName", typeof(string));
            authorsTable.Columns.Add("Percentage", typeof(int));

            foreach (ExchangeAuthor a in authors)
                authorsTable.Rows.Add(a.NodeTypeAdditionalID, a.NodeAdditionalID, a.UserName, a.Percentage);

            SqlParameter authorsParam = new SqlParameter("@Authors", SqlDbType.Structured);
            authorsParam.TypeName = "[dbo].[ExchangeAuthorTableType]";
            authorsParam.Value = authorsTable;
            //end of Add Authors

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.Add(authorsParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdateAuthors");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@CurrentUserID" + sep + "@Authors" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool UpdateUserConfidentialities(Guid applicationId, Guid currentUserId, List<KeyValuePair<string, int>> items)
        {
            string spName = GetFullyQualifiedName("UpdateUserConfidentialities");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, currentUserId, 
                    string.Join(",", items.Select(i => i.Value.ToString() + "|" + i.Key)), '|', ',', DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
        }

        public static bool UpdatePermissions(Guid applicationId, Guid currentUserId, List<ExchangePermission> permissions)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Permissions
            DataTable permissionsTable = new DataTable();
            permissionsTable.Columns.Add("NodeTypeAdditionalID", typeof(string));
            permissionsTable.Columns.Add("NodeAdditionalID", typeof(string));
            permissionsTable.Columns.Add("GroupTypeAdditionalID", typeof(string));
            permissionsTable.Columns.Add("GroupAdditionalID", typeof(string));
            permissionsTable.Columns.Add("UserName", typeof(string));
            permissionsTable.Columns.Add("PermissionType", typeof(string));
            permissionsTable.Columns.Add("Allow", typeof(bool));
            permissionsTable.Columns.Add("DropAll", typeof(bool));

            foreach (ExchangePermission a in permissions)
                permissionsTable.Rows.Add(a.NodeTypeAdditionalID, a.NodeAdditionalID, 
                    a.GroupTypeAdditionalID, a.GroupAdditionalID, a.UserName, a.PermissionType.ToString(), a.Allow, a.DropAll);

            SqlParameter permissionsParam = new SqlParameter("@Items", SqlDbType.Structured);
            permissionsParam.TypeName = "[dbo].[ExchangePermissionTableType]";
            permissionsParam.Value = permissionsTable;
            //end of Add Permissions

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.Add(permissionsParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("UpdatePermissions");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@CurrentUserID" + sep + "@Items" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.DE);
                return false;
            }
            finally { con.Close(); }
        }
    }
}
