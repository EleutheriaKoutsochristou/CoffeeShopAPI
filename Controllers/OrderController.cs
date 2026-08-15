using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers;

//Simulates a user buying a coffee and paying for it with a bank service. The bank service is intentionally down to demonstrate the Retry Pattern.
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

  [HttpPost("buy-coffee")]
    public async Task<IActionResult> BuyCoffee()
    {
        var client = _httpClientFactory.CreateClient("BankService");

        try
        {
            // We call "503" to intentionally trigger a "Service Unavailable" error.
            // Watch the terminal when you run this: Polly will intercept and retry!
            var response = await client.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                return Ok("Payment successful! Here is your coffee ☕");
            }

            return StatusCode(500, "Bank is currently down. Please try again later.");
        }
        catch (Polly.CircuitBreaker.BrokenCircuitException)
        {
            // ΑΥΤΟ ΕΙΝΑΙ ΤΟ CIRCUIT BREAKER! 
            // Το σύστημα ξέρει ότι η τράπεζα έχει πέσει και δεν προσπαθεί καν.
            return StatusCode(503, "CIRCUIT BREAKER OPEN: Το σύστημα πληρωμών της Τράπεζας είναι εκτός λειτουργίας. Παρακαλώ περιμένετε λίγα δευτερόλεπτα.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}