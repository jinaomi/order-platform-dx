# CaseMngmt — Architecture

## Stack

| Tầng | Công nghệ |
|---|---|
| Backend API | ASP.NET Core 6.0 Web API |
| ORM | Entity Framework Core + SQL Server |
| Auth | ASP.NET Identity + JWT Bearer |
| Frontend | React 17 (JSX only) + Material UI v5 + Axios |
| Deployment | Backend serve static frontend (SPA) |

---

## 4-Layer Backend

```
┌─────────────────────────────────────┐
│  CaseMngmt.Server  (Controllers)    │  ← HTTP in/out, auth, model validation
├─────────────────────────────────────┤
│  CaseMngmt.Service  (Services)      │  ← Business logic, orchestration
├─────────────────────────────────────┤
│  CaseMngmt.Repository  (Repos)      │  ← Data access via EF Core
├─────────────────────────────────────┤
│  CaseMngmt.Model  (Entities/DTOs)   │  ← Entities, ViewModels, Migrations, AutoMapper
└─────────────────────────────────────┘
```

**Luồng request:**
```
HTTP Request
  → Controller (validate, extract claims)
    → Service (business rules)
      → Repository (EF Core query)
        → SQL Server
      ← Entity
    ← ViewModel (AutoMapper)
  ← HTTP Response (ViewModel/DTO — không bao giờ trả Entity trực tiếp)
```

**Quy tắc cứng:**
- Controller → Service only (không gọi Repository trực tiếp)
- Service → Repository only (không gọi DbContext trực tiếp)
- Repository → DbContext only (không có business logic)

---

## Multi-Tenant Isolation

Mọi request được scope theo `CompanyId` lấy từ JWT claim:

```csharp
var companyId = User?.FindFirst("CompanyId")?.Value;
// Truyền xuống Service → Repository
// Repository luôn filter: .Where(x => x.CompanyId == companyId)
```

**Không bao giờ** trả dữ liệu cross-company. Đây là invariant quan trọng nhất của hệ thống.

---

## BaseModel

Tất cả entity kế thừa `BaseModel`:

```csharp
Id          Guid        // auto = Guid.NewGuid()
Name        string
CreatedDate DateTime    // auto = DateTime.UtcNow
UpdatedDate DateTime    // auto = DateTime.UtcNow
CreatedBy   Guid
UpdatedBy   Guid
Deleted     bool        // soft-delete toàn hệ thống
```

---

## Keyword Dual Soft-Delete

Keyword có 2 flag riêng biệt với ngữ nghĩa khác nhau:

```
Deleted = true   →  Hard soft-delete: ẩn khỏi MỌI nơi (kể cả case form)
IsHidden = true  →  Form Builder hide: ẩn khỏi admin UI, GIỮ CaseKeyword data
```

Khi admin ẩn field khỏi form builder: set `IsHidden = true` (KHÔNG phải `Deleted = true`).
Lý do: CaseKeyword records chứa dữ liệu lịch sử phải được bảo toàn.

Nếu keyword đang có CaseKeyword references → `SoftDeleteAsync` trả `-1` → Controller trả `409 Conflict`.

Xem ADR: [docs/adr/003-keyword-dual-soft-delete.md](adr/003-keyword-dual-soft-delete.md)

---

## Standard Template Auto-Clone

```
SuperAdmin tạo Company mới
  → CompanyController.Create()
    → CompanyService.AddAsync() → trả companyId mới
    → TemplateService.GetDefaultTemplateAsync() → lấy Template có IsDefault=true
    → TemplateService.CloneToCompanyAsync(defaultTemplateId, newCompanyId)
      → Tạo Template mới (IsDefault=false)
      → Copy tất cả Keyword không IsHidden
      → Tạo CompanyTemplate record (junction)
```

**Invariant:** Luôn có đúng 1 Template với `IsDefault=true` trong toàn hệ thống.
Clone KHÔNG copy `IsDefault=true` — bản clone luôn là `false`.

Xem ADR: [docs/adr/004-standard-template-autoclone.md](adr/004-standard-template-autoclone.md)

---

## Template Create Pattern

`POST /api/template` trả `Guid` (template ID mới), không phải `int`.

```
Frontend: POST /api/template { Name, KeywordRequests: [] }
  ↓
Controller: CompanyId lấy từ JWT claim, gán vào request
  ↓
TemplateService.AddAsync:
  1. Tạo Template với request.Name
  2. Nếu KeywordRequests không rỗng → tạo keywords
  3. Tạo CompanyTemplate { CompanyId, TemplateId } → template luôn linked với company
  4. Trả template.Id (Guid?)
  ↓
Controller: Ok(templateId)
  ↓
Frontend: navigate(/admin/templates/${templateId}/keywords)
```

**Invariant:** Template không bao giờ là "orphan" — luôn có CompanyTemplate record sau khi `AddAsync`.

Xem ADR: [docs/adr/005-template-create-returns-id.md](adr/005-template-create-returns-id.md)

---

## Return Value Convention

Repository → Service → Controller tuân theo (ngoại trừ `TemplateService.AddAsync` trả `Guid?`):

| Giá trị | Ý nghĩa |
|---|---|
| `> 0` | Thành công |
| `0` | Thất bại / không tìm thấy |
| `-1` | Business rule violation |

Controller map:
```
-1   → 409 Conflict
 0   → 400 BadRequest
null → 404 NotFound
```

---

## Frontend Architecture

```
src/
  api/axios.js              → axios instances
  hooks/useAxiosPrivate.js  → JWT interceptor (luôn dùng hook này)
  services/                 → API wrappers (nhận axiosPrivate làm param đầu)
  pages/admin/              → Admin pages (TemplateList, KeywordBuilder)
  components/               → Shared UI + reusable components
  context/AuthProvider.js   → JWT context
```

**Quy tắc useAxiosPrivate:**
```js
// ✅ Đúng
const axiosPrivate = useAxiosPrivate();

// ❌ Sai — mất JWT interceptor
import { axiosPrivate } from '../api/axios';
```

**Service pattern:**
```js
const templateService = {
  getAll: (axiosPrivate, pageSize, pageNumber) =>
    axiosPrivate.get(`/api/template/getAll?...`),
};
```

---

## Drag-and-Drop (KeywordBuilder)

Dùng `@dnd-kit/core` + `@dnd-kit/sortable` + `@dnd-kit/utilities`.

Pattern: optimistic update → gọi API → revert nếu lỗi:
```js
setKeywords(reordered);           // cập nhật UI ngay
try {
  await keywordService.reorder(…);
} catch {
  await fetchKeywords();          // revert về server state
}
```

---

## Migration Convention

- `dotnet CLI` không có trên máy → tạo migration file thủ công
- Namespace: `CaseMngmt.Models.Migrations`
- Cột NOT NULL mới: bắt buộc có `defaultValue`
- Sau khi tạo: update `ApplicationDbContextModelSnapshot.cs`
- Auto-apply: `db.Database.Migrate()` trong `Program.cs`
