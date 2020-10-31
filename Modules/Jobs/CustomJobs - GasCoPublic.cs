using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Documents;

namespace RaaiVan.Modules.Jobs
{
    public class CustomJobs
    {
        public static void do_job(Guid applicationId, string jobName)
        {
            jobName = jobName.ToLower();

            switch (jobName)
            {
                case "syncnodes":
                    sync_nodes(applicationId);
                    break;
            }
        }

        private static void sync_nodes(Guid applicationId)
        {
            string baseUrl = "http://10.110.11.53";

            //login
            NameValueCollection vals = new NameValueCollection();
            vals.Add("username", "developer");
            vals.Add("password", "9B1aLrYSC_E^");

            string res = PublicMethods.web_request(baseUrl + "/Ajax/API.ashx?command=authenticate", vals);

            Dictionary<string, object> jsonRes = PublicMethods.fromJSON(res);

            string ticket = jsonRes.ContainsKey("Ticket") ? (string)jsonRes["Ticket"] : null;
            string accessToken = jsonRes.ContainsKey("AccessToken") ? (string)jsonRes["AccessToken"] : null;

            if (string.IsNullOrEmpty(ticket) || string.IsNullOrEmpty(accessToken)) return;
            //end of login

            List<Node> nodes = CNController.get_nodes(applicationId, null, null, null, null, null, 10, null, null, null);

            List<NodeInfo> nodeInfo = CNController.get_node_info(applicationId, 
                nodes.Select(u => u.NodeID.Value).ToList(), null, true, true, 
                null, null, null, null, null, null, null, null, null);

            List<DocFileInfo> attachments = 
                DocumentsController.get_owner_files(applicationId, nodes.Select(u => u.NodeID.Value).ToList());

            foreach (Node nd in nodes)
            {
                NodeInfo inf = nodeInfo.Where(u => u.NodeID == nd.NodeID).FirstOrDefault();

                string newNodeUrl = baseUrl + "/Ajax/ManageNodes.ashx" + 
                    "?timeStamp=" + DateTime.Now.Ticks.ToString() + "&Command=RegisterNewNode";

                vals.Clear();

                vals.Add("Ticket", ticket);
                vals.Add("acstkn", accessToken);

                vals.Add("CurrentUserID", nd.Creator.UserID.ToString());
                vals.Add("CreationDate", nd.CreationDate.Value.Year.ToString() + "-" +
                    nd.CreationDate.Value.Month.ToString() + "-" + nd.CreationDate.Value.Day.ToString());
                vals.Add("NodeID", nd.NodeID.ToString());
                vals.Add("NodeTypeID", nd.NodeTypeID.ToString());
                vals.Add("Name", Base64.encode(nd.Name));
                if (inf != null && !string.IsNullOrEmpty(inf.Description))
                    vals.Add("Description", Base64.encode(inf.Description));
                if (inf != null && inf.Tags.Count > 0)
                    vals.Add("Tags", string.Join("|", inf.Tags.Select(u => Base64.encode(u))));

                res = PublicMethods.web_request(newNodeUrl, vals);
                jsonRes = PublicMethods.fromJSON(res);

                if (jsonRes.ContainsKey("AccessToken")) accessToken = (string)jsonRes["AccessToken"];

                foreach (DocFileInfo f in attachments.Where(u => u.OwnerID == nd.NodeID))
                {
                    string fileUploadUrl = baseUrl + "/Ajax/FileUpload.ashx" +
                        "?timeStamp=" + DateTime.Now.Ticks.ToString() +
                        "&command=uploadfile" +
                        "&Ticket=" + ticket + 
                        "&acstkn=" + accessToken +
                        "&FileID=" + f.FileID.ToString() +
                        "&OwnerID=" + nd.NodeID.ToString() +
                        "&OwnerType=" + f.OwnerType.ToString() +
                        "&aa=2";

                    FileOwnerTypes attachmentType = f.OwnerType == FileOwnerTypes.WikiContent.ToString() ? 
                        FileOwnerTypes.WikiContent : FileOwnerTypes.Node;

                    FolderNames folderName = DocumentUtilities.get_folder_name(attachmentType);
                    string subFolder = !DocumentUtilities.has_sub_folder(folderName) ? string.Empty :
                        "\\" + DocumentUtilities.get_sub_folder(f.FileID.ToString());

                    string fileAddress = DocumentUtilities.map_path(applicationId, folderName) + subFolder +
                        "\\" + f.FileName + (string.IsNullOrEmpty(f.Extension) ? string.Empty : "." + f.Extension);
                    
                    res = PublicMethods.upload_file(applicationId, fileUploadUrl, fileAddress);
                    jsonRes = PublicMethods.fromJSON(res);

                    if (jsonRes.ContainsKey("AccessToken")) accessToken = (string)jsonRes["AccessToken"];
                }

                CNController.remove_node(applicationId, nd.NodeID.Value, false, nd.Creator.UserID.Value);
            }
        }
    }
}
