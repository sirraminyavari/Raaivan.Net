(function () {
    if (window.ReportOptions && window.ReportOptions.RV && window.ReportOptions.RV.LogsReport) return;
    window.ReportOptions = window.ReportOptions || {};
    window.ReportOptions.RV = window.ReportOptions.RV || {};

    window.ReportOptions.RV.LogsReport = function (containerDiv, params, done) {
        this.ContainerDiv = typeof (containerDiv) == "object" ? containerDiv : document.getElementById(containerDiv);
        if (!this.ContainerDiv) return;

        this.Objects = {
            BeginDate: null,
            FinishDate: null,
            NotAuthorizedCheckbox: null,
            AnonymousCheckbox: null,
            UserSelect: null,
            LevelSelect: null,
            ActionsContainer: null,
            VariableName: "LogReportTemplates" + "_" + String(window.RVGlobal.CurrentUserID).toLowerCase(),
            Templates: {}
        };

        var that = this;

        GlobalUtilities.load_files([
            "Lang/Log/fa.js",
            "SingleDataContainer/NewSingleDataContainer.js"
        ], { OnLoad: function () { that._preinit(params, done); } });
    }

    ReportOptions.RV.LogsReport.prototype = {
        _preinit: function (params, done) {
            var that = this;

            RVAPI.GetVariable({
                Name: that.Objects.VariableName, ParseResults: true,
                ResponseHandler: function (r) {
                    that.Objects.Templates = r.Value ? JSON.parse(Base64.decode(r.Value || {})) : {};
                    that._initialize(params, done);
                }
            });
        },

        _initialize: function (params, done) {
            params = params || {};
            var that = this;

            var levelOptions = [];

            var add_level_option = function (name) {
                levelOptions.push({
                    Type: "option",
                    Attributes: [{ Name: "title", Value: name }],
                    Childs: [{ Type: "text", TextValue: ((((RVDic.RPT || {}).RV || {}).LogsReport || {}).Level_Dic || {})[name] || name }]
                });
            };

            add_level_option("");
            add_level_option("Info");
            add_level_option("Warn");
            add_level_option("Error");
            add_level_option("Trace");

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12", 
                    Childs: [
                        {
                            Type: "div", Style: "display:inline-block;",
                            Childs: [{ Type: "text", TextValue: RVDic.NotAuthorizedRequest + ":" }]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin:0rem 0.5rem;",
                            Childs: [{ Type: "checkbox", Name: "notAuthorized" }]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin-" + RV_Float + ":1.5rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.Anonymous + ":" }]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin:0rem 0.5rem;",
                            Childs: [{ Type: "checkbox", Name: "anonymous" }]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin-" + RV_Float + ":2rem;",
                            Childs: [
                                {
                                    Type: "div", Style: "display:inline-block; margin-" + RV_RevFloat + ":0.5rem;",
                                    Childs: [{ Type: "text", TextValue: RVDic.UserSelect + ":" }]
                                },
                                { Type: "div", Style: "display:inline-block; width:15rem;", Name: "userSelect" }
                            ]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin-" + RV_Float + ":2rem;",
                            Childs: [
                                {
                                    Type: "div", Style: "display:inline-block; margin-" + RV_RevFloat + ":0.5rem;",
                                    Childs: [{ Type: "text", TextValue: RVDic.LevelSelect + ":" }]
                                },
                                {
                                    Type: "select", Class: "rv-input", Name: "levelSelect",
                                    Style: "display:inline-block; width:15rem; font-size:0.7rem;",
                                    Childs: levelOptions
                                }
                            ]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12", Name: "logActionsContainer",
                            Style: "display:none; margin:1rem 0rem; position:relative; padding-" + RV_Float + ":10rem;",
                            Childs: [
                                {
                                    Type: "div", Style: "position:absolute; top:0rem;" + RV_Float + ":0rem;",
                                    Childs: [{ Type: "text", TextValue: RVDic.LimitActionsTo + ":" }]
                                },
                                { Type: "div", Class: "small-12 medium-12 large-12", Name: "logActions" }
                            ]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin-" + RV_RevFloat + ":2rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.ActionDate + ":" }]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin-" + RV_RevFloat + ":0.5rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.From + ":" }]
                        },
                        { Type: "div", Style: "display:inline-block;", Name: "beginDate" },
                        {
                            Type: "div", Style: "display:inline-block; margin:0rem 1.5rem; margin-" + RV_RevFloat + ":0.5rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.To + ":" }]
                        },
                        { Type: "div", Style: "display:inline-block;", Name: "finishDate" }
                    ]
                }
            ], that.ContainerDiv);

            that.Objects.UserSelect = GlobalUtilities.append_autosuggest(elems["userSelect"], {
                InputClass: "rv-input",
                InputStyle: "width:100%; font-size:0.7rem;",
                AjaxDataSource: UsersAPI.GetUsersDataSource(),
                ResponseParser: function (responseText) {
                    var users = JSON.parse(responseText).Users || [];
                    var arr = [];
                    for (var i = 0, lnt = users.length; i < lnt; ++i) {
                        var fullname = Base64.decode(users[i].FirstName) + " " + Base64.decode(users[i].LastName);
                        arr.push([fullname + " - " + Base64.decode(users[i].UserName), users[i].UserID]);
                    }
                    return arr;
                }
            });

            elems["logActionsContainer"].style.display = "block";
            GlobalUtilities.loading(elems["logActions"]);

            var savingTemplate = false;
            var locked = false;

            var save_template = function (items) {
                if (savingTemplate || (items.length == 0)) return;
                savingTemplate = true;

                var ids = [];
                for (var i = 0, lnt = items.length; i < lnt; ++i) ids.push(items[i].ID);

                var do_save = function (name) {
                    locked = true;

                    that.Objects.Templates[name] = ids.join(",");

                    RVAPI.SetVariable({
                        Name: that.Objects.VariableName, Value: Base64.encode(JSON.stringify(that.Objects.Templates)),
                        ResponseHandler: function (r) {
                            locked = savingTemplate = false;
                            showed.Close();
                        }
                    });
                };

                var similarity_check = function (name) {
                    var strIds = ids.sort().join(",").toLowerCase();
                    for (var nm in that.Objects.Templates) {
                        var nmIds = String(that.Objects.Templates[nm]).split(",").sort().join(",").toLowerCase();

                        if ((name != nm) && (strIds == nmIds)) {
                            return GlobalUtilities.confirm("a similar template already exists. do you want to continue?", function (r) {
                                if (r) do_save(name);
                                else jQuery(xEl["_input"]).focus();
                            });
                        }
                    }

                    do_save(name);
                };

                var check_template = function (name) {
                    if (!that.Objects.Templates[name]) similarity_check(name);
                    else {
                        GlobalUtilities.confirm("this name already exists. do you want to continue?", function (r) {
                            if (r) similarity_check(name);
                            else jQuery(xEl["_input"]).focus();
                        });
                    }
                }
                
                var xEl = GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Class: "small-10 medium-8 large-6 rv-border-radius-1 SoftBackgroundColor", 
                        Style: "margin:0rem auto; padding:1rem;", Name: "container",
                        Childs: [
                            {
                                Type: "div", Class: "small-12 medium-12 large-12",
                                Childs: [
                                    {
                                        Type: "input", Class: "rv-input", Style: "width:100%;", Name: "_input",
                                        Attributes: [{ Name: "placeholder", Value: RVDic.Title }]
                                    }
                                ]
                            },
                            {
                                Type: "div", Class: "ActionButton",
                                Style: "width:7rem; margin:0.5rem auto; margin-bottom:0rem;",
                                Properties: [{ Name: "onclick", Value: function () { start_saving(); } }],
                                Childs: [{ Type: "text", TextValue: RVDic.Confirm }]
                            }
                        ]
                    }
                ]);

                var start_saving = function () {
                    if (locked) return;
                    var name = GlobalUtilities.trim(xEl["_input"].value);
                    if (name) check_template(Base64.encode(name));
                };

                //GlobalUtilities.set_onenter(xEl["_input"], function () { start_saving(); });

                var showed = GlobalUtilities.show(xEl["container"], {
                    OnClose: function () { if (!locked) savingTemplate = false; },
                    OnShow: function () { jQuery(xEl["_input"]).focus(); }
                });
            }

            var show_templates = function () {
                var add_template = function (name) {
                    var templateItems = that.Objects.Templates[name].split(",");

                    var tiEl = GlobalUtilities.create_nested_elements([
                        {
                            Type: "div", Style: "cursor:pointer; color:gray; position:relative;", Name: "container",
                            Properties: [
                                { Name: "onmouseover", Value: function () { this.style.backgroundColor = "white"; this.style.color = "black"; } },
                                { Name: "onmouseout", Value: function () { this.style.backgroundColor = "transparent"; this.style.color = "gray"; } },
                                {
                                    Name: "onclick",
                                    Value: function () {
                                        showed.Close();

                                        that.Objects.ActionsContainer.clear({ SelectedOnly: true });

                                        for (var i = 0, lnt = templateItems.length; i < lnt; ++i) {
                                            var ttl = ((window.RaaiVanDic || {}).Log || {})[templateItems[i]] || templateItems[i];
                                            that.Objects.ActionsContainer.add_item(ttl, templateItems[i]);
                                        }
                                    }
                                }
                            ],
                            Childs: [
                                {
                                    Type: "div", Style: "font-weight:bold; text-align:center; margin-bottom:8px;",
                                    Childs: [{ Type: "text", TextValue: Base64.decode(name) }]
                                },
                                {
                                    Type: "div", Name: "tItems",
                                    Style: "color:rgb(60,60,60); text-align:center;" +
                                        "margin-bottom:12px; font-size:0.7rem;"
                                },
                                { Type: "hr" },
                                {
                                    Type: "span", Class: "ActionButton",
                                    Style: "position:absolute; padding-top:0px; padding-bottom:0px;" +
                                        "top:4px; " + window.RV_RevFloat + ":4px;",
                                    Properties: [
                                        {
                                            Name: "onclick",
                                            Value: function (e) {
                                                e.stopPropagation();

                                                that.Objects.Templates[name] = undefined;
                                                that.Objects.Templates = GlobalUtilities.extend({}, that.Objects.Templates);

                                                RVAPI.SetVariable({
                                                    Name: that.Objects.VariableName, Value: Base64.encode(JSON.stringify(that.Objects.Templates)),
                                                    ResponseHandler: function (r) { }
                                                });

                                                jQuery(tiEl["container"]).animate({ height: "toggle" }, 500, function () { this.remove(); });
                                            }
                                        }
                                    ],
                                    Childs: [{ Type: "text", TextValue: RVDic.Remove }]
                                }
                            ]
                        }
                    ], tEl["items"]);

                    for (var i = 0, lnt = templateItems.length; i < lnt; ++i) {
                        var ttl = ((window.RaaiVanDic || {}).Log || {})[templateItems[i]] || templateItems[i];

                        GlobalUtilities.create_nested_elements([
                            {
                                Type: "span", Class: "BorderRadius3 NormalPadding",
                                Style: "margin:4px; background-color:rgb(220,220,220);",
                                Childs: [{ Type: "text", TextValue: ttl }]
                            }
                        ], tiEl["tItems"]);
                    }
                }

                var tEl = GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Class: "BorderRadius4 SoftBackgroundColor NormalPadding", Name: "container",
                        Style: "width:700px; margin:0px auto;",
                        Childs: [{Type: "div", Name: "items"}]
                    }
                ]);

                var hasTemplate = false;
                for (var name in that.Objects.Templates) {
                    if (that.Objects.Templates[name]) {
                        hasTemplate = true;
                        add_template(name);
                    }
                }

                if (!hasTemplate) {
                    GlobalUtilities.create_nested_elements([
                        {
                            Type: "div", Class: "small-12 medium-12 large-12", Style: "text-align:center;",
                            Childs: [{ Type: "text", TextValue: RVDic.NoTemplateExists }]
                        }
                    ], tEl["items"]);
                }

                var showed = GlobalUtilities.show(tEl["container"]);
            }

            RVAPI.GetLogActions({
                ParseResults: true,
                ResponseHandler: function (arr) {
                    if (GlobalUtilities.get_type(arr) != "array") return;

                    var ds = [];
                    for (var i = 0, lnt = arr.length; i < lnt; ++i) {
                        var title = ((window.RaaiVanDic || {}).Log || {})[arr[i]] || arr[i];
                        ds.push([title, arr[i]]);
                    }

                    that.Objects.ActionsContainer = new NewSingleDataContainer(elems["logActions"], {
                        InputClass: "rv-input",
                        InputStyle: "width:100%; font-size:0.7rem;",
                        ArrayDataSource: ds,
                        OnLoad: function () {
                            var bObj = this;

                            var buttonsArea = bObj.buttons_area();
                            if (!buttonsArea) return;

                            GlobalUtilities.create_nested_elements([
                                {
                                    Type: "div", Style: "display:inline-block; padding:0rem 1rem;",
                                    Childs: [
                                        {
                                            Type: "div", Class: "ActionButton",
                                            Style: "display:inline-block; margin-" + RV_RevFloat + ":0.5rem;",
                                            Properties: [{ Name: "onclick", Value: function () { save_template(bObj.get_items());  }}],
                                            Childs: [{ Type: "text", TextValue: RVDic.Save }]
                                        },
                                        {
                                            Type: "div", Class: "ActionButton",
                                            Style: "display:inline-block;",
                                            Properties: [{ Name: "onclick", Value: function () { show_templates(); } }],
                                            Childs: [{ Type: "text", TextValue: RVDic.Templates }]
                                        }
                                    ]
                                }
                            ], buttonsArea);
                        }
                    });
                }
            });

            GlobalUtilities.append_calendar(elems["beginDate"], { ClearButton: true }, function (cal) {
                that.Objects.BeginDate = cal;
            });

            GlobalUtilities.append_calendar(elems["finishDate"], { ClearButton: true }, function (cal) {
                that.Objects.FinishDate = cal;
            });

            that.Objects.NotAuthorizedCheckbox = elems["notAuthorized"];
            that.Objects.AnonymousCheckbox = elems["anonymous"];

            that.Objects.LevelSelect = elems["levelSelect"];
            
            that.set_data(params);

            done();
        },

        set_data: function (params) {
            var that = this;
            params = params || {};

            if (params.BeginDate && that.Objects.BeginDate)
                that.Objects.BeginDate.Set({ Value: params.BeginDate.Value, Label: params.BeginDate.Title });

            if (params.FinishDate && that.Objects.FinishDate)
                that.Objects.FinishDate.Set({ Value: params.FinishDate.Value, Label: params.FinishDate.Title });

            if (params.NotAuthorized) that.Objects.NotAuthorizedCheckbox.check();
            if (params.Anonymous) that.Objects.AnonymousCheckbox.check();

            if (params.UserID) this.Objects.UserSelect.set_item(params.UserID.Value || "", params.UserID.Title || "");

            if (params.Level) {
                for (var i = 0, lnt = that.Objects.LevelSelect.length; i < lnt; ++i)
                    if (that.Objects.LevelSelect[i].title == params.Level) that.Objects.LevelSelect.selectedIndex = i;
            }

            if ((GlobalUtilities.get_type(params.Actions) == "array") && that.Objects.ActionsContainer) {
                for (var i = 0, lnt = params.Actions.length; i < lnt; ++i)
                    that.Objects.ActionsContainer.add_item(params.Actions[i], params.Actions[i]);
            }
        },

        get_data: function () {
            var that = this;

            var index = !this.Objects.UserSelect ? -1 : this.Objects.UserSelect.selectedIndex;
            var userId = index < 0 ? "" : this.Objects.UserSelect.values[index];
            var fullname = GlobalUtilities.trim(index < 0 ? "" : this.Objects.UserSelect.keywords[index]);

            if (userId && (GlobalUtilities.trim(this.Objects.UserSelect.InputElement.value) != fullname))
                userId = fullname = "";

            var beginDate = (that.Objects.BeginDate || { Get: function () { return {} } }).Get();
            var finishDate = (that.Objects.FinishDate || { Get: function () { return {} } }).Get();

            var actions = !that.Objects.ActionsContainer ? [] : that.Objects.ActionsContainer.get_items();

            return {
                UserID: userId, _Title_UserID: fullname,
                Actions: Base64.encode("StringTableType:" + actions.length + ":Value|String" +
                    (actions.length > 0 ? "|" + that.Objects.ActionsContainer.get_items_string(":") : "")),
                Level: that.Objects.LevelSelect[that.Objects.LevelSelect.selectedIndex].title || "",
                NotAuthorized: that.Objects.NotAuthorizedCheckbox.checked,
                Anonymous: that.Objects.AnonymousCheckbox.checked,
                BeginDate: beginDate.Value || "",
                _Title_BeginDate: beginDate.Label || "",
                FinishDate: finishDate.Value || "",
                _Title_FinishDate: finishDate.Label || ""
            };
        },

        clear: function () {
            var that = this;

            if (this.Objects.BeginDate) this.Objects.BeginDate.Clear();
            if (this.Objects.FinishDate) this.Objects.FinishDate.Clear();
            this.Objects.NotAuthorizedCheckbox.uncheck();
            this.Objects.AnonymousCheckbox.uncheck();
            if (this.Objects.UserSelect) this.Objects.UserSelect.empty();
            this.Objects.LevelSelect.selectedIndex = 0;
            if (this.Objects.ActionsContainer) this.Objects.ActionsContainer.clear();
        }
    }
})();