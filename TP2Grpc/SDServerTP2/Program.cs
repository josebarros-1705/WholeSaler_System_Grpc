using SDServerTP2.Authentication;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using SDServerTP2.Broker;
using SDServerTP2.Data;
using SDServerTP2.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SDServerTP2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SDServerTP2Context") ?? throw new InvalidOperationException("Connection string 'SDServerTP2Context' not found.")), ServiceLifetime.Transient);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddScoped<EmitEVENTS>();

builder.Services.AddSingleton(FirebaseApp.Create());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, OperadorAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, (o) => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PedidosService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseAuthentication();

app.Run();
