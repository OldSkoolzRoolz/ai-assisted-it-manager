// Project Name: ClientShared
// File Name: RelayCommand.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// Simple <see cref="ICommand"/> implementation supporting sync or async delegates.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Func<object?, bool>? _canExecute;
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _executeSync;

    /// <summary>Create a command from an asynchronous delegate.</summary>
    /// <param name="execute">Async work to run.</param>
    /// <param name="canExecute">Optional predicate controlling enablement.</param>
    public RelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    { _executeAsync = execute ?? throw new ArgumentNullException(nameof(execute)); _canExecute = canExecute; }

    /// <summary>Create a command from a synchronous delegate.</summary>
    /// <param name="execute">Synchronous work to run.</param>
    /// <param name="canExecute">Optional predicate controlling enablement.</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    { _executeSync = execute ?? throw new ArgumentNullException(nameof(execute)); _canExecute = canExecute; }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc />
    public async void Execute(object? parameter)
    { if (_executeAsync != null) await _executeAsync(parameter).ConfigureAwait(false); else _executeSync?.Invoke(parameter); }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
    /// <summary>Notifies the UI to re-query <see cref="CanExecute"/>.</summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
