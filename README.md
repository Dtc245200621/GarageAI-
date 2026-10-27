# Garage AI – Hệ thống quản lý garage sửa chữa ô tô tích hợp AI

## 1. Giới thiệu dự án

Garage AI là ứng dụng quản lý garage sửa chữa ô tô được xây dựng bằng ASP.NET Core MVC (.NET 8), C# và SQLite.

Hệ thống hỗ trợ quản lý khách hàng, phương tiện, lịch hẹn và phiếu sửa chữa. Ngoài ra, ứng dụng tích hợp Gemini AI để tóm tắt lịch sử sửa chữa của từng xe.

## 2. Công nghệ sử dụng

- Backend: ASP.NET Core MVC (.NET 8), C#
- Frontend: Razor Views, HTML, CSS, Bootstrap
- Database: SQLite
- ORM: Entity Framework Core
- AI: Google Gemini API
- IDE: Visual Studio 2022
- Quản lý mã nguồn: Git và GitHub

## 3. Các chức năng đã triển khai

### 3.1. Quản lý khách hàng
- Hiển thị danh sách khách hàng.
- Thêm, tìm kiếm và quản lý thông tin khách hàng.

### 3.2. Quản lý xe
- Hiển thị danh sách xe và thông tin chủ sở hữu.
- Thêm, chỉnh sửa, xóa và tìm kiếm xe.
- Liên kết xe với khách hàng.

### 3.3. Quản lý lịch hẹn
- Tạo và chỉnh sửa lịch hẹn.
- Quản lý trạng thái lịch hẹn.
- Tìm kiếm và lọc lịch hẹn.

### 3.4. Quản lý phiếu sửa chữa
- Tạo và chỉnh sửa phiếu sửa chữa.
- Phân công kỹ thuật viên.
- Cập nhật trạng thái sửa chữa.
- Tìm kiếm phiếu theo biển số hoặc khách hàng.
- Xem lịch sử sửa chữa theo từng xe.

### 3.5. Tích hợp Gemini AI
- Gửi dữ liệu lịch sử sửa chữa đến Gemini API.
- Tóm tắt các vấn đề xe từng gặp.
- Tổng hợp công việc sửa chữa đã ghi nhận.
- Đưa ra những điểm cần lưu ý khi kiểm tra tiếp.
- Hiển thị kết quả bằng tiếng Việt.
- Thông báo khi thiếu API Key hoặc dịch vụ AI gặp lỗi.

Lưu ý: Nội dung do AI tạo cần được kỹ thuật viên kiểm tra trước khi sử dụng.

## 4. Hướng dẫn cài đặt và chạy

### Bước 1: Chuẩn bị môi trường

Cài đặt:
- Visual Studio 2022 với workload ASP.NET and web development.
- .NET 8 SDK.
- Git.

### Bước 2: Mở dự án

Mở file `GarageAI.slnx` bằng Visual Studio 2022.

Chờ Visual Studio khôi phục các gói NuGet cần thiết.

### Bước 3: Cấu hình Gemini API Key

Tạo API Key từ Google AI Studio.

Cấu hình biến môi trường có tên:

`GEMINI_API_KEY`

Không ghi API Key trực tiếp vào mã nguồn và không đẩy API Key lên GitHub.

Sau khi cấu hình, khởi động lại ứng dụng để nhận biến môi trường.

### Bước 4: Chạy ứng dụng

Nhấn `Ctrl + F5` trong Visual Studio hoặc chạy lệnh:

```powershell
dotnet run
```

Truy cập địa chỉ HTTPS được hiển thị khi ứng dụng khởi chạy.

### Bước 5: Kiểm thử

1. Thêm khách hàng.
2. Thêm xe thuộc khách hàng.
3. Tạo lịch hẹn.
4. Tạo phiếu sửa chữa cho xe.
5. Chuyển trạng thái phiếu sửa chữa.
6. Mở lịch sử sửa chữa.
7. Nhấn nút AI tóm tắt lịch sử sửa chữa.
8. Kiểm tra kết quả và thông báo lỗi khi thiếu API Key.

## 5. Cơ sở dữ liệu

Hệ thống sử dụng SQLite và Entity Framework Core.

Các đối tượng dữ liệu chính:
- Customers
- Vehicles
- Appointments
- RepairOrders

File dữ liệu SQLite được loại khỏi Git bằng `.gitignore` để tránh công khai dữ liệu cục bộ.

## 6. Lộ trình phát triển

- KT1: Phân tích nghiệp vụ, thiết kế hệ thống và cơ sở dữ liệu.
- KT2: Xây dựng các chức năng quản lý và kiểm thử.
- KT3: Tích hợp Gemini AI, kiểm thử và xử lý lỗi.
- Giai đoạn cuối: Hoàn thiện bảo mật, tài liệu, triển khai và demo.

## 7. Lưu ý bảo mật

- Không commit API Key, mật khẩu hoặc thông tin bí mật.
- Không lưu dữ liệu khách hàng thật trong repository công khai.
- Kiểm tra kết quả do AI sinh ra trước khi áp dụng vào thực tế.
- Sử dụng HTTPS khi triển khai hệ thống.