using CaseMngmt.Models;
using CaseMngmt.Models.Invoices;
using CaseMngmt.Repository.Companies;
using CaseMngmt.Repository.Customers;
using CaseMngmt.Repository.Invoices;
using CaseMngmt.Repository.Orders;

namespace CaseMngmt.Service.Invoices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoicePdfService _pdfService;

        private static readonly string[] InvoiceableStatuses = { "Confirmed" };

        public InvoiceService(
            IInvoiceRepository repository,
            IOrderRepository orderRepository,
            ICompanyRepository companyRepository,
            ICustomerRepository customerRepository,
            IInvoicePdfService pdfService)
        {
            _repository = repository;
            _orderRepository = orderRepository;
            _companyRepository = companyRepository;
            _customerRepository = customerRepository;
            _pdfService = pdfService;
        }

        public async Task<InvoiceCreateResult> CreateFromOrderAsync(Guid orderId, Guid companyId, Guid currentUserId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, companyId);
            if (order == null)
            {
                return new InvoiceCreateResult { StatusCode = 0, Message = "Order not found." };
            }

            var existingInvoice = await _repository.GetByOrderIdAsync(orderId, companyId);
            if (existingInvoice != null)
            {
                return new InvoiceCreateResult { StatusCode = -1, Message = "この受注にはすでに請求書が発行されています。" };
            }

            if (!InvoiceableStatuses.Contains(order.Status))
            {
                return new InvoiceCreateResult
                {
                    StatusCode = -1,
                    Message = order.Status == "RiskFlagged"
                        ? "在庫/生産能力の不足が確認された受注です。請求書を発行する前にリスクを解消するか、受注ステータスを確認してください。"
                        : $"ステータスが「{order.Status}」の受注は請求書を発行できません。"
                };
            }

            var invoiceCount = await _repository.GetInvoiceCountAsync(companyId, DateTime.UtcNow.Year);
            var invoiceNumber = $"INV-{DateTime.UtcNow.Year}-{(invoiceCount + 1):D5}";

            var invoice = new Invoice
            {
                Name = invoiceNumber,
                OrderId = order.Id,
                CompanyId = companyId,
                CustomerId = order.CustomerId,
                InvoiceNumber = invoiceNumber,
                IssueDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30),
                SubTotalAmount = order.SubTotalAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                Status = "Issued",
                CreatedBy = currentUserId,
                UpdatedBy = currentUserId
            };

            var result = await _repository.AddAsync(invoice);
            if (result <= 0)
            {
                return new InvoiceCreateResult { StatusCode = 0, Message = "請求書の作成に失敗しました。" };
            }

            await _orderRepository.UpdateStatusAsync(order.Id, companyId, "Invoiced", currentUserId);

            return new InvoiceCreateResult { StatusCode = result, InvoiceId = invoice.Id };
        }

        public async Task<PagedResult<InvoiceViewModel>?> GetAllInvoicesAsync(Guid companyId, int pageSize, int pageNumber)
        {
            var invoicesFromRepository = await _repository.GetAllAsync(companyId, pageSize, pageNumber);
            if (invoicesFromRepository == null)
            {
                return null;
            }

            return new PagedResult<InvoiceViewModel>(
                invoicesFromRepository.Items.Select(MapToViewModel),
                invoicesFromRepository.TotalCount,
                invoicesFromRepository.CurrentPage,
                invoicesFromRepository.PageSize);
        }

        public async Task<InvoiceViewModel?> GetByIdAsync(Guid id, Guid companyId)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);
            return entity == null ? null : MapToViewModel(entity);
        }

        public async Task<InvoiceViewModel?> GetByOrderIdAsync(Guid orderId, Guid companyId)
        {
            var entity = await _repository.GetByOrderIdAsync(orderId, companyId);
            return entity == null ? null : MapToViewModel(entity);
        }

        public async Task<int> UpdateStatusAsync(Guid id, Guid companyId, string status, Guid currentUserId)
        {
            return await _repository.UpdateStatusAsync(id, companyId, status, currentUserId);
        }

        public async Task<byte[]?> GeneratePdfAsync(Guid id, Guid companyId)
        {
            var invoice = await _repository.GetByIdAsync(id, companyId);
            if (invoice?.Order == null || invoice.Customer == null)
            {
                return null;
            }

            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return null;
            }

            return _pdfService.GeneratePdf(invoice, invoice.Order, company, invoice.Customer);
        }

        public async Task<string?> GetInvoiceFileNameAsync(Guid id, Guid companyId)
        {
            var invoice = await _repository.GetByIdAsync(id, companyId);
            return invoice == null ? null : $"{invoice.InvoiceNumber}.pdf";
        }

        private static InvoiceViewModel MapToViewModel(Invoice invoice)
        {
            return new InvoiceViewModel
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                OrderNumber = invoice.Order?.OrderNumber,
                CompanyId = invoice.CompanyId,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.Name,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                SubTotalAmount = invoice.SubTotalAmount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                PdfPath = invoice.PdfPath,
                CreatedDate = invoice.CreatedDate
            };
        }
    }
}
