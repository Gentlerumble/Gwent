using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Gwent.Net;

namespace Gwent
{
    static class Program
    {
        // Point d'entrée de l'application
        // Le formulaire pour choisir les decks (FormDeck) et le mode de jeu
        // Lancer le formulaire principlal du jeu avec la bonne config

        [STAThread] //Single Thread Apartment pour Windows Forms
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var formDeck = new FormDeck())
            {
                // Si l'utilisateur annule la sélection
                if (formDeck.ShowDialog() != DialogResult.OK)
                    return;

                // Cas :  Charger une partie sauvegardée
                if (formDeck.ChargerPartieSelected && formDeck.PartieSauvegardee != null)
                {
                    GererChargementSauvegarde(formDeck.PartieSauvegardee);

                    return;
                }

                // Cas : Partie locale (2 joueurs même PC)
                if (!formDeck.HostSelected && !formDeck.ClientSelected)
                {
                    var deckJ1 = formDeck.DeckJ1;
                    var deckJ2 = formDeck.DeckJ2;

                    var idxJ1 = formDeck.IndexDeckJ1;
                    var idxJ2 = formDeck.IndexDeckJ2;

                    Application.Run(new FPrincipal(deckJ1, deckJ2, idxJ1, idxJ2));

                    return;
                }

                // Cas : Héberger une nouvelle partie réseau
                if (formDeck.HostSelected)
                {
                    LancerPartieHost(formDeck);

                    return;
                }

                // Cas : Rejoindre une nouvelle partie réseau
                if (formDeck.ClientSelected)
                {
                    LancerPartieClient(formDeck);

                    return;
                }
            } //FormDeck Dispose pour libérer les ressources
        }

        #region Chargement de sauvegarde

        // Gère le chargement d'une sauvegarde (locale ou réseau)
        
        private static void GererChargementSauvegarde(GameSaveDto save)
        {
            // Si c'était une partie réseau, demander comment continuer
            if (save.EstPartieReseau)
            {
                using (var formChoix = new FormChoixReprise(save.SaveName))
                {
                    if (formChoix.ShowDialog() != DialogResult.OK)
                        return;

                    switch (formChoix.ModeChoisi)
                    {
                        case ModeReprise.Local:
                            LancerPartieChargeeLocale(save);
                            break;

                        case ModeReprise.Heberger:
                            LancerPartieChargeeHost(save);
                            break;

                        case ModeReprise.Rejoindre:
                            LancerPartieChargeeClient(formChoix.AdresseServeur, formChoix.PortServeur);
                            break;

                        case ModeReprise.Annuler:
                        default:
                            return;
                    }
                }
            }
            else
            {
                // Partie locale :  charger directement
                LancerPartieChargeeLocale(save);
            }
        }

        // Lance une partie chargée en mode local (2 joueurs sur le même PC)

        private static void LancerPartieChargeeLocale(GameSaveDto save)
        {
            var allDecks = Jeu.AvoirDeckDispo();
            var deckJ1 = new List<Carte>(allDecks[save.Joueur1.IndexDeck]);
            var deckJ2 = new List<Carte>(allDecks[save.Joueur2.IndexDeck]);

            var mainForm = new FPrincipal(deckJ1, deckJ2, save.Joueur1.IndexDeck, save.Joueur2.IndexDeck);
            mainForm.ChargerDepuisSauvegarde(save);

            Application.Run(mainForm);
        }

        // Lance une partie chargée en tant qu'hôte (attend un client)
        private static void LancerPartieChargeeHost(GameSaveDto save)
        {
            var server = new Server();
            var mre = new ManualResetEventSlim(false);
            bool clientConnected = false;

            // Quand un client se connecte et envoie DeckChoice, on lui envoie l'état de la sauvegarde
            server.MessageReceived += async (msg) =>
            {
                if (msg == null) return;

                if (msg.Type == MessageType.DeckChoice)
                {
                    System.Diagnostics.Debug.WriteLine("[Host-Save] Client connecté, envoi de l'état de la sauvegarde.. .");

                    // Créer le DTO (Data Transfer Object) avec la sauvegarde
                    var gameState = new GameStateDto
                    {
                        EstChargementSauvegarde = true,
                        SaveData = save,
                        AssignedPlayerIndex = 1  // Le client sera J2
                    };

                    //Sérialiser en JSON et préparer l'envoi
                    var reply = new NetMessage
                    {
                        Type = MessageType.GameState,
                        Payload = JsonConvert.SerializeObject(gameState)
                    };
                    //Envoyer au client
                    await server.SendAsync(reply).ConfigureAwait(false);
                    clientConnected = true;
                    mre.Set();
                }
            };

            // Démarrer le serveur
            server.Start(12345);

            // Afficher la fenêtre d'attente
            using (var waitForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Width = 450,
                Height = 150,
                Text = "Attente d'un joueur.. .",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lbl = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"En attente d'un joueur sur le port 12345...\n\nSauvegarde :  {save.SaveName}\nVotre IP locale : 127.0.0.1",
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Font = new System.Drawing.Font("Segoe UI", 10)
                };

                var btnAnnuler = new Button
                {
                    Text = "Annuler",
                    Dock = DockStyle.Bottom,
                    Height = 35
                };
                btnAnnuler.Click += (s, e) => waitForm.Close();

                waitForm.Controls.Add(lbl);
                waitForm.Controls.Add(btnAnnuler);
                waitForm.Show();

                // Attendre la connexion (max 10 minutes)
                var timeout = TimeSpan.FromMinutes(10);
                var sw = System.Diagnostics.Stopwatch.StartNew();

                while (!mre.IsSet && sw.Elapsed < timeout && waitForm.Visible)
                {
                    Application.DoEvents();
                    Thread.Sleep(50);
                }

                waitForm.Close();
            }

            // Vérifier si un client s'est connecté
            if (!clientConnected)
            {
                MessageBox.Show("Aucun joueur ne s'est connecté.", "Annulé",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                try { server.Stop(); } catch { }
                return;
            }

            // Créer et lancer la partie
            var allDecks = Jeu.AvoirDeckDispo();
            var deckJ1 = new List<Carte>(allDecks[save.Joueur1.IndexDeck]);
            var deckJ2 = new List<Carte>(allDecks[save.Joueur2.IndexDeck]);

            var mainForm = new FPrincipal(deckJ1, deckJ2, save.Joueur1.IndexDeck, save.Joueur2.IndexDeck);
            mainForm.ChargerDepuisSauvegarde(save);
            mainForm.ConfigurerModeReseau(isHost: true, localPlayerIndex: 0, server: server, client: null);

            Application.Run(mainForm);
        }

        // Lance une partie chargée en tant que client (se connecte à un hôte)
        private static void LancerPartieChargeeClient(string adresse, int port)
        {
            var client = new Client();
            var mre = new ManualResetEventSlim(false);
            GameStateDto gameState = null;

            // Attendre de recevoir l'état de la partie du serveur
            client.MessageReceived += (msg) =>
            {
                if (msg == null) return;

                if (msg.Type == MessageType.GameState)
                {
                    try
                    {
                        gameState = JsonConvert.DeserializeObject<GameStateDto>(msg.Payload);
                        System.Diagnostics.Debug.WriteLine($"[Client-Save] État reçu : {gameState?.SaveData?.SaveName}");
                        mre.Set();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Client-Save] Erreur parsing:  {ex.Message}");
                    }
                }
            };

            try
            {
                // Afficher fenêtre de connexion
                using (var waitForm = new Form
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Width = 350,
                    Height = 100,
                    Text = "Connexion.. .",
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var lbl = new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Connexion à {adresse}:{port}...",
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                    };
                    waitForm.Controls.Add(lbl);
                    waitForm.Show();
                    Application.DoEvents();

                    // Se connecter au serveur
                    client.ConnectAsync(adresse, port).GetAwaiter().GetResult();

                    // Envoyer un message pour signaler qu'on veut charger une sauvegarde
                    var request = new NetMessage
                    {
                        Type = MessageType.DeckChoice,
                        Payload = JsonConvert.SerializeObject(new DeckChoiceDto
                        {
                            DeckIndex = -1,  // -1 indique qu'on veut charger une sauvegarde
                            PlayerName = "Client"
                        })
                    };
                    client.SendAsync(request).GetAwaiter().GetResult();

                    waitForm.Close();
                }

                // Attendre la réponse du serveur (max 30 secondes)
                bool received = mre.Wait(TimeSpan.FromSeconds(30));

                if (!received || gameState?.SaveData == null)
                {
                    MessageBox.Show("Impossible de recevoir l'état de la partie du serveur.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    try { client.Disconnect(); } catch { }
                    return;
                }

                // Créer et lancer la partie avec l'état reçu
                var save = gameState.SaveData;
                var allDecks = Jeu.AvoirDeckDispo();
                var deckJ1 = new List<Carte>(allDecks[save.Joueur1.IndexDeck]);
                var deckJ2 = new List<Carte>(allDecks[save.Joueur2.IndexDeck]);

                var mainForm = new FPrincipal(deckJ1, deckJ2, save.Joueur1.IndexDeck, save.Joueur2.IndexDeck);
                mainForm.ChargerDepuisSauvegarde(save);
                mainForm.ConfigurerModeReseau(isHost: false, localPlayerIndex: 1, server: null, client: client);

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de connexion : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { client.Disconnect(); } catch { }
            }
        }

        #endregion

        #region Partie normale (nouvelle partie, sans sauvegarde)

        // Lance une nouvelle partie en tant qu'hôte
        private static void LancerPartieHost(FormDeck formDeck)
        {
            var server = new Server();
            var mre = new ManualResetEventSlim(false);
            StartGameDto startDtoToSend = null;
            int clientDeckIndex = -1;

            server.MessageReceived += async (msg) =>
            {
                if (msg == null || msg.Type != MessageType.DeckChoice) return;

                try
                {
                    var deckChoice = JsonConvert.DeserializeObject<DeckChoiceDto>(msg.Payload);
                    clientDeckIndex = deckChoice?.DeckIndex ?? -1;

                    // Si le client demande une sauvegarde (DeckIndex = -1), ignorer ici
                    // (ce cas est géré par LancerPartieChargeeHost)
                    if (clientDeckIndex < 0) return;

                    var allDecks = Jeu.AvoirDeckDispo();
                    var hostDeckList = new List<Carte>(allDecks[formDeck.LocalDeckIndex]);
                    var clientDeckList = new List<Carte>(allDecks[clientDeckIndex]);
                    var hostJeu = new Jeu(hostDeckList, clientDeckList, formDeck.LocalDeckIndex, clientDeckIndex);

                    int starting = new Random().Next(2);

                    // Ce DTO contient toutes les informations nécessaires pour que
                    // le client puisse démarrer avec le même état que l'hôte
                    startDtoToSend = new StartGameDto
                    {
                        HostDeckIndex = formDeck.LocalDeckIndex,
                        ClientDeckIndex = clientDeckIndex,
                        Seed = Environment.TickCount,
                        StartingPlayerIndex = starting,
                        HostMain = DtoMapper.ToDtoList(hostJeu.Joueur1.Main),
                        HostDeck = DtoMapper.ToDtoList(hostJeu.Joueur1.Deck),
                        ClientMain = DtoMapper.ToDtoList(hostJeu.Joueur2.Main),
                        ClientDeck = DtoMapper.ToDtoList(hostJeu.Joueur2.Deck)
                    };

                    var reply = new NetMessage
                    {
                        Type = MessageType.StartGame,
                        Payload = JsonConvert.SerializeObject(startDtoToSend)
                    };
                    await server.SendAsync(reply).ConfigureAwait(false);

                    mre.Set();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[Host] Error handling DeckChoice: " + ex.Message);
                }
            };

            server.Start(formDeck.HostPort);

            using (var waitForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Width = 420,
                Height = 120,
                Text = "Attente client..."
            })
            {
                var lbl = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"En attente du client sur 127.0.0.1:{formDeck.HostPort}…",
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                };
                waitForm.Controls.Add(lbl);
                waitForm.Show();

                var timeout = TimeSpan.FromMinutes(5);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (!mre.IsSet && sw.Elapsed < timeout)
                {
                    Application.DoEvents();
                    Thread.Sleep(50);
                }

                waitForm.Close();
            }

            if (!mre.IsSet || startDtoToSend == null || clientDeckIndex < 0)
            {
                MessageBox.Show("Timeout : aucun client n'a envoyé son choix de deck.",
                    "Hôte", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                try { server.Stop(); } catch { }
                return;
            }

            var decksAll = Jeu.AvoirDeckDispo();
            var hostDeck = new List<Carte>(decksAll[formDeck.LocalDeckIndex]);
            var clientDeck = new List<Carte>(decksAll[clientDeckIndex]);

            var mainForm = new FPrincipal(hostDeck, clientDeck, formDeck.LocalDeckIndex, clientDeckIndex);

            mainForm.ApplyStartGameDto(startDtoToSend, isHost: true);
            mainForm.SuppressInitialGameStateOnConnect = true;
            mainForm.UseExistingServer(server);

            Application.Run(mainForm);
        }

        // Lance une nouvelle partie en tant que client
        private static void LancerPartieClient(FormDeck formDeck)
        {
            var client = new Client();
            var mre = new ManualResetEventSlim(false);
            StartGameDto startDto = null;

            client.MessageReceived += (msg) =>
            {
                if (msg == null || msg.Type != MessageType.StartGame) return;
                try
                {
                    startDto = JsonConvert.DeserializeObject<StartGameDto>(msg.Payload);
                    mre.Set();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[Client] Error parsing StartGame: " + ex.Message);
                }
            };

            try
            {
                client.ConnectAsync(formDeck.HostAddress, formDeck.HostPort).GetAwaiter().GetResult();

                var choice = new DeckChoiceDto
                {
                    DeckIndex = formDeck.LocalDeckIndex,
                    PlayerName = "Client"
                };
                var msg = new NetMessage
                {
                    Type = MessageType.DeckChoice,
                    Payload = JsonConvert.SerializeObject(choice)
                };
                client.SendAsync(msg).GetAwaiter().GetResult();

                bool ok = mre.Wait(TimeSpan.FromMinutes(1));

                if (!ok || startDto == null)
                {
                    MessageBox.Show("Aucun message de démarrage reçu du serveur.",
                        "Client", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try { client.Disconnect(); } catch { }
                    return;
                }

                var hostDeck = DtoMapper.FromDtoList(startDto.HostDeck);
                var clientDeck = DtoMapper.FromDtoList(startDto.ClientDeck);
                var mainForm = new FPrincipal(hostDeck, clientDeck, startDto.HostDeckIndex, startDto.ClientDeckIndex);

                mainForm.ApplyStartGameDto(new StartGameDto
                {
                    HostDeckIndex = startDto.HostDeckIndex,
                    ClientDeckIndex = startDto.ClientDeckIndex,
                    Seed = startDto.Seed,
                    StartingPlayerIndex = startDto.StartingPlayerIndex,
                    HostMain = startDto.HostMain,
                    ClientMain = startDto.ClientMain
                }, isHost: false);
                mainForm.UseExistingClient(client);

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur réseau : {ex.Message}",
                    "Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { client.Disconnect(); } catch { }
            }
        }

        #endregion
    }
}