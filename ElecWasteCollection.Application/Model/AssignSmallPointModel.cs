using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model
{
    public class AssignSmallPointResult
    {
        public int TeamId { get; set; }
        public List<AssignedSmallPointItem> Assigned { get; set; } = new();
    }

    public class AssignedSmallPointItem
    {
        public Guid PostId { get; set; }
        public int SmallPointId { get; set; }
        public string SmallPointName { get; set; }
        public double DistanceKm { get; set; }
    }

}
