using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Privacy
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[PRVC_" + name + "]"; //'[dbo].' is database owner and 'PRVC_' is module qualifier
        }

        private static Dictionary<Guid, List<Audience>> _parse_audience(ref IDataReader reader)
        {
            Dictionary<Guid, List<Audience>> ret = new Dictionary<Guid, List<Audience>>();

            while (reader.Read())
            {
                try
                {
                    Audience audience = new Audience();

                    if (!string.IsNullOrEmpty(reader["ObjectID"].ToString())) audience.ObjectID = (Guid)reader["ObjectID"];
                    if (!string.IsNullOrEmpty(reader["RoleID"].ToString())) audience.RoleID = (Guid)reader["RoleID"];
                    
                    PermissionType tp = PermissionType.None;
                    if (!Enum.TryParse<PermissionType>(reader["PermissionType"].ToString(), out tp) ||
                        tp == PermissionType.None) continue;
                    audience.PermissionType = tp;
                    
                    if (!string.IsNullOrEmpty(reader["Allow"].ToString())) audience.Allow = (bool)reader["Allow"];
                    if (!string.IsNullOrEmpty(reader["ExpirationDate"].ToString()))
                        audience.ExpirationDate = (DateTime)reader["ExpirationDate"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) audience.RoleName = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["Type"].ToString())) audience.RoleType = (string)reader["Type"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString()))
                        audience.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString()))
                        audience.AdditionalID = (string)reader["AdditionalID"];

                    if (!audience.ObjectID.HasValue) continue;

                    if (!ret.ContainsKey(audience.ObjectID.Value)) ret[audience.ObjectID.Value] = new List<Audience>();

                    ret[audience.ObjectID.Value].Add(audience);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        private static Dictionary<Guid, List<PermissionType>> _parse_access_checked_items(ref IDataReader reader)
        {
            Dictionary<Guid, List<PermissionType>> ret = new Dictionary<Guid, List<PermissionType>>();

            while (reader.Read())
            {
                try
                {
                    Guid id = Guid.Empty;
                    PermissionType type = PermissionType.None;


                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) id = (Guid)reader["ID"];

                    if (!string.IsNullOrEmpty(reader["Type"].ToString()) &&
                        Enum.TryParse<PermissionType>((string)reader["Type"], out type) &&
                        type != PermissionType.None && id != Guid.Empty)
                    {
                        if (!ret.ContainsKey(id)) ret[id] = new List<PermissionType>();
                        if (!ret[id].Any(u => u == type)) ret[id].Add(type);
                    }
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        private static Dictionary<Guid, List<DefaultPermission>> _parse_default_permissions(ref IDataReader reader)
        {
            Dictionary<Guid, List<DefaultPermission>> ret = new Dictionary<Guid, List<DefaultPermission>>();

            while (reader.Read())
            {
                try
                {
                    Guid id = Guid.Empty;
                    PermissionType type = PermissionType.None;
                    PrivacyType defaultValue = PrivacyType.NotSet;


                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) id = (Guid)reader["ID"];

                    if (!string.IsNullOrEmpty(reader["Type"].ToString()) &&
                        !string.IsNullOrEmpty(reader["DefaultValue"].ToString()) &&
                        Enum.TryParse<PermissionType>((string)reader["Type"], out type) &&
                        Enum.TryParse<PrivacyType>((string)reader["DefaultValue"], out defaultValue) &&
                        id != Guid.Empty && type != PermissionType.None && defaultValue != PrivacyType.NotSet)
                    {
                        if (!ret.ContainsKey(id)) ret[id] = new List<DefaultPermission>();
                        if (!ret[id].Any(u => u.PermissionType == type)) ret[id].Add(new DefaultPermission()
                        {
                            PermissionType = type,
                            DefaultValue = defaultValue
                        });
                    }
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        private static List<Privacy> _parse_settings(ref IDataReader reader)
        {
            List<Privacy> ret = new List<Privacy>();

            while (reader.Read())
            {
                try
                {
                    Privacy p = new Privacy();

                    if (!string.IsNullOrEmpty(reader["ObjectID"].ToString())) p.ObjectID = (Guid)reader["ObjectID"];
                    if (!string.IsNullOrEmpty(reader["CalculateHierarchy"].ToString()))
                        p.CalculateHierarchy = (bool)reader["CalculateHierarchy"];
                    if (!string.IsNullOrEmpty(reader["ConfidentialityID"].ToString()))
                        p.Confidentiality.ID = (Guid)reader["ConfidentialityID"];
                    if (!string.IsNullOrEmpty(reader["LevelID"].ToString()))
                        p.Confidentiality.LevelID = (int)reader["LevelID"];
                    if (!string.IsNullOrEmpty(reader["Level"].ToString())) p.Confidentiality.Title = (string)reader["Level"];

                    ret.Add(p);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return ret;
        }

        private static void _parse_user_ids(ref IDataReader reader, ref List<Guid> userIds, ref long totalCount)
        {
            while (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) userIds.Add((Guid)reader["UserID"]);
                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];

                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_confidentiality_levels(ref IDataReader reader, ref List<ConfidentialityLevel> lstLevels)
        {
            while (reader.Read())
            {
                try
                {
                    ConfidentialityLevel confLevel = new ConfidentialityLevel();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) confLevel.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["LevelID"].ToString())) confLevel.LevelID = (int)reader["LevelID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) confLevel.Title = (string)reader["Title"];

                    lstLevels.Add(confLevel);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        public static bool InitializeConfidentialityLevels(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("InitializeConfidentialityLevels");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }

        public static bool RefineAccessRoles(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("RefineAccessRoles");

            try
            {
                return ProviderUtil.succeed((IDataReader)ProviderUtil.execute_reader(spName, applicationId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }
        
        public static bool SetAudience(Guid applicationId, List<Privacy> items, Guid currentUserId)
        {
            if (items == null || items.Count == 0) return true;

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add ObjectIDs
            DataTable objectIdsTable = new DataTable();
            objectIdsTable.Columns.Add("Value", typeof(Guid));

            foreach (Privacy p in items)
                if (p.ObjectID.HasValue) objectIdsTable.Rows.Add(p.ObjectID);

            SqlParameter objectIdsParam = new SqlParameter("@ObjectIDs", SqlDbType.Structured);
            objectIdsParam.TypeName = "[dbo].[GuidTableType]";
            objectIdsParam.Value = objectIdsTable;
            //end of Add ObjectIDs

            //Add Default Permissions
            DataTable defaultPermissionsTable = new DataTable();
            defaultPermissionsTable.Columns.Add("GuidValue", typeof(Guid));
            defaultPermissionsTable.Columns.Add("FirstValue", typeof(string));
            defaultPermissionsTable.Columns.Add("SecondValue", typeof(string));

            foreach (Privacy p in items)
            {
                if (!p.ObjectID.HasValue) continue;

                foreach (DefaultPermission d in p.DefaultPermissions)
                {
                    if (d.PermissionType == PermissionType.None || d.DefaultValue == PrivacyType.NotSet) continue;

                    defaultPermissionsTable.Rows.Add(p.ObjectID, d.PermissionType.ToString(),
                        d.DefaultValue == PrivacyType.NotSet ? null : d.DefaultValue.ToString());
                }
            }

            SqlParameter defaultPermissionsParam = new SqlParameter("@DefaultPermissions", SqlDbType.Structured);
            defaultPermissionsParam.TypeName = "[dbo].[GuidStringPairTableType]";
            defaultPermissionsParam.Value = defaultPermissionsTable;
            //end of Add Default Permissions

            //Add Audience
            DataTable audienceTable = new DataTable();
            audienceTable.Columns.Add("ObjectID", typeof(Guid));
            audienceTable.Columns.Add("RoleID", typeof(Guid));
            audienceTable.Columns.Add("PermissionType", typeof(string));
            audienceTable.Columns.Add("Allow", typeof(bool));
            audienceTable.Columns.Add("ExpirationDate", typeof(DateTime));

            foreach (Privacy p in items)
            {
                if (!p.ObjectID.HasValue) continue;

                foreach (Audience a in p.Audience)
                {
                    if (!a.RoleID.HasValue || !a.Allow.HasValue || a.PermissionType == PermissionType.None) continue;

                    audienceTable.Rows.Add(p.ObjectID, a.RoleID, a.PermissionType.ToString(), a.Allow, a.ExpirationDate);
                }
            }

            SqlParameter audienceParam = new SqlParameter("@Audience", SqlDbType.Structured);
            audienceParam.TypeName = "[dbo].[PrivacyAudienceTableType]";
            audienceParam.Value = audienceTable;
            //end of Add Audience

            //Add Settings
            DataTable settingsTable = new DataTable();
            settingsTable.Columns.Add("FirstValue", typeof(Guid));
            settingsTable.Columns.Add("SecondValue", typeof(Guid));
            settingsTable.Columns.Add("BitValue", typeof(bool));

            foreach (Privacy p in items)
            {
                if (!p.ObjectID.HasValue || (!p.Confidentiality.LevelID.HasValue && !p.CalculateHierarchy.HasValue)) continue;

                settingsTable.Rows.Add(p.ObjectID, p.Confidentiality.ID, p.CalculateHierarchy);
            }

            SqlParameter settingsParam = new SqlParameter("@Settings", SqlDbType.Structured);
            settingsParam.TypeName = "[dbo].[GuidPairBitTableType]";
            settingsParam.Value = settingsTable;
            //end of Add Settings
            
            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.Add(objectIdsParam);
            cmd.Parameters.Add(defaultPermissionsParam);
            cmd.Parameters.Add(audienceParam);
            cmd.Parameters.Add(settingsParam);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);
            
            string spName = GetFullyQualifiedName("SetAudience");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@ObjectIDs" + sep + "@DefaultPermissions" + sep +
                "@Audience" + sep + "@Settings" + sep + "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
            finally { con.Close(); }
        }

        public static Dictionary<Guid, List<PermissionType>> CheckAccess(Guid applicationId, 
            Guid? userId, List<Guid> objectIds, PrivacyObjectType objectType, List<PermissionType> permissions)
        {
            if (!userId.HasValue) userId = Guid.NewGuid();

            if (objectIds.Count == 0) return new Dictionary<Guid, List<PermissionType>>();

            if (permissions.Count == 0)
            {
                foreach (string s in Enum.GetNames(typeof(PermissionType)))
                {
                    PermissionType pt = PermissionType.None;
                    if (Enum.TryParse<PermissionType>(s, out pt) && pt != PermissionType.None) permissions.Add(pt);
                }
            }
            
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add ObjectIDs
            DataTable objectIdsTable = new DataTable();
            objectIdsTable.Columns.Add("Value", typeof(Guid));

            foreach (Guid id in objectIds)
                objectIdsTable.Rows.Add(id);

            SqlParameter objectIdsParam = new SqlParameter("@ObjectIDs", SqlDbType.Structured);
            objectIdsParam.TypeName = "[dbo].[GuidTableType]";
            objectIdsParam.Value = objectIdsTable;
            //end of Add ObjectIDs

            //Add Permissions
            DataTable permissionsTable = new DataTable();
            permissionsTable.Columns.Add("GuidValue", typeof(string));
            permissionsTable.Columns.Add("FirstValue", typeof(string));

            foreach (PermissionType p in permissions)
            {
                if (p == PermissionType.None) continue;

                List<PermissionType> defaultItems = new List<PermissionType>() {
                    PermissionType.Create,
                    PermissionType.View,
                    PermissionType.ViewAbstract,
                    PermissionType.ViewRelatedItems,
                    PermissionType.Download
                };

                string defaultPrivacy = defaultItems.Any(d => d == p) ? RaaiVanSettings.DefaultPrivacy(applicationId) : string.Empty;

                permissionsTable.Rows.Add(p.ToString(), defaultPrivacy);
            }

            SqlParameter permissionsParam = new SqlParameter("@Permissions", SqlDbType.Structured);
            permissionsParam.TypeName = "[dbo].[StringPairTableType]";
            permissionsParam.Value = permissionsTable;
            //end of Add Permissions
            
            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            if (objectType != PrivacyObjectType.None) cmd.Parameters.AddWithValue("@ObjectType", objectType.ToString());
            cmd.Parameters.Add(objectIdsParam);
            cmd.Parameters.Add(permissionsParam);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);


            string spName = GetFullyQualifiedName("CheckAccess");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@UserID" + sep +
                (objectType == PrivacyObjectType.None ? "null" : "@ObjectType") + sep +
                "@ObjectIDs" + sep + "@Permissions" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return _parse_access_checked_items(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return new Dictionary<Guid, List<PermissionType>>();
            }
            finally { con.Close(); }
        }

        public static Dictionary<Guid, List<Audience>> GetAudience(Guid applicationId, List<Guid> objectIds)
        {
            string spName = GetFullyQualifiedName("GetAudience");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(objectIds), ',');
                return _parse_audience(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return new Dictionary<Guid, List<Modules.Privacy.Audience>>();
            }
        }

        public static Dictionary<Guid, List<DefaultPermission>> GetDefaultPermissions(Guid applicationId, List<Guid> objectIds)
        {
            string spName = GetFullyQualifiedName("GetDefaultPermissions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(objectIds), ',');
                return _parse_default_permissions(ref reader);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return new Dictionary<Guid, List<DefaultPermission>>();
            }
        }

        public static List<Privacy> GetSettings(Guid applicationId, List<Guid> objectIds)
        {
            string spName = GetFullyQualifiedName("GetSettings");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(objectIds), ',');
                return _parse_settings(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return new List<Privacy>();
            }
        }

        public static bool AddConfidentialityLevel(Guid applicationId, 
            Guid id, int levelId, string title, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("AddConfidentialityLevel");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, levelId, title, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }

        public static bool ModifyConfidentialityLevel(Guid applicationId, 
            Guid id, int newLevelId, string newTitle, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("ModifyConfidentialityLevel");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, newLevelId, newTitle, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }

        public static bool RemoveConfidentialityLevel(Guid applicationId, Guid id, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("RemoveConfidentialityLevel");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }

        public static void GetConfidentialityLevels(Guid applicationId, ref List<ConfidentialityLevel> retLevels)
        {
            string spName = GetFullyQualifiedName("GetConfidentialityLevels");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId);
                _parse_confidentiality_levels(ref reader, ref retLevels);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
            }
        }

        public static bool SetConfidentialityLevel(Guid applicationId, Guid itemId, Guid levelId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetConfidentialityLevel");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    itemId, levelId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }

        public static bool UnsetConfidentialityLevel(Guid applicationId, Guid itemId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("UnsetConfidentialityLevel");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    itemId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
                return false;
            }
        }

        public static void GetConfidentialityLevelUserIDs(Guid applicationId, ref List<Guid> retUserIds,
            Guid confidentialityId, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetConfidentialityLevelUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    confidentialityId, ProviderUtil.get_search_text(searchText), count, lowerBoundary);
                _parse_user_ids(ref reader, ref retUserIds, ref totalCount);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.PRVC);
            }
        }
    }
}
