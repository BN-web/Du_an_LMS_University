using Microsoft.IdentityModel.Tokens;
using System.Text;
/*using LMS_GV.Models.Data;*/ // import namespace DbContext 
using Microsoft.EntityFrameworkCore; // để sử dụng UseSqlServer


var builder = WebApplication.CreateBuilder(args);

var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 32)
    throw new Exception("JWT Key không hợp lệ hoặc quá ngắn!");

// Add services to the container.
builder.Services.AddControllers();

//Đăng ký DbContext để dependency injection hoạt động
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình JWT Authentication
builder.Services.AddAuthentication("Bearer") // nhớ sau này phải dùng app.UseAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],   // config trong appsettings.json
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
//cấu hình Swagger để nhập Bearer token
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập Bearer token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement{
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme{ Reference = new Microsoft.OpenApi.Models.OpenApiReference{
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer"
            }}, new string[]{}
        }
    });
});


////đăng ký service JwtService vào DI container: AddScoped nghĩa là mỗi request HTTP sẽ có một instance riêng. AddSingleton nghĩa là toàn bộ ứng dụng dùng chung 1 instance.
//builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Authentication phải gọi trước Authorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
