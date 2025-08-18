// Project Name: ClientApp
// File Name: RelayCommand.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using System.Windows.Input;


namespace ClientApp.ViewModels;


public class RelayCommand : ICommand
{
    private readonly Func<object?, bool>? _canExecute;
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _executeSync;





    public RelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }





    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _executeSync = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }





    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }





    public async void Execute(object? parameter)
    {
        if (_executeAsync != null)
        {
            await _executeAsync(parameter).ConfigureAwait(false);
        }
        else
        {
            _executeSync?.Invoke(parameter);
        }
    }





    public event EventHandler? CanExecuteChanged;





    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}