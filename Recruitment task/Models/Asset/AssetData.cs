using MessagePack;
using Recruitment_task.Models.Bars;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruitment_task.Models.Asset{ 

    public class AssetData
    {
        public string id { get; set; }

        public string symbol { get; set; }
        public string kind { get; set; }
        public string description { get; set; }
        public double tickSize { get; set; }
        public string currency { get; set; }
        public string? baseCurrency { get; set; }
        public List<BarsData>? barsData { get; set; }
     
        [NotMapped]
        public Mappings mappings { get; set; }
    }

}