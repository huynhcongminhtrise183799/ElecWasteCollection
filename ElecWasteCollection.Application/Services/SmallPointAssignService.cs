using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.Helpers;
using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Domain.Entities;


namespace ElecWasteCollection.Application.Services
{
    public class SmallPointAssignService : ISmallPointAssignService
    {
        public async Task<AssignSmallPointResult> AssignSmallPointsAsync(int teamId)
        {
            var result = new AssignSmallPointResult { TeamId = teamId };

            var teamPoints = FakeDataSeeder.smallCollectionPoints
                .Where(p => p.City_Team_Id == teamId)
                .ToList();

            var posts = FakeDataSeeder.posts
                .Where(p => p.CollectionTeamId == teamId && p.AssignedSmallPointId == null)
                .ToList();

            foreach (var post in posts)
            {
                var user = FakeDataSeeder.users.First(u => u.UserId == post.SenderId);

                double bestDist = double.MaxValue;
                SmallCollectionPoints bestPoint = null;

                foreach (var point in teamPoints)
                {
                    double dist = GeoHelper.DistanceKm(
                        point.Latitude, point.Longitude,
                        user.Iat.Value, user.Ing.Value
                    );

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestPoint = point;
                    }
                }

                if (bestPoint == null) continue;

                post.AssignedSmallPointId = bestPoint.Id;

                result.Assigned.Add(new AssignedSmallPointItem
                {
                    PostId = post.Id,
                    SmallPointId = bestPoint.Id,
                    SmallPointName = bestPoint.Name,
                    DistanceKm = bestDist
                });
            }

            return await Task.FromResult(result);
        }
    }


}
