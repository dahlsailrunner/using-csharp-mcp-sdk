using CarvedRock.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace CarvedRock.WebApp.Pages;

public partial class ListingModel(IProductService productService, 
    IHttpClientFactory httpClientFactory) : PageModel
{
    public List<ProductModel> Products { get; set; } = [];
    public string CategoryName { get; set; } = "";

    public async Task OnGetAsync()
    {
        var cat = Request.Query["cat"].ToString();
        if (string.IsNullOrEmpty(cat))
            throw new Exception("failed");

        Products = await productService.GetProductsAsync(cat);
        if (Products.Count != 0)
        {
            CategoryName = Products.First().Category[..1].ToUpper() +
                           Products.First().Category[1..];
        }
    }

    public async Task<IActionResult> OnGetChat(string message, CancellationToken cxl)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();

        var client = httpClientFactory.CreateClient("AI");

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"Agent/recommendation?message={Uri.EscapeDataString(message ?? string.Empty)}");

        var apiResponse = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cxl);
        apiResponse.EnsureSuccessStatusCode();

        await using var stream = await apiResponse.Content.ReadAsStreamAsync(cxl);
        var buffer = new byte[4096];
        var decoder = Encoding.UTF8;
        int read;
        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cxl)) > 0 && 
            !cxl.IsCancellationRequested)
        {
            var chunk = decoder.GetString(buffer, 0, read);
            // Split into smaller SSE frames if desired; here we send as-is
            await Response.WriteAsync($"data: {chunk}\n\n", cxl);
            await Response.Body.FlushAsync(cxl);
        }

        await Response.WriteAsync("event: end\ndata: done\n\n", cxl);
        await Response.Body.FlushAsync(cxl);
        return new EmptyResult();
    }
}
