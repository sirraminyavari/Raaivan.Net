﻿(function () {
    if (window.RemoteServerSettings) return;

    window.RemoteServerSettings = function (container, params) {
        this.Container = typeof (container) == "object" ? container : document.getElementById("container");
        if (!this.Container) return;
        params = params || {};

        this.Interface = {
            SearchContainer: null,
            SearchInput: null,
            Items: null
        };

        this.Options = {
            SelectMode: !!params.SelectMode,
            OnSelect: params.OnSelect
        };

        var that = this;
        
        GlobalUtilities.load_files(["API/UsersAPI.js"], {
            OnLoad: function () { that._preinit(params); }
        });
    };

    RemoteServerSettings.prototype = {
        _preinit: function (params) {
            var that = this;
            params = params || {};
            
            if (params.Servers) return that.initialize(params.Servers);

            UsersAPI.GetRemoteServers({
                ParseResults: true,
                ResponseHandler: function (result) {
                    that.initialize(result.Servers || []);
                }
            });
        },

        initialize: function (servers) {
            var that = this;

            that.Container.innerHTML = "";

            var elems = GlobalUtilities.create_nested_elements([
                (that.Options.SelectMode ? null : {
                    Type: "div", Class: "small-10 medium-8 large-6 rv-air-button rv-circle",
                    Style: "margin:0 auto 1rem auto;", Name: "newItem",
                    Childs: [{ Type: "text", TextValue: "+ " + RVDic.NewN.replace("[n]", RVDic.RemoteServer) }]
                }),
                {
                    Type: "div", Class: "small-10 medium-8 large-6", Name: "searchContainer",
                    Style: "margin:0 auto 1rem auto; display:none;",
                    Childs: [{
                        Type: "input", Class: "rv-input", Name: "searchInput",
                        Style: "width:100%; font-size:0.7rem;", InnerTitle: RVDic.Search + "..."
                    }]
                },
                { Type: "div", Class: "small-12 medium-12 large-12 rv-trim-vertical margins", Name: "items" }
            ], that.Container);

            that.Interface.SearchContainer = elems["searchContainer"];
            that.Interface.SearchInput = elems["searchInput"];
            that.Interface.Items = elems["items"];

            jQuery.each(servers || [], function (ind, val) { that.add_item(elems["items"], val); });

            if (elems["newItem"]) elems["newItem"].onclick = function () { that.new_item(elems["items"]); };

            GlobalUtilities.set_onchangeorenter(that.Interface.SearchInput, function () { that.search(); });
        },

        set_search_input_display: function () {
            var that = this;

            var itemsCount = 0;
            var firstChild = that.Interface.Items.firstChild;

            while (firstChild) {
                if (firstChild.TheObject)++itemsCount;
                firstChild = firstChild.nextSibling;
            }

            var hide = itemsCount <= 5;

            jQuery(that.Interface.SearchContainer)[hide ? "fadeOut" : "fadeIn"](500);

            if (hide) {
                that.Interface.SearchInput.value = "";
                that.search();
            }
        },

        search: function () {
            var that = this;
            
            var searchText = GlobalUtilities.trim(that.Interface.SearchInput.value).toLowerCase();
            
            var firstChild = that.Interface.Items.firstChild;

            while (firstChild) {
                if (firstChild.TheObject) {
                    var value = Base64.decode(firstChild.TheObject.Name) + " " +
                        Base64.decode(firstChild.TheObject.URL) + " " + Base64.decode(firstChild.TheObject.UserName);
                    jQuery(firstChild)[value.toLowerCase().indexOf(searchText) >= 0 ? "fadeIn" : "fadeOut"](0);
                }

                firstChild = firstChild.nextSibling;
            }
        },

        new_item: function (itemsContainer) {
            var that = this;

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-10 medium-9 large-8 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0 auto; padding:1rem;", Name: "container",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12 rv-title",
                            Childs: [{ Type: "text", TextValue: RVDic.NewN.replace("[n]", RVDic.RemoteServer) }]
                        },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "_div" }
                    ]
                }
            ], that.Container);

            var showed = GlobalUtilities.show(elems["container"]);

            that.add_item(elems["_div"], null, {
                OnSave: function (item) {
                    showed.Close();
                    that.add_item(itemsContainer, item, { Add2Top: true });
                }
            });
        },

        add_item: function (container, item, options) {
            var that = this;
            item = item || {};
            options = options || {};

            var create_view_element = function (p) {
                p = p || {};

                return {
                    Type: "div", Class: "small-12 medium-6 large-4", Style: "padding:0 0.3rem;",
                    Childs: [
                        {
                            Type: "div",
                            Style: "display:inline-block; text-transform:capitalize; font-weight:bold; color:rgb(100,100,100);",
                            Childs: [{ Type: "text", TextValue: p.Title + ":" }]
                        },
                        {
                            Type: "div", Style: "display:inline-block; margin-" + RV_Float + ":0.5rem;",
                            Childs: [{ Type: "text", TextValue: p.Value }]
                        }
                    ]
                };
            };

            var create_input = function (p) {
                p = p || {};

                return {
                    Type: "div", Class: "small-12 medium-6 large-6",
                    Style: "position:relative; padding:0.3rem; padding-" + RV_Float + ":5rem;",
                    Childs: [
                        {
                            Type: "div",
                            Style: "position:absolute; top:0.3rem;" + RV_Float + ":0.3rem; text-transform:capitalize;",
                            Childs: [{ Type: "text", TextValue: p.Title + ":" }]
                        },
                        {
                            Type: "input", Class: "rv-input", InnerTitle: p.Title,
                            Style: "width:100%; font-size:0.7rem;", Name: p.Name,
                            Attributes: [{ Name: "type", Value: p.Type || "text" }]
                        }
                    ]
                };
            };

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Name: "container",
                    Class: "small-12 medium-12 large-12 " +
                        (item.ID ? "rv-border-radius-half rv-bg-color-white-softer SoftShadow" : ""),
                    Style: "position:relative; padding:0.5rem; margin-top:0.5rem;" + 
                        (item.ID && !that.Options.SelectMode ? "padding-" + RV_Float + ":4rem;" : "") +
                        (item.ID ? "padding-" + RV_RevFloat + ":2rem;" : "") +
                        (that.Options.SelectMode ? "cursor:pointer" : ""),
                    Properties: !that.Options.SelectMode || !that.Options.OnSelect ? null : [
                        { Name: "onclick", Value: function () { that.Options.OnSelect(item); }}
                    ],
                    Childs: [
                        (!item.ID || that.Options.SelectMode ? null : {
                            Type: "div", Style: "position:absolute; top:0.5rem;" + RV_Float + ":0.5rem;",
                            Childs: [
                                {
                                    Type: "i", Class: "fa fa-trash-o fa-lg rv-icon-button",
                                    Tooltip: RVDic.Remove, Name: "removeButton",
                                    Attributes: [{ Name: "aria-hidden", Value: true }]
                                }
                            ]
                        }),
                        (!item.ID || that.Options.SelectMode ? null : {
                            Type: "div", Style: "position:absolute; top:0.5rem;" + RV_Float + ":2rem;",
                            Childs: [
                                {
                                    Type: "i", Class: "fa fa-pencil fa-lg rv-icon-button",
                                    Tooltip: RVDic.Edit, Name: "editButton",
                                    Attributes: [{ Name: "aria-hidden", Value: true }]
                                }
                            ]
                        }),
                        (!item.ID ? null : {
                            Type: "div", Style: "position:absolute; top:0.5rem;" + RV_RevFloat + ":0.5rem;",
                            Childs: [
                                {
                                    Type: "i", Style: (item.ID ? "" : "display:none;"), Name: "checkButton",
                                    Attributes: [{ Name: "aria-hidden", Value: true }]
                                }
                            ]
                        }),
                        { Type: "div", Class: "small-12 medium-12 large-12 row", Name: "viewArea", Style: "margin:0rem;" },
                        (that.Options.SelectMode ? null : {
                            Type: "div", Class: "small-12 medium-12 large-12 row", Name: "editArea",
                            Style: "margin:0rem; display:none;",
                            Childs: [
                                create_input({ Name: "name", Title: RVDic.Name }),
                                create_input({ Name: "url", Title: RVDic.URL }),
                                create_input({ Name: "username", Title: RVDic.UserName }),
                                create_input({ Name: "password", Title: RVDic.Password, Type: "password" })
                            ]
                        }),
                        (item.ID ? null : {
                            Type: "div", Class: "small-6 medium-5 large-4 rv-air-button rv-circle",
                            Style: "margin:1rem auto 0 auto;", Name: "editButton",
                            Childs: [{ Type: "text", TextValue: RVDic.Confirm }]
                        })
                    ]
                }
            ]);

            if (options.Add2Top) container.insertBefore(elems["container"], container.firstChild);
            else container.appendChild(elems["container"]);

            elems["container"].TheObject = item;

            var viewArea = elems["viewArea"];
            var editArea = elems["editArea"];
            var nameInput = elems["name"];
            var urlInput = elems["url"];
            var usernameInput = elems["username"];
            var passwordInput = elems["password"];
            var checkButton = elems["checkButton"];

            var removeButton = elems["removeButton"];
            var editButton = elems["editButton"];

            var processing = false;

            if (removeButton) removeButton.onclick = function () {
                if (processing) return;

                var msg = RVDic.Confirms.DoYouWantToRemoveN.replace("[n]", "'" + Base64.decode(item.Name) + "'");

                GlobalUtilities.confirm(msg, function (r) {
                    if (!r) return;
                    processing = true;

                    UsersAPI.RemoveRemoteServer({
                        ID: item.ID, ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                            else {
                                jQuery(elems["container"]).fadeOut(500, function () {
                                    this.remove();
                                    that.set_search_input_display();
                                });
                            }

                            processing = false;
                        }
                    });
                });
            };

            var check_connection = function () {
                checkButton.style.color = "black";
                checkButton.setAttribute("class", "fa fa-spinner fa-lg fa-pulse");

                RVRequest.check_remote_connection({
                    BaseURL: Base64.decode(item.URL),
                    UserName: Base64.decode(item.UserName),
                    Password: Base64.decode(item.Password)
                }, function (result) {
                    checkButton.style.color = result ? "green" : "red";
                    checkButton.setAttribute("class", result ? "fa fa-check-circle-o fa-lg" : "fa fa-times fa-lg");
                });
            };

            var _set_data = function () {
                viewArea.innerHTML = "";

                GlobalUtilities.create_nested_elements([
                    create_view_element({ Title: RVDic.Name, Value: Base64.decode(item.Name) }),
                    create_view_element({ Title: RVDic.URL, Value: Base64.decode(item.URL) }),
                    create_view_element({ Title: RVDic.UserName, Value: Base64.decode(item.UserName) })
                ], viewArea);
                
                if (item.ID && editArea) {
                    nameInput.value = Base64.decode(item.Name);
                    urlInput.value = Base64.decode(item.URL);
                    usernameInput.value = Base64.decode(item.UserName);
                    passwordInput.value = Base64.decode(item.Password);
                }
            };

            var _on_edit = function () {
                var set_things = function () {
                    editArea.style.display = editButton.__Editing ? "flex" : "none";
                    viewArea.style.display = editButton.__Editing ? "none" : "flex";
                    checkButton.style.display = editButton.__Editing ? "none" : "block";

                    _set_data();

                    if (item.ID) {
                        editButton.setAttribute("class",
                            "fa fa-" + (editButton.__Editing ? "floppy-o" : "pencil") + " fa-lg rv-icon-button");

                        GlobalUtilities.append_tooltip(editButton, editButton.__Editing ? RVDic.Save : RVDic.Edit);
                    }
                };

                if (editButton.__Editing === true) {
                    var newName = GlobalUtilities.trim(nameInput.value);
                    var newUrl = GlobalUtilities.trim(urlInput.value);
                    var newUsername = GlobalUtilities.trim(usernameInput.value);
                    var newPassword = GlobalUtilities.trim(passwordInput.value);

                    if (!newName || !newUrl || !newUsername || !newPassword) return;

                    GlobalUtilities.block(container);

                    UsersAPI[item.ID ? "ModifyRemoteServer" : "AddRemoteServer"]({
                        ID: item.ID, Name: Base64.encode(newName), URL: Base64.encode(newUrl),
                        UserName: Base64.encode(newUsername), Password: Base64.encode(newPassword), ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText)
                                alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                            else {
                                item.Name = result.Server.Name;
                                item.URL = result.Server.URL;
                                item.UserName = result.Server.UserName;
                                item.Password = result.Server.Password;

                                if (item.ID) {
                                    editButton.__Editing = false;
                                    set_things();

                                    check_connection();
                                }
                                else if (options.OnSave) {
                                    item.ID = result.Server.ID;
                                    options.OnSave(item);
                                }
                            }

                            GlobalUtilities.unblock(container);
                        }
                    });
                }
                else editButton.__Editing = true;

                set_things();
            }; //end of _on_edit

            if(editButton) editButton.onclick = _on_edit;

            if (!item.ID && editButton) _on_edit();
            else check_connection();

            _set_data();

            that.set_search_input_display();
        }
    };
})();