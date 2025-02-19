using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe.V2;

namespace PhonePePaymentAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PhonePeController : Controller
    {
        private readonly string _merchantId;
        private readonly string _saltKey;
        private readonly string _saltIndex;
        private readonly string _baseUrl = "https://api.phonepe.com/apis/hermes/";
        private const string ApiEndpoint = "https://api.phonepe.com/apis/hermes/pg/v1/pay";
        private readonly HttpClient _httpClient;

        public PhonePeController(IConfiguration configuration, HttpClient httpClient)
        {
            _merchantId = "";
            _saltKey = "";
            _saltIndex = "1";
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestModel model)
        {
            try
            {
                string transactionid = Guid.NewGuid().ToString();
                var paymentData = new
                {
                    merchantId = _merchantId,
                    merchantTransactionId = transactionid,
                    merchantUserId = "MUID" + DateTime.Now.Ticks,
                    amount = 100,
                    redirectUrl = "https://yourdomai.com/",
                    redirectMode = "POST",
                    callbackUrl = "https://yourdomain.com",
                    mobileNumber = model.MobileNumber,
                    paymentInstrument = new
                    {
                        type = "PAY_PAGE"
                    }
                };

                string payload = JsonConvert.SerializeObject(paymentData);
                string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

                string checksum = CalculateChecksum(base64Payload + "/pg/v1/pay" + _saltKey);
                string xVerify = $"{checksum}###{_saltIndex}";

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                    httpClient.DefaultRequestHeaders.Add("X-VERIFY", xVerify);

                    var content = new StringContent(JsonConvert.SerializeObject(new { request = base64Payload }), Encoding.UTF8, "application/json");

                    var response = httpClient.PostAsync(ApiEndpoint, content).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = JObject.Parse(responseContent);
                        string paymentUrl = jsonResponse["data"]["instrumentResponse"]["redirectInfo"]["url"].ToString();
                        return Ok(new { result = paymentUrl });
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string CalculateChecksum(string payload)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(payload));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private string CalculateChecksum(string payload, string saltKey, string saltIndex)
        {
            string dataToHash = payload + saltKey;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString() + "###" + saltIndex;
            }
        }

        [HttpPost("status")]
        public async Task<string> CheckPaymentStatus(string transactionId)
        {
            string endpoint = $"/pg/v1/status/{_merchantId}/{transactionId}";
            string checksum = CalculateChecksum(endpoint, _saltKey, _saltIndex);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-VERIFY", checksum);
                client.DefaultRequestHeaders.Add("X-MERCHANT-ID", _merchantId);

                string url = $"https://api.phonepe.com/apis/hermes{endpoint}";

                // Use HttpRequestMessage to correctly set headers
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
        }
    }

    public class PaymentRequestModel
    {
        public string? TransactionId { get; set; }
        public int Amount { get; set; }
        public string? CallbackUrl { get; set; }
        public string? MobileNumber { get; set; }
    }
}
