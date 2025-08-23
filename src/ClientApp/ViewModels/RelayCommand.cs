// Project Name: ClientApp
// File Name: RelayCommand.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Windows.Input;


namespace KC.ITCompanion.ClientApp.ViewModels;


public class RelayCommand : ICommand
{
    private readonly Func<object?, bool>? _canExecute;
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _executeSync;





    public RelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        this._executeAsync = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
    }





    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        this._executeSync = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute;
    }





    public bool CanExecute(object? parameter)
    {
        return this._canExecute?.Invoke(parameter) ?? true;
    }





    public async void Execute(object? parameter)
    {
        if (this._executeAsync != null)
            await this._executeAsync(parameter).ConfigureAwait(false);
        else
            this._executeSync?.Invoke(parameter);
    }





    public event EventHandler? CanExecuteChanged;





    public void RaiseCanExecuteChanged()
    {
        this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}