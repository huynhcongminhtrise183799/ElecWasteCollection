using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElecWasteCollection.API.Controllers
{
    [ApiController]
    [Route("api/assign")]
    public class AssignController : ControllerBase
    {
        private readonly ITeamAssignService _teamService;
        private readonly ISmallPointAssignService _smallPointService;
        private readonly ITeamRatioService _ratioService;

        public AssignController(
            ITeamAssignService teamService,
            ISmallPointAssignService smallPointService,
            ITeamRatioService ratioService)
        {
            _teamService = teamService;
            _smallPointService = smallPointService;
            _ratioService = ratioService;
        }

        [HttpPost("team-ratio")]
        public async Task<IActionResult> UpdateRatio(TeamRatioConfigRequest req)
            => Ok(await _ratioService.UpdateRatios(req));

        [HttpPost("team")]
        public async Task<IActionResult> AssignTeam(AssignTeamRequest req)
            => Ok(await _teamService.AssignPostsToTeamsAsync(req));

        [HttpPost("smallpoint/{teamId}")]
        public async Task<IActionResult> AssignSmallPoint(int teamId)
            => Ok(await _smallPointService.AssignSmallPointsAsync(teamId));
    }
}
