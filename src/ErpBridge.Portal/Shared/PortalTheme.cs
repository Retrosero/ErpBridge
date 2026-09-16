using MudBlazor;

namespace ErpBridge.Portal.Shared;

/// <summary>
/// The portal's corporate look: a deep navy brand colour, calm neutrals and the system UI font
/// (no web-font download, so no third-party request from a company's browser).
/// </summary>
public static class PortalTheme
{
    private static readonly string[] FontStack = ["Inter", "Segoe UI", "system-ui", "-apple-system", "Roboto", "Helvetica Neue", "Arial", "sans-serif"];

    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1d3f8f",
            PrimaryDarken = "#152f6c",
            PrimaryLighten = "#3a5fb8",
            Secondary = "#0f766e",
            Tertiary = "#6d28d9",
            Info = "#0369a1",
            Success = "#15803d",
            Warning = "#b45309",
            Error = "#b91c1c",
            Dark = "#0f1b3d",
            Background = "#f3f5f9",
            BackgroundGray = "#eceff5",
            Surface = "#ffffff",
            AppbarBackground = "#ffffff",
            AppbarText = "#0f1b3d",
            DrawerBackground = "#0f1b3d",
            DrawerText = "#c7d2e8",
            DrawerIcon = "#8fa3cc",
            TextPrimary = "#101828",
            TextSecondary = "#5b6478",
            LinesDefault = "#e4e8f0",
            TableLines = "#eef1f6",
            TableHover = "#f5f8fd",
            Divider = "#e4e8f0",
            ActionDefault = "#5b6478",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            AppbarHeight = "64px",
            DrawerWidthLeft = "260px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = FontStack, FontSize = ".9rem", LineHeight = "1.5" },
            H4 = new H4Typography { FontFamily = FontStack, FontSize = "1.6rem", FontWeight = "700", LineHeight = "1.25", LetterSpacing = "-.01em" },
            H5 = new H5Typography { FontFamily = FontStack, FontSize = "1.25rem", FontWeight = "700", LineHeight = "1.3" },
            H6 = new H6Typography { FontFamily = FontStack, FontSize = "1.02rem", FontWeight = "650", LineHeight = "1.4" },
            Subtitle2 = new Subtitle2Typography { FontFamily = FontStack, FontSize = ".8rem", FontWeight = "600", LineHeight = "1.4" },
            Button = new ButtonTypography { FontFamily = FontStack, FontSize = ".875rem", FontWeight = "600", TextTransform = "none", LetterSpacing = "0" },
            Overline = new OverlineTypography { FontFamily = FontStack, FontSize = ".7rem", FontWeight = "700", LetterSpacing = ".08em" },
        },
    };
}
