using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model
{
    public class RecyclerConfigDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double CurrentRatio { get; set; } 
    }

    public class UpdateRecyclerRatioDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public double Ratio { get; set; }
    }
    public class AssignDetailDto
    {
        public string PackageId { get; set; }
        public string AssignedTo { get; set; }
        public string CompanyName { get; set; }
        public string WorkDate { get; set; }
        public string Status { get; set; }
    }

    public class AssignPackageResult
    {
        public int TotalAssigned { get; set; } = 0;
        public int TotalFailed { get; set; } = 0;

        public List<AssignDetailDto> Details { get; set; } = new List<AssignDetailDto>();
    }
    public class PackageByDateDto
    {
        public string PackageId { get; set; }
        public DateTime CreateAt { get; set; }
        public string Status { get; set; }
        public string SmallCollectionPointId { get; set; }
        public string SmallCollectionPointName { get; set; }
        public string SmallCollectionPointAddress { get; set; }
        public string? AssignedRecyclingCompanyId { get; set; }
    }
    public class AssignedPackageDto
    {
        public string PackageId { get; set; }
        public DateTime CreateAt { get; set; }
        public string Status { get; set; }

        public string SmallCollectionPointId { get; set; }
        public string SmallCollectionPointName { get; set; }
        public string SmallCollectionPointAddress { get; set; }
    }
    public class RecyclingCompanyDailyReportDto
    {
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int TotalPackage { get; set; }

        [JsonPropertyName("package")]
        public List<AssignedPackageDto> Packages { get; set; } = new List<AssignedPackageDto>();
    }
    public class RecyclingCompanyDto
    {
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
