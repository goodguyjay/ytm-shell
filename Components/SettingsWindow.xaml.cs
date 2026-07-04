using System.Windows;
using System.Windows.Controls;

namespace YoutubeMusicDesktop.Components;

public partial class SettingsWindow : Window
{
    public event SelectionChangedEventHandler? ThemeSelectionChanged;

    public SettingsWindow(IEnumerable<object> themes, object? currentTheme)
    {
        InitializeComponent();
        foreach (var t in themes)
            ThemeSelector.Items.Add(t);

        ThemeSelector.SelectedItem = currentTheme;
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GeneralPanel is null)
            return;

        GeneralPanel.Visibility =
            NavList.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;

        ExperimentalPanel.Visibility =
            NavList.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ThemeSelectionChanged?.Invoke(sender, e);

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
