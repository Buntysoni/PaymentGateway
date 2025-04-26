using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace PaymentGateway.Controllers
{
    public class CashfreeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> CreateOrder(string name, string Email, string Phone)
        {
            var order_id = Guid.NewGuid().ToString();
            var client = new RestClient("https://sandbox.cashfree.com/pg/orders");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("x-api-version", "2022-09-01");
            request.AddHeader("x-client-id", "YOURCLIENTID");
            request.AddHeader("x-client-secret", "YOURSECRETKEY");

            var body = new
            {
                order_id = order_id,
                order_amount = 100.00,
                order_currency = "INR",
                customer_details = new
                {
                    customer_id = Guid.NewGuid().ToString(),
                    customer_email = Email,
                    customer_phone = Phone,
                    customer_name = name
                }
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var json = JsonConvert.DeserializeObject<JObject>(response.Content);
                var sessionId = json["payment_session_id"]?.ToString();
                return Json(new { payment_session_id = sessionId, order_id = order_id });
            }
            else
            {
                return Json(new { error = response.Content, order_id = order_id });
            }
        }

        [HttpPost]
        public async Task<string> VerifyPayment(string orderId)
        {
            var client = new RestClient($"https://sandbox.cashfree.com/pg/orders/{orderId}");
            var request = new RestRequest("", Method.Get);
            request.AddHeader("x-client-id", "YOURCLIENTID");
            request.AddHeader("x-client-secret", "YOURSECRETKEY");
            request.AddHeader("x-api-version", "2022-09-01");
            var response = await client.ExecuteAsync(request);
            return response.Content;
        }


        public ActionResult PaymentSuccess(string order_id)
        {
            // verify order status from Cashfree API
            return View();
        }

        public ActionResult PaymentFailure(string order_id)
        {
            return View();
        }


    }
}
