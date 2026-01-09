using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using HomePlanner.Shared.Constants;
using HomePlanner.Repository.IRepository;

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
builder.Services.AddSwaggerGen();

//* ASP.NET Core Identity *//
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
