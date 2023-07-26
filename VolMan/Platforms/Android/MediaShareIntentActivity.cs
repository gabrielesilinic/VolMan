using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using CommunityToolkit.Maui.Core;
using static Microsoft.Maui.ApplicationModel.Platform;
using Android.Widget;
using CommunityToolkit.Mvvm.Messaging;

[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage, MaxSdkVersion = 33)]
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.ManageMedia)]

namespace VolMan;

[Activity(Name = "Transcriber.Share", Theme = "@style/Maui.SplashTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait,Exported = true,LaunchMode = LaunchMode.SingleTask)]
[IntentFilter(new string[] { Android.Content.Intent.ActionSend }, Categories = new string[] { Android.Content.Intent.CategoryDefault }, DataMimeType = "audio/*", Label = "Transcribe with VolMan")]
public class MediaShareIntentActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if(Intent!=null) OnNewIntent(this.Intent);
    }
    protected override void OnNewIntent(Android.Content.Intent intent)
    {
        base.OnNewIntent(intent);
        if (intent != null)
        {
            Toast.MakeText(this, Intent.Type, ToastLength.Short).Show();

            if (Android.Content.Intent.ActionSend.Equals(intent.Action) && intent.Type != null)
            {
                if (intent.Type.StartsWith("audio/"))
                {
                    // Get the URI of the shared audio file
                    if (intent.ClipData != null)
                    {
                        var clipData = intent.ClipData;
                        if (clipData.ItemCount > 0)
                        {
                            var item = clipData.GetItemAt(0);
                            var uri = item.Uri;
                            //move as temp file that lasts until app is closed
                            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
                            using (var inputStream = ContentResolver.OpenInputStream(uri))
                            {
                                using (var outputStream = System.IO.File.Create(tempFile))
                                {
                                    inputStream.CopyTo(outputStream);
                                }
                            }
                            //call the the StartTranscription WeakMessage method
                            var messenger = WeakReferenceMessenger.Default;
                            messenger.Send(new StartTranscription(tempFile));

                        }
                    }
                }
            }
        }
    }

}