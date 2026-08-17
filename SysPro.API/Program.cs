using Scalar.AspNetCore;
using SysPro.API.Interfaces;
using SysPro.API.Services;
using SysPro.DB;

var builder = WebApplication.CreateBuilder(args);

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
