using CaseMngmt.Models.Customers;
using CaseMngmt.Models.FileTypes;
using CaseMngmt.Models.Keywords;

namespace CaseMngmt.Models.CaseKeywords
{
    public class DocumentTemplateResponse
    {
        public FileTypeSearchModel FileType { get; set; }
        public List<KeywordSearchModel> Keywords { get; set; }
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
    }
}
