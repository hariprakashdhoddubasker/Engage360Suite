using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Filters;
using Engage360Suite.Infrastructure.Services;
using Engage360Suite.Presentation;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// API versioning
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Add the ApiExplorer support to same API builder
apiVersioningBuilder.AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";    // e.g. "v1"
    setup.SubstituteApiVersionInUrl = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<PingerbotOptions>(builder.Configuration.GetSection("WhatsApp:Pingerbot"));
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
{
    client.BaseAddress = new Uri("https://pingerbot.in/api/");
});
builder.Services.AddScoped<ApiKeyActionFilter>();
builder.Services.AddVersionedSwagger();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseVersionedSwaggerUI();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.MapControllers();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
