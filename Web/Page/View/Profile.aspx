<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="RaaiVan.Web.Page.View.Profile"
    MasterPageFile="~/Page/Master/TopMaster.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="initialJson" runat="server" ClientIDMode="Static" Value="" />

    <div id="coverContainer" class="small-12 medium-12 large-12" 
        style="position:relative; margin-top:-2.2rem; margin-bottom:1rem; height:18rem;">
    </div>

    <div class="small-12 medium-12 large-12" style="padding:0vw 6vw; margin-bottom:8rem;">
        <div id="tabsArea" class="small-12 medium-12 large-12"></div>

        <div id="socialArea" class="small-12 medium-12 large-12"></div>
        <div id="resumeArea" class="small-12 medium-12 large-12"></div>
        <div id="wikiArea" class="small-12 medium-12 large-12" style="margin-top:1rem;">
            <div id="wikiContent" class="small-12 medium-12 large-12"></div>
        </div>
        <div id="relatedArea" class="small-12 medium-12 large-12"></div>
    </div>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value);
            var modules = (window.RVGlobal || {}).Modules || {};

            //Set document title
            var userFullname = initialJson.User ?
                Base64.decode(initialJson.User.FirstName) + " " + Base64.decode(initialJson.User.LastName) :
                Base64.decode(window.RVGlobal.CurrentUser.FirstName) + " " + Base64.decode(window.RVGlobal.CurrentUser.LastName);

            document.title = userFullname + " - " + document.title;
            //end of Set document title

            //Fill first & last name alert
            if ((initialJson.User.UserID == window.RVGlobal.CurrentUserID) &&
                (!initialJson.User.FirstName || !initialJson.User.LastName)) {
                var showed = GlobalUtilities.show(GlobalUtilities.create_nested_elements([
                    {
                        Type: "div", Class: "small-8 medium-7 large-6 rv-border-radius-1 SoftBackgroundColor",
                        Style: "margin:0 auto; padding:1rem;", Name: "container",
                        Childs: [
                            {
                                Type: "div", Class: "small-12 medium-12 large-12", Style: "text-align:center;",
                                Childs: [{ Type: "text", TextValue: RVDic.MSG.FillYourFirstAndLastName }]
                            },
                            {
                                Type: "div", Class: "small-6 medium-6 large-6 rv-air-button rv-circle",
                                Style: "margin:1rem auto 0 auto;",
                                Properties: [{ Name: "onclick", Value: function () { showed.Close(); } }],
                                Childs: [{ Type: "text", TextValue: RVDic.Confirm }]
                            }
                        ]
                    }
                ])["container"]);
            }
            //end of Fill first & last name alert

            GlobalUtilities.load_files(["USR/CoverPhoto.js"], {
                OnLoad: function () {
                    new CoverPhoto("coverContainer", {
                        UserID: RVGlobal.UserID,
                        CurrentUserID: RVGlobal.CurrentUserID,
                        IsSystemAdmin: RVGlobal.IsSystemAdmin === true,
                        IsOwnPage: initialJson.IsOwnPage === true,
                        FriendRequestSenderUserID: initialJson.FriendRequestSenderUserID,
                        AreFriends: initialJson.AreFriends === true,
                        User: initialJson.User,
                        Modules: modules
                    });
                }
            });
        })();
    </script>

    <script type="text/javascript">
        (function () {
            if (window.PersonalPageInitializer) return;

            window.PersonalPageInitializer = function (params) {
                params = params || {};

                this.Objects = {
                    CurrentPage: null,
                    SharingManager: null,
                    EmploymentTypes: params.EmploymentTypes,
                    PhoneNumberTypes: params.PhoneNumberTypes
                };

                this.Options = {
                    ActiveTab: String(params.ActiveTab).toLowerCase()
                };

                var modules = params.Modules;

                var that = this;

                GlobalUtilities.load_files(["TabsManager/TabsManager.js"], {
                    OnLoad: function () {
                        //Initialize tabs
                        var tabs = [];

                        var resumeNo = 0;

                        if (modules.SocialNetwork) {
                            tabs.push({ Page: "socialArea", Title: RVDic.Social, FixedPage: true,
                                OnActive: function () { that.Objects.CurrentPage = "Social"; that._init_social(); }
                            });

                            ++resumeNo;
                        }

                        tabs.push({ Page: "resumeArea", Title: RVDic.Resume, FixedPage: true,
                            OnActive: function () { that.Objects.CurrentPage = "Resume"; that._init_resume(); }
                        });

                        tabs.push({ Page: "wikiArea", Title: RVDic.AboutMe, FixedPage: true,
                            OnActive: function () { that.Objects.CurrentPage = "Wiki"; that._init_wiki(); }
                        });

                        tabs.push({
                            Page: "relatedArea", Title: RVDic.RelatedNodes, FixedPage: true,
                            OnActive: function () { that.Objects.CurrentPage = "Related"; that._init_related_nodes(); }
                        });

                        new TabsManager({ ContainerDiv: "tabsArea", Pages: tabs }).goto_page(tabs[that.Options.ActiveTab == "resume" ? resumeNo : 0].Page);
                        //end of tabs initialization

                        GlobalUtilities.onscrollend(document, { Offset: 10 }, function () {
                            if (that.Objects.CurrentPage == "Social" && (that.Objects.SharingManager || {}).__Loaded)
                                that.Objects.SharingManager.get_posts();
                        });
                    }
                });
            }

            PersonalPageInitializer.prototype = {
                _init_social: function () {
                    var that = this;

                    if (that.__SocialInited) return;
                    that.__SocialInited = true;

                    GlobalUtilities.loading("socialArea");

                    GlobalUtilities.load_files(["SharingManager/SharingManager.js"], {
                        OnLoad: function () {
                            that.Objects.SharingManager = new SharingManager({
                                Container: "socialArea",
                                OwnerObjectID: RVGlobal.UserID || RVGlobal.CurrentUserID, InitialFill: true,
                                OwnerType: "User", NewPostArea: "Advanced", Permissions: { AddPost: true },
                                EnableImageUpload: true, HidePrivacyOptions: true,
                                OnLoad: function () { that.Objects.SharingManager.__Loaded = true; }
                            });
                        }
                    });
                },

                _init_resume: function () {
                    var that = this;

                    if (that.__ResumeInited) return;
                    that.__ResumeInited = true;

                    GlobalUtilities.loading("resumeArea");

                    GlobalUtilities.load_files(["USR/Resume.js"], {
                        OnLoad: function () {
                            var rs = new Resume("resumeArea", {
                                UserID: RVGlobal.UserID,
                                CurrentUserID: RVGlobal.CurrentUserID,
                                IsSystemAdmin: RVGlobal.IsSystemAdmin === true,
                                Editable: RVGlobal.UserID == RVGlobal.CurrentUserID || RVGlobal.IsSystemAdmin === true,
                                EnableProfileImage: !RVGlobal.Modules.SocialNetwork,
                                EmploymentTypes: that.Objects.EmploymentTypes,
                                PhoneNumberTypes: that.Objects.PhoneNumberTypes
                            });
                        }
                    });
                },
                
                _init_wiki: function () {
                    var that = this;

                    if (that.__WikiInited) return;
                    that.__WikiInited = true;

                    GlobalUtilities.loading("wikiContent");

                    GlobalUtilities.load_files(["Wiki/WikiManager.js", ], {
                        OnLoad: function () {
                            var wm = new WikiManager("wikiContent", {
                                OwnerID: RVGlobal.UserID || RVGlobal.CurrentUserID, OwnerType: "User", Downloadable: true
                            });
                        }
                    });
                },

                _init_related_nodes: function () {
                    var that = this;

                    if (that.__RelatedInited) return;
                    that.__RelatedInited = true;

                    var container = document.getElementById("relatedArea");

                    GlobalUtilities.loading(container);

                    GlobalUtilities.load_files(["CN/RelatedNodesViewer.js"], {
                        OnLoad: function () {
                            new RelatedNodesViewer(container, {
                                ObjectID: RVGlobal.UserID || RVGlobal.CurrentUserID, Editable: false
                            });
                        }
                    });
                }
            }
        })();
    </script>

    <script type="text/javascript">
        (function () {
            var initialJson = JSON.parse(document.getElementById("initialJson").value);

            new PersonalPageInitializer({
                Modules: (window.RVGlobal || {}).Modules,
                ActiveTab: initialJson.ActiveTab,
                EmploymentTypes: initialJson.EmploymentTypes,
                PhoneNumberTypes: initialJson.PhoneNumberTypes
            });
        })();
    </script>
</asp:Content>
