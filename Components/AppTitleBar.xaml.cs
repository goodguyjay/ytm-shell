using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YoutubeMusicDesktop.Components;

public partial class AppTitleBar : UserControl
{
    public event SelectionChangedEventHandler? ThemeSelectionChanged;

    private readonly Storyboard? _soundwave;

    public AppTitleBar()
    {
        InitializeComponent();
        _soundwave = (Storyboard)Resources["SoundwaveStoryboard"]!;
    }

    public void SetThemes(IEnumerable<object> themes, object? current)
    {
        foreach (var t in themes)
            ThemeSelector.Items.Add(t);
        ThemeSelector.SelectedItem = current;
    }

    public void SetTitle(string title) => TitleText.Text = title;

    public void SetPlayState(bool playing)
    {
        if (playing)
            _soundwave?.Begin(this, isControllable: true);
        else
            _soundwave?.Stop();
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ThemeSelectionChanged?.Invoke(sender, e);

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)!.WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        var win = Window.GetWindow(this)!;

        if (win.WindowState == WindowState.Maximized)
        {
            win.WindowState = WindowState.Normal;
            MaximizeIcon.Source = (DrawingImage)FindResource("WinMaximizeIcon");
        }
        else
        {
            win.WindowState = WindowState.Maximized;
            MaximizeIcon.Source = (DrawingImage)FindResource("WinRestoreIcon");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)!.Close();
}
