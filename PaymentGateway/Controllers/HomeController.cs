using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PaymentGateway.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new Payment());
        }

        public IActionResult PhonePe()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PhonePe(Payment model)
        {
            string merchantTransactionId = Guid.NewGuid().ToString();
            string merchantUserId = "MUID" + DateTime.Now.Ticks;
            decimal amount = 100 * model.Amount; //1*100 = 100 in paisa

            var paymentData = new
            {
                merchantId = PhonePeConfig.MerchantId,
                merchantTransactionId = merchantTransactionId,
                merchantUserId = merchantUserId,
                amount = amount,
                redirectUrl = "https://unlein.com/PaymentCallback.aspx",
                redirectMode = "POST",
                callbackUrl = "https://unlein.com/PaymentCallback.aspx",
                mobileNumber = model.Mobile,
                paymentInstrument = new
                {
                    type = "PAY_PAGE"
                }
            };

            string payload = JsonConvert.SerializeObject(paymentData);
            string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            string checksum = CalculateChecksum(base64Payload + "/pg/v1/pay" + PhonePeConfig.SaltKey);
            string xVerify = $"{checksum}###{PhonePeConfig.SaltIndex}";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                httpClient.DefaultRequestHeaders.Add("X-VERIFY", xVerify);

                var content = new StringContent(JsonConvert.SerializeObject(new { request = base64Payload }), Encoding.UTF8, "application/json");

                var response = httpClient.PostAsync(PhonePeConfig.ApiEndpoint, content).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JObject.Parse(responseContent);
                    string paymentUrl = jsonResponse["data"]["instrumentResponse"]["redirectInfo"]["url"].ToString();
                    Response.Redirect(paymentUrl);
                }
                else
                {
                    // Handle error
                    //string message = "Payment initiation failed. Please try again.";
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Success(string gateway = "")
        {
            ViewBag.Gateway = gateway;
            return View();
        }
        
        public IActionResult Failed(string gateway = "")
        {
            ViewBag.Gateway = gateway;
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string CalculateChecksum(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public class PhonePeConfig
        {
            public static string? MerchantId { get; set; } = "Your_Merchant_Id";
            public static string? SaltKey { get; set; } = "Your_Salt_Key";
            public static string? SaltIndex { get; set; } = "Your_Salt_Index";
            public static string? ApiEndpoint { get; set; } = "PhonePe_Api_Endpoint";
        }
    }
}
