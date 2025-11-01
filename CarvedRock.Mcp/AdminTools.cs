﻿using CarvedRock.Core;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace CarvedRock.Mcp;

[Authorize(Roles = "admin")]
[McpServerToolType]
public class AdminTools(IHttpClientFactory httpClientFactory, ILogger<AdminTools> logger,
    IHttpContextAccessor httpCtxAccessor)
{
    public record OperationResult(string Status, string? Message = null);

    [McpServerTool(Name = "delete_product"), Description("Delete a single product based on its Id.")]
    public async Task<OperationResult> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");
        var response = await client.DeleteAsync($"product/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode) throw new Exception($"Error deleting product {id}; HttpResponseCode was {(int)response.StatusCode}");

        LogDeletionActivity(id, httpCtxAccessor, logger);
        
        return new OperationResult("ok");
    }

    
    [McpServerTool(Name = "set_product_price")]
    [Description("Update the price of a single product based on its Id.")]    
    public async Task<OperationResult> UpdateProductPriceAsync(int id, double newPrice, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("CarvedRockApi");

        var productToUpdate = await client.GetFromJsonAsync<FullProductModel>($"product/{id}", cancellationToken); // not found throws exception

        if (newPrice == productToUpdate!.Price)
            return new OperationResult("not changed", "new (provided) product price is same as current price");

        productToUpdate.Price = newPrice;

        var response = await client.PutAsJsonAsync($"product/{id}", productToUpdate, cancellationToken: cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var problem = JsonSerializer.Deserialize<ProblemDetails>(content);

            var errorDetails = "";
            if (problem != null)
            {
                errorDetails = problem.Detail;
                foreach (var (key, value) in problem.Extensions)
                {
                    errorDetails += $"; {key}: {value}";
                }
            }

            return new OperationResult("error", errorDetails);
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error updating price on product {id}; HttpResponseCode was {(int)response.StatusCode}");
        }
            

        return new OperationResult("ok");
    }

    private void LogDeletionActivity(int id, IHttpContextAccessor httpCtxAccessor, ILogger<AdminTools> logger)
    {
        var httpCtx = httpCtxAccessor.HttpContext!;
        var user = httpCtx.User;

        var actorClaim = user.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Actor);
        ActorInfo? actorInfo = null;
        if (actorClaim != null)
        {
            actorInfo = JsonSerializer.Deserialize<ActorInfo>(actorClaim.Value);
        }

        // make this conditional and include or exclude the actor info based on whether it exists
        logger.LogInformation("Product {ProductId} deleted by user {User} (subject: {SubjectId}, " +
            "actor client_id: {ActorClientId})",
            id,
            user.Identity?.Name ?? "",
            user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value,
            actorInfo?.client_id ?? "");
    }
}

public record FullProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
    public string ImgUrl { get; set; } = null!;
}