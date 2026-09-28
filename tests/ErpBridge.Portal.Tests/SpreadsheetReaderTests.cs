using System.IO.Compression;
using System.Text;
using ErpBridge.Portal.Api;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_PANEL_ERPSIZ E1d/E2c: the import file reader (CSV and .xlsx, no spreadsheet library).</summary>
public sealed class SpreadsheetReaderTests
{
    private static List<SpreadsheetReader.Row> Csv(string text) =>
        SpreadsheetReader.Read(new MemoryStream(new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray()), "urunler.csv");

    [Fact]
    public void A_turkish_excel_csv_with_semicolons_quotes_and_blank_lines_reads_row_by_row()
    {
        var rows = Csv("Kod;Ad;Fiyat\r\nCAY-1;\"Çay; 1 kg\";1.234,50\r\n\r\nSEKER-1;\"Şeker \"\"5\"\" kg\";10\r\n");

        rows.Should().HaveCount(3);
        rows[1].Cells.Should().Equal("CAY-1", "Çay; 1 kg", "1.234,50");
        rows[2].Cells.Should().Equal("SEKER-1", "Şeker \"5\" kg", "10");
        rows.Select(r => r.Number).Should().Equal(new[] { 1, 2, 4 }, "the blank line keeps its number (Codex #199)");
    }

    [Fact]
    public void A_comma_separated_file_and_a_windows_1254_file_read_too()
    {
        Csv("Kod,Ad\nA,B\n").Should().HaveCount(2).And.Contain(r => r.Cells.SequenceEqual(new[] { "A", "B" }));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(1254).GetBytes("Kod;Ad\nSEKER-1;Şeker ığdır\n");
        SpreadsheetReader.Read(new MemoryStream(bytes), "x.csv")[1].Cells.Should().Equal("SEKER-1", "Şeker ığdır");
    }

    [Fact]
    public void An_xlsx_first_sheet_reads_shared_inline_and_number_cells_in_their_columns_and_rows()
    {
        using var file = new MemoryStream();
        using (var zip = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: true))
        {
            void Part(string path, string xml)
            {
                using var writer = new StreamWriter(zip.CreateEntry(path).Open(), new UTF8Encoding(false));
                writer.Write(xml);
            }
            const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            Part("xl/workbook.xml", $"<workbook xmlns=\"{ns}\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Ürünler\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            Part("xl/_rels/workbook.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Target=\"worksheets/urunler.xml\"/></Relationships>");
            Part("xl/sharedStrings.xml", $"<sst xmlns=\"{ns}\"><si><t>Kod</t></si><si><t>Ad</t></si><si><r><t>Çay </t></r><r><t>1 kg</t></r></si></sst>");
            Part("xl/worksheets/urunler.xml", $"<worksheet xmlns=\"{ns}\"><sheetData>"
                + "<row r=\"1\"><c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"B1\" t=\"s\"><v>1</v></c><c r=\"C1\" t=\"inlineStr\"><is><t>Fiyat</t></is></c></row>"
                + "<row r=\"2\"><c r=\"A2\" t=\"inlineStr\"><is><t>CAY-1</t></is></c><c r=\"C2\"><v>1234.5</v></c><c r=\"B2\" t=\"s\"><v>2</v></c></row>"
                + "<row r=\"5\"><c r=\"C5\"><v>7</v></c></row>"
                + "</sheetData></worksheet>");
        }
        file.Position = 0;

        var rows = SpreadsheetReader.Read(file, "urunler.xlsx");

        rows[0].Cells.Should().Equal("Kod", "Ad", "Fiyat");
        rows[1].Cells.Should().Equal("CAY-1", "Çay 1 kg", "1234.5");
        rows[2].Cells.Should().Equal(new[] { "", "", "7" }, "a skipped cell keeps its column");
        rows[2].Number.Should().Be(5, "the sheet's own row number, rows 3-4 being empty (Codex #199)");
    }

    [Theory]
    [InlineData("1234.5", 1234.5)]
    [InlineData("1234,5", 1234.5)]
    [InlineData("1.234,50", 1234.50)]
    [InlineData("1,234.50", 1234.50)]
    [InlineData(" 18 ", 18)]
    public void Numbers_read_the_way_people_type_them(string text, decimal expected)
    {
        SpreadsheetReader.Number(text, out var valid).Should().Be(expected);
        valid.Should().BeTrue();
    }

    [Fact]
    public void An_empty_number_is_null_and_a_word_is_invalid()
    {
        SpreadsheetReader.Number("", out var emptyValid).Should().BeNull();
        emptyValid.Should().BeTrue();
        SpreadsheetReader.Number("on beş", out var wordValid).Should().BeNull();
        wordValid.Should().BeFalse();
    }
}
