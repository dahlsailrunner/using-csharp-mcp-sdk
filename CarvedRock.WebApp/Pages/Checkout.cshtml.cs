using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace CarvedRock.WebApp.Pages;

[Authorize]
[ValidateAntiForgeryToken]
public class CheckoutModel(IProductService productService, IEmailSender emailService) : PageModel
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string EmailAddress { get; set; } = "";

    public List<CartItem> CartContents { get; set; } = [];
    public double CartTotal => CartContents.Sum(c => c.Total);
    public async Task OnGetAsync()
    {
        EmailAddress = User.Claims.First(c => c.Type == "email").Value;

        var cookie = Request.Cookies["carvedrock-cart"];
        if (string.IsNullOrEmpty(cookie)) return;

        CartContents = JsonSerializer.Deserialize<List<CartItem>>(cookie, _jsonOptions)!;

        var allProducts = await productService.GetProductsAsync();

        CartContents = CartContents.Select(cartItem =>
        {
            var product = allProducts.FirstOrDefault(p => p.Id == cartItem.Id)!;

            return new CartItem(cartItem.Id, cartItem.Quantity, product.Name,
                product.Category, product.Price, product.Price * cartItem.Quantity);
        }).ToList();
    }

    public async Task<IActionResult> OnPostSubmitOrder()
    {
        EmailAddress = User.Claims.First(c => c.Type == "email").Value;

        var cookie = Request.Cookies["carvedrock-cart"];
        if (string.IsNullOrEmpty(cookie)) return RedirectToPage("/Cart");

        CartContents = JsonSerializer.Deserialize<List<CartItem>>(cookie, _jsonOptions)!;

        var allProducts = await productService.GetProductsAsync();

        CartContents = CartContents.Select(cartItem =>
        {
            var product = allProducts.FirstOrDefault(p => p.Id == cartItem.Id)!;

            return new CartItem(cartItem.Id, cartItem.Quantity, product.Name,
                product.Category, product.Price, product.Price * cartItem.Quantity);
        }).ToList();

        // updated email template with this prompt 
        // can you create a new version of the emailTemplate.html code? 
        // it should contain content placeholders for a table listing the products a user 
        // has purchased along with a placeholder for some narrative content. The table 
        // listing purchased products should be left-justified.

        string basePath = AppContext.BaseDirectory;
        string templatePath = Path.Combine(basePath, "emailTemplate.html");
        string template = await System.IO.File.ReadAllTextAsync(templatePath);

        template = template.Replace("{{NarrativeContent}}", "<h1>Thank you for your order!</h1>");

        var productRows = new StringBuilder();
        foreach (var cartItem in CartContents)
        {
            productRows.AppendLine($"<tr><td>{cartItem.Name}</td><td>{cartItem.Quantity}</td><td>{cartItem.Total}</td></tr>");
        }
        template = template.Replace("{{ProductRows}}", productRows.ToString());

        template = template.Replace("{{AdditionalNotes}}", "Enjoy your new gear!");

        await emailService.SendEmailAsync(EmailAddress, "Your CarvedRock Order", template);

        Response.Cookies.Delete("carvedrock-cart");
        return RedirectToPage("/ThankYou");
    }

    public IActionResult OnPostCancelOrder()
    {
        Response.Cookies.Delete("carvedrock-cart");
        return RedirectToPage("/Index");
    }
}
