
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LMS_GV.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 32)
    throw new Exception("JWT Key không hợp lệ hoặc quá ngắn!");

// 1. Add services
builder.Services.AddControllers(); // ✅ Bắt buộc để MapControllers hoạt động

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 2. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. JWT Authentication
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

// 4. Authorization
builder.Services.AddAuthorization();

// 5. Swagger
builder.Services.AddHttpContextAccessor();

// Đăng ký HttpClientFactory để tải ảnh từ URL
builder.Services.AddHttpClient();

// Swagger/OpenAPI
>>>>>>> origin/Hocvien-Backend
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{

    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableTimeOnlyJsonConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdmin", policy =>

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
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme{
                Reference = new Microsoft.OpenApi.Models.OpenApiReference{
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[]{}
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

// 6. JWT Service
builder.Services.AddScoped<JwtService>();
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization();



var app = builder.Build();


app.UseStaticFiles();

// Middleware pipeline
// Chỉ bật Swagger trong môi trường Development
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Swagger UI xuất hiện trực tiếp tại /
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS_GV API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}



app.UseHttpsRedirection();
// CORS trước Authentication & Authorization
app.UseCors("AllowFrontend");
app.UseCors("AllowAdmin");
app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseStaticFiles();
// Authentication phải gọi trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Bắt buộc nếu dùng [ApiController]

app.Run();

sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    const string F = "yyyy-MM-dd";
    public override DateOnly Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => DateOnly.ParseExact(r.GetString()!, F, CultureInfo.InvariantCulture);
    public override void Write(Utf8JsonWriter w, DateOnly v, JsonSerializerOptions o)
        => w.WriteStringValue(v.ToString(F));
}

sealed class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    const string F = "yyyy-MM-dd";
    public override DateOnly? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
    {
        var s = r.GetString();
        return string.IsNullOrEmpty(s) ? (DateOnly?)null : DateOnly.ParseExact(s!, F, CultureInfo.InvariantCulture);
    }
    public override void Write(Utf8JsonWriter w, DateOnly? v, JsonSerializerOptions o)
    {
        if (v.HasValue) w.WriteStringValue(v.Value.ToString(F)); else w.WriteNullValue();
    }
}

sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    const string F = "HH:mm:ss";
    public override TimeOnly Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
        => TimeOnly.ParseExact(r.GetString()!, F, CultureInfo.InvariantCulture);
    public override void Write(Utf8JsonWriter w, TimeOnly v, JsonSerializerOptions o)
        => w.WriteStringValue(v.ToString(F));
}

sealed class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    const string F = "HH:mm:ss";
    public override TimeOnly? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
    {
        var s = r.GetString();
        return string.IsNullOrEmpty(s) ? (TimeOnly?)null : TimeOnly.ParseExact(s!, F, CultureInfo.InvariantCulture);
    }
    public override void Write(Utf8JsonWriter w, TimeOnly? v, JsonSerializerOptions o)
    {
        if (v.HasValue) w.WriteStringValue(v.Value.ToString(F)); else w.WriteNullValue();
    }
}
app.Run();
