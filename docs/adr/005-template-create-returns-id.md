# ADR 005: Template create trả Guid? và redirect đến KeywordBuilder (Hướng B)

**Ngày:** 2026-05-28  
**Trạng thái:** Chấp thuận

---

## Bối cảnh

`POST /api/template` ban đầu yêu cầu `KeywordRequests.Count > 0` và trả `int` (rows affected).
Frontend không thể tạo template rỗng, và không có cách biết ID của template vừa tạo để redirect.

Cần một flow UX hướng dẫn user: tạo template → ngay lập tức đến KeywordBuilder để thêm fields.

## Quyết định

**Backend:**
- `ITemplateService.AddAsync` đổi return type từ `Task<int>` → `Task<Guid?>` (trả template ID mới)
- `TemplateService.AddAsync`:
  - Dùng `request.Name` làm tên template (thay vì hardcode `$"Template {DateTime.Today}"`)
  - Cho phép `KeywordRequests` rỗng — skip keyword creation block nếu empty
  - Tự động tạo `CompanyTemplate` record để link template với company (dùng `request.CompanyId`)
  - Trả `template.Id` thay vì rows count
- `TemplateController.Create`:
  - Bỏ guard `KeywordRequests.Count <= 0`
  - Lấy `CompanyId` từ JWT claim (`"CompanyId"`) thay vì từ request body
  - Trả `Ok(templateId)` — Guid

**Frontend (`TemplateList.jsx`):**
- Sau create thành công: `navigate(/admin/templates/${response.data}/keywords)`
- Không reload template list — user ở lại KeywordBuilder

**`TemplateRequest` model:**
- Thêm `string? Name`
- Bỏ `[Required]` khỏi `CompanyId` và `KeywordRequests`
- `KeywordRequests` init `= new()` (không null)

## Hệ quả

- Template luôn được link với company ngay khi tạo (qua CompanyTemplate) — không bao giờ "orphan template"
- `ITemplateService` interface thay đổi — nếu có code khác gọi `AddAsync` cần kiểm tra lại
- UX rõ ràng: tạo xong → thêm fields ngay, không phải nhớ vào KeywordBuilder thủ công
- `TemplateRequest.CompanyId` không cần gửi từ frontend — controller override bằng JWT claim

## Các phương án đã cân nhắc

**Hướng A — Relax backend, giữ return int:**
- Bỏ guard, cho phép empty keywords, trả `Ok(1)`
- Frontend hiện snackbar thành công → user tự navigate vào KeywordBuilder
- Lý do loại bỏ: UX friction cao, user có thể bỏ qua bước thêm fields; không trả template ID nên không thể auto-redirect

**Phương án giữ nguyên validate CompanyId từ request body:**
- Lý do loại bỏ: Frontend không có CompanyId sẵn (phải gọi thêm API), và CompanyId đã có trong JWT claim — lấy từ JWT an toàn hơn và đơn giản hơn
