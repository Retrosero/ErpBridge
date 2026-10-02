using System.Text;
using System.Xml;
using ErpBridge.CentralApi.Storage;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S7: the server reads the XML feed's codes and pictures by the phone's rules — the cases of Sipariş
/// Cepte <c>XmlFeedCoreTest</c> that concern codes and pictures, plus the server's own: DTDs (XXE) refused, namespaces
/// not resolved, Turkish code pages read.
/// </summary>
public sealed class XmlFeedImageReaderTests
{
    private const string SupplierXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Urunler>
          <Urun id="10">
            <StokKodu>ABC-1</StokKodu>
            <Barkod>8690000000011</Barkod>
            <UrunAdi>Çay Bardağı</UrunAdi>
            <Aciklama><![CDATA[<p>Cam <b>bardak</b></p><ul><li>6 adet</li></ul>]]></Aciklama>
            <Fiyat>1.234,50</Fiyat>
            <Resimler>
              <Resim>https://cdn.example.com/a1.jpg</Resim>
              <Resim>https://cdn.example.com/a2.jpg</Resim>
            </Resimler>
            <Kategori id="5">Mutfak</Kategori>
          </Urun>
          <Urun id="11">
            <StokKodu>abc-2 </StokKodu>
            <Barkod>8690000000028</Barkod>
            <UrunAdi>Tabak</UrunAdi>
            <Aciklama>Porselen</Aciklama>
            <Fiyat>99.90</Fiyat>
            <Resimler><Resim>https://cdn.example.com/b1.png</Resim></Resimler>
            <Kategori id="5">Mutfak</Kategori>
          </Urun>
          <Urun id="12">
            <StokKodu></StokKodu>
            <UrunAdi>Kodsuz</UrunAdi>
            <Resimler/>
          </Urun>
        </Urunler>
        """;

    private const string MerchantXml = """
        <rss xmlns:g="http://base.google.com/ns/1.0" version="2.0"><channel><title>Mağaza</title>
          <item><g:id>P1</g:id><title>Kalem</title><description>Mavi</description>
            <g:image_link>https://x.com/p1.jpg</g:image_link><g:additional_image_link>https://x.com/p1b.jpg</g:additional_image_link>
            <g:price>12.00 TRY</g:price><g:gtin>1234567890123</g:gtin></item>
          <item><g:id>P2</g:id><title>Silgi</title><description>Beyaz</description>
            <g:image_link>https://x.com/p2.jpg</g:image_link><g:price>3.50 TRY</g:price><g:gtin>1234567890124</g:gtin></item>
        </channel></rss>
        """;

    [Fact]
    public void Reader_maps_records_and_skips_those_without_a_code()
    {
        var result = Read(SupplierXml, "Urunler/Urun", ["StokKodu"], ["Resimler/Resim"]);
        result.RecordCount.Should().Be(3);
        result.SkippedWithoutCode.Should().Be(1);
        result.Items.Should().HaveCount(2);
        result.Items[0].Code.Should().Be("ABC-1");
        result.Items[0].Urls.Should().Equal("https://cdn.example.com/a1.jpg", "https://cdn.example.com/a2.jpg");
        result.Items[1].Code.Should().Be("abc-2", "the code is trimmed");
        result.Items[1].Urls.Should().Equal("https://cdn.example.com/b1.png");
    }

    [Fact]
    public void A_record_is_field_paths_to_values_with_attributes_and_without_wrappers()
    {
        var records = new List<Dictionary<string, List<string>>>();
        XmlFeedImageReader.ForEachRecord(Stream(SupplierXml), "Urunler/Urun", records.Add);
        records.Should().HaveCount(3);
        var first = records[0];
        first["@id"].Should().Equal("10");
        first["Kategori/@id"].Should().Equal("5");
        first["Kategori"].Should().Equal("Mutfak");
        first["Resimler/Resim"].Should().HaveCount(2);
        first.Should().NotContainKey("Resimler", "a wrapper whose children were fields is no field");
        first["Aciklama"].Single().Should().StartWith("<p>Cam", "CDATA is the element's text");
        records[2]["StokKodu"].Should().Equal("");
        records[2]["Resimler"].Should().Equal([""], "an empty element without children is an empty field");
    }

    [Fact]
    public void Google_merchant_feed_keeps_prefixes_as_written()
    {
        var result = Read(MerchantXml, "rss/channel/item", ["g:id"], ["g:image_link", "g:additional_image_link"]);
        result.Items.Select(i => i.Code).Should().Equal("P1", "P2");
        result.Items[0].Urls.Should().Equal("https://x.com/p1.jpg", "https://x.com/p1b.jpg");

        // The phone's parser does not resolve namespaces: an undeclared prefix is no error either.
        var undeclared = Read("<rss><channel><item><g:id>Q</g:id><g:image_link>https://x.com/q.jpg</g:image_link></item></channel></rss>",
            "rss/channel/item", ["g:id"], ["g:image_link"]);
        undeclared.Items.Single().Urls.Should().Equal("https://x.com/q.jpg");
    }

    [Fact]
    public void Duplicate_codes_keep_the_first_record()
    {
        var result = Read("<a><p><k>X</k><r>https://h/1.jpg</r></p><p><k> x </k><r>https://h/2.jpg</r></p></a>", "a/p", ["k"], ["r"]);
        result.Items.Should().ContainSingle().Which.Urls.Should().Equal("https://h/1.jpg");
        result.DuplicateCodes.Should().Be(1);
    }

    [Fact]
    public void Image_fields_left_empty_at_setup_are_read_once_the_supplier_fills_them()
    {
        var xml = "<a><p><k>X</k><Resim1>https://h/1.jpg</Resim1><Resim10>https://h/10.jpg</Resim10>" +
                  "<Resim2>https://h/2.jpg</Resim2><Logo>https://h/logo.jpg</Logo><Resim3></Resim3></p></a>";
        Read(xml, "a/p", ["k"], ["Resim1"]).Items[0].Urls.Should().Equal("https://h/1.jpg", "https://h/2.jpg", "https://h/10.jpg");
    }

    [Fact]
    public void Image_field_family_keeps_parent_and_separator_style()
    {
        string[] fields = ["Resimler/Resim_2", "Resim_3", "Resimler/Resim_1", "Resimler/Buyuk_1", "k"];
        XmlFeedImageReader.ImageFields(fields, ["Resimler/Resim_1"]).Should().Equal("Resimler/Resim_1", "Resimler/Resim_2");
        XmlFeedImageReader.ImageFields(fields, []).Should().BeEmpty();
    }

    [Fact]
    public void A_value_with_several_addresses_is_split_and_only_http_addresses_are_kept()
    {
        XmlFeedImageReader.SplitUrls("https://h/1.jpg, https://h/2.jpg|https://h/3.jpg;http://h/4.jpg\nhttps://h/5,6.jpg\r\nftp://h/7.jpg")
            .Should().Equal("https://h/1.jpg", "https://h/2.jpg", "https://h/3.jpg", "http://h/4.jpg", "https://h/5,6.jpg");
        XmlFeedImageReader.SplitUrls("resim.jpg").Should().BeEmpty();

        var result = Read("<a><p id=\"K1\"><img src=\"https://h/a.jpg\"/><img src=\"https://h/a.jpg\"/><img src=\"https://h/b.jpg\"/></p></a>",
            "a/p", ["@id"], ["img/@src"]);
        result.Items.Should().ContainSingle();
        result.Items[0].Code.Should().Be("K1", "the record's own attribute can be the code");
        result.Items[0].Urls.Should().Equal("https://h/a.jpg", "https://h/b.jpg");
    }

    [Fact]
    public void Codes_match_trimmed_and_upper_case()
    {
        XmlFeedImageReader.CodeKey(" abc-1 ").Should().Be("ABC-1");
    }

    [Theory]
    [InlineData("<?xml version=\"1.0\"?><!DOCTYPE a [<!ENTITY x SYSTEM \"file:///etc/passwd\">]><a><p><k>&x;</k></p></a>")]
    [InlineData("<?xml version=\"1.0\"?><!DOCTYPE a [<!ENTITY l \"lol\"><!ENTITY l2 \"&l;&l;&l;&l;&l;&l;&l;&l;\">]><a><p><k>&l2;</k></p></a>")]
    [InlineData("<!DOCTYPE a SYSTEM \"http://169.254.169.254/latest/meta-data\"><a><p><k>X</k></p></a>")]
    public void A_document_with_a_dtd_is_refused(string xml)
    {
        var read = () => Read(xml, "a/p", ["k"], ["r"]);
        read.Should().Throw<XmlException>();
    }

    [Fact]
    public void A_broken_document_throws()
    {
        var read = () => Read("<a><p><k>X</k><r>https://h/1.jpg</a>", "a/p", ["k"], ["r"]);
        read.Should().Throw<XmlException>();
    }

    [Fact]
    public void A_turkish_code_page_is_read()
    {
        var xml = "<?xml version=\"1.0\" encoding=\"windows-1254\"?><a><p><k>ŞİŞE-1</k><r>https://h/ş.jpg</r></p></a>";
        var bytes = CodePages().GetBytes(xml);
        var result = XmlFeedImageReader.Read(new MemoryStream(bytes), "a/p", ["k"], ["r"]);
        result.Items.Single().Code.Should().Be("ŞİŞE-1");
    }

    private static Encoding CodePages()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1254);
    }

    private static XmlFeedImageResult Read(string xml, string recordPath, string[] codes, string[] images) =>
        XmlFeedImageReader.Read(Stream(xml), recordPath, codes, images);

    private static MemoryStream Stream(string xml) => new(Encoding.UTF8.GetBytes(xml));
}
