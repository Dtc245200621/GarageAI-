using System.Net.Http.Json;
using System.Text.Json;

namespace GarageAI.Services
{
    public class GarageAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GarageAiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _apiKey = Environment.GetEnvironmentVariable(
                "GEMINI_API_KEY") ?? "";
        }

        public async Task<string> SummarizeAsync(
            string repairHistory)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Chưa cấu hình Gemini API Key.";

            if (string.IsNullOrWhiteSpace(repairHistory))
                return "Chưa có lịch sử sửa chữa để tóm tắt.";

            var prompt = $"""
                Bạn là trợ lý AI cho garage sửa chữa ô tô.

                Hãy tóm tắt lịch sử sửa chữa dưới đây
                bằng tiếng Việt, dễ hiểu và ngắn gọn.

                Nội dung cần có:
                1. Những vấn đề xe từng gặp.
                2. Các công việc sửa chữa đã ghi nhận.
                3. Những điều cần lưu ý khi kiểm tra tiếp.
                                Yêu cầu định dạng:
                - Trả lời bằng văn bản thuần tiếng Việt.
                - Không sử dụng Markdown, dấu **, dấu # hoặc dấu *.
                - Chia thành 3 mục đánh số rõ ràng.
                - Phân biệt công việc đã thực hiện với công việc mới được ghi chú.
                - Nếu phiếu đang ở trạng thái Mới tiếp nhận, không khẳng định đã sửa chữa xong.

                Chỉ dựa trên dữ liệu được cung cấp.
                Không tự khẳng định đã sửa chữa hoặc
                thay thế bộ phận nếu dữ liệu không nói rõ.
                Không đưa ra chẩn đoán chắc chắn.

                Dữ liệu lịch sử:
                {repairHistory}
                """;

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            try
            {

                HttpResponseMessage? response = null;

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    using var message = new HttpRequestMessage(
    HttpMethod.Post,
    "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent");

                    message.Headers.Add("x-goog-api-key", _apiKey);
                    message.Content = JsonContent.Create(request);

                    response = await _httpClient.SendAsync(message);

                    bool shouldRetry =
                        (int)response.StatusCode == 429 ||
                        (int)response.StatusCode == 500 ||
                        (int)response.StatusCode == 502 ||
                        (int)response.StatusCode == 503 ||
                        (int)response.StatusCode == 504;

                    if (response.IsSuccessStatusCode ||
                        !shouldRetry ||
                        attempt == 3)
                    {
                        break;
                    }

                    response.Dispose();

                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                }

                if (response == null)
                    return "Không nhận được phản hồi từ Gemini.";



                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("=== GEMINI ERROR ===");
                    Console.WriteLine($"HTTP Status: {(int)response.StatusCode}");
                    Console.WriteLine(errorBody);
                    Console.WriteLine("====================");

                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.TooManyRequests =>
                            "Gemini đang vượt hạn mức yêu cầu. Vui lòng thử lại sau.",

                        System.Net.HttpStatusCode.ServiceUnavailable =>
                            "Gemini đang quá tải hoặc tạm thời không khả dụng. Vui lòng thử lại sau.",

                        System.Net.HttpStatusCode.Unauthorized =>
                            "Không thể xác thực Gemini API Key.",

                        System.Net.HttpStatusCode.Forbidden =>
                            "API Key không có quyền sử dụng dịch vụ Gemini.",

                        System.Net.HttpStatusCode.NotFound =>
                            "Model Gemini hiện tại không khả dụng. Vui lòng kiểm tra cấu hình model.",

                        _ =>
                            $"Gemini API gặp lỗi HTTP {(int)response.StatusCode}."
                    };
                }


                using var json = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                var candidates = json.RootElement
                    .GetProperty("candidates");

                if (candidates.GetArrayLength() == 0)
                    return "AI không trả về nội dung.";

                var parts = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                var result = new List<string>();

                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                        result.Add(text.GetString() ?? "");
                }

                return string.Join("\n", result);
            }
            catch (HttpRequestException)
            {
                return "Không thể kết nối đến Gemini API.";
            }
            catch (TaskCanceledException)
            {
                return "Gemini phản hồi quá thời gian cho phép.";
            }
            catch (JsonException)
            {
                return "Phản hồi Gemini không đúng định dạng.";
            }
            catch (KeyNotFoundException)
            {
                return "Gemini không trả về dữ liệu như mong đợi.";
            }
        }


        public async Task<string> SuggestQuotationAsync(
        string vehicleProblem,
        string serviceInformation)
        {
            var prompt = $"""
        Bạn là trợ lý AI của garage ô tô.

        Dựa trên tình trạng xe và danh mục dịch vụ,
        hãy đề xuất phương án sửa chữa và báo giá tham khảo.

        Tình trạng xe:
        {vehicleProblem}

        Danh mục dịch vụ và giá:
        {serviceInformation}

        Yêu cầu:
        1. Tóm tắt vấn đề của xe.
        2. Đề xuất dịch vụ phù hợp.
        3. Ước tính chi phí từ dữ liệu được cung cấp.
        4. Nêu những điều cần kiểm tra trước khi sửa.

        Không tự tạo giá nếu không có dữ liệu.
        Không khẳng định chẩn đoán khi chưa kiểm tra xe.
        Trả lời bằng tiếng Việt, dễ hiểu.
        """;

            return await AskGeminiAsync(prompt);
        }

        public async Task<string> ExplainServiceAsync(
            string serviceName,
            string description)
        {
            var prompt = $"""
        Bạn là cố vấn dịch vụ garage ô tô.

        Hãy giải thích dịch vụ sau cho khách hàng:

        Tên dịch vụ: {serviceName}
        Mô tả: {description}

        Yêu cầu:
        1. Dịch vụ này dùng để làm gì?
        2. Khi nào xe có thể cần dịch vụ này?
        3. Khách hàng cần lưu ý điều gì?

        Trả lời bằng tiếng Việt, ngắn gọn,
        dễ hiểu, hạn chế thuật ngữ kỹ thuật.
        Không khẳng định xe cần sửa khi chưa kiểm tra.
        """;

            return await AskGeminiAsync(prompt);
        }

        private async Task<string> AskGeminiAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Chưa cấu hình Gemini API Key.";

            try
            {
                HttpResponseMessage? response = null;

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    using var request = new HttpRequestMessage(
    HttpMethod.Post,
    "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent");

                    request.Headers.Add("x-goog-api-key", _apiKey);

                    request.Content = JsonContent.Create(new
                    {
                        contents = new[]
                        {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
                    });

                    response = await _httpClient.SendAsync(request);

                    bool shouldRetry =
                        (int)response.StatusCode == 429 ||
                        (int)response.StatusCode == 500 ||
                        (int)response.StatusCode == 502 ||
                        (int)response.StatusCode == 503 ||
                        (int)response.StatusCode == 504;

                    if (response.IsSuccessStatusCode ||
                        !shouldRetry ||
                        attempt == 3)
                    {
                        break;
                    }

                    response.Dispose();
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                }

                if (response == null)
                    return "Không nhận được phản hồi từ Gemini.";

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("=== GEMINI ERROR ===");
                    Console.WriteLine($"HTTP Status: {(int)response.StatusCode}");
                    Console.WriteLine(errorBody);
                    Console.WriteLine("====================");

                    return $"Gemini API gặp lỗi HTTP {(int)response.StatusCode}.";
                }

                using var json = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                if (!json.RootElement.TryGetProperty(
                        "candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    return "AI không trả về nội dung.";
                }

                var parts = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                var result = new List<string>();

                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                        result.Add(text.GetString() ?? "");
                }

                return string.Join("\n", result);
            }
            catch (HttpRequestException)
            {
                return "Không thể kết nối đến Gemini API.";
            }
            catch (TaskCanceledException)
            {
                return "Gemini phản hồi quá thời gian cho phép.";
            }
            catch (JsonException)
            {
                return "Phản hồi Gemini không đúng định dạng.";
            }
            catch (KeyNotFoundException)
            {
                return "Gemini không trả về dữ liệu như mong đợi.";
            }
        }
    }
}
