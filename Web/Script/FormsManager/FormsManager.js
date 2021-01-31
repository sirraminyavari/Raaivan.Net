(function () {
    if (window.FormsManager) return;

    window.FormsManager = function (container, params) {
        this.Container = typeof (container) == "object" ? container : document.getElementById(container);
        if (!this.Container) return;
        params = params || {};

        this.Interface = {
            Header: null,
            Forms: null,
            FormDetail: null,
            ArchiveCheckbox: null
        };

        var that = this;

        GlobalUtilities.load_files(["API/FGAPI.js", "Public/NameDialog.js"], {
            OnLoad: function () { that.initialize(); }
        });
    };

    FormsManager.prototype = {
        initialize: function () {
            var that = this;

            that.Container.innerHTML = "";

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12", Name: "header",
                    Style: "position:relative; padding-" + RV_RevFloat + ":10rem;",
                    Childs: [
                        {
                            Type: "div", Class: "RevTextAlign RevDirection",
                            Style: "position:absolute; top:0; bottom:0;" + RV_RevFloat + ":1rem;",
                            Childs: [
                                {
                                    Type: "middle", Class: "TextAlign Direction",
                                    Style: "display:inline-block; margin-bottom:0.5rem;",
                                    Childs: [
                                        {
                                            Type: "checkbox", Name: "archiveChb", Params: { OnChange: function () { that.show_forms(this.Checked); } },
                                            Style: "width:1rem; height:1rem; cursor:pointer; margin-" + RV_RevFloat + ":0.5rem;"
                                        },
                                        { Type: "text", TextValue: RVDic.ShowRemovedForms }
                                    ]
                                }
                            ]
                        },
                        {
                            Type: "div",
                            Style: "display:flex; flex-flow:row; align-items:center; margin-bottom:1rem;",
                            Childs: [
                                {
                                    Type: "div",
                                    Style: "flex:0 0 auto; display:flex; flex-flow:row; align-items:center;" +
                                        "font-size:1.2rem; color:rgb(100,100,100); font-weight:bold;",
                                    Childs: [
                                        { Type: "text", TextValue: RVDic.Forms },
                                        { Type: "help", Style: "margin-" + RV_Float + ":0.5rem;", Params: { Name: "systemsettings_forms" } }
                                    ]
                                },
                                {
                                    Type: "div", Style: "flex:0 0 auto; padding-" + RV_Float + ":5rem; width:20rem;",
                                    Childs: [{
                                        Type: "input", Class: "rv-input", Name: "searchInput",
                                        Style: "width:100%; font-size:0.7rem;", Placeholder: RVDic.Search,
                                    }]
                                }
                            ]
                        }
                    ]
                },
                { Type: "div", Class: "small-12 medium-12 large-12  row", Name: "forms", Style: "margin:0rem;" },
                {Type: "div", Class: "small-12 medium-12 large-12", Name: "formDetail"}
            ], that.Container);

            that.Interface.Header = elems["header"];
            that.Interface.Forms = elems["forms"];
            that.Interface.FormDetail = elems["formDetail"];

            GlobalUtilities.set_onchangeorenter(elems["searchInput"], function () {
                var txt = GlobalUtilities.trim(elems["searchInput"].value);

                [].forEach.call(elems["forms"].childNodes, nd => {
                    var show = GlobalUtilities.is_search_match(Base64.decode((nd.ItemObject || {}).Title), txt);
                    jQuery(nd)[show ? "fadeIn" : "fadeOut"]();
                });
            }, { Timeout: 10 });

            that.show_forms(false);
        },

        show_forms: function (archive) {
            var that = this;

            that.Interface.Forms.innerHTML = "";
            GlobalUtilities.loading(that.Interface.Forms);

            FGAPI.GetForms({
                Archive: archive, Count: 1000000, ParseResults: true,
                ResponseHandler: function (result) {
                    that.Interface.Forms.innerHTML = "";

                    if (!archive) that.new_item(that.Interface.Forms);
                    else if (!((result || {}).Forms || []).length) {
                        GlobalUtilities.create_nested_elements([{
                            Type: "div", Style: "text-align:center; font-size:1.2rem; padding-top:2rem; color:rgb(100,100,100);",
                            Childs: [{ Type: "text", TextValue: RVDic.NothingToDisplay }]
                        }], that.Interface.Forms);
                    }

                    jQuery.each((result || {}).Forms || [], function (ind, val) {
                        that.add_item(that.Interface.Forms, val, false, archive);
                    });
                }
            });
        },

        new_item: function (container) {
            var that = this;

            var btn = GlobalUtilities.create_nested_elements([{
                Type: "div", Class: "small-12 medium-6 large-4", Style: "padding:0.5rem;",
                Childs: [{
                    Type: "div", Style: "height:100%; font-size:1.2rem;", Name: "btn",
                    Class: "small-12 medium-12 large-12 rv-border-radius-half rv-air-button",
                    Childs: [{
                        Type: "middle", Style: "display:inline-block;",
                        Childs: [
                            {
                                Type: "i", Class: "fa fa-plus-circle fa-lg",
                                Style: "margin-" + RV_RevFloat + ":0.5rem;",
                                Attributes: [{ Name: "aria-hidden", Value: true }]
                            },
                            { Type: "text", TextValue: RVDic.NewN.replace("[n]", RVDic.Form) }
                        ]
                    }]
                }]
            }], container)["btn"];

            var saving = false;

            btn.onclick = function () {
                if (saving) return;
                saving = true;

                new NameDialog({
                    Title: RVDic.Name, InnerTitle: RVDic.Name,
                    OnActionCall: function (name, callback) {
                        if (!name) return callback(!(saving = false));

                        FGAPI.CreateForm({
                            Title: Base64.encode(name), ParseResults: true,
                            ResponseHandler: function (result) {
                                if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                                else if (result.Form) that.add_item(container, result.Form, true, false);

                                callback(!!(result || {}).Form);

                                saving = false;
                            }
                        });
                    }
                });
            };
        },

        add_item: function (container, item, add2top, archive) {
            var that = this;

            var id = item.ID;

            var action_button = function (p, size) {
                p = p || {};

                return {
                    Type: "div", Style: "padding:0rem 0.25rem;",
                    Class: "small-" + size + " medium-" + size + " large-" + size, 
                    Childs: [{
                        Type: "div", Class: "small-12 medium-12 large-12 rv-air-button rv-circle", Name: p.Name,
                        Childs: [
                            {
                                Type: "i", Class: "fa " + p.Icon, Style: "margin-" + RV_RevFloat + ":0.4rem;",
                                Attributes: [{ Name: "aria-hidden", Value: true }]
                            },
                            { Type: "text", TextValue: p.Title }
                        ]
                    }]
                };
            };
            
            var create_buttons = function (btns) {
                btns = btns.filter(function (val) { return !!val; });
                var size = btns.length == 2 ? "6" : (btns.length == 3 ? "4" : "12");
                return btns.map(function (val) { return action_button(val, size); });
            };

            var elems = GlobalUtilities.create_nested_elements([{
                Type: "div", Class: "small-12 medium-6 large-4",
                Style: "padding:0.5rem; opacity:0;", Name: "container",
                Childs: [{
                    Type: "div",
                    Class: "small-12 medium-12 large-12 rv-border-radius-half rv-bg-color-softer-soft",
                    Style: "position:relative; height:100%; padding:0.4rem; padding-bottom:3.5rem; text-align:center;",
                    Childs: [
                        {
                            Type: "div", Class: "rv-border-radius-quarter",
                            Style: "position:absolute; bottom:0.5rem; left:0.5rem; right:0.5rem;" +
                                "padding:0.3rem; background-color:white;",
                            Childs: [{
                                Type: "div", Class: "small-12 medium-12 large-12 row", Style: "margin:0rem;",
                                Childs: create_buttons([
                                    { Name: "rename", Title: RVDic.Rename, Icon: "fa-i-cursor" },
                                    (archive ? null : { Name: "edit", Title: RVDic.Edit, Icon: "fa-pencil" }),
                                    {
                                        Name: "remove", Title: archive ? RVDic.Recycle : RVDic.Remove,
                                        Icon: archive ? "fa-recycle" : "fa-times"
                                    }
                                ])
                            }]
                        },
                        {
                            Type: "middle", Style: "display:inline-block; font-size:1rem;",
                            Childs: [{ Type: "text", TextValue: Base64.decode(item.Title), Name: "formName" }]
                        }
                    ]
                }]
            }]);
            
            if (!add2top) container.appendChild(elems["container"]);
            else container.insertBefore(elems["container"], (container.firstChild || {}).nextSibling);

            jQuery(elems["container"]).animate({ opacity: 1 }, 500);

            elems["container"].ItemObject = item;

            elems["rename"].onclick = function () {
                that.rename(item, function (name) {
                    item.Title = Base64.encode(name);
                    elems["formName"].data = GlobalUtilities.convert_numbers_to_persian(name);
                });
            };

            if (elems["edit"]) elems["edit"].onclick = function () { that.edit(item); }
            
            elems["remove"].onclick = function () {
                that.remove_recycle(item, archive, function () {
                    jQuery(elems["container"]).fadeOut(500, function () { jQuery(elems["container"]).remove(); });
                });
            };
        },

        rename: function (item, done) {
            var that = this;

            if (that.SavingName) return;
            that.SavingName = true;
            
            new NameDialog({
                Title: RVDic.NewN.replace("[n]", RVDic.Name), InitialValue: Base64.decode(item.Title), InnerTitle: RVDic.Name,
                OnActionCall: function (name, callback) {
                    if (!name) return callback(!(that.SavingName = false));

                    FGAPI.SetFormTitle({
                        FormID: item.FormID, Title: Base64.encode(name), ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                            else if (result.Succeed) done(name);

                            callback(!!(result || {}).Succeed);

                            that.SavingName = false;
                        }
                    });
                }
            });
        },

        edit: function (item) {
            var that = this;

            that.__FormDetails = that.__FormDetails || [];

            if (that.__FormDetails[item.FormID]) return that.show_edit_panel(that.__FormDetails[item.FormID]);

            var elems = GlobalUtilities.create_nested_elements([{
                Type: "div", Class: "small-12 medium-12 large-12", Name: "container"
            }]);

            that.__FormDetails[item.FormID] = elems["container"];

            GlobalUtilities.loading(elems["container"]);

            that.show_edit_panel(that.__FormDetails[item.FormID]);

            GlobalUtilities.load_files(["FormsManager/FormElementsManager.js"], {
                OnLoad: function () {
                    new FormElementsManager(elems["container"], {
                        FormID: item.FormID,
                        Form: item,
                        TopMargin: 6,
                        OnReturn: function (p) {
                            if ((p || {}).Destroy) that.__FormDetails[item.FormID] = null;
                            that.hide_edit_panel();
                        }
                    });
                }
            });
        },

        show_edit_panel: function (container) {
            var that = this;
            
            jQuery(that.Interface.Header).fadeOut(200);

            jQuery(that.Interface.Forms).fadeOut(200, () => {
                that.Interface.FormDetail.innerHTML = "";
                that.Interface.FormDetail.appendChild(container);
                jQuery(that.Interface.FormDetail).fadeIn(500);
            });
        },

        hide_edit_panel: function () {
            var that = this;

            jQuery(that.Interface.FormDetail).fadeOut(200, () => {
                jQuery(that.Interface.Header).fadeIn(500);
                jQuery(that.Interface.Forms).fadeIn(500);
            });
        },

        remove_recycle: function (item, recycle, done) {
            var that = this;

            if (that.Removing) return;
            that.Removing = true;

            var msg = RVDic.Confirms[recycle ? "DoYouWantToRecycleTheN" : "DoYouWantToRemoveN"]
                .replace("[n]", "'" + Base64.decode(item.Title) + "'");

            GlobalUtilities.confirm(msg, function (r) {
                if (!r) return (that.Removing = false);

                FGAPI[recycle ? "RecycleForm" : "RemoveForm"]({
                    FormID: item.FormID, ParseResults: true,
                    ResponseHandler: function (result) {
                        if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                        else if (result.Succeed) done();

                        that.Removing = false;
                    }
                });
            });
        }
    };
})();