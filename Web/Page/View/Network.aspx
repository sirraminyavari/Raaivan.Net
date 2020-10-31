<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Network.aspx.cs" Inherits="RaaiVan.Web.Page.View.Network"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link type="text/css" rel="stylesheet" href="../../Script/CN/CNExplorer/CNExplorer.css" />
    <script type="text/javascript" src="../../Script/CN/CNExplorer/CNExplorer.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="pageContainer" class="small-12 medium-12 large-12" style="padding:0vw 6vw; margin-bottom:5rem;"></div>

    <script type="text/javascript">
        (function () {
            var rvGlobal = window.RVGlobal || {};

            var loc = window.location.toString();
            var parameters = loc.substr((loc.indexOf("?") + 1)).split("&");
            var openTab = null;

            jQuery.each(parameters, function (ind, val) {
                var index = val.indexOf("=");
                var parameter = val.substr(0, index);
                var pValue = val.substr(index + 1);
                if (parameter == "OpenTab") openTab = pValue;
            });

            GlobalUtilities.load_files(["USR/UserConnections.js"], {
                OnLoad: function () {
                    new UserConnections(document.getElementById("pageContainer"), {
                        UserID: rvGlobal.CurrentUserID,
                        ProfileImageURL: (rvGlobal.CurrentUser || {}).ImageURL,
                        OpenTab: openTab,
                        Modules: rvGlobal.Modules || {}
                    });
                }
            });
        })();
    </script>
</asp:Content>