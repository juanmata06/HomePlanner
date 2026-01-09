using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//* BBDD Configuration *//
var dbConnectionString = builder.Configuration.GetConnectionString("ConnectionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(dbConnectionString)
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
