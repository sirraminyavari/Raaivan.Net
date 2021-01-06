(function () {
    if (window.FormElementsManager) return;

    window.FormElementsManager = function (containerDiv, params) {
        this.ContainerDiv = typeof (containerDiv) == "object" ? containerDiv : document.getElementById(containerDiv);
        if (this.ContainerDiv == null) return;

        params = params || {};

        this.Interface = {
            NameArea: null,
            DescriptionArea: null,
            PreviewButton: null,
            SortButton: null,
            ElementsArea: null,
            NewElementTitleArea: null,
            NewElementNameInput: null,
            NewElementHelpArea: null,
            ElementTypeSelect: null,
            AddElementButton: null
        };

        this.Objects = {
            FormID: params.FormID,
            Description: "",
            Name: "",
            Elements: [],
            NewElementTitle: null,
            NewElementHelp: null,
            FormDescription: null,
            Options: {}
        };

        var that = this;

        that.ContainerDiv.innerHTML = "";
        GlobalUtilities.block(that.ContainerDiv);

        GlobalUtilities.load_files([
            { Root: "API/", Ext: "js", Childs: ["FGAPI", "CNAPI", "UsersAPI"] },
            "SingleDataContainer/NewSingleDataContainer.js",
            "AdvancedTextArea/AdvancedTextArea.js",
            "FormsManager/FormElementTypes.js"
        ], { OnLoad: function () { that._preinit(); } });
    }

    FormElementsManager.prototype = {
        _preinit: function () {
            var that = this;

            var elemTypes = [];

            var _add_type = function (_eType, _title) {
                elemTypes.push({
                    Type: "option", Attributes: (_eType ? [{ Name: "title", Value: _eType }] : []),
                    Childs: [{ Type: "text", TextValue: RVDic.FG.ElementTypes[_title] || _title }]
                });

                if (_eType && (FormElementTypes[_eType] || {}).edit) that.Objects.Options[_eType] = FormElementTypes[_eType].edit(null, { Type: _eType });
            }

            _add_type(null, RVDic.OfType + "...");
            for (var tp in FormElementTypes) _add_type(tp, tp);
            
            var elems = GlobalUtilities.create_nested_elements([
                { Type: "div", Class: "small-12 medium-12 large-12", Name: "nameArea" },
                { Type: "div", Class: "small-12 medium-12 large-12", Name: "descriptionArea" },
                {
                    Type: "div", Class: "small-12 medium-12 large-12 RevTextAlign RevDirection", Style: "display:flex; flex-flow:row;",
                    Childs: [
                        {
                            Type: "div", Class: "rv-air-button rv-circle", Name: "previewButton",
                            Style: "display:none; flex:0 1 auto; margin-top:0.5rem; padding:0.3rem 1.5rem; margin-" + RV_Float + ":0.5rem;", 
                            Properties: [{ Name: "onclick", Value: function () { that.preview(); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.Preview }]
                        },
                        {
                            Type: "div", Class: "rv-air-button rv-circle", Name: "sortButton",
                            Style: "display:none; flex:0 1 auto; margin-top:0.5rem; padding:0.3rem 1.5rem; margin-" + RV_Float + ":0.5rem;", 
                            Properties: [{ Name: "onclick", Value: function () { that.sort_dialog(); } }],
                            Childs: [{ Type: "text", TextValue: RVDic.Sort }]
                        }
                    ]
                },
                { Type: "hr", Class: "small-12 medium-12 large-12", Style: "margin-top:0.5rem;" },
                { Type: "div", Class: "small-12 medium-12 large-12", Style: "margin-top:0.5rem;", Name: "elementsArea" },
                { Type: "hr", Class: "small-12 medium-12 large-12", Style: "margin-top:0.5rem;" },
                { Type: "div", Class: "small-12 medium-12 large-12", Style: "margin-top:0.5rem;", Name: "newElementTitle" },
                {
                    Type: "div", Class: "small-12 medium-12 large-12",
                    Style: "margin-top:0.5rem; display:none;", Name: "nameContainer",
                    Childs: [
                        {
                            Type: "input", Class: "rv-input", InnerTitle: RVDic.Name, Tooltip: RVDic.Name,
                            Style: "width:100%;", Name: "elementNameInput"
                        }
                    ]
                },
                {
                    Type: "div", Class: "small-12 medium-12 large-12",
                    Style: "margin-top:0.5rem; display:none;", Name: "helpContainer"
                },
                {
                    Type: "div", Class: "small-12 medium-12 large-12 row", Style: "margin:0rem; margin-top:0.5rem;",
                    Childs: [
                        {
                            Type: "div", Class: "small-6 medium-6 large-6",
                            Childs: [
                                {
                                    Type: "select", Class: "rv-input", Name: "elementTypeSelect", Style: "width:20rem;",
                                    Childs: elemTypes
                                }
                            ]
                        },
                        {
                            Type: "div", Class: "small-6 medium-6 large-6 RevTextAlign RevDirection",
                            Childs: [
                                {
                                    Type: "div", Class: "ActionButton TextAlign Direction", Name: "addElementButton",
                                    Style: "display:inline-block; font-weight:bold; padding:0.2rem 2rem;",
                                    Childs: [{ Type: "text", TextValue: RVDic.Add }]
                                }
                            ]
                        }
                    ]
                },
                {
                    Type: "div", Class: "small-12 medium-12 large-12", Name: "optionsArea",
                    Style: "display:none; margin-top:0.5rem;"
                }
            ], that.ContainerDiv);

            that.Interface.NameArea = elems["nameArea"];
            that.Interface.DescriptionArea = elems["descriptionArea"];
            that.Interface.PreviewButton = elems["previewButton"];
            that.Interface.SortButton = elems["sortButton"];
            that.Interface.ElementsArea = elems["elementsArea"];
            that.Interface.NewElementTitleArea = elems["newElementTitle"];
            that.Interface.NewElementNameInput = elems["elementNameInput"];
            that.Interface.NewElementHelpArea = elems["helpContainer"];
            that.Interface.ElementTypeSelect = elems["elementTypeSelect"];
            that.Interface.AddElementButton = elems["addElementButton"];

            var optionsArea = elems["optionsArea"];

            that.Interface.ElementTypeSelect.onchange = function () {
                var _type = this[this.selectedIndex].title;

                elems["nameContainer"].style.display = /*(_type == "Separator") ||*/ !_type ? "none" : "block";
                elems["helpContainer"].style.display = (_type == "Separator") || !_type ? "none" : "block";

                jQuery(optionsArea).fadeOut(0);
                if (!_type) return;

                optionsArea.innerHTML = "";
                var opContainer = (that.Objects.Options[_type] || {}).Container;
                if (opContainer) {
                    optionsArea.appendChild(opContainer);
                    jQuery(optionsArea).fadeIn(0);
                }
            }

            that.initialize();
        },

        initialize: function () {
            var that = this;

            that.Objects.NewElementTitle = new AdvancedTextArea({
                ContainerDiv: that.Interface.NewElementTitleArea,
                DefaultText: RVDic.NewFieldTitle + "...",
                QueryTemplate: "RelatedThings",
                ItemTemplate: { ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL" }
            });

            that.Objects.NewElementHelp = new AdvancedTextArea({
                ContainerDiv: that.Interface.NewElementHelpArea,
                DefaultText: RVDic.Help + "...",
                QueryTemplate: "RelatedThings",
                ItemTemplate: { ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL" }
            });

            FGAPI.GetFormElements({
                FormID: that.Objects.FormID, ParseResults: true,
                ResponseHandler: function (result) {
                    that.Objects.Description = Base64.decode(result.FormDescription);
                    that.Objects.Name = Base64.decode(result.FormName || "");
                    
                    that.set_description();
                    that.set_name();

                    var elements = result.Elements || [];

                    for (var i = 0, lnt = elements.length; i < lnt; ++i)
                        that.add_element(elements[i]);

                    GlobalUtilities.unblock(that.ContainerDiv);
                }
            });

            that.Interface.AddElementButton.onclick = function () {
                if (this.__Adding) return;
                this.__Adding = true;

                var title = GlobalUtilities.trim(that.Objects.NewElementTitle.get_data());
                var name = GlobalUtilities.trim(that.Interface.NewElementNameInput.value);
                var help = GlobalUtilities.trim(that.Objects.NewElementHelp.get_data());

                if (!title) {
                    alert(RVDic.TitleCannotBeEmpty);
                    this.__Adding = false;
                    return;
                }

                var index = that.Interface.ElementTypeSelect.selectedIndex;
                var type = index >= 0 ? that.Interface.ElementTypeSelect[index].title : "";
                if (!type) {
                    alert(RVDic.PleaseSelectFieldType);
                    this.__Adding = false;
                    return;
                }

                var options = ((that.Objects.Options[type] || {}).Get || function () { })();
                if (options === false) {
                    alert(RVDic.PleaseFillNecessaryFields);
                    this.__Adding = false;
                    return;
                }

                FGAPI.AddFormElement({
                    FormID: that.Objects.FormID, Title: Base64.encode(title), Name: Base64.encode(name),
                    Help: Base64.encode(help), Type: type, SequenceNumber: that.get_last_sequence_number() + 1,
                    Info: Base64.encode(JSON.stringify(options || {})), ParseResults: true,
                    ResponseHandler: function (result) {
                        if (result.ErrorText)
                            alert(RVDic.MSG[result.ErrorText] || result.ErrorText, { Timeout: 10000 });
                        else if (result.Succeed) {
                            that.add_element(result.Element);

                            that.Objects.NewElementTitle.set_data("");
                            that.Interface.NewElementNameInput.value = "";
                            that.Objects.NewElementHelp.set_data("");

                            for (var tp in that.Objects.Options)
                                ((that.Objects.Options[tp] || {}).Clear || function () { })();
                        }

                        that.Interface.AddElementButton.__Adding = false;
                    }
                });
            }
        },

        clear: function () {
            this.ContainerDiv.innerHTML = "";
        },

        set_description: function () {
            var that = this;

            var description = that.Objects.Description;

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12", Name: "container",
                    Style: "position:relative; min-height:2.5rem; padding-" + RV_Float + ":2.5rem;",
                    Childs: [
                        {
                            Type: "div",
                            Style: "position:absolute; top:0rem; bottom:0rem; " + RV_Float + ":0rem; width:2.5rem;",
                            Childs: [
                                {
                                    Type: "i", Class: "fa fa-pencil fa-2x rv-icon-button", Name: "editButton",
                                    Attributes: [{ Name: "aria-hidden", Value: true }]
                                }
                            ]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "text-align:justify;", Name: "descValue"
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "display:none;", Name: "descInput"
                        }
                    ]
                }
            ], that.Interface.DescriptionArea);

            var descValue = elems["descValue"];
            var editButton = elems["editButton"];
            var descInput = null;

            var _set_desc = function () {
                descValue.innerHTML = "";

                var desc = GlobalUtilities.get_text_begining(description, 3000, "", { RichText: false });

                if (desc) GlobalUtilities.append_markup_text(descValue, desc);
                else descValue.innerHTML =
                    "<span style='color:gray; font-weight:bold;'>" + "(" + RVDic.ThereIsNoDescription + ")" + "</span>";

                if (descInput) descInput.set_data(description);
            };

            var _on_edit = function () {
                var set_things = function () {
                    elems["descInput"].style.display = editButton.__Editing ? "block" : "none";
                    descValue.style.display = editButton.__Editing ? "none" : "block";

                    if (editButton.__Editing && !descInput) {
                        descInput = that.Objects.FormDescription = new AdvancedTextArea({
                            ContainerDiv: elems["descInput"], DefaultText: RVDic.Description,
                            QueryTemplate: "RelatedThings",
                            ItemTemplate: {
                                ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL"
                            }
                        });
                    }

                    _set_desc();

                    editButton.setAttribute("class",
                        "fa fa-" + (editButton.__Editing ? "save" : "pencil") + " fa-2x rv-icon-button");
                }

                if (editButton.__Editing === true) {
                    var newDescription = GlobalUtilities.trim(descInput.get_data());

                    var ____sv = function () {
                        GlobalUtilities.block(elems["container"]);

                        FGAPI.SetFormDescription({
                            FormID: that.Objects.FormID, Description: Base64.encode(newDescription), ParseResults: true,
                            ResponseHandler: function (result) {
                                if (result.ErrorText)
                                    alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                                else {
                                    description = that.Objects.Description = newDescription;
                                    editButton.__Editing = false;
                                    set_things();
                                }

                                GlobalUtilities.unblock(elems["container"]);
                            }
                        });
                    };

                    if (!newDescription) {
                        GlobalUtilities.confirm(RVDic.Confirms.DoYouWantToSaveEmptyDescription, function (result) {
                            if (!result) return;
                            ____sv();
                        });
                    }
                    else ____sv();
                }
                else editButton.__Editing = true;

                set_things();
            } //end of _on_edit

            editButton.onclick = _on_edit;
            if (!description) _on_edit();
            _set_desc();
        },

        set_name: function () {
            var that = this;

            var name = that.Objects.Name;

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12", Name: "container",
                    Style: "position:relative; min-height:2.5rem; padding-" + RV_Float + ":2.5rem; margin-top:0.5rem;",
                    Childs: [
                        {
                            Type: "div",
                            Style: "position:absolute; top:0rem; bottom:0rem; " + RV_Float + ":0rem; width:2.5rem;",
                            Childs: [
                                {
                                    Type: "i", Class: "fa fa-pencil fa-2x rv-icon-button", Name: "editButton",
                                    Attributes: [{ Name: "aria-hidden", Value: true }]
                                }
                            ]
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "text-align:justify;", Name: "nameValue"
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "display:none;", Name: "nameInput",
                            Childs: [
                                {
                                    Type: "input", Class: "rv-input", Name: "theInput",
                                    Style: "width:100%;", InnerTitle: RVDic.Name
                                }
                            ]
                        }
                    ]
                }
            ], that.Interface.DescriptionArea);

            var nameValue = elems["nameValue"];
            var editButton = elems["editButton"];
            var nameInput = elems["theInput"];

            var _set_name = function () {
                nameValue.innerHTML = "";

                if (name) nameValue.innerHTML = name;
                else nameValue.innerHTML = "<span style='color:gray; font-weight:bold;'>" + "(" + RVDic.NotSet + ")" + "</span>";

                nameInput.value = name;
            };

            var _on_edit = function () {
                var set_things = function () {
                    elems["nameInput"].style.display = editButton.__Editing ? "block" : "none";
                    nameValue.style.display = editButton.__Editing ? "none" : "block";

                    _set_name();

                    editButton.setAttribute("class",
                        "fa fa-" + (editButton.__Editing ? "save" : "pencil") + " fa-2x rv-icon-button");
                }

                if (editButton.__Editing === true) {
                    var newName = GlobalUtilities.trim(nameInput.value);

                    GlobalUtilities.block(elems["container"]);

                    FGAPI.SetFormName({
                        FormID: that.Objects.FormID, Name: Base64.encode(newName), ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText)
                                alert(RVDic.MSG[result.ErrorText] || result.ErrorText, { Timeout: 10000 });
                            else {
                                name = that.Objects.Name = newName;
                                editButton.__Editing = false;
                                set_things();
                            }

                            GlobalUtilities.unblock(elems["container"]);
                        }
                    });
                }
                else editButton.__Editing = true;

                set_things();
            } //end of _on_edit

            editButton.onclick = _on_edit;
            if (!name) _on_edit();
            _set_name();
        },

        get_last_sequence_number: function () {
            for (var i = this.Objects.Elements.length; i >= 0; --i) {
                if (this.Objects.Elements[i] == null) continue;
                return this.Objects.Elements[i].SequenceNumber;
            }
            return 0;
        },

        get_element_index: function (elementId) {
            for (var i = 0, lnt = this.Objects.Elements.length; i < lnt; ++i)
                if (this.Objects.Elements[i] != null && this.Objects.Elements[i].ElementID == elementId) return i;
            return -1;
        },

        get_next_element_index: function (elementId) {
            var index = this.get_element_index(elementId);
            if (index < 0) return index;
            for (var i = index + 1, lnt = this.Objects.Elements.length; i < lnt; ++i)
                if (this.Objects.Elements[i] != null) return i;
            return -1;
        },

        get_previous_element_index: function (elementId) {
            var index = this.get_element_index(elementId);
            if (index < 0) return index;
            for (var i = index - 1; i >= 0; --i)
                if (this.Objects.Elements[i] != null) return i;
            return -1;
        },

        add_element: function (params) {
            params = params || {};

            var that = this;

            params.Title = Base64.decode(params.Title);
            params.Name = Base64.decode(params.Name);
            params.Help = Base64.decode(params.Help);

            var elementId = params.ElementID;
            var title = params.Title;
            var name = params.Name;
            var help = params.Help;
            var type = params.Type;
            var info = JSON.parse(Base64.decode(params.Info) || "{}");
            var weight = params.Weight;
            var sequenceNumber = +params.SequenceNumber;
            if (isNaN(sequenceNumber)) sequenceNumber = 0;

            var necessary = params.Necessary === true;
            var uniqueValue = params.UniqueValue === true;

            var hasUniqueCheckbox = (type == "Text") || (type == "Numeric");

            var _create_checkbox = function (p) {
                p = p || {};

                var theId = "r" + GlobalUtilities.random_str(10);
                var processing = false;
                
                return {
                    Type: "div", Class: "rv-border-radius-quarter rv-bg-color-white-softer SoftShadow",
                    Style: "display:" + (p.Hide ? "none" : "inline-block") + "; padding:0rem 0.3rem;" +
                        "cursor:pointer; margin-" + RV_Float + ":0.5rem;",
                    Properties: [{
                        Name: "onclick", Value: function () {
                            if (processing) return;
                            processing = true;
                            var curVal = _el[theId].Checked;
                            p.Action(!curVal, function () { processing = false; _el[theId][curVal ? "uncheck" : "check"]({ StopOnChange: true }); });
                        }
                    }],
                    Childs: [
                        {
                            Type: "checkbox", Name: theId,
                            Style: "width:0.8rem; height:0.8rem; cursor:pointer; margin-" + RV_RevFloat + ":0.3rem;",
                            Params: {
                                Checked: p.Value, Width: 14, Height: 14,
                                OnClick: function (e, done) {
                                    e.preventDefault();
                                    if (processing) return;
                                    processing = true;
                                    p.Action(!this.Checked, function () { processing = false; done(true); });
                                }
                            }
                        },
                        {
                            Type: "div", Style: "display:inline-block; font-size:0.6rem;",
                            Childs: [{ Type: "text", TextValue: p.Title }]
                        }
                    ]
                };
            };

            var _el = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-12 medium-12 large-12 rv-border-radius-quarter rv-bg-color-trans-white",
                    Style: "margin-bottom:1rem; padding:0.3rem;", Name: "elementDiv",
                    Childs: [
                        {
                            Type: "div", Class: "small-12 medium-12 large-12", Style: "margin-bottom:0.3rem;",
                            Childs: [
                                {
                                    Type: "div", Style: "display:inline-block; width:1.5rem;",
                                    Childs: [
                                        {
                                            Type: "i", Class: "fa fa-times fa-lg rv-icon-button",
                                            Tooltip: RVDic.Remove, Name: "removeButton",
                                            Attributes: [{ Name: "aria-hidden", Value: true }]
                                        }
                                    ]
                                },
                                {
                                    Type: "div", Style: "display:inline-block; width:2rem;",
                                    Childs: [
                                        {
                                            Type: "i", Class: "fa fa-pencil fa-lg rv-icon-button",
                                            Tooltip: RVDic.Edit, Name: "editButton",
                                            Attributes: [{ Name: "aria-hidden", Value: true }]
                                        }
                                    ]
                                },
                                {
                                    Type: "div",
                                    Style: "display:inline-block; font-weight:bold; font-size:0.7rem;",
                                    Childs: [{ Type: "text", TextValue: "(" + (RVDic.FG.ElementTypes[type] || RVDic.FG.ElementTypes.Text) + ")" }]
                                },
                                _create_checkbox({
                                    Value: necessary, Hide: type == "Separator", Title: RVDic.Necessity,
                                    Action: function (value, done) {
                                        FGAPI.SetFormElementNecessity({
                                            ElementID: elementId, Necessary: value, ParseResults: true,
                                            ResponseHandler: function (result) { if (!result.ErrorText) done(); }
                                        });
                                    }
                                }),
                                _create_checkbox({
                                    Value: uniqueValue, Hide: !hasUniqueCheckbox, Title: RVDic.UniqueValue,
                                    Action: function (value, done) {
                                        FGAPI.SetFormElementUniqueness({
                                            ElementID: elementId, Value: value, ParseResults: true,
                                            ResponseHandler: function (result) { if (!result.ErrorText) done(); }
                                        });
                                    }
                                }),
                                {
                                    Type: "div", Class: "rv-air-button rv-border-radius-quarter", Name: "privacyButton",
                                    Style: "display:inline-block; font-weight:bold; font-size:0.6rem; margin-" + RV_Float + ":1rem;",
                                    Childs: [
                                        { Type: "i", Class: "fa fa-key", Style: "margin-" + RV_RevFloat + ":0.4rem;" },
                                        { Type: "text", TextValue: RVDic.Privacy }
                                    ]
                                }
                            ]
                        },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "titleArea" },
                        { Type: "div", Class: "small-12 medium-12 large-12", Style: "display:none;", Name: "titleEditArea" },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "nameArea", Style: "margin-top:0.5rem;" },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "margin-top:0.5rem; display:none;", Name: "nameEditArea",
                            Childs: [
                                {
                                    Type: "input", Class: "rv-input", InnerTitle: RVDic.Name, Tooltip: RVDic.Name,
                                    Style: "width:100%;", Name: "nameInput"
                                }
                            ]
                        },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "helpArea", Style: "margin-top:0.5rem;" },
                        { Type: "div", Class: "small-12 medium-12 large-12", Name: "helpEditArea", Style: "margin-top:0.5rem; display:none;" },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12", Name: "optionsView",
                            Style: "font-size:0.7rem; margin-top:0.5rem; margin-" + RV_Float + ":1rem;"
                        },
                        {
                            Type: "div", Class: "small-12 medium-12 large-12",
                            Style: "margin-top:0.5rem; display:none;", Name: "optionsEdit"
                        }
                    ]
                }
            ], this.Interface.ElementsArea);

            var elementDiv = _el["elementDiv"];

            elementDiv.ElementObject = params;

            var removeButton = _el["removeButton"];
            var editButton = _el["editButton"];
            var titleArea = _el["titleArea"];
            var titleEditArea = _el["titleEditArea"];
            var nameArea = _el["nameArea"];
            var nameEditArea = _el["nameEditArea"];
            var helpArea = _el["helpArea"];
            var helpEditArea = _el["helpEditArea"];
            var optionsViewArea = _el["optionsView"];
            var optionsEditArea = _el["optionsEdit"];

            //if (type == "Separator") nameArea.style.display = nameEditArea.style.display = "none";

            var optionsView = (FormElementTypes[type] || {}).view ? FormElementTypes[type].view({ Info: info, Weight: weight }) : null;
            var optionsEdit = (FormElementTypes[type] || {}).edit ? FormElementTypes[type].edit(info, params) : null;
            if (optionsView) optionsViewArea.appendChild(optionsView.Container);
            if (optionsEdit) optionsEditArea.appendChild(optionsEdit.Container);

            var element = {
                ElementID: elementId, Type: type, Title: title, Name: name, Help: help,
                SequenceNumber: sequenceNumber, Info: info, ContainerDiv: elementDiv,
                TitleInput: null, NameInput: _el["nameInput"], HelpInput: null, SelectOptionsContainer: null
            };

            this.Objects.Elements.push(element);

            element.TitleInput = new AdvancedTextArea({
                ContainerDiv: titleEditArea, DefaultText: RVDic.FieldTitle + "...",
                QueryTemplate: "RelatedThings",
                ItemTemplate: { ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL" }
            });

            element.HelpInput = new AdvancedTextArea({
                ContainerDiv: helpEditArea, DefaultText: RVDic.Help + "...",
                QueryTemplate: "RelatedThings",
                ItemTemplate: { ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL" }
            });

            var _set_data = function () {
                var ttl = GlobalUtilities.get_text_begining(element.Title, 3000, "", { RichText: false });

                titleArea.innerHTML = "";
                GlobalUtilities.append_markup_text(titleArea, ttl);
                element.TitleInput.set_data(element.Title);

                nameArea.innerHTML = RVDic.Name + ": " + (element.Name || RVDic.NotSet);
                element.NameInput.value = element.Name;

                helpArea.innerHTML = "";
                GlobalUtilities.append_markup_text(helpArea, RVDic.Help + ": " + element.Help);
                element.HelpInput.set_data(element.Help)

                if (optionsEdit && optionsEdit.Set) optionsEdit.Set(info, weight);
                if (optionsView && optionsView.Set) optionsView.Set({ Info: info, Weight: weight });
            };

            _set_data();

            removeButton.onclick = function () {
                if (removeButton.__Removing === true) return;

                GlobalUtilities.confirm(RVDic.Confirms.DoYouWantToRemoveTheField, function (result) {
                    if (!result) return;

                    removeButton.__Removing = true;

                    FGAPI.RemoveFormElement({
                        ElementID: elementId, ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                            else that.remove_element(elementId);

                            removeButton.__Removing = false;
                        }
                    });
                });
            };

            var _on_edit = function () {
                var set_things = function () {
                    titleEditArea.style.display = nameEditArea.style.display = helpEditArea.style.display =
                        editButton.__Editing ? "block" : "none";
                    optionsEditArea.style.display = editButton.__Editing ? "block" : "none";
                    titleArea.style.display = nameArea.style.display = helpArea.style.display = editButton.__Editing ? "none" : "block";
                    optionsViewArea.style.display = editButton.__Editing ? "none" : "block";

                    //if (type == "Separator") nameArea.style.display = nameEditArea.style.display = "none";

                    _set_data();

                    editButton.setAttribute("class",
                        "fa " + (editButton.__Editing ? "fa-floppy-o" : "fa-pencil") + " fa-lg rv-icon-button");
                    GlobalUtilities.append_tooltip(editButton, editButton.__Editing ? RVDic.Save : RVDic.Edit);
                };

                if (editButton.__Editing === true) {
                    var newTitle = GlobalUtilities.trim(element.TitleInput.get_data());
                    var newName = GlobalUtilities.trim(element.NameInput.value);
                    var newHelp = GlobalUtilities.trim(element.HelpInput.get_data());
                    var newInfo = optionsEdit && optionsEdit.Get ? optionsEdit.Get() : null;
                    var newWeight = optionsEdit && optionsEdit.Weight ? optionsEdit.Weight() : null;

                    if (!newTitle || (newInfo === false)) return;

                    GlobalUtilities.block(element.ContainerDiv);

                    FGAPI.ModifyFormElement({
                        ElementID: elementId, Title: Base64.encode(newTitle), Name: Base64.encode(newName), Help: Base64.encode(newHelp),
                        Info: Base64.encode(JSON.stringify(newInfo)), Weight: newWeight, ParseResults: true,
                        ResponseHandler: function (result) {
                            if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText, { Timeout: 10000 });
                            else {
                                title = element.Title = newTitle;
                                name = element.Name = newName;
                                help = element.Help = newHelp;
                                info = element.Info = newInfo;
                                weight = newWeight;
                                editButton.__Editing = false;
                                set_things();
                            }

                            GlobalUtilities.unblock(element.ContainerDiv);
                        }
                    });
                }
                else {
                    editButton.__Editing = true;
                    set_things();
                }
            } //end of _on_edit

            editButton.onclick = _on_edit;

            var privacyButton = _el["privacyButton"];
            var showed = null;

            privacyButton.onclick = function () {
                if (privacyButton.__Div) return (showed = GlobalUtilities.show(privacyButton.__Div));

                var _div = GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Class: "small-10 medium-8 large-6 rv-border-radius-1 SoftBackgroundColor",
                        Style: "margin:0rem auto; padding:1rem;", Name: "container"
                    }
                ])["container"];

                privacyButton.__Div = _div;

                GlobalUtilities.loading(_div);
                showed = GlobalUtilities.show(_div);

                GlobalUtilities.load_files(["PrivacyManager/PermissionSetting.js"], {
                    OnLoad: function () {
                        var pv = new PermissionSetting(_div, {
                            ObjectID: elementId,
                            Options: {
                                ConfidentialitySelect: true,
                                PermissionTypes: ["View"],
                                ObjectType: "FormElement",
                                OnSave: function (data) { showed.Close(); }
                            }
                        });
                    }
                });
            };


            that.set_sort_button_visibility();
        },

        remove_element: function (elementId) {
            var that = this;

            var index = that.get_element_index(elementId);
            if (index < 0) return;

            var _div = that.Objects.Elements[index].ContainerDiv;

            jQuery(_div).animate({ height: "toggle" }, 500, function () {
                jQuery(_div).remove();
                that.set_sort_button_visibility();
            });

            that.Objects.Elements[index] = null;
        },

        set_sort_button_visibility: function () {
            var that = this;
            var count = 0, first = that.Interface.ElementsArea.firstChild;
            while (first) {
                if (first.ElementObject)++count;
                first = first.nextSibling;
            }
            jQuery(that.Interface.SortButton)[count > 1 ? "fadeIn" : "fadeOut"](500);
            jQuery(that.Interface.PreviewButton)[count > 1 ? "fadeIn" : "fadeOut"](500);
        },

        sort_dialog: function () {
            var that = this;

            if (that.LoadingSortDialog) return;
            that.LoadingSortDialog = true;

            GlobalUtilities.load_files(["Public/SortDialog.js"], {
                OnLoad: function () {
                    new SortDialog({
                        Container: that.Interface.ElementsArea,
                        Title: RVDic._HelpSortNames,
                        CheckContainerItemValidity: function (item) { return !!item.ElementObject; },
                        GetItemID: function (item) { return item.ElementObject.ElementID; },
                        GetItemTitle: function (item) { return item.ElementObject.Title; },
                        APIFunction: function (data, done) {
                            FGAPI.SetElementsOrder({
                                ElementIDs: (data.SortedIDs || []).join("|"), ParseResults: true,
                                ResponseHandler: function (result) {
                                    if (result.ErrorText) alert(RVDic.MSG[result.ErrorText] || result.ErrorText);
                                    done(!result.ErrorText);
                                }
                            });
                        }
                    });


                    that.LoadingSortDialog = false;
                }
            });
        },

        preview: function () {
            var that = this;

            var elems = GlobalUtilities.create_nested_elements([
                {
                    Type: "div", Class: "small-11 medium-11 large-10 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0rem auto; padding:1rem;", Name: "container"
                }
            ]);

            GlobalUtilities.loading(elems["container"]);
            GlobalUtilities.show(elems["container"]);

            GlobalUtilities.load_files(["FormsManager/FormViewer.js",], {
                OnLoad: function () {
                    new FormViewer(elems["container"], {
                        FormID: that.Objects.FormID,
                        Editable: true, ElementsEditable: false, HideDescription: true,
                        FillButton: false, Exportable: false, HasConfidentiality: false
                    });
                }
            });
        }
    }
})();