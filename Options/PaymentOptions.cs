using System.ComponentModel.DataAnnotations;

namespace TmsApi.Options;

// Exercise 3: Strongly-typed options class
// [Required] and [Range] make the app crash at startup if config is missing or invalid
public class PaymentOptions
{
    [Required]
    public required string GatewayUrl { get; init; }

    [Range(100, 100000)]
    public decimal MaxDepositBirr { get; init; }
}