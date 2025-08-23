// Project Name: ClientApp
// File Name: Splash.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


namespace KC.ITCompanion.ClientApp.Views;


/// <summary>
///     Interaction logic for Splash.xaml
/// </summary>
// SplashWindow.xaml.cs
public partial class Splash : Window
{
    private readonly string[] _messages = new[]
    {
        "LOADING FROM TAPE DRIVE... PLEASE WAIT",
        "> Parsing ADMX...",
        "> Initializing audit trail...",
        "> Configuring workspace presets...",
        "\"Back in my day, we deployed policies uphill both ways.\"",
        "READY TO EMPOWER YOUR IT STACK."
    };





    public Splash()
    {
        InitializeComponent();
        this.Loaded += async (s, e) => await ShowMessages();
    }





    private async Task ShowMessages()
    {
        foreach (var msg in this._messages)
        {
            this.SplashText.Text += "\n" + msg;
            await Task.Delay(1000); // Simulate tape delay
        }

        await Task.Delay(1500);
        Close(); // Or transition to MainWindow
    }
}