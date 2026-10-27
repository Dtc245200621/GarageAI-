# Garage AI - Giai đoạn khởi tạo

Bản chạy đầu tiên bằng ASP.NET Core MVC (.NET 8) và SQLite để không cần cài SQL Server ngay. Bao gồm dashboard và quản lý khách hàng (thêm, xem, tìm kiếm, xóa). **Chưa** có đăng nhập/phân quyền, quản lý xe, lịch hẹn, hóa đơn, AI hay chức năng sửa khách hàng. Không dùng bản này để lưu dữ liệu khách hàng thật trên internet.

## Chạy trên Visual Studio 2022
1. Cài Visual Studio 2022 với workload **ASP.NET and web development**, có .NET 8 SDK.
2. Giải nén và mở `GarageAI.csproj` bằng Visual Studio.
3. Chờ NuGet restore. Nhấn `Ctrl+F5` hoặc nút chạy HTTP/HTTPS.
4. Chọn **Khách hàng** để thêm và tìm kiếm dữ liệu thử.

Database `garage.db` được tự tạo ở thư mục làm việc khi chạy lần đầu.

## Lộ trình
KT1: phân tích nghiệp vụ, use case, ERD, kiến trúc, prompt và tài liệu.
KT2: xác thực/phân quyền, CRUD xe, lịch hẹn, phiếu sửa chữa, hóa đơn, dashboard, kiểm thử.
KT3: AI qua backend, prompt riêng, dữ liệu có kiểm soát, kiểm thử timeout và lỗi model.
Cuối kỳ: bảo mật, test, triển khai, báo cáo và demo.
