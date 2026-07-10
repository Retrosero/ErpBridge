using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ErpBridge.Agent.UI.Converters;

/// <summary>
/// Returns the logical negation of a boolean. Used by WPF bindings that
/// need to enable a control when a flag is OFF (for example the SQL
/// username/password textboxes when <c>UseWindowsAuth</c> is OFF).
/// </summary>
public sealed class BoolNegationConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : DependencyProperty.UnsetValue;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : DependencyProperty.UnsetValue;
}