# ADR 003: Keyword có 2 flag soft-delete riêng biệt (IsHidden vs Deleted)

**Ngày:** 2026-05-28  
**Trạng thái:** Chấp thuận

---

## Bối cảnh

Admin cần khả năng ẩn field khỏi form builder mà không mất dữ liệu lịch sử.
`BaseModel.Deleted = true` là soft-delete toàn hệ thống — ẩn khỏi mọi nơi.
Nếu chỉ dùng `Deleted`, admin không thể ẩn field mà vẫn giữ CaseKeyword data.

Thêm vào đó: nếu keyword đang được dùng trong Cases, không thể xóa tùy tiện.

## Quyết định

Thêm `Keyword.IsHidden` (bool, default false) với ngữ nghĩa riêng:

| Flag | Người set | Ảnh hưởng |
|---|---|---|
| `Deleted = true` | System/admin muốn xóa hoàn toàn | Ẩn khỏi mọi nơi: admin UI, case form, queries |
| `IsHidden = true` | Admin ẩn field trong Form Builder | Ẩn khỏi admin Form Builder UI, nhưng CaseKeyword data còn nguyên |

**Guard rule:** Nếu keyword có CaseKeyword references:
- `SoftDeleteAsync` trả `-1`
- Controller trả `409 Conflict`
- Frontend bắt `error.response?.status === 409` và hiển thị message riêng

**Query logic:**
- `GetByTemplateIdAsync` (cho case form): filter `!IsHidden && !Deleted`
- `GetByTemplateIdForBuilderAsync` (cho admin): filter `!Deleted` only (thấy cả IsHidden)

## Hệ quả

- CaseKeyword data được bảo toàn khi admin ẩn field
- Admin thấy hidden keywords trong builder (bgcolor khác) và có thể restore
- API DELETE trả 409 rõ ràng thay vì silently fail
- Frontend xử lý 409 với message tiếng Nhật riêng

## Các phương án đã cân nhắc

**Phương án loại bỏ — chỉ dùng Deleted:**
Lý do loại bỏ: `Deleted = true` ẩn khỏi mọi nơi, mất CaseKeyword history.

**Phương án loại bỏ — cascade delete CaseKeyword:**
Lý do loại bỏ: mất dữ liệu hồ sơ đã nhập — không chấp nhận được.
