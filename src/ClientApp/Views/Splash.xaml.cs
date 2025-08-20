using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KC.ITCompanion.ClientApp.Views;
/// <summary>
/// Interaction logic for Splash.xaml
/// </summary>
// SplashWindow.xaml.cs
public partial class Splash : Window
{
    private readonly string[] messages = new[]
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
        Loaded += async (s, e) => await ShowMessages();
    }

    private async Task ShowMessages()
    {
        foreach (var msg in messages)
        {
            SplashText.Text += "\n" + msg;
            await Task.Delay(1000); // Simulate tape delay
        }

        await Task.Delay(1500);
        Close(); // Or transition to MainWindow
    }
}
