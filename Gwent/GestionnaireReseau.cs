using System;
using System.Threading.Tasks;
using Gwent.Net;
using Newtonsoft.Json;

namespace Gwent
{
    // Gère toutes les communications réseau de manière centralisée. 
    // Simplifie la gestion du mode multijoueur.
    public class GestionnaireReseau : IDisposable
    {
        #region Propriétés

        public bool EstConnecte { get; private set; }
        public bool EstServeur { get; private set; }
        public int IndexJoueurLocal { get; private set; }

        private Server _server;
        private Client _client;
        private bool _disposed;

        #endregion

        #region Événements

        public event Action<PlayCardDto> CarteJoueeRecue;
        public event Action<int> TourChangeRecu;
        public event Action<int> PasseRecu;
        public event Action<StartGameDto> PartieCommenceeRecue;
        public event Action Connecte;
        public event Action Deconnecte;
        public event Action<string> Erreur;

        #endregion

        #region Méthodes publiques - Serveur

        public async Task DemarrerServeurAsync(int port, Func<DeckChoiceDto, StartGameDto> handshakeFactory)
        {
            if (_server != null)
            {
                _server.Stop();
                _server.Dispose();
            }

            _server = new Server();
            _server.EnableHandshake(handshakeFactory);
            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnClientDisconnected;
            _server.MessageReceived += OnMessageReceived;

            _server.Start(port);

            EstServeur = true;
            IndexJoueurLocal = 0; // L'hôte est toujours J1

            await Task.CompletedTask;
        }

        public void ArreterServeur()
        {
            _server?.Stop();
            _server?.Dispose();
            _server = null;
            EstConnecte = false;
        }

        #endregion

        #region Méthodes publiques - Client

        public async Task ConnecterAsync(string host, int port)
        {
            if (_client != null)
            {
                _client.Disconnect();
                _client.Dispose();
            }

            _client = new Client();
            _client.Connected += OnConnected;
            _client.Disconnected += OnDisconnected;
            _client.MessageReceived += OnMessageReceived;

            await _client.ConnectAsync(host, port);

            EstServeur = false;
            IndexJoueurLocal = 1; // Le client est toujours J2
        }

        public void Deconnecter()
        {
            _client?.Disconnect();
            _client?.Dispose();
            _client = null;
            EstConnecte = false;
        }

        #endregion

        #region Envoi de messages

        public async Task EnvoyerChoixDeckAsync(int deckIndex, string playerName)
        {
            var dto = new DeckChoiceDto
            {
                DeckIndex = deckIndex,
                PlayerName = playerName
            };

            var msg = new NetMessage
            {
                Type = MessageType.DeckChoice,
                Payload = JsonConvert.SerializeObject(dto)
            };

            await EnvoyerAsync(msg);
        }

        public async Task EnvoyerCarteJoueeAsync(Carte carte, string zoneName, int playerIndex)
        {
            var dto = new PlayCardDto
            {
                CardName = carte.Nom,
                PlayerIndex = playerIndex,
                Zone = zoneName,
                IsWeather = carte.Type == TypeCarte.Meteo,
                Power = carte.Puissance,
                ImagePath = carte.ImagePath,
                Type = (int)carte.Type,
                Pouvoir = (int)carte.Pouvoir
            };

            var msg = new NetMessage
            {
                Type = MessageType.PlayCard,
                Payload = JsonConvert.SerializeObject(dto)
            };

            await EnvoyerAsync(msg);
        }

        public async Task EnvoyerChangementTourAsync(int nouveauTourIndex)
        {
            var msg = new NetMessage
            {
                Type = MessageType.TurnSwitched,
                Payload = nouveauTourIndex.ToString()
            };

            await EnvoyerAsync(msg);
        }

        public async Task EnvoyerPasseAsync(int playerIndex)
        {
            var msg = new NetMessage
            {
                Type = MessageType.Pass,
                Payload = playerIndex.ToString()
            };

            await EnvoyerAsync(msg);
        }

        private async Task EnvoyerAsync(NetMessage msg)
        {
            try
            {
                if (_server != null)
                {
                    await _server.SendAsync(msg);
                }
                else if (_client != null)
                {
                    await _client.SendAsync(msg);
                }
                else
                {
                    throw new InvalidOperationException("Aucune connexion active");
                }
            }
            catch (Exception ex)
            {
                OnErreur($"Erreur d'envoi : {ex.Message}");
            }
        }

        #endregion

        #region Gestionnaires d'événements

        private void OnClientConnected()
        {
            EstConnecte = true;
            Connecte?.Invoke();
        }

        private void OnClientDisconnected()
        {
            EstConnecte = false;
            Deconnecte?.Invoke();
        }

        private void OnConnected()
        {
            EstConnecte = true;
            Connecte?.Invoke();
        }

        private void OnDisconnected()
        {
            EstConnecte = false;
            Deconnecte?.Invoke();
        }

        private void OnMessageReceived(NetMessage msg)
        {
            if (msg == null) return;

            try
            {
                switch (msg.Type)
                {
                    case MessageType.StartGame:
                        var startDto = JsonConvert.DeserializeObject<StartGameDto>(msg.Payload);
                        PartieCommenceeRecue?.Invoke(startDto);
                        break;

                    case MessageType.PlayCard:
                        var playDto = JsonConvert.DeserializeObject<PlayCardDto>(msg.Payload);
                        CarteJoueeRecue?.Invoke(playDto);
                        break;

                    case MessageType.TurnSwitched:
                        if (int.TryParse(msg.Payload, out int tourIndex))
                        {
                            TourChangeRecu?.Invoke(tourIndex);
                        }
                        break;

                    case MessageType.Pass:
                        if (int.TryParse(msg.Payload, out int passeIndex))
                        {
                            PasseRecu?.Invoke(passeIndex);
                        }
                        break;

                    case MessageType.Error:
                        OnErreur(msg.Payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnErreur($"Erreur de traitement du message : {ex.Message}");
            }
        }

        private void OnErreur(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[GestionnaireReseau] Erreur : {message}");
            Erreur?.Invoke(message);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                ArreterServeur();
                Deconnecter();
            }

            _disposed = true;
        }

        #endregion
    }
}