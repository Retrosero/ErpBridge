using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ErpBridge.Agent.UI.Converters;

/// <summary>
/// Converts a <see cref="string"/> to <see cref="Visibility"/>: <c>Visible</c>
/// when the value is non-null and non-empty, <c>Collapsed</c> otherwise.
/// Used for binding the troubleshooting-hint panel's visibility to the
/// <c>TroubleshootingHint</c> property — the panel disappears when the
/// hint is empty (a successful test clears the hint).
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
