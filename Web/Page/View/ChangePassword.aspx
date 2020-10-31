<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="RaaiVan.Web.Page.View.ChangePassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>تغییر رمز عبور</title>

    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <link rel="shortcut icon" href="../../Images/RaaiVanFav.ico" />

    <link rel="stylesheet" href="../../CSS/Global.css" />
    <link rel="stylesheet" href="../../Script/Foundation/foundation.css" />
    <link rel="stylesheet" href="../../CSS/foundation-fix.css" />

    <script type="text/javascript" src="../../Script/jQuery/jquery.js"></script>
    <script type="text/javascript" src="../../Script/jQuery/jquery.mb.browser.js" charset="utf-8"></script>

    <script type="text/javascript" src="../../Script/API/RVRequest.js"></script>
    <script type="text/javascript" src="../../Script/TextEncoding.js"></script>
    <script type="text/javascript" src="../../Script/GlobalUtilities.js"></script>
    <script type="text/javascript" src="../../Script/OverrideAlerts.js"></script>
    <script type="text/javascript" src="../../Script/Lang/fa.js"></script>
    <script type="text/javascript" src="../../Script/json2.js"></script>

    <script type="text/javascript" src="../../Script/API/RVAPI.js"></script>
    <script type="text/javascript" src="../../Script/API/UsersAPI.js"></script>

    <script type="text/javascript" src="../../Script/Foundation/foundation.js"></script>

    <script type="text/javascript">
        jQuery(document).ready(function () { jQuery(document).foundation(); });
    </script>
</head>
<body class="Direction TextAlign" style="font-family:IRANSans;">
    <form id="form1" runat="server">
        <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" />

        <script type="text/javascript">
            (function () { window.RVGlobal = JSON.parse(document.getElementById("initialJson").value); })();
        </script>
    </form>

    <div class="small-12 medium-12 large-12 row align-center" style="margin:1.5rem;">
        <div id="homeButton" class="smalll-5 medium-4 large-3" 
            style="text-align:center; font-weight:bold; color:blue; cursor:pointer; display:none;">
        </div>
        <div class="small-2 medium-4 large-6"></div>
        <div id="logoutButton" class="smalll-5 medium-4 large-3" 
            style="text-align:center; font-weight:bold; color:blue; cursor:pointer;">
        </div>
    </div>

    <div class="small-12 medium-12 large-12 row align-center" style="margin:0rem; padding:0vw 4vw 0vw 4vw;">
        <div class="small-12 medium-10 large-6 row align-center SoftBackgroundColor rv-border-radius-1" 
            style="margin:2rem 0rem 1rem 0rem; padding:1rem;">
            <div id="formTitle" class="small-12 medium-12 large-12" 
                style="font-weight:bold; text-align:center; margin-bottom:2rem; font-size:1.5rem;">
            </div>
            <div class="small-12 medium-12 large-12">
                <input type="password" id="currentPassword" class="rv-input" style="width:100%; margin-bottom:1rem;" />
                <input type="password" id="newPassword" class="rv-input" style="width:100%; margin-bottom:1rem;" />
                <input type="password" id="newPasswordRepeat" class="rv-input" style="width:100%; margin-bottom:1rem;" />
            </div>
            <div class="small-6 medium-4 large-3">
                <div id="submitButton" class="button" style="width:100%;"></div>
            </div>
        </div>
        <div class="small-12 medium-12 large-12"></div>

        <div id="reasonInfo" class="small-12 medium-12 large-12 row align-center" style="margin-top:20px; font-weight:bold;"></div>

        <div id="pwdPolicy" class="small-12 medium-10 large-6 row" style="margin:1rem 0rem; display:none;">
            <div id="pwdPolicyTitle" class="small-12 medium-12 large-12" style="margin-bottom:1rem;"></div>
            <div id="pwdMinLength" class="small-12 medium-12 large-12" style="display:none; color:#f00;"></div>
            <div id="pwdNewCharacters" class="small-12 medium-12 large-12" style="display:none; color:#f00;"></div>
            <div id="pwdUpperLower" class="small-12 medium-12 large-12" style="display:none; color:#f00;"></div>
            <div id="pwdNonAlphabetic" class="small-12 medium-12 large-12" style="display:none; color:#f00;"></div>
            <div id="pwdNumber" class="small-12 medium-12 large-12" style="display:none; color:#f00;"></div>
            <div id="pwdNonAlphaNumeric" class="small-12 medium-12 large-12" style="display:none; color:#f00;"></div>
        </div>
    </div>

    <script type="text/javascript">
        (function () {
            "use strict";

            if (window.ChangePasswordDialog) return;

            window.ChangePasswordDialog = function () {
                this.Objects = {
                    HasPolicy: false,
                    Policy: null
                };

                var that = this;
                
                GlobalUtilities.load_files(["API/UsersAPI.js"], {
                    OnLoad: function () {
                        UsersAPI.GetPasswordPolicy({
                            ParseResults: true,
                            ResponseHandler: function (result) {
                                result = result || {};

                                that.Objects.HasPolicy = !!result.MinLength || (result.NewCharacters && (result.NewCharacters > 1)) ||
                                    result.UpperLower || result.NonAlphabetic || result.Number || result.NonAlphaNumeric;
                                that.Objects.Policy = result;

                                that.initialize();
                            }
                        });
                    }
                });
            }

            ChangePasswordDialog.prototype = {
                initialize: function () {
                    var that = this;

                    if (that.Objects.HasPolicy) {
                        jQuery("#pwdPolicyTitle").html(RVDic.PasswordPolicyIsAsFollows + ":");
                        jQuery("#pwdPolicy").fadeIn(0);
                    }
                    if (that.Objects.Policy.MinLength) {
                        jQuery("#pwdMinLength").html("- " + GlobalUtilities.convert_numbers_to_persian(
                            RVDic.PasswordPolicyMinLength.replace("n", that.Objects.Policy.MinLength)));
                        jQuery("#pwdMinLength").fadeIn(0);
                    }
                    if (that.Objects.Policy.NewCharacters) {
                        jQuery("#pwdNewCharacters").html("- " + GlobalUtilities.convert_numbers_to_persian(
                            RVDic.PasswordPolicyNewCharacters.replace("n", that.Objects.Policy.NewCharacters)));
                        jQuery("#pwdNewCharacters").fadeIn(0);
                    }
                    if (that.Objects.Policy.UpperLower) {
                        jQuery("#pwdUpperLower").html("- " + RVDic.PasswordPolicyUpperLower);
                        jQuery("#pwdUpperLower").fadeIn(0);
                    }
                    if (that.Objects.Policy.NonAlphabetic) {
                        jQuery("#pwdNonAlphabetic").html("- " + RVDic.PasswordPolicyNonAlphabetic);
                        jQuery("#pwdNonAlphabetic").fadeIn(0);
                    }
                    if (that.Objects.Policy.Number) {
                        jQuery("#pwdNumber").html("- " + RVDic.PasswordPolicyNumber);
                        jQuery("#pwdNumber").fadeIn(0);
                    }
                    if (that.Objects.Policy.NonAlphaNumeric) {
                        jQuery("#pwdNonAlphaNumeric").html("- " + RVDic.PasswordPolicyNonAlphaNumeric);
                        jQuery("#pwdNonAlphaNumeric").fadeIn(0);
                    }

                    var homeButton = document.getElementById("homeButton");
                    var logoutButton = document.getElementById("logoutButton");
                    var formTitle = document.getElementById("formTitle");
                    var currentPasswordInput = document.getElementById("currentPassword");
                    var newPasswordInput = document.getElementById("newPassword");
                    var newPasswordRepeatInput = document.getElementById("newPasswordRepeat");
                    var submitButtonInput = document.getElementById("submitButton");

                    homeButton.innerHTML = RVDic.Home;
                    logoutButton.innerHTML = RVDic.Logout;
                    formTitle.innerHTML = RVDic.ChangePassword;
                    currentPasswordInput.setAttribute("placeholder", RVDic.CurrentPassword + "...");
                    newPasswordInput.setAttribute("placeholder", RVDic.NewPassword + "...");
                    newPasswordRepeatInput.setAttribute("placeholder", RVDic.RepeatNewPassword + "...");
                    submitButton.innerHTML = RVDic.ChangePassword;

                    if (window.RVGlobal.PasswordChangeNeeded) {
                        var reason = window.RVGlobal.PasswordChangeReason;

                        document.getElementById("reasonInfo").innerHTML =
                            reason == "FirstPassword" ? "لازم است تا اولین رمز عبور خود را تغییر دهید" :
                            (reason == "PasswordExpired" ? "رمز عبور شما منقضی شده است و باید آن را تغییر دهید" :
                            "لازم است تا رمز عبور خود را تغییر دهید");
                    }
                    else homeButton.style.display = "block";

                    var check_repeat = function () {
                        var pass = newPasswordInput.value, passRepeat = newPasswordRepeatInput.value;
                        jQuery(newPasswordRepeatInput).get(0).style.backgroundColor = !passRepeat ? "white" :
                            (passRepeat == pass ? "rgba(160, 251, 160, 0.47)" : "#FCDDFB");
                    };

                    var check_pass = function () {
                        var pass = newPasswordInput.value;
                        var result = that.check_policy(pass, currentPasswordInput.value);
                        jQuery(newPasswordInput).get(0).style.backgroundColor = !pass ? "white" :
                            (result ? "rgba(160, 251, 160, 0.47)" : "#FCDDFB");
                        check_repeat();
                    };

                    jQuery(currentPasswordInput).keyup(check_pass);
                    jQuery(newPasswordInput).keyup(check_pass);
                    jQuery(newPasswordRepeatInput).keyup(check_repeat);

                    submitButton.onclick = function () {
                        if (submitButton.Processing) return;

                        var password = currentPasswordInput.value;
                        var newPassword = newPasswordInput.value;
                        var newPasswordRepeat = newPasswordRepeatInput.value;

                        if (!password) return alert(RVDic.PleaseEnterYourCurrentPassword, { Timeout: 20000 });
                        else if (!that.check_policy(newPassword, password)) return alert(RVDic.MSG.PasswordPolicyDidntMeet, { Timeout: 20000 });
                        else if (newPassword != newPasswordRepeat) return alert(RVDic.Checks.PasswordsDoesntMatch, { Timeout: 20000 });

                        submitButton.Processing = true;

                        UsersAPI.ChangePassword({
                            CurrentPassword: Base64.encode(password), NewPassword: Base64.encode(newPassword),
                            ParseResults: true,
                            ResponseHandler: function (results) {
                                submitButton.Processing = false;
                                var msg = results.ErrorText || results.Succeed;
                                alert(RVDic.MSG[msg] || msg, { Timeout: results.ErrorText ? 20000 : null });

                                if (results.Succeed) {
                                    setTimeout(function () {
                                        window.location.href = UsersAPI.UserPageURL()
                                    }, 2000);
                                }
                            }
                        });
                    }

                    logoutButton.onclick = function () {
                        var logoutButton = this;
                        if (logoutButton._Processing) return;
                        logoutButton._Processing = true;

                        RVAPI.Logout({
                            ParseResults: true,
                            ResponseHandler: function (result) { window.location.href = RVAPI.LoginPageURL(); }
                        });
                    }

                    homeButton.onclick = function () {
                        window.location.href = RVAPI.HomePageURL();
                    }
                },

                check_policy: function (pass, oldPass) {
                    var that = this;

                    pass = String(pass);

                    var result = {
                        MinLength: pass && (!that.Objects.Policy.MinLength || (pass.length >= that.Objects.Policy.MinLength)),
                        NewCharacters: pass && that.Objects.Policy.NewCharacters &&
                            (GlobalUtilities.diff(pass, oldPass).length >= that.Objects.Policy.NewCharacters),
                        UpperLower: pass && (!that.Objects.Policy.UpperLower || (/[a-z]/g.test(pass) && /[A-Z]/g.test(pass))),
                        NonAlphabetic: pass && (!that.Objects.Policy.NonAlphabetic || !/^[a-zA-Z]+$/g.test(pass)),
                        Number: pass && (!that.Objects.Policy.Number || /[0-9]/g.test(pass)),
                        NonAlphaNumeric: pass && (!that.Objects.Policy.NonAlphaNumeric || !/^[a-zA-Z0-9]+$/g.test(pass))
                    }

                    jQuery("#pwdMinLength").css({ color: result.MinLength ? "rgb(22,188,31)" : "#f00" });
                    jQuery("#pwdNewCharacters").css({ color: result.NewCharacters ? "rgb(22,188,31)" : "#f00" });
                    jQuery("#pwdUpperLower").css({ color: result.UpperLower ? "rgb(22,188,31)" : "#f00" });
                    jQuery("#pwdNonAlphabetic").css({ color: result.NonAlphabetic ? "rgb(22,188,31)" : "#f00" });
                    jQuery("#pwdNumber").css({ color: result.Number ? "rgb(22,188,31)" : "#f00" });
                    jQuery("#pwdNonAlphaNumeric").css({ color: result.NonAlphaNumeric ? "rgb(22,188,31)" : "#f00" });

                    for (var k in result)
                        if (!result[k]) return false;

                    return true;
                }
            }
        })();
    </script>

    <script type="text/javascript">
        (function () {
            new ChangePasswordDialog();
        })();
    </script>
</body>
</html>