<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchUsers.aspx.cs" Inherits="RaaiVan.Web.Page.View.SearchUsers"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="../../Script/API/CNAPI.js"></script>
    <script type="text/javascript" src="../../Script/API/UsersAPI.js"></script>
    <script type="text/javascript" src="../../Script/USR/AdvancedUserSearch.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div id="searchArea" class="small-12 medium-12 large-12 row align-center" style="margin:0rem; padding:0vw 10vw;"></div>

    <script type="text/javascript">
        (function () {
            GlobalUtilities.load_files(["USR/AdvancedUserSearch.js"], {
                OnLoad: function () { new AdvancedUserSearch("searchArea"); }
            });
        })();
    </script>
</asp:Content>
