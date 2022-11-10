using System;
using RpcSelf;

namespace Picon.Subscripts
{
    public class PiconViewerBridge : IDisposable
    {
        private RpcSelfHost<string> FilePathReceiverHost { get; }
        private RpcSelfClient<string> PiconViewerInitializedClient { get; }

        private static PiconViewerBridge _instance;
        private static PiconViewerBridge Instance => _instance ?? (_instance = new PiconViewerBridge());

        private PiconViewerBridge()
        {
            // Creating Host which will receive image paths
            FilePathReceiverHost = new RpcSelfHost<string>(11100);
            
            // Creating Client which will send signal about success start
            PiconViewerInitializedClient = new RpcSelfClient<string>(11101);
        }

        public static void OnPiconViewerInitialized(Action<string> onFilePathReceived)
        {
            // Assign action on receiving image path
            Instance.FilePathReceiverHost.MessageReceived += (sender, args) =>
            {
                onFilePathReceived.Invoke(args.Object);
            };
            
            // Send signal about success start of PiconViewer
            Instance.PiconViewerInitializedClient.SendData(string.Empty);
        }
        
        public void Dispose()
        {
            FilePathReceiverHost?.Dispose();
        }
    }
}