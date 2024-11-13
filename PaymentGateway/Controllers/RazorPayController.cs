using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;

namespace PaymentGateway.Controllers
{
    public class RazorPayController : Controller
    {
        private readonly IConfiguration _configuration;
        private string key = "xxx";
        private string secret = "xxx";
        public RazorPayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            // Configure Razorpay client
            RazorpayClient client = new RazorpayClient(key, secret);

            // Order details
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", 50000);
            options.Add("currency", "INR");
            options.Add("receipt", "order_rcptid_11");
            options.Add("payment_capture", 1);

            // Create order
            Razorpay.Api.Order order = client.Order.Create(options);
            ViewBag.OrderId = order["id"].ToString();
            ViewBag.RazorpayKey = key;
            return View();
        }

        public ActionResult PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            // Validate the payment signature
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", razorpay_payment_id);
            attributes.Add("razorpay_order_id", razorpay_order_id);
            attributes.Add("razorpay_signature", razorpay_signature);

            Utils.verifyPaymentSignature(attributes);

            // Handle success logic here, such as updating the order status
            ViewBag.PaymentId = razorpay_payment_id;
            ViewBag.OrderId = razorpay_order_id;

            return View("Success");
        }
    }
}
