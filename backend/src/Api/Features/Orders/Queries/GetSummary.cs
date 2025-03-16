using System.Security.Claims;
using Carter;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Orders.Queries;

public static class GetSummary
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "orders/{orderId}/summary",
                async Task<IResult> (IMediator mediator, ClaimsPrincipal user, Guid orderId) =>
                {
                    var query = new Query(user, orderId);
                    var result = await mediator.Send(query);
                    if (result == null)
                    {
                        return TypedResults.NotFound();
                    }
                    return TypedResults.Ok(result);
                })
                .WithName(nameof(GetSummary))
                .WithTags(nameof(Order))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get summary of an order by order ID";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid OrderId) : IRequest<SummaryResponse?>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, SummaryResponse?>
    {
        public async Task<SummaryResponse?> Handle(Query message, CancellationToken cancellationToken)
        {
            // Načítame objednávku spolu s kontajnermi, položkami a produktmi (vrátane VAT kategórie)
            var order = await context.Orders
                .Include(o => o.ItemContainers)
                    .ThenInclude(ic => ic.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.VATCategory)
                .FirstOrDefaultAsync(o => o.Id == message.OrderId, cancellationToken);

            if (order == null)
            {
                return null;
            }

            // Zjednotíme všetky položky zo všetkých kontajnerov
            var allItems = order.ItemContainers.SelectMany(ic => ic.Items);

            // Vypočítame cenu bez DPH pre položky s VAT kategóriou "Reduced" (bez ohľadu na kapitalizáciu)
            var priceExcVatReduced = allItems
                .Where(i => i.Product?.VATCategory?.Name != null &&
                            string.Equals(i.Product.VATCategory.Name, "Reduced", StringComparison.OrdinalIgnoreCase))
                .Sum(i => i.Quantity * (i.Product.PricePerPiecePack ?? 0));

            // Vypočítame cenu bez DPH pre položky s VAT kategóriou "Normal"
            var priceExcVatNormal = allItems
                .Where(i => i.Product?.VATCategory?.Name != null &&
                            string.Equals(i.Product.VATCategory.Name, "Normal", StringComparison.OrdinalIgnoreCase))
                .Sum(i => i.Quantity * (i.Product.PricePerPiecePack ?? 0));

            // Sadzby pre daň
            decimal vatReducedRate = 12;
            decimal vatNormalRate = 21;

            // Výpočet dane pre každú kategóriu
            var vatReducedSum = priceExcVatReduced * vatReducedRate / 100;
            var vatNormalSum = priceExcVatNormal * vatNormalRate / 100;

            // Cena s DPH pre každú kategóriu
            var priceInclVatReduced = priceExcVatReduced + vatReducedSum;
            var priceInclVatNormal = priceExcVatNormal + vatNormalSum;

            // Celková suma s DPH
            var total = priceInclVatReduced + priceInclVatNormal;

            return new SummaryResponse(
                order.Id,
                new VatSummary(priceExcVatReduced, vatReducedSum, priceInclVatReduced),
                new VatSummary(priceExcVatNormal, vatNormalSum, priceInclVatNormal),
                total
            );
        }
    }

    public record SummaryResponse(
        Guid OrderId,
        VatSummary VatReduced,
        VatSummary VatNormal,
        decimal Total
    );

    public record VatSummary(
        decimal PriceExcVat,
        decimal Vat,
        decimal PriceInclVat
    );
}
