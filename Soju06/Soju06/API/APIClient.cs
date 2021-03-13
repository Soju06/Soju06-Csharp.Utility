using System.Net.Http;

namespace Soju06.API {
    public static class APIClient {
        private static HttpClient Client;
        public static HttpClient GetClient() => Client ?? (Client = new HttpClient());
    }
}