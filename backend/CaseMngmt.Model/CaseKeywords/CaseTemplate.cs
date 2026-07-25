using CaseMngmt.Models.Customers;
using CaseMngmt.Models.Keywords;

namespace CaseMngmt.Models.CaseKeywords
{
    public class CaseTemplate
    {
        public List<KeywordSearchModel> CaseKeywordValues { get; set; }
        public List<Customer> Customers { get; set; }
    }
}
