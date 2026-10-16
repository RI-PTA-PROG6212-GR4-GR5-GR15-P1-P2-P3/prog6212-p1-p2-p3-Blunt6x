using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => {
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "RaceDay API", Version = "v1", Description = "API for race event management." });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "Paste the token returned by /api/auth/login." });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<RaceDayDbContext>(o => o.UseSqlServer(connection));
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddScoped<JwtService>();
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DEV_ONLY_CHANGE_THIS_KEY_TO_A_LONG_RANDOM_SECRET_2026";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => {
    o.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RaceDayApi", ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RaceDayClients", IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ClockSkew = TimeSpan.FromSeconds(30) };
});
builder.Services.AddAuthorization();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
