using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TestOrderService.API.Converters;
using TestOrderService.API.gRPC.Services;
using TestOrderService.API.Middleware;
using TestOrderService.Application;
using TestOrderService.Application.Behaviors;
using TestOrderService.Application.Interfaces;
using TestOrderService.Infrastructure.Data;
using TestOrderService.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/run/secrets/secrets_file", true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
        }
    );

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TestOrderServiceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITestOrderRepository, TestOrderRepository>();

var applicationAssembly = typeof(IAssemblyReference).Assembly;
builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(applicationAssembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
);
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

builder.Services.AddGrpc();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowFrontend");
app.MapGet("/", () => Results.Ok("Welcome to Test Order Service")).AllowAnonymous();
app.MapScalarApiReference().AllowAnonymous();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGrpcService<TestOrderGrpcService>();

await app.MigrateDbContextAsync<TestOrderServiceDbContext>();

await app.RunAsync();
