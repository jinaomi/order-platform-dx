namespace CaseMngmt.Models.Dashboard
{
    public class DashboardAiCommentViewModel
    {
        public string Headline { get; set; } = string.Empty;
        public List<string> Highlights { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }
}
