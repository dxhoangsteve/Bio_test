# BioWeb - Personal Portfolio

Website cá nhân & Portfolio với hệ thống quản lý nội dung tích hợp.

---

## Screenshots

| Home | Projects | Articles |
|:----:|:--------:|:--------:|
| ![Home](image/0050028e-aa33-4396-8fc9-e7b694479c8c.jfif) | ![Projects](image/675749be-be5e-48c5-ac40-d89d24a09292.jfif) | ![Articles](image/69ef6809-03ec-4587-85d4-031e2f6fc040.jfif) |

| Admin Dashboard | Configuration | Content Edit |
|:---------------:|:-------------:|:------------:|
| ![Admin](image/aa9fa5ca-2e34-4f92-ac4b-25a5ff90c838.jfif) | ![Config](image/e3ef95b1-d6a5-42b5-a9dc-fe7ff6bd6317.jfif) | ![Edit](image/ef223998-53b8-46cf-a7ce-43673177046e.jfif) |

---

## Giới thiệu

BioWeb là một website portfolio cá nhân hiện đại, được xây dựng theo kiến trúc **Client-Server** với công nghệ Blazor WebAssembly. Dự án bao gồm:

- **Frontend**: Blazor WebAssembly (C#)
- **Backend**: .NET 8 Web API
- **Database**: SQLite (nhẹ, portable)
- **Hosting**: Hỗ trợ deploy lên dxhoang.site

---

## Tính năng chính

### 🌐 Dành cho Khách truy cập

- **Trang chủ** - Hiển thị thông tin cá nhân, avatar, bio
- **Xem dự án** - Danh sách các project đã thực hiện
- **Đọc bài viết** - Blog/Articles được phân loại theo category
- **Liên hệ** - Thông tin liên lạc, social links

### 🛠 Dành cho Admin

- **Quản lý dự án** - Thêm/sửa/xóa project portfolio
- **Quản lý bài viết** - Viết và quản lý blog posts
- **Quản lý danh mục** - Phân loại nội dung
- **Cấu hình site** - Thay đổi thông tin cá nhân, avatar, theme
- **Upload media** - Quản lý hình ảnh và tài liệu

---

## Cấu trúc dự án

```
Bio_test/
├── BioWeb.server/           # .NET Web API Backend
│   ├── Controllers/         # API Controllers
│   │   ├── ArticleController.cs
│   │   ├── AuthController.cs
│   │   ├── CategoryController.cs
│   │   ├── ProjectController.cs
│   │   ├── SiteConfigurationController.cs
│   │   └── UploadController.cs
│   ├── Services/            # Business Logic Layer
│   ├── Models/              # Entity Models
│   ├── ViewModels/          # DTOs
│   └── Program.cs           # Entry point
├── BioWeb.client/           # Blazor WebAssembly Frontend
│   ├── Components/          # Razor Components
│   ├── Services/            # API Client Services
│   ├── Models/              # Client-side Models
│   └── wwwroot/             # Static assets
├── BioWeb.Shared/           # Shared code giữa Client & Server
└── image/                   # Screenshots
```

---

## Cài đặt và Chạy

### Yêu cầu

- .NET 8 SDK
- Visual Studio 2022 hoặc VS Code

### Chạy ứng dụng

```bash
# Clone repository
git clone https://github.com/dxhoangsteve/Bio_test.git
cd Bio_test

# Chạy backend (server sẽ khởi động tại https://localhost:7255)
cd BioWeb.server
dotnet run

# Hoặc chạy cả solution
dotnet run --project BioWeb.server
```

Server sẽ chạy tại `https://localhost:7255`

- API Docs: `https://localhost:7255/swagger`
- Scalar UI: `https://localhost:7255/scalar`
- Health Check: `https://localhost:7255/health`

---

## API Endpoints

### Authentication

| Method | Endpoint        | Mô tả          |
| ------ | --------------- | -------------- |
| POST   | `/api/auth`     | Đăng nhập      |

### Projects

| Method | Endpoint            | Mô tả               |
| ------ | ------------------- | ------------------- |
| GET    | `/api/project`      | Lấy danh sách dự án |
| POST   | `/api/project`      | Tạo dự án mới       |
| PUT    | `/api/project/{id}` | Cập nhật dự án      |
| DELETE | `/api/project/{id}` | Xóa dự án           |

### Articles

| Method | Endpoint            | Mô tả               |
| ------ | ------------------- | ------------------- |
| GET    | `/api/article`      | Lấy danh sách bài viết |
| POST   | `/api/article`      | Tạo bài viết mới    |
| PUT    | `/api/article/{id}` | Cập nhật bài viết   |
| DELETE | `/api/article/{id}` | Xóa bài viết        |

### Site Configuration

| Method | Endpoint                  | Mô tả                  |
| ------ | ------------------------- | ---------------------- |
| GET    | `/api/siteconfiguration`  | Lấy cấu hình site      |
| PUT    | `/api/siteconfiguration`  | Cập nhật cấu hình site |

---

## Production Deployment

Website đã được deploy tại: **[dxhoang.site](https://dxhoang.site)**

### Environment Variables

```bash
# Production domain
ProductionDomain=dxhoang.site
```

---

## License

MIT License - Hoàng Đức Xuân

