using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Modules.Jobs
{
    public static class SSO
    {
        private static string SecretKey = "a2263E53c04f1CD00c0cD48DaeddCD4D";
        private static string ClientID = "d22dDDC237eeC2A3136e31BD690b164D";
        private static string APIUserName = "raaivan";

        private static string BaseURL = "https://login.mashhad.ir/ManmailOperation.svc";
        private static string GetLoginKeyURL = BaseURL + "/api/client/loginKey";
        private static string GetCurrentTimeURL = BaseURL + "/api/client/getCurrentTime";
        private static string GetAccessTokenURL = BaseURL + "/api/client/getAccessToken";
        private static string GetUserInfoURL = BaseURL + "/api/user/getUserInfo";

        private static string get_timestamp() {
            Dictionary<string, object> jsonResponse = PublicMethods.fromJSON(
                PublicMethods.web_request(GetCurrentTimeURL, null, HTTPMethod.GET));

            return jsonResponse != null && jsonResponse.ContainsKey("timestamp") ?
                jsonResponse["timestamp"].ToString() : ((long)(PublicMethods.miliseconds() / 1000)).ToString();
        }

        private static NameValueCollection get_headers(string timeStamp)
        {
            NameValueCollection headers = new NameValueCollection();

            headers.Add("apiName", APIUserName);
            headers.Add("requestTime", timeStamp);
            headers.Add("apiSecret", PublicMethods.sha256(SecretKey + timeStamp));

            return headers;
        }

        private static string get_login_key()
        {
            string timeStamp = get_timestamp();

            NameValueCollection values = new NameValueCollection();
            values.Add("time", timeStamp);
            values.Add("hash", PublicMethods.sha256(SecretKey + timeStamp));
            values.Add("clientId", ClientID);

            Dictionary<string, object> jsonResponse = PublicMethods.fromJSON(
                PublicMethods.web_request(GetLoginKeyURL, values, HTTPMethod.PUT, get_headers(timeStamp)));

            return jsonResponse == null || !jsonResponse.ContainsKey("loginKey") ? string.Empty : jsonResponse["loginKey"].ToString();
        }

        private static string get_access_token(string username, string refreshToken)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(refreshToken)) return string.Empty;

            NameValueCollection values = new NameValueCollection();
            values.Add("username", username);
            values.Add("refreshToken", refreshToken);
            values.Add("clientId", ClientID);

            string timeStamp = get_timestamp();

            Dictionary<string, object> jsonResponse = PublicMethods.fromJSON(
                PublicMethods.web_request(GetAccessTokenURL, values, HTTPMethod.PUT, get_headers(timeStamp)));

            return (jsonResponse != null && jsonResponse.ContainsKey("accessToken") ?
                jsonResponse["accessToken"].ToString() : string.Empty).Trim();
        }

        private static string get_user_name(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken)) return string.Empty;

            string timeStamp = get_timestamp();

            NameValueCollection values = new NameValueCollection();
            values.Add("Token", accessToken);

            Dictionary<string, object> jsonResponse = PublicMethods.fromJSON(
                PublicMethods.web_request(GetUserInfoURL, values, HTTPMethod.PUT, get_headers(timeStamp)));

            if (jsonResponse != null && jsonResponse.ContainsKey("basicInfo") &&
                jsonResponse["basicInfo"].GetType() == typeof(Dictionary<string, object>))
                jsonResponse = (Dictionary<string, object>)jsonResponse["basicInfo"];

            return jsonResponse != null && jsonResponse.ContainsKey("username") ?
                jsonResponse["username"].ToString().Trim() : string.Empty;
        }

        public static string get_login_url(Guid applicationId)
        {
            string loginKey = get_login_key();
            return string.IsNullOrEmpty(loginKey) ? string.Empty : "https://login.mashhad.ir/login.aspx?loginkey=" + loginKey;
        }

        public static string get_ticket(Guid applicationId, HttpContext context)
        {
            return context.Request.Params["refresh_token"];
        }

        public static bool validate_ticket(Guid applicationId, HttpContext context, string ticket, ref string validatedUserName)
        {
            string token = get_access_token(context.Request.Params["username"], ticket);
            validatedUserName = get_user_name(token);
            return !string.IsNullOrEmpty(validatedUserName);
        }
    }
}
