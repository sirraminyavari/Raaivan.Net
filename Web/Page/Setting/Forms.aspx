﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Forms.aspx.cs" Inherits="RaaiVan.Web.Page.Setting.Forms"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="formsArea" class="small-12 medium-12 large-12 row align-center" 
        style="margin:0rem; padding:0vw 6vw;">
    </div>

    <script type="text/javascript">
        (function () {
            GlobalUtilities.load_files(["FormsManager/FormsManager.js"], {
                OnLoad: function () { new FormsManager("formsArea"); }
            });
        })();
    </script>
</asp:Content>
