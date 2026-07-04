using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YoutubeMusicDesktop.Components;

public partial class AppTitleBar : UserControl
{
    private readonly Storyboard? _soundwave;

    public event EventHandler? SettingsClicked;

    public event SelectionChangedEventHandler? ThemeSelectionChanged;

    public event EventHandler? BackRequested;

    public event EventHandler? ForwardRequested;

    public AppTitleBar()
    {
        InitializeComponent();
        _soundwave = (Storyboard)Resources["SoundwaveStoryboard"]!;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public void SetTitle(string title) => TitleText.Text = title;

    public void SetPlayState(bool playing)
    {
        if (playing)
            _soundwave?.Resume(this);
        else
            _soundwave?.Pause(this);
    }

    public void SetNavState(bool canGoBack, bool canGoForward)
    {
        BackButton.IsEnabled = canGoBack;
        ForwardButton.IsEnabled = canGoForward;
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ThemeSelectionChanged?.Invoke(sender, e);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _soundwave?.Begin(this, isControllable: true);
        _soundwave?.Pause(this);
        
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

    private void SettingsButton_Click(object sender, RoutedEventArgs e) =>
        SettingsClicked?.Invoke(this, EventArgs.Empty);

    private void BackButton_Click(object sender, RoutedEventArgs e) =>
        BackRequested?.Invoke(this, EventArgs.Empty);

    private void ForwardButton_Click(object sender, RoutedEventArgs e) =>
        ForwardRequested?.Invoke(this, EventArgs.Empty);
}
