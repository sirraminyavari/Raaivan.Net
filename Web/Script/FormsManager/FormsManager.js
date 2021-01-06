(function () {
    if (window.FormsManager) return;

    window.FormsManager = function (containerDiv, params) {
        this.ContainerDiv = typeof (containerDiv) == "object" ? containerDiv : document.getElementById(containerDiv);
        if (this.ContainerDiv == null) return;

        params = params || {};

        this.Interface = {
            SearchInput: null,
            NewButton: null,
            ItemsArea: null,
            CommonPage: null,
            ShowRemovedButton: null,
            ShowExistingButton: null
        };

        this.Objects = {
            Items: {},
            LastSelectedItemID: null
        };

        var that = this;

        GlobalUtilities.loading(that.ContainerDiv);

        GlobalUtilities.load_files(["API/FGAPI.js", "FormsManager/FormElementsManager.js", "Public/NameDialog.js"], {
            OnLoad: function () { that._initialize(); }
        });
    }
    
    FormsManager.prototype = {
        _initialize: function () {
            var that = this;

            that.ContainerDiv.innerHTML = "";

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-4 large-3",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "display:flex; flex-flow:row; align-items:center; font-weight:bold;" +
                                "margin-bottom:1rem; padding:0 0.5rem; font-size:1.2rem; color:rgb(100,100,100);",
                            Childs: [
                                { Type: "text", TextValue: RVDic.Forms },
                                { Type: "help", Style: "margin-" + RV_Float + ":0.5rem;", Params: { Name: "systemsettings_forms" } }
                            ]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12 rv-air-button rv-border-radius-half",
                            Style: "text-align:center; margin-bottom:0.6rem;", Name: "removedButton",
                            Properties: [{ Name: "onclick", Value: function () { that.show_items({ Archive: true }); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.ShowRemovedForms }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12 rv-air-button rv-border-radius-half",
                            Style: "text-align:center; margin-bottom:0.6rem; display:none;", Name: "existingButton",
                            Properties: [{ Name: "onclick", Value: function () { that.show_items({ Archive: false }); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.ShowExistingForms }]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "position:relative; padding-" + RV_RevFloat + ":6rem;",
                            Childs: [
                                {
                                    Type: "div", Class: "rv-air-button rv-border-radius-quarter", Name: "newForm",
                                    Style: "position:absolute; top:0; bottom:0;" + RV_RevFloat + ":0; width:5.5rem;",
                                    Childs: [{ Type: "text", TextValue: "+ " + RVDic.NewN.replace("[n]", RVDic.Form) }]
                                },
                                { Type: "input", Class: "rv-input", Style: "width:100%;", Name: "searchInput", InnerTitle: RVDic.Search }
                            ]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "margin-top:0.5rem;", Name: "itemsArea"
                        }
                    ]
                },
                {
                    Type: "div", Class: "small-12 medium-8 large-9", Name: "commonPage",
                    Style: "padding-" + RV_Float + ":1rem;"
                }
            ], that.ContainerDiv);

            that.Interface.ItemsArea = elems["itemsArea"];
            that.Interface.CommonPage = elems["commonPage"];
            that.Interface.ShowRemovedButton = elems["removedButton"];
            that.Interface.ShowExistingButton = elems["existingButton"];
            that.Interface.SearchInput = elems["searchInput"];
            that.Interface.NewButton = elems["newForm"];

            var saving = false;

            that.Interface.NewButton.onclick = function () {
                if (saving) return;
                saving = true;

                new NameDialog({
                    Title: RVDic.NewN.replace("[n]", RVDic.Form), InnerTitle: RVDic.NewFormName,
                    OnActionCall: function (name, callback) {
                        if (!name) return callback(!(saving = false));

                        FGAPI.CreateForm({
                            Title: Base64.encode(name), ParseResults: true,
                            ResponseHandler: function (result) {
                                if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                                else if(result.Form) that.add_item(result.Form, false, true);

                                callback(!!result.Form);

                                saving = false;
                            }
                        });
                    }
                });
            };

            GlobalUtilities.set_onchangeorenter(that.Interface.SearchInput, function () { that.search_items(); });

            that.show_items({ Archive: false });
        },

        search_items: function () {
            var that = this;

            var searchText = GlobalUtilities.trim(that.Interface.SearchInput.value);

            for (var k in that.Objects.Items)
                if ((that.Objects.Items[k] || {}).SetDisplay) that.Objects.Items[k].SetDisplay(searchText);
        },

        show_items: function (params) {
            params = params || {};
            var that = this;

            if (that.__GettingItems) return;
            that.__GettingItems = true;

            var archive = params.Archive;

            var fadeInButton = that.Interface[archive ? "ShowExistingButton" : "ShowRemovedButton"];
            var fadeOutButton = that.Interface[archive ? "ShowRemovedButton" : "ShowExistingButton"];

            jQuery(fadeOutButton).fadeOut(500, function () { jQuery(fadeInButton).fadeIn(500); });

            if (archive) {
                jQuery(that.Interface.NewButton).fadeOut(500, function () {
                    jQuery(that.Interface.NewButton.parentNode).animate({ ["padding-" +RV_RevFloat]: 0 }, 500);
                });
            }
            else {
                jQuery(that.Interface.NewButton.parentNode).animate({ ["padding-" +RV_RevFloat]: "6rem" }, 500, function () {
                    jQuery(that.Interface.NewButton).fadeIn(500);
                });
            }

            that._show_items(params, function () {
                that.__GettingItems = false;

                that.search_items();
            });
        },

        _show_items: function (params, done) {
            params = params || {};
            var that = this;

            var archive = params.Archive;

            that.Interface.CommonPage.innerHTML = "";
            that.Interface.ItemsArea.innerHTML = "";
            GlobalUtilities.loading(that.Interface.ItemsArea);

            FGAPI.GetForms({
                Archive: archive, Count: 1000000, ParseResults: true,
                ResponseHandler: function (result) {
                    var forms = result.Forms || [];

                    that.Interface.ItemsArea.innerHTML = "";

                    for (var i = 0, lnt = (forms || []).length; i < lnt; ++i)
                        that.add_item(forms[i], archive);

                    done();
                }
            });
        },

        add_item: function (params, archive, add2top) {
            params = params || {};
            var that = this;
            
            var itemId = params.FormID;
            var name = Base64.decode(params.Title);
            var description = Base64.decode(params.Description);

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Name: "itemDiv",
                    Class: "small-12 medium-12 large-12 row rv-bg-color-softer-soft rv-border-radius-quarter",
                    Style: "padding:0.2rem; margin:0.2rem 0rem; position:relative;",
                    Childs: [
                        {
                            Type: "div", Style: "position:absolute; top:0rem; bottom:0rem; " + RV_Float + ":0rem; width:1.5rem; text-align:center;",
                            Childs: [
                                {
                                    Type: "middle",
                                    Childs: [
                                        {
                                            Type: "i", Class: "fa fa-" + (archive ? "undo" : "times") + " rv-icon-button",
                                            Attributes: [{ Name: "aria-hidden", Value: true }],
                                            Properties: [{ Name: "onclick", Value: function () { that.remove_recycle(itemId, archive); } }]
                                        }
                                    ]
                                }
                            ]
                        },
                        {
                            Type: "div", Style: "position:absolute; top:0rem; bottom:0rem; " + RV_Float + ":1.5rem; width:1.5rem; text-align:center;",
                            Childs: [
                                {
                                    Type: "middle",
                                    Childs: [
                                        {
                                            Type: "i", Class: "fa fa-pencil rv-icon-button", Name: "editButton",
                                            Attributes: [{ Name: "aria-hidden", Value: true }]
                                        }
                                    ]
                                }
                            ]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "padding-" + RV_Float + ":3.2rem;",
                            Childs: [
                                {
                                    Type: "div", Name: "nameDiv", Tooltip: name,
                                    Class: "small-12 medium-12 large-12 Ellipsis", Style: "cursor:pointer;",
                                    Properties: [{ Name: "onclick", Value: function () { that.on_item_select(itemId); } }],
                                    Childs: [{ Type: "text", TextValue: name, Name: "nameText" }]
                                },
                                {
                                    Type: "div", Class: "small-12 medium-12 large-12", Style: "display:none;", Name: "editDiv",
                                    Childs: [
                                        {
                                            Type: "input", Class: "rv-input", Style: "width:100%;", Name: "nameInput",
                                            Attributes: [{ Name: "type", Value: "text" }]
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]);

            if (add2top) that.Interface.ItemsArea.insertBefore(elems["itemDiv"], that.Interface.ItemsArea.firstChild);
            else that.Interface.ItemsArea.appendChild(elems["itemDiv"]);

            var itemDiv = elems["itemDiv"];
            var editButton = elems["editButton"];
            var nameDiv = elems["nameDiv"];
            var editDiv = elems["editDiv"];
            var nameInput = elems["nameInput"];
            var nameText = elems["nameText"];

            editButton.onclick = function () {
                var set_things = function () {
                    var itm = that.Objects.Items[itemId];

                    nameText.data = itm.Title || "";
                    nameInput.value = itm.Title || "";

                    editDiv.style.display = editButton.__Editing ? "block" : "none";
                    nameDiv.style.display = editButton.__Editing ? "none" : "block";

                    GlobalUtilities.append_tooltip(nameDiv, itm.Title);

                    editButton.setAttribute("class",
                        "fa fa-" + (editButton.__Editing ? "floppy-o" : "pencil") + " rv-icon-button");
                }

                if (this.__Editing === true) {
                    var newName = GlobalUtilities.trim(nameInput.value);

                    if (!newName) return;

                    GlobalUtilities.block(nameDiv);

                    FGAPI.SetFormTitle({
                        FormID: itemId, Title: Base64.encode(newName), ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                            else {
                                that.Objects.Items[itemId].Title = newName;
                                editButton.__Editing = false;
                                set_things();
                            }

                            GlobalUtilities.unblock(nameDiv);
                        }
                    });
                }
                else this.__Editing = true;

                set_things();
            }

            that.Objects.Items[itemId] = GlobalUtilities.extend(params, {
                ContainerDiv: itemDiv,
                FormID: itemId,
                Title: name,
                Description: description,
                Settings: null,
                Selected: false,
                SetDisplay: function (searchText) {
                    var isSearched = GlobalUtilities.is_search_match(that.Objects.Items[itemId].Title, searchText);
                    jQuery(itemDiv)[isSearched ? "fadeIn" : "fadeOut"](500);
                }
            });
        },

        on_item_select: function (itemId) {
            var that = this;

            var lastSelectedItemId = that.Objects.LastSelectedItemID;
            if (lastSelectedItemId != null && that.Objects.Items[lastSelectedItemId]) {
                var _itm = that.Objects.Items[lastSelectedItemId];
                _itm.Selected = false;

                _itm.ContainerDiv.classList.remove("rv-bg-color-white-softer");
                _itm.ContainerDiv.classList.remove("SoftBorder");
                _itm.ContainerDiv.classList.add("rv-bg-color-softer-soft");
            }

            that.Objects.LastSelectedItemID = itemId;
            
            var item = that.Objects.Items[itemId];
            item.Selected = true;
            item.ContainerDiv.classList.remove("rv-bg-color-softer-soft");
            item.ContainerDiv.classList.add("rv-bg-color-white-softer");
            item.ContainerDiv.classList.add("SoftBorder");
            
            if (!item.Settings) {
                var _div = item.Settings = GlobalUtilities.create_nested_elements([{
                    Type: "div", Class: "small-12 medium-12 large-12", Name: "_div"
                }])["_div"];

                GlobalUtilities.loading(_div);

                new FormElementsManager(_div, { FormID: itemId, Form: item, TopMargin: 6 });
            }
            
            jQuery(that.Interface.CommonPage).fadeOut(500, function () {
                that.Interface.CommonPage.innerHTML = "";
                
                jQuery(that.Interface.CommonPage).fadeOut(0, function () {
                    that.Interface.CommonPage.appendChild(item.Settings);
                    jQuery(that.Interface.CommonPage).fadeIn(500);
                });
            });
        },

        remove_recycle: function (itemId, recycle) {
            var that = this;

            var msg = recycle ? RVDic.Confirms.DoYouWantToRecycleTheN.replace("[n]", RVDic.Form) :
                RVDic.Confirms.DoYouWantToRemoveN.replace("[n]", RVDic.Form);

            GlobalUtilities.confirm(msg, function (result) {
                if (!result) return;

                FGAPI[recycle ? "RecycleForm" : "RemoveForm"]({
                    FormID: itemId, ParseResults: true,
                    ResponseHandler: function (result) {
                        if (result.ErrorText) return alert(RVDic.MSG[result.ErrorText] || result.ErrorText);

                        var item = that.Objects.Items[itemId];
                        if (!item) return;

                        if (item.ContainerDiv) item.ContainerDiv.parentNode.removeChild(item.ContainerDiv);
                        that.Objects.Items[itemId] = null;
                    }
                });
            });
        }
    }
})();