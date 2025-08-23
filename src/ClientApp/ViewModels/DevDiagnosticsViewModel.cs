// Project Name: ClientApp
// File Name: DevDiagnosticsViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;


namespace KC.ITCompanion.ClientApp.ViewModels;


public sealed class RegistryNode
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public ObservableCollection<RegistryNode> Children { get; } = [];
}



public sealed class DevDiagnosticsViewModel : INotifyPropertyChanged
{
    public DevDiagnosticsViewModel()
    {
        this.RefreshCommand = new RelayCommand(_ => LoadRegistry(), _ => true);
        LoadRegistry();
    }

    public ObservableCollection<RegistryNode> RegistryRoots { get; } = [];
    public ICommand RefreshCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;





    private void LoadRegistry()
    {
        this.RegistryRoots.Clear();
        var machineRoot = new RegistryNode { Name = "HKLM\\Software\\AIManager\\Client\\Settings" };
        LoadRegistryBranch(Registry.LocalMachine, machineRoot, "Software\\AIManager\\Client\\Settings");
        if (machineRoot.Children.Count > 0) this.RegistryRoots.Add(machineRoot);
        var userRoot = new RegistryNode { Name = "HKCU\\Software\\AIManager\\Client\\Settings" };
        LoadRegistryBranch(Registry.CurrentUser, userRoot, "Software\\AIManager\\Client\\Settings");
        if (userRoot.Children.Count > 0) this.RegistryRoots.Add(userRoot);
    }

    private static void LoadRegistryBranch(RegistryKey baseKey, RegistryNode parent, string subPath) // CA1822 -> static
    {
        try
        {
            using RegistryKey? key = baseKey.OpenSubKey(subPath, false);
            if (key == null) return;
            foreach (var valName in key.GetValueNames())
                parent.Children.Add(new RegistryNode { Name = valName, Value = key.GetValue(valName)?.ToString() });
            foreach (var sub in key.GetSubKeyNames())
            {
                var child = new RegistryNode { Name = sub };
                parent.Children.Add(child);
                LoadRegistryBranch(baseKey, child, subPath + "\\" + sub);
            }
        }
        catch
        {
        }
    }





    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}