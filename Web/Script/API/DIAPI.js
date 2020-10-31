if (!window.DIAPI) window.DIAPI = {
    ResponseURL: "../../api/import",

    _send: function (url, params, queryString) {
        if (queryString && (queryString[0] == "&")) queryString = queryString.substring(1);

        if (!params.ResponseHandler) return url + (!queryString ? "" : "&" + queryString);
        else send_post_request(url, queryString, params.ResponseHandler, null, null, null, params.ParseResults, params);
    },

    ImportData: function (params) {
        params = params || {};

        params.Timeout = 120000;

        var url = DIAPI.ResponseURL + "/ImportData?timeStamp=" + new Date().getTime();
        var queryString = (params.AttachedFile ? "&AttachedFile=" + params.AttachedFile : "");

        return DIAPI._send(url, params, queryString);
    }
}