using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Recruitment_task.Data;
using Recruitment_task.Models;
using Recruitment_task.Models.Asset;
using Recruitment_task.Models.DataTransferObject;
using Recruitment_task.Services;

namespace Recruitment_task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketAssetsController : ControllerBase
    {
        private readonly AssetContext _assetContext;
        private readonly FetchAssetService _fetchDataService;
        private readonly WebSocketService _websocketService;
        private readonly IConfiguration _configuration;

        public MarketAssetsController(AssetContext assetContext, FetchAssetService fetchDataService, WebSocketService webSocketService, IConfiguration configuration)
        {
            _assetContext = assetContext;
            _websocketService = webSocketService;
            _fetchDataService = fetchDataService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarketAssetDto>>> GetAssets()
        {
            try
            {
                var url = _configuration.GetValue<string>("Settings:url");
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest("URL is not configured properly.");
                }

                await _fetchDataService.FetchAllAssets(url);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching assets.");
            }
        }

        // GET: api/MarketAssets/assets
        [HttpGet("assets")]
        public async Task<ActionResult<IEnumerable<MarketAssetDto>>> GetMarketAssets()
        {
            try
            {
                var assets = await _assetContext.AssetData
                    .Select(item => new MarketAssetDto
                    {
                        Id = item.id,
                        Symbol = item.symbol
                    }).ToListAsync();

                return Ok(assets);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving market assets.");
            }
        }

        [HttpGet("price")]
        public async Task<ActionResult<IEnumerable<AssetPriceDto>>> GetPrice([FromQuery] List<string> id)
        {
            if (id == null || !id.Any())
            {
                return BadRequest("The id list cannot be null or empty.");
            }

            try
            {
                var assets = await _assetContext.AssetData
                    .Where(item => id.Contains(item.id))
                    .Select(item => new AssetPriceDto
                    {
                        Id = item.id,
                        Symbol = item.symbol,
                        Price = item.barsData.OrderByDescending(x => x.t).FirstOrDefault() != null ? item.barsData.OrderByDescending(x => x.t).FirstOrDefault().c : 0
                    }).ToListAsync();

                if (!assets.Any())
                {
                    return NotFound("No assets found for the provided ids.");
                }

                return Ok(assets);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) for further investigation
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving asset prices.");
            }
        }
    }
}
