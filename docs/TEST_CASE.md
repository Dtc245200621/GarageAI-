# TEST CASES – GARAGE AI

## KT2 – Kiểm thử chức năng quản lý

| ID | Chức năng | Thao tác kiểm thử | Kết quả mong đợi | Kết quả thực tế |
|---|---|---|---|---|
| TC01 | Khách hàng | Thêm khách hàng hợp lệ | Lưu và hiển thị khách hàng | Đạt – Thêm và hiển thị khách hàng thành công |
| TC02 | Quản lý xe | Thêm xe gắn với khách hàng | Xe xuất hiện trong danh sách | Đạt – Xe được lưu và liên kết đúng khách hàng |
| TC03 | Quản lý xe | Nhập biển số trùng | Hiển thị thông báo phù hợp | Đạt – Hệ thống từ chối biển số trùng |
| TC04 | Lịch hẹn | Tạo lịch hẹn mới | Lịch hẹn được lưu | Đạt – Lịch hẹn được tạo và hiển thị đúng |
| TC05 | Phiếu sửa chữa | Tạo phiếu mới | Phiếu xuất hiện trong danh sách | Đạt – Phiếu sửa chữa được tạo và hiển thị đúng |
| TC06 | Phiếu sửa chữa | Chuyển Mới tiếp nhận sang Đang sửa chữa | Trạng thái được cập nhật | Đạt – Trạng thái chuyển sang Đang sửa chữa thành công |
| TC07 | Lịch sử sửa chữa | Xem lịch sử của xe | Hiển thị đúng các phiếu liên quan | Đạt – Lịch sử sửa chữa hiển thị đúng xe và phiếu liên quan |

## KT3 – Kiểm thử Gemini AI

| ID | Trường hợp | Kết quả mong đợi | Kết quả thực tế |
|---|---|---|---|
| AI01 | Xe có lịch sử sửa chữa | AI tóm tắt dựa trên dữ liệu | Đạt – Gemini tóm tắt đúng lịch sử sửa chữa của xe |
| AI02 | Xe chưa có lịch sử sửa chữa | Hiển thị thông báo phù hợp | Đạt – Hệ thống thông báo xe chưa có lịch sử sửa chữa |
| AI03 | Thiếu Gemini API Key | Hiển thị thông báo cấu hình | Đạt - Hiển thị thông báo chưa cấu hình Gemini API Key |
| AI04 | Gemini API từ chối yêu cầu (API Key giả) | Hiển thị thông báo lỗi phù hợp | Đạt - Hiển thị Gemini API gặp lỗi HTTP 400, ứng dụng vẫn hoạt động |
| AI05 | Phiếu đang sửa chữa | AI không khẳng định đã hoàn thành | Đạt - AI xác nhận xe đang sửa chữa, chưa có công việc nào được xác nhận hoàn thành |
## Kết luận

Đã thực hiện và ghi nhận kết quả đạt cho 12 trường hợp kiểm thử:
- KT2: 7/7 trường hợp đạt.
- KT3: 5/5 trường hợp đạt.

Các chức năng quản lý khách hàng, xe, lịch hẹn, phiếu sửa chữa và lịch sử sửa chữa hoạt động theo các trường hợp đã kiểm thử.

Gemini AI có thể tóm tắt lịch sử sửa chữa dựa trên dữ liệu xe, xử lý trường hợp không có lịch sử, thiếu API Key và API trả lỗi HTTP 400.

Hệ thống cũng thể hiện đúng trạng thái phiếu đang sửa chữa, không tự khẳng định công việc đã hoàn thành.

Hạn chế: Chưa kiểm thử riêng trường hợp API không phản hồi (timeout) và các tình huống tải lớn.