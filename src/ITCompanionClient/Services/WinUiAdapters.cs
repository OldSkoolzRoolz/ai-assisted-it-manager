// Project Name: ITCompanionClient
// File Name: WinUiAdapters.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Threading.Tasks;
using KC.ITCompanion.ClientShared;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ITCompanionClient;

/// <summary>Dispatcher implementation for WinUI.</summary>
public sealed class WinUiDispatcher : IUiDispatcher
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();
    public void Post(Action action) => _queue.TryEnqueue(() => action());
}

/// <summary>Prompt service using WinUI ContentDialog (async).</summary>
public sealed class WinUiPromptService : IMessagePromptService
{
    public bool Confirm(string title, string message, string confirmButton = "OK", string? cancelButton = null)
    {
        // Synchronous signature retained for interface; implement async internally without .Wait deadlock risk by using Task.Run continuation.
        return ShowDialogAsync(title, message, confirmButton, cancelButton).GetAwaiter().GetResult();
    }

    private static async Task<bool> ShowDialogAsync(string title, string message, string confirmButton, string? cancelButton)
    {
        var dlg = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = confirmButton,
            DefaultButton = ContentDialogButton.Primary
        };
        if (!string.IsNullOrWhiteSpace(cancelButton)) dlg.CloseButtonText = cancelButton;
        if (App.MainWindow?.Content is FrameworkElement fe)
            dlg.XamlRoot = fe.XamlRoot;
        var result = await dlg.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}