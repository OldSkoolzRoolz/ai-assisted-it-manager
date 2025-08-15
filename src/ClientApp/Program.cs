using System;
using Microsoft.UI.Xaml;

namespace ClientApp;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.Start(_ => new App());
    }
}
