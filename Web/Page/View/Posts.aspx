<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Posts.aspx.cs" Inherits="RaaiVan.Web.Page.View.Posts"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="../../Script/SharingManager/SharingManager.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div class="small-12 medium-12 large-12" style="padding:0 10% 5rem 10%; margin:0rem auto;">
        <div id="ownerInfoArea" class="small-12 medium-12 large-12" style="margin-bottom:1.5rem; display:none;"></div>
        <div id="idDivSharingArea" class="small-12 medium-12 large-12"></div>
    </div>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value);
            
            var container = document.getElementById("idDivSharingArea");
            var infoArea = document.getElementById("ownerInfoArea");

            var ownerId = initialJson.OwnerID;
            var ownerType = initialJson.OwnerType;
            var postId = initialJson.PostID;

            if (postId || ownerId) jQuery(infoArea).fadeIn(0);

            if (ownerId || postId) {
                new SharingManager({
                    PostsContainerDivID: container, OwnerInfoArea: infoArea, OwnerObjectID: ownerId,
                    InitialFill: true, OwnerType: ownerType, PostID: postId, Permissions: null,
                    RealTime: true, RealTimeFeedID: postId || ownerId
                });
            }
            else {
                var sm = new SharingManager({
                    PostsContainerDivID: container, OwnerObjectID: (window.RVGlobal || {}).CurrentUserID,
                    InitialFill: true, OwnerType: "User", Permissions: { AddPost: true }, NewPostArea: "Advanced", News: true,
                    OnLoad: function () {
                        GlobalUtilities.onscrollend(document, { Offset: 10 }, function () { sm.get_posts(); });
                    }
                });
            }
        })();
    </script>
</asp:Content>
