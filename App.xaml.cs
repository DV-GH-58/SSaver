namespace SSaver;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;
    public static Window MainWindow = Window.Current;
    public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        MainWindow.Title = MainWindow.AppWindow.Title = "SSaver";
        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");
        MainWindow.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
        MainWindow.Activate();
    }
}

