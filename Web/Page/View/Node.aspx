<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Node.aspx.cs" Inherits="RaaiVan.Web.Page.View.Node" 
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="fragment" content="!" >
    <link type="text/css" rel="stylesheet" href="../../Script/jQuery/highlight/styles/vs.css" />
    <script type="text/javascript" src="../../Script/CN/NodeAccessDeniedResponse.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div id="nodeView" class="small-12 medium-12 large-12" style="padding:0vw 10vw; margin-bottom:5rem;"></div>

    <script type="text/javascript">
        (function () {
            var modules = (window.RVGlobal || {}).Modules || {};
            var initialJson = JSON.parse(document.getElementById("initialJson").value);
            var nodeId = initialJson.NodeID || "";
            var showWorkFlow = initialJson.ShowWorkFlow === true;
            var showKnowledgeOptions = initialJson.ShowKnowledgeOptions === true;
            var hideContributors = initialJson.HideContributors === true;

            if (initialJson.AccessDenied) return new NodeAccessDeniedResponse("nodeView", initialJson);
            
            GlobalUtilities.loading("nodeView");
            
            GlobalUtilities.load_files(["CN/NodeViewer.js"], {
                OnLoad: function () {
                    new NodeViewer("nodeView", {
                        NodeID: nodeId,
                        Modules: modules,
                        ShowWorkFlow: showWorkFlow,
                        ShowKnowledgeOptions: showKnowledgeOptions,
                        HideContributors: hideContributors
                    });
                }
            });

            GlobalUtilities.append_goto_top_button();
        })();
    </script>
</asp:Content>
