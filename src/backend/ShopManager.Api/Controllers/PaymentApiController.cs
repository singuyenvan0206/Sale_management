using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Core.DTOs;
using ShopManager.Core.Enums;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Settings;
using ShopManager.Services;

namespace ShopManager.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/payment")]
    [Produces("application/json")]
    public class PaymentApiController : ControllerBase
    {
        private readonly IBankStatementService _bankService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<PaymentApiController> _logger;

        public PaymentApiController(
            IBankStatementService bankService,
            IInvoiceService invoiceService,
            ILogger<PaymentApiController> logger)
        {
            _bankService = bankService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo mã QR VietQR động (qua dịch vụ SePay VietQR Api)
        /// </summary>
        [HttpPost("create-qr")]
        [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
        public IActionResult CreateVietQr([FromBody] CreateQrRequest request)
        {
            if (request == null || request.Amount <= 0)
            {
                return BadRequest(ApiResponse.Fail("Số tiền không hợp lệ."));
            }

            try
            {
                var settings = PaymentSettingsManager.Load();
                string bankCode = !string.IsNullOrWhiteSpace(request.BankName) ? request.BankName : settings.BankCode;
                string bankAccount = !string.IsNullOrWhiteSpace(request.BankAccount) ? request.BankAccount : settings.BankAccount;
                string accountName = settings.AccountHolder;
                string description = !string.IsNullOrWhiteSpace(request.Description) ? request.Description : $"HD{request.InvoiceId}";

                // Build VietQR image URL via standard VietQR / SePay QR service
                string qrUrl = $"https://img.vietqr.io/image/{bankCode}-{bankAccount}-compact2.png?amount={(long)request.Amount}&addInfo={Uri.EscapeDataString(description)}&accountName={Uri.EscapeDataString(accountName)}";

                var response = new CreateQrResponse
                {
                    QrCodeUrl = qrUrl,
                    BankName = bankCode,
                    BankAccount = bankAccount,
                    AccountName = accountName,
                    Amount = request.Amount,
                    Description = description
                };

                return Ok(ApiResponse<CreateQrResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo QR payment API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi tạo QR: {ex.Message}"));
            }
        }

        /// <summary>
        /// Webhook nhận thông báo chuyển khoản tự động từ SePay
        /// </summary>
        [HttpPost("sepay-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookPayload payload)
        {
            _logger.LogInformation("Nhận Webhook từ SePay: Content={Content}, Amount={Amount}", payload?.content, payload?.transferAmount);

            if (payload == null)
            {
                return BadRequest(ApiResponse.Fail("Payload Webhook rỗng."));
            }

            try
            {
                // Parse Content to match Invoice ID (e.g. "HD102" or "102")
                if (!string.IsNullOrWhiteSpace(payload.content))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(payload.content, @"(?i)(?:HD|INV)?(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int invoiceId))
                    {
                        var details = await _invoiceService.GetInvoiceDetailsAsync(invoiceId);
                        if (details.Header != null && details.Header.Id > 0 && details.Header.Status != ShopManager.Core.Enums.InvoiceStatus.Completed.ToDbString())
                        {
                            var invoice = details.Header;
                            invoice.Items = details.Items;
                            invoice.Paid += payload.transferAmount;
                            if (invoice.Paid >= invoice.Total)
                            {
                                invoice.Status = ShopManager.Core.Enums.InvoiceStatus.Completed.ToDbString();
                            }
                            await _invoiceService.SaveInvoiceAsync(invoice, invoice.VoucherId);
                            _logger.LogInformation("Đã cập nhật tự động hóa đơn Id={InvoiceId} qua Webhook SePay", invoiceId);
                            return Ok(new { success = true, message = $"Cập nhật hóa đơn HD{invoiceId} thành công." });
                        }
                    }
                }

                return Ok(new { success = true, message = "Webhook received successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý SePay Webhook");
                return StatusCode(500, ApiResponse.Fail($"Lỗi Webhook: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái thanh toán chuyển khoản cho hóa đơn
        /// </summary>
        [HttpGet("check-status/{invoiceId:int}")]
        [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
        public async Task<IActionResult> CheckPaymentStatus(int invoiceId, [FromQuery] decimal amount)
        {
            try
            {
                var details = await _invoiceService.GetInvoiceDetailsAsync(invoiceId);
                var invoice = details.Header;
                if (invoice != null && invoice.Id > 0 && invoice.Status == ShopManager.Core.Enums.InvoiceStatus.Completed.ToDbString())
                {
                    return Ok(ApiResponse<object>.Ok(new { IsPaid = true, Status = invoice.Status, PaidAmount = invoice.Paid }));
                }

                string code = $"HD{invoiceId}";
                var tx = await _bankService.VerifyPaymentAsync(amount, code);

                if (tx != null)
                {
                    if (invoice != null && invoice.Id > 0)
                    {
                        invoice.Items = details.Items;
                        invoice.Paid = invoice.Total;
                        invoice.Status = ShopManager.Core.Enums.InvoiceStatus.Completed.ToDbString();
                        await _invoiceService.SaveInvoiceAsync(invoice, invoice.VoucherId);
                    }
                    return Ok(ApiResponse<object>.Ok(new { IsPaid = true, Transaction = tx }));
                }

                return Ok(ApiResponse<object>.Ok(new { IsPaid = false, Message = "Chưa nhận được chuyển khoản." }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kiểm tra trạng thái thanh toán API");
                return StatusCode(500, ApiResponse.Fail($"Lỗi hệ thống: {ex.Message}"));
            }
        }
    }
}
