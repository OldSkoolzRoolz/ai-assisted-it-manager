// Project Name: ClientShared
// File Name: Abstractions.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Threading;
using System.Threading.Tasks;

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// Abstraction for UI thread dispatching used by shared view models to avoid direct framework dependency.
/// </summary>
public interface IUiDispatcher
{
    /// <summary>Posts an action to the UI thread.</summary>
    void Post(Action action);
}

/// <summary>
/// Abstraction for prompting the user (message boxes / dialogs) decoupled from WPF / WinUI specifics.
/// </summary>
public interface IMessagePromptService
{
    /// <summary>Shows an information / confirmation prompt.</summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Dialog body.</param>
    /// <param name="confirmButton">Text for confirm button.</param>
    /// <param name="cancelButton">Optional cancel button text (null = only confirm).</param>
    /// <returns>True if user confirmed; otherwise false.</returns>
    bool Confirm(string title, string message, string confirmButton = "OK", string? cancelButton = null);
}
