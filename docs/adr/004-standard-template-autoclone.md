# ADR 004: Auto-clone Standard Template khi tạo Company mới

**Ngày:** 2026-05-28  
**Trạng thái:** Chấp thuận

---

## Bối cảnh

Khi SuperAdmin tạo Company mới, company đó cần có sẵn một template với các fields mặc định
để có thể bắt đầu tạo hồ sơ (case) ngay. Nếu không, admin phải thủ công tạo template và link
với company — friction cao, dễ bỏ sót.

## Quyết định

Tự động clone Standard Template khi tạo Company mới:

1. Có đúng **1** Template với `IsDefault = true` trong toàn hệ thống (Standard Template)
2. `CompanyController.Create` inject `ITemplateService`
3. Sau khi `AddAsync(company)` thành công:
   ```csharp
   var defaultTemplate = await _templateService.GetDefaultTemplateAsync();
   if (defaultTemplate != null)
       await _templateService.CloneToCompanyAsync(defaultTemplate.Id, result.Value);
   ```
4. Clone tạo Template mới (`IsDefault = false`) + copy Keywords (non-hidden) + tạo CompanyTemplate record

**Clone KHÔNG copy `IsDefault = true`** — chỉ có 1 Standard Template duy nhất.

**Seed guard:** `context.Template.Any(t => t.IsDefault)` — idempotent, không seed nếu đã có.

## Hệ quả

- Company mới luôn có template sẵn sàng
- Standard Template là nguồn chuẩn — SuperAdmin sửa Standard Template để thay đổi default fields
- Mỗi company có bản clone riêng → có thể customize độc lập
- Clone chỉ copy keywords `!IsHidden` (keywords đã ẩn trong Standard Template không được clone)
- Nếu clone thất bại (Standard Template chưa tồn tại): Company vẫn được tạo, chỉ không có template

## Các phương án đã cân nhắc

**Phương án loại bỏ — share 1 template cho nhiều company:**
Thực hiện qua CompanyTemplate junction (đã có) nhưng mọi company sẽ dùng chung fields.
Lý do loại bỏ: company cần khả năng customize template riêng.

**Phương án loại bỏ — admin tự tạo template thủ công:**
Lý do loại bỏ: UX kém, dễ bỏ sót, không nhất quán giữa các company.
