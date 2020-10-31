(function () {
    if (window.NewQuestion) return;

    window.NewQuestion = function () {
        this.Interface = {
            PageTitle: null,
            Tabs: null,
            TitlePage: null,
            TitleIntro: null,
            TitleInput: null,
            TitleNextButton: null,
            Questions: null,
            DescIntro: null,
            DescPage: null,
            DescArea: null,
            DescInput: null,
            DescNextButton: null,
            TagsPage: null,
            TagsIntro: null,
            SuggestedTagsLabel: null,
            TagsArea: null,
            /*
            TagsNextButton: null,
            UsersPage: null,
            UsersIntro: null,
            SelectedUsers: null,
            UserInput: null,
            UsersArea: null,
            */
            ConfirmButton: null
        };

        this.Objects = {
            SuggestedTags: null,
            QATags: null,
            UsersList: null,
            AllPagesSeen: false
        };

        var that = this;

        GlobalUtilities.load_files([
            { Root: "API/", Ext: "js", Childs: ["QAAPI", "UsersAPI"] },
            { Root: "QA/", Ext: "js", Childs: ["QuestionMini", "QATags"] },
            "TabsManager/TabsManager.js"/*,*/
            /*"SimpleListViewer/NewSimpleListViewer.js"*/
        ], { OnLoad: function () { that.initialize(); } });
    }

    NewQuestion.prototype = {
        initialize: function () {
            var that = this;

            that.Interface.PageTitle = document.getElementById("pageTitle");
            that.Interface.Tabs = document.getElementById("tabs");
            that.Interface.TitlePage = document.getElementById("titlePage");
            that.Interface.TitleIntro = document.getElementById("titleIntro");
            that.Interface.TitleInput = document.getElementById("titleInput");
            that.Interface.TitleNextButton = document.getElementById("titleNextButton");
            that.Interface.Questions = document.getElementById("questions");
            that.Interface.DescIntro = document.getElementById("descIntro");
            that.Interface.DescPage = document.getElementById("descPage");
            that.Interface.DescNextButton = document.getElementById("descNextButton");
            that.Interface.TagsPage = document.getElementById("tagsPage");
            that.Interface.TagsIntro = document.getElementById("tagsIntro");
            that.Interface.SuggestedTagsLabel = document.getElementById("suggestedTagsLabel");
            that.Interface.TagsArea = document.getElementById("tagsArea");
            /*that.Interface.TagsNextButton = document.getElementById("tagsNextButton");*/
            that.Interface.DescArea = document.getElementById("descArea");
            that.Interface.UsersPage = document.getElementById("usersPage");
            /*that.Interface.UsersIntro = document.getElementById("usersIntro");
            that.Interface.SelectedUsers = document.getElementById("selectedUsers");
            that.Interface.UserInput = document.getElementById("userInput");
            that.Interface.UsersArea = document.getElementById("usersArea");
            */
            that.Interface.ConfirmButton = document.getElementById("confirmButton");

            that.Interface.PageTitle.innerHTML = RVDic.RegisterNewQuestion;
            that.Interface.TitleIntro.innerHTML = RVDic.QA.NewTitleIntro;
            that.Interface.DescIntro.innerHTML = RVDic.QA.NewDescriptionIntro;
            that.Interface.TagsIntro.innerHTML = RVDic.QA.NewTagsIntro;
            that.Interface.SuggestedTagsLabel.innerHTML = RVDic.QA.SuggestedTagsLabel;
            /*that.Interface.UsersIntro.innerHTML = RVDic.QA.UsersIntro;*/

            GlobalUtilities.set_inner_title(that.Interface.TitleInput, RVDic.Title);
            /*GlobalUtilities.set_inner_title(that.Interface.UserInput, RVDic.UserSelect);*/

            that.Interface.TitleNextButton.innerHTML = RVDic.Next;
            that.Interface.DescNextButton.innerHTML = RVDic.Next;
            /*that.Interface.TagsNextButton.innerHTML = RVDic.Next;*/

            that.Interface.ConfirmButton.innerHTML = "<i class='fa fa-save fa-lg' aria-hidden='true' " +
                "style='margin-" + RV_RevFloat + ":0.5rem;'></i>" + RVDic.SaveN.replace("[n]", RVDic.Question);

            that.create_tabs();

            GlobalUtilities.set_onchangeorenter(that.Interface.TitleInput, function () {
                var searchText = GlobalUtilities.trim(that.Interface.TitleInput.value);

                jQuery(that.Interface.TitleNextButton)[searchText ? "fadeIn" : "fadeOut"](500);
                jQuery(that.Interface.ConfirmButton)[searchText && that.Objects.AllPagesSeen ? "fadeIn" : "fadeOut"](500);

                jQuery(that.Interface.Questions).fadeOut(500, function () {
                    that.Interface.Questions.innerHTML = "";

                    if (!searchText) return;

                    QAAPI.GetQuestions({
                        SearchText: Base64.encode(searchText), StartWithSearch: false,
                        Count: 5, LowerBoundary: 0, ParseResults: true,
                        ResponseHandler: function (result) {
                            var questions = (result || {}).Questions || [];

                            var _div = GlobalUtilities.create_nested_elements([
                                {
                                    Type: "div", Class: "small-12 medium-12 large-12 rv-dark-gray",
                                    Style: "padding:0rem 0.5rem; font-weight:bold; font-size:1.5rem;",
                                    Childs: [{ Type: "text", TextValue: RVDic.QA.NewSimilarQuestions }]
                                },
                                { Type: "hr", Class: "rv-light-gray" },
                                {
                                    Type: "div", Class: "small-12 medium-12 large-12",
                                    Style: "margin-top:1rem;", Name: "_div"
                                }
                            ], that.Interface.Questions)["_div"];

                            for (var i = 0, lnt = questions.length; i < lnt; ++i)
                                new QuestionMini(_div, result.Questions[i], { HideSender: true });

                            if (questions.length > 0) jQuery(that.Interface.Questions).fadeIn(500);
                        }
                    });
                });
            });

            GlobalUtilities.append_rich_text_editor(that.Interface.DescArea, null, function (editor) {
                that.Interface.DescInput = editor;
            });

            var searchText = "";

            /*
            GlobalUtilities.set_onchangeorenter(that.Interface.UserInput, function () {
                searchText = GlobalUtilities.trim(that.Interface.UserInput.value);

                if (that.Objects.UsersList) {
                    that.Objects.UsersList.clear();
                    that.Objects.UsersList.data_request();
                }
            });

            that.Objects.UsersList = new NewSimpleListViewer(that.Interface.UsersArea, {
                AutoGrow: false,
                Options: {
                    InnerWidthOffset: 0, Width: null,
                    OnDataRequest: function (options, done) {
                        if (!searchText) return done([]);

                        UsersAPI.GetUsers(GlobalUtilities.extend(options || {}, {
                            SearchText: Base64.encode(searchText), ParseResults: true,
                            ResponseHandler: function (result) {
                                done(result.Users || []);
                            }
                        }));
                    },
                    ItemBuilder: function (container, item, params) {
                        that.create_user(container, item, {
                            OnClick: function () {
                                that.create_user(that.Interface.SelectedUsers, item, { Removable: true });
                            }
                        });
                        params.OnAfterAdd();
                    },
                    MoreButtonBuilder: function (container) {
                        return GlobalUtilities.create_nested_elements([
                            {
                                Type: "div", Class: "SoftBorder BorderRadius3", Name: "moreButton",
                                Style: "text-align:center; font-weight:bold; color:gray; margin:8px auto 0px auto;" +
                                    "cursor:pointer; border-style:dashed; width:600px; font-size:medium;",
                                Properties: [
                                    { Name: "onmouseover", Value: function () { this.style.color = "black"; } },
                                    { Name: "onmouseout", Value: function () { this.style.color = "gray"; } }
                                ]
                            }
                        ], container)["moreButton"];
                    }
                }
            });
            */

            var saving = false;

            that.Interface.ConfirmButton.onclick = function () {
                if (saving) return;

                var title = GlobalUtilities.trim(that.Interface.TitleInput.value);
                var description = !that.Interface.DescInput ? "" : that.Interface.DescInput.get_data();
                var nodeIds = [];
                var userIds = [];

                var tags = !that.Objects.QATags ? [] : that.Objects.QATags.get_selected_tags();
                for (var i = 0, lnt = tags.length; i < lnt; ++i)
                    nodeIds.push(tags[i].NodeID);

                /*
                var firstChild = that.Interface.SelectedUsers.firstChild;
                while (firstChild) {
                    if ((firstChild.UserInfo || {}).UserID) userIds.push(firstChild.UserInfo.UserID);
                    firstChild = firstChild.nextSibling;
                }
                */

                saving = true;

                QAAPI.AddQuestion({
                    Title: Base64.encode(title), Description: Base64.encode(description),
                    NodeIDs: nodeIds.join('|'), UserIDs: userIds.join('|'), ParseResults: true,
                    ResponseHandler: function (result) {
                        if (result.ErrorText) {
                            alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                            saving = false;
                        }
                        else if (result.Succeed) {
                            alert(RVDic.MSG[result.Succeed] || result.Succeed);

                            return setTimeout(function () {
                                window.location.href = window.location.href;
                            }, 4000);
                        }
                    }
                });
            };
        },

        create_tabs: function () {
            var that = this;

            var _create_button = function (container, page) {
                return GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Class: "small-12 medium-4 large-4", Style: "padding:0.2rem;",
                        Childs: [
                            {
                                Type: "div", Name: "_button",
                                Style: GlobalUtilities.border_radius("0.2rem"),
                                Childs: [{ Type: "text", TextValue: page.Title }]
                            }
                        ]
                    }
                ], container)["_button"];
            };

            var tabs = [];

            var titleInited = false;
            var tagsInited = false;
            var usersInited = false;

            tabs.push({
                Page: that.Interface.TitlePage,
                Title: "1. " + RVDic.Title,
                FixedPage: true,
                Button: _create_button,
                OnActive: function () {
                    if (!titleInited) that.Interface.TitleInput.value = "";
                    titleInited = true;

                    jQuery(that.Interface.TitleInput).focus();
                }
            });

            tabs.push({
                Page: that.Interface.DescPage,
                Title: "2. " + RVDic.Description,
                FixedPage: true,
                Button: _create_button,
                Disabled: true,
                OnActive: function () {
                    if (that.Interface.DescInput) that.Interface.DescInput.focus();
                }
            });

            tabs.push({
                Page: that.Interface.TagsPage,
                Title: "3. " + RVDic.RelatedNodes,
                FixedPage: true,
                Button: _create_button,
                Disabled: true,
                OnActive: function () {
                    if (!tagsInited) GlobalUtilities.loading(that.Interface.TagsArea);
                    tagsInited = true;

                    jQuery(that.Interface.SuggestedTagsLabel).fadeOut(0);

                    QAAPI.SearchNodes({
                        SearchText: Base64.encode(that.Interface.TitleInput.value), StartWithSearch: false,
                        ParseResults: true,
                        ResponseHandler: function (result) {
                            if (!that.Objects.QATags) {
                                that.Objects.QATags = new QATags(that.Interface.TagsArea, {
                                    Tags: [], HideSearchInput: true, Editable: true
                                });
                            }

                            that.Objects.QATags.remove_temporary();

                            var hasTemp = false;

                            for (var i = 0, lnt = ((result || {}).Tags || []).length; i < lnt; ++i)
                                hasTemp = that.Objects.QATags.add_tag(result.Tags[i], { Temp: true }) || hasTemp;

                            if (hasTemp) jQuery(that.Interface.SuggestedTagsLabel).fadeIn(500);
                        }
                    });

                    that.Objects.AllPagesSeen = true;
                    jQuery(that.Interface.ConfirmButton).fadeIn(500);
                }
            });

            /*
            tabs.push({
                Page: that.Interface.UsersPage,
                Title: "4. " + RVDic.Experts,
                FixedPage: true,
                Button: _create_button,
                Disabled: true,
                OnActive: function () {
                    if (!usersInited) that.Interface.UserInput.value = "";
                    usersInited = true;

                    jQuery(that.Interface.UserInput).focus();

                    that.Objects.AllPagesSeen = true;
                    jQuery(that.Interface.ConfirmButton).fadeIn(500);
                }
            });
            */

            var tabsManager = new TabsManager({ ContainerDiv: that.Interface.Tabs, Pages: tabs });
            tabsManager.goto_page(0);

            that.Interface.TitleNextButton.onclick = function () {
                jQuery(that.Interface.TitleNextButton).fadeOut(500, function () { this.remove(); });
                tabsManager.enable(that.Interface.DescPage);
                tabsManager.goto_page(that.Interface.DescPage);
            };

            that.Interface.DescNextButton.onclick = function () {
                jQuery(that.Interface.DescNextButton).fadeOut(500, function () { this.remove(); });
                tabsManager.enable(that.Interface.TagsPage);
                tabsManager.goto_page(that.Interface.TagsPage);
            };

            /*
            that.Interface.TagsNextButton.onclick = function () {
                jQuery(that.Interface.TagsNextButton).fadeOut(500, function () { this.remove(); });
                tabsManager.enable(that.Interface.UsersPage);
                tabsManager.goto_page(that.Interface.UsersPage);
            };
            */
        },

        create_user: function (container, user, params) {
            var that = this;

            user = user || {};
            params = params || {};

            var firstChild = container.firstChild;
            while (firstChild) {
                if ((firstChild.UserInfo || {}).UserID == user.UserID) return;
                firstChild = firstChild.nextSibling;
            }

            var removable = params.Removable === true;

            var fullname = Base64.decode(user.FirstName) + " " + Base64.decode(user.LastName);

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-6 medium-4 large-3",
                    Style: "padding:" + (removable ? "0.6" : "0.2") + "rem;", Name: "container",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12 row rv-border-radius-1 WarmBorder",
                            Style: "margin:0rem; position:relative; padding:0.4rem; height:100%;" +
                                (params.OnClick ? "cursor:pointer;" : ""),
                            Properties: !params.OnClick ? null : [
                                { Name: "onclick", Value: function () { params.OnClick(user); } }
                            ],
                            Childs: [
                                {
                                    Type: "div", Class: "rv-circle rv-icon-button SoftBorder",
                                    Style: "position:absolute; width:2rem; height:2rem; cursor:pointer;" +
                                        "top:-0.8rem; " + RV_Float + ":-0.8rem;" +
                                        "text-align:center; background-color:white;" + (removable ? "" : "display:none;"),
                                    Childs: [
                                        {
                                            Type: "i", Class: "fa fa-times fa-2x",
                                            Style: "line-height:1.8rem;",
                                            Attributes: [{ Name: "aria-hidden", Value: true }],
                                            Properties: [{ Name: "onclick", Value: function () { jQuery(elems["container"]).fadeOut(500, function () { this.remove(); }); } }]
                                        }
                                    ]
                                },
                                {
                                    Type: "div", Class: "small-6 medium-6 large-6",
                                    Childs: [
                                        {
                                            Type: "img", Class: "rv-circle SoftBorder",
                                            Style: "width:80%; border-color:rgb(200,200,200); border-width:0.15rem;",
                                            Link: RVAPI.UserPageURL({ UserID: user.UserID }),
                                            Params: { Confirmation: true },
                                            Attributes: [{ Name: "src", Value: user.ProfileImageURL || user.ImageURL }],
                                            Properties: [
                                                { Name: "onmouseover", Value: function () { this.style.borderColor = "rgb(120,120,120)"; } },
                                                { Name: "onmouseout", Value: function () { this.style.borderColor = "rgb(200,200,200)"; } }
                                            ]
                                        }
                                    ]
                                },
                                {
                                    Type: "div", Class: "small-6 meidum-6 large-6",
                                    Childs: [{ Type: "middle", Class: "TextAlign", Name: "fullname" }]
                                }
                            ]
                        }
                    ]
                }
            ], container);

            elems["fullname"].innerHTML = fullname;
            elems["container"].UserInfo = user;
        }
    }
})();