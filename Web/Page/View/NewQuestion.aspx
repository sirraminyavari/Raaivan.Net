<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewQuestion.aspx.cs" Inherits="RaaiVan.Web.Page.View.NewQuestion"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="../../Script/QA/NewQuestion.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div class="small-12 medium-12 large-12 row align-center" style="margin:0rem; margin-bottom:5rem; padding:0vw 10vw;">
        <div class="small-12 medium-12 large-12 row align-center" style="margin:0rem;">
            <div class="small-12 medium-8 large-9" style="padding:1rem 0rem;">
                <div style="display:table; width:100%; height:100%;">
                    <div id="pageTitle" style="display:table-cell; vertical-align:middle; font-size:1.5rem; font-weight:bold;"></div>
                </div>
            </div>
            <div class="small-6 medium-4 large-3" style="padding:1rem 0rem;">
                <div id="confirmButton" class="small-12 medium-12 large-12 rv-air-button rv-circle" 
                    style="padding:0.5rem 0rem; display:none;">
                </div>
            </div>
        </div>

        <div id="tabs" class="small-12 medium-12 large-12 row" 
            style="margin:0rem; margin-bottom:1rem;">
        </div>

        <div id="titlePage" class="small-12 medium-12 large-12 row align-center" style="margin:0rem;">
            <div id="titleIntro" class="small-12 medium-10 large-8 rv-dark-gray" 
                style="text-align:center; font-size:1.2rem;">
            </div>
            <div class="small-12 medium-10 large-8" style="margin-top:1.5rem;">
                <input id="titleInput" class="rv-input" style="width:100%;" />
            </div>
            <div class="small-12 medium-12 large-12"></div>
            <div id="titleNextButton" class="small-6 medium-4 large-3 rv-air-button rv-circle"
                style="padding:0.5rem 1rem; text-align:center; font-weight:bold; margin-top:2rem; display:none;">
            </div>
            <div id="questions" class="small-12 medium-12 large-12" style="margin-top:4rem;"></div>
        </div>

        <div id="descPage" class="small-12 medium-12 large-12 row align-center" style="margin:0rem; display:none;">
            <div id="descIntro" class="small-12 medium-10 large-10 rv-dark-gray" 
                style="text-align:center; font-size:1.2rem; margin:0rem 0rem 1.5rem 0rem;">
            </div>
            <div id="descArea" class="small-12 medium-10 large-10"></div>
            <div id="descNextButton" class="small-6 medium-4 large-3 rv-air-button rv-circle"
                style="padding:0.5rem 1rem; text-align:center; font-weight:bold; margin-top:2rem;">
            </div>
        </div>

        <div id="tagsPage" class="small-12 medium-12 large-12 row align-center" style="margin:0rem; display:none;">
            <div id="tagsIntro" class="small-12 medium-10 large-10 rv-dark-gray" 
                style="text-align:center; font-size:1.2rem; margin:0rem 0rem 1.5rem 0rem;">
            </div>
            <div id="suggestedTagsLabel" class="small-12 medium-10 large-10 rv-circle SoftBorder" 
                style="margin-bottom:0.5rem; color:rgb(80,80,80); background-color:rgb(245,245,245);
                    text-align:center; font-weight:bold; padding:0.5rem 0rem; display:none;">
            </div>
            <div id="tagsArea" class="small-12 medium-10 large-10"></div>
            <div class="small-12 medium-12 large-12"></div>
            <!--
            <div id="tagsNextButton" class="small-6 medium-4 large-3 rv-air-button rv-circle"
                style="padding:0.5rem 1rem; text-align:center; font-weight:bold; margin-top:2rem;">
            </div>
            -->
        </div>

        <!--
        <div id="usersPage" class="small-12 medium-12 large-12 row align-center" style="margin:0rem; display:none;">
            <div id="usersIntro" class="small-12 medium-10 large-10 rv-dark-gray" 
                style="text-align:center; font-size:1.2rem; margin:0rem 0rem 1.5rem 0rem;">
            </div>
            <div id="selectedUsers" class="small-12 medium-12 large-12 row" style="margin:0rem;"></div>
            <div class="small-12 medium-8 large-6" style="margin:1.5rem 0rem;">
                <input id="userInput" class="rv-input" style="width:100%;" />
            </div>
            <div id="usersArea" class="small-12 medium-10 large-10"></div>
        </div>
        -->
    </div>

    <script type="text/javascript">
        (function () {
            GlobalUtilities.load_files(["QA/NewQuestion.js"], { OnLoad: function () { new NewQuestion(); } });
        })();
    </script>
</asp:Content>
