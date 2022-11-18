using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context) =>

{
    var Category = context.Category.Where(c => c.Id == productRequest.CategoryId).FirstOrDefault();
    var product = new Product
    {
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = Category,

    };

    if (productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach (var tag in productRequest.Tags)
        {
            product.Tags.Add(new Tag { Name = tag });
        }
    }

    context.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}", product.Id);
});
app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
    var product = context.Products
    .Include(p => p.Category)
    .Include(p => p.Tags)
    .Where(p => p.Id == id).FirstOrDefault();
    if (product != null)
    {
        return Results.Ok(product);
    }

    return Results.NotFound();
});
app.MapPut("/products/{id}", ([FromRoute] int id, ApplicationDbContext context, ProductRequest productRequest) =>
{
    var product = context.Products
    .Include(p => p.Tags)
    .Where(p => p.Id == id).First();

    var category = context.Category.Where(p => p.Id == id).First();


    product.Code = productRequest.Code;
    product.Name = productRequest.Name;
    product.Description = productRequest.Description;
    product.Category = category;
    if (productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach (var tag in productRequest.Tags)
        {
            product.Tags.Add(new Tag { Name = tag });
        }
    }

    context.SaveChanges();
    return Results.Ok();
});
app.MapDelete("products/{id}", ([FromRoute] int id, ApplicationDbContext context, ProductRequest productRequest) =>
{
    var product = context.Products.Where(p => p.Id == id).FirstOrDefault();
    context.Products.Remove(product);
    context.SaveChanges();
    return Results.Ok();
});
app.Run();