<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="RaaiVan.Web.Page.View.Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <link rel="shortcut icon" href="../../Images/RaaiVanFav.ico" />

    <link rel="stylesheet" type="text/css" href="../../Script/jQuery/Tooltip/style.css" />
    <link rel="stylesheet" type="text/css" href="../../Fonts/font-awesome.css"/>
    <link rel="stylesheet" type="text/css" href="../../Script/Foundation/foundation.css"/>
    <link rel="stylesheet" type="text/css" href="../../CSS/foundation-fix.css"/>

    <script type="text/javascript" src="../../Script/jQuery/jquery.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.mb.browser.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.mousewheel.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.blockUI.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/Foundation/foundation.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/Foundation/what-input.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/API/RVRequest.js"></script>
    <script type="text/javascript" src="../../Script/TextEncoding.js"></script>
    <script type="text/javascript" src="../../Script/GlobalUtilities.js"></script>
    <script type="text/javascript" src="../../Script/OverrideAlerts.js"></script>
    <script type="text/javascript" src="../../Script/jQuery/Tooltip/jquery.tooltip.js"></script>
    <script type="text/javascript" src="../../Script/json2.js"></script>

    <script type="text/javascript" src="../../Script/API/RVAPI.js"></script>

    <script type="text/javascript">
        jQuery(document).ready(function () {
            jQuery(document).foundation();
        });
    </script>
</head>
<body class="Direction TextAlign" style="font-family:IRANSans;">
    <form id="idFrmMain" runat="server">
        <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" />
    </form>

    <div class="small-12 medium-12 large-12" style="width:100vw; height:100vh;">
        <div style="display:table; width:100%; height:100%;">
            <div id="errorArea" style="display:table-cell; vertical-align:middle; text-align:center;"></div>
        </div>
    </div>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value);

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
                    Type: "div", Style: "font-size:2rem; margin-bottom:1rem;",
                    Childs: [{ Type: "text", TextValue: RVDic.AnErrorOccurred }]
                },
                {
                    Type: "div", Style: "font-size:2rem; color:blue;" + (initialJson.Code ? "" : "display:none;"),
                    Childs: [{ Type: "text", TextValue: RVDic.ErrorCode + ": " + initialJson.Code }]
                }
            ], document.getElementById("errorArea"));
        })();
    </script>
</body>
</html>
