# ADR 002: OptionsList lưu dạng chuỗi pipe-separated, không tạo bảng riêng

**Ngày:** 2026-05-28  
**Trạng thái:** Chấp thuận

---

## Bối cảnh

Keyword loại `list` cần lưu danh sách các lựa chọn (options).
Cần quyết định cách lưu: bảng riêng hay lưu inline trong Keyword.

## Quyết định

Lưu trong `Keyword.OptionsList` dạng chuỗi pipe-separated:
```
"選択肢A|選択肢B|選択肢C"
```

- Kiểu: `string?` (nullable — null nếu không phải list type)
- Giới hạn: max 2000 ký tự (đủ cho ~20-30 options thực tế)
- Dấu phân cách: `|` (pipe)

## Hệ quả

- Schema đơn giản, không cần join thêm bảng
- Migration dễ: chỉ thêm 1 cột varchar(2000)
- Frontend: split bằng `|` để render dropdown
- Backend: truyền thẳng string, không parse phía server
- Giới hạn: không phù hợp nếu options cần metadata riêng (label, value tách biệt)

## Các phương án đã cân nhắc

**Phương án loại bỏ — bảng `KeywordOption`:**
```
KeywordOption { Id, KeywordId, Value, Order }
```
Lý do loại bỏ: over-engineering cho use case hiện tại, thêm complexity cho API và migration, options hiếm khi > 20 items.

**Phương án loại bỏ — JSON array trong cột:**
```
"[\"選択肢A\",\"選択肢B\"]"
```
Lý do loại bỏ: pipe-separated đơn giản hơn để parse, không cần JSON library phía frontend.
