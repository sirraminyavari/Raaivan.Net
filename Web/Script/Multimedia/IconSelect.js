(function () {
    if (window.IconSelect) return;

    window.IconSelect = function (container, params) {
        var that = this;

        GlobalUtilities.load_files(["API/DocsAPI.js"], { OnLoad: function () { that._initialize(container, params); } });
    }

    IconSelect.prototype = {
        _initialize: function (container, params) {
            params = params || {};

            var editable = params.Editable === true;
            var imageUrl = params.IconURL;
            var highQualityImageUrl = params.HighQualityIconURL;
            var iconType = params.IconType;
            var saveWidth = params.SaveWidth || 120;
            var saveHeight = params.SaveHeight || 120;
            var imageWidth = params.ImageWidth || 120;
            var imageHeight = params.ImageHeight || 120;
            var aspectRatio = params.AspectRatio || (imageWidth / imageHeight);
            var circular = params.Circular === true;

            var objectId = params.ObjectID || "";

            var imageDimensions = null;
            var dimensionsVariableName = params.DimensionsVariableName + "_" + objectId;

            var elems = GlobalUtilities.create_nested_elements([{
                Type: "div", Style: "width:" + imageWidth + "px; margin:0px auto 0px auto; text-align:center;",
                Childs: [
                    { Type: "div", Name: "uploadArea" },
                    { Type: "div", Name: "imageArea" }
                ]
            }], container);

            var uploadArea = elems["uploadArea"];
            var imageArea = elems["imageArea"];

            var _set_image = function (newUrl, maxWidth, maxHeight) {
                uploadArea.style.display = "none";
                imageArea.style.display = "block";
                imageArea.innerHTML = "";

                var imageOut = true;

                var elems = GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Style: "text-align:center; width:" + imageWidth + "px; position:relative;",
                        Properties: [{ Name: "onclick", Value: function (e) { e.stopImmediatePropagation(); } }],
                        Childs: [
                            {
                                Type: "div", Style: (editable && highQualityImageUrl ? "" : "display:none;"),
                                Childs: [{
                                    Type: "div", Name: "moveDiv",
                                    Style: "position:absolute; " + RV_Float + ":2px; top:2px; z-index:1; display:none;",
                                    Properties: [
                                        { Name: "onmouseover", Value: function () { this.style.display = elems["changeButton"].style.display = "inline-block"; } },
                                        { Name: "onmouseout", Value: function () { if (imageOut) this.style.display = elems["changeButton"].style.display = "none"; } },
                                        { Name: "onclick", Value: function () { _move_image(); } }
                                    ],
                                    Childs: [{
                                        Type: "img", Style: "cursor:pointer;",
                                        Attributes: [{ Name: "src", Value: GlobalUtilities.icon("Move20.png") }]
                                    }]
                                }]
                            },
                            {
                                Type: "div",
                                Childs: [{
                                    Type: "img",
                                    Class: (circular ? "rv-circle" : "rv-border-radius-quarter") + " " +
                                        (params.ImageClass || " "),
                                    Style: "width:" + imageWidth + "px; height:" + imageHeight + "px; cursor:default;" + (params.ImageStyle || " "),
                                    Attributes: [{ Name: "src", Value: (newUrl || imageUrl) + "?timeStamp=" + (new Date()).getTime() }],
                                    Properties: [
                                        {
                                            Name: "onmouseover",
                                            Value: function () {
                                                elems["changeButton"].style.display = "inline-block";
                                                elems["moveDiv"].style.display = "inline-block";
                                                imageOut = false;
                                            }
                                        },
                                        {
                                            Name: "onmouseout",
                                            Value: function () {
                                                elems["changeButton"].style.display = "none";
                                                elems["moveDiv"].style.display = "none";
                                                imageOut = true;
                                            }
                                        }
                                    ]
                                }]
                            },
                            {
                                Type: "div", Style: (editable ? "" : "display:none;"),
                                Childs: [{
                                    Type: "div", Class: "NormalBorder rv-border-radius-quarter", Name: "changeButton",
                                    Style: "display:none; margin-top:-14px; cursor:pointer; padding:4px; font-size:x-small; font-weight:bold;" +
                                        "color:white; background: rgba(0, 0, 0, 0.5);",
                                    Tooltip: RVDic.Checks.ImageDimensionsMusteBeAtLeastNPixels.replace("[n]", saveWidth).replace("[m]", saveHeight),
                                    Properties: [
                                        { Name: "onmouseover", Value: function () { this.style.display = elems["moveDiv"].style.display = "inline-block"; } },
                                        { Name: "onmouseout", Value: function () { if (imageOut) this.style.display = elems["moveDiv"].style.display = "none"; } },
                                        { Name: "onclick", Value: function (e) { e.stopPropagation(); if (uploader) uploader.browse(); } }
                                    ],
                                    Childs: [{ Type: "text", TextValue: RVDic.NewImage }]
                                }]
                            }
                        ]
                    }
                ], imageArea);
            }

            _set_image();

            var _saveDimensionsVariable = function (newDimensions, callback) {
                RVAPI.SetVariable({
                    Name: dimensionsVariableName, Value: Base64.encode(JSON.stringify(newDimensions)),
                    ParseResults: true,
                    ResponseHandler: function (result) {
                        if (GlobalUtilities.get_type(callback) == "function") callback();
                    }
                });
            }

            var _move_image = function () {
                var _div = GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Class: "SoftBackgroundColor BorderRadius4 NormalPadding", Name: "_div",
                        Style: "width:756px; margin:0px auto 0px auto;"
                    }
                ])["_div"];

                GlobalUtilities.loading(_div);
                var showedDiv = GlobalUtilities.show(_div, { OnClose: function () { if (_imageCrop) delete _imageCrop; } });

                var _imageCrop = null;

                var _create_crop_object = function () {
                    _imageCrop = new ImageCrop(_div, {
                        ImageURL: highQualityImageUrl + "?timeStamp=" + (new Date().getTime()),
                        Dimensions: imageDimensions,
                        AspectRatio: aspectRatio,
                        OnSave: function (dimensions) {
                            _set_image(GlobalUtilities.icon("Loading-Circle.gif"));

                            var _newDimensions = {
                                X: dimensions.X, Y: dimensions.Y,
                                Width: dimensions.Width, Height: dimensions.Height
                            }

                            DocsAPI.CropIcon(GlobalUtilities.extend({}, _newDimensions, {
                                IconID: objectId, Type: iconType,
                                ParseResults: true,
                                ResponseHandler: function (result) {
                                    _saveDimensionsVariable(_newDimensions, function () {
                                        imageDimensions = _newDimensions;
                                        showedDiv.Close();
                                        _set_image();
                                    });
                                }
                            }));
                        }
                    });
                }

                var _get_high_quality_image = function () {
                    if (highQualityImageUrl)
                        _create_crop_object();
                    else {
                        DocsAPI.Icon({
                            IconID: objectId, Type: params.HighQualityIconType, ParseResults: true,
                            ResponseHandler: function (result) {
                                highQualityImageUrl = result.IconURL;
                                _create_crop_object();
                            }
                        });
                    }
                }

                GlobalUtilities.load_files(["Multimedia/ImageCrop.js"], {
                    OnLoad: function () {
                        if (imageDimensions)
                            _get_high_quality_image();
                        else {
                            RVAPI.GetVariable({
                                Name: dimensionsVariableName, ParseResults: true,
                                ResponseHandler: function (result) {
                                    imageDimensions = result.Value ? JSON.parse(Base64.decode(result.Value)) : null;
                                    _get_high_quality_image();
                                }
                            });
                        }
                    }
                });
            }

            var uploader = null;

            var uploadParams = {
                UploadDataSource: DocsAPI.UploadIcon({ IconID: objectId, Type: iconType }),
                OnFileAdd: function () { _set_image(GlobalUtilities.icon("Loading-Circle.gif")); },
                OnUpload: function (file, jsonResponse) {
                    if (jsonResponse.succeess === false || (jsonResponse.Message || {}).ErrorText) {
                        alert(RVDic.MSG[(jsonResponse.Message || {}).ErrorText || "OperationFailed"]);
                        _set_image();
                        return;
                    }

                    uploader.remove(file);

                    imageUrl = jsonResponse.Message.ImageURL;
                    highQualityImageUrl = jsonResponse.Message.HighQualityImageURL;

                    var _newDimensions = {
                        X: jsonResponse.Message.X, Y: jsonResponse.Message.Y,
                        Width: jsonResponse.Message.Width, Height: jsonResponse.Message.Height
                    };

                    _saveDimensionsVariable(_newDimensions, function () {
                        imageDimensions = _newDimensions;
                        _set_image();
                    });
                }
            };

            GlobalUtilities.uploader(uploadArea, uploadParams, function (up) { uploader = up; });
        }
    }
})();