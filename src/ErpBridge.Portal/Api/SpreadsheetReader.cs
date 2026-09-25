using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ErpBridge.Portal.Api;

/// <summary>
/// Reads the rows of an imported product/customer file (GOAL_PANEL_ERPSIZ E1d/E2c): a CSV (<c>;</c>, <c>,</c> or tab,
/// quoted fields, UTF-8 with or without BOM, Windows-1254 as a fallback for Turkish Excel) or the first sheet of an
/// <c>.xlsx</c> workbook — without a spreadsheet library: an <c>.xlsx</c> is a zip of XML parts.
/// </summary>
public static class SpreadsheetReader
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";

    /// <summary>A decompressed part larger than this is refused (a zip bomb, or not a product list).</summary>
    private const long MaxPartBytes = 64L * 1024 * 1024;

    /// <summary>The file's rows, each a list of cell texts (trimmed; missing cells empty). Blank rows are dropped.</summary>
    /// <exception cref="InvalidDataException">The file is neither a readable CSV nor an <c>.xlsx</c>.</exception>
    public static List<string[]> Read(Stream stream, string fileName)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        buffer.Position = 0;
        var rows = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ? ReadXlsx(buffer) : ReadCsv(buffer.ToArray());
        return rows.Where(r => r.Any(c => c.Length > 0)).ToList();
    }

    // ---- CSV ------------------------------------------------------------------------------

    private static List<string[]> ReadCsv(byte[] bytes)
    {
        var text = Decode(bytes);
        var firstLine = text.Split('\n', 2)[0];
        var delimiter = new[] { ';', '\t', ',' }.OrderByDescending(d => firstLine.Count(ch => ch == d)).First();

        var rows = new List<string[]>();
        var row = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (quoted)
            {
                if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                else if (ch == '"') quoted = false;
                else cell.Append(ch);
            }
            else if (ch == '"' && cell.Length == 0) quoted = true;
            else if (ch == delimiter) { row.Add(cell.ToString().Trim()); cell.Clear(); }
            else if (ch == '\n' || ch == '\r')
            {
                if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                row.Add(cell.ToString().Trim());
                cell.Clear();
                rows.Add([.. row]);
                row.Clear();
            }
            else cell.Append(ch);
        }
        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString().Trim());
            rows.Add([.. row]);
        }
        return rows;
    }

    /// <summary>UTF-8 when the bytes are valid UTF-8 (BOM or not); otherwise Windows-1254, the Turkish Excel default.</summary>
    private static string Decode(byte[] bytes)
    {
        try
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes).TrimStart('﻿');
        }
        catch (DecoderFallbackException)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(1254).GetString(bytes);
        }
    }

    // ---- XLSX -----------------------------------------------------------------------------

    private static List<string[]> ReadXlsx(Stream stream)
    {
        ZipArchive zip;
        try
        {
            zip = new ZipArchive(stream, ZipArchiveMode.Read);
        }
        catch (InvalidDataException)
        {
            throw new InvalidDataException("Not an .xlsx workbook.");
        }
        using (zip)
        {
            var shared = zip.GetEntry("xl/sharedStrings.xml") is { } sharedPart
                ? Load(sharedPart).Root!.Elements(Main + "si").Select(si => string.Concat(si.Descendants(Main + "t").Select(t => t.Value))).ToList()
                : [];
            var sheet = zip.GetEntry(FirstSheetPath(zip)) ?? throw new InvalidDataException("The workbook has no sheet.");

            var rows = new List<string[]>();
            foreach (var row in Load(sheet).Root!.Element(Main + "sheetData")?.Elements(Main + "row") ?? [])
            {
                var cells = new SortedDictionary<int, string>();
                var next = 0;
                foreach (var c in row.Elements(Main + "c"))
                {
                    var column = c.Attribute("r") is { } reference ? ColumnIndex(reference.Value) : next;
                    next = column + 1;
                    cells[column] = CellText(c, shared).Trim();
                }
                if (cells.Count == 0) continue;
                var values = new string[cells.Keys.Max() + 1];
                Array.Fill(values, string.Empty);
                foreach (var (column, value) in cells) values[column] = value;
                rows.Add(values);
            }
            return rows;
        }
    }

    /// <summary>The workbook's first sheet, as its relationships name it; <c>sheet1.xml</c> when they cannot be read.</summary>
    private static string FirstSheetPath(ZipArchive zip)
    {
        if (zip.GetEntry("xl/workbook.xml") is { } workbookPart && zip.GetEntry("xl/_rels/workbook.xml.rels") is { } relsPart)
        {
            var id = Load(workbookPart).Root!.Element(Main + "sheets")?.Elements(Main + "sheet").FirstOrDefault()?.Attribute(Relationships + "id")?.Value;
            var target = Load(relsPart).Root!.Elements(PackageRelationships + "Relationship")
                .FirstOrDefault(r => r.Attribute("Id")?.Value == id)?.Attribute("Target")?.Value;
            if (target is not null) return target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target;
        }
        return "xl/worksheets/sheet1.xml";
    }

    private static string CellText(XElement c, List<string> shared)
    {
        var value = c.Element(Main + "v")?.Value;
        return c.Attribute("t")?.Value switch
        {
            "s" => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < shared.Count ? shared[index] : string.Empty,
            "inlineStr" => string.Concat(c.Element(Main + "is")?.Descendants(Main + "t").Select(t => t.Value) ?? []),
            "b" => value == "1" ? "1" : "0",
            _ => value ?? string.Empty,
        };
    }

    /// <summary>"C7" → 2.</summary>
    private static int ColumnIndex(string reference)
    {
        var column = 0;
        foreach (var ch in reference.TakeWhile(char.IsLetter)) column = column * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
        return column - 1;
    }

    private static XDocument Load(ZipArchiveEntry entry)
    {
        if (entry.Length > MaxPartBytes) throw new InvalidDataException("The workbook is too large.");
        using var part = entry.Open();
        using var reader = XmlReader.Create(part, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        return XDocument.Load(reader);
    }

    /// <summary>
    /// A number as a person typed it: <c>1234.5</c>, <c>1234,5</c> or <c>1.234,50</c> (the last separator is the decimal one).
    /// Null for empty text; <paramref name="valid"/> false for text that is not a number.
    /// </summary>
    public static decimal? Number(string? text, out bool valid)
    {
        valid = true;
        var value = text?.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (string.IsNullOrEmpty(value)) return null;
        var lastComma = value.LastIndexOf(',');
        var lastDot = value.LastIndexOf('.');
        var normalized = lastComma > lastDot
            ? value.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.')
            : value.Replace(",", string.Empty, StringComparison.Ordinal);
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)) return number;
        valid = false;
        return null;
    }
}
