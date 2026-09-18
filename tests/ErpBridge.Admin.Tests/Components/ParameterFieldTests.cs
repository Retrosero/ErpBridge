using Bunit;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Admin.Tests.Components;

/// <summary>
/// The editor controls (P2c). The kind comes from the catalogue, which read it off Fora's own
/// designer, so these tests pin that a boolean is a switch and a colour channel is a number —
/// not a free-text box that lets someone store what the phone cannot read.
/// </summary>
public sealed class ParameterFieldTests : BunitContext
{
    private static ParameterValueDto Field(
        string editor,
        string value = "",
        string def = "",
        string? optionsJson = null,
        string? referenceKind = null,
        bool overridden = false,
        bool implemented = true,
        string? lastChangedBy = null,
        string? lastChangeSource = null,
        DateTimeOffset? lastChangedAtUtc = null) => new()
    {
        CatalogEntryId = Guid.NewGuid(),
        ParametreId = 58,
        Name = "DefaultKaynakDepoNo",
        Label = "Kaynak depo no :",
        Editor = editor,
        Value = value,
        DefaultValue = def,
        OptionsJson = optionsJson,
        ReferenceKind = referenceKind,
        IsOverridden = overridden,
        IsImplemented = implemented,
        LastChangedBy = lastChangedBy,
        LastChangeSource = lastChangeSource,
        LastChangedAtUtc = lastChangedAtUtc,
    };

    private IRenderedComponent<ParameterField> RenderField(
        ParameterValueDto field, Action<string>? onChange = null) =>
        Render<ParameterField>(ps => ps
            .Add(p => p.Field, field)
            .Add(p => p.Value, field.Value)
            .Add(p => p.ValueChanged, v => onChange?.Invoke(v)));

    [Fact]
    public void A_boolean_is_a_switch_that_writes_Foras_one_and_zero()
    {
        string? written = null;
        var cut = RenderField(Field("boolean", value: "0"), v => written = v);

        var box = cut.Find("input[type=checkbox]");
        box.HasAttribute("checked").Should().BeFalse();

        box.Change(true);

        // Fora stores "1"/"0"; writing "True" would leave a value the phone cannot read.
        written.Should().Be("1");
    }

    [Fact]
    public void A_boolean_that_is_on_shows_as_on()
    {
        RenderField(Field("boolean", value: "1")).Find("input[type=checkbox]")
            .HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void A_number_gets_a_number_box()
    {
        RenderField(Field("integer", value: "3")).Find("input[type=number]")
            .GetAttribute("step").Should().Be("1");

        RenderField(Field("decimal", value: "1,5")).Find("input[type=number]")
            .GetAttribute("step").Should().Be("any");
    }

    [Fact]
    public void A_colour_is_one_channel_between_zero_and_255_not_a_colour_picker()
    {
        var cut = RenderField(Field("color", value: "85"));

        // Fora's ColorPickEdit stores Red, Green and Blue as three separate 0–255 parameters. A
        // colour input would write "#aabbcc" into a field the phone reads as a number.
        var input = cut.Find("input[type=number]");
        input.GetAttribute("min").Should().Be("0");
        input.GetAttribute("max").Should().Be("255");
        cut.FindAll("input[type=color]").Should().BeEmpty();
    }

    [Fact]
    public void A_choice_lists_the_options_the_catalogue_carries()
    {
        var cut = RenderField(Field("choice", value: "1",
            optionsJson: """[{"value":"0","label":"Normal"},{"value":"1","label":"İade"}]"""));

        cut.FindAll("select option").Select(o => o.TextContent).Should().Equal("Normal", "İade");
    }

    [Fact]
    public void A_stored_value_the_option_list_does_not_know_is_kept_and_marked()
    {
        var cut = RenderField(Field("choice", value: "7",
            optionsJson: """[{"value":"0","label":"Normal"}]"""));

        // Dropping it would change the setting the moment someone opens the screen.
        cut.Markup.Should().Contain("listede yok");
        cut.FindAll("select option").Should().HaveCount(2);
    }

    [Fact]
    public void A_broken_option_list_leaves_an_empty_dropdown_not_a_broken_screen()
    {
        var cut = RenderField(Field("choice", value: "1", optionsJson: "{bozuk"));

        cut.FindAll("select").Should().ContainSingle();
    }

    [Fact]
    public void A_secret_is_not_shown_in_clear()
    {
        RenderField(Field("secret", value: "hunter2")).Find("input")
            .GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void A_reference_says_it_is_still_free_text()
    {
        var cut = RenderField(Field("reference", value: "1", referenceKind: "depo"));

        // D12 wants a picker over the ERP's own list; that feed arrives with the agent (P3f).
        // Until then it says so rather than pretending to validate.
        cut.Markup.Should().Contain("Depo numarası").And.Contain("serbest metin");
    }

    [Fact]
    public void A_parameter_Fora_has_no_editor_for_still_gets_one()
    {
        var cut = RenderField(Field(editor: null!, value: "1"));

        // Thirteen akilli parameters have no editor in Fora at all. They are still settings.
        cut.FindAll("input").Should().ContainSingle();
        cut.Markup.Should().Contain("tipi bilinmiyor");
    }

    [Fact]
    public void The_field_says_what_it_would_fall_back_to()
    {
        RenderField(Field("integer", value: "3", def: "1")).Markup.Should().Contain("varsayılan:");
        RenderField(Field("text", value: "x", def: "")).Markup.Should().Contain("(boş)");
    }

    [Fact]
    public void A_setting_this_release_ignores_is_labelled()
    {
        // Otherwise someone changes it and wonders why nothing happened (D16).
        RenderField(Field("boolean", implemented: false)).Markup.Should().Contain("etkisiz");
        RenderField(Field("boolean", overridden: true)).Markup.Should().Contain("sapmış");
    }

    [Fact]
    public void Revert_is_offered_only_when_there_is_a_row_to_remove()
    {
        RenderField(Field("text", value: "3", def: "1", overridden: true))
            .Markup.Should().Contain("varsayılana dön");

        // At its default there is no stored row, so there is nothing to put back.
        RenderField(Field("text", value: "1", def: "1"))
            .Markup.Should().NotContain("varsayılana dön");
    }

    [Fact]
    public void Revert_raises_the_field_rather_than_writing_the_default_itself()
    {
        ParameterValueDto? reverted = null;
        var field = Field("text", value: "3", def: "1", overridden: true);

        var cut = Render<ParameterField>(ps => ps
            .Add(p => p.Field, field)
            .Add(p => p.Value, field.Value)
            .Add(p => p.Reverted, f => reverted = f));

        cut.Find("button.prm-field__revert").Click();

        // The page removes the stored row; an operator should not have to know the default value
        // in order to get back to it.
        reverted.Should().BeSameAs(field);
    }

    [Fact]
    public void A_changed_field_says_who_changed_it_and_when()
    {
        var cut = RenderField(Field("text", value: "3", overridden: true,
            lastChangedBy: "gurbuz", lastChangeSource: "panel",
            lastChangedAtUtc: DateTimeOffset.Parse("2026-09-18T07:30:00Z")));

        cut.Markup.Should().Contain("gurbuz").And.Contain("18 Eyl 2026");
    }

    [Fact]
    public void A_change_with_nobody_behind_it_names_its_source()
    {
        var cut = RenderField(Field("text", value: "3", overridden: true,
            lastChangeSource: "import",
            lastChangedAtUtc: DateTimeOffset.Parse("2026-09-18T07:30:00Z")));

        // An import or a reset has no person behind it; a blank line would look like a bug.
        cut.Markup.Should().Contain("import");
    }

    [Fact]
    public void An_unsaved_edit_is_marked()
    {
        var field = Field("text", value: "eski");

        var cut = Render<ParameterField>(ps => ps
            .Add(p => p.Field, field)
            .Add(p => p.Value, "yeni"));

        // The operator may be several tabs away by the time they save.
        cut.Find("div.prm-field").ClassList.Should().Contain("prm-field--dirty");
    }
}
