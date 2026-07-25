# CaseMngmt — Architecture & Design Reference

Tài liệu này ghi lại kiến trúc, các quyết định thiết kế, convention và ràng buộc đã được thống nhất.
Claude phải đọc file này trước khi làm bất kỳ thay đổi nào vào codebase.

Tài liệu bổ sung:
- Kiến trúc chi tiết: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- Quyết định thiết kế (ADR): [docs/adr/](docs/adr/)
- Nhật ký phát triển: [docs/devlog/](docs/devlog/)
- Hướng dẫn sử dụng / demo cho khách hàng (tiếng Nhật): [docs/USER_GUIDE.md](docs/USER_GUIDE.md)

---

## Current State (2026-07-25)

Ngoài hệ thống Case/Template gốc mô tả bên dưới, project đã được mở rộng thành nền tảng **受注業務DX** (order-processing) cho SME sản xuất Nhật Bản, dùng để demo bán hàng. Toàn bộ 6 bước của flow đã triển khai và test qua API/UI thật:

`受注 → データ化 → AI照合 → 請求作成 → 売上分析 → 経営判断`

- **受注 (Order/OrderItem)**: module quan hệ riêng (`CaseMngmt.Model/Orders/`), KHÔNG dùng EAV Case/Keyword. FK thật tới Customer, `OrderNumber` tự sinh.
- **Product/tồn kho** (`CaseMngmt.Model/Products/`): `StockQuantity`, `ProductionCapacityPerDay`, CRUD admin.
- **データ化**: `IAiOrderExtractionService` — Claude vision (forced tool-call) đọc ảnh/PDF đơn hàng → trả draft chưa lưu DB → `OrderIntakeUpload.js` cho duyệt trước khi confirm lưu.
- **AI照合**: `IAiMatchingService` — risk level tính deterministic bằng C#, Claude chỉ enrich giải thích tiếng Nhật (forced tool-call). Entity `OrderRiskLineResult`. Tự chạy khi confirm order.
- **請求作成**: entity `Invoice`, PDF qua QuestPDF (font MS Gothic), chặn tạo invoice từ order `RiskFlagged` (409 theo convention `-1`).
- **売上分析**: `DashboardService.GetSummaryAsync` (LINQ thuần, không entity mới) + `SalesDashboard.js`.
- **経営判断**: `IDashboardCommentService` — dashboard AI comment (headline/highlights/recommendation tiếng Nhật, forced tool-call), không lưu DB, endpoint trả 204/ẩn lặng lẽ nếu lỗi.
- **Chat AI**: `IChatAssistantService` — trợ lý hỏi-đáp read-only qua **agentic tool-use loop thật** (`while stop_reason == "tool_use"`, khác các service khác chỉ gọi Claude 1 lần với forced tool-call), 4 tool (dashboard/orders/products/invoices). `companyId` luôn lấy server-side từ JWT, không bao giờ nhận từ input Claude. Lịch sử chat KHÔNG lưu DB (chỉ React state, mất khi refresh).

Toàn bộ AI feature dùng chung `AnthropicClient` (`CaseMngmt.Service/Ai/AnthropicClient.cs`), model `claude-opus-4-8`, API key qua `dotnet user-secrets` (KHÔNG trong appsettings.json).

**Build health (2026-07-25)**: `dotnet build` (backend) và `npm run build` (frontend) đều pass — chỉ còn warning cũ không liên quan (nullable/eslint exhaustive-deps), không có lỗi.

**Ghi chú môi trường quan trọng**: `git` KHÔNG khả dụng qua PowerShell tool trong môi trường Claude Code hiện tại (không tìm thấy `git.exe` trong PATH) — mọi thao tác git (status/diff/commit/push) phải do user tự làm qua terminal/VSCode khác. Custom command `/wrap-up` (`.claude/commands/wrap-up.md`) đã được chỉnh để bỏ qua hoàn toàn bước Git vì lý do này.

## Next Steps

1. **User tự commit + push** toàn bộ thay đổi (Order/Invoice/AI照合/データ化/Dashboard/Chat AI modules) — Claude không thể thao tác git qua shell trong môi trường hiện tại.
2. Test Chat AI qua giao diện trình duyệt thật (mới test qua API/PowerShell, chưa test UI thật).
3. Rotate AWS S3 access key/secret key đang hardcode plaintext trong `backend/CaseMngmt.Server/appsettings.json` — cần user xác nhận trước vì ảnh hưởng môi trường đang deploy.
4. Quyết định có triển khai RAG hay không (hướng mở rộng AI thứ 3 đã thảo luận, sau Dashboard comment + Chat AI), hoặc chuyển sang các việc treo khác.
5. Excel import cho Product (`ClosedXML`) — nguồn dữ liệu tồn kho thực tế của SME hiện quản lý bằng Excel.
6. Chart đẹp bằng `@mui/x-charts` thay stat tile/bảng thô trên `SalesDashboard.js`.
7. Nâng cấp đánh số `OrderNumber`/`InvoiceNumber` từ COUNT-based sang sequence table atomic trước khi chạy production thật (rủi ro concurrency hiện tại chấp nhận được cho demo, không cho production).

---

## Tổng quan hệ thống

CaseMngmt là hệ thống quản lý hồ sơ/case **đa tenant** (multi-tenant).
- **Backend**: ASP.NET Core 6.0 Web API + Entity Framework Core + SQL Server
- **Frontend**: React 17 (JavaScript/JSX) + Material UI v5 + Axios
- **Auth**: ASP.NET Identity + JWT Bearer token
- **Deployment**: Backend serve cả static frontend (SPA)

---

## Kiến trúc Backend

### Cấu trúc project (4 layer)

```
CaseMngmt.Model/          → Entities, ViewModels, DTOs, Migrations, AutoMapper, DbContext
CaseMngmt.Repository/     → Data access: IXxxRepository + XxxRepository (EF Core)
CaseMngmt.Service/        → Business logic: IXxxService + XxxService
CaseMngmt.Server/         → ASP.NET Core: Controllers, Program.cs, DbInitializerExtension
```

### Quy tắc layer (BẮT BUỘC)

- Controller chỉ gọi Service — không chứa business logic
- Service chỉ gọi Repository — không gọi DbContext trực tiếp
- Repository gọi DbContext — không có business logic
- API không trả Entity trực tiếp — luôn qua ViewModel/DTO + AutoMapper

### BaseModel

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

### Các module chính

| Module | Repository | Service | Controller | Ghi chú |
|---|---|---|---|---|
| Company | ICompanyRepository | ICompanyService | CompanyController | Tự động clone Standard Template khi tạo mới |
| Template | ITemplateRepository | ITemplateService | TemplateController | IsDefault=true là Standard Template |
| Keyword | IKeywordRepository | IKeywordService | KeywordController | Form Builder fields |
| Case | ICaseRepository | ICaseService | CaseController | Hồ sơ chính |
| CaseKeyword | ICaseKeywordRepository | ICaseKeywordService | — | Junction: Case ↔ Keyword |
| CompanyTemplate | ICompanyTemplateRepository | ICompanyTemplateService | — | Junction: Company ↔ Template |
| Type | ITypeRepository | ITypeService | TypeController | Kiểu dữ liệu cho Keyword |
| Customer | ICustomerRepository | ICustomerService | CustomerController | |
| KeywordRole | IKeywordRoleRepository | IKeywordRoleService | — | Phân quyền theo field |

### Authentication & Authorization

- JWT Bearer token — `[Authorize(AuthenticationSchemes = "Bearer")]` trên toàn controller
- Claims trong token: `ClaimTypes.NameIdentifier` (userId), `CompanyId` (custom claim), `ClaimTypes.Role`
- Role-based: `[ClaimRequirement(ClaimTypes.Role, "SuperAdmin")]` cho write operations
- Roles: `SuperAdmin`, `Admin`, `Editor`, `User`
- Lấy companyId trong controller: `User?.FindFirst("CompanyId")?.Value`

### Multi-tenant Design

- Mọi query phải scope theo `CompanyId` — không bao giờ trả dữ liệu của company khác
- Template và Company liên kết qua `CompanyTemplate` (junction table, composite key `CompanyId + TemplateId`)
- Keyword thuộc Template, không có CompanyId trực tiếp

### Các Entity quan trọng

**Template**
```csharp
IsDefault   bool    // true = Standard Template dùng để clone cho company mới
                    // Chỉ có đúng 1 record IsDefault=true trong toàn hệ thống
```

**Keyword**
```csharp
TypeId      Guid    // FK → Type.Id (Admin chọn từ Type có sẵn, KHÔNG dùng enum)
TemplateId  Guid
IsHidden    bool    // soft-delete cho Form Builder — ẩn khỏi UI nhưng giữ CaseKeyword data
Deleted     bool    // hard soft-delete — ẩn khỏi mọi nơi (từ BaseModel)
OptionsList string? // pipe-separated options, e.g. "選択肢A|選択肢B|選択肢C", max 2000 chars
Order       int
```

**Type**
```csharp
Value           string  // "alphanumeric", "date", "number", "list", v.v.
IsDefaultType   bool    // true = type hệ thống (seeded), false = custom list type
IsFileType      bool    // true = loại file (dùng riêng cho document)
Metadata        string  // comma-separated options (legacy, cho BOAT types)
```

**CompanyTemplate** (junction, không kế thừa BaseModel)
```csharp
CompanyId   Guid    // composite PK
TemplateId  Guid    // composite PK
```

### Return value convention (Repository → Service → Controller)

| Giá trị | Ý nghĩa |
|---|---|
| `> 0` | Thành công (số rows affected) |
| `0` | Thất bại / không tìm thấy |
| `-1` | Business rule violation (ví dụ: soft-delete keyword đang được dùng) |

Controller map:
- `-1` → `Conflict(409)`
- `0` → `BadRequest(400)`
- `null` → `NotFound(404)`

### AutoMapper (CustomProfile.cs)

AutoMapper convention tự map property cùng tên. Chỉ cần explicit mapping khi:
- Bỏ qua `Id` khi map Request → Entity: `.ForMember(x => x.Id, opt => opt.Ignore())`
- Property tên khác nhau giữa source và destination

Namespace: `CaseMngmt.Models.AutoMapper.CustomProfile`

### Migration

- **dotnet CLI không có trên máy** — tạo migration file thủ công theo pattern EF Core
- Namespace migration: `CaseMngmt.Models.Migrations`
- Tên class phải match tên file, kế thừa `Migration`, có `Up()` và `Down()`
- Cột NOT NULL mới: **bắt buộc có `defaultValue`** để an toàn với data hiện có
- Sau khi tạo migration: cập nhật `ApplicationDbContextModelSnapshot.cs` thủ công
- Auto-apply khi startup: `db.Database.Migrate()` trong `Program.cs` (đã có sẵn)

### Seed data (DbInitializerExtension)

- Guard pattern: kiểm tra tồn tại trước khi insert (idempotent)
- Standard Template seed: `context.Template.Any(t => t.IsDefault)` làm guard
- Seed chạy cho cả install mới và install cũ (existing data)
- Types được seed với tên tiếng Anh + IsDefaultType=true

---

## Kiến trúc Frontend

### Cấu trúc thư mục

```
frontend/src/
  api/axios.js              → axios instances (default + axiosPrivate)
  hooks/
    useAxiosPrivate.js      → JWT interceptor — luôn dùng hook này cho authenticated calls
    useAuth.js              → auth context
    useRefreshToken.js
  context/AuthProvider.js   → JWT context provider
  services/                 → API wrappers (nhận axiosPrivate làm param đầu)
    templateService.js
    keywordService.js
    typeService.js
  pages/
    admin/
      TemplateList.jsx      → /admin/templates
      KeywordBuilder.jsx    → /admin/templates/:templateId/keywords
  components/
    Admin.js                → Admin landing page
    CaseDetail.js           → Tạo/xem hồ sơ
    CaseSearch.js           → Tìm kiếm hồ sơ
    until/                  → Reusable UI components
      FormSnackbar.js       → Alert/notification (prop: item, setItem)
      LoadingSpinner.js     → Backdrop spinner (prop: loading)
      ConfirmBox.js         → Confirm dialog
      ContentDialog.js, DialogHandle.js, FormButton.js, ...
  App.js                    → Routes
  index.js
```

### Routing (App.js)

Tất cả route cần auth nằm trong `RequireAuth` block với `allowedRoles`:
```jsx
<Route element={<RequireAuth allowedRoles={[ROLES.Admin, ROLES.User, ROLES.SuperAdmin]} />}>
  <Route path="admin" element={<Admin />} />
  <Route path="/admin/templates" element={<TemplateList />} />
  <Route path="/admin/templates/:templateId/keywords" element={<KeywordBuilder />} />
</Route>
```

### useAxiosPrivate — Quy tắc BẮT BUỘC

```js
// ✅ ĐÚNG — dùng hook để có interceptor
const axiosPrivate = useAxiosPrivate();
const data = await axiosPrivate.get('/api/...');

// ❌ SAI — import trực tiếp, mất JWT interceptor
import { axiosPrivate } from '../api/axios';
```

### Service pattern

Service file export default object, nhận `axiosPrivate` làm tham số đầu:
```js
const templateService = {
  getAll: (axiosPrivate, pageSize = 25, pageNumber = 1) =>
    axiosPrivate.get(`/api/template/getAll?pageSize=${pageSize}&pageNumber=${pageNumber}`),
};
export default templateService;
```

### Component state pattern

```js
const [snackbar, setSnackbar] = useState({ isOpen: false, status: "success", message: "" });
const [loading, setLoading] = useState(false);

// Usage:
<LoadingSpinner loading={loading} />
<FormSnackbar item={snackbar} setItem={setSnackbar} />
```

### Drag-and-drop (KeywordBuilder)

Dùng `@dnd-kit/core` + `@dnd-kit/sortable` + `@dnd-kit/utilities`.
Pattern: optimistic update local state → gọi API → revert nếu lỗi.

---

## API Endpoints

### Keyword API (Form Builder — mới)

```
GET    /api/keywords?templateId={id}    Lấy ALL keywords (kể cả IsHidden) cho admin
POST   /api/keywords                    Tạo keyword mới [SuperAdmin]
PUT    /api/keywords/{id}               Cập nhật keyword [SuperAdmin]
DELETE /api/keywords/{id}               Soft-hide (IsHidden=true) [SuperAdmin]
                                        → 409 nếu keyword đang được dùng trong CaseKeyword
PATCH  /api/keywords/reorder            Bulk reorder [{id, order}] [SuperAdmin]
```

### Template API (existing + mới)

```
GET    /api/template/getAll             Templates của company hiện tại (paged) [SuperAdmin]
GET    /api/template?templateId={id}    Chi tiết template
GET    /api/template/template           Template của company từ JWT (dùng cho case form)
POST   /api/template                    Tạo template + keywords [SuperAdmin]
PUT    /api/template/{id}               Cập nhật template [SuperAdmin]
DELETE /api/template/{id}               Xóa template [SuperAdmin]
POST   /api/template/{id}/clone         Clone template sang company [SuperAdmin]
```

### Type API

```
GET    /api/type/type       Lấy tất cả kiểu dữ liệu (không phải file type)
GET    /api/type/file-type  Lấy kiểu file
```

---

## Các quyết định thiết kế đã chốt

### 1. TypeId FK (không dùng enum)

Keyword.TypeId là FK trỏ đến Type table. Admin chọn từ danh sách Type có sẵn qua dropdown.
**Không** refactor sang DataType enum. Lý do: Type table có metadata linh hoạt và đã có data.

### 2. OptionsList là chuỗi pipe-separated

`Keyword.OptionsList` lưu options dưới dạng `"選択肢A|選択肢B|選択肢C"`, max 2000 ký tự.
**Không** tạo bảng riêng. Lý do: đơn giản, đủ cho use case hiện tại.

### 3. IsHidden vs Deleted (Keyword)

| Flag | Ý nghĩa | Ảnh hưởng |
|---|---|---|
| `Deleted=true` | Hard soft-delete | Ẩn khỏi mọi nơi, kể cả case form |
| `IsHidden=true` | Form Builder hide | Ẩn khỏi admin UI, vẫn giữ trong CaseKeyword history |

Khi admin muốn ẩn field: set `IsHidden=true` (KHÔNG set `Deleted=true`).
Lý do: CaseKeyword data phải được bảo toàn dù field bị ẩn.

### 4. Standard Template + Auto-clone

- Có đúng **1** Template với `IsDefault=true` trong toàn hệ thống (Standard Template)
- Khi tạo Company mới: `CompanyController.Create` tự động clone Standard Template và link với company
- Clone: tạo Template mới + copy Keywords (chỉ non-hidden) + tạo CompanyTemplate record
- Clone KHÔNG copy `IsDefault=true` — bản clone luôn có `IsDefault=false`

### 5. Frontend language: JavaScript (không TypeScript)

Tất cả file frontend là `.js` hoặc `.jsx`. **Không tạo `.ts` hay `.tsx`**.
Lý do: project đã bắt đầu với JS, không muốn migration cost.

---

## Ràng buộc và quy tắc bắt buộc

### Backend

1. **Không hard-delete Keyword** khi có CaseKeyword references → trả 409 Conflict
2. **Không trả Entity trực tiếp** từ API — luôn dùng ViewModel/DTO
3. **Migration mới**: luôn có `defaultValue` cho cột NOT NULL; luôn update snapshot
4. **Seed idempotent**: luôn có guard check trước khi insert
5. **Async/await**: tất cả DB call phải async — **không dùng `.Result` hay `.Wait()`**
6. **Layer không được xuyên**: Service không gọi DbContext, Controller không gọi Repository
7. **Multi-tenant**: mọi query filter theo CompanyId — không bao giờ leak data cross-company

### Frontend

1. **Luôn dùng `useAxiosPrivate` hook** — không import `axiosPrivate` trực tiếp
2. **Service nhận `axiosPrivate` làm param đầu** — không bind trong service file
3. **Không thêm comment** trừ khi lý do kỹ thuật không rõ ràng
4. **Không dùng TypeScript** — chỉ JS/JSX
5. **Snackbar shape**: `{ isOpen: bool, status: "success"|"error", message: string }`
6. **Xử lý 409**: khi DELETE keyword, bắt `error.response?.status === 409` riêng

### Chung

1. **Không thêm feature**, refactor, hay abstraction ngoài scope của task
2. **Đọc file trước khi Edit** — không overwrite mà không đọc
3. **Giữ nguyên code hiện có** khi thêm tính năng mới

---

## Môi trường phát triển

- **OS**: Windows 11 Pro
- **Shell**: PowerShell (dùng PowerShell syntax, KHÔNG bash syntax cho file ops)
- **dotnet CLI**: **KHÔNG có trên máy** — không chạy được `dotnet ef migrations add`
- **npm**: có trong frontend/
- **Database**: SQL Server (connection string trong appsettings)
- **Auto-migration**: `db.Database.Migrate()` trong `Program.cs` — apply khi app start

---

## Subagents

Project có 2 custom subagent định nghĩa trong `.claude/agents/`:

| Agent | File | Khi nào dùng |
|---|---|---|
| code-reviewer | `.claude/agents/code-reviewer.md` | Sau khi viết/sửa code đáng kể |
| code-tester | `.claude/agents/code-tester.md` | Khi cần viết test cho tính năng |

Gọi bằng: `@code-reviewer` hoặc `@code-tester` trong chat.
