using FluentValidation;
using Grpc.Net.Client;
using IAMService.API.gRPC.Protos;
using IAMService.API.gRPC.Protos.UserProto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Patient_TestOrder_Service.API.gRPC.Protos.PatientProto;
using Scalar.AspNetCore;
using TestOrderService.API.Converters;
using TestOrderService.API.gRPC.Services;
using TestOrderService.API.Middleware;
using TestOrderService.API.Middleware.Authentication;
using TestOrderService.API.Middleware.Authorization;
using TestOrderService.Application;
using TestOrderService.Application.Behaviors;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Infrastructure.Data;
using TestOrderService.Infrastructure.EventBus;
using TestOrderService.Infrastructure.EventBus.Kafka;
using TestOrderService.Infrastructure.gRPC.Clients;
using TestOrderService.Infrastructure.Repositories;
using TestOrderService.Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

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

// Register Patient gRPC client
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var grpcAddress = configuration["PATIENT_GRPC_URL"] ?? "http://localhost:5249";
    var channel = GrpcChannel.ForAddress(grpcAddress);
    return new PatientService.PatientServiceClient(channel);
});
builder.Services.AddScoped<IPatientGrpcClient, PatientGrpcClient>();

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var grpcAddress = configuration["IAM_GRPC_URL"] ?? "http://localhost:5095";
    var channel = GrpcChannel.ForAddress(grpcAddress);
    return new UserService.UserServiceClient(channel);
});
builder.Services.AddScoped<IUserGrpcClient, UserGrpcClient>();

builder.Services.AddGrpcClient<Privilege.PrivilegeClient>(o =>
    {
        o.Address = new Uri(builder.Configuration["IAM_GRPC_URL"] ?? "");
    }
);
builder.Services.AddScoped<IPrivilegeGrpcClient, PrivilegeGrpcClient>();

builder.Services.AddGrpc();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.AddKafkaProducer("kafka");
var kafkaTopic = builder.Configuration["EVENT_PUBLISHING_TOPICS"];
if (!string.IsNullOrEmpty(kafkaTopic))
    builder.AddKafkaEventPublisher(kafkaTopic);
else
    builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();

builder.Services.AddAuthentication("JwtBearer")
    .AddScheme<LabAuthenticationSchemeOptions, LabAuthenticationHandler>("JwtBearer", configureOptions =>
        {
            var sectionJwt = configuration.GetSection("Jwt");
            configureOptions.IssuerSigningKey = sectionJwt["SigningKey"] ?? throw new InvalidOperationException("'SigningKey' can't be read.");
            configureOptions.ValidIssuer = sectionJwt["Issuer"] ?? throw new InvalidOperationException("'Issuer' can't be read.");
            configureOptions.ValidAudience = sectionJwt["Audience"] ?? throw new InvalidOperationException("'Audience' can't be read.");
        }
    );
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/", () => Results.Ok("Welcome to Test Order Service")).AllowAnonymous();
app.MapScalarApiReference().AllowAnonymous();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<AuthenticationGateMiddleware>();
app.UseAuthorization();


app.MapControllers();

app.MapGrpcService<TestOrderGrpcService>();

await app.MigrateDbContextAsync<TestOrderServiceDbContext>();

await app.RunAsync();
