using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SysPro.API.Interfaces;
using SysPro.API.Services;
using SysPro.Application.Interfaces;
using SysPro.Application.Repositories;
using SysPro.DB.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var conn = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(conn));

builder.Services.AddScoped<IOrdersService, OrderServices>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

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
