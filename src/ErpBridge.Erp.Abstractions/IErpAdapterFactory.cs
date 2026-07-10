namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// Produces <see cref="IErpAdapter"/> instances for the requested <see cref="ErpType"/>.
/// The factory MUST throw <see cref="NotSupportedException"/> for ERP types that are
/// not yet implemented (Logo, Paraşüt, Netsis — reserved for later phases).
/// </summary>
public interface IErpAdapterFactory
{
    /// <summary>Create a fresh adapter instance — concrete adapters may be expensive to construct.</summary>
    IErpAdapter Create(ErpType erpType);
}
