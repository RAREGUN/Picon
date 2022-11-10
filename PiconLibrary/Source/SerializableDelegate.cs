using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PiconLibrary.Source
{
    [Serializable]
    public class SerializableDelegate<TDelegate> where TDelegate : Delegate
    {
        private byte[] _instanceData;

        [NonSerialized]
        private TDelegate _instance;
        
        public TDelegate Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                
                Deserialize();

                return _instance;
            }
            set
            {
                _instance = value;
                
                Serialize();
            }
        }

        public SerializableDelegate(TDelegate target)
        {
            Instance = target;
        }
        
        private void Deserialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(_instanceData);
            _instance = (TDelegate) formatter.Deserialize(stream);
        }

        private void Serialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.Serialize(stream, _instance);
            stream.Position = 0;
            _instanceData = stream.GetBuffer();
        }
    }
}