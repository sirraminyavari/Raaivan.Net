﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Question.aspx.cs" Inherits="RaaiVan.Web.Page.View.Question"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div class="small-12 medium-12 large-12 row align-center" style="margin:0rem; padding:0vw 10vw;">
        <div id="qArea" class="small-12 medium-12 large-12 row" style="margin:0rem; margin-top:1rem;"></div>
    </div>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value || "{}");

            GlobalUtilities.loading("qArea");
            
            GlobalUtilities.load_files(["QA/QuestionView.js"], {
                OnLoad: function () {
                    new QuestionView("qArea", { QuestionID: initialJson.QuestionID });
                }
            });
        })();
    </script>
</asp:Content>
