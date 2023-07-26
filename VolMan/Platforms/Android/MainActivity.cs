using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using static Microsoft.Maui.ApplicationModel.Platform;

[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage, MaxSdkVersion = 33)]
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.ManageMedia)]
namespace VolMan;

[Activity(Name = "Transcriber.Main",Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
}
