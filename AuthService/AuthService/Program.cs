using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
if (string.IsNullOrEmpty(postgresPassword))
{
    throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is not set.");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!
    .Replace("_PASSWORD_", postgresPassword);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



