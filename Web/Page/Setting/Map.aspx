<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Map.aspx.cs" Inherits="RaaiVan.Web.Page.Setting.Map"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="mapArea" class="small-12 medium-12 large-12 row" 
        style="margin:0rem; margin-bottom:5rem; padding:0vw 6vw;">
    </div>

    <script type="text/javascript">
        (function () {
            GlobalUtilities.load_files(["CN/MapManager.js"], {
                OnLoad: function () { new MapManager("mapArea"); }
            });
        })();
    </script>
</asp:Content>