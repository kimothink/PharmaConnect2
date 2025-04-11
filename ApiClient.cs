using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;


namespace PharmaConnect2
{
    internal class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private string _accessToken;
        private const string BaseUrl = "https://g-pham.awsome-app.kr/api";

        public ApiClient(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> InsertDosageAsync(PrescriptionDetail dosage)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new InvalidOperationException("Access Token not available. Please call GetTokenAsync first.");
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var signature = GenerateSignature(timestamp, _apiKey);

            // Create request with POST method
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/dosage");

            // Serialize the dosage object to JSON
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var jsonContent = JsonSerializer.Serialize(dosage, jsonOptions);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Add headers
            AddCommonHeaders(request, timestamp, signature);

            // Add token to headers
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");

            // Send request
            var response = await _httpClient.SendAsync(request);

            // Ensure success and return content
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetTokenAsync(string memId, string memPwd)
        {
            // Generate timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Generate signature
            var signature = GenerateSignature(timestamp, _apiKey);

            // Build query parameters
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["mem_id"] = memId;
            queryString["mem_pwd"] = memPwd;

            // Create request
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/v1/token?{queryString}");

            // Add empty content with JSON content type
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            // Add headers
            AddCommonHeaders(request, timestamp, signature);

            // Send request
            var response = await _httpClient.SendAsync(request);

            // Ensure success and return content
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse token from response and save it
            try
            {
                var responseObj = JsonSerializer.Deserialize<ApiResponse<LoginData>>(responseContent);
                if (responseObj?.data?.accessToken != null)
                {
                    _accessToken = responseObj.data.accessToken;
                    Console.WriteLine($"Access Token successfully retrieved: {_accessToken.Substring(0, 20)}...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse token: {ex.Message}");
            }

            return responseContent;
        }
        private string GenerateSignature(string timestamp, string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var data = Encoding.UTF8.GetBytes(timestamp + apiKey);
                var hash = sha256.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }

        private void AddCommonHeaders(HttpRequestMessage request, string timestamp, string signature)
        {
            request.Headers.Add("Cookie", "PHPSESSID=8om4feub2lpa4e6p94sct38crr");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Host", new Uri(BaseUrl).Host);
            request.Headers.Add("User-Agent", "PostmanRuntime/7.43.0");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("x-awapp-apigw-timestamp", timestamp);
            request.Headers.Add("x-awapp-apigw-signature-v2", signature);
        }
    }
}
