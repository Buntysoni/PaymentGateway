using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;

namespace PaymentGateway.Controllers
{
    public class RazorPayController : Controller
    {
        private readonly IConfiguration _configuration;
        private string key = "***";
        private string secret = "***";
        public RazorPayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            ViewBag.RazorpayKey = key;
            return View();
        }

        [HttpPost]
        public IActionResult Index(PaymentGateway.Models.Payment payment)
        {
            //RazorpayClient client = new RazorpayClient(key, secret);
            //Dictionary<string, object> options = new Dictionary<string, object>();
            //options.Add("amount", payment.Amount * 100);
            //options.Add("currency", "INR");
            //options.Add("receipt", "order_rcptid_11");
            //options.Add("payment_capture", 1);
            //Razorpay.Api.Order order = client.Order.Create(options);
            //ViewBag.OrderId = order["id"].ToString();
            
            return View();
        }

        public ActionResult PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", razorpay_payment_id);
            //attributes.Add("razorpay_order_id", razorpay_order_id);
            attributes.Add("razorpay_signature", razorpay_signature);
            Utils.verifyPaymentSignature(attributes);
            ViewBag.PaymentId = razorpay_payment_id;
            ViewBag.OrderId = razorpay_order_id;
            return RedirectToAction("Success","Home");
        }
    }
}
