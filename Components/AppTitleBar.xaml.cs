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
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var win = Window.GetWindow(this)!;
        win.StateChanged += OnWindowStateChanged;
        SyncMaximizeIcon(win.WindowState);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        var win = Window.GetWindow(this);
        if (win is not null)
            win.StateChanged -= OnWindowStateChanged;
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        var win = Window.GetWindow(this)!;
        SyncMaximizeIcon(win.WindowState);
    }

    private void SyncMaximizeIcon(WindowState state)
    {
        MaximizeIcon.Source =
            state == WindowState.Maximized
                ? (DrawingImage)FindResource("WinRestoreIcon")
                : (DrawingImage)FindResource("WinMaximizeIcon");
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)!.WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        var win = Window.GetWindow(this)!;
        win.WindowState =
            win.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)!.Close();
}
