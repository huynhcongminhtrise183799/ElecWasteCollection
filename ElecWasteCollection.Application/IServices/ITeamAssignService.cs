using ElecWasteCollection.Application.Model;

namespace ElecWasteCollection.Application.IServices
{
    public interface ITeamAssignService
    {
        Task<AssignTeamResult> AssignPostsToTeamsAsync(AssignTeamRequest request);
    }
}
