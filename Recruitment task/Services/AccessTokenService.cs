using System.Text.Json;

namespace Recruitment_task.Services
{
    public class AccessTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public AccessTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<string> GetTokenAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var tokenUrl = _configuration.GetValue<string>("Settings:accessTokenUrl");

            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", _configuration.GetValue<string>("Settings:username") },
                { "password", _configuration.GetValue<string>("Settings:password") },
                { "client_id", "app-cli" }
            };

            var response = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(requestData));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                if (document.RootElement.TryGetProperty("access_token", out JsonElement accessToken))
                {
                    return accessToken.ToString();
                }
            }
            else
            {
                throw new Exception("Failed to obtain access token.");
            }

            return String.Empty;
        }

    }
}
