using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using HomePlanner.Shared.Constants;
using HomePlanner.Repository.IRepository;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//* BBDD Configuration *//
var dbConnectionString = builder.Configuration.GetConnectionString("ConnectionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(dbConnectionString)
);

//* App repositories *//
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddRouting(options => options.LowercaseUrls = true); // This displays routes as lower case

//* AutoMapper *//
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//* Cache configs *//
// Weigth:
builder.Services.AddResponseCaching(options =>
{
  options.MaximumBodySize = 1024 * 1024; //* => 1mb
  options.UseCaseSensitivePaths = true;
});
// Duration:
builder.Services.AddControllers(option =>
{
  option.CacheProfiles.Add(CacheProfiles.Default10, new CacheProfile { Duration = 10 });
  option.CacheProfiles.Add(CacheProfiles.Default20, new CacheProfile { Duration = 20 });
});

builder.Services.AddEndpointsApiExplorer();

//* Swagger configs *//
builder.Services.AddSwaggerGen(
  options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Our API uses JWT Authentication using the Bearer scheme. \n\r\n\r" +
            "Enter the token generated at login below.\n\r\n\r" +
            "Example: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });
  }
);

//* ASP.NET Core Identity *//
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//* Endpoints auth with Bearer Tokens config
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
var Issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");
var Audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");
if (string.IsNullOrEmpty(secretKey))
{
  throw new InvalidOperationException("SecretKey has not been configured");
}
builder.Services.AddAuthentication(opts =>
{
  opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
  opts.RequireHttpsMetadata = false;
  opts.SaveToken = true;
  opts.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    ValidIssuer = Issuer,
    ValidAudience = Audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    ValidateIssuer = false,
    ValidateAudience = true
  };
});

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
