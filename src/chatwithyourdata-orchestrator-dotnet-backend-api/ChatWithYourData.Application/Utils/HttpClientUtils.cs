namespace ChatWithYourData.Application.Utils
{
    using Newtonsoft.Json;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text;

    public static class HttpClientUtils
    {
        private static readonly ConcurrentDictionary<Guid, HttpClient> httpClients = new();

        public enum ContentTypeEnum
        {
            Json,
            Xml,
            FormUrlEncoded
        }

        public static string Get(string url, int timeout = 30)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout);
            HttpResponseMessage response = client.GetAsync(route).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Get(string url, string token, int timeout = 30)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout, bearerToken: token);
            HttpResponseMessage response = client.GetAsync(route).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Get(string url, string username, string password, int timeout = 30)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout, apiKeyUser: username, apiKeyPassword: password);
            HttpResponseMessage response = client.GetAsync(route).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Post(string url, dynamic requestObj, int timeout = 30, ContentTypeEnum contentType = ContentTypeEnum.Json)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout);
            HttpContent httpContent = CreateHttpContent(requestObj, contentType);
            HttpResponseMessage response = client.PostAsync(route, httpContent).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Post(string url, dynamic requestObj, string token, int timeout = 30, ContentTypeEnum contentType = ContentTypeEnum.Json)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout, bearerToken: token);
            HttpContent httpContent = CreateHttpContent(requestObj, contentType);
            HttpResponseMessage response = client.PostAsync(route, httpContent).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Post(string url, dynamic requestObj, string username, string password, int timeout = 30, ContentTypeEnum contentType = ContentTypeEnum.Json)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout, apiKeyUser: username, apiKeyPassword: password);
            HttpContent httpContent = CreateHttpContent(requestObj, contentType);
            HttpResponseMessage response = client.PostAsync(route, httpContent).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Put(string url, dynamic requestObj, int timeout = 30, ContentTypeEnum contentType = ContentTypeEnum.Json)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout);
            HttpContent httpContent = CreateHttpContent(requestObj, contentType);
            HttpResponseMessage response = client.PutAsync(route, httpContent).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Delete(string url, int timeout = 30)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout);
            HttpResponseMessage response = client.DeleteAsync(route).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public static string Delete(string url, dynamic requestObj, int timeout = 30, ContentTypeEnum contentType = ContentTypeEnum.Json)
        {
            var (baseAddress, route) = SplitUrl(url);
            HttpClient client = ConfigureHttpClient(baseAddress, timeout);
            HttpContent httpContent = CreateHttpContent(requestObj, contentType);
            HttpRequestMessage requestMessage = new()
            {
                Content = httpContent,
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            HttpResponseMessage response = client.SendAsync(requestMessage).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Divide una URL completa en su dirección base (BaseAddress) y la ruta (Route).
        /// </summary>
        /// <param name="url">La URL completa que se desea dividir.</param>
        /// <returns>Una tupla que contiene la dirección base y la ruta. El primer elemento es la dirección base y el segundo elemento es la ruta.</returns>
        /// <exception cref="UriFormatException">Se lanza si la URL proporcionada no es válida.</exception>
        private static (string BaseAddress, string Route) SplitUrl(string url)
        {
            Uri uri = new Uri(url);

            // Obtener la BaseAddress
            string baseAddress = $"{uri.Scheme}://{uri.Host}";
            if (!uri.IsDefaultPort)
            {
                baseAddress += $":{uri.Port}";
            }

            // Obtener la Route
            string route = uri.PathAndQuery;

            return (baseAddress, route);
        }

        private static HttpClient ConfigureHttpClient(string baseAddress, int timeout, string apiKeyUser = "", string apiKeyPassword = "", string bearerToken = "")
        {
            #region HttpClient

            string hostName = Dns.GetHostName() ?? "localhost";
            Guid uuid = baseAddress.ToGuid();

            HttpClient client;

            if (!httpClients.ContainsKey(uuid))
            {
                client = new HttpClient { BaseAddress = new Uri(baseAddress) };
                client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
                client.Timeout = TimeSpan.FromSeconds(timeout);

                try
                {
                    httpClients.TryAdd(uuid, client);
                }
                catch { }
            }
            else client = httpClients[uuid];

            #endregion

            #region Authorization

            if (!string.IsNullOrEmpty(apiKeyUser) && !string.IsNullOrEmpty(apiKeyPassword))
            {
                client.AddBasicAuthentication(apiKeyUser, apiKeyPassword);
            }
            else if (!string.IsNullOrEmpty(bearerToken))
            {
                client.AddBearerToken(bearerToken);
            }

            #endregion

            return client;
        }

        private static HttpContent CreateHttpContent(dynamic request, ContentTypeEnum contentType)
        {
            if (request == null) return null;

            switch (contentType)
            {
                case ContentTypeEnum.Json:
                    string jsonString = JsonConvert.SerializeObject(request);
                    return new StringContent(jsonString, Encoding.UTF8, "application/json");

                case ContentTypeEnum.Xml:
                    string xmlString = request.ToString();
                    return new StringContent(xmlString, Encoding.UTF8, "application/xml");

                case ContentTypeEnum.FormUrlEncoded:
                    var keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.ToString());
                    return new FormUrlEncodedContent(keyValuePairs);

                default:
                    throw new ArgumentException("Unsupported content type.");
            }
        }

        private static void AddBasicAuthentication(this HttpClient client, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Username and password must be provided for basic authentication.");

            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        private static void AddBearerToken(this HttpClient client, string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token must be provided for bearer authentication.");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
