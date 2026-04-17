using System.ComponentModel;
using System.Text;
using MediatR;
using ModelContextProtocol.Server;
using ChinookApi.Features.Customers;
using ChinookApi.Features.Invoices;

namespace ChinookApi.Mcp;

[McpServerToolType]
public sealed class CustomerHistoryTool(IMediator mediator)
{
    [Description("Look up a customer by name or email address and retrieve their complete purchase history — every invoice with its date, total amount, and the individual tracks purchased. Useful for reviewing what a customer has bought.")]
    [McpServerTool(Name = "get_customer_purchase_history")]
    public async Task<string> GetCustomerPurchaseHistoryAsync(
        [Description("Customer name (first, last, or full) or email address to search for")] string customerQuery,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerQuery))
            return "Please provide a customer name or email.";

        var customers = await mediator.Send(new GetAllCustomersQuery(customerQuery, 1, 5), cancellationToken);

        if (customers.Items.Count == 0)
            return $"No customer found matching \"{customerQuery}\".";

        var sb = new StringBuilder();

        if (customers.Items.Count > 1)
        {
            sb.AppendLine($"Multiple customers matched \"{customerQuery}\". Showing all:");
            foreach (var match in customers.Items)
                sb.AppendLine($"  • [{match.CustomerId}] {match.FirstName} {match.LastName}  <{match.Email}>");
            sb.AppendLine();
        }

        foreach (var customer in customers.Items)
        {
            var invoices = await mediator.Send(new GetCustomerInvoicesWithLinesQuery(customer.CustomerId), cancellationToken);

            var totalSpent = invoices.Sum(i => i.Total);
            sb.AppendLine($"Customer: {customer.FirstName} {customer.LastName} (ID: {customer.CustomerId})");
            sb.AppendLine($"Email:    {customer.Email}");
            if (!string.IsNullOrWhiteSpace(customer.Company))
                sb.AppendLine($"Company:  {customer.Company}");
            sb.AppendLine($"Invoices: {invoices.Count}  |  Total spent: ${totalSpent:F2}");

            if (invoices.Count == 0)
            {
                sb.AppendLine("  (no purchases yet)");
            }
            else
            {
                foreach (var invoice in invoices)
                {
                    sb.AppendLine();
                    sb.AppendLine($"  Invoice #{invoice.InvoiceId}  |  {invoice.InvoiceDate:yyyy-MM-dd}  |  ${invoice.Total:F2}");
                    if (!string.IsNullOrWhiteSpace(invoice.BillingCity))
                        sb.AppendLine($"  Billed to: {invoice.BillingCity}, {invoice.BillingCountry}");

                    if (invoice.Lines is { Count: > 0 } lines)
                    {
                        sb.AppendLine($"  Items ({lines.Count}):");
                        foreach (var line in lines)
                            sb.AppendLine($"    • {line.Track?.Name ?? $"Track #{line.TrackId}"}  x{line.Quantity}  @${line.UnitPrice:F2}");
                    }
                }
            }

            if (customers.Items.Count > 1)
                sb.AppendLine(new string('-', 40));
        }

        return sb.ToString();
    }
}
