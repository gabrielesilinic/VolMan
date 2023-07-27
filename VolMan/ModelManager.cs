using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace VolMan
{
    public class WhisperModelManager
    {
        private Dictionary<string, WhisperModel> Models;
        public WhisperModel ModelTiny { get => Models["Tiny"]; }
        public WhisperModel ModelBase { get => Models["Base"]; }
        public WhisperModel[] whisperModels { get => Models.Values.ToArray(); }
        private static WhisperModelManager _instance;
        public static WhisperModelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WhisperModelManager();
                }
                return _instance;
            }
        }
        private WhisperModelManager()
        {
            WhisperModel[] Modelsarr = new[]
            {
                new WhisperModel("models/ggml-tiny.bin","Tiny",true),
                new WhisperModel("models/ggml-base.bin","Base",true),
            };
            Models = Modelsarr.ToDictionary(x => x.Name);
            if (!Preferences.ContainsKey("preferredModel"))
            {
                Preferences.Set("preferredModel", this.ModelBase.Name);
            }
        }
        public byte[] GetPreferredModelBytes()
        {
            return Models[Preferences.Get("preferredModel", this.ModelBase.Name)].GetBytes();
        }
        public void setPreferredModel(WhisperModel model)
        {
            //if preferred model does not exist add it to the dictionary
            if (!Models.ContainsKey(model.Name))
            {
                Models.Add(model.Name, model);
            }
            Preferences.Set("preferredModel", model.Name);
        }
    }
    public class WhisperModel
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public bool IsEmbedded { get; private set; }
        public WhisperModel(string path, string name, bool isEmbedded)
        {
            Path = path;
            Name = name;
            IsEmbedded = isEmbedded;
        }
        public async Task<byte[]> GetBytesAsync()
        {
            if (IsEmbedded)
            {
                /*
                 * PLEASE keep the code like this or android will blow in your face,
                 * seems like the stream length is not properly supported by android
                 * for this reason Read and ReadAsync will make the app crash.
                 * https://github.com/dotnet/maui/issues/7471
                 */
                var modeltask = FileSystem.OpenAppPackageFileAsync(this.Path);//await is the worst thing that ever happened to dotnet MAUI
                modeltask.Wait();
                using var stream = modeltask.Result;
                using var memoryStream = new MemoryStream();
                stream.CopyToAsync(memoryStream).Wait();//not await becase behaves weirdly:
                return memoryStream.ToArray();          //gets stuck and by using .ConfigureAwait(false) changes thread and makes extra mess
            }
            else
            {
                //open file of this path and return bytes
                if (File.Exists(this.Path))
                {
                    return File.ReadAllBytes(this.Path);
                }
                else
                {
                    throw new FileNotFoundException("File not found", this.Path);
                }
            }
        }
        public byte[] GetBytes()
        {
            var task = GetBytesAsync();
            task.Wait();
            return task.Result;
        }
    }
}
