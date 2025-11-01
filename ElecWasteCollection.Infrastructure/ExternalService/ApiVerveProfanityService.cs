//using ElecWasteCollection.Application.IServices;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using APIVerve.Api.ProfanityFilter;

//namespace ElecWasteCollection.Infrastructure.ExternalService
//{
//    public class ApiVerveProfanityService : IApiVerveProfanityService
//    {
//        private const string APIKEY = "55c0c029-230d-4dd2-a19d-a4d9401d21e4";

//        // Thêm async vào signature của method
//        public async Task<string> FilterAndCensorAsync(string textToCheck)
//        {
//            var client = new ProfanityFilterClient(APIKEY);

//            // 1. Tạo đối tượng request
//            var request = new ProfanityFilterRequest
//            {
//                Content = textToCheck  // Chú ý: property name có thể là 'Content' thay vì 'content'
//            };

//            try
//            {
//                // 2. Gọi API bất đồng bộ
//                var response = await client.FilterProfanityAsync(request);

//                // 3. Xử lý kết quả
//                if (response != null && response.Status == "ok" && response.Data != null)
//                {
//                    if (response.Data.IsProfane)
//                    {
//                        Console.WriteLine($"⚠️ Phát hiện vi phạm: {string.Join(", ", response.Data.Profanity)}");
//                        return response.Data.Censored;
//                    }
//                    else
//                    {
//                        Console.WriteLine("✅ Nội dung an toàn.");
//                        return textToCheck;
//                    }
//                }
//                else
//                {
//                    Console.WriteLine($"Lỗi API: {response?.Error ?? "Không rõ lỗi."}");
//                    return textToCheck;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Lỗi kết nối hoặc ngoại lệ: {ex.Message}");
//                return textToCheck;
//            }
//        }
//    }
//}