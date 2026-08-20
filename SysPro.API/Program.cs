using Scalar.AspNetCore;
using SysPro.API.Interfaces;
using SysPro.API.Services;
using SysPro.DB;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
if (builder.Environment.IsDevelopment())
{
    var machineName = Environment.MachineName;
    if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
            .Windows))
    {
        machineName = machineName.ToLowerInvariant();
    }
    
    var machineOverrideFile = Path.Combine("dev", $"appsettings.{machineName}.json");
    builder.Configuration.AddJsonFile(machineOverrideFile, optional: true, reloadOnChange: true);
}
#endif

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var conn = builder.Configuration.GetConnectionString("Default")
           ?? throw new InvalidOperationException("ConnectionStrings:Default is not set.");

builder.Services.AddInfrastructure(conn, builder.Environment.IsDevelopment());

builder.Services.AddScoped<IOrdersService, OrderServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.MapOpenApi();
    app.MapScalarApiReference();
// }

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
