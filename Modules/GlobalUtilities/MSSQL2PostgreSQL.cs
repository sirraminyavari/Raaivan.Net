using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public enum MSSQLDataType
    {
        None,

        BIGINT,
        BIT,
        CHAR,
        DATETIME,
        DECIMAL,
        FLOAT,
        IMAGE,
        INT,
        NTEXT,
        NVARCHAR,
        UNIQUEIDENTIFIER,
        VARBINARY,
        VARCHAR
    }

    public class SchemaInfo
    {
        public string Table;
        public string Column;
        public bool? IsPrimaryKey;
        public bool? IsIdentity;
        public bool? IsNullable;
        public MSSQLDataType DataType;
        public int? MaxLength;
        public int? Order;
        public string DefaultValue;

        public SchemaInfo()
        {
            DataType = MSSQLDataType.None;
        }
    }

    public static class MSSQL2PostgreSQL
    {
        private static string resolve_name(string name)
        {
            string postgreName = string.Empty;

            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            name.ToList().ForEach(chr => postgreName += (upper.IndexOf(chr) >= 0 ? "_" : "") + chr);

            string retStr = postgreName.ToLower();

            if (retStr.StartsWith("_") && retStr.Length > 1) retStr = retStr.Substring(1);

            if (retStr == "i_d") retStr = "id";
            else if (retStr == "e_mail") retStr = "email";
            else if (retStr == "user_name") retStr = "username";
            else if (retStr == "u_r_l") retStr = "url";
            else if (retStr == "birth_day") retStr = "birthdate";
            else if (retStr == "like") retStr = "like_status";

            return retStr
                .Replace("__", "_")
                .Replace("___", "_")
                .Replace("_i_d", "_id")
                .Replace("w_f_", "wf_")
                .Replace("_p_i_n", "_pin")
                .Replace("work_space", "workspace")
                .Replace("sub_type", "subtype")
                .Replace("m_i_m_e", "mime")
                .Replace("work_flow", "workflow")
                .Replace("feed_back", "feedback")
                .Replace("_f_a_q", "_faq")
                .Replace("f_a_q_", "faq_")
                .Replace("_e_mail", "_email")
                .Replace("_user_name", "_username")
                .Replace("_u_r_l", "_url")
                .Replace("aspnet_", "rv_");
        }

        private static string resolve_table_name(string mssqlName)
        {
            int ind = mssqlName.IndexOf("_");

            string moduleIdentifier = ind > 0 ? mssqlName.Substring(0, ind + 1).ToLower() : string.Empty;
            string tableName = string.IsNullOrEmpty(moduleIdentifier) ? mssqlName : mssqlName.Substring(ind + 1);

            return resolve_name(moduleIdentifier + tableName);
        }

        private static string resolve_data_type(MSSQLDataType type, int? maxLength, bool? identity)
        {
            string strMaxLength = !maxLength.HasValue || maxLength.Value <= 0 ? 
                string.Empty : "(" + maxLength.Value.ToString() + ")";

            string retStr = string.Empty;

            switch (type)
            {
                case MSSQLDataType.INT:
                    retStr = identity.HasValue && identity.Value ? "SERIAL" : "INTEGER";
                    break;
                case MSSQLDataType.BIGINT:
                    retStr = identity.HasValue && identity.Value ? "BIGSERIAL" : "BIGINT";
                    break;
                case MSSQLDataType.FLOAT:
                    retStr = "FLOAT";
                    break;
                case MSSQLDataType.BIT:
                    retStr = "BOOLEAN";
                    break;
                case MSSQLDataType.CHAR:
                    retStr = "CHAR";
                    break;
                case MSSQLDataType.DATETIME:
                    retStr = "TIMESTAMPTZ";
                    break;
                case MSSQLDataType.VARCHAR:
                    retStr = "VARCHAR" + strMaxLength;
                    break;
                case MSSQLDataType.NVARCHAR:
                    retStr = "VARCHAR" + strMaxLength;
                    break;
                case MSSQLDataType.VARBINARY:
                    retStr = "BYTEA";
                    break;
                case MSSQLDataType.UNIQUEIDENTIFIER:
                    retStr = "UUID";
                    break;
                default:
                    retStr = string.Empty;
                    break;
            }

            return retStr;
        }

        private static string toScript(string tableName, List<SchemaInfo> columns)
        {
            List<MSSQLDataType> invalidDataTypes = new List<MSSQLDataType>() {
                MSSQLDataType.None,
                MSSQLDataType.DECIMAL,
                MSSQLDataType.IMAGE,
                MSSQLDataType.NTEXT
            };

            columns = columns
                .Where(c => c.Table == tableName && !invalidDataTypes.Any(x => x == c.DataType) &&
                    !string.IsNullOrEmpty(resolve_data_type(c.DataType, c.MaxLength, c.IsIdentity)))
                .OrderBy(c => !c.Order.HasValue ? 0 : c.Order.Value).ToList();

            List<string> strColumns = columns.Select(c =>
            {
                string dt = resolve_data_type(c.DataType, c.MaxLength, c.IsIdentity);
                string nullable = c.IsNullable.HasValue && c.IsNullable.Value ? string.Empty : " NOT NULL";
                string defaultValue = c.DataType == MSSQLDataType.UNIQUEIDENTIFIER &&
                    !string.IsNullOrEmpty(c.DefaultValue) && c.DefaultValue.ToLower().Contains("newid()") ?
                    " DEFAULT gen_random_uuid()" : string.Empty;

                return resolve_name(c.Column) + " " + dt + nullable + defaultValue;
            }).Where(x => !string.IsNullOrEmpty(x)).ToList();

            List<string> primary = columns
                .Where(c => c.IsPrimaryKey.HasValue && c.IsPrimaryKey.Value)
                .Select(c => resolve_name(c.Column)).ToList();

            if (primary.Count > 0) strColumns.Add("PRIMARY KEY (" + string.Join(", ", primary) + ")");

            return "CREATE TABLE IF NOT EXISTS " + resolve_table_name(tableName) + " (" +
                "\r\n\t" +
                string.Join(",\r\n\t", strColumns) +
                "\r\n" +
                ");";
        }

        public static string toScript(List<SchemaInfo> info)
        {
            List<string> tables = info.Select(i => i.Table).Distinct().ToList();

            return string.Join("\r\n\r\n", tables.Select(t => toScript(t, info)));
        }
    }
}
