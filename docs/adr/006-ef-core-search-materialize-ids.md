---
date: 2026-05-31
status: accepted
---

# ADR-006: Fix EF Core search slowness — materialize case IDs trước thay vì IQueryable subquery

## Bối cảnh

`CaseKeywordRepository.GetAllAsync` dùng `.AsEnumerable()` trước khi filter → load toàn bộ bảng vào RAM. Fix trước dùng `IQueryable<Guid>.Contains()` để thêm SQL subquery vào base query phức tạp (Include + 4 joins + anonymous type projection). Tuy nhiên EF Core 6 không đảm bảo translate được pattern này trong mọi trường hợp → intermittent: đôi khi fast (SQL), đôi khi slow (in-memory fallback).

## Quyết định

Tách việc lấy matching case IDs thành các query riêng biệt chạy trước:

```csharp
IEnumerable<Guid>? intersected = null;
foreach (var kv in searchRequest.KeywordValues)
{
    var ids = await _context.CaseKeyword
        .Where(x => !x.Deleted && x.KeywordId == kv.KeywordId && x.Value.Contains(kv.Value))
        .Select(x => x.CaseId)
        .ToListAsync();
    intersected = intersected == null ? (IEnumerable<Guid>)ids : intersected.Intersect(ids);
}
filterCaseIds = intersected?.ToList() ?? new List<Guid>();
```

Sau đó dùng `List<Guid>.Contains()` trong main query — EF Core luôn translate thành SQL `IN (...)`.

## Hệ quả

- Search với conditions luôn chạy SQL-side (deterministic, không còn intermittent slowness)
- Mỗi keyword condition = 1 SQL query đơn giản (fast, indexed)
- Main query nhận `IN (id1, id2, ...)` filter trước khi `.AsEnumerable()` → load ít data hơn
- Trade-off: N+1 queries thay vì 1 query (nhưng N nhỏ = số keyword conditions, thường 1-3)

## Các phương án đã cân nhắc

| Phương án | Lý do không chọn |
|---|---|
| Giữ `IQueryable.Contains()` subquery | Intermittent — EF Core đôi khi fail translate complex query |
| Rewrite toàn bộ query sang raw SQL | Over-engineering, mất type safety |
| Thêm database index | Chưa đủ thông tin về schema để quyết định index phù hợp |
