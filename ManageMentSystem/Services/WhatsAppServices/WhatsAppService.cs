using Newtonsoft.Json;
using System.Text;

namespace ManageMentSystem.Services.WhatsAppServices
{
    public class WhatsAppService : IWhatsAppService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string BASE_URL = "http://localhost:3000";
        private const int DEFAULT_TIMEOUT_SECONDS = 10;
        private const int LONG_TIMEOUT_SECONDS = 30;

        public async Task<(bool IsConnected, bool SessionExists, bool ServerError)> GetSessionStatusAsync(string userId)
        {
            try
            {
                var result = await SendRequestAsync($"session-status/{userId}", userId);

                if (result.Success)
                {
                    var data = JsonConvert.DeserializeObject<dynamic>(result.Content);
                    return (data?.isReady == true, data?.exists == true, false);
                }

                return (false, false, false);
            }
            catch
            {
                return (false, false, true);
            }
        }

        public async Task<(bool Success, dynamic ResponseData, string ErrorMessage)> CreateSessionAsync(string userId)
        {
            var result = await SendRequestAsync("create-session", userId, null, "POST", LONG_TIMEOUT_SECONDS);
            
            if (result.Success)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(result.Content);
                return (true, responseData, null);
            }

            return (false, null, result.ErrorMessage);
        }

        public async Task<(bool Success, string ErrorMessage)> DisconnectSessionAsync(string userId)
        {
            var result = await SendRequestAsync("logout-session", userId, null, "POST", LONG_TIMEOUT_SECONDS);
            return (result.Success, result.ErrorMessage);
        }

        public async Task<(bool Success, string QrCodeContent, string ErrorMessage)> GetQrCodeAsync(string userId)
        {
            var result = await SendRequestAsync($"get-qr/{userId}", userId);
            return (result.Success, result.Content, result.ErrorMessage);
        }

        public async Task<(bool Success, string Content, string ErrorMessage)> SendRequestAsync(
            string endpoint, 
            string userId, 
            object data = null, 
            string method = "GET",
            int timeoutSeconds = DEFAULT_TIMEOUT_SECONDS)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                HttpResponseMessage response;

                if (method.ToUpper() == "POST")
                {
                    var payload = data ?? new { userId };
                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync($"{BASE_URL}/{endpoint}", content, cts.Token);
                }
                else
                {
                    response = await _httpClient.GetAsync($"{BASE_URL}/{endpoint}", cts.Token);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, responseContent, responseContent);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
            }
        }
        public async Task<(bool Success, string Message, string ErrorMessage)> SendInvoicePdfAsync(
            string userId, 
            string phone, 
            string customerName, 
            string message, 
            byte[] pdfBytes, 
            string fileName)
        {
            try
            {
                // We use a new HttpClient here because we need a longer timeout for file uploads
                // and we generally want to avoid blocking the shared client with long operations.
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);

                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(userId), "userId");
                form.Add(new StringContent(phone), "phone");
                form.Add(new StringContent(customerName), "customerName");
                form.Add(new StringContent(message), "message");

                var streamContent = new ByteArrayContent(pdfBytes);
                streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/pdf");
                form.Add(streamContent, "invoice", fileName);

                var response = await client.PostAsync($"{BASE_URL}/send-invoice", form);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "✅ تم إرسال الفاتورة عبر واتساب", null);
                }
                else
                {
                    var errorData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return (false, null, errorData?.message?.ToString() ?? "فشل في الإرسال");
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SendMessageAsync(string userId, string phone, string message)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(2);

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("userId", userId),
                    new KeyValuePair<string, string>("phone", phone),
                    new KeyValuePair<string, string>("message", message)
                });

                var response = await client.PostAsync($"{BASE_URL}/send-message", formData);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return (false, errorData?.error?.ToString() ?? errorData?.message?.ToString() ?? "فشل في الإرسال");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool Success, int SuccessCount, int FailCount, string Message)> SendBulkMessageAsync(string userId, List<string> phones, List<object> customerData, List<string> personalizedMessages)
        {
            try
            {
                if (phones.Count != personalizedMessages.Count)
                {
                    return (false, 0, 0, "عدد الأرقام لا يتطابق مع عدد الرسائل");
                }

                int successCount = 0;
                int failCount = 0;
                var errors = new List<string>();

                // إرسال كل رسالة على حدة لأن السيرفر لا يدعم personalizedMessages في البلك
                for (int i = 0; i < phones.Count; i++)
                {
                    try
                    {
                        using var client = new HttpClient();
                        client.Timeout = TimeSpan.FromMinutes(2);

                        var formData = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("userId", userId),
                            new KeyValuePair<string, string>("phone", phones[i]),
                            new KeyValuePair<string, string>("message", personalizedMessages[i])
                        });

                        var response = await client.PostAsync($"{BASE_URL}/send-message", formData);

                        if (response.IsSuccessStatusCode)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                            var errorContent = await response.Content.ReadAsStringAsync();
                            errors.Add($"فشل إرسال للرقم {phones[i]}: {errorContent}");
                        }

                        // تأخير بسيط بين الرسائل لتجنب الحظر
                        if (i < phones.Count - 1)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errors.Add($"خطأ في إرسال للرقم {phones[i]}: {ex.Message}");
                    }
                }

                var resultMessage = $"✅ تم إرسال الرسالة إلى {successCount} عميل من أصل {phones.Count}";
                if (failCount > 0)
                {
                    resultMessage += $" (فشل: {failCount})";
                }

                return (successCount > 0, successCount, failCount, resultMessage);
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
            }
        }

        public async Task<(bool Success, int SuccessCount, int FailCount, string Message)> SendBulkMessageWithFileAsync(string userId, List<string> phones, List<object> customerData, List<string> personalizedMessages, Stream fileStream, string fileName, string contentType)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);

                var form = new MultipartFormDataContent($"----formdata-{Guid.NewGuid()}");
                form.Add(new StringContent(userId), "userId");
                form.Add(new StringContent(string.Join(",", phones)), "phones");
                form.Add(new StringContent(JsonConvert.SerializeObject(customerData)), "customerData");
                form.Add(new StringContent(JsonConvert.SerializeObject(personalizedMessages)), "personalizedMessages");

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
                form.Add(streamContent, "file", fileName);

                var response = await client.PostAsync($"{BASE_URL}/send-bulk-with-file", form);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    int successCount = 0;
                    int failCount = 0;

                    if (result.summary != null)
                    {
                        successCount = (int)result.summary.success;
                        failCount = (int)result.summary.failed;
                    }

                    return (true, successCount, failCount, $"✅ تم إرسال الرسالة مع الملف إلى {successCount} عميل من أصل {phones.Count}");
                }
                else
                {
                    var errorData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return (false, 0, 0, errorData?.message?.ToString() ?? "فشل في الإرسال");
                }
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
            }
        }
    }
}


