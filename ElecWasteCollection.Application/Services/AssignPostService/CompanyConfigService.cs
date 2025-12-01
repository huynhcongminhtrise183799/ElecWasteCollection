using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.IServices.IAssignPost;
using ElecWasteCollection.Application.Model.AssignPost;

namespace ElecWasteCollection.Application.Services.AssignPostService
{
    public class CompanyConfigService : ICompanyConfigService
    {
        public CompanyConfigResponse UpdateCompanyConfig(CompanyConfigRequest request)
        {
            if (request == null || request.Teams == null || !request.Teams.Any())
                throw new Exception("Danh sách team không được rỗng.");

            double totalPercent = request.Teams.Sum(t => t.RatioPercent);
            if (Math.Abs(totalPercent - 100) > 0.0001)
                throw new Exception("Tổng % ratio phải bằng 100%.");

            foreach (var team in request.Teams)
            {
                if (team.SmallPoints == null)
                    throw new Exception($"Team {team.TeamId} không có smallPoints.");

                foreach (var sp in team.SmallPoints)
                {
                    if (sp.RadiusKm <= 0)
                        throw new Exception($"SmallPoint {sp.SmallPointId} radiusKm không hợp lệ.");

                    if (sp.MaxRoadDistanceKm <= 0)
                        throw new Exception($"SmallPoint {sp.SmallPointId} maxRoadDistanceKm không hợp lệ.");
                }
            }

            FakeDataSeeder.CompanyConfigs = request.Teams;

            return new CompanyConfigResponse
            {
                Message = "Company configuration updated successfully.",
                Teams = request.Teams
            };
        }

        public CompanyConfigResponse GetCompanyConfig()
        {
            return new CompanyConfigResponse
            {
                Message = "Success",
                Teams = FakeDataSeeder.CompanyConfigs
            };
        }
    }
}
