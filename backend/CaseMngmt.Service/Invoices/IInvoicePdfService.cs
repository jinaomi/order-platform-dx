using CaseMngmt.Models.Companies;
using CaseMngmt.Models.Customers;
using CaseMngmt.Models.Invoices;
using CaseMngmt.Models.Orders;

namespace CaseMngmt.Service.Invoices
{
    public interface IInvoicePdfService
    {
        byte[] GeneratePdf(Invoice invoice, Order order, Company company, Customer customer);
    }
}
