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

// 1. קריאת ההגדרות מתוך appsettings.json > EmailSettings
builder.Services.Configure<EmailSettings>(options =>
{
    options.From = Environment.GetEnvironmentVariable("EMAIL_FROM");
    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
    options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
});

// 2. הגדרת Serilog (לוגים לקונסול ולקובץ)
builder.Logging.AddConsole();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// 3. CORS - מדיניות שמאפשרת לכל מקור
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
      policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// 4. JWT Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 5. Authorization מבוסס תפקידים
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TypeistOnly", policy => policy.RequireRole("typist"));
    options.AddPolicy("ClientOrTypeist", policy => policy.RequireRole("user", "typist"));
    options.AddPolicy("ClientOnly", policy => policy.RequireRole("user"));
});

// 6. DbContext (MySQL via Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 7. AWS S3 Service
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<Is3Service, S3Service>();

// 8. רישום מאגרי הנתונים (Repositories) ושירותי העסקי (Services)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();

// **9. רישום EmailService עצמו** (הבעיה נפתרת כאן)
builder.Services.AddScoped<EmailService>();

// 10. רישום AuthService (אם יש בו תלות ב־EmailService או VerificationService)
builder.Services.AddScoped<AuthService>();

// 11. רישום לוגר של UserService (אם משתמש בו)
builder.Services.AddScoped<ILogger<UserService>, Logger<UserService>>();

// 12. Controllers
builder.Services.AddControllers();

// 13. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // תמיכה ב‑JWT ב‑Swagger
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

// 14. שימוש ב‑CORS middleware
app.UseCors("AllowAll");

// 15. Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// 16. HTTPS redirection, Authentication, Authorization, Static files
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// 17. MapControllers (כשורה אחת מספיק)
app.MapControllers();

app.Run();
