# ADR 001: TypeId là FK trỏ đến Type table, không dùng enum

**Ngày:** 2026-05-28  
**Trạng thái:** Chấp thuận

---

## Bối cảnh

Keyword cần biết loại dữ liệu của nó (text, number, date, list, file, v.v.).
Có hai cách tiếp cận phổ biến: dùng `enum` trong code, hoặc dùng bảng `Type` trong database.

## Quyết định

Dùng `Keyword.TypeId` là Foreign Key trỏ đến bảng `Type`.
Admin chọn TypeId từ dropdown khi tạo/sửa keyword.

## Hệ quả

- Admin có thể thêm Type mới mà không cần deploy code
- `Type.Value` (string) được dùng để identify loại trong business logic (e.g. `"list"`, `"date"`)
- `Type.IsFileType = true` dùng riêng cho document upload
- `Type.IsDefaultType = true` = seeded types (system), `false` = custom list types
- Frontend kiểm tra `type.value.includes('list')` để show OptionsList field
- Không bao giờ hardcode TypeId — luôn lấy từ API

## Các phương án đã cân nhắc

**Phương án loại bỏ — DataType enum:**
```csharp
public enum DataType { Text, Number, Date, List, File }
public DataType Type { get; set; }
```
Lý do loại bỏ: không linh hoạt khi thêm type mới, mất metadata (Metadata field, IsFileType), và đã có data trong Type table.
