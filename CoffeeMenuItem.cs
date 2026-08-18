namespace CoffeeShopAPI;

public class CoffeeMenuItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // Blob name stored in Azure Storage for this item's image (optional)
    public string? ImageBlobName { get; set; }
    // URL that the UI can use to fetch the image. For images stored in the static
    // frontend assets, set this to a path like '/espresso.png'. If `ImageBlobName`
    // is set, the controller can still serve the image via the API endpoint.
    public string? ImageUrl { get; set; }
}
