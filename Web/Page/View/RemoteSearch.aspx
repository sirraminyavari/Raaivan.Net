<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RemoteSearch.aspx.cs" Inherits="RaaiVan.Web.Page.View.RemoteSearch"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="contentArea" class="small-12 medium-12 large-12" style="padding:1rem 10vw 5rem 10vw;"></div>

    <script type="text/javascript">
        (function () {
            if (window.RemoteSearch) return;

            window.RemoteSearch = function (container) {
                this.Container = typeof (container) == "object" ? container : document.getElementById(container);
                if (!this.Container) return;

                this.Interface = {
                    ServersList: null,
                    SearchInput: null,
                    Results: null
                };

                this.Objects = {
                    RequestHandlers: []
                };

                var that = this;

                GlobalUtilities.load_files(["API/RAPI.js", "API/UsersAPI.js", "API/SearchAPI.js", "RemoteServers/RemoteServerSettings.js"], {
                    OnLoad: function () { that.preinit(); }
                });
            };

            RemoteSearch.prototype = {
                preinit: function () {
                    var that = this;

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

                    var elems = GlobalUtilities.create_nested_elements([{
                        Type: "div", Style: "display:flex; flex-flow:row;",
                        Childs: [
                            {
                                Type: "div", Class: "rv-border-radius-half SurroundingShadow",
                                Style: "flex:0 0 auto; width:16rem; background-color:white; padding:1rem;" +
                                    "margin-" + RV_RevFloat + ":1rem;",
                                Childs: [
                                    {
                                        Type: "div", Class: "WarmColor", Style: "text-align:center; font-weight:bold;",
                                        Childs: [{ Type: "text", TextValue: RVDic.ServersList }]
                                    },
                                    { Type: "div", Name: "servers" }
                                ]
                            },
                            {
                                Type: "div", Class: "rv-border-radius-half SurroundingShadow",
                                Style: "flex:1 1 auto; padding:1rem; background-color:white;",
                                Childs: [
                                    {
                                        Type: "div", Style: "display:flex; flex-flow:row; align-items:center; margin-bottom:2rem;",
                                        Childs: [
                                            {
                                                Type: "div", Style: "flex:1 1 auto;",
                                                Childs: [{
                                                    Type: "input", Class: "rv-input", Name: "searchInput",
                                                    Style: "width:100%;", Placeholder: RVDic.SearchText
                                                }]
                                            },
                                            {
                                                Type: "div", Class: "rv-border-radius-quarter SoftBorder",
                                                Style: "flex:0 0 auto; display:flex; flex-flow:row; margin-" + RV_Float + ":4rem;" +
                                                    "height:100%; border-color:rgb(240,240,240); padding:0.3rem;",
                                                Childs: [
                                                    {
                                                        Type: "div", Class: "rv-flat-label", Style: "flex:0 0 auto;",
                                                        Childs: [{ Type: "text", TextValue: RVDic.Grouping }]
                                                    },
                                                    {
                                                        Type: "div", Style: "flex:0 0 auto; margin-" + RV_Float + ":0.5rem; padding-top:0.1rem;",
                                                        Childs: [{
                                                            Type: "switch", Name: "grouping",
                                                            Params: {
                                                                Height: 1, MiniMode: true, Checked: true,
                                                                OnChange: function () { that.start_search(); }
                                                            }
                                                        }]
                                                    }
                                                ]
                                            }
                                        ]
                                    },
                                    { Type: "div", Class: "rv-trim-vertical-margins", Name: "results" }
                                ]
                            }
                        ]
                    }], that.Container);

                    that.Interface.SearchInput = elems["searchInput"];
                    that.Interface.GroupSwitch = elems["grouping"].Checkbox;
                    that.Interface.Results = elems["results"];

                    that.Objects.ServersList = new RemoteServerSettings(elems["servers"], {
                        Servers: servers, SelectMode: true, Checkbox: true
                    });

                    GlobalUtilities.set_onchangeorenter(elems["searchInput"], function () {
                        that.start_search();
                    });

                    that.start_search();
                },

                start_search: function () {
                    var that = this;

                    that.Interface.Results.innerHTML = "";

                    var searchText = GlobalUtilities.trim(that.Interface.SearchInput.value);
                    var grouping = that.Interface.GroupSwitch.checked;

                    if (!searchText) {
                        GlobalUtilities.create_nested_elements([{
                            Type: "div", Style: "text-align:center; font-weight:bold; color:rgb(150,150,150); font-size:1.2rem; margin-top:2rem;",
                            Childs: [{ Type: "text", TextValue: RVDic.NothingToDisplay }]
                        }], that.Interface.Results);

                        return;
                    }

                    var items = that.Objects.ServersList.get_checked_items();

                    items.filter(i => !that.Objects.RequestHandlers.some(r => r.Server.ID == i.ID))
                        .forEach(i => {
                            var reqObj = RVRequest.new();

                            reqObj.set_remote_server({
                                BaseURL: Base64.decode(i.URL),
                                UserName: Base64.decode(i.UserName),
                                Password: Base64.decode(i.Password)
                            });

                            that.Objects.RequestHandlers.push({ Server: i, Request: reqObj });
                        });

                    items.map(i => that.Objects.RequestHandlers.filter(r => r.Server.ID == i.ID)[0])
                        .forEach(rh => {
                            that.search({ SearchText: searchText, RequestHandler: rh, LowerBoundary: 0 }, result => {
                                if (grouping) that.add_group_results(rh.Server, result);
                                else that.show_results(that.Interface.Results, result, rh.Server);
                            });
                        });
                },

                search: function (params, callback) {
                    var that = this;
                    params = params || {};

                    var text = GlobalUtilities.trim(params.SearchText || " ").toLowerCase();
                    var requestHandler = params.RequestHandler;
                    var nodeTypeId = params.NodeTypeID || "ntid";
                    var lowerBoundary = params.LowerBoundary;

                    if (!lowerBoundary) lowerBoundary = 0;

                    var dic = that.__RESULTS_DIC = that.__RESULTS_DIC || {};
                    var sIdDic = dic[requestHandler.Server.ID] = dic[requestHandler.Server.ID] || {};
                    var ntIdDic = sIdDic[nodeTypeId] = sIdDic[nodeTypeId] || {};
                    var txtDic = ntIdDic[text] = ntIdDic[text] || {};

                    if (txtDic[lowerBoundary]) return callback(txtDic[lowerBoundary]);

                    SearchAPI.Search({
                        SearchText: Base64.encode(text),
                        ItemTypes: "Node",
                        TypeIDs: params.NodeTypeID,
                        Title: true,
                        Description: true,
                        Content: true,
                        Tags: true,
                        FileContent: true,
                        ForceHasContent: true,
                        SuggestNodeTypes: true,
                        Count: 10,
                        LowerBoundary: lowerBoundary,
                        RequestHandler: requestHandler.Request,
                        ParseResults: true,
                        ResponseHandler: function (result) {
                            txtDic[lowerBoundary] = result;
                            callback(result);
                        }
                    });
                },

                add_group_results: function (server, result) {
                    var that = this;
                    
                    var elems = GlobalUtilities.create_nested_elements([{
                        Type: "div", Class: "rv-border-radius-quarter SoftShadow",
                        Style: "padding:1rem; margin-top:1rem;",
                        Childs: [
                            {
                                Type: "div", Class: "WarmColor", Style: "font-weight:500;",
                                Childs: [{ Type: "text", TextValue: Base64.decode(server.Name) }]
                            },
                            {
                                Type: "div", Style: "font-size:0.7rem; color:rgb(150,150,150);",
                                Childs: [{ Type: "text", TextValue: Base64.decode(server.URL) }]
                            },
                            (!(result.NodeTypes || []).length ? null : {
                                Type: "div", Style: "margin-top:0.5rem;",
                                Childs: result.NodeTypes.map(nt => {
                                    return {
                                        Type: "div", Class: "rv-border-radius-quarter rv-bg-color-white-softer SoftBorder SoftShadow",
                                        Style: "display:inline-block; border-color:rgb(200,200,200); cursor:pointer;" +
                                            "margin-bottom:0.3rem; margin-" + RV_RevFloat + ":0.3rem; font-size:0.7rem; padding:0.1rem 0.5rem;",
                                        Childs: [{ Type: "text", TextValue: Base64.decode(nt.Title) }] //nt.ID
                                    };
                                })
                            }),
                            (!(result.Items || []).length ? null : {
                                Type: "div", Class: "rv-trim-vertical-margins", Name: "results", Style: "margin-top:1rem;",
                                Childs: [{
                                    Type: "div", Style: "text-align:center; color:blue; cursor:pointer;",
                                    Childs: [{ Type: "text", TextValue: RVDic.ViewResults }]
                                }]
                            })
                        ]
                    }], that.Interface.Results);

                    elems["results"].onclick = function () {
                        elems["results"].onclick = null;

                        jQuery(elems["results"]).fadeOut(200, function () {
                            elems["results"].innerHTML = "";

                            that.show_results(elems["results"], result);

                            jQuery(elems["results"]).animate({ 'height': 'toggle' }, 500);
                        });
                    };
                },

                show_results: function (container, result, server) {
                    var that = this;

                    (result.Items || []).forEach(item => that.add_result(container, item, server));
                },

                add_result: function (container, item, server) {
                    var that = this;

                    var elems = GlobalUtilities.create_nested_elements([{
                        Type: "div", Class: "rv-border-radius-quarter rv-bg-color-softer-soft SoftShadow", Name: "container",
                        Style: "padding:0.5rem; margin-top:0.5rem;",
                        Childs: [{
                            Type: "div", Style: "display:flex; flex-flow:row;",
                            Childs: [
                                {
                                    Type: "div", Style: "flex:1 1 auto;",
                                    Childs: [
                                        {
                                            Type: "div", Style: "font-weight:bold;",
                                            Childs: [{ Type: "text", TextValue: Base64.decode(item.Title) }]
                                        },
                                        (!server ? null : {
                                            Type: "div", Style: "margin-top:0.3rem; font-size:0.7rem; color:blue;",
                                            Childs: [{ Type: "text", TextValue: Base64.decode(server.Name) + " - " + Base64.decode(server.URL) }]
                                        }),
                                        (!item.Description ? null : {
                                            Type: "div", Name: "desc",
                                            Style: "margin-top:1rem; font-size:0.7rem; color:rgb(100,100,100);"
                                        })
                                    ]
                                },
                                {
                                    Type: "div", Class: "rv-trim-vertical-margins",
                                    Style: "flex:0 0 auto; display:flex; flex-flow:column; align-items:center; justify-content:center;" +
                                        "width:8rem; margin-" + RV_Float + ":2rem; font-size:0.7rem;",
                                    Childs: [
                                        (!item.AdditionalID ? null : {
                                            Type: "div", Style: "margin-bottom:0.25rem; width:100%; cursor:default; padding:0.1rem 0.3rem;",
                                            Class: "rv-air-button-base rv-air-button-black rv-border-radius-quarter",
                                            Childs: [{ Type: "text", TextValue: Base64.decode(item.AdditionalID) }]
                                        }),
                                        (!item.Type ? null : {
                                            Type: "div", Style: "width:100%; cursor:default; padding:0.1rem 0.3rem;",
                                            Class: "rv-air-button rv-border-radius-quarter",
                                            Childs: [{ Type: "text", TextValue: Base64.decode(item.Type) }]
                                        })
                                    ]
                                }
                            ]
                        }]
                    }], container);
                    
                    if (elems["desc"]) elems["desc"].innerHTML = Base64.decode(item.Description);
                }
            }
        })();
    </script>

    <script type="text/javascript">
        (function () {
            new RemoteSearch("contentArea");

            GlobalUtilities.append_goto_top_button();
        })();
    </script>
</asp:Content>
