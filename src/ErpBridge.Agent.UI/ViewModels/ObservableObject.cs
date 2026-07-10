using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ErpBridge.Agent.UI.ViewModels;

/// <summary>Minimal <see cref="INotifyPropertyChanged"/> base for the UI layer.</summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
