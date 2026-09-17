using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.ErpWrite;

/// <summary>
/// The <c>erpContext</c> sent with each leased job (goal GOAL_ERP_YAZIM Y1d): the company's
/// <see cref="ErpWriteSettings"/> merged with the document creator's <see cref="MobileUserErpMapping"/>.
/// A value the user has wins; a missing one (null) falls back to the company. An empty series on
/// the user is kept — it deliberately means Mikro's series-less numbering for that user.
/// Built at lease time, so fixing a mapping and retrying the job uses the fixed value.
/// </summary>
public static class ErpWriteContextBuilder
{
    public static JobErpContextResponse Build(ErpWriteSettings? settings, MobileUserErpMapping? user, string? createdByUsername)
    {
        settings ??= new ErpWriteSettings();
        return new JobErpContextResponse
        {
            SalesDocumentKind = settings.SalesDocumentKind,
            OrderApprovalMode = settings.OrderApprovalMode,
            Series = new JobErpSeriesResponse
            {
                Order = user?.OrderSeries ?? settings.OrderSeries,
                Dispatch = user?.DispatchSeries ?? settings.DispatchSeries,
                Invoice = user?.InvoiceSeries ?? settings.InvoiceSeries,
                Return = user?.ReturnSeries ?? settings.ReturnSeries,
                Collection = user?.CollectionSeries ?? settings.CollectionSeries,
            },
            WarehouseNo = user?.WarehouseNo ?? settings.DefaultWarehouseNo,
            CashCode = Code(user?.CashCode) ?? Code(settings.DefaultCashCode),
            CardBankCode = Code(user?.CardBankCode) ?? Code(settings.DefaultCardBankCode),
            TransferBankCode = Code(user?.TransferBankCode) ?? Code(settings.DefaultTransferBankCode),
            ErpUserNo = user?.ErpUserNo ?? settings.DefaultErpUserNo,
            SalespersonCode = Code(user?.SalespersonCode) ?? Code(settings.DefaultSalespersonCode),
            PriceListNo = settings.DefaultPriceListNo,
            ChequePortfolioCode = settings.ChequePortfolioCode,
            NotePortfolioCode = settings.NotePortfolioCode,
            CreatedByUsername = createdByUsername,
        };
    }

    /// <summary>A blank code is no code: it must not hide the company default.</summary>
    private static string? Code(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
