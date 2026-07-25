namespace CaseMngmt.Models.CaseKeywords
{
    public class CaseKeywordSearch
    {
        public List<KeywordValue> KeywordValues { get; set; }
        public List<KeywordSearchRangeValue> KeywordDateValues { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }

        public bool IsValidDatetime()
        {
            try
            {
                if (KeywordDateValues != null && KeywordDateValues.Any())
                {
                    foreach (var item in KeywordDateValues)
                    {
                        if (!string.IsNullOrEmpty(item.FromValue) && string.IsNullOrEmpty(item.ToValue))
                        {
                            DateTime.TryParse(item.FromValue, out _);
                        }
                        else if (string.IsNullOrEmpty(item.FromValue) && !string.IsNullOrEmpty(item.ToValue))
                        {
                            DateTime.TryParse(item.ToValue, out _);
                        }
                        else
                        {
                            DateTime.TryParse(item.FromValue, out _);
                            DateTime.TryParse(item.ToValue, out _);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class CaseKeywordSearchRequest : CaseKeywordSearch
    {
        public List<Guid> RoleIds { get; set; }
        public Guid TemplateId { get; set; }
        public Guid CompanyId { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}
