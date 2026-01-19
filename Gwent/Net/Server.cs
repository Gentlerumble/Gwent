using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Gwent.Net
{
    public class Server : IDisposable
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _clientStream;
        private CancellationTokenSource _cts;

        public event Action<NetMessage> MessageReceived;
        public event Action ClientConnected;
        public event Action ClientDisconnected;

        // Handshake: fabrique StartGameDto à partir du DeckChoice reçu
        private Func<DeckChoiceDto, StartGameDto> _handshakeFactory;
        private bool _handshakeEnabled;

        public void Start(int port)
        {
            Stop(); // au cas où
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Task.Run(() => AcceptLoop(_cts.Token));
            Debug.WriteLine($"[Server] Started listening on port {port}");
        }

        // Active la gestion du handshake interne au serveur
        public void EnableHandshake(Func<DeckChoiceDto, StartGameDto> startDtoFactory)
        {
            _handshakeFactory = startDtoFactory ?? throw new ArgumentNullException(nameof(startDtoFactory));
            _handshakeEnabled = true;
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var tcp = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);

                    // Active KeepAlive (si supporté)
                    try { tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); } catch { }

                    // Remplace proprement une connexion existante par la nouvelle
                    ReplaceClient(tcp);

                    Debug.WriteLine("[Server] Client connected");
                    SafeInvoke(ClientConnected);

                    // Démarre la boucle de réception pour ce client
                    _ = Task.Run(() => ReceiveLoop(_client, _clientStream, token));
                }
            }
            catch (ObjectDisposedException) { /* listener arrêté */ }
            catch (Exception ex) { Debug.WriteLine("[Server] AcceptLoop error: " + ex.Message); }
        }

        private void ReplaceClient(TcpClient newClient)
        {
            try { _clientStream?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }

            _client = newClient;
            _clientStream = _client.GetStream();

            // Optionnel: timeouts
            try
            {
                _client.ReceiveTimeout = 0; // pas de timeout côté socket
                _client.SendTimeout = 0;
            }
            catch { }
        }

        private async Task ReceiveLoop(TcpClient tcp, NetworkStream ns, CancellationToken token)
        {
            if (tcp == null || ns == null) return;

            try
            {
                while (!token.IsCancellationRequested && tcp.Connected)
                {
                    try
                    {
                        // Lire entête longueur
                        var lenBuf = new byte[4];
                        int gotHeader = await ReadExactAsync(ns, lenBuf, 0, 4, token).ConfigureAwait(false);
                        if (gotHeader == 0)
                        {
                            Debug.WriteLine("[Server] ReceiveLoop: header read returned 0 (remote closed)");
                            break;
                        }

                        int len = BitConverter.ToInt32(lenBuf, 0);
                        if (len <= 0)
                        {
                            Debug.WriteLine("[Server] Invalid length header: " + len);
                            continue;
                        }

                        // Lire payload
                        var buf = new byte[len];
                        int gotBody = await ReadExactAsync(ns, buf, 0, len, token).ConfigureAwait(false);
                        if (gotBody == 0)
                        {
                            Debug.WriteLine("[Server] ReceiveLoop: body read returned 0 (remote closed)");
                            break;
                        }

                        var json = Encoding.UTF8.GetString(buf);
                        Debug.WriteLine("[Server] Received JSON: " + json);

                        NetMessage msg = null;
                        try
                        {
                            msg = JsonConvert.DeserializeObject<NetMessage>(json);
                        }
                        catch (Exception exDeser)
                        {
                            Debug.WriteLine("[Server] JSON deserialize error: " + exDeser.Message);
                            continue;
                        }

                        // Handshake si activé
                        if (_handshakeEnabled && msg?.Type == MessageType.DeckChoice)
                        {
                            try
                            {
                                var choice = JsonConvert.DeserializeObject<DeckChoiceDto>(msg.Payload);
                                var startDto = _handshakeFactory?.Invoke(choice);
                                if (startDto != null)
                                {
                                    var reply = new NetMessage
                                    {
                                        Type = MessageType.StartGame,
                                        Payload = JsonConvert.SerializeObject(startDto)
                                    };
                                    await SendAsync(reply).ConfigureAwait(false);
                                    Debug.WriteLine("[Server] Handshake StartGame sent");
                                }
                                else
                                {
                                    Debug.WriteLine("[Server] Handshake factory returned null StartGameDto");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("[Server] Handshake error: " + ex.Message);
                            }
                        }
                        else
                        {
                            SafeInvoke(MessageReceived, msg);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("[Server] ReceiveLoop canceled");
                        break;
                    }
                    catch (Exception exRead)
                    {
                        Debug.WriteLine("[Server] ReceiveLoop error: " + exRead.Message);
                        if (!tcp.Connected) break;
                        await Task.Delay(50, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                Debug.WriteLine("[Server] Client disconnected (receive loop end)");
                SafeInvoke(ClientDisconnected);

                // Fermer uniquement si c’est bien la connexion active
                if (tcp == _client)
                {
                    try { _clientStream?.Dispose(); } catch { }
                    try { _client?.Close(); } catch { }
                    _clientStream = null;
                    _client = null;
                }
            }
        }

        private static async Task<int> ReadExactAsync(NetworkStream ns, byte[] buffer, int offset, int count, CancellationToken token)
        {
            int read = 0;
            while (read < count)
            {
                int r = await ns.ReadAsync(buffer, offset + read, count - read, token).ConfigureAwait(false);
                if (r == 0) return 0;
                read += r;
            }
            return read;
        }

        public async Task SendAsync(NetMessage msg)
        {
            if (_client == null || !_client.Connected || _clientStream == null)
            {
                Debug.WriteLine("[Server] No client connected, cannot send");
                return;
            }

            var json = JsonConvert.SerializeObject(msg);
            var data = Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(data.Length);

            Debug.WriteLine("[Server] Sending JSON: " + json);

            try
            {
                await _clientStream.WriteAsync(len, 0, len.Length).ConfigureAwait(false);
                await _clientStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                Debug.WriteLine("[Server] SendAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Server] SendAsync error: " + ex.Message);
            }
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _listener?.Stop(); } catch { }
            try { _clientStream?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }
            _clientStream = null;
            _client = null;
            Debug.WriteLine("[Server] Stopped");
        }

        public void Dispose()
        {
            Stop();
            try { _cts?.Dispose(); } catch { }
        }

        private void SafeInvoke(Action action)
        {
            try { action?.Invoke(); } catch (Exception ex) { Debug.WriteLine("[Server] Handler error: " + ex.Message); }
        }

        private void SafeInvoke(Action<NetMessage> action, NetMessage msg)
        {
            try { action?.Invoke(msg); } catch (Exception ex) { Debug.WriteLine("[Server] Handler error: " + ex.Message); }
        }
    }
}
