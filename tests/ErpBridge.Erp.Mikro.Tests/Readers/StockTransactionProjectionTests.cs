using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Mikro.Readers;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Readers;

/// <summary>
/// Dapper binds a positional record's constructor by column order and exact type. A SELECT whose columns drift
/// from <see cref="StockTransactionPayload"/>'s parameters fails every read with "a parameterless default
/// constructor or one matching signature is required", and with it the agent's whole synchronisation (#178 put
/// the discount/VAT columns mid-list; #185 moved them back). The live reader tests do not run in CI, so these
/// rebuild the columns SQL Server returns for the projection — name, order and CAST type — and materialise them
/// through Dapper without a database.
/// </summary>
public class StockTransactionProjectionTests
{
    private static readonly StockTransactionPayload Expected = new(
        Id: "85054",
        ErpRef: "85054",
        Erp: "MIKRO",
        StockCode: "KLİPS",
        ProductCode: "KLİPS",
        Date: new DateTime(2026, 9, 28),
        Type: 0,
        Kind: 0,
        DocumentType: 3,
        DocumentNo: "-409",
        InQuantity: 10m,
        OutQuantity: 0m,
        SignedQuantity: 10m,
        UnitPrice: 4m,
        Amount: 40m,
        CustomerCode: "07RÜZGAR",
        InWarehouseNo: 1,
        OutWarehouseNo: null,
        Description: "Alış faturası",
        UpdatedAt: new DateTime(2026, 9, 28, 14, 5, 15),
        InvoiceRecNo: 85054,
        DiscountAmount: 15.01m,
        VatAmount: 3.20m);

    [Fact]
    public void Projection_columns_follow_the_constructor_order()
    {
        var parameters = typeof(StockTransactionPayload).GetConstructors().Single().GetParameters()
            .Select(p => p.Name!);

        ProjectedColumns(MikroDbReader.StockTransactionsSql).Select(c => c.Name)
            .Should().Equal(parameters, (column, parameter) => string.Equals(column, parameter, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Projection_materialises_into_the_payload_with_discount_and_vat()
    {
        var columns = ProjectedColumns(MikroDbReader.StockTransactionsSql);
        using var table = new DataTable();
        foreach (var (name, type) in columns)
        {
            table.Columns.Add(name, type);
        }
        table.Rows.Add(columns.Select(c => ExpectedValue(c.Name)).ToArray());

        using var reader = table.CreateDataReader();
        var row = reader.Parse<StockTransactionPayload>().Single();

        row.Should().Be(Expected);
    }

    private static object ExpectedValue(string column)
    {
        var property = typeof(StockTransactionPayload).GetProperty(column)
            ?? throw new InvalidOperationException($"Column {column} has no StockTransactionPayload property.");
        return property.GetValue(Expected) ?? DBNull.Value;
    }

    /// <summary>
    /// The SELECT list's columns with the .NET type SqlClient returns for their CAST. Every top-level item has to
    /// end in <c>AS &lt;type&gt;) AS &lt;name&gt;</c>: an uncast column's type depends on the Mikro schema, which
    /// the constructor match cannot tolerate.
    /// </summary>
    private static IReadOnlyList<(string Name, Type Type)> ProjectedColumns(string sql)
    {
        var withoutComments = Regex.Replace(sql, "--[^\r\n]*", string.Empty);
        var select = withoutComments.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) + "SELECT".Length;
        var from = Regex.Match(withoutComments, @"^\s*FROM\s", RegexOptions.Multiline | RegexOptions.IgnoreCase).Index;

        return SplitTopLevel(withoutComments[select..from]).Select(item =>
        {
            var match = Regex.Match(item,
                @"AS\s+(?<type>NVARCHAR\s*\(\s*\d+\s*\)|INT|DECIMAL\s*\(\s*\d+\s*,\s*\d+\s*\)|DATETIME)\s*\)\s+AS\s+(?<name>\w+)\s*$",
                RegexOptions.IgnoreCase);
            match.Success.Should().BeTrue($"every projected column is CAST to a fixed type, but got: {item.Trim()}");
            var type = match.Groups["type"].Value.ToUpperInvariant();
            var clrType = type.StartsWith("NVARCHAR", StringComparison.Ordinal) ? typeof(string)
                : type.StartsWith("DECIMAL", StringComparison.Ordinal) ? typeof(decimal)
                : type == "INT" ? typeof(int)
                : typeof(DateTime);
            return (match.Groups["name"].Value, clrType);
        }).ToList();
    }

    private static IEnumerable<string> SplitTopLevel(string selectList)
    {
        var depth = 0;
        var inString = false;
        var start = 0;
        for (var i = 0; i < selectList.Length; i++)
        {
            switch (selectList[i])
            {
                case '\'':
                    inString = !inString;
                    break;
                case '(' when !inString:
                    depth++;
                    break;
                case ')' when !inString:
                    depth--;
                    break;
                case ',' when !inString && depth == 0:
                    yield return selectList[start..i];
                    start = i + 1;
                    break;
            }
        }
        yield return selectList[start..];
    }
}
