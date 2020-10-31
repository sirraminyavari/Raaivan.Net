(function () {
    if (window.NewSingleDataContainer) return;

    window.NewSingleDataContainer = function (containerDiv, params) {
        this.ContainerDiv = typeof (containerDiv) == "object" ? containerDiv : document.getElementById(containerDiv);
        if (!this.ContainerDiv) return;
        params = params || {};

        this.Interface = {
            ItemsArea: null,
            InputDiv: null,
            ButtonsArea: null
        };

        this.Objects = {
            SelectedItems: {},
            Autosuggest: null
        };

        this.Options = {
            EnableTextItem: params.EnableTextItem === true,
            CustomData: params.CustomData,
            NoButtons: params.NoButtons,
            OnItemAdd: params.OnItemAdd,
            OnAfterAdd: params.OnAfterAdd,
            OnItemRemove: params.OnItemRemove,
            OnAfterRemove: params.OnAfterRemove,
            OnLoad: params.OnLoad
        };

        this._initialize(params);
    }

    NewSingleDataContainer.prototype = {
        _initialize: function (params) {
            var that = this;

            that.ContainerDiv.innerHTML = "";

            var _inputClass = that.Options.NoButtons ? "small-12 medium-12 large-12" : "small-12 medium-6 large-6";

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12 row", Style: "margin:0rem;",
                    Childs: [
                        { Type: "div", Class: _inputClass, Name: "input" },
                        {
                            Type: "div", Class: "small-12 medium-6 large-6", Name: "buttons",
                            Style: that.Options.NoButtons ? "display:none;" : ""
                        }
                    ]
                },
                { Type: "div", Class: "small-12 medium-12 large-12", Name: "items", Style: "margin-top:0.5rem;" }
            ], that.ContainerDiv);

            that.Interface.ItemsArea = elems["items"];
            that.Interface.InputDiv = elems["input"];
            that.Interface.ButtonsArea = elems["buttons"];

            params.OnSelect = function () {
                var index = this.selectedIndex;
                that._add_item(this.keywords[index], this.values[index]);

                as.InputElement.value = "";
                jQuery(as.InputElement).focus();
            };

            var _oldOnEnter = params.OnEnter;
            var _onenter = null;
            
            if (that.Options.EnableTextItem) {
                _onenter = function (e) {
                    if (_oldOnEnter) _oldOnEnter();

                    var value = GlobalUtilities.trim(as.InputElement.value);
                    if (!value) return;
                    
                    as.InputElement.value = "";
                    if (that.Objects.SelectedItems[value]) return;
                    
                    if (as.selectedIndex < 0 || (as.selectedIndex >= 0 && value != as.keywords[as.selectedIndex]) ||
                        !as.getRow()) {
                        that._add_item(value, value);
                        return false;
                    }
                }
            }

            params.OnEnter = _onenter;
            
            var as = that.Objects.Autosuggest = GlobalUtilities.append_autosuggest(elems["input"], params);

            if (that.Options.OnLoad) that.Options.OnLoad.call(that);
        },

        buttons_area: function () {
            return this.Interface.ButtonsArea;
        },

        _add_item: function (title, id) {
            var that = this;
            
            if (that.Objects.SelectedItems[id]) return;
            var _done = false;
            
            var onAdd = that.Options.OnItemAdd || function (e, d) { d(); };
            var succeed = function () {
                if (_done) return;
                _done = true;
                that.add_item(title, id);
            }

            var prevented = false;

            var _event = { Item: { ID: id, Title: title }, preventDefault: function () { prevented = true } };

            if (onAdd.call(that, _event, succeed) !== false && !prevented) succeed();
        },

        bind_data_source: function (datasource) {
            var that = this;

            var isArray = typeof (datasource) == "object" ? true : false;
            that.clear();
            isArray ? that.Objects.Autosuggest.bindArray(datasource) : that.Objects.Autosuggest.bindURL(datasource);
        },

        add_item: function (title, id, data) {
            var that = this;
            
            if (that.Objects.SelectedItems[id]) return;

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "rv-border-radius-quarter rv-bg-color-white-softer SoftShadow SoftBorder", Name: "container",
                    Style: "display:inline-block; margin:0.3rem; padding:0.3rem; margin-" + RV_Float + ":0rem;" +
                        "cursor:default; border-color:rgb(220,220,220);",
                    Childs: [
                        {
                            Type: "i", Class: "fa fa-times fa-lg rv-icon-button",
                            Style: "margin-" + RV_RevFloat + ":0.5rem;", Tooltip: RVDic.Remove,
                            Attributes: [{ Name: "aria-hidden", Value: true }],
                            Properties: [
                                {
                                    Name: "onclick",
                                    Value: function () {
                                        that.Objects.SelectedItems[id] = null
                                        elems["container"].parentNode.removeChild(elems["container"]);
                                        if (that.Options.OnItemRemove) that.Options.OnItemRemove();
                                        if (that.Options.OnAfterRemove) that.Options.OnAfterRemove({ ID: id, Title: title });
                                    }
                                }
                            ]
                        },
                        { Type: "text", TextValue: title },
                        { Type: "div", Style: "display:none; margin-" + RV_Float + ":0.5rem;", Name: "customData" }
                    ]
                }
            ], that.Interface.ItemsArea);

            that.Objects.SelectedItems[id] = { Title: title, CustomData: null };

            if (that.Options.CustomData) {
                elems["customData"].style.display = "inline-block";

                that.Options.CustomData(elems["customData"], { ID: id, Title: title, Data: data }, function (p) {
                    that.Objects.SelectedItems[id].CustomData = p;
                });
            }

            if (that.Options.OnAfterAdd) that.Options.OnAfterAdd({ ID: id, Title: title });
        },

        hide_input: function () {
            this.Interface.InputDiv.style.display = "none";
        },

        show_input: function () {
            this.Interface.InputDiv.style.display = "block";
        },

        get_items: function () {
            var that = this;
            var retArr = [];
            
            for (var id in that.Objects.SelectedItems) {
                if (that.Objects.SelectedItems[id]) {
                    var customData = that.Objects.SelectedItems[id].CustomData;

                    retArr.push({
                        ID: id,
                        Title: that.Objects.SelectedItems[id].Title,
                        Data: GlobalUtilities.get_type((customData || {}).Get) != "function" ? null : customData.Get()
                    });
                }
            }

            return retArr;
        },

        get_items_string: function (delimiter) {
            var that = this;
            var retStr = "";
            for (var id in that.Objects.SelectedItems) {
                if (!that.Objects.SelectedItems[id]) continue;
                retStr += (retStr == "" ? "" : delimiter) + id;
            }
            return retStr;
        },

        clear: function (params) {
            params = params || {};
            var that = this;

            that.Interface.ItemsArea.innerHTML = "";
            that.Objects.SelectedItems = {};
            
            if (!params.SelectedOnly) that.Objects.Autosuggest.clear();
        }
    }
})();