using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZasNet.Application;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Infrastruture;
using ZasNet.Infrastruture.Persistence;
using ZasNet.Infrastruture.Repositories;
using ZasNet.Infrastruture.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ZasNetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection")));

builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});
builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(nameof(AuthSettings)));

var authSettings = builder.Configuration.GetSection(nameof(AuthSettings)).Get<AuthSettings>()
                      ?? throw new InvalidOperationException("AuthSettings not found");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   IssuerSigningKey =
                       new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.secretKey))
               };
           });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
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
