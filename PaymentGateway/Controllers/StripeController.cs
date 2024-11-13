using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;
using Stripe.Checkout;

namespace PaymentGateway.Controllers
{
    public class StripeController : Controller
    {
        private readonly IConfiguration _configuration;
        public StripeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.StripePublishableKey = _configuration["Publishablekey"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] Payment request)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = Convert.ToInt64(request.Amount),
                        Currency = _configuration["Currency"],
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Test Product",
                        },
                    },
                    Quantity = 1,
                },
            },
                Mode = "payment",
                SuccessUrl = "https://localhost:7192/home/success?gateway=stripe",
                CancelUrl = "https://localhost:7192/home/failed?gateway=stripe",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(new { sessionId = session.Id });
        }
    }
}
