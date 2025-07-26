using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Models;
using BioWeb.Server.Services;

namespace BioWeb.Server.Data
{
    public static class SeedData
    {
        // Phương thức gọi từ Program.cs
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("Tạo data ban đầu");

            await context.Database.MigrateAsync();
            Console.WriteLine("Đã tạo bản cấu trúc dữ liệu");

            // Tạo từng bảng sử dụng await để tránh xung đột
            await SeedAdminUsers(context);
            await SeedSiteConfigurations(context);
            await SeedCategories(context);
            await SeedProjects(context);
            await SeedArticles(context);

            Console.WriteLine("Đã tạo toàn bộ bảng. Tiếp theo drop data ban đầu");
        }

        private static async Task SeedAdminUsers(ApplicationDbContext context)
        {
            if (await context.AdminUsers.AnyAsync())
            {
                Console.WriteLine("Đang tạo user admin của bio");
                return;
            }
            var adminUser = new AdminUser
            {
                Username = "admin",  // Default username - có thể đổi thành tên bạn muốn
                PasswordHash = PasswordService.HashPassword("BioWeb2024!"), // Password mạnh hơn
                LastLogin = null
            };

            context.AdminUsers.Add(adminUser);
            await context.SaveChangesAsync();
            Console.WriteLine("pass");
        }

        private static async Task SeedSiteConfigurations(ApplicationDbContext context)
        {
            if (await context.SiteConfigurations.AnyAsync())
            {
                Console.WriteLine("Đang cấu hình trang web");
                return;
            }
            var siteConfig = new SiteConfiguration
            {
                FullName = "Đinh Xuân Hoàng",
                JobTitle = "Full-stack Developer",
                AvatarURL = "", // Placeholder avatar
                BioSummary = "Tôi là Đinh Xuân Hoàng, lập trình viên mobile và đang trên con đường học tập web để trở thành fullstack developer sau đó là AI. Tôi luôn cố gắng học hỏi công nghệ sử dụng mới để biết thêm nhiều kiến thức. Cảm ơn bạn đã xem bio này!!",
                Email = "sterbe2k4@gmail.com",
                PhoneNumber = "+84 329474859",
                Address = "Quận Tân Bình, Thành phố Hồ Chí Minh",
                GitHubURL = "https://github.com/dxhoangsteve",
                LinkedInURL = "",
                FacebookURL = "https://www.facebook.com/Wikileakss",
                CV_FilePath = "", // Placeholder CV
                UpdatedAt = DateTime.UtcNow
            };
            context.SiteConfigurations.Add(siteConfig);
            await context.SaveChangesAsync();
            Console.WriteLine("pass");
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
            {
                Console.WriteLine("Đang tạo các mục trong trang web để viết blog");
                return;
            }
            var categories = new List<Category>
            {
                new Category { CategoryName = "Lập Trình" }
                // new Category { CategoryName = "Hướng Dẫn" },
                // new Category { CategoryName = "Chia Sẻ Cá Nhân" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("pass");
        }

        private static async Task SeedProjects(ApplicationDbContext context)
        {
            if (await context.Projects.AnyAsync())
            {
                Console.WriteLine("Đang tạo project");
                return;
            }
            var projects = new List<Project>
            {
                new Project
                {
                    ProjectName = "Bio Website",
                    Description = "Website cá nhân được xây dựng bằng ASP.NET Core và Blazor",
                    GitHubURL = "https://github.com/dxhoangsteve/Bio_test",
                    ProjectURL = "",
                    ThumbnailURL = "",
                    Technologies = "ASP.NET Core, Blazor, Entity Framework, SQL Server",
                    DisplayOrder = 1,
                    IsPublished = true
                },
                new Project
                {
                    ProjectName = "CKCQUIZZ - Ứng dụng di động và website thi trắc nghiệm",
                    Description = "Ứng dụng mobile demo sử dụng React Native",
                    GitHubURL = "https://github.com/thongle321/CKCQUIZZ",
                    ProjectURL = "",
                    ThumbnailURL = "",
                    Technologies = "VueJS, .Net core 9, SQL Server, Flutter",
                    DisplayOrder = 2,
                    IsPublished = true
                }
            };

            context.Projects.AddRange(projects);
            await context.SaveChangesAsync();
            Console.WriteLine("pass");
        }

        private static async Task SeedArticles(ApplicationDbContext context)
        {
            if (await context.Articles.AnyAsync())
            {
                Console.WriteLine("Đang tạo bài viết");
                return;
            }
            var techCategoryId = context.Categories.First(c => c.CategoryName == "Lập Trình").CategoryID;

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Về mình - Đinh Xuân Hoàng",
                    Content = "Xuất phát điểm của mình ở trường là mobile developer. Nhưng sau này mình rất hứng thú với web developer, nên mình đang cố gắng từng chút một học web backend do đã làm quen từ trước rồi sau đó là frontend và cuối cùng là fullstack developer."
                    +"Đây là trang blog mình tạo ra với ngôn ngữ C# cho cả backend lẫn frontend. Frontend mình sử dụng framework Blazor để làm trang web này. Backend mình sử dụng ASP.NET Core để làm API cho trang web này.",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    CategoryID = techCategoryId
                }
                // new Article
                // {
                //     Title = "Hành trình học lập trình",
                //     Content = "",
                //     IsPublished = true,
                //     CreatedAt = DateTime.UtcNow.AddDays(-1),
                //     CategoryID = techCategoryId
                // }
            };

            context.Articles.AddRange(articles);
            await context.SaveChangesAsync();
            Console.WriteLine("pass");
        }

    }
}
