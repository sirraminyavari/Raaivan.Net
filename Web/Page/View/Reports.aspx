<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="RaaiVan.Web.Page.View.Reports"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../../Script/Kendo/kendo.common.css" type="text/css" />
    <link rel="stylesheet" href="../../Script/Kendo/kendo.blueopal.css" type="text/css" />
    <script type="text/javascript" src="../../Script/Kendo/kendo.core.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.data.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.reorderable.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.resizable.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.selectable.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.sortable.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.pager.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.userevents.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.draganddrop.js"></script>
    <script type="text/javascript" src="../../Script/Kendo/kendo.grid.js"></script>

    <script type="text/javascript" src="../../Script/GridManager/GridManager.js"></script>
    <script type="text/javascript" src="../../Script/API/ReportsAPI.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div class="small-12 medium-12 large-12" style="margin:0rem; padding:0vw 6vw; margin-bottom:5rem;">
        <div id="serverSelector" class="small-10 medium-8 large-6" style="margin:0 auto 1rem auto; display:none;"></div>
        <div id="reportsContainer" class="small-12 medium-12 large-12 row align-center" style="margin:0rem;"></div>
    </div>

    <script type="text/javascript">
        (function () {
            var reports = JSON.parse(document.getElementById("initialJson").value).Reports || {};
            var modules = (window.RVGlobal || {}).Modules || {};
            
            modules.KW = modules.KW === true || modules.DCT === true;
            
            GlobalUtilities.load_files(["Reports/ReportsManager.js", "RemoteServers/RemoteServerSelector.js"], {
                OnLoad: function () {
                    new RemoteServerSelector("serverSelector", { AutoHide: true });
                    new ReportsManager("reportsContainer", { Reports: reports, Modules: modules });
                }
            });
        })();
    </script>
</asp:Content>
