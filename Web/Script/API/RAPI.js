(function () {
    if (window.RAPI) return;

    window.RAPI = {
        _BaseURL: "",
        _Username: "",
        _Password: "",

        Ticket: null,

        init: function (params) {
            RAPI._BaseURL = (params || {}).BaseURL || RAPI._BaseURL;
            RAPI._Username = (params || {}).Username || RAPI._Username;
            RAPI._Password = (params || {}).Password || RAPI._Password;

            RAPI.Ticket = null;
        },

        EndPoints: {
            API: "api/",
            Knowledge: "api/knowledge/"
        },

        get_type: (function () {
            var f = (function () { }).constructor, j = ({}).constructor, a = ([]).constructor,
                s = ("gesi").constructor, n = (2).constructor, b = (true).constructor, t = (new Date()).constructor;

            return function (value) {
                if (value === null) return "null";
                else if (value === undefined) return "undefined";

                switch (value.constructor) {
                    case f: return "function";
                    case j: return "json";
                    case a: return "array";
                    case s: return "string";
                    case n: return "number";
                    case b: return "boolean";
                    case t: return "datetime";
                    default: return String(typeof (value));
                }
            }
        })(),

        _parse: function (input) {
            try { return RAPI.get_type(input) == "json" ? input : JSON.parse(String(input || "{}")); }
            catch (e) { return { error: "json parse error" }; }
        },

        _ajax_request: function (data, callback, params) {
            var url = RAPI._BaseURL;
            if (url) {
                if (url[url.length - 1] != '/') url += '/';
                url += params.Action;
            }

            if (RAPI.get_type(callback) != "function") return;
            data = jQuery.extend(data || {}, { time_stamp: (new Date()).getTime(), Ticket: RAPI.Ticket ? RAPI.Ticket : null });
            params = params || {};

            jQuery[params.Method ? String(params.Method).toLowerCase() : "post"](url, data, function (d) {
                callback(RAPI._parse(d));
            });
        },

        authenticate: function (callback) {
            if ((RAPI.Ticket === false) || RAPI.Ticket) return callback(RAPI.Ticket, true);

            var authenticate = RAPI._ajax_request({ username: RAPI._Username, password: RAPI._Password }, function (d) {
                callback(RAPI.Ticket = (d || {}).Ticket ? d.Ticket : false);
            }, { Action: RAPI.EndPoints.API + "authenticate" });
        },

        send_request: function (data, callback, params) {
            if (RAPI.get_type(callback) != "function") return;

            RAPI.authenticate(function (d, oldTicketUsed) {
                if (d === false) return callback({ error: "authentication failed" });

                RAPI._ajax_request(data, !oldTicketUsed ? callback : function (r) {
                    if ((r || {}).InvalidTicket === true) {
                        RAPI.Ticket = null;
                        RAPI.send_request(data, callback, params);
                    }
                    else callback(r);
                }, params);
            });
        },

        post: function (action, data, callback) {
            RAPI.send_request(data, callback, { Method: "POST", Action: action });
        },

        get: function (action, data, callback) {
            RAPI.send_request(data, callback, { Method: "GET", Action: action });
        },

        get_form: function (data, callback) {
            RAPI.post(RAPI.EndPoints.API + "get_form", data, callback);
        },

        search_form_instances: function (data, callback) {
            RAPI.post(RAPI.EndPoints.API + "search_form_instances", data, callback);
        },

        save_form: function (data, callback) {
            RAPI.post(RAPI.EndPoints.API + "save_form", data, callback);
        },

        get_form_instance: function (data, callback) {
            RAPI.post(RAPI.EndPoints.API + "get_form_instance", data, callback);
        },

        new_node: function (data, callback) {
            RAPI.post(RAPI.EndPoints.API + "new_node", data, callback);
        },

        get_nodes: function (data, callback) {
            RAPI.post(RAPI.EndPoints.API + "get_nodes", data, callback);
        },

        //Knowledge EndPoint

        send_node_to_admin: function (data, callback) {
            RAPI.post(RAPI.EndPoints.Knowledge + "send_to_admin", data, callback);
        }

        //end of Knowledge EndPoint
    }
})();