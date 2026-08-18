using Azure.Storage.Blobs.Models;
using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeMenuController : ControllerBase
{
    private readonly IAzureBlobService? _blobService;

    private static readonly List<CoffeeMenuItem> Menu = new()
    {
        new() { Id = 1, Name = "Espresso", Description = "Bold and rich single-origin coffee.", Price = 3.50m, ImageUrl = "/espresso.png" },
        new() { Id = 2, Name = "Latte", Description = "Smooth espresso with steamed milk.", Price = 4.75m, ImageUrl = "/latte.png" },
        new() { Id = 3, Name = "Cappuccino", Description = "Classic espresso with foamy milk.", Price = 4.50m, ImageUrl = "/cappucino.png" },
        new() { Id = 4, Name = "Cold Brew", Description = "Slow-steeped for a smooth finish.", Price = 5.00m, ImageUrl = "/cold_brew.png" }
    };

    public CoffeeMenuController(IAzureBlobService? blobService = null)
    {
        _blobService = blobService;
    }

    [HttpGet(Name = "GetCoffeeMenu")]
    public ActionResult<IEnumerable<CoffeeMenuItem>> Get()
    {
        return Ok(Menu);
    }

    // Upload image for a coffee menu item. Requires Azure connection configured in appsettings or env.
    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        var item = Menu.FirstOrDefault(m => m.Id == id);
        if (item == null) return NotFound("Menu item not found.");
        if (file == null || file.Length == 0) return BadRequest("No file provided.");
        // Uploads are optional in this setup; if Blob storage isn't configured reject.
        if (_blobService == null) return StatusCode(500, "Blob storage not configured.");

        var blobName = await _blobService.UploadFileAsync(file);
        item.ImageBlobName = blobName;
        // When a blob is uploaded, expose API endpoint URL for the image
        item.ImageUrl = $"/api/coffeemenu/{id}/image";

        return Ok(new { imageUrl = item.ImageUrl });
    }

    // Stream image from Azure Blob Storage for a menu item
    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetImage(int id)
    {
        var item = Menu.FirstOrDefault(m => m.Id == id);
        if (item == null || string.IsNullOrEmpty(item.ImageBlobName)) return NotFound();
        if (_blobService == null) return StatusCode(500, "Blob storage not configured.");

        BlobDownloadInfo blob;
        try
        {
            blob = await _blobService.GetBlobAsync(item.ImageBlobName);
        }
        catch
        {
            return NotFound();
        }

        return File(blob.Content, blob.ContentType ?? "application/octet-stream");
    }
}
