using KasKelasApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

// ============================
// Konfigurasi Builder & Service
// ============================
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<KasKelasApiDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Add Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(
            "http://213.35.123.110:5173",
            "http://103.31.235.237:5173",
            "https://213.35.123.110:5173",
            "https://103.31.235.237:5173",
            "http://213.35.123.110:5280",
            "http://103.31.235.237:5280",
            "https://213.35.123.110:5280",
            "https://103.31.235.237:5280",
            "http://213.35.123.110:5279",
            "http://103.31.235.237:5279",
            "https://213.35.123.110:5279",
            "https://103.31.235.237:5279",
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "https://localhost:5173",
            "https://127.0.0.1:5173",
            "http://localhost:5280",
            "https://localhost:5280",
            "http://localhost:5279",
            "https://kas.mbindotama.com",
            "https://localhost:5279",
            "https://kas.kondangcloud.my.id",
            "https://kas1.kondangcloud.my.id",
            "http://192.168.100.140:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kas Kelas REST API",
        Version = "v1",
        Description = "API untuk mengelola kas kelas, pembayaran, pengeluaran, dan notifikasi"
    });

    // Mendukung file upload
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

 
    options.OperationFilter<FormFileOperationFilter>();

    // Dokumentasi XML (jika ada)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// ============================
// Build & Middleware Pipeline
// ============================
var app = builder.Build();

// Tambahkan baris ini agar wwwroot bisa diakses publik
app.UseStaticFiles();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
