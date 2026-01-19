using Gwent.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace Gwent
{
    public partial class FPrincipal : Form
    {
        #region Champs

        // Logique métier
        private PartieGwent _partie;
        private GestionnairePouvoirs _gestionnairePouvoirs;
        private GestionnaireUI _gestionnaireUI;

        // Contrôles UI
        private PlateauJoueurControl _plateauControlJ1;
        private PlateauJoueurControl _plateauControlJ2;
        private Panel _overlayAttente;
        private Button _boutonAide;

        // Zones de main séparées
        private FlowLayoutPanel _zoneMainJ1;
        private FlowLayoutPanel _zoneMainJ2;

        // État de sélection
        private Carte _carteSelectionnee;
        private PictureBox _pbSelectionnee;
        private bool _modeLeurre;
        private Carte _carteLeurre;
        private FlowLayoutPanel _zoneLeurre;

        // Réseau
        private Server _server;
        private Client _client;
        private bool _isNetworkGame;
        private bool _isHostInstance;
        private int _localPlayerIndex;

        // Autres
        private SoundPlayer _player;
        private bool _enPleinEcran = true;
        private List<Carte> _deckInitialJ1;
        private List<Carte> _deckInitialJ2;
        private string _dosCarteJ1;
        private string _dosCarteJ2;
        private ToolTip _toolTip;

        // Sauvegarde
        private Button _boutonSauvegarder;
        private string _hostAddress = " 172.20.10.2";
        private int _hostPort = 12345;

        public bool SuppressInitialGameStateOnConnect { get; set; } = false;

        #endregion

        #region Constructeur

        public FPrincipal(List<Carte> deckJ1, List<Carte> deckJ2, int indexDeckJ1, int indexDeckJ2)
        {
            InitializeComponent();

            _toolTip = new ToolTip();

            // Initialiser la partie
            _partie = new PartieGwent(deckJ1, deckJ2, indexDeckJ1, indexDeckJ2);

            // Sauvegarder les decks initiaux pour l'aperçu
            _deckInitialJ1 = new List<Carte>(deckJ1);
            _deckInitialJ2 = new List<Carte>(deckJ2);

            // Dos de cartes
            _dosCarteJ1 = FormDeck.DosCartesDecks[indexDeckJ1];
            _dosCarteJ2 = FormDeck.DosCartesDecks[indexDeckJ2];

            // Initialiser les gestionnaires
            InitialiserGestionnaires();

            // Initialiser l'UI
            InitialiserUI();

            // S'abonner aux événements de la partie
            AbonnerEvenements();

            // Configurer la fenêtre
            ConfigurerFenetre();

            // Charger la musique
            ChargerMusique();

            // Gérer le pouvoir Scoia'Tael (choix du premier joueur)
            GererPouvoirScoiaTael();

            // Affichage initial
            ForceRechargerMains();
            RafraichirTout();
        }

        #endregion

        #region Initialisation

        private void InitialiserGestionnaires()
        {
            _gestionnairePouvoirs = new GestionnairePouvoirs(
                _partie,
                AfficherMessage,
                ChoisirCarteCimetiere
            );

            _gestionnaireUI = new GestionnaireUI(_partie);
        }

        private void InitialiserUI()
        {
            this.SuspendLayout();

            // Layout principal :  3 lignes (Main J2, Plateau central, Main J1)
            var layoutPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(60, 40, 25),
                Margin = new Padding(0),
                Padding = new Padding(5)
            };

            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));  // Main J2
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Plateau central
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));  // Main J1

            // Créer les contrôles de plateau
            _plateauControlJ2 = new PlateauJoueurControl();
            _plateauControlJ2.Initialiser(_partie.Plateau2, 1, estInverse: true);
            _plateauControlJ2.CarteClicked += OnCarteClicked;
            _plateauControlJ2.ZoneClicked += OnZoneClicked;
            _plateauControlJ2.PasserClicked += OnPasserClicked;
            _plateauControlJ2.PouvoirClicked += OnPouvoirClicked;
            _plateauControlJ2.ApercuClicked += OnApercuClicked;

            _plateauControlJ1 = new PlateauJoueurControl();
            _plateauControlJ1.Initialiser(_partie.Plateau1, 0, estInverse: false);
            _plateauControlJ1.CarteClicked += OnCarteClicked;
            _plateauControlJ1.ZoneClicked += OnZoneClicked;
            _plateauControlJ1.PasserClicked += OnPasserClicked;
            _plateauControlJ1.PouvoirClicked += OnPouvoirClicked;
            _plateauControlJ1.ApercuClicked += OnApercuClicked;

            // Charger les images
            _plateauControlJ1.ChargerImages(null);
            _plateauControlJ2.ChargerImages(null);

            // Créer les zones de main
            var panelMainJ2 = CreerPanelMain(true);
            var panelMainJ1 = CreerPanelMain(false);

            // Créer le plateau central
            var plateauCentral = CreerPlateauCentral();

            // Ajouter au layout
            layoutPrincipal.Controls.Add(panelMainJ2, 0, 0);
            layoutPrincipal.Controls.Add(plateauCentral, 0, 1);
            layoutPrincipal.Controls.Add(panelMainJ1, 0, 2);

            // Overlay d'attente
            _overlayAttente = CreerOverlayAttente();

            this.Controls.Add(layoutPrincipal);
            this.Controls.Add(_overlayAttente);
            _overlayAttente.BringToFront();

            this.ResumeLayout();
        }

        private Panel CreerPanelMain(bool estAdversaire)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 25, 15),
                Padding = new Padding(10, 5, 10, 5)
            };

            var titre = new Label
            {
                Text = estAdversaire ? "Main Adversaire" : "Votre Main",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(45, 32, 20),
                Name = estAdversaire ? "zoneMainJ2" : "zoneMainJ1"  // Ajout d'un nom pour le debug
            };

            if (estAdversaire)
            {
                _zoneMainJ2 = flow;
                _partie.Plateau2.ZoneMain = flow;
                System.Diagnostics.Debug.WriteLine("[CreerPanelMain] _zoneMainJ2 créé");
            }
            else
            {
                _zoneMainJ1 = flow;
                _partie.Plateau1.ZoneMain = flow;
                System.Diagnostics.Debug.WriteLine("[CreerPanelMain] _zoneMainJ1 créé");
            }

            panel.Controls.Add(flow);
            panel.Controls.Add(titre);
            return panel;
        }

        private Panel CreerPlateauCentral()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Layout :  3 colonnes (Info gauche | Zones | Info droite)
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));  // Info J2
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // Zones
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));  // Info J1

            // Panels latéraux
            var panelInfoJ2 = CreerPanelInfoLateral(_partie.Plateau2, _plateauControlJ2, "Joueur 2");
            var panelInfoJ1 = CreerPanelInfoLateral(_partie.Plateau1, _plateauControlJ1, "Joueur 1");

            // Zones centrales
            var zonesPanel = CreerZonesCentrales();

            layout.Controls.Add(panelInfoJ2, 0, 0);
            layout.Controls.Add(zonesPanel, 1, 0);
            layout.Controls.Add(panelInfoJ1, 2, 0);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreerPanelInfoLateral(PlateauJoueur plateau, PlateauJoueurControl control, string nomJoueur)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 35, 20),
                Padding = new Padding(8)
            };

            // Nom du joueur
            var lblNom = new Label
            {
                Text = nomJoueur,
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Panel vies
            var panelVies = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.Transparent
            };

            // Retirer les vies de leur parent actuel
            if (control.PbVie1.Parent != null) control.PbVie1.Parent.Controls.Remove(control.PbVie1);
            if (control.PbVie2.Parent != null) control.PbVie2.Parent.Controls.Remove(control.PbVie2);

            control.PbVie1.Size = new Size(28, 28);
            control.PbVie2.Size = new Size(28, 28);
            control.PbVie1.Location = new Point(25, 3);
            control.PbVie2.Location = new Point(58, 3);

            panelVies.Controls.Add(control.PbVie1);
            panelVies.Controls.Add(control.PbVie2);

            // Boutons
            if (control.BoutonPasser.Parent != null) control.BoutonPasser.Parent.Controls.Remove(control.BoutonPasser);
            if (control.BoutonPouvoir.Parent != null) control.BoutonPouvoir.Parent.Controls.Remove(control.BoutonPouvoir);
            if (control.BoutonApercu.Parent != null) control.BoutonApercu.Parent.Controls.Remove(control.BoutonApercu);

            // Panel boutons
            var panelBoutons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            var layoutBoutons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            layoutBoutons.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layoutBoutons.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layoutBoutons.RowStyles.Add(new RowStyle(SizeType.Percent, 33));

            control.BoutonPasser.Dock = DockStyle.Fill;
            control.BoutonPouvoir.Dock = DockStyle.Fill;
            control.BoutonApercu.Dock = DockStyle.Fill;

            layoutBoutons.Controls.Add(control.BoutonPasser, 0, 0);
            layoutBoutons.Controls.Add(control.BoutonPouvoir, 0, 1);
            layoutBoutons.Controls.Add(control.BoutonApercu, 0, 2);

            panelBoutons.Controls.Add(layoutBoutons);

            // Score
            var panelScore = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var lblScoreTitre = new Label
            {
                Text = "SCORE",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (control.LabelScoreTotal.Parent != null)
                control.LabelScoreTotal.Parent.Controls.Remove(control.LabelScoreTotal);

            control.LabelScoreTotal.Dock = DockStyle.Top;
            control.LabelScoreTotal.Height = 50;
            control.LabelScoreTotal.TextAlign = ContentAlignment.MiddleCenter;
            control.LabelScoreTotal.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            control.LabelScoreTotal.ForeColor = Color.Gold;

            // Pioche / Cimetière
            var panelPiocheCim = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.Transparent
            };

            if (control.LabelPioche.Parent != null) control.LabelPioche.Parent.Controls.Remove(control.LabelPioche);
            if (control.LabelCimetiere.Parent != null) control.LabelCimetiere.Parent.Controls.Remove(control.LabelCimetiere);

            var lblPioche = new Label
            {
                Text = "Pioche:",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8),
                Location = new Point(5, 5),
                AutoSize = true
            };

            control.LabelPioche.Location = new Point(55, 5);
            control.LabelPioche.AutoSize = true;
            control.LabelPioche.ForeColor = Color.White;
            control.LabelPioche.Font = new Font("Segoe UI", 8, FontStyle.Bold);

            var lblCim = new Label
            {
                Text = "Cimetière:",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8),
                Location = new Point(5, 22),
                AutoSize = true
            };

            control.LabelCimetiere.Location = new Point(55, 22);
            control.LabelCimetiere.AutoSize = true;
            control.LabelCimetiere.ForeColor = Color.White;
            control.LabelCimetiere.Font = new Font("Segoe UI", 8, FontStyle.Bold);

            panelPiocheCim.Controls.Add(lblPioche);
            panelPiocheCim.Controls.Add(control.LabelPioche);
            panelPiocheCim.Controls.Add(lblCim);
            panelPiocheCim.Controls.Add(control.LabelCimetiere);

            panelScore.Controls.Add(control.LabelScoreTotal);
            panelScore.Controls.Add(lblScoreTitre);

            // Ajouter dans l'ordre (de bas en haut pour Dock. Top)
            panel.Controls.Add(panelScore);
            panel.Controls.Add(panelBoutons);
            panel.Controls.Add(panelVies);
            panel.Controls.Add(lblNom);
            panel.Controls.Add(panelPiocheCim);

            return panel;
        }

        private Panel CreerZonesCentrales()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(2)
            };

            // Répartition des lignes
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // Siège J2
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // Distance J2
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // Mêlée J2
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Séparateur
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // Mêlée J1
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // Distance J1
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // Siège J1

            // Créer les lignes
            var ligneSiegeJ2 = CreerLigneZone(_plateauControlJ2, "Siege");
            var ligneDistanceJ2 = CreerLigneZone(_plateauControlJ2, "Distance");
            var ligneMeleeJ2 = CreerLigneZone(_plateauControlJ2, "Melee");

            var separateur = CreerSeparateurCentral();

            var ligneMeleeJ1 = CreerLigneZone(_plateauControlJ1, "Melee");
            var ligneDistanceJ1 = CreerLigneZone(_plateauControlJ1, "Distance");
            var ligneSiegeJ1 = CreerLigneZone(_plateauControlJ1, "Siege");

            layout.Controls.Add(ligneSiegeJ2, 0, 0);
            layout.Controls.Add(ligneDistanceJ2, 0, 1);
            layout.Controls.Add(ligneMeleeJ2, 0, 2);
            layout.Controls.Add(separateur, 0, 3);
            layout.Controls.Add(ligneMeleeJ1, 0, 4);
            layout.Controls.Add(ligneDistanceJ1, 0, 5);
            layout.Controls.Add(ligneSiegeJ1, 0, 6);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreerLigneZone(PlateauJoueurControl plateauControl, string typeZone)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // Colonnes :  Effet (50) | Zone principale (stretch) | Météo (50) | Score (55)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));

            FlowLayoutPanelSurbrillance zoneEffet;
            FlowLayoutPanelSurbrillance zonePrincipale;
            FlowLayoutPanelSurbrillance zoneMeteo;
            Label labelScore;

            switch (typeZone)
            {
                case "Melee":
                    zoneEffet = plateauControl.ZoneEffetMelee;
                    zonePrincipale = plateauControl.ZoneMelee;
                    zoneMeteo = plateauControl.ZoneMeteoMelee;
                    labelScore = plateauControl.LabelScoreMelee;
                    break;
                case "Distance":
                    zoneEffet = plateauControl.ZoneEffetDistance;
                    zonePrincipale = plateauControl.ZoneDistance;
                    zoneMeteo = plateauControl.ZoneMeteoDistance;
                    labelScore = plateauControl.LabelScoreDistance;
                    break;
                case "Siege":
                    zoneEffet = plateauControl.ZoneEffetSiege;
                    zonePrincipale = plateauControl.ZoneSiege;
                    zoneMeteo = plateauControl.ZoneMeteoSiege;
                    labelScore = plateauControl.LabelScoreSiege;
                    break;
                default:
                    return panel;
            }

            // Retirer des parents actuels
            if (zoneEffet.Parent != null) zoneEffet.Parent.Controls.Remove(zoneEffet);
            if (zonePrincipale.Parent != null) zonePrincipale.Parent.Controls.Remove(zonePrincipale);
            if (zoneMeteo.Parent != null) zoneMeteo.Parent.Controls.Remove(zoneMeteo);
            if (labelScore.Parent != null) labelScore.Parent.Controls.Remove(labelScore);

            // Configurer les zones
            zoneEffet.Dock = DockStyle.Fill;
            zoneEffet.Margin = new Padding(1);
            zoneEffet.BackColor = Color.FromArgb(60, 45, 30);

            zonePrincipale.Dock = DockStyle.Fill;
            zonePrincipale.Margin = new Padding(1);

            zoneMeteo.Dock = DockStyle.Fill;
            zoneMeteo.Margin = new Padding(1);
            zoneMeteo.BackColor = Color.FromArgb(60, 45, 30);

            labelScore.Dock = DockStyle.Fill;
            labelScore.TextAlign = ContentAlignment.MiddleCenter;
            labelScore.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            labelScore.ForeColor = Color.White;
            labelScore.BackColor = Color.FromArgb(45, 32, 20);

            layout.Controls.Add(zoneEffet, 0, 0);
            layout.Controls.Add(zonePrincipale, 1, 0);
            layout.Controls.Add(zoneMeteo, 2, 0);
            layout.Controls.Add(labelScore, 3, 0);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreerSeparateurCentral()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(80, 55, 30)
            };

            // Bouton aide
            _boutonAide = new Button
            {
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Location = new Point(10, 2)
            };
            _boutonAide.FlatAppearance.BorderSize = 0;

            string imageHelp = Path.Combine(Application.StartupPath, "Images", "help.png");
            if (File.Exists(imageHelp))
            {
                _boutonAide.BackgroundImage = ImageHelper.ChargerImage(imageHelp);
                _boutonAide.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                _boutonAide.Text = "?";
                _boutonAide.ForeColor = Color.White;
                _boutonAide.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }
            _boutonAide.Click += BoutonAide_Click;

            // Bouton sauvegarder
            _boutonSauvegarder = new Button
            {
                Text = "💾 Sauvegarder",
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 120, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _boutonSauvegarder.FlatAppearance.BorderSize = 1;
            _boutonSauvegarder.Click += BoutonSauvegarder_Click;

            // Centrer les boutons
            panel.Resize += (s, e) =>
            {
                _boutonAide.Location = new Point(
                    (panel.Width / 2) - _boutonAide.Width - 10,
                    (panel.Height - _boutonAide.Height) / 2
                );
                _boutonSauvegarder.Location = new Point(
                    (panel.Width / 2) + 10,
                    (panel.Height - _boutonSauvegarder.Height) / 2
                );
            };

            panel.Controls.Add(_boutonAide);
            panel.Controls.Add(_boutonSauvegarder);
            return panel;
        }

        private async void BoutonSauvegarder_Click(object sender, EventArgs e)
        {
            // Demander un nom pour la sauvegarde
            string nomSauvegarde = null;

            using (var inputForm = new Form())
            {
                inputForm.Text = "Sauvegarder la partie";
                inputForm.Size = new Size(400, 150);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var lblNom = new Label
                {
                    Text = "Nom de la sauvegarde :",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                var txtNom = new TextBox
                {
                    Text = $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}",
                    Location = new Point(20, 45),
                    Size = new Size(340, 25)
                };

                var btnOk = new Button
                {
                    Text = "Sauvegarder",
                    DialogResult = DialogResult.OK,
                    Location = new Point(180, 80),
                    Size = new Size(100, 30)
                };

                var btnAnnuler = new Button
                {
                    Text = "Annuler",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(290, 80),
                    Size = new Size(80, 30)
                };

                inputForm.Controls.Add(lblNom);
                inputForm.Controls.Add(txtNom);
                inputForm.Controls.Add(btnOk);
                inputForm.Controls.Add(btnAnnuler);
                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnAnnuler;

                if (inputForm.ShowDialog(this) == DialogResult.OK)
                {
                    nomSauvegarde = txtNom.Text.Trim();
                    if (string.IsNullOrEmpty(nomSauvegarde))
                    {
                        nomSauvegarde = $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}";
                    }
                }
                else
                {
                    return; // Annulé
                }
            }

            // Construire le DTO de sauvegarde (pour écriture locale + envoi réseau éventuel)
            var saveDto = GameSaveManager.ConstruireDtoSauvegarde(
                _partie,
                _plateauControlJ1,
                _plateauControlJ2,
                _isNetworkGame,
                _localPlayerIndex,
                _hostAddress,
                _hostPort,
                nomSauvegarde
            );

            bool success = GameSaveManager.EcrireSauvegarde(saveDto);

            if (success)
            {
                MessageBox.Show($"Partie sauvegardée avec succès !\n\nNom :  {nomSauvegarde}",
                    "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Si en réseau : envoyer le même DTO au pair pour qu'il crée la sauvegarde localement
                if (_isNetworkGame)
                {
                    await EnvoyerSaveGameAsync(saveDto);
                }
            }
            else
            {
                MessageBox.Show("Erreur lors de la sauvegarde de la partie.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task EnvoyerSaveGameAsync(GameSaveDto save)
        {
            var msg = new NetMessage
            {
                Type = MessageType.SaveGame,
                Payload = JsonConvert.SerializeObject(save)
            };

            System.Diagnostics.Debug.WriteLine($"[EnvoyerSaveGameAsync] Envoi sauvegarde '{save.SaveName}'");

            try
            {
                if (_server != null) await _server.SendAsync(msg);
                else if (_client != null) await _client.SendAsync(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EnvoyerSaveGameAsync] Erreur:  {ex.Message}");
            }
        }

        // Charge l'état d'une partie sauvegardée
        public void ChargerDepuisSauvegarde(GameSaveDto save)
        {
            if (save == null) return;

            try
            {
                // Restaurer l'état de la partie
                _partie.IndexJoueurCourant = save.IndexJoueurCourant;

                // Restaurer Joueur 1
                RestaurerJoueur(_partie.Plateau1, _plateauControlJ1, _partie.Jeu.Joueur1, save.Joueur1);

                // Restaurer Joueur 2
                RestaurerJoueur(_partie.Plateau2, _plateauControlJ2, _partie.Jeu.Joueur2, save.Joueur2);

                // Restaurer les infos réseau
                _isNetworkGame = save.EstPartieReseau;
                _localPlayerIndex = save.LocalPlayerIndex;
                _hostAddress = save.HostAddress;
                _hostPort = save.HostPort;

                // Rafraîchir l'affichage
                ForceRechargerMains();
                RafraichirTout();

                System.Diagnostics.Debug.WriteLine("[ChargerDepuisSauvegarde] Partie restaurée avec succès");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChargerDepuisSauvegarde] Erreur:  {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement :  {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestaurerJoueur(PlateauJoueur plateau, PlateauJoueurControl control, Joueur joueur, PlayerSaveDto save)
        {
            // Restaurer l'état du plateau
            plateau.Vies = save.Vies;
            plateau.APasse = save.APasse;
            plateau.PouvoirUtilise = save.PouvoirUtilise;
            plateau.MeteoMeleeActive = save.MeteoMeleeActive;
            plateau.MeteoDistanceActive = save.MeteoDistanceActive;
            plateau.MeteoSiegeActive = save.MeteoSiegeActive;
            plateau.ChargeMeleeActive = save.ChargeMeleeActive;
            plateau.ChargeDistanceActive = save.ChargeDistanceActive;
            plateau.ChargeSiegeActive = save.ChargeSiegeActive;

            // Restaurer les cartes du joueur
            joueur.Main.Clear();
            joueur.Main.AddRange(DtoMapper.FromDtoList(save.Main));

            joueur.Deck.Clear();
            joueur.Deck.AddRange(DtoMapper.FromDtoList(save.Deck));

            joueur.Cimetiere.Clear();
            joueur.Cimetiere.AddRange(DtoMapper.FromDtoList(save.Cimetiere));

            // Vider et restaurer les zones du plateau
            RestaurerZone(control.ZoneMelee, save.ZoneMelee, control);
            RestaurerZone(control.ZoneDistance, save.ZoneDistance, control);
            RestaurerZone(control.ZoneSiege, save.ZoneSiege, control);

            RestaurerZone(control.ZoneEffetMelee, save.ZoneEffetMelee, control);
            RestaurerZone(control.ZoneEffetDistance, save.ZoneEffetDistance, control);
            RestaurerZone(control.ZoneEffetSiege, save.ZoneEffetSiege, control);

            RestaurerZone(control.ZoneMeteoMelee, save.ZoneMeteoMelee, control);
            RestaurerZone(control.ZoneMeteoDistance, save.ZoneMeteoDistance, control);
            RestaurerZone(control.ZoneMeteoSiege, save.ZoneMeteoSiege, control);
        }

        private void RestaurerZone(FlowLayoutPanel zone, List<CardDto> cartes, PlateauJoueurControl control)
        {
            if (zone == null) return;

            zone.Controls.Clear();

            if (cartes == null) return;

            foreach (var cardDto in cartes)
            {
                var carte = DtoMapper.FromDto(cardDto);
                control.AjouterCarteZone(zone, carte);
            }
        }

        private Panel CreerOverlayAttente()
        {
            var overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(140, 0, 0, 0),
                Visible = false
            };

            var label = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold),
                Text = "En attente de l'adversaire..."
            };

            overlay.Controls.Add(label);
            return overlay;
        }

        private void GererSaveGameRecu(NetMessage msg)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<GameSaveDto>(msg.Payload);
                if (dto == null)
                {
                    System.Diagnostics.Debug.WriteLine("[GererSaveGameRecu] DTO null");
                    return;
                }

                // Adapter le point de vue local pour cette machine (facultatif mais plus cohérent)
                dto.LocalPlayerIndex = _localPlayerIndex;
                dto.EstPartieReseau = true;

                bool ok = GameSaveManager.EcrireSauvegarde(dto);

                System.Diagnostics.Debug.WriteLine(ok
                    ? $"[GererSaveGameRecu] Sauvegarde écrite: {dto.SaveName}"
                    : "[GererSaveGameRecu] Échec écriture sauvegarde");

                // Feedback discret à l'utilisateur
                if (ok)
                {
                    MessageBox.Show($"Sauvegarde reçue et créée localement : {dto.SaveName}",
                        "Sauvegarde réseau", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GererSaveGameRecu] Erreur: {ex.Message}");
            }
        }

        private void AbonnerEvenements()
        {
            _partie.Message += AfficherMessage;
            _partie.TourChange += OnTourChange;
            _partie.MancheTerminee += OnMancheTerminee;
            _partie.PartieGagnee += OnPartieGagnee;
            _partie.EtatChange += OnEtatChange;
        }

        private void ConfigurerFenetre()
        {
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += FPrincipal_KeyDown;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void ChargerMusique()
        {
            try
            {
                string cheminMusique = Path.Combine(Application.StartupPath, "Musique", "gwent.wav");
                if (File.Exists(cheminMusique))
                {
                    _player = new SoundPlayer(cheminMusique);
                    _player.PlayLooping();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur musique : {ex.Message}");
            }
        }

        private void GererPouvoirScoiaTael()
        {
            bool j1ScoiaTel = _partie.Jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel;
            bool j2ScoiaTel = _partie.Jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel;

            if (j1ScoiaTel && !j2ScoiaTel)
            {
                var result = MessageBox.Show("Voulez-vous que Joueur 1 commence ?", "Pouvoir Scoia'Tael",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                _partie.DefinirPremierJoueur(result == DialogResult.Yes ? 0 : 1);
            }
            else if (j2ScoiaTel && !j1ScoiaTel)
            {
                var result = MessageBox.Show("Voulez-vous que Joueur 2 commence ?", "Pouvoir Scoia'Tael",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                _partie.DefinirPremierJoueur(result == DialogResult.Yes ? 1 : 0);
            }

            AfficherMessage($"{_partie.JoueurCourant.Nom} commence !");
        }






        #endregion

        #region Rafraîchissement UI

        private void RafraichirTout()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RafraichirTout));
                return;
            }

            // Mettre à jour les scores et compteurs
            _plateauControlJ1.MettreAJourAffichage();
            _plateauControlJ2.MettreAJourAffichage();

            // Activer les contrôles selon le tour
            ActiverControles();

            // Gérer l'overlay réseau
            GererOverlayReseau();
        }

        private void ForceRechargerMains()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ForceRechargerMains));
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[ForceRechargerMains] _isNetworkGame={_isNetworkGame}, _localPlayerIndex={_localPlayerIndex}, IndexJoueurCourant={_partie.IndexJoueurCourant}");

            bool masquerJ1;
            bool masquerJ2;

            if (_isNetworkGame)
            {
                // Mode réseau:  Je vois MA main, l'adversaire voit la sienne
                // Si je suis J1 (localPlayerIndex=0) -> je vois J1, je masque J2
                // Si je suis J2 (localPlayerIndex=1) -> je masque J1, je vois J2
                masquerJ1 = (_localPlayerIndex != 0);
                masquerJ2 = (_localPlayerIndex != 1);

                System.Diagnostics.Debug.WriteLine($"[ForceRechargerMains] Mode RESEAU: masquerJ1={masquerJ1}, masquerJ2={masquerJ2}");
            }
            else
            {
                // Mode local: Le joueur courant voit sa main, l'autre est masquée
                masquerJ1 = (_partie.IndexJoueurCourant != 0);
                masquerJ2 = (_partie.IndexJoueurCourant != 1);

                System.Diagnostics.Debug.WriteLine($"[ForceRechargerMains] Mode LOCAL: masquerJ1={masquerJ1}, masquerJ2={masquerJ2}");
            }

            ChargerMainDansZone(_zoneMainJ1, _partie.Plateau1.Joueur.Main, _dosCarteJ1, masquerJ1, _plateauControlJ1);
            ChargerMainDansZone(_zoneMainJ2, _partie.Plateau2.Joueur.Main, _dosCarteJ2, masquerJ2, _plateauControlJ2);
        }

        private void ChargerMainDansZone(FlowLayoutPanel zone, List<Carte> main, string dosCartePath, bool masquer, PlateauJoueurControl plateauControl)
        {
            if (zone == null)
            {
                System.Diagnostics.Debug.WriteLine("[ChargerMainDansZone] ERREUR: zone est null!");
                return;
            }

            if (main == null)
            {
                System.Diagnostics.Debug.WriteLine("[ChargerMainDansZone] ERREUR:  main est null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[ChargerMainDansZone] Zone={zone.Name}, Cartes={main.Count}, Masquer={masquer}");

            zone.Controls.Clear();

            foreach (var carte in main)
            {
                var pb = new PictureBox
                {
                    Width = 65,
                    Height = 100,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Tag = carte,
                    Margin = new Padding(3)
                };

                if (masquer)
                {
                    // Carte masquée (dos visible)
                    if (!string.IsNullOrEmpty(dosCartePath) && File.Exists(dosCartePath))
                    {
                        ImageHelper.AppliquerImage(pb, dosCartePath);
                    }
                    else
                    {
                        pb.BackColor = Color.DarkBlue;
                    }
                    pb.Cursor = Cursors.Default;
                    pb.Enabled = false;
                    _toolTip.SetToolTip(pb, "Carte cachée");

                    System.Diagnostics.Debug.WriteLine($"[ChargerMainDansZone] Carte MASQUÉE: {carte.Nom}");
                }
                else
                {
                    // Carte visible et cliquable
                    ImageHelper.AppliquerImage(pb, carte.ImagePath);
                    pb.Cursor = Cursors.Hand;
                    pb.Enabled = true;
                    _toolTip.SetToolTip(pb, $"{carte.Nom}\nPuissance: {carte.Puissance}\nPouvoir:  {carte.Pouvoir}");

                    // Capturer les variables pour le closure
                    var carteCourante = carte;
                    var pbCourant = pb;
                    var zoneSource = zone;
                    var control = plateauControl;

                    pb.Click += (s, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"[PictureBox. Click] Clic sur {carteCourante.Nom}");
                        var args = new CarteClickEventArgs(carteCourante, pbCourant, zoneSource);
                        OnCarteClicked(control, args);
                    };

                    System.Diagnostics.Debug.WriteLine($"[ChargerMainDansZone] Carte VISIBLE: {carte.Nom}");
                }

                zone.Controls.Add(pb);
            }

            System.Diagnostics.Debug.WriteLine($"[ChargerMainDansZone] Terminé - {zone.Controls.Count} contrôles ajoutés");
        }

        private void ActiverControles()
        {
            bool tourJ1 = _partie.IndexJoueurCourant == 0;

            if (_isNetworkGame)
            {
                bool estMonTour = (_partie.IndexJoueurCourant == _localPlayerIndex);
                _plateauControlJ1.ActiverControles(estMonTour && tourJ1);
                _plateauControlJ2.ActiverControles(estMonTour && !tourJ1);
            }
            else
            {
                _plateauControlJ1.ActiverControles(tourJ1);
                _plateauControlJ2.ActiverControles(!tourJ1);
            }
        }

        private void GererOverlayReseau()
        {
            if (!_isNetworkGame)
            {
                _overlayAttente.Visible = false;
                return;
            }

            bool estMonTour = (_partie.IndexJoueurCourant == _localPlayerIndex);

            System.Diagnostics.Debug.WriteLine($"[GererOverlayReseau] estMonTour={estMonTour}, IndexJoueurCourant={_partie.IndexJoueurCourant}, _localPlayerIndex={_localPlayerIndex}");

            _overlayAttente.Visible = !estMonTour;

            if (_overlayAttente.Visible)
                _overlayAttente.BringToFront();
            else
                _overlayAttente.SendToBack();
        }

        #endregion

        #region Gestionnaires d'événements UI

        private void OnCarteClicked(object sender, CarteClickEventArgs e)
        {
            var plateauControl = sender as PlateauJoueurControl;
            if (plateauControl == null) return;

            System.Diagnostics.Debug.WriteLine($"[OnCarteClicked] Carte={e.Carte.Nom}, PlateauIndex={plateauControl.IndexJoueur}");
            System.Diagnostics.Debug.WriteLine($"[OnCarteClicked] _modeLeurre={_modeLeurre}");

            // Mode Leurre : gérer le remplacement de carte sur le plateau
            if (_modeLeurre)
            {
                GererModeLeurre(e);
                return;
            }

            // Vérifier si c'est le tour du bon joueur
            if (_isNetworkGame)
            {
                if (_localPlayerIndex != _partie.IndexJoueurCourant)
                {
                    System.Diagnostics.Debug.WriteLine("[OnCarteClicked] BLOQUÉ:  Pas mon tour (réseau)");
                    return;
                }

                bool estMaMain = (_localPlayerIndex == 0 && e.ZoneSource == _zoneMainJ1) ||
                                 (_localPlayerIndex == 1 && e.ZoneSource == _zoneMainJ2);

                if (!estMaMain)
                {
                    System.Diagnostics.Debug.WriteLine("[OnCarteClicked] BLOQUÉ: Ce n'est pas ma main");
                    return;
                }
            }
            else
            {
                bool estMainJ1 = (e.ZoneSource == _zoneMainJ1);
                bool estMainJ2 = (e.ZoneSource == _zoneMainJ2);

                if (estMainJ1 && _partie.IndexJoueurCourant != 0)
                {
                    System.Diagnostics.Debug.WriteLine("[OnCarteClicked] BLOQUÉ: Pas le tour de J1 (local)");
                    return;
                }
                if (estMainJ2 && _partie.IndexJoueurCourant != 1)
                {
                    System.Diagnostics.Debug.WriteLine("[OnCarteClicked] BLOQUÉ:  Pas le tour de J2 (local)");
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine("[OnCarteClicked] AUTORISÉ:  Traitement de la carte");
            SelectionnerCarte(e);
        }

        private void SelectionnerCarte(CarteClickEventArgs e)
        {
            // Désélectionner l'ancienne carte
            if (_pbSelectionnee != null)
            {
                _pbSelectionnee.BorderStyle = BorderStyle.None;
                _pbSelectionnee.Size = new Size(65, 100);
                _pbSelectionnee.BackColor = Color.Transparent;
                _pbSelectionnee.Padding = new Padding(0);
            }

            // Si on clique sur la même carte, désélectionner
            if (_pbSelectionnee == e.PictureBox)
            {
                _carteSelectionnee = null;
                _pbSelectionnee = null;
                ResetToutesSurbrillances();
                return;
            }

            // Sélectionner la nouvelle carte
            _carteSelectionnee = e.Carte;
            _pbSelectionnee = e.PictureBox;

            _pbSelectionnee.BorderStyle = BorderStyle.FixedSingle;
            _pbSelectionnee.Size = new Size(70, 105);
            _pbSelectionnee.BackColor = Color.Yellow;
            _pbSelectionnee.Padding = new Padding(3);

            System.Diagnostics.Debug.WriteLine($"[SelectionnerCarte] Carte sélectionnée: {_carteSelectionnee.Nom}");

            // Surligner les zones valides
            SurlirlignerZonesValides();
        }

        private void SurlirlignerZonesValides()
        {
            ResetToutesSurbrillances();

            if (_carteSelectionnee == null) return;

            var plateauCourant = _partie.IndexJoueurCourant == 0 ? _plateauControlJ1 : _plateauControlJ2;
            var plateauAdverse = _partie.IndexJoueurCourant == 0 ? _plateauControlJ2 : _plateauControlJ1;

            // Cas Espion : zone adverse
            if (_carteSelectionnee.Pouvoir == PouvoirSpecial.Espion)
            {
                var zone = GetZonePrincipale(plateauAdverse, _carteSelectionnee.Type);
                if (zone != null) plateauAdverse.SurlignerZone(zone, true);
                return;
            }

            // Cas Agile
            if (_carteSelectionnee.Pouvoir == PouvoirSpecial.Agile)
            {
                plateauCourant.SurlignerZone(plateauCourant.ZoneMelee, true);
                plateauCourant.SurlignerZone(plateauCourant.ZoneDistance, true);
                return;
            }

            // Cas Leurre
            if (_carteSelectionnee.Pouvoir == PouvoirSpecial.Leurre)
            {
                plateauCourant.SurlignerZone(plateauCourant.ZoneMelee, true);
                plateauCourant.SurlignerZone(plateauCourant.ZoneDistance, true);
                plateauCourant.SurlignerZone(plateauCourant.ZoneSiege, true);
                return;
            }

            // Cas Météo
            if (_carteSelectionnee.Type == TypeCarte.Meteo)
            {
                switch (_carteSelectionnee.Pouvoir)
                {
                    case PouvoirSpecial.Gel:
                        plateauAdverse.SurlignerZone(plateauAdverse.ZoneMeteoMelee, true);
                        break;
                    case PouvoirSpecial.Brouillard:
                        plateauAdverse.SurlignerZone(plateauAdverse.ZoneMeteoDistance, true);
                        break;
                    case PouvoirSpecial.Pluie:
                        plateauAdverse.SurlignerZone(plateauAdverse.ZoneMeteoSiege, true);
                        break;
                }
                return;
            }

            // Cas Effet
            if (_carteSelectionnee.Type == TypeCarte.Effet)
            {
                plateauCourant.SurlignerZone(plateauCourant.ZoneEffetMelee, true);
                plateauCourant.SurlignerZone(plateauCourant.ZoneEffetDistance, true);
                plateauCourant.SurlignerZone(plateauCourant.ZoneEffetSiege, true);
                return;
            }

            // Cas standard
            var zoneStandard = GetZonePrincipale(plateauCourant, _carteSelectionnee.Type);
            if (zoneStandard != null) plateauCourant.SurlignerZone(zoneStandard, true);
        }

        private FlowLayoutPanel GetZonePrincipale(PlateauJoueurControl control, TypeCarte type)
        {
            switch (type)
            {
                case TypeCarte.Melee: return control.ZoneMelee;
                case TypeCarte.Distance: return control.ZoneDistance;
                case TypeCarte.Siege: return control.ZoneSiege;
                default: return null;
            }
        }

        private void ResetToutesSurbrillances()
        {
            _plateauControlJ1.ResetToutesSurbrillances();
            _plateauControlJ2.ResetToutesSurbrillances();
        }

        private void OnZoneClicked(object sender, ZoneClickEventArgs e)
        {
            if (_carteSelectionnee == null) return;

            System.Diagnostics.Debug.WriteLine($"[OnZoneClicked] Zone={e.NomZone}");

            if (_isNetworkGame && _localPlayerIndex != _partie.IndexJoueurCourant)
                return;

            // Vérifier que la zone est valide pour cette carte
            if (!EstZoneValide(e.Zone))
                return;

            // Placer la carte
            PlacerCarte(e.Zone);
        }

        private bool EstZoneValide(FlowLayoutPanel zoneCible)
        {
            if (_carteSelectionnee == null) return false;

            var plateau = _partie.PlateauCourant;
            var adversaire = _partie.PlateauAdversaire;

            // Cas Espion
            if (_carteSelectionnee.Pouvoir == PouvoirSpecial.Espion)
            {
                return zoneCible == adversaire.GetZonePourType(_carteSelectionnee.Type);
            }

            // Cas Agile
            if (_carteSelectionnee.Pouvoir == PouvoirSpecial.Agile)
            {
                return zoneCible == plateau.ZoneMelee || zoneCible == plateau.ZoneDistance;
            }

            // Cas Leurre
            if (_carteSelectionnee.Pouvoir == PouvoirSpecial.Leurre)
            {
                return plateau.ZonesCombat().Contains(zoneCible);
            }

            // Cas Météo
            if (_carteSelectionnee.Type == TypeCarte.Meteo)
            {
                return adversaire.ZonesMeteo().Contains(zoneCible);
            }

            // Cas Effet
            if (_carteSelectionnee.Type == TypeCarte.Effet)
            {
                return plateau.ZonesEffet().Contains(zoneCible);
            }

            // Cas standard
            return zoneCible == plateau.GetZonePourType(_carteSelectionnee.Type);
        }

        private async void PlacerCarte(FlowLayoutPanel zoneCible)
        {
            var carte = _carteSelectionnee;
            var plateauCourant = _partie.PlateauCourant;
            var plateauAdverse = _partie.PlateauAdversaire;

            System.Diagnostics.Debug.WriteLine($"[PlacerCarte] Carte={carte.Nom}, Zone={GetNomZone(zoneCible)}");

            // Retirer de la main
            _partie.JoueurCourant.Main.Remove(carte);

            // Ajouter visuellement
            var plateauControl = _partie.IndexJoueurCourant == 0 ? _plateauControlJ1 : _plateauControlJ2;
            plateauControl.AjouterCarteZone(zoneCible, carte);

            // Désélectionner
            _carteSelectionnee = null;
            _pbSelectionnee = null;
            ResetToutesSurbrillances();

            // Exécuter le pouvoir spécial
            var resultat = _gestionnairePouvoirs.ExecuterPouvoir(carte, plateauCourant, plateauAdverse, zoneCible);

            // Mode Leurre
            if (resultat.AttendreLeurre)
            {
                _modeLeurre = true;
                _carteLeurre = resultat.CarteLeurre;
                _zoneLeurre = resultat.ZoneLeurre;
                ForceRechargerMains();
                RafraichirTout();
                return;
            }

            // *** NOUVEAU : Gérer les cartes supplémentaires (Rassembler) ***
            if (resultat.CartesSupplementaires != null && resultat.CartesSupplementaires.Count > 0)
            {
                foreach (var carteSup in resultat.CartesSupplementaires)
                {
                    plateauControl.AjouterCarteZone(zoneCible, carteSup);

                    // Envoyer chaque carte supplémentaire via réseau
                    if (_isNetworkGame)
                    {
                        await EnvoyerPlayCardAsync(carteSup, zoneCible);
                    }
                }
            }

            // *** NOUVEAU : Gérer les cartes à détruire (Brûlure) ***
            if (resultat.CartesADetruire != null && resultat.CartesADetruire.Count > 0)
            {
                foreach (var (carteADetruire, zone, proprio) in resultat.CartesADetruire)
                {
                    // Retirer visuellement
                    foreach (Control ctrl in zone.Controls)
                    {
                        if (ctrl is PictureBox pb && pb.Tag is Carte c && c == carteADetruire)
                        {
                            zone.Controls.Remove(pb);
                            break;
                        }
                    }
                    // Ajouter au cimetière
                    proprio.Cimetiere.Add(carteADetruire);
                }
            }

            // *** NOUVEAU :  Vider les zones météo si nécessaire (Soleil) ***
            if (resultat.ViderZonesMeteo)
            {
                ViderToutesZonesMeteo();
            }

            // Envoyer la carte principale via réseau
            if (_isNetworkGame)
            {
                await EnvoyerPlayCardAsync(carte, zoneCible);
            }

            // Terminer le tour
            if (resultat.TerminerTour)
            {
                if (_isNetworkGame)
                {
                    int adversaireIndex = _localPlayerIndex == 0 ? 1 : 0;
                    _partie.IndexJoueurCourant = adversaireIndex;
                }
                else
                {
                    _partie.TerminerTour();
                }
            }

            ForceRechargerMains();
            RafraichirTout();
        }

       

        private void ViderToutesZonesMeteo()
        {
            // Vider les zones météo des deux joueurs
            foreach (var zone in _partie.Plateau1.ZonesMeteo())
            {
                if (zone != null)
                {
                    foreach (Control ctrl in zone.Controls.OfType<PictureBox>().ToList())
                    {
                        if (ctrl is PictureBox pb && pb.Tag is Carte carte)
                        {
                            _partie.Plateau1.Joueur.Cimetiere.Add(carte);
                        }
                        zone.Controls.Remove(ctrl);
                        ctrl.Dispose();
                    }
                }
            }

            foreach (var zone in _partie.Plateau2.ZonesMeteo())
            {
                if (zone != null)
                {
                    foreach (Control ctrl in zone.Controls.OfType<PictureBox>().ToList())
                    {
                        if (ctrl is PictureBox pb && pb.Tag is Carte carte)
                        {
                            _partie.Plateau2.Joueur.Cimetiere.Add(carte);
                        }
                        zone.Controls.Remove(ctrl);
                        ctrl.Dispose();
                    }
                }
            }
        }

        private void GererModeLeurre(CarteClickEventArgs e)
        {
            // Vérifier que la carte est sur le plateau du joueur courant
            var plateau = _partie.PlateauCourant;
            if (!plateau.ZonesCombat().Contains(e.ZoneSource))
            {
                AfficherMessage("Vous ne pouvez remplacer qu'une carte sur votre plateau.");
                return;
            }

            // Vérifier que ce n'est pas un Leurre
            if (e.Carte.Pouvoir == PouvoirSpecial.Leurre)
            {
                AfficherMessage("Vous ne pouvez pas remplacer un Leurre.");
                return;
            }

            // Échanger les cartes
            e.ZoneSource.Controls.Remove(e.PictureBox);
            _partie.JoueurCourant.Main.Add(e.Carte);

            // Quitter le mode Leurre
            _modeLeurre = false;
            _carteLeurre = null;
            _zoneLeurre = null;

            AfficherMessage($"Leurre :  {e.Carte.Nom} retourne dans votre main !");

            ForceRechargerMains();
            RafraichirTout();

            // CORRECTION : Gérer correctement le passage de tour
            if (_isNetworkGame)
            {
                // En réseau :  envoyer l'action et passer au tour adverse si l'adversaire n'a pas passé
                // TODO:  Envoyer l'action Leurre via réseau

                if (!_partie.PlateauAdversaire.APasse)
                {
                    int adversaireIndex = _localPlayerIndex == 0 ? 1 : 0;
                    _partie.IndexJoueurCourant = adversaireIndex;
                }
                // Si l'adversaire a passé, on reste sur notre tour
            }
            else
            {
                // En local : utiliser la logique de TerminerTour qui gère déjà le cas où l'adversaire a passé
                _partie.TerminerTour();
            }

            ForceRechargerMains();
            RafraichirTout();
        }

        private async void OnPasserClicked(object sender, EventArgs e)
        {
            var plateauControl = sender as PlateauJoueurControl;
            if (plateauControl == null) return;

            // Vérifier que c'est bien notre tour
            if (_isNetworkGame)
            {
                if (_localPlayerIndex != _partie.IndexJoueurCourant)
                {
                    System.Diagnostics.Debug.WriteLine("[OnPasserClicked] Pas mon tour (réseau)");
                    return;
                }
            }
            else
            {
                if (_partie.IndexJoueurCourant != plateauControl.IndexJoueur)
                    return;
            }

            System.Diagnostics.Debug.WriteLine($"[OnPasserClicked] Joueur {_partie.IndexJoueurCourant} passe");

            // Marquer que ce joueur a passé
            _partie.PlateauCourant.APasse = true;

            if (_isNetworkGame)
            {
                await EnvoyerPassAsync();

                // Vérifier si les deux ont passé
                if (_partie.Plateau1.APasse && _partie.Plateau2.APasse)
                {
                    System.Diagnostics.Debug.WriteLine("[OnPasserClicked] Fin de manche");
                    _partie.TerminerTour();
                }
                else
                {
                    // Passer au tour de l'adversaire
                    int adversaireIndex = _localPlayerIndex == 0 ? 1 : 0;
                    _partie.IndexJoueurCourant = adversaireIndex;
                }

                ForceRechargerMains();
                RafraichirTout();
            }
            else
            {
                _partie.PasserTour();
                ForceRechargerMains();
                RafraichirTout();
            }
        }

        private void OnPouvoirClicked(object sender, EventArgs e)
        {
            var plateauControl = sender as PlateauJoueurControl;
            if (plateauControl == null) return;

            if (_partie.IndexJoueurCourant != plateauControl.IndexJoueur)
                return;

            if (_isNetworkGame && _localPlayerIndex != _partie.IndexJoueurCourant)
                return;

            _gestionnairePouvoirs.ExecuterPouvoirDeck(
                _partie.PlateauCourant,
                _partie.PlateauAdversaire
            );

            ForceRechargerMains();
            RafraichirTout();
        }

        private void OnApercuClicked(object sender, EventArgs e)
        {
            var plateauControl = sender as PlateauJoueurControl;
            if (plateauControl == null) return;

            // Afficher le deck adverse
            var deckApercu = plateauControl.IndexJoueur == 0 ? _deckInitialJ2 : _deckInitialJ1;
            var nomAdverse = plateauControl.IndexJoueur == 0 ? _partie.Jeu.Joueur2.Nom : _partie.Jeu.Joueur1.Nom;

            using (var form = new FormDeckApercu(nomAdverse, deckApercu))
            {
                form.ShowDialog(this);
            }
        }

        private void BoutonAide_Click(object sender, EventArgs e)
        {
            string aide = @"Déroulement d'une partie :  
- Chaque joueur pioche ses cartes et joue à tour de rôle.  
- À chaque tour, posez une carte ou passez.
- La manche se termine quand les deux joueurs passent.
- Le joueur avec le plus de points remporte la manche.

Effets passifs des decks :
- Royaumes du Nord : Pioche une carte supplémentaire à chaque manche gagnée. 
- Monstres : Garde une carte aléatoire sur le plateau à la fin de chaque manche.  
- Scoia'Tael : Choisit qui commence la partie.  
- Nilfgaard : Remporte la manche en cas d'égalité de score.

Pouvoirs activables des decks (utilisation unique par partie) :
- Royaumes du Nord : Retire tous les effets météo (Soleil).
- Monstres :  Récupère une carte du cimetière (Medic).
- Scoia'Tael : Brûle les cartes les plus fortes de la mêlée adverse.
- Nilfgaard : Double la puissance de votre ligne mêlée.

Pouvoirs spéciaux des cartes :
- Medic : Récupère une carte du cimetière.
- Espion : Pioche 2 cartes (se place chez l'adversaire).
- Rassembler : Pose toutes les cartes identiques.  
- Brûlure : Détruit les cartes les plus puissantes.  
- Leurre :  Remplace une de vos cartes.  
- Charge : Double la puissance d'une zone. 
- Boost Morale : +1 à chaque carte de la zone. 
- Lien Étroits : Multiplie les cartes identiques.  
- Agile : Peut être jouée en mêlée ou distance.  

Effets météo :
- Gel : Réduit les cartes de mêlée à 1.
- Brouillard :  Réduit les cartes de distance à 1.
- Pluie : Réduit les cartes de siège à 1.
- Soleil : Retire tous les effets météo.";

            using (var form = new FormAide(aide))
            {
                form.ShowDialog(this);
            }
        }

        #endregion

        #region Événements de la partie

        private void OnTourChange()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnTourChange));
                return;
            }

            ForceRechargerMains();
            RafraichirTout();
        }

        private void OnMancheTerminee()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnMancheTerminee));
                return;
            }

            ForceRechargerMains();
            RafraichirTout();
        }

        private void OnPartieGagnee(PlateauJoueur gagnant)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPartieGagnee(gagnant)));
                return;
            }

            MessageBox.Show($"{gagnant.Joueur.Nom} remporte la partie !", "Fin de partie",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void OnEtatChange()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnEtatChange));
                return;
            }

            ForceRechargerMains();
            RafraichirTout();
        }

        #endregion

        #region Méthodes utilitaires

        private void AfficherMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AfficherMessage(message)));
                return;
            }

            MessageBox.Show(message);
        }

        private Carte ChoisirCarteCimetiere(List<Carte> cimetiere)
        {
            using (var form = new FormCimetiere(cimetiere))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.CarteChoisie;
                }
            }
            return null;
        }

        public void ApplyStartGameDto(StartGameDto dto, bool isHost)
        {
            if (dto == null) return;
            ForceRechargerMains();
            RafraichirTout();
        }

        #endregion

        #region Réseau

        public void ConfigurerModeReseau(bool isHost, int localPlayerIndex, Server server = null, Client client = null)
        {
            System.Diagnostics.Debug.WriteLine($"[ConfigurerModeReseau] AVANT:  _isNetworkGame={_isNetworkGame}, _localPlayerIndex={_localPlayerIndex}");

            _isNetworkGame = true;
            _isHostInstance = isHost;
            _localPlayerIndex = localPlayerIndex;
            _server = server;
            _client = client;
            _hostAddress = "192.168.1.4";
            _hostPort = 12345;

            System.Diagnostics.Debug.WriteLine($"[ConfigurerModeReseau] APRES: _isNetworkGame={_isNetworkGame}, _localPlayerIndex={_localPlayerIndex}");

            // S'abonner aux événements réseau
            if (_server != null)
            {
                _server.MessageReceived -= OnNetworkMessageReceived;
                _server.MessageReceived += OnNetworkMessageReceived;
            }

            if (_client != null)
            {
                _client.MessageReceived -= OnNetworkMessageReceived;
                _client.MessageReceived += OnNetworkMessageReceived;
            }

            // IMPORTANT: Recharger les mains avec la bonne logique de masquage
            ForceRechargerMains();
            RafraichirTout();

            System.Diagnostics.Debug.WriteLine($"[ConfigurerModeReseau] Configuration terminée");
        }

        public void UseExistingServer(Server server)
        {
            ConfigurerModeReseau(isHost: true, localPlayerIndex: 0, server: server, client: null);
        }

        public void UseExistingClient(Client client)
        {
            ConfigurerModeReseau(isHost: false, localPlayerIndex: 1, server: null, client: client);
        }

        private void OnNetworkMessageReceived(NetMessage msg)
        {
            if (msg == null) return;

            System.Diagnostics.Debug.WriteLine($"[OnNetworkMessageReceived] Type={msg.Type}");

            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => OnNetworkMessageReceived(msg)));
                }
                catch (ObjectDisposedException)
                {
                    // Le formulaire a été fermé
                }
                return;
            }

            switch (msg.Type)
            {
                case MessageType.PlayCard:
                    GererPlayCardRecu(msg);
                    break;

                case MessageType.TurnSwitched:
                    GererTurnSwitchedRecu(msg);
                    break;

                case MessageType.Pass:
                    GererPassRecu(msg);
                    break;

                case MessageType.SaveGame:
                    GererSaveGameRecu(msg);
                    break;
            }
        }

        private void GererPlayCardRecu(NetMessage msg)
        {
            System.Diagnostics.Debug.WriteLine("[GererPlayCardRecu] Début");

            var dto = JsonConvert.DeserializeObject<PlayCardDto>(msg.Payload);
            if (dto == null)
            {
                System.Diagnostics.Debug.WriteLine("[GererPlayCardRecu] DTO null");
                return;
            }

            // Ignorer si c'est notre propre carte
            if (dto.PlayerIndex == _localPlayerIndex)
            {
                System.Diagnostics.Debug.WriteLine($"[GererPlayCardRecu] Ignoré - notre propre carte");
                return;
            }

            string localImagePath = ImageHelper.ToAbsolutePath(dto.ImagePath);
            System.Diagnostics.Debug.WriteLine($"[GererPlayCardRecu] ImagePath converti:  {dto.ImagePath} -> {localImagePath}");
            // Recréer l'objet Carte
            var carte = new Carte(
                dto.CardName,
                dto.Power,
                localImagePath, (TypeCarte)dto.Type,
                (PouvoirSpecial)dto.Pouvoir
            );

            // Identifier les plateaux
            PlateauJoueur plateauJoueur = dto.PlayerIndex == 0 ? _partie.Plateau1 : _partie.Plateau2;
            PlateauJoueur plateauAdverse = dto.PlayerIndex == 0 ? _partie.Plateau2 : _partie.Plateau1;
            PlateauJoueurControl plateauControl = dto.PlayerIndex == 0 ? _plateauControlJ1 : _plateauControlJ2;

            // Retirer la carte de la main
            var carteARetirer = plateauJoueur.Joueur.Main.Find(c => c.Nom == dto.CardName);
            if (carteARetirer != null)
            {
                plateauJoueur.Joueur.Main.Remove(carteARetirer);
                System.Diagnostics.Debug.WriteLine("[GererPlayCardRecu] Carte retirée de la main");
            }

            // Trouver la zone cible
            var zoneCible = GetZoneFromName(dto.Zone, dto.PlayerIndex);
            if (zoneCible == null)
            {
                System.Diagnostics.Debug.WriteLine($"[GererPlayCardRecu] Zone inconnue:  {dto.Zone}");
                return;
            }

            // Ajouter la carte visuellement
            plateauControl.AjouterCarteZone(zoneCible, carte);
            System.Diagnostics.Debug.WriteLine("[GererPlayCardRecu] Carte ajoutée à la zone");

            // Appliquer les effets
            AppliquerEffetsCarteRecue(carte, plateauJoueur, plateauAdverse, zoneCible);

            // C'est maintenant mon tour
            _partie.IndexJoueurCourant = _localPlayerIndex;
            System.Diagnostics.Debug.WriteLine($"[GererPlayCardRecu] Tour passé à _localPlayerIndex={_localPlayerIndex}");

            ForceRechargerMains();
            RafraichirTout();

            System.Diagnostics.Debug.WriteLine("[GererPlayCardRecu] Fin");
        }
        #endregion

        #region Gestion clavier et fermeture

        private void FPrincipal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_enPleinEcran)
                {
                    _enPleinEcran = false;
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.Bounds = Screen.FromControl(this).WorkingArea;
                }
                else
                {
                    _enPleinEcran = true;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
                e.Handled = true;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Arrêter la musique
            try
            {
                _player?.Stop();
                _player?.Dispose();
            }
            catch { }

            // Libérer les ressources réseau
            try
            {
                _server?.Dispose();
                _client?.Dispose();
            }
            catch { }

            // Libérer le cache d'images
            ImageHelper.LibererCache();

            // Libérer les gestionnaires
            _gestionnaireUI?.Dispose();

            base.OnFormClosing(e);
        }

        #endregion

        #region Méthodes réseau (suite)

        private void GererTurnSwitchedRecu(NetMessage msg)
        {
            System.Diagnostics.Debug.WriteLine("[GererTurnSwitchedRecu] C'est mon tour !");

            _partie.IndexJoueurCourant = _localPlayerIndex;

            ForceRechargerMains();
            RafraichirTout();
        }

        private void GererPassRecu(NetMessage msg)
        {
            System.Diagnostics.Debug.WriteLine("[GererPassRecu] L'adversaire a passé");

            int adversaireIndex = _localPlayerIndex == 0 ? 1 : 0;
            var plateauAdverse = _partie.GetPlateau(adversaireIndex);
            plateauAdverse.APasse = true;

            if (_partie.Plateau1.APasse && _partie.Plateau2.APasse)
            {
                System.Diagnostics.Debug.WriteLine("[GererPassRecu] Les deux ont passé - fin de manche");
                _partie.TerminerTour();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[GererPassRecu] C'est mon tour");
                _partie.IndexJoueurCourant = _localPlayerIndex;
            }

            ForceRechargerMains();
            RafraichirTout();
        }

        private async System.Threading.Tasks.Task EnvoyerPlayCardAsync(Carte carte, FlowLayoutPanel zone)
        {
            var dto = new PlayCardDto
            {
                CardName = carte.Nom,
                PlayerIndex = _localPlayerIndex,
                Zone = GetNomZone(zone),
                Power = carte.Puissance,
                ImagePath = ImageHelper.ToRelativePath(carte.ImagePath),
                Type = (int)carte.Type,
                Pouvoir = (int)carte.Pouvoir
            };

            var msg = new NetMessage
            {
                Type = MessageType.PlayCard,
                Payload = JsonConvert.SerializeObject(dto)
            };

            System.Diagnostics.Debug.WriteLine($"[EnvoyerPlayCardAsync] Envoi carte {carte.Nom} vers zone {dto.Zone}");

            try
            {
                if (_server != null) await _server.SendAsync(msg);
                else if (_client != null) await _client.SendAsync(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EnvoyerPlayCardAsync] Erreur:  {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task EnvoyerPassAsync()
        {
            var msg = new NetMessage
            {
                Type = MessageType.Pass,
                Payload = _localPlayerIndex.ToString()
            };

            System.Diagnostics.Debug.WriteLine("[EnvoyerPassAsync] Envoi Pass");

            try
            {
                if (_server != null) await _server.SendAsync(msg);
                else if (_client != null) await _client.SendAsync(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EnvoyerPassAsync] Erreur:  {ex.Message}");
            }
        }

        private string GetNomZone(FlowLayoutPanel zone)
        {
            // Zones de combat J1
            if (zone == _partie.Plateau1.ZoneMelee) return "Melee";
            if (zone == _partie.Plateau1.ZoneDistance) return "Distance";
            if (zone == _partie.Plateau1.ZoneSiege) return "Siege";

            // Zones de combat J2
            if (zone == _partie.Plateau2.ZoneMelee) return "Melee";
            if (zone == _partie.Plateau2.ZoneDistance) return "Distance";
            if (zone == _partie.Plateau2.ZoneSiege) return "Siege";

            // Zones d'effet J1
            if (zone == _partie.Plateau1.ZoneEffetMelee) return "EffetMelee";
            if (zone == _partie.Plateau1.ZoneEffetDistance) return "EffetDistance";
            if (zone == _partie.Plateau1.ZoneEffetSiege) return "EffetSiege";

            // Zones d'effet J2
            if (zone == _partie.Plateau2.ZoneEffetMelee) return "EffetMelee";
            if (zone == _partie.Plateau2.ZoneEffetDistance) return "EffetDistance";
            if (zone == _partie.Plateau2.ZoneEffetSiege) return "EffetSiege";

            // Zones météo J1
            if (zone == _partie.Plateau1.ZoneMeteoMelee) return "MeteoMelee";
            if (zone == _partie.Plateau1.ZoneMeteoDistance) return "MeteoDistance";
            if (zone == _partie.Plateau1.ZoneMeteoSiege) return "MeteoSiege";

            // Zones météo J2
            if (zone == _partie.Plateau2.ZoneMeteoMelee) return "MeteoMelee";
            if (zone == _partie.Plateau2.ZoneMeteoDistance) return "MeteoDistance";
            if (zone == _partie.Plateau2.ZoneMeteoSiege) return "MeteoSiege";

            return "Unknown";
        }

        private FlowLayoutPanel GetZoneFromName(string zoneName, int playerIndex)
        {
            var plateau = playerIndex == 0 ? _partie.Plateau1 : _partie.Plateau2;

            switch (zoneName)
            {
                case "Melee": return plateau.ZoneMelee;
                case "Distance": return plateau.ZoneDistance;
                case "Siege": return plateau.ZoneSiege;
                case "EffetMelee": return plateau.ZoneEffetMelee;
                case "EffetDistance": return plateau.ZoneEffetDistance;
                case "EffetSiege": return plateau.ZoneEffetSiege;
                case "MeteoMelee": return plateau.ZoneMeteoMelee;
                case "MeteoDistance": return plateau.ZoneMeteoDistance;
                case "MeteoSiege": return plateau.ZoneMeteoSiege;
                default: return null;
            }
        }

        #endregion
        private void AppliquerEffetsCarteRecue(Carte carte, PlateauJoueur plateauJoueur, PlateauJoueur plateauAdverse, FlowLayoutPanel zoneCible)
        {
            if (carte.Type == TypeCarte.Meteo)
            {
                switch (carte.Pouvoir)
                {
                    case PouvoirSpecial.Gel:
                        plateauJoueur.MeteoMeleeActive = true;
                        plateauAdverse.MeteoMeleeActive = true;
                        break;
                    case PouvoirSpecial.Brouillard:
                        plateauJoueur.MeteoDistanceActive = true;
                        plateauAdverse.MeteoDistanceActive = true;
                        break;
                    case PouvoirSpecial.Pluie:
                        plateauJoueur.MeteoSiegeActive = true;
                        plateauAdverse.MeteoSiegeActive = true;
                        break;
                    case PouvoirSpecial.Soleil:
                        plateauJoueur.MeteoMeleeActive = false;
                        plateauJoueur.MeteoDistanceActive = false;
                        plateauJoueur.MeteoSiegeActive = false;
                        plateauAdverse.MeteoMeleeActive = false;
                        plateauAdverse.MeteoDistanceActive = false;
                        plateauAdverse.MeteoSiegeActive = false;
                        break;
                }
            }
            else if (carte.Pouvoir == PouvoirSpecial.Charge)
            {
                if (zoneCible == plateauJoueur.ZoneMelee || zoneCible == plateauJoueur.ZoneEffetMelee)
                    plateauJoueur.ChargeMeleeActive = true;
                else if (zoneCible == plateauJoueur.ZoneDistance || zoneCible == plateauJoueur.ZoneEffetDistance)
                    plateauJoueur.ChargeDistanceActive = true;
                else if (zoneCible == plateauJoueur.ZoneSiege || zoneCible == plateauJoueur.ZoneEffetSiege)
                    plateauJoueur.ChargeSiegeActive = true;
            }
            else if (carte.Pouvoir == PouvoirSpecial.Espion)
            {
                // L'espion fait piocher le joueur qui l'a joué
                int nbAPiocher = Math.Min(2, plateauJoueur.Joueur.Deck.Count);
                for (int i = 0; i < nbAPiocher; i++)
                {
                    plateauJoueur.Joueur.Piocher();
                }
            }
        }

    }
}