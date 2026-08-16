using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeMenuController : ControllerBase
{
    private static readonly List<CoffeeMenuItem> Menu = new()
    {
        new() { Id = 1, Name = "Espresso", Description = "Bold and rich single-origin coffee.", Price = 3.50m },
        new() { Id = 2, Name = "Latte", Description = "Smooth espresso with steamed milk.", Price = 4.75m },
        new() { Id = 3, Name = "Cappuccino", Description = "Classic espresso with foamy milk.", Price = 4.50m },
        new() { Id = 4, Name = "Cold Brew", Description = "Slow-steeped for a smooth finish.", Price = 5.00m }
    };

    [HttpGet(Name = "GetCoffeeMenu")]
    public ActionResult<IEnumerable<CoffeeMenuItem>> Get()
    {
        return Ok(Menu);
    }
}
