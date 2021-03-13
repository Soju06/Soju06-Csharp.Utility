/* ========= Soju06 Web Utility =========
 * NAMESPACE: Soju06.Web.Utility
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 Web Utility ========= */
using Soju06.API;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Soju06.Web.Utility {
    public static class TimeUtility {
        /// <summary>
        /// 서버 시간을 가져옵니다.
        /// </summary>
        /// <param name="url">주소</param>
        /// <returns>UTC 서버 시간 반환</returns>
        public static async Task<DateTime?> GetServerDateTime(string url = "https://www.google.com") {
            try {
                using (var response = await APIClient.GetClient().GetAsync(url))
                    if (response.Headers.Date.HasValue)
                        return response.Headers.Date.Value.UtcDateTime;
            } catch (Exception ex) {
                Debug.WriteLine($"GetServerDateTime exception! {ex}");
                throw ex;
            } return null;
        }
}
}