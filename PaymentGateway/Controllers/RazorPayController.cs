using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using PaymentGateway.Models;

namespace PaymentGateway.Controllers
{
    public class RazorPayController : Controller
    {
        private readonly IConfiguration _configuration;
        public RazorPayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
