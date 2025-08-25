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

public sealed class WinUiDispatcher : IUiDispatcher
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();
    public void Post(Action action) => _queue.TryEnqueue(() => action());
}

public sealed class WinUiPromptService : IMessagePromptService
{
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
        var showTask = dlg.ShowAsync().AsTask();
        showTask.Wait();
        var result = showTask.GetAwaiter().GetResult();
        return result == ContentDialogResult.Primary;
    }
}