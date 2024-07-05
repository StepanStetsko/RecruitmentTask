using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace Recruitment_task.Models.Asset{ 

    public class MarketAsset
    {
        public Paging paging { get; set; }
        public List<AssetData> data { get; set; }
    }
}