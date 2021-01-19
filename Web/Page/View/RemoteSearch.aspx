<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RemoteSearch.aspx.cs" Inherits="RaaiVan.Web.Page.View.RemoteSearch"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="contentArea" class="small-12 medium-12 large-12" style="padding:1rem 10vw 0 10vw;"></div>

    <script type="text/javascript">
        (function () {
            if (window.RemoteSearch) return;

            window.RemoteSearch = function (container) {
                this.Container = typeof (container) == "object" ? container : document.getElementById(container);
                if (!this.Container) return;

                this.Objects = {
                    RequestHandlers: []
                };

                var that = this;

                GlobalUtilities.load_files(["API/UsersAPI.js", "API/SearchAPI.js", "RemoteServers/RemoteServerSettings.js"], {
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
                                Style: "flex:0 0 auto; width:16rem; background-color:white; padding:1rem;",
                                Childs: [
                                    {
                                        Type: "div", Class: "WarmColor", Style: "text-align:center; font-weight:bold;",
                                        Childs: [{Type: "text", TextValue: "Servers List"}]
                                    },
                                    {Type: "div", Name: "servers"}
                                ]
                            },
                            {
                                Type: "div", Style: "flex:1 1 auto;",
                                Childs: [
                                    {
                                        Type: "div", Class: "small-12 medium-10 large-8", Style: "margin:0 auto;",
                                        Childs: [{
                                            Type: "input", Class: "rv-input", Name: "searchInput"
                                        }]
                                    }
                                ]
                            }
                        ]
                    }], that.Container);

                    var remoteObj = new RemoteServerSettings(elems["servers"], { Servers: servers, SelectMode: true, Checkbox: true });

                    GlobalUtilities.set_onchangeorenter(elems["searchInput"], function () {
                        var searchText = GlobalUtilities.trim(elems["searchInput"].value);

                        if (!searchText) return;

                        var items = remoteObj.get_checked_items();
                        
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
                                SearchAPI.Search({
                                    SearchText: Base64.encode(searchText),
                                    ItemTypes: "Node",
                                    RequestHandler: rh.Request,
                                    ParseResults: true,
                                    ResponseHandler: function (result) {
                                        console.log(result);
                                    }
                                });
                            });
                    });
                }
            };
        })();
    </script>

    <script type="text/javascript">
        (function () {
            new RemoteSearch("contentArea");
        })();
    </script>
</asp:Content>
