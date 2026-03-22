using Application.DTOs.Payment;
using Application.Interfaces.Payment;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController(IBillingStatementService billingStatement,
        IPaystackService paystackService) : ControllerBase
    {
        private readonly IBillingStatementService _billingStatementService = billingStatement ?? throw new ArgumentNullException(nameof(billingStatement));
        private readonly IPaystackService _paystackService = paystackService ?? throw new ArgumentNullException(nameof(paystackService));
        [HttpGet("pay/{billingId}")]
        public async Task<IActionResult> Pay([FromRoute] Guid billingId)
        {
            var invoice = await _billingStatementService.GetBillingInvoiceAsync(billingId);
            var checkoutUrl = await _paystackService.InitializeTransactionAsync(billingId);

            Console.WriteLine($"CheckoutUrl: {checkoutUrl.Data}");

            if (string.IsNullOrEmpty(checkoutUrl.Data))
            {
                return BadRequest("Could not initialize payment with Paystack. Check server logs.");
            }

            return Ok(checkoutUrl.Data);
        }

        [HttpGet]
        public IActionResult Callback(string reference)
        {
            
            return Ok($"PaymentProcessing: {reference}");
        }

        [AllowAnonymous]
        [HttpPost("webhooks")]
        public async Task<IActionResult> Handle()
        {
            string json;
            using (var reader = new StreamReader(Request.Body))
            {
                json = await reader.ReadToEndAsync();
            }

            var signature = Request.Headers["x-paystack-signature"];

            if(!_paystackService.VerifySignature(json, signature))
            {
                return Unauthorized();
            }

            var ev = JsonConvert.DeserializeObject<PaystackEvent>(json);
            if (ev == null || ev.Event != "charge.success")
            {
                return Ok();
            }

            var result = await _paystackService.ProcessWebhookAsync(ev);

            return result.Data ? Ok() : StatusCode(500);

        }
    }


}
