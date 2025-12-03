using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model.AssignPost
{
    public class ProductAssignCandidate
    {
        public Guid ProductId { get; set; }
        public int CompanyId { get; set; }
        public int SmallPointId { get; set; }
        public double HaversineKm { get; set; }

        public double RoadKm { get; set; }
    }
}
