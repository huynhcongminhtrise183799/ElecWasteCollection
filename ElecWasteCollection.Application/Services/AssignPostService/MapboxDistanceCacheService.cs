using ElecWasteCollection.Application.IServices.IAssignPost;
using ElecWasteCollection.Application.Helper;
using ElecWasteCollection.Application.Helpers;

namespace ElecWasteCollection.Application.Services.AssignPostService
{
    public class MapboxDistanceCacheService : IMapboxDistanceCacheService
    {
        private readonly MapboxDirectionsClient _client;
        private static readonly Dictionary<string, (double dist, double eta)> _cache = new();

        public MapboxDistanceCacheService(MapboxDirectionsClient client)
        {
            _client = client;
        }

        public async Task<double> GetRoadDistanceKm(double latA, double lngA, double latB, double lngB)
        {
            var (dist, _) = await GetRoadDistanceAndEta(latA, lngA, latB, lngB);
            return dist;
        }

        public async Task<(double distanceKm, double durationMinutes)> GetRoadDistanceAndEta(
            double latA, double lngA, double latB, double lngB)
        {
            string key = $"{latA},{lngA}|{latB},{lngB}";

            if (_cache.ContainsKey(key))
                return _cache[key];

            var route = await _client.GetRouteAsync(latA, lngA, latB, lngB);

            if (route != null && route.Distance > 0)
            {
                double distKm = route.Distance / 1000.0;
                double etaMin = route.Duration / 60.0;

                _cache[key] = (distKm, etaMin);
                return (distKm, etaMin);
            }

            // fallback Haversine
            double fallback = GeoHelper.DistanceKm(latA, lngA, latB, lngB);

            _cache[key] = (fallback, 0);
            return (fallback, 0);
        }
    }
}
