using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model
{
    public class AssignTeamRequest
    {
        public List<Guid> PostIds { get; set; }
        public bool RespectRatio { get; set; } = true;
    }
    public class AssignTeamResult
    {
        public int Processed { get; set; }
        public List<AssignedTeamItem> Assigned { get; set; } = new();
    }

    public class AssignedTeamItem
    {
        public Guid PostId { get; set; }
        public int TeamId { get; set; }
        public double RatioPercent { get; set; }
        public double DistanceKm { get; set; }
        public string Reason { get; set; }
    }
    public class TeamQuotaItem
    {
        public int TeamId { get; set; }
        public double Ratio { get; set; }
        public int Quota { get; set; }
    }

}
