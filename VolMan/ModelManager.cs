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
        /*public async Task<byte[]> GetBytesAsync()
        {
            if (IsEmbedded)
            {
                

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
        }*/
        public byte[] GetBytes()
        {
            byte[] buff;
            Stream stream;
            var streamtask = FileSystem.OpenAppPackageFileAsync(this.Path);
            streamtask.Wait();
            stream = streamtask.Result;
            buff = new byte[stream.Length];
            stream.Read(buff, 0, (int)stream.Length);
            return buff;
        }
    }
}
