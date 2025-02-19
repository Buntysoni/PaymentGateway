using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models
{
    public class Payment
    {
        [Required]
        public string? Name { get; set; }
        public string? Email { get; set; }
        public decimal Amount { get; set; }
        public string? Mobile { get; set; }
    }
}
