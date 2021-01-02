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

        this.Options = {
            CssClass: {
                TypesContainer: "r" + GlobalUtilities.random_str(10),
                ElementsContainer: "r" + GlobalUtilities.random_str(10),
                Elements: "r" + GlobalUtilities.random_str(10),
                Placeholder: "r" + GlobalUtilities.random_str(10),
                DraggableItem: "r" + GlobalUtilities.random_str(10),
                Draggable: "r" + GlobalUtilities.random_str(10)
            },
            Icons: {
                Text: "textbox",
                Paragraph: "text-area",
                Date: "date",
                Select: "radio-button",
                Checkbox: "checkbox",
                Binary: "toggle-on-off",
                File: "file-upload",
                Separator: "divider",
                User: "user",
                Node: "at-sign",
                Form: "table",
                MultiLevel: "multilevel",
                Numeric: "numeric"
            }
        };

        var that = this;

        GlobalUtilities.block(that.ContainerDiv);

        GlobalUtilities.load_files([
            { Root: "API/", Ext: "js", Childs: ["FGAPI", "CNAPI", "UsersAPI", "PrivacyAPI"] },
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

            //_add_type(null, RVDic.OfType + "...");
            //for (var tp in FormElementTypes) _add_type(tp, tp);
            for (var tp in FormElementTypes) {
                elemTypes.push({
                    ID: tp,
                    Name: tp,
                    Title: RVDic.FG.ElementTypes[tp] || RVDic[tp] || tp,
                    Icon: that.icon(tp)
                });

                if (tp == "Text") {
                    elemTypes.push({
                        ID: "Paragraph", Name: "Text", Title: RVDic.Paragraph,
                        Icon: that.icon("Paragraph")
                    });
                }
            }

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
                {
                    Type: "div", Style: "display:flex; flex-flow:row;",
                    Childs: [
                        {
                            Type: "div", Class: "rv-border-radius-quarter SurroundingShadow",
                            Style: "flex:0 0 auto; padding:0.5rem; background-color:white;" +
                                "margin-" + RV_RevFloat + ":1rem; min-width:12rem;",
                            Childs: [
                                {
                                    Type: "div", Class: "WarmColor",
                                    Style: "text-align:center; font-weight:bold; margin-bottom:1rem;",
                                    Childs: [{ Type: "text", TextValue: RVDic.FieldTypes }]
                                },
                                {
                                    Type: "div", Name: "typesContainer",
                                    Class: "rv-trim-vertical-margins " + that.Options.CssClass.TypesContainer
                                }
                            ]
                        },
                        {
                            Type: "div", Name: "elementsContainer",
                            Class: "rv-border-radius-quarter rv-trim-vertical-margins SurroundingShadow " +
                                that.Options.CssClass.ElementsContainer,
                            Style: "flex:1 1 auto; padding:1rem; background-color:rgb(250,250,250);"
                        }
                    ]
                },
                {
                    Type: "div", Class: "small-12 medium-12 large-12", Style: "margin:1rem 0;",
                    Childs: [{
                        Type: "div", Class: "small-8 medium-6 large-4 rv-air-button rv-circle",
                        Style: "margin:0 auto;", Name: "saveButton",
                        Childs: [{ Type: "text", TextValue: RVDic.Save }]
                    }]
                },
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
                            Childs: [{ Type: "select", Class: "rv-input", Name: "elementTypeSelect", Style: "width:20rem;" }]
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
            that.Interface.ElementsArea = elems["elementsContainer"];
            that.Interface.NewElementTitleArea = elems["newElementTitle"];
            that.Interface.NewElementNameInput = elems["elementNameInput"];
            that.Interface.NewElementHelpArea = elems["helpContainer"];
            that.Interface.ElementTypeSelect = elems["elementTypeSelect"];
            that.Interface.AddElementButton = elems["addElementButton"];

            var rebuild_types = function () {
                elems["typesContainer"].innerHTML = "";

                GlobalUtilities.create_nested_elements(elemTypes.map(options => {
                    return {
                        Type: "div", Class: that.Options.CssClass.DraggableItem, Style: "margin-top:0.5rem;",
                        Properties: [{ Name: "TPData", Value: options }],
                        Childs: [{
                            Type: "div",
                            Class: "rv-border-radius-quarter " + that.Options.CssClass.Draggable + " " +
                                that.Options.CssClass.Placeholder,
                            Style: "display:flex; flex-flow:row; cursor:all-scroll;",
                            Childs: [
                                {
                                    Type: "div",
                                    Class: "rv-border-radius-quarter WarmBackgroundColor rv-ignore-" + RV_RevFloat.toLowerCase() + "-radius",
                                    Style: "flex:0 0 auto; display:flex; align-items:center; justify-content:center;" +
                                        "width:2.5rem; padding:0.5rem;",
                                    Childs: [{
                                        Type: "img", Style: "width:1.2rem; height:1.2rem; fill:white;",
                                        Attributes: [{ Name: "src", Value: GlobalUtilities.icon(options.Icon) }]
                                    }]
                                },
                                {
                                    Type: "div",
                                    Class: "rv-border-radius-quarter SoftBackgroundColor rv-ignore-" + RV_Float.toLowerCase() + "-radius",
                                    Style: "flex:1 1 auto; display:flex; align-items:center; justify-content:center;" +
                                        "padding:0.3rem 1rem; font-size:0.8rem;",
                                    Childs: [{ Type: "text", TextValue: options.Title }]
                                }
                            ]
                        }]
                    };
                }), elems["typesContainer"]);
            };

            rebuild_types();

            setTimeout(() => {
                GlobalUtilities.sortable("." + that.Options.CssClass.TypesContainer + ", ." + that.Options.CssClass.ElementsContainer, {
                    Exchangeable: true,
                    Filters: [{ draggable: that.Options.CssClass.DraggableItem, droppable: that.Options.CssClass.ElementsContainer }],
                    DraggableClass: that.Options.CssClass.Draggable,
                    PlaceholderTarget: that.Options.CssClass.Placeholder,
                    OnDrop: function (e, targetElement) {
                        if (targetElement.parentNode != elems["elementsContainer"]) return;

                        rebuild_types();

                        that.add_element(targetElement, {
                            Type: targetElement.TPData.Name,
                            IsTextInput: targetElement.TPData.ID == "Text"
                        });
                    }
                });
            }, 2000);

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
            };

            elems["saveButton"].onclick = function () {
                var first = elems["elementsContainer"].firstChild;

                var arr = [];
                var privacyData = {};

                while (first) {
                    if (first.GetData && !first.Removed) {
                        var dt = first.GetData(true);

                        if (dt === false) return GlobalUtilities.scroll_into_view(first, {
                            Done: function () {
                                first.Validate();
                                GlobalUtilities.shake(first);
                            }
                        });

                        arr.push(dt);

                        if (dt.Privacy) privacyData = GlobalUtilities.extend(privacyData, dt.Privacy);
                    }

                    first = first.nextSibling;
                }
                return console.log(arr);
                FGAPI.SaveFormElements({
                    FormID: that.Objects.FormID,
                    Elements: Base64.encode(JSON.stringify({ Elements: arr })),
                    ParseResults: true,
                    ResponseHandler: function (result) {
                        console.log(result);
                    }
                });
            };

            that.initialize();
        },

        new_element_container: function () {
            var that = this;

            return GlobalUtilities.create_nested_elements([{
                Type: "div", Class: that.Options.CssClass.DraggableItem, Style: "margin-top:0.5rem;", Name: "_div"
            }])["_div"];
        },

        icon: function (iconName) {
            var that = this;
            return !that.Options.Icons[iconName] ? "" : "svg/" + that.Options.Icons[iconName] + ".svg"
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
                        that.add_element(that.new_element_container(), elements[i]);

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
                            that.add_element(that.new_element_container(), result.Element);

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

        add_element: function (container, params) {
            var that = this;
            params = params || {};

            params.ElementID = params.ElementID || GlobalUtilities.generate_new_guid();

            container.innerHTML = "";
            container.style.marginTop = "1rem";

            params.Title = Base64.decode(params.Title);
            params.Name = Base64.decode(params.Name);
            params.Help = Base64.decode(params.Help);

            var elementId = params.ElementID;
            var title = params.Title;
            var name = params.Name;
            var help = params.Help;
            var type = params.Type;
            var info = JSON.parse(Base64.decode(params.Info) || "{}") || {};
            var weight = params.Weight;
            var sequenceNumber = +params.SequenceNumber;
            if (isNaN(sequenceNumber)) sequenceNumber = 0;
            
            var necessary = params.Necessary === true;
            var uniqueValue = params.UniqueValue === true;

            var isParagraph = (type == "Text") && !params.IsTextInput &&
                !info.UseSimpleEditor && !info.PatternName && !info.Pattern;
            var hasUniqueCheckbox = !isParagraph && ["Text", "Numeric"].some(t => type == t);
            
            var typeName = isParagraph ? RVDic.Paragraph : (RVDic.FG.ElementTypes[type] || RVDic.FG.ElementTypes.Text);
            var iconUrl = that.icon(isParagraph ? "Paragraph" : type);
            
            var _create_checkbox = function (p) {
                p = p || {};

                return {
                    Type: "div",
                    Style: "display:flex; flex-flow:row; align-items:center; margin-bottom:0.5rem;",
                    Childs: [
                        {
                            Type: "div", Class: "rv-flat-label", Style: "flex:1 1 auto;",
                            Childs: [{ Type: "text", TextValue: p.Title + ":" }]
                        },
                        {
                            Type: "switch", Name: p.Name,
                            Style: "flex:0 0 auto; margin-" + RV_RevFloat + ":0.5rem; margin-top:0.2rem;", 
                            Params: { Height: 1, MiniMode: true, Checked: p.Value }
                        }
                    ]
                };
            };

            var elems = GlobalUtilities.create_nested_elements([{
                Type: "div", Class: that.Options.CssClass.Placeholder, Name: "elementDiv",
                Style: "display:flex; flex-flow:row;", 
                Childs: [
                    {
                        Type: "div", Class: "rv-color-soft-warm " + that.Options.CssClass.Draggable,
                        Style: "flex:0 0 auto; display:flex; align-items:center; justify-content:center;" +
                            "cursor:all-scroll; padding:0 0.5rem; padding-" + RV_Float + ":0;",
                        Properties: [{
                            Name: "onclick", Value: function (e) {
                                e.stopPropagation();
                                if (elems["settings"].style.display != "none") elementDiv.Deactive();
                                else elementDiv.Activate(true);
                            }
                        }],
                        Childs: [1, 2].map(() => {
                            return {
                                Type: "i", Class: "fa fa-ellipsis-v", Style: "margin:0 1px;",
                                Attributes: [{ Name: "aria-hidden", Value: true }]
                            };
                        })
                    },
                    {
                        Type: "div",
                        Class: "rv-border-radius-quarter rv-bg-color-trans-white SoftBorder",
                        Style: "flex:1 1 auto; display:flex; flex-flow:row; border-color:rgb(240,240,240); background-color:white;", 
                        Childs: [
                            {
                                Type: "div", Name: "sideColor",
                                Class: "rv-border-radius-quarter rv-ignore-" + RV_RevFloat.toLowerCase() + "-radius SoftBackgroundColor",
                                Style: "flex:0 0 auto; padding-" + RV_Float + ":0.3rem; margin-" + RV_RevFloat + ":0.8rem;"
                            },
                            {
                                Type: "div", Style: "flex:1 1 auto; padding-" + RV_Float + ":0;",
                                Childs: [
                                    {
                                        Type: "div", Class: "small-12 medium-12 large-12",
                                        Style: "display:flex; flex-flow:row;",
                                        Childs: [
                                            {
                                                Type: "div", Name: "titleEditArea",
                                                Style: "flex:1 1 auto; padding:1rem; padding-" + RV_Float + ":0;"
                                            },
                                            { Type: "div", Style: "flex:1 1 auto; display:none;", Name: "removedArea" },
                                            {
                                                Type: "div", Style: "flex:0 0 auto;",
                                                Childs: [{
                                                    Type: "div", Tooltip: typeName,
                                                    Class: "rv-border-radius-quarter WarmBackgroundColor " +
                                                        "rv-ignore-top-" + RV_Float.toLowerCase() + "-radius " +
                                                        "rv-ignore-bottom-" + RV_RevFloat.toLowerCase() + "-radius",
                                                    Style: "flex:0 0 auto; display:flex; align-items:center;" +
                                                        "justify-content:center; padding:0.3rem 0.5rem; opacity:0.8;",
                                                    Childs: [{
                                                        Type: "img", Style: "width:0.8rem; height:0.8rem;",
                                                        Attributes: [{ Name: "src", Value: GlobalUtilities.icon(iconUrl) }]
                                                    }]
                                                }]
                                            }
                                        ]
                                    },
                                    {
                                        Type: "div", Class: "small-12 medium-12 large-12", Name: "settings",
                                        Style: "display:none; padding:1rem; padding-top:0; padding-" + RV_Float + ":0;" +
                                            "padding-" + RV_RevFloat + ":2.8rem;",
                                        Childs: [
                                            {
                                                Type: "div", Class: "small-8 medium-6 large-4", Style: "margin-bottom:0.3rem;",
                                                Childs: [
                                                    (type == "Separator" ? null : _create_checkbox({
                                                        Value: necessary, Title: RVDic.Necessity, Name: "necessaryCheckbox"
                                                    })),
                                                    (!hasUniqueCheckbox ? null : _create_checkbox({
                                                        Value: uniqueValue, Title: RVDic.UniqueValue, Name: "uniqueCheckbox"
                                                    }))
                                                ]
                                            },
                                            { Type: "div", Class: "small-12 medium-12 large-12", Name: "helpEditArea", Style: "margin-top:0.5rem;" },
                                            { Type: "div", Class: "small-12 medium-12 large-12", Style: "margin-top:0.5rem;", Name: "optionsEdit" },
                                            {
                                                Type: "div",
                                                Style: "display:flex; flex-flow:row; align-items:center; justify-content:center; margin:1rem 0;",
                                                Childs: [
                                                    {
                                                        Type: "div", Class: "rv-flat-label",
                                                        Style: "flex:0 0 auto; padding-" + RV_RevFloat + ":0.5rem;",
                                                        Childs: [{ Type: "text", TextValue: RVDic.ID + ":" }]
                                                    },
                                                    {
                                                        Type: "div", Style: "flex:1 1 auto;",
                                                        Childs: [{
                                                            Type: "div", Class: "small-12 medium-8 large-4", Style: "text-align:left; direction:ltr;",
                                                            Childs: [{
                                                                Type: "input", Class: "rv-input-simple rv-placeholder-align-left",
                                                                Style: "width:100%; font-size:0.7rem;", Name: "nameInput", InnerTitle: "e.g. field_id"
                                                            }]
                                                        }]
                                                    }
                                                ]
                                            },
                                            {
                                                Type: "div", Class: "small-12 medium-12 large-12",
                                                Style: "display:flex; flex-flow:row; margin-top:0.5rem;",
                                                Childs: [
                                                    { Type: "div", Style: "flex:1 1 auto;" },
                                                    {
                                                        Type: "div", Name: "privacyButton",
                                                        Style: "flex:0 0 auto; cursor:pointer; font-size:0.7rem; margin-" + RV_RevFloat + ":1rem;",
                                                        Childs: [
                                                            { Type: "i", Class: "fa fa-key fa-lg", Style: "margin-" + RV_RevFloat + ":0.3rem;" },
                                                            { Type: "text", TextValue: RVDic.Privacy }
                                                        ]
                                                    },
                                                    {
                                                        Type: "div", Name: "removeButton",
                                                        Style: "flex:0 0 auto; color:red; cursor:pointer; font-size:0.7rem;",
                                                        Childs: [
                                                            { Type: "i", Class: "fa fa-trash fa-lg", Style: "margin-" + RV_RevFloat + ":0.3rem;" },
                                                            { Type: "text", TextValue: RVDic.Remove }
                                                        ]
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }], container);

            if (!container.parentNode) that.Interface.ElementsArea.appendChild(container);

            var elementDiv = elems["elementDiv"];

            elementDiv.ElementObject = params;

            var removeButton = elems["removeButton"];
            var editButton = elems["editButton"];
            var titleEditArea = elems["titleEditArea"];
            var helpEditArea = elems["helpEditArea"];
            var optionsEditArea = elems["optionsEdit"];

            var optionsEdit = (FormElementTypes[type] || {}).edit ? FormElementTypes[type].edit(info, params) : null;
            if (optionsEdit) optionsEditArea.appendChild(optionsEdit.Container);

            var element = {
                ElementID: elementId, Type: type, Title: title, Name: name, Help: help,
                SequenceNumber: sequenceNumber, Info: info, ContainerDiv: elementDiv,
                TitleInput: null, NameInput: elems["nameInput"], HelpInput: null, SelectOptionsContainer: null
            };

            this.Objects.Elements.push(element);

            element.TitleInput = new AdvancedTextArea({
                ContainerDiv: titleEditArea, DefaultText: RVDic.FieldTitle + ' "' + typeName + '"' + '...',
                QueryTemplate: "RelatedThings",
                ItemTemplate: { ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL" },
                OnLoad: function () {
                    var obj = this;

                    obj.set_data(title);

                    var txtArea = obj.textarea();

                    jQuery(txtArea).on("keyup", function () {
                        var dt = GlobalUtilities.trim(obj.get_data());

                        jQuery(txtArea).css({
                            'direction': GlobalUtilities.textdirection(dt),
                            'background-color': !dt ? "rgb(252, 221, 251)" : "white"
                        });
                    });
                }
            });

            element.HelpInput = new AdvancedTextArea({
                ContainerDiv: helpEditArea, DefaultText: RVDic.Help + "...",
                QueryTemplate: "RelatedThings",
                ItemTemplate: { ItemsTitle: "Items", ID: "ItemID", Name: "Name", Type: "Type", ImageURL: "ImageURL" },
                OnLoad: function () { this.set_data(help); }
            });

            element.NameInput.value = name;

            elementDiv.Activate = function (collapseOthers) {
                if (container.Removed) return;

                jQuery(elementDiv).css({ 'border-color': 'rgb(200,200,200)' });
                elems["sideColor"].classList.remove("SoftBackgroundColor");
                elems["sideColor"].classList.add("WarmBackgroundColor");
                if (elems["settings"].style.display == "none") jQuery(elems["settings"]).animate({ height: 'toggle' });

                if (collapseOthers) {
                    [].forEach.call(that.Interface.ElementsArea.getElementsByClassName(that.Options.CssClass.Placeholder), function (obj) {
                        if ((obj != elementDiv) && obj.Deactive) obj.Deactive();
                    });
                }
            };

            elementDiv.Deactive = function (callback) {
                callback = GlobalUtilities.get_type(callback) == "function" ? callback : function () { };

                jQuery(elementDiv).css({ 'border-color': 'rgb(240,240,240)' });
                elems["sideColor"].classList.remove("WarmBackgroundColor");
                elems["sideColor"].classList.add("SoftBackgroundColor");

                if (elems["settings"].style.display != "none") jQuery(elems["settings"]).animate({ height: 'toggle' }, callback);
                else callback();
            };

            elementDiv.onclick = function () { elementDiv.Activate(true); };

            if (optionsEdit && optionsEdit.Set) optionsEdit.Set(info, weight);

            removeButton.onclick = function (e) {
                e.stopImmediatePropagation();

                container.Removed = true;
                GlobalUtilities.scroll_into_view(container);

                return elementDiv.Deactive(() => {
                    var counter = 15;

                    var set_counter = () => {
                        if (counter < 0) {
                            clearInterval(_int);
                            jQuery(container).animate({ height: 'toggle' }, 500, function () { jQuery(container).remove(); });
                        }
                        else {
                            elems["removedArea"].innerHTML = "";
                            
                            GlobalUtilities.create_nested_elements([{
                                Type: "div", Class: "rv-link", Style: "padding:1rem;",
                                Properties: [{
                                    Name: "onclick",
                                    Value: function () {
                                        container.Removed = false;
                                        clearInterval(_int);

                                        jQuery(elems["removedArea"]).fadeOut(100, function () { jQuery(titleEditArea).fadeIn(200); });

                                        elementDiv.Activate(true);
                                    }
                                }],
                                Childs: [{ Type: "text", TextValue: RVDic.Undo + " " + (counter-- || "0") }]
                            }], elems["removedArea"]);
                        }
                    };

                    var _int = setInterval(set_counter, 1000);
                    set_counter();

                    jQuery(titleEditArea).fadeOut(200, function () { jQuery(elems["removedArea"]).fadeIn(0); });
                });

                GlobalUtilities.confirm(RVDic.Confirms.DoYouWantToRemoveTheField, function (result) {
                    if (result) jQuery(container).animate({ height: 'toggle' }, 500, function () { jQuery(container).remove(); });
                });
            };

            var _on_edit = function () {
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

            var privacyButton = elems["privacyButton"];
            var showed = null;

            var privacyObj = null;

            privacyButton.onclick = function () {
                if (privacyButton.__Div) return (showed = GlobalUtilities.show(privacyButton.__Div));

                var _div = GlobalUtilities.create_nested_elements([{
                    Type: "div", Class: "small-10 medium-8 large-6 rv-border-radius-1 SoftBackgroundColor",
                    Style: "margin:0rem auto; padding:1rem;", Name: "container"
                }])["container"];

                privacyButton.__Div = _div;

                GlobalUtilities.loading(_div);
                showed = GlobalUtilities.show(_div);

                GlobalUtilities.load_files(["PrivacyManager/PermissionSetting.js"], {
                    OnLoad: function () {
                        privacyObj = new PermissionSetting(_div, {
                            ObjectID: elementId,
                            Options: {
                                ConfidentialitySelect: true,
                                PermissionTypes: ["View"],
                                ObjectType: "FormElement",
                                OnSave: function (data) { showed.Close(); },
                                DontSave: true
                            }
                        });
                    }
                });
            };

            container.Validate = function () {
                if ((element.TitleInput || {}).textarea)
                    jQuery(element.TitleInput.textarea()).keyup();
            };

            container.GetData = function (encode) {
                var newTitle = GlobalUtilities.trim(element.TitleInput.get_data());
                var newName = GlobalUtilities.trim(element.NameInput.value);
                var newHelp = GlobalUtilities.trim(element.HelpInput.get_data());
                var newInfo = optionsEdit && optionsEdit.Get ? optionsEdit.Get() : null;
                var newWeight = optionsEdit && optionsEdit.Weight ? optionsEdit.Weight() : null;

                if (!newTitle || (newInfo === false)) return false;

                return {
                    ElementID: elementId,
                    Type: element.Type,
                    Title: encode ? Base64.encode(newTitle) : newTitle,
                    Name: encode ? Base64.encode(newName) : newName,
                    Help: encode ? Base64.encode(newHelp) : newHelp,
                    Info: !newInfo ? null : (encode ? Base64.encode(JSON.stringify(newInfo)) : JSON.stringify(newInfo)),
                    Weight: newWeight,
                    Necessary: elems["necessaryCheckbox"] ? elems["necessaryCheckbox"].Checkbox.checked : null,
                    UniqueValue: elems["uniqueCheckbox"] ? elems["uniqueCheckbox"].Checkbox.checked : null,
                    Privacy: (privacyObj || {}).get_data ? privacyObj.get_data() : null
                };
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