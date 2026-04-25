namespace ManageMentSystem.Services.WhatsAppServices
{
    public interface IWhatsAppService
    {
        Task<(bool IsConnected, bool SessionExists, bool ServerError)> GetSessionStatusAsync(string userId);
        Task<(bool Success, dynamic ResponseData, string ErrorMessage)> CreateSessionAsync(string userId);
        Task<(bool Success, string ErrorMessage)> DisconnectSessionAsync(string userId);
        Task<(bool Success, string QrCodeContent, string ErrorMessage)> GetQrCodeAsync(string userId);
        Task<(bool Success, string Content, string ErrorMessage)> SendRequestAsync(string endpoint, string userId, object data = null, string method = "GET", int timeoutSeconds = 10);
        Task<(bool Success, string Message, string ErrorMessage)> SendInvoicePdfAsync(string userId, string phone, string customerName, string message, byte[] pdfBytes, string fileName);
        Task<(bool Success, string ErrorMessage)> SendMessageAsync(string userId, string phone, string message);
        Task<(bool Success, int SuccessCount, int FailCount, string Message)> SendBulkMessageAsync(string userId, List<string> phones, List<object> customerData, List<string> personalizedMessages);
        Task<(bool Success, int SuccessCount, int FailCount, string Message)> SendBulkMessageWithFileAsync(string userId, List<string> phones, List<object> customerData, List<string> personalizedMessages, Stream fileStream, string fileName, string contentType);
    }
}
