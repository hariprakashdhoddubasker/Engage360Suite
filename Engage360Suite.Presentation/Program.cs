using Engage360Suite.Application.Interfaces;
using Engage360Suite.Application.Models;
using Engage360Suite.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string instanceId = builder.Configuration["WhatsApp:Pingerbot:InstanceId"]!;
string accessToken = builder.Configuration["WhatsApp:Pingerbot:AccessToken"]!;
string groupId = builder.Configuration["WhatsApp:Pingerbot:GroupId"]!;
builder.Services
       .Configure<PingerbotOptions>(
          builder.Configuration.GetSection("WhatsApp:Pingerbot"));

builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
{
    client.BaseAddress = new Uri("https://pingerbot.in/api/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
