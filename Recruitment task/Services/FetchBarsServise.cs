using Microsoft.EntityFrameworkCore;
using Recruitment_task.Data;
using Recruitment_task.Models.Asset;
using Recruitment_task.Models.Bars;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Recruitment_task.Services
{
    public class FetchBarsServise
    {
        private readonly AssetContext _assetContext;
        private readonly AccessTokenService _accessTokenServise;
        public FetchBarsServise(AssetContext assetContext, AccessTokenService accessTokenServise)
        {
            _assetContext = assetContext;
            _accessTokenServise = accessTokenServise;
        }
        public async Task UpdateBars(AssetData data)
        {
            _assetContext.Entry(data).State = EntityState.Modified;
            await _assetContext.SaveChangesAsync();
        }
        public async Task<List<BarsData>> GetBarsAsync(HttpClient httpClient, string url, string assetId, string date, List<BarsData> allBars = null)
        {
            allBars ??= new List<BarsData>();
            var response = await httpClient.GetAsync($"{url}&date={date}&instrumentId={assetId}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _accessTokenServise.GetTokenAsync());
                response = await httpClient.GetAsync($"{url}&date={date}&instrumentId={assetId}");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine($"{assetId}: 400 Bad Request error");
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                Console.WriteLine($"{assetId}: 500 Internal Server Error");
            }
            else
            {
                response.EnsureSuccessStatusCode();

                var barsResponse = JsonSerializer.Deserialize<Bar>(await response.Content.ReadAsStringAsync());
                allBars.AddRange(barsResponse.data);

                if (barsResponse.data.Count() == 1000)
                {
                    return await GetBarsAsync(httpClient, url, assetId, allBars.First<BarsData>().t.ToString("yyyy-MM-dd hh:mm:ss"), allBars);
                }
            }
            return allBars;
        }

    }
}
