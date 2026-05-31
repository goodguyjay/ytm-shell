using System.Windows.Input;

namespace YoutubeMusicDesktop.Core;

public sealed class RelayCommand(Func<Task> execute) : ICommand
{
    // turn off the CS0067 warning because we don't need it for this simple implementation
#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute();
}
