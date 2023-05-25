using System.IO.Pipes;

namespace SimpleWatchDog
{
    public class SimpleIPC
    {
        public abstract class IPCBase
        {
            public string PipeName { get; init; }

            public bool IsHost { get; init; }

            public IPCBase(string pipeName, bool isHost = false)
            {
                PipeName = pipeName;
                IsHost = isHost;
            }
        }

        public class Server : IPCBase
        {
            public event EventHandler<string> MessageReceived;

            public Server(string pipeName) : base(pipeName, true)
            {
                WaitForNewConnection();
            }

            void WaitForNewConnection()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    pipe.BeginWaitForConnection(async (o) =>
                    {
                        var server = (NamedPipeServerStream)o.AsyncState;
                        server.EndWaitForConnection(o);
                        WaitForNewConnection();
                        var reader = new StreamReader(server);
                        string message;
                        while (!string.IsNullOrEmpty(message = await reader.ReadLineAsync()))
                        {
                            MessageReceived?.Invoke(this, message);
                        }
                        server.Disconnect();
                        await server.DisposeAsync();
                    }, pipe);
                });
            }
        }

        public class Client : IPCBase
        {
            Queue<string> MessageList { get; init; } = new Queue<string>();

            public Client(string pipeName) : base(pipeName, false)
            {
                new Thread(SendMessageThread)
                {
                    IsBackground = true
                }.Start();
            }

            public void SendMessage(string message)
            {
                MessageList.Enqueue(message);
            }

            async void SendMessageThread()
            {
                while (true)
                {
                    if (MessageList.Count > 0)
                    {
                        var pipe = new NamedPipeClientStream(PipeName);
                        await pipe.ConnectAsync();
                        var writer = new StreamWriter(pipe)
                        {
                            AutoFlush = true
                        };
                        while (MessageList.Count > 0)
                        {
                            await writer.WriteLineAsync(MessageList.Dequeue());
                        }
                        await pipe.DisposeAsync();
                    }
                }
            }
        }
    }
}
