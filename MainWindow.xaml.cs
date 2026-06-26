using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using YoutubeMusicDesktop.Core;

namespace YoutubeMusicDesktop;

public partial class MainWindow : FluentWindow
{
    private readonly WebViewManager _webViewManager;

    public MainWindow()
    {
        InitializeComponent();
        SetAppIcon();

        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Acrylic);

        var themeManager = new ThemeManager();
        _webViewManager = new WebViewManager(WebView, themeManager);

        AppTitleBar.SetThemes(themeManager.Available, themeManager.Current);

        Loaded += async (_, _) =>
        {
            try
            {
                await _webViewManager.InitializeAsync();

                var chrome = WindowChrome.GetWindowChrome(this);
                chrome?.ResizeBorderThickness = new Thickness(6);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize WebView: {ex}");
            }
        };

        _webViewManager.PlayStateChanged += playing =>
            Dispatcher.Invoke(() => UpdatePlayPauseIcon(playing));

        _webViewManager.TrackChanged += track =>
            Dispatcher.Invoke(() =>
                AppTitleBar.SetTitle(
                    string.IsNullOrEmpty(track.Artist)
                        ? track.Title
                        : $"{track.Title} - {track.Artist}"
                )
            );

        SetupTaskbar();
    }

    private void SetupTaskbar()
    {
        TaskbarItemInfo = new TaskbarItemInfo
        {
            ThumbButtonInfos =
            [
                new ThumbButtonInfo
                {
                    Description = "Previous",
                    ImageSource = RenderIcon("TaskbarPreviousIcon"),
                    Command = new RelayCommand(async () => await _webViewManager.PreviousAsync()),
                },
                new ThumbButtonInfo
                {
                    Description = "Play / Pause",
                    ImageSource = RenderIcon("TaskbarPlayIcon"),
                    Command = new RelayCommand(async () =>
                    {
                        await _webViewManager.TogglePlayPauseAsync();
                        var playing = await _webViewManager.IsPlayingAsync();
                        Dispatcher.Invoke(() => UpdatePlayPauseIcon(playing));
                    }),
                },
                new ThumbButtonInfo
                {
                    Description = "Next",
                    ImageSource = RenderIcon("TaskbarNextIcon"),
                    Command = new RelayCommand(async () => await _webViewManager.NextAsync()),
                },
            ],
        };
    }

    private void UpdatePlayPauseIcon(bool playing)
    {
        var key = playing ? "TaskbarPauseIcon" : "TaskbarPlayIcon";
        TaskbarItemInfo.ThumbButtonInfos[1].ImageSource = RenderIcon(key);
        AppTitleBar.SetPlayState(playing);
    }

    private RenderTargetBitmap RenderIcon(string resourceKey, int size = 24)
    {
        var drawing = (DrawingImage)FindResource(resourceKey);
        var visual = new DrawingVisual();

        using (var ctx = visual.RenderOpen())
            ctx.DrawImage(drawing, new Rect(0, 0, size, size));

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private void SetAppIcon() => Icon = RenderIcon("AppIcon", 256);

    protected override async void OnKeyDown(KeyEventArgs e)
    {
        try
        {
            base.OnKeyDown(e);

            if (e.Key != Key.Space)
                return;

            await _webViewManager.TogglePlayPauseAsync();
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to toggle play/pause: {ex.Message}");
        }
    }

    private async void AppTitleBar_ThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ThemeEntry theme)
                await _webViewManager.ApplyThemeAsync(theme);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Failed to apply theme: {exception.Message}");
        }
    }

    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var win = GetWindow(this)!;

            if (win.WindowState == WindowState.Maximized)
            {
                var mousePos = e.GetPosition(this);
                var screenPos = win.PointToScreen(mousePos);

                var ratio = mousePos.X / win.ActualWidth;

                win.WindowState = WindowState.Normal;
                win.UpdateLayout();

                win.Left = screenPos.X - win.ActualWidth * ratio;
                win.Top = screenPos.Y - mousePos.Y;
            }

            win.DragMove();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error during window drag: {exception.Message}");
        }
    }
}
