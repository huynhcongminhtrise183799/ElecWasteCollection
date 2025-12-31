using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Domain.Entities;
using ElecWasteCollection.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Services.AssignPackageService
{
    public class PackageAssignService : IPackageAssignService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly string CONFIG_KEY = SystemConfigKey.RECYCLING_ASSIGN_RATIO.ToString();

        public PackageAssignService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<RecyclerConfigDto>> GetRecyclerConfigsAsync()
        {
            var recyclers = await _unitOfWork.Companies.GetAllAsync(c => c.CompanyType == "RecyclingCompany");
            var allConfigs = await _unitOfWork.SystemConfig.GetAllAsync(x => x.Key == CONFIG_KEY);

            var result = new List<RecyclerConfigDto>();

            foreach (var comp in recyclers)
            {
                var cfg = allConfigs.FirstOrDefault(x => x.CompanyId == comp.CompanyId);
                double percent = 0;

                if (cfg != null && double.TryParse(cfg.Value, out double val))
                {
                    percent = val;
                }

                result.Add(new RecyclerConfigDto
                {
                    CompanyId = comp.CompanyId,
                    CompanyName = comp.Name,
                    Address = comp.Address ?? "Chưa cập nhật",
                    CurrentRatio = percent
                });
            }

            return result.OrderBy(x => x.CompanyName).ToList();
        }

        public async Task UpdateRecyclerRatiosAsync(List<UpdateRecyclerRatioDto> configs)
        {
            double total = configs.Sum(c => c.Ratio);
            if (Math.Abs(total - 100) > 0.1)
            {
                throw new Exception($"Lỗi cấu hình: Tổng tỉ lệ phải bằng 100%. Hiện tại là: {total}%");
            }

            var existingConfigs = await _unitOfWork.SystemConfig.GetAllAsync(x => x.Key == CONFIG_KEY);

            foreach (var item in configs)
            {
                var company = await _unitOfWork.Companies.GetAsync(c => c.CompanyId == item.CompanyId);
                if (company == null)
                    throw new Exception($"Không tìm thấy công ty có ID: {item.CompanyId}");

                var record = existingConfigs.FirstOrDefault(x => x.CompanyId == item.CompanyId);

                if (record != null)
                {
                    record.Value = item.Ratio.ToString();
                    _unitOfWork.SystemConfig.Update(record);
                }
                else
                {

                    var newConfig = new SystemConfig
                    {
                        SystemConfigId = Guid.NewGuid(),
                        Key = CONFIG_KEY,
                        Value = item.Ratio.ToString(),
                        CompanyId = item.CompanyId,
                        DisplayName = $"Tỉ lệ phân bổ - {company.Name}",
                        GroupName = "RecyclingAssignment",
                        Status = SystemConfigStatus.Active.ToString(),
                        SmallCollectionPointId = null
                    };
                    await _unitOfWork.SystemConfig.AddAsync(newConfig);
                }
            }
            await _unitOfWork.SaveAsync();
        }
        public async Task<List<PackageByDateDto>> GetPackagesByDateAsync(DateTime date)
        {
            var (startUtc, endUtc) = GetUtcRangeForVietnamDate(date);

            var packages = await _unitOfWork.Packages.GetAllAsync(
                filter: p => p.CreateAt >= startUtc && p.CreateAt <= endUtc,
                includeProperties: "SmallCollectionPoints"
            );

            return MapToDto(packages);
        }

        public async Task<AssignPackageResult> AssignPackagesToRecyclersAsync(List<string> packageIds)
        {
            var result = new AssignPackageResult();
            var recyclerDtos = await GetRecyclerConfigsAsync();

            var activeRecyclers = recyclerDtos.Where(r => r.CurrentRatio > 0).OrderBy(r => r.CompanyId).ToList();

            if (!activeRecyclers.Any())
                throw new Exception("Chưa cấu hình tỷ lệ cho bất kỳ công ty nào. Vui lòng cấu hình trước.");
            var companyNameMap = activeRecyclers.ToDictionary(k => k.CompanyId, v => v.CompanyName);

            var ranges = new List<RangeMap>();
            double currentPivot = 0.0;

            foreach (var r in activeRecyclers)
            {
                var map = new RangeMap
                {
                    CompanyId = r.CompanyId,
                    Ratio = r.CurrentRatio,
                    Min = currentPivot
                };
                currentPivot += (r.CurrentRatio / 100.0);
                map.Max = currentPivot;
                ranges.Add(map);
            }

            var packages = await _unitOfWork.Packages.GetAllAsync(p => packageIds.Contains(p.PackageId));

            foreach (var package in packages)
            {
                try
                {
                    double magicNumber = GetStableHashRatio(package.PackageId);
                    var target = ranges.FirstOrDefault(t => magicNumber >= t.Min && magicNumber < t.Max);

                    if (target == null) target = ranges.Last();

                    package.CompanyId = target.CompanyId;
                    _unitOfWork.Packages.Update(package);

                    string targetCompanyName = companyNameMap.ContainsKey(target.CompanyId)
                                               ? companyNameMap[target.CompanyId]
                                               : "Unknown Company";

                    result.TotalAssigned++;
                    result.Details.Add(new AssignDetailDto
                    {
                        PackageId = package.PackageId,
                        AssignedTo = target.CompanyId,

                        CompanyName = targetCompanyName,
                        WorkDate = DateTime.Now.ToString("yyyy-MM-dd"), 

                        Status = "success"
                    });
                }
                catch (Exception ex)
                {
                    result.TotalFailed++;
                    result.Details.Add(new AssignDetailDto
                    {
                        PackageId = package.PackageId,
                        Status = $"error: {ex.Message}",
                        AssignedTo = "",
                        CompanyName = "",
                        WorkDate = ""
                    });
                }
            }

            await _unitOfWork.SaveAsync();
            return result;
        }

        public async Task<RecyclingCompanyDailyReportDto> GetAssignedPackagesByCompanyAsync(DateTime date, string companyId)
        {
            var company = await _unitOfWork.Companies.GetAsync(c => c.CompanyId == companyId);
            if (company == null)
            {
                throw new Exception($"Không tìm thấy công ty tái chế với ID: {companyId}");
            }
            var (startUtc, endUtc) = GetUtcRangeForVietnamDate(date);

            var packages = await _unitOfWork.Packages.GetAllAsync(
                filter: p => p.CreateAt >= startUtc &&
                             p.CreateAt <= endUtc &&
                             p.CompanyId == companyId,
                includeProperties: "SmallCollectionPoints"
            );

            var result = new RecyclingCompanyDailyReportDto
            {
                CompanyId = company.CompanyId,
                CompanyName = company.Name,
                TotalPackage = packages.Count(),
                Packages = packages.Select(p => new AssignedPackageDto
                {
                    PackageId = p.PackageId,
                    CreateAt = p.CreateAt,
                    Status = p.Status,
                    SmallCollectionPointId = p.SmallCollectionPointsId,
                    SmallCollectionPointName = p.SmallCollectionPoints?.Name ?? "Không xác định",
                    SmallCollectionPointAddress = p.SmallCollectionPoints?.Address ?? "Chưa cập nhật"
                }).ToList()
            };

            return result;
        }
        public async Task<List<RecyclingCompanyDto>> GetRecyclingCompaniesAsync()
        {
            var companies = await _unitOfWork.Companies.GetAllAsync(
                filter: c => c.CompanyType == "RecyclingCompany"
            );

            return companies.Select(c => new RecyclingCompanyDto
            {
                CompanyId = c.CompanyId,
                Name = c.Name,
                Phone = c.Phone,  
                Address = c.Address
            }).ToList();
        }

        private double GetStableHashRatio(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            int hash = id.GetHashCode();
            if (hash < 0) hash = -hash;
            return (hash % 10000) / 10000.0;
        }

        private class RangeMap
        {
            public string CompanyId { get; set; } = string.Empty;
            public double Ratio { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
        }
        private (DateTime StartUtc, DateTime EndUtc) GetUtcRangeForVietnamDate(DateTime date)
        {
            var vnDateStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            var vnDateEnd = vnDateStart.AddDays(1).AddTicks(-1);
            var startUtc = vnDateStart.AddHours(-7);
            var endUtc = vnDateEnd.AddHours(-7);

            return (DateTime.SpecifyKind(startUtc, DateTimeKind.Utc), DateTime.SpecifyKind(endUtc, DateTimeKind.Utc));
        }
        private List<PackageByDateDto> MapToDto(IEnumerable<Packages> packages)
        {
            return packages.Select(p => new PackageByDateDto
            {
                PackageId = p.PackageId,
                CreateAt = p.CreateAt, 
                Status = p.Status,

                SmallCollectionPointId = p.SmallCollectionPointsId,
                SmallCollectionPointName = p.SmallCollectionPoints?.Name ?? "Không xác định",

                SmallCollectionPointAddress = p.SmallCollectionPoints?.Address ?? "Chưa cập nhật",

                AssignedRecyclingCompanyId = p.CompanyId
            }).ToList();
        }
    }
}