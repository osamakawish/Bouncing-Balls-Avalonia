using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace Bouncing_Balls_Avalonia.Android
{
    [Activity(Label = "Bouncing_Balls_Avalonia.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaMainActivity
    {
    }
}