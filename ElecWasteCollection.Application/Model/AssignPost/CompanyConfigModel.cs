using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model.AssignPost
{
    public class CompanyConfigRequest
    {
        public List<CompanyConfigItem> Teams { get; set; } = new();
    }
    public class CompanyConfigResponse
    {
        public string Message { get; set; }
        public List<CompanyConfigItem> Teams { get; set; }
    }

    public class CompanyConfigItem
    {
        public int TeamId { get; set; }
        public double RatioPercent { get; set; }
        public List<SmallPointConfigItem> SmallPoints { get; set; } = new();
        public int Quota { get; set; }
    }
}
