﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewNode.aspx.cs" Inherits="RaaiVan.Web.Page.View.NewNode"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="../../Script/CN/RegisterNewNode.js"></script>
    <script type="text/javascript" src="../../Script/CN/RegisterNewNoContentNode.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div id="nodeArea" class="small-12 medium-12 large-12" style="padding:0vw 6vw; margin-bottom:8rem;"></div>

    <script type="text/javascript">
        (function () {
            var options = JSON.parse(document.getElementById("initialJson").value || "{}") || {};
            
            setTimeout(function () {
                document.title = Base64.decode((options.Service || {}).Title) + (!document.title ? "" : " - " + document.title);
            }, 1000);

            if (options.DocumentTreeNode) {
                var nd = options.DocumentTreeNode;

                var dic = { ID: null, Tree: null, Path: [] };

                if ((nd.Path || []).length) {
                    nd.Path = nd.Path.reverse();

                    dic.ID = nd.Path[nd.Path.length - 1].NodeID;

                    jQuery.each(nd.Path, function (ind, val) {
                        dic.Path.push({ ID: val.NodeID, Name: Base64.decode(val.Name) });
                    });
                }

                if (nd.Tree) dic.Tree = { ID: nd.Tree.ID, Name: Base64.decode(nd.Tree.Title) };

                options.DocumentTreeNode = dic;
            }

            if ((options.PreviousVersion || {}).NodeID) {
                options.PreviousVersion = {
                    ID: options.PreviousVersion.NodeID,
                    Name: Base64.decode(options.PreviousVersion.Name)
                };
            }
            
            new RegisterNewNode("nodeArea", GlobalUtilities.extend(options || {}, {
                Options: {
                    IsServiceAdmin: options.IsServiceAdmin,
                    NodeSelectType: (options.KnowledgeType || {}).NodeSelectType,
                    PreviousVersion: options.PreviousVersion,
                    ParentNode: options.ParentNode,
                    DocumentTreeNode: options.DocumentTreeNode
                }
            }));

            GlobalUtilities.append_goto_top_button();
        })();
    </script>
</asp:Content>