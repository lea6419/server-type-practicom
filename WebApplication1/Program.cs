

using Amazon.Auth.AccessControlPolicy;
using Amazon.S3;
using Core.Interface.Repositories;
using Data;
using Data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Service;
using Service.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//builder.WebHost.UseUrls("http://*:80");
// קריאת ההגדרות מתוך appsettings.json > EmailSettings
builder.Services.Configure<EmailSettings>(options =>
{
    options.From = Environment.GetEnvironmentVariable("EMAIL_FROM");
    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
    options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
});

builder.Logging.AddConsole();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

//builder.Host.UseSerilog();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
      policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});


// הוספת JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// הוספת הרשאות מבוססות-תפקידים
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TypeistOnly", policy => policy.RequireRole("typist"));
    options.AddPolicy("ClientOrTypeist", policy => policy.RequireRole("user", "typist"));
    options.AddPolicy("ClientOnly", policy => policy.RequireRole("user", "typist"));


});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddAWSService<IAmazonS3>();
// רישום השירות של S3
IServiceCollection serviceCollection = builder.Services.AddScoped<Is3Service, S3Service>();
// Add services to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<VerificationService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ILogger<UserService>, Logger<UserService>>();
builder.Services.AddControllers();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // ✅ הגדרת תמיכה ב-JWT ב-Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();


// שימוש ב-CORS middleware
app.UseCors("AllowAll");


//Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();
app.UseStaticFiles(); // הפעלת הגשת קבצים סטטיים

app.MapControllers();

app.MapControllers();

app.Run();
