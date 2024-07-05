using Microsoft.EntityFrameworkCore;
using Recruitment_task.Data;
using Recruitment_task.Models.Asset;
using Recruitment_task.Models.Bars;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Recruitment_task.Services
{
    public class FetchAssetService
    {
        private readonly AssetContext _assetContext;
        private readonly AccessTokenService _accessTokenServise;
        private readonly FetchBarsServise _fetchBarsServise;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public FetchAssetService(AssetContext assetContext, AccessTokenService accessTokenServise, FetchBarsServise fetchBarsServise, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _assetContext = assetContext;
            _fetchBarsServise = fetchBarsServise;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _accessTokenServise = accessTokenServise;
        }

        public async Task FetchAllAssets(string url)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("Settings:url"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _accessTokenServise.GetTokenAsync());

            await GetHttpResponseAsync(httpClient, url);
        }

        private async Task GetHttpResponseAsync(HttpClient httpClient, string url, int page = 1)
        {
            string assetsUrl = _configuration.GetValue<string>("Settings:assetsUrl");
            string barsUrl = _configuration.GetValue<string>("Settings:barsUrl");

            var response = await httpClient.GetAsync($"{url}{assetsUrl}{page}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _accessTokenServise.GetTokenAsync());
                response = await httpClient.GetAsync($"{url}{assetsUrl}{page}");
            }

            var marketAssetResponse = JsonSerializer.Deserialize<MarketAsset>(await response.Content.ReadAsStringAsync());

            marketAssetResponse.data.ForEach(async item =>
                 item.barsData = await _fetchBarsServise.GetBarsAsync(httpClient, barsUrl, item.id, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"))
                 );

            await AddToDataBaseAsync(marketAssetResponse);

            if (marketAssetResponse.paging.pages > page)
            {
                await GetHttpResponseAsync(httpClient, url, marketAssetResponse.paging.page + 1);
            }
        }

        private async Task AddToDataBaseAsync(MarketAsset market)
        {
            foreach (var item in market.data)
            {
                if (!_assetContext.AssetData.Any(x => x.id == item.id))
                    _assetContext.AssetData.Add(item);
            }

            await _assetContext.SaveChangesAsync();
        }       
    }
}
