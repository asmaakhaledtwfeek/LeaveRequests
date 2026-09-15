using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HrPortal;
using HrPortal.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("http://localhost:5050/") });

builder.Services.AddScoped<LeaveApiClient>(sp =>
{
    var http = sp.GetRequiredService<HttpClient>();
    return new LeaveApiClient(http);
});

await builder.Build().RunAsync();
