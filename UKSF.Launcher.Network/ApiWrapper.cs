using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UKSF.Launcher.Network {
    public static class ApiWrapper {
        public static string Token;

        public static async Task Login(string email, string password) {
            using (HttpClient client = new HttpClient()) {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(new {email, password}), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{Global.URL}/login", content);
                string responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) {
                    throw new LoginFailedException(JObject.Parse(responseString)["message"].ToString());
                }

                Token = responseString.Replace("\"", "");
            }
        }

        public static async Task<string> Get(string path) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await client.GetAsync($"{Global.URL}/{path}");
                if (!response.IsSuccessStatusCode) {
                    if (response.StatusCode == HttpStatusCode.Unauthorized) {
                        
                    }
                    throw new StatusCodeException(response.StatusCode);
                }
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<Stream> GetFile(string path) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await client.GetAsync($"{Global.URL}/{path}");
                if (!response.IsSuccessStatusCode) {
                    throw new StatusCodeException(response.StatusCode);
                }
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public static async Task<string> Post(string path, object body) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{Global.URL}/{path}", content);
                if (!response.IsSuccessStatusCode) {
                    throw new StatusCodeException(response.StatusCode);
                }
                return await response.Content.ReadAsStringAsync();
            }
        }
    }

    public class LoginFailedException : Exception {
        public readonly string Reason;

        public LoginFailedException(string reason) : base(reason) => Reason = reason;
    }

    public class StatusCodeException : Exception {
        public readonly HttpStatusCode StatusCode;

        public StatusCodeException(HttpStatusCode statusCode) : base($"{statusCode}") => StatusCode = statusCode;
    }
}
