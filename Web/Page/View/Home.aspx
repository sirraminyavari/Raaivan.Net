﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="RaaiVan.Web.Page.View.Home"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div id="homeArea" class="small-12 medium-12 large-12 row align-center" style="margin:0;"></div>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value) || {};
            var container = document.getElementById("homeArea");

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-9", Style: "padding:0 1rem;",
                    Childs: [
                        { Type: "div", Style: "margin-bottom:1rem; padding:0 4vw; display:none;", Name: "pollsArea" },
                        { Type: "div", Style: "padding:0 4vw;", Name: "centerArea"}
                    ]
                },
                { Type: "div", Class: "small-12 medium-12 large-3 show-for-large", Name: "sideInfo"}
            ], container);

            var currentUser = initialJson.User || {};

            var modules = (window.RVGlobal || {}).Modules || {};
            
            GlobalUtilities.set_cookie("ck_theme", (window.RVGlobal || {}).Theme, 1000);

            GlobalUtilities.load_files(["USR/HomePageSideInfo.js"], {
                OnLoad: function () { new HomePageSideInfo(elems["sideInfo"], { User: currentUser, Modules: modules }); }
            });

            if (modules.FG) {
                GlobalUtilities.load_files(["Polls/PollInitializer.js"], {
                    OnLoad: function () {
                        new PollInitializer(elems["pollsArea"], {
                            OnInit: function (data) { if ((data || {}).TotalCount) jQuery(elems["pollsArea"]).fadeIn(500); }
                        });
                    }
                });
            }
            
            GlobalUtilities.load_files(["USR/HomePageMainContent.js"], {
                OnLoad: function () {
                    new HomePageMainContent(elems["centerArea"], {
                        User: currentUser,
                        Priorities: initialJson.PersonalPagePriorities || {}
                    });
                }
            });
        })();
    </script>
</asp:Content>
