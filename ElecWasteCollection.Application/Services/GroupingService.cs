//using System.Text.Json;
//using ElecWasteCollection.Application.Data;
//using ElecWasteCollection.Application.Helpers;
//using ElecWasteCollection.Application.Interfaces;
//using ElecWasteCollection.Application.Model;
//using ElecWasteCollection.Domain.Entities;
//using static ElecWasteCollection.Application.Data.FakeDataSeeder;

//namespace ElecWasteCollection.Application.Services
//{
//    public class GroupingService : IGroupingService
//    {
//        private sealed class TimeSlotDetailDto
//        {
//            public string? StartTime { get; set; }
//            public string? EndTime { get; set; }
//        }

//        private sealed class DailyTimeSlotsDto
//        {
//            public string? DayName { get; set; }
//            public string? PickUpDate { get; set; }
//            public TimeSlotDetailDto? Slots { get; set; }
//        }

//        //  PARSE SCHEDULE
//        private static bool TryGetWindow(
//            string rawSchedule,
//            TimeOnly shiftStart,
//            TimeOnly shiftEnd,
//            out string pickUpDate,
//            out TimeOnly windowStart,
//            out TimeOnly windowEnd)
//        {
//            pickUpDate = "";
//            windowStart = shiftStart;
//            windowEnd = shiftEnd;

//            if (string.IsNullOrWhiteSpace(rawSchedule))
//                return false;

//            try
//            {
//                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//                var parsed = JsonSerializer.Deserialize<List<DailyTimeSlotsDto>>(rawSchedule, opts);
//                var first = parsed?.FirstOrDefault();

//                if (first?.Slots != null &&
//                    TimeOnly.TryParse(first.Slots.StartTime, out var s) &&
//                    TimeOnly.TryParse(first.Slots.EndTime, out var e))
//                {
//                    pickUpDate = first.PickUpDate ?? "";
//                    windowStart = s < shiftStart ? shiftStart : s;
//                    windowEnd = e > shiftEnd ? shiftEnd : e;
//                    return true;
//                }
//            }
//            catch { }

//            return false;
//        }
//        public async Task<GroupingByPointResponse> GroupByCollectionPointAsync(GroupingByPointRequest request)
//        {
//            var point = FakeDataSeeder.smallCollectionPoints
//                .FirstOrDefault(p => p.Id == request.CollectionPointId)
//                ?? throw new Exception("Không tìm thấy trạm thu gom.");

//            double allowedRadius = request.RadiusKm <= 0 ? 10 : request.RadiusKm;

//            var approved = FakeDataSeeder.posts
//                .Where(p =>
//                {
//                    var prod = FakeDataSeeder.products.First(x => x.Id == p.ProductId);
//                    return prod.Status == "Chờ gom nhóm";
//                })
//                .ToList();

//            if (!approved.Any())
//                throw new Exception("Không có bài 'Chờ gom nhóm'.");

//            var availableDates = approved
//                .Select(p =>
//                {
//                    TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
//                        out var dateStr, out _, out _);

//                    return DateOnly.TryParse(dateStr, out var d) ? d : DateOnly.MinValue;
//                })
//                .Where(d => d != DateOnly.MinValue)
//                .Distinct()
//                .OrderBy(d => d)
//                .ToList();

//            var response = new GroupingByPointResponse
//            {
//                CollectionPoint = point.Name,
//                SavedToDatabase = request.SaveResult
//            };

//            HashSet<Guid> assignedPosts = new();
//            int groupCounter = 1;

//            foreach (var workDate in availableDates)
//            {
//                var shiftsToday = FakeDataSeeder.shifts
//                    .Where(s => s.WorkDate == workDate)
//                    .OrderBy(s => s.Shift_Start_Time)
//                    .ToList();

//                if (!shiftsToday.Any())
//                    continue;

//                var postsToday = approved
//                    .Where(p =>
//                    {
//                        TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
//                            out var dateStr, out _, out _);

//                        return DateOnly.TryParse(dateStr, out var d) && d == workDate;
//                    })
//                    .ToList();

//                if (!postsToday.Any()) continue;

//                var sortedPosts = postsToday
//                    .Where(p => !assignedPosts.Contains(p.Id))
//                    .Select(p =>
//                    {
//                        var user = FakeDataSeeder.users.First(u => u.UserId == p.SenderId);
//                        var prod = FakeDataSeeder.products.First(pr => pr.Id == p.ProductId);

//                        TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
//                            out var dateStr, out var st, out var ed);

//                        var sizeTier = FakeDataSeeder.sizeTiers.FirstOrDefault(t => t.SizeTierId == prod.SizeTierId);

//                        double distance = GeoHelper.DistanceKm(
//                            point.Latitude, point.Longitude,
//                            user.Iat ?? point.Latitude,
//                            user.Ing ?? point.Longitude);

//                        return new
//                        {
//                            Post = p,
//                            User = user,
//                            Start = st,
//                            End = ed,
//                            Weight = sizeTier?.EstimatedWeight ?? 10,
//                            Volume = sizeTier?.EstimatedVolume ?? 1,
//                            SizeTierName = sizeTier?.Name ?? "Unknown",
//                            Lat = user.Iat ?? point.Latitude,
//                            Lng = user.Ing ?? point.Longitude,
//                            DistanceFromPoint = distance
//                        };
//                    })
//                    .OrderBy(x => x.Start)
//                    .ThenBy(x => x.DistanceFromPoint)
//                    .ToList();

//                //  XỬ LÝ THEO SHIFT
//                foreach (var shift in shiftsToday)
//                {
//                    var vehicle = FakeDataSeeder.vehicles.First(v => v.Id == shift.Vehicle_Id);
//                    var collector = FakeDataSeeder.collectors.First(c => c.CollectorId == shift.CollectorId);

//                    double maxKg = vehicle.Capacity_Kg;
//                    double maxM3 = vehicle.Capacity_M3;
//                    double curKg = 0;
//                    double curM3 = 0;

//                    TimeOnly shiftStart = TimeOnly.FromDateTime(shift.Shift_Start_Time);
//                    TimeOnly shiftEnd = TimeOnly.FromDateTime(shift.Shift_End_Time);

//                    var selectedForThisShift = new List<dynamic>();

//                    // LỌC BÀI ĐỦ TẢI
//                    foreach (var x in sortedPosts)
//                    {
//                        if (assignedPosts.Contains(x.Post.Id))
//                            continue;

//                        if (curKg + x.Weight > maxKg || curM3 + x.Volume > maxM3)
//                            continue;

//                        if (x.Start > shiftEnd)
//                            continue;

//                        curKg += x.Weight;
//                        curM3 += x.Volume;

//                        selectedForThisShift.Add(x);
//                        assignedPosts.Add(x.Post.Id);
//                    }

//                    if (!selectedForThisShift.Any())
//                        continue;

//                    List<CollectionRoutes> routes = new();
//                    List<RouteDetail> displayRoutes = new();

//                    double curLat = point.Latitude;
//                    double curLng = point.Longitude;

//                    TimeOnly timeCursor = shiftStart;
//                    double speed = vehicle.Vehicle_Type.Contains("lớn") ? 30 : 25;

//                    int order = 1;
//                    var unvisited = new List<dynamic>(selectedForThisShift);

//                    while (unvisited.Any())
//                    {
//                        var next = unvisited
//                            .Where(x =>
//                            {
//                                double travelMin = GeoHelper.DistanceKm(
//                                    curLat, curLng, x.Lat, x.Lng
//                                ) / speed * 60;

//                                var eta = timeCursor.AddMinutes(travelMin);
//                                if (eta < x.Start) eta = x.Start;

//                                return eta <= x.End && eta <= shiftEnd;
//                            })
//                            .OrderBy(x => GeoHelper.DistanceKm(curLat, curLng, x.Lat, x.Lng))
//                            .FirstOrDefault();

//                        if (next == null)
//                            break;

//                        double travel = GeoHelper.DistanceKm(curLat, curLng, next.Lat, next.Lng) / speed * 60;
//                        var etaReal = timeCursor.AddMinutes(travel);
//                        if (etaReal < next.Start) etaReal = next.Start;

//                        displayRoutes.Add(new RouteDetail
//                        {
//                            PickupOrder = order++,
//                            PostId = next.Post.Id,
//                            UserName = next.User.Name,
//                            Address = next.Post.Address,
//                            DistanceKm = Math.Round(next.DistanceFromPoint, 2),
//                            Schedule = next.Post.ScheduleJson,
//                            EstimatedArrival = etaReal.ToString("HH:mm"),
//                            WeightKg = next.Weight,
//                            VolumeM3 = next.Volume,
//                            SizeTier = next.SizeTierName
//                        });

//                        //Update Status Product thành 'Chờ thu gom'

//                        var prodToUpdate = FakeDataSeeder.products.FirstOrDefault(p => p.Id == next.Post.ProductId);
//                        if (prodToUpdate != null)
//                        {
//                            prodToUpdate.Status = "Chờ thu gom";
//                        }

//                        routes.Add(new CollectionRoutes
//                        {
//                            CollectionRouteId = Guid.NewGuid(),
//                            PostId = next.Post.Id,
//                            CollectionDate = workDate,
//                            EstimatedTime = etaReal,
//                            Status = "Chưa bắt đầu",
//                            CollectionGroupId = FakeDataSeeder.collectionGroups.Count + 1  
//                        });

//                        curLat = next.Lat;
//                        curLng = next.Lng;
//                        timeCursor = etaReal.AddMinutes(10);

//                        unvisited.Remove(next);
//                    }

//                    // TẠO GROUP
//                    var group = new CollectionGroups
//                    {
//                        Id = FakeDataSeeder.collectionGroups.Count + 1,
//                        Group_Code = $"GRP-{workDate:MMdd}-{groupCounter++}",
//                        Name = $"{vehicle.Vehicle_Type} - {vehicle.Plate_Number}",
//                        Shift_Id = shift.Id,
//                        Created_At = DateTime.Now
//                    };

//                    foreach (var rt in routes)
//                    {
//                        rt.CollectionGroupId = group.Id;
//                    }

//                    if (request.SaveResult)
//                    {
//                        FakeDataSeeder.collectionGroups.Add(group);
//                        FakeDataSeeder.collectionRoutes.AddRange(routes);
//                    }

//                    response.CreatedGroups.Add(new GroupSummary
//                    {
//                        GroupCode = group.Group_Code,
//                        ShiftId = shift.Id,
//                        Vehicle = $"{vehicle.Plate_Number} ({vehicle.Vehicle_Type})",
//                        Collector = collector.Name,
//                        TotalPosts = routes.Count,
//                        TotalWeightKg = curKg,
//                        TotalVolumeM3 = curM3,
//                        GroupDate = workDate,
//                        Routes = displayRoutes
//                    });
//                }
//            }

//            return await Task.FromResult(response);
//        }
//        public async Task<PreAssignResponse> PreAssignAsync(PreAssignRequest request)
//        {
//            var point = FakeDataSeeder.smallCollectionPoints
//                .FirstOrDefault(p => p.Id == request.CollectionPointId)
//                ?? throw new Exception("Không tìm thấy trạm thu gom.");

//            var approved = FakeDataSeeder.posts
//                .Where(p =>
//                {
//                    var prod = FakeDataSeeder.products.First(x => x.Id == p.ProductId);
//                    return prod.Status == "Chờ gom nhóm";
//                })
//                .ToList();

//            if (!approved.Any())
//                throw new Exception("Không có bài 'Chờ gom nhóm'.");

//            var availableDates = approved
//                .Select(p =>
//                {
//                    TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
//                        out var dateStr, out _, out _);

//                    return DateOnly.TryParse(dateStr, out var d) ? d : DateOnly.MinValue;
//                })
//                .Where(d => d != DateOnly.MinValue)
//                .Distinct()
//                .OrderBy(d => d)
//                .ToList();

//            var response = new PreAssignResponse
//            {
//                CollectionPoint = point.Name,
//                LoadThresholdPercent = request.LoadThresholdPercent
//            };

//            foreach (var date in availableDates)
//            {
//                var postsToday = approved
//                    .Where(p =>
//                    {
//                        TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
//                            out var dateStr, out _, out _);

//                        return DateOnly.TryParse(dateStr, out var d) && d == date;
//                    })
//                    .ToList();

//                var detailed = postsToday
//                    .Select(p =>
//                    {
//                        var user = FakeDataSeeder.users.First(u => u.UserId == p.SenderId);
//                        var prod = FakeDataSeeder.products.First(pr => pr.Id == p.ProductId);
//                        var size = FakeDataSeeder.sizeTiers.FirstOrDefault(t => t.SizeTierId == prod.SizeTierId);

//                        return new PreAssignPost
//                        {
//                            PostId = p.Id,
//                            UserName = user.Name,
//                            Address = user.Address,
//                            Weight = size?.EstimatedWeight ?? 10,
//                            Volume = size?.EstimatedVolume ?? 1
//                        };
//                    })
//                    .ToList();

//                double totalWeight = detailed.Sum(x => x.Weight);
//                double totalVolume = detailed.Sum(x => x.Volume);

//                Vehicles? suggested = FakeDataSeeder.vehicles
//                    .Where(v => v.Status == "active")
//                    .OrderBy(v => v.Capacity_Kg)
//                    .FirstOrDefault(v =>
//                        totalWeight <= v.Capacity_Kg * (request.LoadThresholdPercent / 100.0)
//                    )
//                    ?? FakeDataSeeder.vehicles.OrderBy(v => v.Capacity_Kg).Last();

//                response.Days.Add(new PreAssignDay
//                {
//                    WorkDate = date,
//                    TotalWeight = totalWeight,
//                    TotalVolume = totalVolume,
//                    SuggestedVehicle = suggested,
//                    Posts = detailed
//                });
//            }

//            return await Task.FromResult(response);
//        }

//        public async Task<bool> AssignDayAsync(AssignDayRequest request)
//        {
//            FakeDataSeeder.stagingAssignDays.RemoveAll(s =>
//                s.Date == request.WorkDate &&
//                s.PointId == request.CollectionPointId
//            );

//            FakeDataSeeder.stagingAssignDays.Add(new StagingAssignDay
//            {
//                Date = request.WorkDate,
//                PointId = request.CollectionPointId,
//                VehicleId = request.VehicleId,
//                PostIds = request.PostIds
//            });

//            return await Task.FromResult(true);
//        }

//    }
//}
using System.Text.Json;
using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.Helpers;
using ElecWasteCollection.Application.Interfaces;
using ElecWasteCollection.Application.Model.GroupModel;
using ElecWasteCollection.Domain.Entities;
using static ElecWasteCollection.Application.Data.FakeDataSeeder;

namespace ElecWasteCollection.Application.Services
{
    public class GroupingService : IGroupingService
    {

        private sealed class TimeSlotDetailDto
        {
            public string? StartTime { get; set; }
            public string? EndTime { get; set; }
        }

        private sealed class DailyTimeSlotsDto
        {
            public string? DayName { get; set; }
            public string? PickUpDate { get; set; }
            public TimeSlotDetailDto? Slots { get; set; }
        }

        private static bool TryGetWindow(
            string rawSchedule,
            TimeOnly shiftStart,
            TimeOnly shiftEnd,
            out string pickUpDate,
            out TimeOnly windowStart,
            out TimeOnly windowEnd)
        {
            pickUpDate = "";
            windowStart = shiftStart;
            windowEnd = shiftEnd;

            if (string.IsNullOrWhiteSpace(rawSchedule))
                return false;

            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<List<DailyTimeSlotsDto>>(rawSchedule, opts);
                var first = parsed?.FirstOrDefault();

                if (first?.Slots != null &&
                    TimeOnly.TryParse(first.Slots.StartTime, out var s) &&
                    TimeOnly.TryParse(first.Slots.EndTime, out var e))
                {
                    pickUpDate = first.PickUpDate ?? "";
                    windowStart = s < shiftStart ? shiftStart : s;
                    windowEnd = e > shiftEnd ? shiftEnd : e;
                    return true;
                }
            }
            catch { }

            return false;
        }


        public async Task<PreAssignResponse> PreAssignAsync(PreAssignRequest request)
        {
            var point = FakeDataSeeder.smallCollectionPoints
                .FirstOrDefault(p => p.Id == request.CollectionPointId)
                ?? throw new Exception("Không tìm thấy trạm thu gom.");

            var approved = FakeDataSeeder.posts
                .Where(p =>
                {
                    var prod = FakeDataSeeder.products.First(x => x.Id == p.ProductId);
                    return prod.Status == "Chờ gom nhóm";
                })
                .ToList();

            if (!approved.Any())
                throw new Exception("Không có bài 'Chờ gom nhóm'.");

            var availableDates = approved
                .Select(p =>
                {
                    TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
                        out var dateStr, out _, out _);
                    return DateOnly.TryParse(dateStr, out var d) ? d : DateOnly.MinValue;
                })
                .Where(d => d != DateOnly.MinValue)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var response = new PreAssignResponse
            {
                CollectionPoint = point.Name,
                LoadThresholdPercent = request.LoadThresholdPercent
            };


            // Don Bai
            var tempMap = new Dictionary<DateOnly, List<PreAssignPost>>();

            foreach (var date in availableDates)
            {
                var postsToday = approved
                    .Where(p =>
                    {
                        TryGetWindow(p.ScheduleJson!, TimeOnly.MinValue, TimeOnly.MaxValue,
                            out var dateStr, out _, out _);
                        return DateOnly.TryParse(dateStr, out var d) && d == date;
                    })
                    .ToList();

                var detailed = postsToday.Select(p =>
                {
                    var user = FakeDataSeeder.users.First(u => u.UserId == p.SenderId);
                    var prod = FakeDataSeeder.products.First(pr => pr.Id == p.ProductId);
                    var size = FakeDataSeeder.sizeTiers.FirstOrDefault(t => t.SizeTierId == prod.SizeTierId);

                    return new PreAssignPost
                    {
                        PostId = p.Id,
                        UserName = user.Name,
                        Address = user.Address,
                        Weight = size?.EstimatedWeight ?? 10,
                        Volume = size?.EstimatedVolume ?? 1
                    };
                }).ToList();

                tempMap[date] = detailed;
            }

            // Gộp ngày có tải trọng nhỏ hơn 
            for (int i = 0; i < availableDates.Count - 1; i++)
            {
                var d1 = availableDates[i];
                var d2 = availableDates[i + 1];

                double w1 = tempMap[d1].Sum(x => x.Weight);
                double threshold = FakeDataSeeder.vehicles
                    .OrderBy(v => v.Capacity_Kg)
                    .First().Capacity_Kg * (request.LoadThresholdPercent / 100.0);

                if (w1 < threshold)
                {
                    tempMap[d1].AddRange(tempMap[d2]);
                    tempMap[d2].Clear();
                }
            }

            foreach (var date in availableDates)
            {
                var posts = tempMap[date];
                if (posts.Count == 0) continue;

                double totalWeight = posts.Sum(x => x.Weight);
                double totalVolume = posts.Sum(x => x.Volume);

                var suggested = FakeDataSeeder.vehicles
                    .Where(v => v.Status == "active")
                    .OrderBy(v => v.Capacity_Kg)
                    .FirstOrDefault(v =>
                        totalWeight <= v.Capacity_Kg * (request.LoadThresholdPercent / 100.0)
                    )
                    ?? FakeDataSeeder.vehicles.OrderBy(v => v.Capacity_Kg).Last();

                response.Days.Add(new PreAssignDay
                {
                    WorkDate = date,
                    OriginalPostCount = posts.Count,
                    TotalWeight = totalWeight,
                    TotalVolume = totalVolume,

                    SuggestedVehicle = new SuggestedVehicle
                    {
                        Id = suggested.Id,
                        Plate_Number = suggested.Plate_Number,
                        Vehicle_Type = suggested.Vehicle_Type,
                        Capacity_Kg = suggested.Capacity_Kg,
                        AllowedCapacityKg = suggested.Capacity_Kg * (request.LoadThresholdPercent / 100.0)
                    },

                    Posts = posts
                });
            }

            return await Task.FromResult(response);
        }


        public async Task<bool> AssignDayAsync(AssignDayRequest request)
        {
            FakeDataSeeder.stagingAssignDays.RemoveAll(s =>
                s.Date == request.WorkDate &&
                s.PointId == request.CollectionPointId
            );

            FakeDataSeeder.stagingAssignDays.Add(new StagingAssignDay
            {
                Date = request.WorkDate,
                PointId = request.CollectionPointId,
                VehicleId = request.VehicleId,
                PostIds = request.PostIds
            });

            return await Task.FromResult(true);
        }

        public async Task<GroupingByPointResponse> GroupByCollectionPointAsync(GroupingByPointRequest request)
        {
            var point = FakeDataSeeder.smallCollectionPoints
                .FirstOrDefault(p => p.Id == request.CollectionPointId)
                ?? throw new Exception("Không tìm thấy trạm thu gom.");

            var staging = FakeDataSeeder.stagingAssignDays
                .Where(s => s.PointId == request.CollectionPointId)
                .OrderBy(s => s.Date)
                .ToList();

            if (!staging.Any())
                throw new Exception("Chưa có ngày nào được Assign.");

            var response = new GroupingByPointResponse
            {
                CollectionPoint = point.Name,
                SavedToDatabase = request.SaveResult
            };

            int groupCounter = 1;


            foreach (var assignDay in staging)
            {
                var workDate = assignDay.Date;

                var posts = FakeDataSeeder.posts
                    .Where(p => assignDay.PostIds.Contains(p.Id))
                    .ToList();

                if (!posts.Any())
                    continue;

                var shiftsToday = FakeDataSeeder.shifts
                    .Where(s => s.WorkDate == workDate)
                    .OrderBy(s => s.Shift_Start_Time)
                    .ToList();

                if (!shiftsToday.Any())
                    throw new Exception($"Không có ca làm cho ngày {workDate}");

                var vehicle = FakeDataSeeder.vehicles
                    .FirstOrDefault(v => v.Id == assignDay.VehicleId)
                    ?? throw new Exception("Xe được assign không tồn tại.");


                var sortedPosts = posts
                    .Select(p =>
                    {
                        var user = FakeDataSeeder.users.First(u => u.UserId == p.SenderId);
                        var prod = FakeDataSeeder.products.First(pr => pr.Id == p.ProductId);
                        var sizeTier = FakeDataSeeder.sizeTiers.First(t => t.SizeTierId == prod.SizeTierId);

                        TryGetWindow(
                            p.ScheduleJson!,
                            TimeOnly.MinValue,
                            TimeOnly.MaxValue,
                            out var pickUp,
                            out var st,
                            out var ed);

                        return new
                        {
                            Post = p,
                            User = user,
                            Start = st,
                            End = ed,
                            Weight = sizeTier?.EstimatedWeight ?? 10,
                            Volume = sizeTier?.EstimatedVolume ?? 1,
                            SizeTierName = sizeTier?.Name ?? "Unknown",
                            Lat = user.Iat ?? point.Latitude,
                            Lng = user.Ing ?? point.Longitude,
                            DistanceFromPoint = GeoHelper.DistanceKm(
                                point.Latitude, point.Longitude,
                                user.Iat ?? point.Latitude,
                                user.Ing ?? point.Longitude)
                        };
                    })
                    .OrderBy(x => x.Start)
                    .ThenBy(x => x.DistanceFromPoint)
                    .ToList();

                foreach (var shift in shiftsToday)
                {
                    var collector = FakeDataSeeder.collectors
                        .First(c => c.CollectorId == shift.CollectorId);

                    double maxKg = vehicle.Capacity_Kg;
                    double maxM3 = vehicle.Capacity_M3;

                    TimeOnly shiftStart = TimeOnly.FromDateTime(shift.Shift_Start_Time);
                    TimeOnly shiftEnd = TimeOnly.FromDateTime(shift.Shift_End_Time);

                    double curKg = 0;
                    double curM3 = 0;

                    var selectedForShift = new List<dynamic>();

                    foreach (var x in sortedPosts)
                    {
                        if (curKg + x.Weight > maxKg) continue;
                        if (curM3 + x.Volume > maxM3) continue;
                        if (x.Start > shiftEnd) continue;

                        curKg += x.Weight;
                        curM3 += x.Volume;
                        selectedForShift.Add(x);
                    }

                    if (!selectedForShift.Any()) continue;


                    List<CollectionRoutes> routes = new();
                    List<RouteDetail> displayRoutes = new();

                    double curLat = point.Latitude;
                    double curLng = point.Longitude;

                    TimeOnly timeCursor = shiftStart;
                    double speed = vehicle.Vehicle_Type.Contains("lớn") ? 30 : 25;

                    int order = 1;
                    var unvisited = new List<dynamic>(selectedForShift);

                    while (unvisited.Any())
                    {
                        var next = unvisited
                            .Where(x =>
                            {
                                double travelMin = GeoHelper.DistanceKm(curLat, curLng, x.Lat, x.Lng) / speed * 60;
                                var eta = timeCursor.AddMinutes(travelMin);
                                if (eta < x.Start) eta = x.Start;
                                return eta <= x.End && eta <= shiftEnd;
                            })
                            .OrderBy(x => GeoHelper.DistanceKm(curLat, curLng, x.Lat, x.Lng))
                            .FirstOrDefault();

                        if (next == null)
                            break;

                        double travel = GeoHelper.DistanceKm(curLat, curLng, next.Lat, next.Lng) / speed * 60;

                        var etaReal = timeCursor.AddMinutes(travel);
                        if (etaReal < next.Start) etaReal = next.Start;

                        displayRoutes.Add(new RouteDetail
                        {
                            PickupOrder = order++,
                            PostId = next.Post.Id,
                            UserName = next.User.Name,
                            Address = next.Post.Address,
                            DistanceKm = Math.Round(next.DistanceFromPoint, 2),
                            Schedule = next.Post.ScheduleJson,
                            EstimatedArrival = etaReal.ToString("HH:mm"),
                            WeightKg = next.Weight,
                            VolumeM3 = next.Volume,
                            SizeTier = next.SizeTierName
                        });

                        var prodUpdate = FakeDataSeeder.products.First(p => p.Id == next.Post.ProductId);
                        prodUpdate.Status = "Chờ thu gom";

                        routes.Add(new CollectionRoutes
                        {
                            CollectionRouteId = Guid.NewGuid(),
                            PostId = next.Post.Id,
                            CollectionDate = workDate,
                            EstimatedTime = etaReal,
                            Status = "Chưa bắt đầu",
                            CollectionGroupId = FakeDataSeeder.collectionGroups.Count + 1
                        });

                        curLat = next.Lat;
                        curLng = next.Lng;
                        timeCursor = etaReal.AddMinutes(10);

                        unvisited.Remove(next);
                    }

                    var group = new CollectionGroups
                    {
                        Id = FakeDataSeeder.collectionGroups.Count + 1,
                        Group_Code = $"GRP-{workDate:MMdd}-{groupCounter++}",
                        Name = $"{vehicle.Vehicle_Type} - {vehicle.Plate_Number}",
                        Shift_Id = shift.Id,
                        Created_At = DateTime.Now
                    };

                    foreach (var rt in routes)
                        rt.CollectionGroupId = group.Id;

                    if (request.SaveResult)
                    {
                        FakeDataSeeder.collectionGroups.Add(group);
                        FakeDataSeeder.collectionRoutes.AddRange(routes);
                    }

                    response.CreatedGroups.Add(new GroupSummary
                    {
                        GroupCode = group.Group_Code,
                        ShiftId = shift.Id,
                        Vehicle = $"{vehicle.Plate_Number} ({vehicle.Vehicle_Type})",
                        Collector = collector.Name,
                        GroupDate = workDate,
                        TotalPosts = routes.Count,
                        TotalWeightKg = curKg,
                        TotalVolumeM3 = curM3,
                        Routes = displayRoutes
                    });
                }
            }

            return await Task.FromResult(response);
        }

    }
}
