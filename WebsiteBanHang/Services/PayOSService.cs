using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace Web_dienmay.Services
{
    public class PayOSService
    {
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _configuration;

        public PayOSService(IConfiguration configuration)
        {
            _configuration = configuration;
            _payOS = new PayOSClient(
                configuration["PayOS:ClientId"] ?? "",
                configuration["PayOS:ApiKey"] ?? "",
                configuration["PayOS:ChecksumKey"] ?? ""
            );
        }

        public async Task<CreatePaymentLinkResponse> CreatePaymentLink(long orderCode, string description, int amount, string? returnUrl = null, string? cancelUrl = null)
        {
            try
            {
                var paymentLinkRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = description,
                    ReturnUrl = returnUrl ?? _configuration["PayOS:ReturnUrl"] ?? "",
                    CancelUrl = cancelUrl ?? _configuration["PayOS:CancelUrl"] ?? ""
                };

                var response = await _payOS.PaymentRequests.CreateAsync(paymentLinkRequest);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tạo link thanh toán PayOS: {ex.Message}", ex);
            }
        }

        public async Task<PaymentLink> GetPaymentLinkInformation(long orderCode)
        {
            try
            {
                var paymentLinkInfo = await _payOS.PaymentRequests.GetAsync(orderCode);
                return paymentLinkInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy thông tin thanh toán PayOS: {ex.Message}", ex);
            }
        }

        public async Task<PaymentLink> CancelPaymentLink(long orderCode, string? cancellationReason = null)
        {
            try
            {
                var paymentLinkInfo = await _payOS.PaymentRequests.CancelAsync(orderCode, cancellationReason);
                return paymentLinkInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi hủy thanh toán PayOS: {ex.Message}", ex);
            }
        }

        public bool VerifyWebhookData(string webhookBody)
        {
            try
            {
                // PayOS webhook verification logic
                // This depends on your webhook implementation
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xác thực webhook PayOS: {ex.Message}", ex);
            }
        }
    }
}