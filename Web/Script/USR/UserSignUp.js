(function () {
    if (window.UserSignUp) return;

    window.UserSignUp = function () {
        var that = this;

        GlobalUtilities.load_files(["API/UsersAPI.js", "API/CNAPI.js", "CaptchaImage.js"], {
            OnLoad: function () {
                UsersAPI.GetPasswordPolicy({
                    ParseResults: true,
                    ResponseHandler: function (result) {
                        //_alert(JSON.stringify(result));

                        that.initialize();
                    }
                });
            }
        });
    }

    var __UserNames = {};

    UserSignUp.prototype = {
        initialize: function () {
            var that = this;

            var firstNameInnerTitle = ((window.RVDic || {}).FirstName || "FirstName") + "...";
            var lastNameInnerTitle = ((window.RVDic || {}).LastName || "LastName") + "...";
            var userNameInnerTitle = ((window.RVDic || {}).UserName || "UserName") + "...";
            var passwordInnerTitle = ((window.RVDic || {}).Password || "Password") + "...";
            var emailInnerTitle = ((window.RVDic || {}).Email || "Email") + "...";
            var emailConfirmInnerTitle = ((window.RVDic || {}).EmailConfirm || "EmailConfirm") + "...";

            var agreementNodeURL = CNAPI.NodePageURL({ NodeID: "6ac76f7c-1c98-4f13-9bc4-f71578df5e79" });

            var elems = GlobalUtilities.create_nested_elements([{
                Type: "div", Name: "container",
                Class: "small-10 medium-8 large-6 row rv-border-radius-1 SoftBackgroundColor",
                Style: "margin:0rem auto; padding:1rem 0.5rem;",
                Childs: [
                    {
                        Type: "div", Class: "small-12 medium-6 large-6",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            {
                                Type: "input", Class: "rv-input", Style: "width:100%;",
                                InnerTitle: firstNameInnerTitle, Name: "firstNameInput",
                                Attributes: [{ Name: "type", Value: "text" }],
                                Properties: [{
                                    Name: "onkeyup",
                                    Value: function () { this.style.direction = (GlobalUtilities.textdirection(this.value) || ''); }
                                }]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-12 medium-6 large-6",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            {
                                Type: "input", Class: "rv-input", Style: "width:100%;",
                                InnerTitle: lastNameInnerTitle, Name: "lastNameInput",
                                Attributes: [{ Name: "type", Value: "text" }],
                                Properties: [
                                    {
                                        Name: "onkeyup", Value:
                                            function () { this.style.direction = (GlobalUtilities.textdirection(this.value) || ''); }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            {
                                Type: "input", Class: "rv-input", Style: "width:100%;",
                                InnerTitle: userNameInnerTitle, Name: "userNameInput",
                                Attributes: [{ Name: "type", Value: "text" }],
                                Properties: [
                                    {
                                        Name: "onkeyup", Value:
                                            function () { this.style.direction = (GlobalUtilities.textdirection(this.value) || ''); }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            {
                                Type: "input", Class: "rv-input", Style: "width:100%;",
                                InnerTitle: passwordInnerTitle, Name: "passwordInput",
                                Attributes: [{ Name: "type", Value: "text" }],
                                Properties: [
                                    {
                                        Name: "onkeyup", Value:
                                            function () { this.style.direction = (GlobalUtilities.textdirection(this.value) || ''); }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            {
                                Type: "input", Class: "rv-input", Style: "width:100%;",
                                InnerTitle: emailInnerTitle, Name: "emailInput",
                                Attributes: [{ Name: "type", Value: "text" }],
                                Properties: [
                                    {
                                        Name: "onkeyup", Value:
                                            function () { this.style.direction = (GlobalUtilities.textdirection(this.value) || ''); }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            {
                                Type: "input", Class: "rv-input", Style: "width:100%; dir:ltr",
                                InnerTitle: emailConfirmInnerTitle, Name: "emailConfirmInput",
                                Attributes: [{ Name: "type", Value: "text" }],
                                Properties: [
                                    {
                                        Name: "onkeyup", Value:
                                            function () { this.style.direction = (GlobalUtilities.textdirection(this.value) || ''); }
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-12 medium-12 large-12",
                        Style: "margin-top:0.5rem; padding:0rem 0.5rem;",
                        Childs: [
                            { Type: "checkbox", Name: "userAgreementCheckbox" },
                            {
                                Type: "span", Style: "margin-" + RV_Float + ":0.3rem;",
                                Childs: [{ Type: "text", TextValue: " با " }]
                            },
                            {
                                Type: "span", Style: "color:blue;",
                                Style: "margin-" + RV_Float + ":0.3rem; color:blue; cursor:pointer;",
                                Properties: [{ Name: "onclick", Value: function (e) { GlobalUtilities.link_click(e, agreementNodeURL, { Open: true }); } }],
                                Childs: [{ Type: "text", TextValue: " قوانین سایت " }]
                            },
                            {
                                Type: "span", Style: "margin-" + RV_Float + ":0.3rem;",
                                Childs: [{ Type: "text", TextValue: " موافق هستم و آنها را رعایت می کنم " }]
                            }
                        ]
                    },
                    {
                        Type: "div", Class: "small-10 medium-8 large-4",
                        Style: "margin:1rem auto 0rem auto;", Name: "captchaArea"
                    },
                    { Type: "div", Class: "small-12 medium-12 large-12" },
                    {
                        Type: "div", Class: "small-10 medium-6 large-4 ActionButton", Name: "signUpButton",
                        Style: "margin:1rem auto 0rem auto; font-weight:bold;",
                        Childs: [{ Type: "text", TextValue: ((window.RVDic || {}).SignUp || "SignUp") }]
                    }
                ]
            }]);

            var emailInput = elems["emailInput"];
            var userAgreementCheckbox = elems["userAgreementCheckbox"];

            GlobalUtilities.necessary_input({ Input: elems["emailInput"], InnerTitle: emailInnerTitle });

            if (window.Email) {
                emailInput.value = Base64.decode(window.Email);
                emailInput.style.direction = (GlobalUtilities.textdirection(emailInput.value) || '');
                emailInput.style.color = "black";
                emailInput.style.fontWeight = "normal";
            };

            jQuery(elems["passwordInput"]).focus(function () { elems["passwordInput"].setAttribute("type", "password"); });

            var captchaArea = elems["captchaArea"];

            var showedDiv = GlobalUtilities.show(elems["container"]);

            var captchaObj = new CaptchaImage(captchaArea);

            var _checkTimeout = null;

            var _ucheck = function (result) {
                elems["userNameInput"].style.color = result === true ? "red" : "green";
                elems["userNameInput"].style.backgroundColor = result === true ? "#FCDDFB" : "rgba(160, 251, 160, 0.47)";
            }

            elems["userNameInput"].onkeyup = function () {
                if (_checkTimeout) { clearTimeout(_checkTimeout); _checkTimeout = null; }

                elems["userNameInput"].style.color = "black";
                elems["userNameInput"].style.backgroundColor = "white";

                var uname = GlobalUtilities.trim(elems["userNameInput"].value);
                if (uname == userNameInnerTitle || uname == "") return;

                if (typeof (__UserNames[uname]) != "undefined") return _ucheck(__UserNames[uname]);

                _checkTimeout = setTimeout(function () {
                    UsersAPI.CheckUserName({ UserName: Base64.encode(uname), ParseResults: true,
                        ResponseHandler: function (result) {
                            if (uname != GlobalUtilities.trim(elems["userNameInput"].value)) return;
                            __UserNames[uname] = result;
                            _ucheck(result);
                        }
                    });
                }, 1000);
            }

            var _checkMailConfirm = function () {
                var em = GlobalUtilities.trim(elems["emailInput"].value);
                var emc = GlobalUtilities.trim(elems["emailConfirmInput"].value);

                if (emc != emailConfirmInnerTitle) elems["emailConfirmInput"].style.color = "black";
                elems["emailConfirmInput"].style.backgroundColor = "white";

                if (em == emailInnerTitle) em = "";
                if (emc == emailConfirmInnerTitle) emc = "";

                if (em == "" || emc == "") return;

                elems["emailConfirmInput"].style.color = em == emc ? "green" : "red";
                elems["emailConfirmInput"].style.backgroundColor = em == emc ? "rgba(160, 251, 160, 0.47)" : "#FCDDFB";
            }

            elems["emailConfirmInput"].onkeyup = elems["emailInput"].onkeyup = _checkMailConfirm;
            var _processing = false;

            elems["signUpButton"].onclick = function () {
                if (_processing) return;

                var englishReg = /[A-Za-z]/g;
                var userNameReg = /^[a-zA-Z0-9.\-_$@*!]{3,30}$/;

                var firstname = GlobalUtilities.trim(elems["firstNameInput"].value);
                var lastname = GlobalUtilities.trim(elems["lastNameInput"].value);
                var username = GlobalUtilities.trim(elems["userNameInput"].value);
                var password = elems["passwordInput"].value;
                var email = elems["emailInput"].value;
                var emailConfirm = elems["emailConfirmInput"].value;
                var captcha = captchaObj ? captchaObj.get() : "";

                if (firstname == firstNameInnerTitle) firstname = "";
                if (lastname == lastNameInnerTitle) lastname = "";
                if (username == userNameInnerTitle) username = "";
                if (password == passwordInnerTitle) password = "";
                if (email == emailInnerTitle) email = "";
                if (emailConfirm == emailConfirmInnerTitle) emailConfirm = "";
                
                if (!firstname) return alert(RVDic.Checks.PleaseEnterYourFirstName);
                else if (!lastname) return alert(RVDic.Checks.PleaseEnterYourLastName);
                else if (englishReg.test(firstname) || englishReg.test(lastname))
                    return alert(RVDic.Checks["CannotUseEnglishCharactersForFirstNameAndLastName"]);
                else if (!username) return alert(RVDic.Checks.PleaseEnterYourUserName);
                else if (!password) return alert(RVDic.Checks.PleaseEnterYourPassword);
                else if (String(password).length < 5) return alert(RVDic.Checks.PasswordLengthMustBeGreaterThanN.replace("[n]", 4));
                else if (!email || !GlobalUtilities.is_valid_email(email)) return alert(RVDic.Checks.EmailIsNotValid);
                else if (email != emailConfirm) return alert(RVDic.Checks.EmailsDoesntMatch);
                else if (!captcha) return alert(RVDic.Checks.PleaseEnterSecurityCode);
                else if (!userNameReg.test(username)) return alert(RVDic.Checks.UserNameShouldOnlyIncludeEnglishCharactersNumbersAndSpecialCharacters);
                else if (!userAgreementCheckbox.checked) return alert(RVDic.Checks.YouMustAgreeWithTermsAndConditions);

                _processing = true;
                GlobalUtilities.block(elems["signUpButton"]);

                var reqParams = GlobalUtilities.request_params();
                
                UsersAPI.CreateTemporaryUser({
                    FirstName: Base64.encode(firstname), LastName: Base64.encode(lastname),
                    UserName: Base64.encode(username), Password: Base64.encode(password),
                    Email: email, InvitationID: reqParams.get_value("inv"), Captcha: Base64.encode(captcha),
                    ParseResults: true,
                    ResponseHandler: function (results) {
                        if (results.ErrorText) alert(RVDic.MSG[results.ErrorText] || results.ErrorText, { Timeout: 20000 });
                        else {
                            alert(RVDic.MSG[results.Succeed] + ". " + RVDic.MSG["AnEmailContainingActivationLinkSentToYou"], { Timeout: 20000 });
                            showedDiv.Close();
                        }

                        GlobalUtilities.unblock(elems["signUpButton"]);
                        _processing = false;
                    }
                });

                /*
                UsersAPI.IsInvited({ Email: Base64.encode(email),
                    ResponseHandler: function (responseText) {
                        if (responseText === true) _createTempUser();
                        else {
                            alert((window.RVDic.MSG.EmailHasNotBeedInvited || "EmailHasNotBeedInvited") + ". " +
                                (window.RVDic.MSG.EmailAddressAlreadyExists || "EmailAddressAlreadyExists"), { Timeout: 15000 });
                            GlobalUtilities.unblock(elems["signUpButton"]);
                            _processing = false;
                        }
                    }
                });
                */
            }
        }
    }
})();