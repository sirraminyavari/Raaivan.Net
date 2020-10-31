using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.DataExchange;
using RaaiVan.Modules.Log;
using System.ServiceModel;

namespace RaaiVan.Modules.Jobs
{
    public class CustomJobs
    {
        public static void do_job(Guid applicationId, string jobName)
        {
            jobName = jobName.ToLower();

            switch (jobName)
            {
                case "updateusers":
                    update_users(applicationId);
                    break;
                case "updatechart":
                    update_chart(applicationId);
                    break;
                case "updateprojects":
                    for (int i = 390; i < 410; i++)
                    {
                        for (int p = 0; p < 10; p++)
                        {
                            int maxCount = 300;

                            string from = i.ToString() + (1001 + (p * maxCount)).ToString();
                            string to = i.ToString() + (1000 + ((p + 1) * maxCount)).ToString();

                            update_projects(applicationId, from, to);
                        }
                    }
                    break;
            }
        }

        private static void update_users(Guid applicationId)
        {
            try
            {
                string res = CallWebService.CallWebMethod("http://wsrefahicard.isfahan.ir/RefahService.asmx",
                        "GetknowledgemanagementEmployeeWithArg", "knowledgemanagement", "EmpknowledgemanageWebService@128",
                        new Dictionary<string, string>());
                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(res);

                XmlNodeList nodes = doc.GetElementsByTagName("Personel");
                
                List<ExchangeUser> users = new List<ExchangeUser>();

                foreach (XmlNode nd in nodes)
                {
                    string un = nd.SelectSingleNode("MelliCode").InnerText.Trim();

                    if (!string.IsNullOrEmpty(un))
                    {
                        string pass = un;
                        while (pass.Length < 5) pass += un;

                        users.Add(new ExchangeUser()
                        {
                            //UserName = nd.SelectSingleNode("empno").InnerText.Trim(),
                            UserName = un,
                            FirstName = PublicMethods.verify_string(nd.SelectSingleNode("Name").InnerText.Trim()),
                            LastName = PublicMethods.verify_string(nd.SelectSingleNode("Family").InnerText.Trim()),
                            Password = new Password(pass)
                        });
                    }
                }
                
                for (int i = 0; i < users.Count;)
                {
                    List<ExchangeUser> exLst = new List<ExchangeUser>();

                    for (int j = 0; j < 200 && i < users.Count; ++j)
                        exLst.Add(users[i++]);
                    
                    DEController.update_users(applicationId, exLst);
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "job_updateusers_esf_webservice", ex, ModuleIdentifier.Jobs);
            }
        }

        private static void update_chart(Guid applicationId)
        {
            try
            {
                string depTypeAddId = "6";

                string res = CallWebService.CallWebMethod("http://wsrefahicard.isfahan.ir/RefahService.asmx",
                        "GetknowledgeManagementEmpChartWithArg", "knowledgemanagement", "EmpknowledgemanageWebService@128",
                        new Dictionary<string, string>());

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(res);

                XmlNodeList nodes = doc.GetElementsByTagName("Personel");

                List<ExchangeNode> exNodes = new List<ExchangeNode>();
                List<ExchangeMember> exMembers = new List<ExchangeMember>();
                
                Dictionary<string, bool> ids = new Dictionary<string, bool>();

                foreach (XmlNode nd in nodes)
                {
                    string id = nd.SelectSingleNode("ChartId").InnerText.Trim();
                    string parentId = nd.SelectSingleNode("parentid").InnerText.Trim();
                    string title = nd.SelectSingleNode("title").InnerText.Trim();
                    string username = nd.SelectSingleNode("melicode").InnerText.Trim();
                    string isAdmin = nd.SelectSingleNode("IsManagement").InnerText.Trim();

                    long val = 0;
                    if (!long.TryParse(id, out val) || val <= 0) id = null;
                    if (!long.TryParse(parentId, out val) || val <= 0) parentId = null;
                    if (!long.TryParse(username, out val) || val <= 0) username = string.Empty;
                    if (!long.TryParse(isAdmin, out val) || val <= 0) isAdmin = string.Empty;

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(title) &&
                        !exNodes.Any(u => u.AdditionalID == id))
                    {
                        exNodes.Add(new ExchangeNode()
                        {
                            AdditionalID = id,
                            ParentAdditionalID = parentId,
                            Name = title
                        });
                    }

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(username) &&
                        !exMembers.Any(u => u.NodeAdditionalID == id && u.UserName == username))
                    {
                        exMembers.Add(new ExchangeMember()
                        {
                            NodeTypeAdditionalID = depTypeAddId,
                            NodeAdditionalID = id,
                            UserName = username,
                            IsAdmin = !string.IsNullOrEmpty(isAdmin) && isAdmin != "0"
                        });
                    }
                }

                List<Guid> nIds = new List<Guid>();

                DEController.update_nodes(applicationId, exNodes, null, depTypeAddId, 
                    Guid.Parse("6B9E8414-C1EA-4E59-8AA8-34B4BCEB74E7"), ref nIds);

                for (int i = 0; i < exMembers.Count;)
                {
                    List<ExchangeMember> exLst = new List<ExchangeMember>();

                    for (int j = 0; j < 200 && i < exMembers.Count; ++j)
                        exLst.Add(exMembers[i++]);

                    DEController.update_members(applicationId, exLst);
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "job_update_dep_and_members_esf_webservice", ex, ModuleIdentifier.Jobs);
            }
        }

        private static void update_projects(Guid applicationId, string from, string to)
        {
            try
            {
                string projectTypeAddId = "2";

                Dictionary<string, string> parameters = new Dictionary<string, string>();

                string username = @"sigma\knowledgetosigma";
                string password = "knsigma1015";

                parameters["p1"] = "knowledgePrjList";
                parameters["From"] = from;
                parameters["To"] = to;

                BasicHttpBinding binding = new BasicHttpBinding();
                EndpointAddress address = new EndpointAddress("http://sigma.isfahan.ir/_layouts/15/ahdaUtility15/ahdaUtility15.asmx?op=QuerySQL2");
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

                binding.MessageEncoding = WSMessageEncoding.Text;
                binding.TextEncoding = Encoding.UTF8;
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                binding.MaxReceivedMessageSize = 10000000;

                EsfahanProjects.ahdaUtility15SoapClient client = new EsfahanProjects.ahdaUtility15SoapClient(binding, address);
                client.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(username, password);
                client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                XmlNode res = client.QuerySQL2(parameters["p1"], parameters["From"], parameters["To"]);

                XmlNodeList nodes = res.SelectNodes("item");

                List<ExchangeNode> exNodes = new List<ExchangeNode>();

                Dictionary<string, bool> ids = new Dictionary<string, bool>();

                foreach (XmlNode nd in nodes)
                {
                    string id = nd.SelectSingleNode("ProjectCode").InnerText.Trim();
                    string title = nd.SelectSingleNode("Title").InnerText.Trim();

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(title) &&
                        !exNodes.Any(u => u.AdditionalID == id))
                    {
                        exNodes.Add(new ExchangeNode()
                        {
                            AdditionalID = id,
                            Name = title
                        });
                    }
                }

                if (exNodes.Count == 0) return;

                PublicMethods.split_list<ExchangeNode>(exNodes, 200, lst =>
                {
                    List<Guid> nIds = new List<Guid>();

                    DEController.update_nodes(applicationId, lst, null, projectTypeAddId,
                        Guid.Parse("6B9E8414-C1EA-4E59-8AA8-34B4BCEB74E7"), ref nIds);
                });

                //Save Log
                LogController.save_error_log(applicationId, null, "job_update_projects_esf_webservice",
                    "From: " + from + ", To: " + to + ", Count: " + exNodes.Count.ToString(), ModuleIdentifier.Jobs, LogLevel.Trace);
                //end of Save Log
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "job_update_projects_esf_webservice", ex, ModuleIdentifier.Jobs);
            }
        }
    }
}
