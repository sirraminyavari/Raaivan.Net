using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Jobs
{
    public class CustomNotifications
    {
        public static List<Dashboard> send_dashboards(Guid applicationId, Guid? currentUserId, List<Dashboard> dashboards)
        {
            string username = "baridwebservice";
            string password = "P@ssw0rd";

            List<User> users = UsersController.get_users(applicationId,
                dashboards.Where(u => u.UserID.HasValue).Select(x => x.UserID.Value).ToList());
            List<Node> nodes = CNController.get_nodes(applicationId,
                dashboards.Where(u => u.NodeID.HasValue).Select(x => x.NodeID.Value).ToList(), full: null);

            List<Dashboard> failed = new List<Dashboard>();

            foreach (Dashboard d in dashboards)
            {
                if (!d.UserID.HasValue || d.Type != DashboardType.Knowledge ||
                    !users.Any(u => u.UserID == d.UserID) || !nodes.Any(u => u.NodeID == d.NodeID)) continue;

                User theUser = users.Where(u => u.UserID == d.UserID).FirstOrDefault();
                Node theNode = nodes.Where(u => u.NodeID == d.NodeID).FirstOrDefault();

                decimal nationalCode = 0;
                if (theUser == null || theNode == null || !decimal.TryParse(theUser.UserName, out nationalCode)) continue;

                string subject = string.Empty, message = string.Empty;

                switch (d.SubType)
                {
                    case DashboardSubType.Admin:
                    case DashboardSubType.ExpirationDate:
                    case DashboardSubType.EvaluationRefused:
                    case DashboardSubType.EvaluationDone:
                    case DashboardSubType.Knowledgable:
                        break;
                    case DashboardSubType.Evaluator:
                        subject = "ارجاع دانش جهت داوری";
                        message = "با سلام و احترام، " +
                            "خبره گرامی، '" + theUser.FullName + "'، '" + theNode.NodeType +
                            "' با عنوان '" + theNode.Name + "' و کد رهگیری '" + theNode.AdditionalID +
                            "' جهت ارزیابی برای شما ارسال شده است. لطفا به کارتابل رای ون مراجعه فرمایید. " + 
                            "با تشکر، دبیرخانه مدیریت دانش، تلفن تماس: 83735503";
                        break;
                    case DashboardSubType.Revision:
                        subject = "ارجاع دانش جهت اصلاح";
                        message = "با سلام و احترام، " +
                            "دانشکار گرامی، '" + theUser.FullName + "'، '" + theNode.NodeType +
                            "' شما با عنوان '" + theNode.Name + "' و کد رهگیری '" + theNode.AdditionalID +
                            "' جهت اصلاح به شما ارجاع داده شده است. لطفا به کارتابل رای ون مراجعه فرمایید. " +
                            "با تشکر، دبیرخانه مدیریت دانش، تلفن تماس: 83735503";
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message)) continue;

                try
                {
                    Barid.WebService.CreatePostItemService service =
                        new Barid.WebService.CreatePostItemService();

                    service.Url = "http://10.110.11.12/SendPostItem/";
                    
                    Barid.WebService.WebSendPostItemType result = service.SendPostItem(username, password, subject, message, 
                        new string[] { theUser.UserName }, new Barid.WebService.WebAttachment[] { }, 0, 0);
                    
                    if (string.IsNullOrEmpty(result.ToString()) || result.ToString().ToLower() != "sendsuccessful")
                    {
                        LogController.save_error_log(applicationId, currentUserId, "Gas_SendNotificationToBarid",
                            "Username: " + theUser.UserName + ", " + result.ToString(), ModuleIdentifier.Jobs);
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(d);

                    LogController.save_error_log(applicationId, currentUserId, "Gas_SendNotificationToBarid",
                        PublicMethods.get_exception(ex), ModuleIdentifier.Jobs);
                }
            }

            return failed;
        }

        /*
        public static List<Dashboard> send_dashboards(Guid applicationId, Guid? currentUserId, List<Dashboard> dashboards)
        {
            string baseUrl = "http://10.110.11.12/SendPostItem/CreatePostItemService.asmx";
            string username = "baridwebservice";
            string password = "P@ssw0rd";

            List<User> users = UsersController.get_users(applicationId,
                dashboards.Where(u => u.UserID.HasValue).Select(x => x.UserID.Value).ToList());
            List<Node> nodes = CNController.get_nodes(applicationId,
                dashboards.Where(u => u.NodeID.HasValue).Select(x => x.NodeID.Value).ToList(), full: null);

            List<Dashboard> failed = new List<Dashboard>();

            foreach (Dashboard d in dashboards)
            {
                if (!d.UserID.HasValue || d.Type != DashboardType.Knowledge ||
                    !users.Any(u => u.UserID == d.UserID) || !nodes.Any(u => u.NodeID == d.NodeID)) continue;

                User theUser = users.Where(u => u.UserID == d.UserID).FirstOrDefault();
                Node theNode = nodes.Where(u => u.NodeID == d.NodeID).FirstOrDefault();

                theUser.UserName = "3258281534";

                long nationalCode = 0;
                if (theUser == null || theNode == null || !long.TryParse(theUser.UserName, out nationalCode)) continue;

                string subject = string.Empty, message = string.Empty;

                switch (d.SubType)
                {
                    case DashboardSubType.Admin:
                    case DashboardSubType.ExpirationDate:
                    case DashboardSubType.EvaluationRefused:
                    case DashboardSubType.EvaluationDone:
                    case DashboardSubType.Knowledgable:
                        break;
                    case DashboardSubType.Evaluator:
                        subject = "ارجاع دانش جهت اصلاح";
                        message = "خبره گرامی، '" + theUser.FullName + "'، '" + theNode.NodeType +
                            "' با عنوان '" + theNode.Name + "' و کد رهگیری '" + theNode.AdditionalID +
                            "' جهت ارزیابی برای شما ارسال شده است. لطفا به کارتابل رای ون مراجعه فرمایید.";
                        break;
                    case DashboardSubType.Revision:
                        subject = "ارجاع دانش جهت اصلاح";
                        message = "دانشکار گرامی، '" + theUser.FullName + "'، '" + theNode.NodeType + 
                            "' شما با عنوان '" + theNode.Name + "' و کد رهگیری '" + theNode.AdditionalID + 
                            "' جهت اصلاح به شما ارجاع داده شده است. لطفا به کارتابل رای ون مراجعه فرمایید.";
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message)) continue;

                Dictionary<string, string> parameters = new Dictionary<string, string>();

                parameters["userName"] = username;
                parameters["passWord"] = HttpUtility.UrlEncode(password);
                parameters["postItem"] = "<AttachmentList></AttachmentList>" +
                    "<InvolvedPersonList><WebInvolvedPerson>" +
                        "<OrganizationId></OrganizationId>" +
                        "<DisplayString></DisplayString>" +
                        "<PersonName></PersonName>" +
                        "<PersonId></PersonId>" +
                        "<OrganizationUnitName></OrganizationUnitName>" +
                        "<PositionName></PositionName>" +
                        "<Id></Id>" +
                        "<Description></Description>" +
                        "<InvolvedParticipantId>" + nationalCode.ToString() + "</InvolvedParticipantId>" +
                        "<InvolvedType>" + "MainReceiver" + "</InvolvedType>" +
                    "</WebInvolvedPerson></InvolvedPersonList>" +
                    "<ImportanceType>" + "Immediate" + "</ImportanceType>" +
                    "<SensitivityType>" + "Normal" + "</SensitivityType>" +
                    "<Description>" + HttpUtility.UrlEncode(message) + "</Description>" +
                    "<Subject>" + HttpUtility.UrlEncode(subject) + "</Subject>" +
                    "<HasAttachment>false</HasAttachment>";

                string res = CallWebService.CallWebMethod(baseUrl, "SendPostItem", parameters, encode: false, actionUrl: "http://Baridsoft.ir/");
                
                bool result = false;
                string exceptionMsg = string.Empty;

                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(res);

                    result = doc.SelectSingleNode("SendPostItemResult").InnerText.ToLower() == "SendSuccessful";
                }
                catch (Exception ex) { exceptionMsg = res + "   -   " + PublicMethods.get_exception(ex); }

                if (!string.IsNullOrEmpty(exceptionMsg))
                {
                    LogController.save_error_log(applicationId, 
                        currentUserId, "Gas_SendNotificationToBarid", exceptionMsg, ModuleIdentifier.Jobs);
                }

                if (!result) failed.Add(d);
            }

            return failed;
        }
        */
    }
}
