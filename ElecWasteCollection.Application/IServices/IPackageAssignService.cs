using ElecWasteCollection.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices
{
    public interface IPackageAssignService
    {
        Task<List<RecyclerConfigDto>> GetRecyclerConfigsAsync();
        Task UpdateRecyclerRatiosAsync(List<UpdateRecyclerRatioDto> configs);
        Task<AssignPackageResult> AssignPackagesToRecyclersAsync(List<string> packageIds);
        Task<List<PackageByDateDto>> GetPackagesByDateAsync(DateTime date);
        Task<RecyclingCompanyDailyReportDto> GetAssignedPackagesByCompanyAsync(DateTime date, string companyId);
        Task<List<RecyclingCompanyDto>> GetRecyclingCompaniesAsync();
    }
}
