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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LMS API",
        Version = "v1",
        Description = "API cho hệ thống LMS - Trưởng khoa"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme. Ví dụ: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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
});

builder.Services.AddCors(options =>
{
    // Policy cho development - cho phép tất cả origins
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // Policy cho production - chỉ cho phép specific origins
    options.AddPolicy("AllowAdmin", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:3001",
                "https://localhost:3001"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Token = null;
            }
            return Task.CompletedTask;
        }
    };
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Sử dụng AllowAll trong development, AllowAdmin trong production
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowAdmin");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
