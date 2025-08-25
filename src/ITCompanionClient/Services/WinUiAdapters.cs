// Project Name: ITCompanionClient
// File Name: WinUiAdapters.cs
// Author: Kyle Crowder
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using KC.ITCompanion.ClientShared;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ITCompanionClient;

/// <summary>WinUI implementation of UI dispatcher abstraction.</summary>
public sealed class WinUiDispatcher : IUiDispatcher
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();
    /// <inheritdoc />
    public void Post(Action action) => _queue.TryEnqueue(() => action());
}

/// <summary>Blocking dialog prompt service (WinUI). TODO: provide async non-blocking variant.</summary>
public sealed class WinUiPromptService : IMessagePromptService
{
    /// <inheritdoc />
    public bool Confirm(string title, string message, string confirmButton = "OK", string? cancelButton = null)
    {
        var dlg = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = confirmButton,
            DefaultButton = ContentDialogButton.Primary
        };
        if (!string.IsNullOrWhiteSpace(cancelButton)) dlg.CloseButtonText = cancelButton;
        if (App.MainWindow is not null && App.MainWindow.Content is FrameworkElement fe)
            dlg.XamlRoot = fe.XamlRoot;
        // NOTE: ShowAsync used with .GetAwaiter to preserve simple IMessagePromptService sync contract; future async API may replace.
        var result = dlg.ShowAsync().AsTask().GetAwaiter().GetResult();
        return result == ContentDialogResult.Primary;
    }
}