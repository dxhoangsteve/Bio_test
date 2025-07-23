// --- PHẦN 1: KHAI BÁO CÁC THƯ VIỆN CẦN DÙNG ---
using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Data;
using Scalar.AspNetCore;

// --- PHẦN 2: "CHUẨN BỊ CÔNG CỤ" (ĐĂNG KÝ SERVICES) ---
var builder = WebApplication.CreateBuilder(args);

// 2.1. Lấy chuỗi kết nối từ file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2.2. Đăng ký "Người quản lý kho" DbContext
// Ra lệnh: "Khi ai đó cần ApplicationDbContext, hãy tạo nó và dùng chuỗi kết nối SQL Server này"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2.3. Không cần Identity nữa, sử dụng authentication đơn giản

// 2.4. Đăng ký các dịch vụ cần thiết cho Controller và API docs
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 2.5. Thêm Scalar UI
builder.Services.AddOpenApi();

// 2.6. Đăng ký services
builder.Services.AddScoped<BioWeb.Server.Services.IAuthService, BioWeb.Server.Services.AuthService>();
builder.Services.AddScoped<BioWeb.Server.Services.ISiteConfigurationService, BioWeb.Server.Services.SiteConfigurationService>();
builder.Services.AddScoped<BioWeb.Server.Services.IProjectService, BioWeb.Server.Services.ProjectService>();
builder.Services.AddScoped<BioWeb.Server.Services.IArticleService, BioWeb.Server.Services.ArticleService>();
builder.Services.AddScoped<BioWeb.Server.Services.ICategoryService, BioWeb.Server.Services.CategoryService>();

// --- PHẦN 3: XÂY DỰNG ỨNG DỤNG ---
var app = builder.Build();


// --- PHẦN 4: TỰ ĐỘNG HÓA (CHẠY SEED DATA KHI ỨNG DỤNG KHỞI ĐỘNG) ---
// Đây là đoạn code quan trọng để tự động tạo user và data ban đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ra lệnh: "Hãy tìm đến lớp SeedData và chạy hàm InitializeAsync"
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        // Ghi lại lỗi nếu có sự cố xảy ra trong quá trình seed data
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// --- PHẦN 5: CẤU HÌNH "DÂY CHUYỀN XỬ LÝ" (MIDDLEWARE PIPELINE) ---
// Cấu hình thứ tự xử lý khi có một request gửi đến server

// Nếu đang trong môi trường phát triển, bật API docs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Thêm Scalar UI tại /scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Tự động chuyển hướng từ http:// sang https://
app.UseHttpsRedirection();

// Không cần authentication/authorization middleware nữa

// Tìm đến Controller phù hợp để xử lý request
app.MapControllers();


// --- PHẦN 6: CHẠY ỨNG DỤNG ---
app.Run();