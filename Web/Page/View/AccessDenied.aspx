<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="RaaiVan.Web.Page.View.AccessDenied" 
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="small-12 medium-12 large-12" style="width:100vw; height:calc(100vh - 8rem);">
        <div style="display:table; width:100%; height:100%;">
            <div id="msgAD" style="display:table-cell; vertical-align:middle; text-align:center;"></div>
        </div>
    </div>

    <script type="text/javascript">
        (function () {
            GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Style: "text-align:center; margin-bottom:1rem;",
                    Childs: [
                        {
                            Type: "img", Style: "max-width:20rem;", Link: RVAPI.HomePageURL(),
                            Attributes: [{ Name: "src", Value: GlobalUtilities.icon("RaaiVanLogo.png") }]
                        }
                    ]
                },
                {
                    Type: "div", Style: "font-size:2rem; margin-bottom:1rem; color:blue;",
                    Childs: [{ Type: "text", TextValue: RVDic.MSG.AccessDenied }]
                }
            ], document.getElementById("msgAD"));
        })();
    </script>
</asp:Content>
