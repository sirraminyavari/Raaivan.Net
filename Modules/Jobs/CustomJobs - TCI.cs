using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.DataExchange;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Jobs
{
    public class CustomJobs
    {
        public static void do_job(Guid applicationId, string jobName)
        {
            jobName = jobName.ToLower();

            switch (jobName)
            {
                case "update_users":
                    update_users(applicationId);
                    break;
            }
        }

        private static void update_users(Guid applicationId)
        {
            string nodeTypeAdditionalId = "2246"; //کد کلاس مناطق
            Guid? nodeTypeId = CNController.get_node_type_id(applicationId, nodeTypeAdditionalId);

            if (!nodeTypeId.HasValue) return;

            Dictionary<string, string> codes = new Dictionary<string, string>(); //Key: HRIS Code, Value: RaaiVan AdditionalID

            codes["83"] = "10028"; //منطقه کرمانشاه
            codes["76"] = "10035"; //منطقه هرمزگان
            codes["17"] = "10030"; //منطقه گلستان
            codes["44"] = "10012"; //منطقه آذربایجان غربی
            codes["63"] = "10019"; //منطقه خوزستان
            codes["35"] = "10037"; //منطقه یزد
            codes["86"] = "10034"; //منطقه مرکزی
            codes["34"] = "10027"; //منطقه کرمان
            codes["45"] = "10006"; //منطقه اردبیل
            codes["77"] = "10013"; //منطقه بوشهر
            codes["25"] = "10025"; //منطقه قم
            codes["24"] = "10020"; //منطقه زنجان
            codes["13"] = "10031"; //منطقه گیلان
            codes["38"] = "10015"; //منطقه چهار محال و بختیاری
            codes["15"] = "10033"; //منطقه مازندران
            codes["41"] = "10010"; //منطقه آذربایجان شرقی
            codes["56"] = "10016"; //منطقه خراسان جنوبی
            codes["23"] = "10021"; //منطقه سمنان
            codes["31"] = "10007"; //منطقه اصفهان
            codes["54"] = "10022"; //منطقه سیستان و بلوچستان
            codes["10"] = "10003"; //شرکت مخابرات ایران
            codes["58"] = "10018"; //منطقه خراسان شمالی
            codes["87"] = "10026"; //منطقه کردستان
            codes["28"] = "10024"; //منطقه قزوین
            codes["66"] = "10032"; //منطقه لرستان
            codes["81"] = "10036"; //منطقه همدان
            codes["84"] = "10009"; //منطقه ایلام
            codes["74"] = "10029"; //منطقه کهگیلویه و بویراحمد
            codes["51"] = "10017"; //منطقه خراسان رضوی
            codes["71"] = "10023"; //منطقه فارس
            codes["92"] = "10005"; //شرکت مدیریت املاک( سامان سازه غدیر)
            codes["21"] = "10014"; //منطقه تهران

            /*
            codes[""] = "10008"; //منطقه البرز
            codes[""] = "10001"; //شرکت گسترش خدمات ارتباطات کاراشاب
            */

            codes.Keys.ToList().ForEach(key =>
            {
                List<ExchangeUser> users = get_useres(applicationId, key);

                if (users.Count == 0) return;
                
                Guid nodeId = CNController.get_node_id(applicationId, codes[key], nodeTypeId.Value);

                if (nodeId == Guid.Empty) return;

                List<NodeMember> admins = CNController.get_members(applicationId, nodeId, null, true);

                List<ExchangeMember> members = new List<ExchangeMember>();

                users.ForEach(u => members.Add(new ExchangeMember()
                {
                    NodeTypeAdditionalID = nodeTypeAdditionalId,
                    NodeAdditionalID = codes[key],
                    UserName = u.UserName,
                    IsAdmin = admins.Any(m => m.Member.UserName.ToLower() == u.UserName.ToLower())
                }));

                PublicMethods.split_list<ExchangeUser>(users, 200, items => {
                    DEController.update_users(applicationId, items);
                }, 2000);

                PublicMethods.split_list<ExchangeMember>(members, 200, items => {
                    DEController.update_members(applicationId, items);
                }, 2000);

                //Save Log
                LogController.save_error_log(applicationId, null, "update_users_with_hris", 
                    "Code: " + key + ", Count: " + members.Count.ToString(), ModuleIdentifier.Jobs, LogLevel.Trace);
                //end of Save Log
            });
        }

        private static List<ExchangeUser> get_useres(Guid applicationId, string provinceCode, int itemFrom = 1)
        {
            string baseUrl = "https://service.tci.ir";
            string username = "TCI_KM", password = "GcEft1d4w02LHPi13GKj";
            int itemTo = itemFrom + 500 - 1;
            
            string personnelCode = string.Empty, nationalCode = string.Empty, personnelType = "0";

            string apiUrl = baseUrl + "/api/hr/v1.1/personnel/profile/" + personnelCode + "/" + nationalCode +
                "/" + personnelType + "/" + provinceCode + "/" + itemFrom.ToString() + "/" + itemTo.ToString();

            NameValueCollection headers = new NameValueCollection();
            headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password)));
            
            string responseText = PublicMethods.web_request(apiUrl, values: null, method: HTTPMethod.GET, headers: headers);
            
            Dictionary<string, object> response = PublicMethods.fromJSON(responseText);
            
            Dictionary<string, object> result = response != null && response.ContainsKey("result") &&
                response["result"].GetType() == typeof(Dictionary<string, object>) ? (Dictionary<string, object>)response["result"] : null;
            
            Dictionary<string, object> data = result != null && result.ContainsKey("data") &&
                result["data"].GetType() == typeof(Dictionary<string, object>) ? (Dictionary<string, object>)result["data"] : null;
            
            int? totalCount = data == null || !data.ContainsKey("totalItem") ? null :
                PublicMethods.parse_int(data["totalItem"].ToString());
            
            ArrayList users = data != null && data.ContainsKey("items") && data["items"].GetType() == typeof(ArrayList) ?
                (ArrayList)data["items"] : null;
            
            List<ExchangeUser> exUsers = new List<ExchangeUser>();
            
            if (users != null)
            {
                foreach (object obj in users)
                {
                    if (obj == null) continue;

                    Dictionary<string, object> u = null;

                    try
                    {
                        u = obj.GetType() == typeof(Dictionary<string, object>) ?
                            (Dictionary<string, object>)obj : new Dictionary<string, object>();

                        string nationalNumber = u.ContainsKey("nationalNumber") && u["nationalNumber"] != null ?
                                u["nationalNumber"].ToString().Trim() : null;
                        string jobTitle = u.ContainsKey("positionTitleName") && u["positionTitleName"] != null ? 
                            u["positionTitleName"].ToString().Trim() : null;
                        string mobileNumber = u.ContainsKey("mobileNumber") && u["mobileNumber"] != null ? 
                            u["mobileNumber"].ToString().Trim() : null;
                        string email = u.ContainsKey("email") && u["email"] != null ? u["email"].ToString().Trim() : null;

                        if (!string.IsNullOrEmpty(nationalNumber))
                        {
                            if (nationalNumber.Length < 10)
                                for (int i = 0; i < 10 - nationalNumber.Length; i++) nationalNumber = "0" + nationalNumber;

                            add_item(exUsers, new ExchangeUser()
                            {
                                UserName = nationalNumber,
                                FirstName = u.ContainsKey("firstName") && u["firstName"] != null ? u["firstName"].ToString().Trim() : null,
                                LastName = u.ContainsKey("lastName") && u["lastName"] != null ? u["lastName"].ToString().Trim() : null
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        if (u != null)
                            LogController.save_error_log(applicationId, null, "hris_user_parse_error",
                                PublicMethods.toJSON(u), ModuleIdentifier.Jobs);
                    }
                }
            }

            if (totalCount.HasValue && totalCount.Value >= (itemFrom + users.Count))
                get_useres(applicationId, provinceCode, itemFrom + users.Count).ForEach(x => add_item(exUsers, x));
            
            return exUsers;
        }

        private static void add_item(List<ExchangeUser> allUsers, ExchangeUser user)
        {
            if (!string.IsNullOrEmpty(user.UserName)) {
                while (user.UserName.Length < 10)
                    user.UserName = "0" + user.UserName;
            }

            if (string.IsNullOrEmpty(user.UserName) || allUsers.Any(x => x.UserName.ToLower() == user.UserName.ToLower())) return;

            string password = user.UserName;
            while (password.Length < 5) password += user.UserName;

            user.Password = new Password(password);

            allUsers.Add(user);
        }
    }
}
