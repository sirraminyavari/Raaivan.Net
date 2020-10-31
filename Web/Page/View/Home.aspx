<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RaaiVan.Web.Page.View.Home"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div class="small-12 medium-12 large-12 row align-center" style="margin:0rem;">
        <div class="small-12 medium-12 large-9" style="padding:0rem 1rem;">
            <div id="pollsArea" class="small-12 medium-12 large-12" style="margin-bottom:1rem; padding:0 4vw; display:none;"></div>
            <div id="centerArea" class="small-12 medium-12 large-12" style="padding:0 4vw;"></div>
        </div>
        <div id="sideInfo" class="small-12 medium-12 large-3 show-for-large"></div>
    </div>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value) || {};
            var currentUser = initialJson.User || {};

            var modules = (window.RVGlobal || {}).Modules || {};
            
            GlobalUtilities.set_cookie("ck_theme", (window.RVGlobal || {}).Theme, 1000);

            GlobalUtilities.load_files(["USR/HomePageSideInfo.js"], {
                OnLoad: function () { new HomePageSideInfo("sideInfo", { User: currentUser, Modules: modules }); }
            });

            if (modules.FG) {
                GlobalUtilities.load_files(["Polls/PollInitializer.js"], {
                    OnLoad: function () {
                        new PollInitializer("pollsArea", {
                            OnInit: function (data) { if ((data || {}).TotalCount) jQuery("#pollsArea").fadeIn(500); }
                        });
                    }
                });
            }
            
            GlobalUtilities.load_files(["USR/HomePageMainContent.js"], {
                OnLoad: function () {
                    new HomePageMainContent("centerArea", {
                        User: currentUser,
                        Priorities: initialJson.PersonalPagePriorities || {}
                    });
                }
            });
        })();
    </script>
</asp:Content>
