using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ErpBridge.Agent.UI.Converters;

/// <summary>Converts <see cref="bool"/> to <see cref="Visibility"/>.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
