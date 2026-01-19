using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Gwent.Net
{
    public class Client : IDisposable
    {
        private TcpClient _tcp;
        private CancellationTokenSource _cts;
        private volatile bool _receiveLoopStarted;

        public event Action<NetMessage> MessageReceived;
        public event Action Connected;
        public event Action Disconnected;

        public async Task ConnectAsync(string host, int port)
        {
            // Empêche une double connexion
            if (_tcp != null && _tcp.Connected)
            {
                Debug.WriteLine("[Client] Already connected");
                Connected?.Invoke();
                return;
            }

            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _tcp = new TcpClient();

            // Active TCP KeepAlive pour limiter les déconnexions silencieuses
            try
            {
                _tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
            catch { /* certaines plateformes peuvent ne pas supporter */ }

            await _tcp.ConnectAsync(host, port).ConfigureAwait(false);
            Debug.WriteLine($"[Client] Connected to {host}:{port}");
            Connected?.Invoke();

            // Démarre une seule fois la boucle de réception
            if (!_receiveLoopStarted)
            {
                _receiveLoopStarted = true;
                _ = Task.Run(() => ReceiveLoop(_cts.Token));
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            if (_tcp == null)
                return;

            NetworkStream ns = null;
            try
            {
                ns = _tcp.GetStream();
            }
            catch (Exception exGet)
            {
                Debug.WriteLine("[Client] GetStream error: " + exGet.Message);
                SignalDisconnected();
                return;
            }

            try
            {
                while (!token.IsCancellationRequested && _tcp.Connected)
                {
                    try
                    {
                        // Lire l'en-tête longueur
                        var lenBuf = new byte[4];
                        int gotHeader = await ReadExactAsync(ns, lenBuf, 0, 4, token).ConfigureAwait(false);
                        if (gotHeader == 0)
                        {
                            // Flux fermé proprement par le serveur
                            Debug.WriteLine("[Client] ReceiveLoop: stream closed by remote");
                            break;
                        }

                        int len = BitConverter.ToInt32(lenBuf, 0);
                        if (len <= 0)
                        {
                            // Paquet invalide, on continue
                            Debug.WriteLine("[Client] Invalid length header: " + len);
                            continue;
                        }

                        // Lire le corps
                        var buf = new byte[len];
                        int gotBody = await ReadExactAsync(ns, buf, 0, len, token).ConfigureAwait(false);
                        if (gotBody == 0)
                        {
                            Debug.WriteLine("[Client] ReceiveLoop: body read returned 0 (remote closed)");
                            break;
                        }

                        var json = Encoding.UTF8.GetString(buf);
                        Debug.WriteLine("[Client] Received JSON: " + json);

                        NetMessage msg = null;
                        try
                        {
                            msg = JsonConvert.DeserializeObject<NetMessage>(json);
                        }
                        catch (Exception exDeser)
                        {
                            Debug.WriteLine("[Client] JSON deserialize error: " + exDeser.Message);
                            continue;
                        }

                        try
                        {
                            MessageReceived?.Invoke(msg);
                        }
                        catch (Exception exHandler)
                        {
                            Debug.WriteLine("[Client] MessageReceived handler error: " + exHandler.Message);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Annulation demandée: quitter proprement
                        Debug.WriteLine("[Client] ReceiveLoop canceled");
                        break;
                    }
                    catch (Exception exRead)
                    {
                        // Erreur de lecture: journaliser et vérifier l'état
                        Debug.WriteLine("[Client] ReceiveLoop error: " + exRead.Message);
                        // Si la socket n'est plus connectée, sortir
                        if (_tcp == null || !_tcp.Connected)
                            break;

                        // Petite pause pour éviter boucles serrées en cas d'erreurs répétées
                        await Task.Delay(50, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                Debug.WriteLine("[Client] Disconnected (receive loop end)");
                SignalDisconnected();

                // Ne pas réutiliser la même instance
                try { ns?.Dispose(); } catch { }
                try { _tcp?.Close(); } catch { }
                _receiveLoopStarted = false;
            }
        }

        public static async Task<int> ReadExactAsync(NetworkStream ns, byte[] buffer, int offset, int count, CancellationToken token)
        {
            int read = 0;
            while (read < count)
            {
                int r = await ns.ReadAsync(buffer, offset + read, count - read, token).ConfigureAwait(false);
                if (r == 0)
                {
                    // Le flux a été fermé par l'autre extrémité
                    break;
                }
                read += r;
            }
            return read;
        }

        public async Task SendAsync(NetMessage msg)
        {
            if (_tcp == null || !_tcp.Connected)
            {
                Debug.WriteLine("[Client] Not connected, cannot send");
                return;
            }

            var ns = _tcp.GetStream();
            var json = JsonConvert.SerializeObject(msg);
            var data = Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(data.Length);

            Debug.WriteLine("[Client] Sending JSON: " + json);

            try
            {
                await ns.WriteAsync(len, 0, len.Length).ConfigureAwait(false);
                await ns.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                Debug.WriteLine("[Client] SendAsync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Client] SendAsync error: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                Debug.WriteLine("[Client] Disconnect called");
                _cts?.Cancel();
            }
            catch { }

            try
            {
                _tcp?.Close();
            }
            catch { }

            // Evite de relancer la loop après déconnexion
            _receiveLoopStarted = false;
        }

        private void SignalDisconnected()
        {
            try
            {
                Disconnected?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Client] Disconnected handler error: " + ex.Message);
            }
        }

        public void Dispose()
        {
            Disconnect();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
