using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolMan
{
    public class LibVlcSingleton
    {
        private static LibVLC _instance;

        private LibVlcSingleton()
        {
            Core.Initialize();
            _instance = new LibVLC();
        }

        public static LibVLC Instance
        {
            get
            {
                if (_instance == null)
                {
                    new LibVlcSingleton();
                }
                return _instance;
            }
        }
    }
}
