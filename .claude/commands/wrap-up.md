---
description: Kết thúc phiên làm việc — chạy build/test, cập nhật devlog và CLAUDE.md, dọn dẹp, tóm tắt
---

# /wrap-up

Kết thúc phiên làm việc hiện tại. Thực hiện các bước sau theo thứ tự.

Lưu ý: KHÔNG chạy bất kỳ lệnh `git` nào (không `git status`, không `git diff`, không commit) — môi trường này không có git khả dụng qua shell. Bỏ qua hoàn toàn phần kiểm tra Git.

## 1. Chạy kiểm tra sức khỏe code
- Chạy build/test suite hiện có (`dotnet build`, `npm run build`, test command...)
- Nếu fail: ghi rõ lỗi gì, KHÔNG được báo "wrap-up thành công" nếu code đang broken
- Nếu chưa xong việc và cố ý để code ở trạng thái broken: ghi rõ trong devlog lý do và cách để tiếp tục

## 2. Cập nhật devlog (docs/devlog/YYYY-MM-DD.md)
Ghi theo cấu trúc:
- **Đã làm**: liệt kê việc cụ thể (file, function, feature)
- **Quyết định quan trọng & lý do**: bất kỳ trade-off, approach đã bỏ, hoặc lý do chọn 1 giải pháp thay vì giải pháp khác
- **Vấn đề còn tồn đọng / blocked**: nêu rõ, kèm lý do nếu biết
- **Next steps**: liệt kê cụ thể, có thứ tự ưu tiên (không viết mơ hồ kiểu "tiếp tục sửa bug")

## 3. Cập nhật CLAUDE.md
- Cập nhật phần "Current State" — mô tả ngắn gọn trạng thái dự án NGAY BÂY GIỜ
- Cập nhật phần "Next Steps" — đồng bộ với devlog
- Xóa các ghi chú/next-steps đã hoàn thành trong phiên này

## 4. Dọn dẹp
- Liệt kê các file tạm/debug/log không cần thiết được tạo ra trong phiên, hỏi tôi có muốn xóa không

## 5. Tóm tắt cuối
In ra bản tóm tắt ngắn (5-7 dòng) gồm:
- Trạng thái hiện tại (✅ ổn định / ⚠️ có vấn đề / 🚧 đang dở)
- Việc đã hoàn thành
- Việc tiếp theo cần làm khi mở phiên mới
