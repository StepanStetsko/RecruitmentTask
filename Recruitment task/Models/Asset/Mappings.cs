using Newtonsoft.Json; 
namespace Recruitment_task.Models.Asset{ 

    public class Mappings
    {
        public ActiveTick activetick { get; set; }
        public Simulation simulation { get; set; }
        public Oanda oanda { get; set; }
    }

}