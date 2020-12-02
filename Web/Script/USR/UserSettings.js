(function () {
    if (window.UserSettings) return;

    window.UserSettings = function (containerDiv, params) {
        this.ContainerDiv = typeof (containerDiv) == "object" ? containerDiv : document.getElementById(containerDiv);
        if (!this.ContainerDiv) return;
        params = params || {};

        var that = this;

        GlobalUtilities.load_files(["API/UsersAPI.js"], { OnLoad: function () { that._initialize(); } });
    };

    UserSettings.prototype = {
        _initialize: function () {
            var that = this;

            var modules = (window.RVGlobal || {}).Modules || {};

            that.ContainerDiv.innerHTML = "";

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12 rv-title",
                    Childs: [{ Type: "text", TextValue: RVDic.PersonalSettings }]
                },
                {
                    Type: "div", Class: "small-12 medium-12 large-12 rv-trim-vertical-margins",
                    Childs: [
                        {
                            Type: "div", Name: "themeArea", Style: "margin-bottom:0.5rem;",
                            Class: "small-12 medium-12 large-12 rv-air-button rv-circle",
                            Properties: [{ Name: "onclick", Value: function () { that.select_theme(elems["themeArea"]); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.ThemeSelect }]
                        },
                        {
                            Type: "div", Name: "changePassword", Style: "margin-bottom:0.5rem;",
                            Class: "small-12 medium-12 large-12 rv-air-button rv-circle",
                            Properties: [{ Name: "onclick", Value: function () { that.change_password(); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.ChangePassword }]
                        },
                        (!modules.SMSEMailNotifier ? null : {
                            Type: "div", Name: "notificationSettings", Style: "margin-bottom:0.5rem;",
                            Class: "small-12 medium-12 large-12 rv-air-button rv-circle",
                            Properties: [{ Name: "onclick", Value: function () { that.notification_settings(); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.NotificationSettings }]
                        }),
                        (!modules.RestAPI ? null : {
                            Type: "div", Name: "remoteServers", Style: "margin-bottom:0.5rem;",
                            Class: "small-12 medium-12 large-12 rv-air-button rv-circle",
                            Properties: [{ Name: "onclick", Value: function () { that.remote_servers(); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.RemoteServers }]
                        })
                    ]
                }
            ], that.ContainerDiv);
        },

        select_theme: function (container) {
            var that = this;

            if (!window.RVGlobal.EnableThemes) return container.parentNode.removeChild(container);

            if (that.__Themes)
                that._select_theme(that.__Themes);
            else {
                RVAPI.GetThemes({
                    ParseResults: true,
                    ResponseHandler: function (result) {
                        that.__Themes = result.Themes
                        that._select_theme();
                    }
                });
            }
        },

        _select_theme: function () {
            var that = this;

            var themes = that.__Themes;

            var container = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-10 medium-8 large-5 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0rem auto 0rem auto; padding:1rem;", Name: "container"
                }
            ])["container"];

            var showedDiv = GlobalUtilities.show(container);

            var arr = [];

            var _add = function (thm) {
                var codes = thm.Codes;

                var thmCode = thm.Name.substr(thm.Name.indexOf("_") + 1);
                
                var items = [];

                var _add_item = function (itm) {
                    itm = itm || {};

                    items.push({
                        Type: "div", Class: "rv-circle",
                        Style: "display:inline-block; padding:0.001rem; margin:0.1rem; width:1.5rem; height:1.5rem;" +
                            "border-width:0.1rem; border-style:solid; border-color:transparent;" +
                            (itm.Style ? itm.Style : "")
                    });
                };

                _add_item({ Name: "color", Style: "background-color:" + codes.color + ";" });
                _add_item({ Name: "vsoft", Style: "background-color:" + codes.verysoft + ";" });
                _add_item({ Name: "soft", Style: "background-color:" + codes.soft + ";" });
                _add_item({ Name: "warm", Style: "background-color:" + codes.warm + ";" });
                _add_item({ Name: "vwarm", Style: "background-color:" + codes.verywarm + ";" });
                _add_item({ Name: "wborder", Style: "background-color:transparent; border-color:" + codes.warmborder + ";" });
                _add_item({ Name: "sborder", Style: "background-color:transparent; border-color:" + codes.softborder + ";" });
                _add_item({ Name: "acbtn", Style: "background-color:" + codes.actionbutton + "; border-color:" + codes.warmborder + "; color:white;" });
                _add_item({ Name: "htwarm", Style: "background-color:" + codes.highlytransparentwarm + ";" });
                _add_item({ Name: "vtwarm", Style: "background-color:" + codes.verytransparentwarm + ";" });
                _add_item({ Name: "mtwarm", Style: "background-color:" + codes.mediumtransparentwarm + ";" });
                _add_item({ Name: "twarm", Style: "background-color:" + codes.transparentwarm + ";" });

                items.push({
                    Type: "div", Class: "check-button", 
                    Style: "position:absolute; top:0.1rem;" + RV_Float + ":0rem; width:2rem; text-align:center;" +
                        "display:" + (String(thm.Name).toLowerCase() == String(window.RVGlobal.Theme).toLowerCase() ? "block;" : "none;"),
                    Properties: [{ Name: "themeName", Value: thm.Name }],
                    Childs: [
                        {
                            Type: "i", Class: "fa fa-check fa-2x rv-icon-button", Style: "cursor:default;",
                            Attributes: [{ Name: "aria-hidden", Value: true }]
                        }
                    ]
                });

                arr.push({
                    Type: "div", Class: "small-12 medium-12 large-12", 
                    Style: "position:relative; margin:0.2rem; padding:0.2rem; background-color:white; cursor:pointer;" +
                        "padding-" + RV_Float + ":3rem; height:2.1rem;" + GlobalUtilities.border_radius("0.3rem"),
                    Tooltip: !thmCode || (thmCode.toLowerCase() == "default") ? null : thmCode,
                    Properties: [
                        { Name: "onmouseover", Value: function () { this.style.backgroundColor = thm.Codes.reverse; } },
                        { Name: "onmouseout", Value: function () { this.style.backgroundColor = "white"; } },
                        {
                            Name: "onclick",
                            Value: function () {
                                UsersAPI.SetTheme({
                                    Theme: thm.Name, ParseResults: true,
                                    ResponseHandler: function (result) {
                                        if (result.ErrorText) return;

                                        var curThmUrl = RVAPI.ThemeURL({ Name: window.RVGlobal.Theme || "Default" });
                                        var newThmUrl = RVAPI.ThemeURL({ Name: thm.Name });
                                        
                                        DynamicFileUtilities.replace_css(curThmUrl, newThmUrl);

                                        window.RVGlobal.Theme = thm.Name;

                                        var chks = container.getElementsByClassName("check-button");

                                        for (var i = 0; i < chks.length; ++i)
                                            chks[i].style.display = (String(chks[i].themeName).toLowerCase() == String(thm.Name).toLowerCase() ? "block" : "none");
                                    }
                                });
                            }
                        }
                    ],
                    Childs: items
                });

                if (String(thm.Name).toLowerCase() == "default") {
                    var temp = [];
                    temp.push(arr[arr.length - 1]);
                    for (var i = 0; i < (arr.length - 1) ; ++i)
                        temp.push(arr[i]);
                    arr = temp;
                }
            }

            for (var i = 0, lnt = (themes || []).length; i < lnt; ++i)
                _add(themes[i]);

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12",
                    Style: "text-align:center; font-weight:bold; margin-bottom:0.5rem;",
                    Childs: [{ Type: "text", TextValue: RVDic.ThemeSelect }]
                },
                { Type: "div", Childs: arr }
            ], container);
        },

        change_password: function () {
            var that = this;

            UsersAPI.GetPasswordPolicy({
                ParseResults: true,
                ResponseHandler: function (result) {
                    result = result || {};

                    var hasPolicy = !!result.MinLength || (result.NewCharacters && (result.NewCharacters > 1)) ||
                        result.UpperLower || result.NonAlphabetic || result.Number || result.NonAlphaNumeric;

                    that._change_password({ HasPolicy: hasPolicy, Policy: result });
                }
            });
        },

        _change_password: function (settings) {
            var that = this;

            var container = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-10 medium-8 large-6 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0rem auto; padding:1rem;", Name: "container"
                }
            ])["container"];

            var showedDiv = GlobalUtilities.show(container);

            var _inputs = [];

            var _add_input = function (name, title) {
                _inputs.push({
                    Type: "div", Class: "small-12 medium-12 large-12 row",
                    Style: "margin:0rem; margin-bottom:0.5rem;",
                    Childs: [
                        {
                            Type: "div", Class: "small-6 medium-4 large-3", Style: "font-weight:bold;",
                            Childs: [{ Type: "text", TextValue: title + ":" }]
                        },
                        {
                            Type: "div", Class: "small-6 medium-8 large-9",
                            Childs: [
                                {
                                    Type: "input", Class: "rv-input", Name: name, Style: "width:100%;",
                                    Attributes: [{ Name: "type", Value: "password" }]
                                }
                            ]
                        }
                    ]
                });
            };

            _add_input("curPassInput", RVDic.CurrentPassword);
            _add_input("newPassInput", RVDic.NewPassword);
            _add_input("repNewPassInput", RVDic.RepeatNewPassword);

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12", Style: "margin:0rem;",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "text-align:center; margin:0.3rem 0rem 1rem 0.3rem; font-weight:bold;",
                            Childs: [{ Type: "text", TextValue: RVDic.ChangePassword }]
                        },
                        { Type: "div", Childs: _inputs },
                        {
                            Type: "div", Class: "small-8 medium-6 large-4 ActionButton",
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
                                                if (results.Succeed) showedDiv.Close();
                                                alert(RVDic.MSG[msg] || msg, { Timeout: results.ErrorText ? 20000 : null });
                                            }
                                        });
                                    }
                                }
                            ],
                            Childs: [{ Type: "text", TextValue: RVDic.Save }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdPolicy", Style: "margin:1rem 0rem 0.5rem 0rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.PasswordPolicyIsAsFollows + ":" }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdMinLength", Style: "color:#f00; display:none;",
                            Childs: [{ Type: "text", TextValue: "- " + RVDic.PasswordPolicyMinLength.replace("n", settings.Policy.MinLength || "0") }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdNewCharacters", Style: "color:#f00; display:none;",
                            Childs: [{ Type: "text", TextValue: "- " + RVDic.PasswordPolicyNewCharacters.replace("n", settings.Policy.NewCharacters || "0") }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdUpperLower", Style: "color:#f00; display:none;",
                            Childs: [{ Type: "text", TextValue: "- " + RVDic.PasswordPolicyUpperLower }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdNonAlphabetic", Style: "color:#f00; display:none;",
                            Childs: [{ Type: "text", TextValue: "- " + RVDic.PasswordPolicyNonAlphabetic }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdNumber", Style: "color:#f00; display:none;",
                            Childs: [{ Type: "text", TextValue: "- " + RVDic.PasswordPolicyNumber }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Name: "pwdNonAlphaNumeric", Style: "color:#f00; display:none;",
                            Childs: [{ Type: "text", TextValue: "- " + RVDic.PasswordPolicyNonAlphaNumeric }]
                        }
                    ]
                }
            ], container);

            if (settings.HasPolicy) jQuery(elems["pwdPolicy"]).fadeIn(0);
            if (settings.Policy.MinLength) jQuery(elems["pwdMinLength"]).fadeIn(0);
            if (settings.Policy.NewCharacters && settings.Policy.NewCharacters > 1) jQuery(elems["pwdNewCharacters"]).fadeIn(0);
            if (settings.Policy.UpperLower) jQuery(elems["pwdUpperLower"]).fadeIn(0);
            if (settings.Policy.NonAlphabetic) jQuery(elems["pwdNonAlphabetic"]).fadeIn(0);
            if (settings.Policy.Number) jQuery(elems["pwdNumber"]).fadeIn(0);
            if (settings.Policy.NonAlphaNumeric) jQuery(elems["pwdNonAlphaNumeric"]).fadeIn(0);

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
        },

        notification_settings: function () {
            var that = this;

            if (that.NotificationSettingsPanel) return GlobalUtilities.show(that.NotificationSettingsPanel);

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-11 medium-10 large-9 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0 auto; padding:1rem;", Name: "container",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "text-align:center; margin-bottom:1rem; font-size:1.2rem; font-weight:bold;",
                            Childs: [{ Type: "text", TextValue: RVDic.NotificationSettings }]
                        },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "_div" }
                    ]
                }
            ]);

            that.NotificationSettingsPanel = elems["container"];

            GlobalUtilities.loading(elems["_div"]);
            GlobalUtilities.show(elems["container"]);

            GlobalUtilities.load_files(["Notifications/SendMessageUserSetting.js"], {
                OnLoad: function () {
                    var settings = window.RVGlobal || {};
                    
                    new SendMessageUserSetting(elems["_div"], {
                        UserID: settings.UserID,
                        CurrentUserID: settings.CurrentUserID,
                        IsSystemAdmin: settings.IsSystemAdmin === true
                    });
                }
            });
        },

        remote_servers: function () {
            var that = this;

            if (that.RemoteServersPanel) return GlobalUtilities.show(that.RemoteServersPanel);

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-11 medium-10 large-9 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0 auto; padding:1rem;", Name: "container",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12 rv-title",
                            Childs: [{ Type: "text", TextValue: RVDic.RemoteServers }]
                        },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "_div" }
                    ]
                }
            ]);

            that.RemoteServersPanel = elems["container"];

            GlobalUtilities.loading(elems["_div"]);
            GlobalUtilities.show(elems["container"]);

            GlobalUtilities.load_files(["RemoteServers/RemoteServerSettings.js"], {
                OnLoad: function () { new RemoteServerSettings(elems["_div"]); }
            });
        }
    }
})();