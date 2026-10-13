using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QLTBYT.API.Configuration;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Infrastructure;
using QLTBYT.API.Middleware;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Services;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/qltbyt-.log", rollingInterval: RollingInterval.Day));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<FirebaseSettings>(builder.Configuration.GetSection("FirebaseSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<QLTBYT.API.Validators.CreateDeviceRequestValidator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceEventService, DeviceEventService>();
builder.Services.AddScoped<ISystemAuditService, SystemAuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IUsageLogService, UsageLogService>();
builder.Services.AddScoped<IBiddingService, BiddingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdminMasterDataService, AdminMasterDataService>();
builder.Services.AddScoped<ILegalDocumentService, LegalDocumentService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IPdfSignatureService, PdfSignatureService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IEmailNotificationService, MailKitEmailNotificationService>();
builder.Services.AddSingleton<IFirebasePushService, FirebasePushNotificationService>();
builder.Services.AddScoped<IIncidentAlertService, IncidentAlertService>();
builder.Services.AddHostedService<DeviceExpiryNotificationJob>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHostedService<DevelopmentDataSeeder>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("TechnicianAndAbove", p => p.RequireRole("Admin", "Technician"));
    options.AddPolicy("AllUsers", p => p.RequireRole("Admin", "Technician", "User"));

    // Legacy baocao.aspx: LoaiTK 0,1,2 được vào; LoaiTK 3 bị chặn
    options.AddPolicy("ReportsAccess", p => p.RequireAssertion(ctx =>
    {
        if (ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Technician"))
            return true;
        var loai = ctx.User.FindFirst("LoaiTK")?.Value;
        return loai is "0" or "1" or "2";
    }));

    // Legacy goithau.aspx: chỉ LoaiTK 0,1
    options.AddPolicy("BiddingAccess", p => p.RequireAssertion(ctx =>
    {
        if (ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Technician"))
            return true;
        var loai = ctx.User.FindFirst("LoaiTK")?.Value;
        return loai is "0" or "1";
    }));

    // Legacy Nhaphanghoa/baocaokho: LoaiTK 0 hoặc 3
    options.AddPolicy("WarehouseAccess", p => p.RequireAssertion(ctx =>
    {
        if (ctx.User.IsInRole("Admin"))
            return true;
        var loai = ctx.User.FindFirst("LoaiTK")?.Value;
        return loai is "0" or "3";
    }));
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Dữ liệu không hợp lệ." : x.ErrorMessage))
                .ToList();

            var response = ApiResponse<object>.CreateFail("Dữ liệu không hợp lệ.", errors);
            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "QLTBYT API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

var fileRoot = Path.Combine(app.Environment.ContentRootPath, "FILELUU");
Directory.CreateDirectory(fileRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(fileRoot),
    RequestPath = "/files"
});

var capturesRoot = Path.Combine(app.Environment.ContentRootPath, "Captures");
Directory.CreateDirectory(capturesRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(capturesRoot),
    RequestPath = "/captures"
});

var hdBaotriRoot = Path.Combine(app.Environment.ContentRootPath, "HDBaotri");
Directory.CreateDirectory(hdBaotriRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(hdBaotriRoot),
    RequestPath = "/hdbaotri"
});
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await LegacySchemaEnsurer.EnsureMatkhauForBcryptAsync(db);
}

app.Run();

public partial class Program { }