/* ========= Soju06 Web Json Utility =========
 * NAMESPACE: Soju06.Web.Json
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Web Json Utility ========= */
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Soju06.Web.Json {
    public class JsonUtility {
        public static string Convert(object obj) {
            var serializer = new DataContractJsonSerializer(obj?.GetType());
            using (var ms = new MemoryStream()) {
                serializer.WriteObject(ms, obj);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                return json;
            }
        }

        public static T Convert<T>(string json) => (T)Convert(json, typeof(T));

        public static object Convert(string json, Type type) {
            var serializer = new DataContractJsonSerializer(type);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                return serializer.ReadObject(ms);
        }
    }
}
