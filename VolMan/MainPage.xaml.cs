using Whisper.net.Ggml;
using LibVLCSharp.Shared;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Alerts;
using Whisper.net;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Windows.Input;

namespace VolMan;
public record StartTranscription(string mediapath);
public partial class MainPage : ContentPage
{
    string currentFile = null;
    public MainPage()
    {
        InitializeComponent();
        var messenger = WeakReferenceMessenger.Default;
        messenger.Register<StartTranscription>(this, OnStartTranscription);
        //make the model picker source the list of models from the model manager but make the model name the display text
        // Set the ItemsSource of the pkModelPicker to the whisperModels
        this.pkModelPicker.ItemsSource = WhisperModelManager.Instance.whisperModels;
        // Set the DisplayMemberPath to the Name property of the WhisperModel
        this.pkModelPicker.ItemDisplayBinding = new Binding("Name");
        this.pkModelPicker.SelectedIndex = WhisperModelManager.Instance.whisperModels.ToList().FindIndex(x => x.Name == Preferences.Get("preferredModel", WhisperModelManager.Instance.ModelBase.Name));
        this.pkModelPicker.SelectedIndexChanged += PkModelPicker_SelectedIndexChanged;
    }

    private void PkModelPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        WhisperModelManager.Instance.setPreferredModel((WhisperModel)this.pkModelPicker.SelectedItem);
    }

    private void OnStartTranscription(object recipient, StartTranscription message)
    {
        //Note: this behaviour is a temporary implementation, later it will be replaced with a more robust one that supports queuing
        //and actually starts the transcription process
        this.currentFile = message.mediapath;
        btnFileSelector.Text = "Selected: " + Path.GetFileName(message.mediapath);
    }

    private async void btnFileSelector_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            await Toast.Make("File permission not granted", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            return;
        }
        PickOptions pickopt = new()
        {
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.audio" } }, // UTType values
                { DevicePlatform.Android, new[] { "audio/*" } }, // MIME types
                { DevicePlatform.WinUI, new[] { ".opus", ".ogg", ".wav", ".mp3", ".m4a" } }, // file extensions
                { DevicePlatform.macOS, new[] { "public.audio" } }, // UTType values
                { DevicePlatform.Tizen, new[] { "audio/*" } }, // MIME types
            }),
            PickerTitle = "Select an audio to transcribe"
        };
        FileResult file = await FilePicker.Default.PickAsync(pickopt);
        if(file!=null)
        {
            this.currentFile = file.FullPath;
            btn.Text = "Selected: " + file.FileName;
        }
    }

    private async void btnTranscribe_Clicked(object sender, EventArgs e)
    {
        long mediaDuration = 0;
        if (this.currentFile == null)
        {
            await Toast.Make("No file was selected").Show(); return;
        }
        if (!File.Exists(this.currentFile))
        {
            await Toast.Make("The file currently selected was not found").Show(); return;
        }
        this.btnTranscribe.IsEnabled = false;
        var lvlc = LibVlcSingleton.Instance;
        Environment.CurrentDirectory = FileSystem.Current.AppDataDirectory;
        if(File.Exists("conv.wav")) File.Delete("conv.wav");
        this.btnTranscribe.Text = "Encoding...";
        var media = new Media(lvlc, this.currentFile, FromType.FromPath);
        media.AddOption(":sout=#transcode{acodec=s16l,channels=1,ab=128,samplerate=16000}:std{access=file,mux=wav,dst=conv.wav}");
        media.AddOption(":sout-keep");
        using (var mediaPlayer = new MediaPlayer(media))
        {
            mediaPlayer.Play();
            Thread.Sleep(200);
            mediaDuration = mediaPlayer.Length;
            // Wait for the media to finish playing
            while (mediaPlayer.State != VLCState.Ended)
            {
                System.Threading.Thread.Sleep(200);
                prgTranscriptionProgress.Progress = mediaPlayer.Position;
            }
        }
        prgTranscriptionProgress.Progress = 0;
        this.btnTranscribe.Text = "Loading Model...";
        using var whisperFactory = WhisperFactory.FromBuffer(WhisperModelManager.Instance.GetPreferredModelBytes());

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        using var fileStream = File.OpenRead("conv.wav");
        this.txtTranscribedText.Text = "";
        this.btnTranscribe.Text = "Transcribing...";
        try
        {
            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                //Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                txtTranscribedText.Text += result.Text;
                if (result.End.TotalMilliseconds>0)
                {
                    prgTranscriptionProgress.Progress = ((double)result.End.TotalMilliseconds) / ((double)mediaDuration);
                }
                Thread.Sleep(50);
            }
        }
        finally
        {
            this.btnTranscribe.IsEnabled = true;
            btnTranscribe.Text = "Transcribe";
        }
        
    }
    public ICommand OpenGithubUrl => new Command(async () =>
    {
        string url = "https://github.com";
        await Launcher.OpenAsync(new Uri(url));
    });

    private async void tbiGithub_Clicked(object sender, EventArgs e)
    {
        Uri uri = new("https://github.com/gabrielesilinic/VolMan/");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}

