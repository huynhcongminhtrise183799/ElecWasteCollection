using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;

namespace ElecWasteCollection.Application.Services
{
    public class TeamRatioService : ITeamRatioService
    {
        public Task<TeamRatioConfigResponse> UpdateRatios(TeamRatioConfigRequest request)
        {
            double total = request.Teams.Sum(x => x.RatioPercent);
            if (total != 100)
                throw new Exception("Tổng tỷ lệ phải bằng 100%.");

            FakeDataSeeder.TeamRatios = request.Teams;

            return Task.FromResult(new TeamRatioConfigResponse
            {
                Message = "Updated successfully",
                Ratios = request.Teams
            });
        }
    }

}
