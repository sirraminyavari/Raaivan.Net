<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Graph.aspx.cs" Inherits="RaaiVan.Web.Page.View.Graph" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>

    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <meta http-equiv="X-UA-Compatible" content="IE=EmulateIE8" />

    <link rel="shortcut icon" href="../../Images/RaaiVanFav.ico" />

    <link type="text/css" href="../../Script/JIT/base.css" rel="stylesheet" />

    <link rel="stylesheet" type="text/css" href="../../Script/jQuery/Tooltip/style.css" />
    <link rel="Stylesheet" type="text/css" href="../../Script/jQuery/nanoScroller/nanoscroller.css" />
    <link rel="stylesheet" type="text/css" href="../../CSS/Global.css"/>
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
    <script type="text/javascript" src="../../Script/jQuery/nanoScroller/jquery.nanoscroller.js"></script>
    <script type="text/javascript" src="../../Script/jQuery/nanoScroller/overthrow.js"></script>
    <script type="text/javascript" src="../../Script/json2.js"></script>
    <script type="text/javascript" src="../../Script/AutoSuggest/autosuggest.js"></script>

    <script src="../../Script/API/RVAPI.js" type="text/javascript"></script>
    <script src="../../Script/GraphViewer/GraphViewer.js" type="text/javascript" ></script>

    <!--[if IE]><script type="text/javascript" src="../../Script/JIT/excanvas.js"></script><![endif]-->
    <script type="text/javascript" src="../../Script/JIT/jit.js"></script>
</head>

<body class="Direction TextAlign" style="font-family:IRANSans;">
    <form runat="server"></form>

    <div id="graphContainer"></div>

    <script type="text/javascript">
        (function () {
            document.title = Base64.decode((window.RVGlobal || {}).SystemTitle);

            if ((window.RVGlobal || {}).NoApplicationFound) return alert(RVDic.MSG.NoApplicationFound);

            (function () { new GraphViewer("graphContainer"); })();
        })();
    </script>
</body>
</html>