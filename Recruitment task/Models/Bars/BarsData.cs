using Recruitment_task.Models.Asset;
using System;
namespace Recruitment_task.Models.Bars
{
    public class BarsData
    {
        public int Id { get; set; }
        public DateTime t { get; set; }
        public double o { get; set; }
        public double h { get; set; }
        public double l { get; set; }
        public double c { get; set; }
        public int v { get; set; }
        public string AssetId { get; set; } 
        public AssetData AssetData { get; set; } 
    }

}