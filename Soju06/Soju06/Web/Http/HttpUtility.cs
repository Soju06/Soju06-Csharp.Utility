/* ========= Soju06 Web Http Utility =========
 * NAMESPACE: Soju06.Web.Http
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Web Http Utility ========= */
using System.Collections.Generic;
using System.Text;

namespace Soju06.Web.Http {
    public static class HttpUtility {
        public static string EscapeURL(string s) =>
            System.Net.WebUtility.UrlEncode(s);
    }

    public class HttpQuery : Dictionary<string, string>, IDictionary<string, string> {
        public static HttpQuery ParseQueryString(string query) {
            var Query = new HttpQuery();
            if (query == null || query.Length < 1) return Query;
            if (query[0] == '?') query = query.Remove(0, 1);
            var q = query.Split('&');
            for (int i = 0; i < q.Length; i++) {
                var p = q[i].IndexOf('=');
                if (p == -1) continue;
                var key = q[i].Substring(0, p);
                if (string.IsNullOrWhiteSpace(key)) continue;
                var value = q[i].Substring(p + 1);
                Query.Add(key, string.IsNullOrEmpty(value) ? string.Empty : value);
            }
            return Query;
        }

        public string GetQueryString() {
            var i = 0;
            var sb = new StringBuilder();
            foreach (var value in this) {
                if(i > 0) sb.Append('&'); i++;
                sb.Append($"{HttpUtility.EscapeURL(value.Key)}={HttpUtility.EscapeURL(value.Value)}");
            } return sb.ToString();
        }
    }
}
