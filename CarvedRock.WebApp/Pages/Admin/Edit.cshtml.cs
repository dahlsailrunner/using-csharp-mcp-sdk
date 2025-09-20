using CarvedRock.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarvedRock.WebApp.Pages.Admin;

[Authorize(Roles = "admin")]
public class EditModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IProductService productService, ILogger<EditModel> logger)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Convert ProductModel to NewProductModel for the API call
            var updatedProduct = new NewProductModel
            {
                Name = Product.Name,
                Category = Product.Category,
                Description = Product.Description,
                Price = Product.Price,
                ImgUrl = Product.ImgUrl
            };
            
            var validationErrors = await _productService.UpdateProductAsync(Product.Id, updatedProduct);
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError($"Product.{error.Key}", string.Join(",", error.Value.Split('|')));
                }
                return Page();
            }

            return RedirectToPage("./Index");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found: {id}", Product.Id);
            return NotFound();
        }
    }
}