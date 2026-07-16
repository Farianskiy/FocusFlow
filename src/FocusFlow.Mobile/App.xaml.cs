using MauiApplication = Microsoft.Maui.Controls.Application;
using MauiWindow = Microsoft.Maui.Controls.Window;

namespace FocusFlow.Mobile;

public partial class App : MauiApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiWindow CreateWindow(
        IActivationState? activationState)
    {
        return new MauiWindow(new AppShell());
    }
}