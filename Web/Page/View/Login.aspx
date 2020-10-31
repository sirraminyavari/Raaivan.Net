<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RaaiVan.Web.Page.View.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <meta name="viewport" content="width=device-width, initial-scale=0.7" />

    <link rel="shortcut icon" href="../../Images/RaaiVanFav.ico" />

    <link rel="stylesheet" type="text/css" href="../../Script/jQuery/TextNTags/jquery-textntags.css" />
    <link rel="stylesheet" type="text/css" href="../../Script/jQuery/Tooltip/style.css" />
    <link rel="Stylesheet" type="text/css" href="../../Script/jQuery/nanoScroller/nanoscroller.css" />
    <link rel="stylesheet" type="text/css" href="../../CSS/Global.css"/>
    <link rel="stylesheet" type="text/css" href="../../Fonts/font-awesome.css"/>
    <link rel="stylesheet" type="text/css" href="../../Script/Foundation/foundation.css"/>
    <link rel="stylesheet" type="text/css" href="../../CSS/foundation-fix.css"/>
    <link rel="stylesheet" type="text/css" href="../../CSS/input-anim-label.css"/>

    <script type="text/javascript" src="../../Script/jQuery/jquery.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.mb.browser.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.mousewheel.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.blockUI.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/Foundation/foundation.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/Foundation/what-input.js" charset="utf-8"></script>
    <script type="text/javascript" src="../../Script/API/RVRequest.js?v=28"></script>
    <script type="text/javascript" src="../../Script/TextEncoding.js"></script>
    <script type="text/javascript" src="../../Script/GlobalUtilities.js?v=28"></script>
    <script type="text/javascript" src="../../Script/OverrideAlerts.js?v=28"></script>
    <script type="text/javascript" src="../../Script/Lib/underscore.js"></script>
    <script type="text/javascript" src="../../Script/jQuery/TextNTags/jquery-textntags.js"></script>
    <script type="text/javascript" src="../../Script/AdvancedTextArea/AdvancedTextArea.js?v=28"></script>
    <script type="text/javascript" src="../../Script/jQuery/Tooltip/jquery.tooltip.js"></script>
    <script type="text/javascript" src="../../Script/jQuery/nanoScroller/jquery.nanoscroller.js"></script>
    <script type="text/javascript" src="../../Script/jQuery/nanoScroller/overthrow.js"></script>
    <script type="text/javascript" src="../../Script/json2.js"></script>
    <script type="text/javascript" src="../../Script/AutoSuggest/autosuggest.js?v=28"></script>

    <script type="text/javascript" src="../../Script/API/UsersAPI.js?v=28"></script>
    <script type="text/javascript" src="../../Script/API/RVAPI.js?v=28"></script>
    <script type="text/javascript" src="../../Script/API/PrivacyAPI.js?v=28"></script>

    <script type="text/javascript">
        jQuery(document).ready(function () {
            jQuery(document).foundation();
        });
    </script>
</head>
<body class="Direction TextAlign" style="font-family:IRANSans;">
    <script type="text/javascript" src="../../Script/USR/LoginControl.js<% = "?rnd=" + DateTime.Now.Millisecond.ToString() %>"></script>

    <form id="idFrmMain" runat="server">
        <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" />
    </form>

    <div id="mainContent" class="small-12 medium-12 large-12" 
        style="background-repeat:no-repeat; background-attachment:fixed; background-position:center; background-size:cover; 
               height:100vh; display:flex; flex-flow:column; align-items:center; justify-content:center; position:relative;">
        <div style="width:100%;">
            <div class="small-12 medium-8 large-6 rv-border-radius-1" 
                style="padding:1rem 5rem; background-color:rgba(0,0,0,0.7); margin:0 auto;">
                <div id="loginArea" class="small-12 medium-12 large-12"></div>
            </div>
        </div>
        <div style="position:absolute; bottom:0.5rem; left:0; right:0; text-align:center;">
            <div id="pageDownButton" class="rv-air-button-base rv-air-button-white rv-circle" 
                style="display:inline-flex; align-items:center; justify-content:center; width:3rem; height:3rem;">
                <i class="fa fa-chevron-down fa-2x" aria-hidden="true"></i>
            </div>
        </div>
    </div>

    <div id="statisticsArea" class="small-12 medium-12 large-12 row align-center" 
        style="margin:0rem; padding:0vw 4vw; min-height:100vh; position:relative;">
        <div class="small-12 medium-12 large-12" style="text-align:center; padding-top:0.5rem; height:4rem;">
            <div id="pageUpButton" class="rv-air-button-base rv-air-button-black rv-circle" 
                style="display:inline-flex; align-items:center; justify-content:center; width:3rem; height:3rem;">
                <i class="fa fa-chevron-up fa-2x" aria-hidden="true" style="margin-bottom:0.5rem;"></i>
            </div>
        </div>
        <div id="loginPageContent" style="min-height:calc(100vh - 4rem); width:100%;"></div>
    </div>

    <script type="text/javascript">
        (function () {
            var ind = GlobalUtilities.random(6, 28);
            var bgUrl = GlobalUtilities.icon("background/RV-BG-" + (ind < 10 ? "0" : "") + ind + ".jpg");
            document.getElementById("mainContent").style.backgroundImage = "url(" + bgUrl + ")";

            var firstPage = document.getElementById("mainContent");
            var secondPage = document.getElementById("loginPageContent");

            document.getElementById("pageDownButton").onclick = function () {
                GlobalUtilities.scroll_into_view(secondPage, { Offset: 0 });
            };

            document.getElementById("pageUpButton").onclick = function () {
                GlobalUtilities.scroll_into_view(firstPage, { Offset: 0 });
            };

            var result = window.RVGlobal = JSON.parse(document.getElementById("initialJson").value);

            if (result.LoggedIn) {
                var msg = result.LoginMessage ? Base64.decode(result.LoginMessage) : null;

                GlobalUtilities.set_auth_cookie(result.AuthCookie);
                
                if (msg || ((result.LastLogins || []).length > 0))
                    (new LoginControl()).show_last_logins(msg, result.LastLogins, function () { window.location.href = "../../home"; });
                else window.location.href = "../../home";

                return;
            }

            document.title = RVDic.Login + " - " + document.title;
            
            if ((window.RVGlobal || {}).NoApplicationFound) return alert(RVDic.MSG.NoApplicationFound);
            
            if (result.SysID) {
                var sysElems = GlobalUtilities.create_nested_elements([{
                    Type: "div", Class: "small-10, medium-8 large-6 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0 auto; padding:1rem;", Name: "container",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12", Style: "text-align:center; font-size:1rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.MSG.InvalidSystemIDMessage }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12", Style: "text-align:center; margin-top:1rem;",
                            Childs: [
                                {
                                    Type: "div", Class: "rv-border-radius-half WarmTextShadow", Name: "sysId",
                                    Style: "display:inline-block; font-size:1rem; padding:0.5rem; background-color:white; color:rgb(80,80,80);"
                                }
                            ]
                        },
                        {
                            Type: "div", Class: "small-4 medium-4 large-4 rv-air-button rv-circle", Style: "margin:1rem auto 0 auto;",
                            Properties: [{ Name: "onclick", Value: function () { sysShowed.Close(); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.GotIt + "!" }]
                        }
                    ]
                }]);

                sysElems["sysId"].innerHTML = result.SysID;

                var sysShowed = GlobalUtilities.show(sysElems["container"]);
            }

            GlobalUtilities.cheadget("sudoku", "gesi", "ramin", "fliptext", "instaprofile");

            var loginPageModel = result.LoginPageModel || "";
            
            if (loginPageModel) {
                var _jsFileName = "lp_" + loginPageModel;
                var statistics = window.RVGlobal[loginPageModel];

                if (statistics) {
                    GlobalUtilities.load_files(["LoginPage/" + _jsFileName + ".js"], {
                        OnLoad: function () { new window[_jsFileName]("loginPageContent", statistics); }
                    });
                }
                else {
                    jQuery("#pageDownButton").fadeOut(0);
                    jQuery("#statisticsArea").fadeOut(0);
                }
            }

            var ssoUrl = Base64.decode((window.RVGlobal || {}).SSOLoginURL);
            
            new LoginControl("loginArea", {
                ReloadAfterLogin: false, ContainerClass: false, IgnoreSSO: true, InitialFocus: false,
                ReturnURL: (window.RVGlobal || {}).ReturnURL ? Base64.decode(window.RVGlobal.ReturnURL) : null,
                OnLogin: function (d) { }
            });
        })();
    </script>

    <script type="text/javascript">
        (function () {
            function browser_version() {
                var ua = navigator.userAgent, tem,
                    M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];

                if (/trident/i.test(M[1])) {
                    tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
                    return { Name: 'msie', Version: tem[1] || '' };
                }

                if (M[1] === 'Chrome') {
                    tem = ua.match(/\bOPR\/(\d+)/);
                    if (tem != null) return { Name: 'Opera', Version: tem[1] || '' };
                }

                M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
                if ((tem = ua.match(/version\/(\d+)/i)) != null) M.splice(1, 1, tem[1]);
                return { Name: String(M[0]).toLowerCase(), Version: M[1] };
            }

            var brv = browser_version();

            if (brv.Name == "msie")
                alert("با این مرورگر، بخشی از قابلیت های «رای ون » را از دست خواهید داد! اگر تجربه کاربری بهتری را می خواهید، لطفا آدرس این صفحه را در یکی از مرورگرهای زیر بارگذاری فرمایید:\nFirefox 10+, Chrome 10+, Opera 21+", { Original: true });
        })();
    </script>
</body>
</html>
