(function () {
    if (window.ChangePasswordDialog) return;

    window.ChangePasswordDialog = function (container, params) {
        this.Container = typeof (container) == "object" ? container : document.getElementById(container);
        if (!this.Container) return;
        params = params || {};
        
        this.Options = {
            OnPasswordChange: params.OnPasswordChange
        };

        var that = this;
        
        GlobalUtilities.load_files(["API/UsersAPI.js"], {
            OnLoad: function () { that.preinit(params); }
        });
    };

    ChangePasswordDialog.prototype = {
        preinit: function (params) {
            var that = this;
            
            UsersAPI.GetPasswordPolicy({
                ParseResults: true,
                ResponseHandler: function (result) {
                    result = result || {};
                    
                    var hasPolicy = !!result.MinLength || (result.NewCharacters && (result.NewCharacters > 1)) ||
                        result.UpperLower || result.NonAlphabetic || result.Number || result.NonAlphaNumeric;

                    that.initialize(GlobalUtilities.extend(params || {}, { HasPolicy: hasPolicy, Policy: result }));
                }
            });
        },

        initialize: function (settings) {
            var that = this;
            settings = settings || {};

            that.Container.innerHTML = "";

            var elems = GlobalUtilities.create_nested_elements([{
                Type: "div", Class: "small-12 medium-12 large-12", Style: "margin:0rem;",
                Childs: [
                    {
                        Type: "div", Class: "small-12 medium-12 large-12 rv-title",
                        Childs: [{ Type: "text", TextValue: RVDic.ChangePassword }]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Childs: [
                            { Name: "curPassInput", Title: RVDic.CurrentPassword },
                            { Name: "newPassInput", Title: RVDic.NewPassword },
                            { Name: "repNewPassInput", Title: RVDic.RepeatNewPassword }
                        ].map(itm => {
                            return {
                                Type: "div", Class: "small-12 medium-12 large-12 row",
                                Style: "margin:0rem; margin-bottom:0.5rem;",
                                Childs: [
                                    {
                                        Type: "div", Class: "small-6 medium-4 large-3", Style: "font-weight:bold;",
                                        Childs: [{ Type: "text", TextValue: itm.Title + ":" }]
                                    },
                                    {
                                        Type: "div", Class: "small-6 medium-8 large-9",
                                        Childs: [{
                                            Type: "input", Class: "rv-input", Name: itm.Name, Style: "width:100%;",
                                            Attributes: [{ Name: "type", Value: "password" }]
                                        }]
                                    }
                                ]
                            };
                        })
                    },
                    {
                        Type: "div", Class: "small-8 medium-6 large-4 rv-air-button rv-circle",
                        Style: "margin:1rem auto 0.6rem auto;",
                        Properties: [
                            {
                                Name: "onclick",
                                Value: function () {
                                    var btn = this;

                                    if (btn.Processing) return;
                                    var curPass = elems["curPassInput"].value;
                                    var newPass = elems["newPassInput"].value;
                                    var repNewPass = elems["repNewPassInput"].value;

                                    if (!curPass) return alert(RVDic.PleaseEnterYourCurrentPassword, { Timeout: 20000 });
                                    else if (!check_policy(newPass)) return alert(RVDic.MSG.PasswordPolicyDidntMeet, { Timeout: 20000 });
                                    else if (newPass != repNewPass) return alert(RVDic.Checks.PasswordsDoesntMatch, { Timeout: 20000 });

                                    btn.Processing = true;

                                    UsersAPI.ChangePassword({
                                        CurrentPassword: Base64.encode(curPass), NewPassword: Base64.encode(newPass),
                                        ParseResults: true,
                                        ResponseHandler: function (results) {
                                            btn.Processing = false;
                                            var msg = results.ErrorText || results.Succeed;
                                            if (results.Succeed) {
                                                if (GlobalUtilities.get_type(that.Options.OnPasswordChange) == "function")
                                                    that.Options.OnPasswordChange();
                                            }
                                            alert(RVDic.MSG[msg] || msg, { Timeout: results.ErrorText ? 20000 : null });
                                        }
                                    });
                                }
                            }
                        ],
                        Childs: [{ Type: "text", TextValue: RVDic.Save }]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12", Name: "reason",
                        Style: "text-align:center; font-weight:bold; margin:2rem 0 1rem 0; display:none;"
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Name: "pwdPolicy", Style: "margin:1rem 0rem 0.5rem 0rem;",
                        Childs: [{ Type: "text", TextValue: RVDic.PasswordPolicyIsAsFollows + ":" }]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Childs: [
                            { Name: "pwdMinLength", Title: RVDic.PasswordPolicyMinLength.replace("n", settings.Policy.MinLength || "0") },
                            { Name: "pwdNewCharacters", Title: RVDic.PasswordPolicyMinLength.replace("n", settings.Policy.MinLength || "0") },
                            { Name: "pwdUpperLower", Title: RVDic.PasswordPolicyUpperLower },
                            { Name: "pwdNonAlphabetic", Title: RVDic.PasswordPolicyNonAlphabetic },
                            { Name: "pwdNumber", Title: RVDic.PasswordPolicyNumber },
                            { Name: "pwdNonAlphaNumeric", Title: RVDic.PasswordPolicyNonAlphaNumeric }
                        ].map(lbl => {
                            return {
                                Type: "div", Class: "small-12 medium-12 large-12", Name: lbl.Name, Style: "color:#f00; display:none;",
                                Childs: [{ Type: "text", TextValue: "- " + lbl.Title }]
                            };
                        })
                    }
                ]
            }], that.Container);

            if (settings.HasPolicy) jQuery(elems["pwdPolicy"]).fadeIn(0);
            if (settings.Policy.MinLength) jQuery(elems["pwdMinLength"]).fadeIn(0);
            if (settings.Policy.NewCharacters && (settings.Policy.NewCharacters > 1)) jQuery(elems["pwdNewCharacters"]).fadeIn(0);
            if (settings.Policy.UpperLower) jQuery(elems["pwdUpperLower"]).fadeIn(0);
            if (settings.Policy.NonAlphabetic) jQuery(elems["pwdNonAlphabetic"]).fadeIn(0);
            if (settings.Policy.Number) jQuery(elems["pwdNumber"]).fadeIn(0);
            if (settings.Policy.NonAlphaNumeric) jQuery(elems["pwdNonAlphaNumeric"]).fadeIn(0);
            
            if (settings.PasswordChangeReason) {
                jQuery(elems["reason"]).fadeIn(0);

                elems["reason"].innerHTML =
                    settings.PasswordChangeReason == "FirstPassword" ? RVDic.MSG.YouHaveToChangeYourFirstPassword :
                        (settings.PasswordChangeReason == "PasswordExpired" ? RVDic.MSG.YouHaveToChangeYourExpiredPassword :
                            RVDic.MSG.YouHaveToChangeYourPassword);
            }

            var check_policy = function (pass, oldPass) {
                pass = String(pass);

                //elems["pwdStrength"].style.backgroundColor = GlobalUtilities.password_score(pass).Color;

                var result = {
                    MinLength: pass && (!settings.Policy.MinLength || (pass.length >= settings.Policy.MinLength)),
                    NewCharacters: pass && settings.Policy.NewCharacters &&
                        (GlobalUtilities.diff(pass, oldPass).length >= settings.Policy.NewCharacters),
                    UpperLower: pass && (!settings.Policy.UpperLower || (/[a-z]/g.test(pass) && /[A-Z]/g.test(pass))),
                    NonAlphabetic: pass && (!settings.Policy.NonAlphabetic || !/^[a-zA-Z]+$/g.test(pass)),
                    Number: pass && (!settings.Policy.Number || /[0-9]/g.test(pass)),
                    NonAlphaNumeric: pass && (!settings.Policy.NonAlphaNumeric || !/^[a-zA-Z0-9]+$/g.test(pass))
                };

                jQuery(elems["pwdMinLength"]).css({ color: result.MinLength ? "rgb(22,188,31)" : "#f00" });
                jQuery(elems["pwdNewCharacters"]).css({ color: result.NewCharacters ? "rgb(22,188,31)" : "#f00" });
                jQuery(elems["pwdUpperLower"]).css({ color: result.UpperLower ? "rgb(22,188,31)" : "#f00" });
                jQuery(elems["pwdNonAlphabetic"]).css({ color: result.NonAlphabetic ? "rgb(22,188,31)" : "#f00" });
                jQuery(elems["pwdNumber"]).css({ color: result.Number ? "rgb(22,188,31)" : "#f00" });
                jQuery(elems["pwdNonAlphaNumeric"]).css({ color: result.NonAlphaNumeric ? "rgb(22,188,31)" : "#f00" });

                for (var k in result)
                    if (!result[k]) return false;

                return true;
            };

            var check_repeat = function () {
                var pass = elems["newPassInput"].value, passRepeat = elems["repNewPassInput"].value;
                elems["repNewPassInput"].style.backgroundColor = !passRepeat ? "white" :
                    (passRepeat == pass ? "rgba(160, 251, 160, 0.47)" : "#FCDDFB");
            };

            var check_pass = function () {
                var pass = elems["newPassInput"].value;
                var result = check_policy(pass, elems["curPassInput"].value);
                elems["newPassInput"].style.backgroundColor = !pass ? "white" :
                    (result ? "rgba(160, 251, 160, 0.47)" : "#FCDDFB");
                check_repeat();
            };

            jQuery(elems["curPassInput"]).keyup(check_pass);
            jQuery(elems["newPassInput"]).keyup(check_pass);
            jQuery(elems["repNewPassInput"]).keyup(check_repeat);
        }
    };
})();