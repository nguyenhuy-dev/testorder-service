using Microsoft.EntityFrameworkCore;
using TestOrderService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/run/secrets/secrets_file", optional: true);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TestOrderServiceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


await app.MigrateDbContextAsync<TestOrderServiceDbContext>();
app.UseHttpsRedirection();


app.Run();


