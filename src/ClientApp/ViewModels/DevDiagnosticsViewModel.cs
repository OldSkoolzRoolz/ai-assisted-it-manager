// Project Name: ClientApp
// File Name: DevDiagnosticsViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace KC.ITCompanion.ClientApp.ViewModels;

public sealed class RegistryNode
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public ObservableCollection<RegistryNode> Children { get; } = [];
}

public sealed class DevDiagnosticsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<RegistryNode> RegistryRoots { get; } = [];
    public ICommand RefreshCommand { get; }

    public DevDiagnosticsViewModel()
    {
        RefreshCommand = new RelayCommand(_ => LoadRegistry(), _ => true);
        LoadRegistry();
    }

    private void LoadRegistry()
    {
        RegistryRoots.Clear();
        var machineRoot = new RegistryNode { Name = "HKLM\\Software\\AIManager\\Client\\Settings" };
        LoadRegistryBranch(Registry.LocalMachine, machineRoot, "Software\\AIManager\\Client\\Settings");
        if (machineRoot.Children.Count > 0) RegistryRoots.Add(machineRoot);
        var userRoot = new RegistryNode { Name = "HKCU\\Software\\AIManager\\Client\\Settings" };
        LoadRegistryBranch(Registry.CurrentUser, userRoot, "Software\\AIManager\\Client\\Settings");
        if (userRoot.Children.Count > 0) RegistryRoots.Add(userRoot);
    }

    private void LoadRegistryBranch(RegistryKey baseKey, RegistryNode parent, string subPath)
    {
        try
        {
            using var key = baseKey.OpenSubKey(subPath, false);
            if (key == null) return;
            foreach (var valName in key.GetValueNames())
            {
                parent.Children.Add(new RegistryNode { Name = valName, Value = key.GetValue(valName)?.ToString() });
            }
            foreach (var sub in key.GetSubKeyNames())
            {
                var child = new RegistryNode { Name = sub };
                parent.Children.Add(child);
                LoadRegistryBranch(baseKey, child, subPath + "\\" + sub);
            }
        }
        catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
