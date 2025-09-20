using CarvedRock.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarvedRock.WebApp.Pages.Admin;

[Authorize(Roles = "admin")]
public class DeleteModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IProductService productService, ILogger<DeleteModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [BindProperty]
    public ProductModel Product { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        int id = Product.Id;
        
        try
        {
            bool deleted = await _productService.DeleteProductAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("Product not found when trying to delete: {id}", id);
                return NotFound();
            }
            
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {id}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the product.");
            
            // Reload the product data for the view
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            Product = product;
            return Page();
        }
    }
}