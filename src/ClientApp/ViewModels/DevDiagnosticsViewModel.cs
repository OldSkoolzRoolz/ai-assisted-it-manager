// Project Name: ClientApp
// File Name: DevDiagnosticsViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Input;

namespace ClientApp.ViewModels;

public sealed class RegistryNode
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public ObservableCollection<RegistryNode> Children { get; } = [];
}

public sealed class DevDiagnosticsViewModel : INotifyPropertyChanged
{
    private readonly string _dbPath;
    public ObservableCollection<object> Tables { get; } = [];
    public ObservableCollection<string> TableNames { get; } = [];
    public ObservableCollection<IDictionary<string, object?>> TableRows { get; } = [];
    public ObservableCollection<RegistryNode> RegistryRoots { get; } = [];

    private string? _selectedTable;
    public string? SelectedTable { get => _selectedTable; set { if (_selectedTable != value) { _selectedTable = value; OnPropertyChanged(); LoadTableRows(); } } }

    private string? _selectedSchema;
    public string? SelectedSchema { get => _selectedSchema; private set { _selectedSchema = value; OnPropertyChanged(); } }

    private string? _rowFilter;
    public string? RowFilter { get => _rowFilter; set { if (_rowFilter != value) { _rowFilter = value; OnPropertyChanged(); LoadTableRows(); } } }

    private string? _sortColumn;
    public string? SortColumn { get => _sortColumn; set { if (_sortColumn != value) { _sortColumn = value; OnPropertyChanged(); LoadTableRows(); } } }

    private bool _sortDesc;
    public bool SortDesc { get => _sortDesc; set { if (_sortDesc != value) { _sortDesc = value; OnPropertyChanged(); LoadTableRows(); } } }

    public ICommand RefreshCommand { get; }

    public DevDiagnosticsViewModel()
    {
        _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIManager","client","behavior.db");
        RefreshCommand = new RelayCommand(_ => { LoadSchemas(); LoadTableRows(); LoadRegistry(); }, _ => true);
        LoadSchemas();
        LoadRegistry();
    }

    private void LoadSchemas()
    {
        TableNames.Clear();
        if (!File.Exists(_dbPath)) return;
        using var c = new SqliteConnection($"Data Source={_dbPath}");
        c.Open();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' ORDER BY name";
            using var r = cmd.ExecuteReader();
            var schemaText = new System.Text.StringBuilder();
            while (r.Read())
            {
                var name = r.GetString(0);
                if (!TableNames.Contains(name)) TableNames.Add(name);
                var sql = r.IsDBNull(1) ? "" : r.GetString(1);
                if (!string.IsNullOrWhiteSpace(sql))
                {
                    schemaText.AppendLine(sql).AppendLine(";");
                }
            }
            SelectedSchema = schemaText.ToString();
        }
        if (TableNames.Count > 0 && (SelectedTable == null || !TableNames.Contains(SelectedTable))) SelectedTable = TableNames[0];
    }

    private void LoadTableRows()
    {
        TableRows.Clear();
        if (string.IsNullOrWhiteSpace(SelectedTable) || !File.Exists(_dbPath)) return;
        using var c = new SqliteConnection($"Data Source={_dbPath}");
        c.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = $"SELECT * FROM {SelectedTable}";
        using var r = cmd.ExecuteReader();
        var data = new List<IDictionary<string, object?>>();
        while (r.Read())
        {
            var dict = new Dictionary<string, object?>();
            for (int i = 0; i < r.FieldCount; i++)
            {
                dict[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
            }
            data.Add(dict);
        }
        if (!string.IsNullOrWhiteSpace(RowFilter))
        {
            var filter = RowFilter.Trim().ToLowerInvariant();
            data = data.Where(d => d.Values.Any(v => v?.ToString()?.ToLowerInvariant().Contains(filter) == true)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(SortColumn) && data.Count > 0 && data[0].ContainsKey(SortColumn))
        {
            data = (_sortDesc ? data.OrderByDescending(d => d[SortColumn!]) : data.OrderBy(d => d[SortColumn!])).ToList();
        }
        foreach (var row in data) TableRows.Add(row);
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
                parent.Children.Add(new RegistryNode
                {
                    Name = valName,
                    Value = key.GetValue(valName)?.ToString()
                });
            }
            foreach (var sub in key.GetSubKeyNames())
            {
                var child = new RegistryNode { Name = sub };
                parent.Children.Add(child);
                LoadRegistryBranch(baseKey, child, Path.Combine(subPath, sub));
            }
        }
        catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
