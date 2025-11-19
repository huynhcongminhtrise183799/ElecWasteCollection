using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model
{
    public class TeamRatioItem
    {
        public int TeamId { get; set; }
        public double RatioPercent { get; set; }
    }

    public class TeamRatioConfigRequest
    {
        public List<TeamRatioItem> Teams { get; set; }
    }

    public class TeamRatioConfigResponse
    {
        public string Message { get; set; }
        public List<TeamRatioItem> Ratios { get; set; }
    }
}
