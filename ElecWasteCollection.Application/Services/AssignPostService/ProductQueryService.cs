using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.Helpers;
using ElecWasteCollection.Application.IServices.IAssignPost;
using ElecWasteCollection.Application.Model.AssignPost;
using System.Linq;
using System.Text.Json;

namespace ElecWasteCollection.Application.Services.AssignPostService
{
    public class ProductQueryService : IProductQueryService
    {
        private readonly IMapboxDistanceCacheService _distance;

        public ProductQueryService(IMapboxDistanceCacheService distance)
        {
            _distance = distance;
        }
        public async Task<GetCompanyProductsResponse> GetCompanyProductsAsync(int companyId, DateOnly workDate)
        {
            var config = FakeDataSeeder.CompanyConfigs
                .FirstOrDefault(c => c.TeamId == companyId)
                ?? throw new Exception("Company not found.");

            var posts = FakeDataSeeder.posts
                .Where(p => p.CollectionCompanyId == companyId)
                .Where(p =>
                {
                    if (!TryParseDates(p.ScheduleJson!, out var list))
                        return false;
                    return list.Contains(workDate);
                })
                .ToList();

            var products = posts
                .Select(p => FakeDataSeeder.products.First(x => x.Id == p.ProductId))
                .ToList();

            var response = new GetCompanyProductsResponse
            {
                CompanyId = companyId,
                CompanyName = $"Team {companyId}",
                WorkDate = workDate.ToString("yyyy-MM-dd"),
                TotalProducts = products.Count
            };

            double totalWeight = 0, totalVolume = 0;

            var grouped = posts.GroupBy(p => p.AssignedSmallPointId);

            foreach (var grp in grouped)
            {
                int spId = grp.Key ?? 0;

                var sp = FakeDataSeeder.smallCollectionPoints.First(s => s.Id == spId);
                var spConfig = config.SmallPoints.First(s => s.SmallPointId == spId);

                var spDto = new SmallPointProductGroupDto
                {
                    SmallPointId = spId,
                    SmallPointName = sp.Name,
                    RadiusMaxConfigKm = spConfig.RadiusKm,
                    MaxRoadDistanceKm = spConfig.MaxRoadDistanceKm

                };

                foreach (var post in grp)
                {
                    var product = FakeDataSeeder.products.First(x => x.Id == post.ProductId);
                    var user = FakeDataSeeder.users.First(u => u.UserId == post.SenderId);
                    var address = FakeDataSeeder.userAddress.First(a => a.UserId == user.UserId);

                    var pv = FakeDataSeeder.productValues.Where(v => v.ProductId == product.Id).ToList();

                    double weight = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_TrongLuong)?.Value ?? 0;
                    double dai = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_ChieuDai)?.Value ?? 0;
                    double rong = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_ChieuRong)?.Value ?? 0;
                    double cao = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_ChieuCao)?.Value ?? 0;

                    double volume = (dai * rong * cao) / 1_000_000.0; 

                    double radiusKm = GeoHelper.DistanceKm(
                        sp.Latitude,
                        sp.Longitude,
                        address.Iat.Value,
                        address.Ing.Value);

                    double roadKm = await _distance.GetRoadDistanceKm(
                        sp.Latitude,
                        sp.Longitude,
                        address.Iat.Value,
                        address.Ing.Value);

                    spDto.Products.Add(new ProductDetailDto
                    {
                        PostId = post.Id,
                        ProductId = product.Id,
                        SenderId = user.UserId,
                        UserName = user.Name,
                        Address = address.Address,
                        WeightKg = weight,
                        VolumeM3 = Math.Round(volume, 3),
                        RadiusKm = $"{Math.Round(radiusKm, 2):0.00} km",
                        RoadKm = $"{Math.Round(roadKm, 2):0.00} km"
                    });

                    spDto.TotalWeightKg += weight;
                    spDto.TotalVolumeM3 = Math.Round(spDto.TotalVolumeM3 + volume, 3);
                }

                spDto.Total = spDto.Products.Count;

                totalWeight += spDto.TotalWeightKg;
                totalVolume = Math.Round(totalVolume + spDto.TotalVolumeM3, 3);

                response.TotalVolumeM3 = totalVolume;
                response.Points.Add(spDto);
            }

            response.TotalWeightKg = totalWeight;
            response.TotalVolumeM3 = Math.Round(totalVolume, 3);

            return response;
        }
        public async Task<SmallPointProductGroupDto> GetSmallPointProductsAsync(int smallPointId, DateOnly workDate)
        {
            var posts = FakeDataSeeder.posts
                .Where(p => p.AssignedSmallPointId == smallPointId)
                .Where(p =>
                {
                    if (!TryParseDates(p.ScheduleJson!, out var list))
                        return false;
                    return list.Contains(workDate);
                })
                .ToList();

            var sp = FakeDataSeeder.smallCollectionPoints.First(s => s.Id == smallPointId);

            var configItem = FakeDataSeeder.CompanyConfigs
                .SelectMany(c => c.SmallPoints)
                .FirstOrDefault(x => x.SmallPointId == smallPointId);

            var spDto = new SmallPointProductGroupDto
            {
                SmallPointId = smallPointId,
                SmallPointName = sp.Name,
                RadiusMaxConfigKm = configItem?.RadiusKm ?? 0,
                MaxRoadDistanceKm = configItem?.MaxRoadDistanceKm ?? 0

            };

            double maxRadius = 0;

            foreach (var post in posts)
            {
                var product = FakeDataSeeder.products.First(p => p.Id == post.ProductId);
                var user = FakeDataSeeder.users.First(u => u.UserId == post.SenderId);
                var address = FakeDataSeeder.userAddress.First(a => a.UserId == user.UserId);

                var pv = FakeDataSeeder.productValues.Where(v => v.ProductId == product.Id).ToList();

                double weight = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_TrongLuong)?.Value ?? 0;
                double dai = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_ChieuDai)?.Value ?? 0;
                double rong = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_ChieuRong)?.Value ?? 0;
                double cao = pv.FirstOrDefault(v => v.AttributeId == FakeDataSeeder.att_ChieuCao)?.Value ?? 0;

                double volume = (dai * rong * cao) / 1_000_000.0;  

                double radiusKm = GeoHelper.DistanceKm(
                    sp.Latitude,
                    sp.Longitude,
                    address.Iat.Value,
                    address.Ing.Value);

                double roadKm = await _distance.GetRoadDistanceKm(
                    sp.Latitude,
                    sp.Longitude,
                    address.Iat.Value,
                    address.Ing.Value);

                maxRadius = Math.Max(maxRadius, radiusKm);

                spDto.Products.Add(new ProductDetailDto
                {
                    PostId = post.Id,
                    ProductId = product.Id,
                    SenderId = post.SenderId,
                    UserName = user.Name,
                    Address = address.Address,
                    WeightKg = weight,
                    VolumeM3 = Math.Round(volume, 3),
                    RadiusKm = $"{Math.Round(radiusKm, 2):0.00} km",
                    RoadKm = $"{Math.Round(roadKm, 2):0.00} km"
                });

                spDto.TotalWeightKg += weight;
                spDto.TotalVolumeM3 = Math.Round(spDto.TotalVolumeM3 + volume, 3);
            }

            spDto.Total = spDto.Products.Count;
            return spDto;
        }

        private bool TryParseDates(string scheduleJson, out List<DateOnly> dates)
        {
            dates = new();
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<ScheduleDayDto>>(scheduleJson, opts);

                if (list == null) return false;

                foreach (var d in list)
                {
                    if (DateOnly.TryParse(d.PickUpDate, out var dt))
                        dates.Add(dt);
                }

                return dates.Any();
            }
            catch
            {
                return false;
            }
        }

        private class ScheduleDayDto
        {
            public string? PickUpDate { get; set; }
            public ScheduleSlotDto? Slots { get; set; }
        }

        private class ScheduleSlotDto
        {
            public string? StartTime { get; set; }
            public string? EndTime { get; set; }
        }

        public async Task<List<CompanyWithPointsResponse>> GetCompaniesWithSmallPointsAsync()
        {
            await Task.Yield();

            return FakeDataSeeder.CompanyConfigs
                .Select(company => new CompanyWithPointsResponse
                {
                    CompanyId = company.TeamId,
                    SmallPoints = FakeDataSeeder.smallCollectionPoints
                        .Where(p => p.City_Team_Id == company.TeamId)
                        .Select(p => new SmallPointDto
                        {
                            SmallPointId = p.Id,
                            Name = p.Name,
                            Lat = p.Latitude,
                            Lng = p.Longitude
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}
