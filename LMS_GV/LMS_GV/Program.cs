using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 32)
    throw new Exception("JWT Key không hợp lệ hoặc quá ngắn!");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Tăng kích thước upload file
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
});

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddHttpContextAccessor();

// Đăng ký HttpClientFactory để tải ảnh từ URL
builder.Services.AddHttpClient();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });

    // THÊM CẤU HÌNH NÀY để xử lý TimeOnly
    c.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "time"
    });

    // THÊM CẤU HÌNH NÀY để xử lý TimeSpan (bắt buộc cho API TruongKhoa)
    // Sử dụng format "string" đơn giản vì Swagger có thể gặp vấn đề với TimeSpan
    c.MapType<TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Description = "TimeSpan format: HH:mm:ss (e.g., 08:00:00, 14:30:00)",
        Example = new Microsoft.OpenApi.Any.OpenApiString("08:00:00")
    });
    
    // Cấu hình TimeSpan? (nullable)
    c.MapType<TimeSpan?>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Description = "TimeSpan format: HH:mm:ss (e.g., 08:00:00, 14:30:00)",
        Nullable = true,
        Example = new Microsoft.OpenApi.Any.OpenApiString("08:00:00")
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập Bearer token (không cần thêm 'Bearer ' phía trước)",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Cấu hình để Swagger hiểu IFormFile
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    // Bỏ qua các property đánh dấu obsolete
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    
    // Xử lý lỗi khi generate schema - đảm bảo schema ID unique
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

var app = builder.Build();

app.UseDeveloperExceptionPage();

// Tạo thư mục uploads nếu chưa tồn tại
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

var avatarsPath = Path.Combine(uploadsPath, "avatars");
if (!Directory.Exists(avatarsPath))
{
    Directory.CreateDirectory(avatarsPath);
}

// Cấu hình static files để truy cập file upload từ thư mục uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS_GV API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseStaticFiles();
// Authentication phải gọi trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();