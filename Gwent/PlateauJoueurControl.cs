using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Gwent
{
    /// <summary>
    /// Contrôle utilisateur représentant le plateau d'un joueur. 
    /// Layout fidèle au Gwent original.
    /// </summary>
    public partial class PlateauJoueurControl : UserControl
    {
        #region Propriétés

        public PlateauJoueur Plateau { get; private set; }
        public int IndexJoueur { get; private set; }
        public bool EstInverse { get; set; } = false;

        // Zones de combat
        public FlowLayoutPanelSurbrillance ZoneMelee { get; private set; }
        public FlowLayoutPanelSurbrillance ZoneDistance { get; private set; }
        public FlowLayoutPanelSurbrillance ZoneSiege { get; private set; }

        // Zones d'effet (Charge)
        public FlowLayoutPanelSurbrillance ZoneEffetMelee { get; private set; }
        public FlowLayoutPanelSurbrillance ZoneEffetDistance { get; private set; }
        public FlowLayoutPanelSurbrillance ZoneEffetSiege { get; private set; }

        // Zones météo
        public FlowLayoutPanelSurbrillance ZoneMeteoMelee { get; private set; }
        public FlowLayoutPanelSurbrillance ZoneMeteoDistance { get; private set; }
        public FlowLayoutPanelSurbrillance ZoneMeteoSiege { get; private set; }

        // Zone main
        public FlowLayoutPanel ZoneMain { get; private set; }

        // Labels
        public Label LabelScoreMelee { get; private set; }
        public Label LabelScoreDistance { get; private set; }
        public Label LabelScoreSiege { get; private set; }
        public Label LabelScoreTotal { get; private set; }
        public Label LabelPioche { get; private set; }
        public Label LabelCimetiere { get; private set; }
        public Label LabelNomJoueur { get; private set; }

        // Boutons
        public Button BoutonPasser { get; private set; }
        public Button BoutonPouvoir { get; private set; }
        public Button BoutonApercu { get; private set; }

        // PictureBox vies
        public PictureBox PbVie1 { get; private set; }
        public PictureBox PbVie2 { get; private set; }

        // Panels
        private Panel _panelInfoJoueur;
        private Panel _panelScores;
        private ToolTip _toolTip;

        #endregion

        #region Événements

        public event EventHandler<CarteClickEventArgs> CarteClicked;
        public event EventHandler<ZoneClickEventArgs> ZoneClicked;
        public event EventHandler PasserClicked;
        public event EventHandler PouvoirClicked;
        public event EventHandler ApercuClicked;

        #endregion

        #region Constructeur

        public PlateauJoueurControl()
        {
            InitialiserComposants();
        }

        public void Initialiser(PlateauJoueur plateau, int indexJoueur, bool estInverse = false)
        {
            Plateau = plateau ?? throw new ArgumentNullException(nameof(plateau));
            IndexJoueur = indexJoueur;
            EstInverse = estInverse;

            LierZonesAuPlateau();
            LabelNomJoueur.Text = plateau.Joueur.Nom;
            MettreAJourAffichage();
        }

        #endregion

        #region Initialisation des composants

        private void InitialiserComposants()
        {
            this.SuspendLayout();

            _toolTip = new ToolTip();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(101, 67, 33);

            // Layout principal :  3 colonnes (Info | Plateau | Scores)
            var layoutPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(5)
            };

            // Colonnes :  Info (80px) | Plateau (stretch) | Scores (100px)
            layoutPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layoutPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layoutPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Créer les 3 sections
            _panelInfoJoueur = CreerPanelInfoJoueur();
            var panelPlateau = CreerPanelPlateau();
            _panelScores = CreerPanelScores();

            layoutPrincipal.Controls.Add(_panelInfoJoueur, 0, 0);
            layoutPrincipal.Controls.Add(panelPlateau, 1, 0);
            layoutPrincipal.Controls.Add(_panelScores, 2, 0);

            this.Controls.Add(layoutPrincipal);
            this.ResumeLayout();
        }

        private Panel CreerPanelInfoJoueur()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 40, 20),
                Padding = new Padding(5)
            };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                BackColor = Color.Transparent
            };

            // Nom du joueur
            LabelNomJoueur = new Label
            {
                Text = "Joueur",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            };

            // Vies
            var panelVies = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)
            };
            PbVie1 = new PictureBox { Size = new Size(25, 25), SizeMode = PictureBoxSizeMode.StretchImage, Margin = new Padding(2) };
            PbVie2 = new PictureBox { Size = new Size(25, 25), SizeMode = PictureBoxSizeMode.StretchImage, Margin = new Padding(2) };
            panelVies.Controls.Add(PbVie1);
            panelVies.Controls.Add(PbVie2);

            // Boutons
            BoutonPasser = CreerBouton("Passer", Color.DarkRed);
            BoutonPasser.Click += (s, e) => PasserClicked?.Invoke(this, EventArgs.Empty);

            BoutonPouvoir = CreerBouton("Pouvoir", Color.DarkBlue);
            BoutonPouvoir.Click += (s, e) => PouvoirClicked?.Invoke(this, EventArgs.Empty);

            BoutonApercu = CreerBouton("Aperçu", Color.DarkGreen);
            BoutonApercu.Click += (s, e) => ApercuClicked?.Invoke(this, EventArgs.Empty);

            layout.Controls.Add(LabelNomJoueur);
            layout.Controls.Add(panelVies);
            layout.Controls.Add(BoutonPasser);
            layout.Controls.Add(BoutonPouvoir);
            layout.Controls.Add(BoutonApercu);

            panel.Controls.Add(layout);
            return panel;
        }

        private Button CreerBouton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Size = new Size(70, 28),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8),
                Margin = new Padding(0, 3, 0, 3)
            };
        }

        private Panel CreerPanelPlateau()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Layout :  4 lignes (3 zones + main)
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            // Colonnes : Effet (50px) | Combat (stretch) | Météo (50px)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));

            // Lignes :  3 zones (25% chacune) + Main (25%)
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

            // Créer les zones
            CreerZonesCombat();
            CreerZonesEffet();
            CreerZonesMeteo();
            CreerPanelMain();

            // Ordre des zones selon si inversé ou non
            // Normal (J1 en bas) : Siège, Distance, Mêlée (de haut en bas)
            // Inversé (J2 en haut) : Mêlée, Distance, Siège (de haut en bas)
            int[] lignes = EstInverse ? new[] { 0, 1, 2 } : new[] { 0, 1, 2 };

            // Ligne 0 : Siège (ou Mêlée si inversé)
            var zoneHaut = EstInverse ? ZoneMelee : ZoneSiege;
            var effetHaut = EstInverse ? ZoneEffetMelee : ZoneEffetSiege;
            var meteoHaut = EstInverse ? ZoneMeteoMelee : ZoneMeteoSiege;

            layout.Controls.Add(effetHaut, 0, 0);
            layout.Controls.Add(zoneHaut, 1, 0);
            layout.Controls.Add(meteoHaut, 2, 0);

            // Ligne 1 : Distance
            layout.Controls.Add(ZoneEffetDistance, 0, 1);
            layout.Controls.Add(ZoneDistance, 1, 1);
            layout.Controls.Add(ZoneMeteoDistance, 2, 1);

            // Ligne 2 : Mêlée (ou Siège si inversé)
            var zoneBas = EstInverse ? ZoneSiege : ZoneMelee;
            var effetBas = EstInverse ? ZoneEffetSiege : ZoneEffetMelee;
            var meteoBas = EstInverse ? ZoneMeteoSiege : ZoneMeteoMelee;

            layout.Controls.Add(effetBas, 0, 2);
            layout.Controls.Add(zoneBas, 1, 2);
            layout.Controls.Add(meteoBas, 2, 2);

            // Ligne 3 : Main
            layout.Controls.Add(ZoneMain, 0, 3);
            layout.SetColumnSpan(ZoneMain, 3);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreerPanelScores()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 40, 20),
                Padding = new Padding(5)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Color.Transparent
            };

            // Scores par zone
            LabelScoreSiege = CreerLabelScore();
            LabelScoreDistance = CreerLabelScore();
            LabelScoreMelee = CreerLabelScore();

            // Score total
            LabelScoreTotal = CreerLabelScore();
            LabelScoreTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            LabelScoreTotal.ForeColor = Color.Gold;

            // Pioche et cimetière
            LabelPioche = CreerLabelScore();
            LabelCimetiere = CreerLabelScore();

            // Ajouter dans l'ordre (haut en bas)
            int row = 0;

            // Score total en haut
            layout.Controls.Add(CreerLabelTitre("SCORE"), 0, row++);
            layout.Controls.Add(LabelScoreTotal, 0, row++);

            // Scores par zone selon l'ordre
            if (EstInverse)
            {

                layout.Controls.Add(LabelScoreSiege, 0, row++);
                layout.Controls.Add(LabelScoreDistance, 0, row++);
                layout.Controls.Add(LabelScoreMelee, 0, row++);
            }
            else
            {
                layout.Controls.Add(LabelScoreMelee, 0, row++);
                layout.Controls.Add(LabelScoreDistance, 0, row++);
                layout.Controls.Add(LabelScoreSiege, 0, row++);
            }

            // Pioche et cimetière
            layout.Controls.Add(CreerLabelTitre("Pioche"), 0, row++);
            layout.Controls.Add(LabelPioche, 0, row++);
            layout.Controls.Add(CreerLabelTitre("Cimetière"), 0, row++);
            layout.Controls.Add(LabelCimetiere, 0, row++);

            panel.Controls.Add(layout);
            return panel;
        }

        private Label CreerLabelTitre(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 0)
            };
        }

        private void CreerZonesCombat()
        {
            ZoneMelee = CreerZoneCombat("Mêlée");
            ZoneDistance = CreerZoneCombat("Distance");
            ZoneSiege = CreerZoneCombat("Siège");
        }

        private FlowLayoutPanelSurbrillance CreerZoneCombat(string nom)
        {
            var zone = new FlowLayoutPanelSurbrillance
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                AllowDrop = true,
                Margin = new Padding(2),
                BackColor = Color.FromArgb(50, 50, 50)
            };

            zone.Click += (s, e) => OnZoneClicked(zone, nom);
            return zone;
        }

        private void CreerZonesEffet()
        {
            ZoneEffetMelee = CreerZoneEffet();
            ZoneEffetDistance = CreerZoneEffet();
            ZoneEffetSiege = CreerZoneEffet();
        }

        private FlowLayoutPanelSurbrillance CreerZoneEffet()
        {
            var zone = new FlowLayoutPanelSurbrillance
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                BackColor = Color.FromArgb(70, 50, 30)
            };
            zone.Click += (s, e) => OnZoneClicked(zone, "Effet");
            return zone;
        }

        private void CreerZonesMeteo()
        {
            ZoneMeteoMelee = CreerZoneMeteo();
            ZoneMeteoDistance = CreerZoneMeteo();
            ZoneMeteoSiege = CreerZoneMeteo();
        }

        private FlowLayoutPanelSurbrillance CreerZoneMeteo()
        {
            var zone = new FlowLayoutPanelSurbrillance
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                BackColor = Color.FromArgb(70, 50, 30)
            };
            zone.Click += (s, e) => OnZoneClicked(zone, "Météo");
            return zone;
        }

        private void CreerPanelMain()
        {
            ZoneMain = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(40, 40, 40),
                Margin = new Padding(2)
            };
        }

        private Label CreerLabelScore()
        {
            return new Label
            {
                Text = "0",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Margin = new Padding(0, 2, 0, 2)
            };
        }

        #endregion

        #region Liaison avec PlateauJoueur

        private void LierZonesAuPlateau()
        {
            if (Plateau == null) return;

            Plateau.ZoneMelee = ZoneMelee;
            Plateau.ZoneDistance = ZoneDistance;
            Plateau.ZoneSiege = ZoneSiege;

            Plateau.ZoneEffetMelee = ZoneEffetMelee;
            Plateau.ZoneEffetDistance = ZoneEffetDistance;
            Plateau.ZoneEffetSiege = ZoneEffetSiege;

            Plateau.ZoneMeteoMelee = ZoneMeteoMelee;
            Plateau.ZoneMeteoDistance = ZoneMeteoDistance;
            Plateau.ZoneMeteoSiege = ZoneMeteoSiege;

            Plateau.ZoneMain = ZoneMain;

            Plateau.LabelScoreMelee = LabelScoreMelee;
            Plateau.LabelScoreDistance = LabelScoreDistance;
            Plateau.LabelScoreSiege = LabelScoreSiege;
            Plateau.LabelScoreTotal = LabelScoreTotal;
            Plateau.LabelPioche = LabelPioche;
            Plateau.LabelCimetiere = LabelCimetiere;

            Plateau.BoutonPasser = BoutonPasser;
            Plateau.BoutonPouvoir = BoutonPouvoir;
            Plateau.BoutonApercu = BoutonApercu;

            Plateau.PbVie1 = PbVie1;
            Plateau.PbVie2 = PbVie2;
        }

        #endregion

        #region Chargement des images

        public void ChargerImages(string cheminImages)
        {
            string basePath = cheminImages ?? Path.Combine(Application.StartupPath, "Images");

            // Images de fond des zones de combat
            ImageHelper.AppliquerFond(ZoneSiege, Path.Combine(basePath, EstInverse ? "Plateau_siege1_Cartes.png" : "Plateau_siege2_Cartes.png"));
            ImageHelper.AppliquerFond(ZoneDistance, Path.Combine(basePath, EstInverse ? "Plateau_archer1_Cartes.png" : "Plateau_archer2_Cartes.png"));
            ImageHelper.AppliquerFond(ZoneMelee, Path.Combine(basePath, EstInverse ? "Plateau_melee1_Cartes.png" : "Plateau_melee2_Cartes.png"));

            // Images des zones effet/météo
            string imageEffet = Path.Combine(basePath, "Plateau_Effet. png");
            ImageHelper.AppliquerFond(ZoneEffetMelee, imageEffet);
            ImageHelper.AppliquerFond(ZoneEffetDistance, imageEffet);
            ImageHelper.AppliquerFond(ZoneEffetSiege, imageEffet);
            ImageHelper.AppliquerFond(ZoneMeteoMelee, imageEffet);
            ImageHelper.AppliquerFond(ZoneMeteoDistance, imageEffet);
            ImageHelper.AppliquerFond(ZoneMeteoSiege, imageEffet);

            // Images des vies
            string imageVie = Path.Combine(basePath, "vie. png");
            ImageHelper.AppliquerImage(PbVie1, imageVie);
            ImageHelper.AppliquerImage(PbVie2, imageVie);
        }

        public void ChargerMain(string dosCartePath, bool masquer)
        {
            ZoneMain.Controls.Clear();

            if (Plateau?.Joueur?.Main == null) return;

            System.Diagnostics.Debug.WriteLine($"[ChargerMain] Joueur={Plateau.Joueur.Nom}, Cartes={Plateau.Joueur.Main.Count}, Masquer={masquer}");

            foreach (var carte in Plateau.Joueur.Main)
            {
                var pb = new PictureBox
                {
                    Width = 70,
                    Height = 105,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Tag = carte,
                    Margin = new Padding(3),
                    Cursor = Cursors.Hand  // ← Important ! 
                };

                if (masquer && !string.IsNullOrEmpty(dosCartePath))
                {
                    ImageHelper.AppliquerImage(pb, dosCartePath);
                    _toolTip.SetToolTip(pb, "Carte cachée");
                    pb.Enabled = false;
                    pb.Cursor = Cursors.Default;
                }
                else
                {
                    ImageHelper.AppliquerImage(pb, carte.ImagePath);
                    _toolTip.SetToolTip(pb, $"Nom: {carte.Nom}\nPuissance: {carte.Puissance}\nPouvoir: {carte.Pouvoir}");

                    // ← Le gestionnaire de clic doit être ajouté ICI
                    pb.Click += (s, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"[ChargerMain] Clic sur {carte.Nom}");
                        OnCarteClicked(carte, pb, ZoneMain);
                    };

                    pb.Enabled = true;  // ← S'assurer que c'est activé
                }

                ZoneMain.Controls.Add(pb);
            }
        }

        #endregion

        #region Mise à jour de l'affichage

        public void MettreAJourAffichage()
        {
            if (Plateau == null) return;

            // Scores des zones
            int scoreMelee = CalculateurScore.CalculerScoreZone(ZoneMelee, Plateau.MeteoMeleeActive, Plateau.ChargeMeleeActive);
            int scoreDistance = CalculateurScore.CalculerScoreZone(ZoneDistance, Plateau.MeteoDistanceActive, Plateau.ChargeDistanceActive);
            int scoreSiege = CalculateurScore.CalculerScoreZone(ZoneSiege, Plateau.MeteoSiegeActive, Plateau.ChargeSiegeActive);

            LabelScoreMelee.Text = scoreMelee.ToString();
            LabelScoreDistance.Text = scoreDistance.ToString();
            LabelScoreSiege.Text = scoreSiege.ToString();
            LabelScoreTotal.Text = (scoreMelee + scoreDistance + scoreSiege).ToString();

            // Compteurs
            LabelPioche.Text = Plateau.Joueur.Deck?.Count.ToString() ?? "0";
            LabelCimetiere.Text = Plateau.Joueur.Cimetiere?.Count.ToString() ?? "0";

            // Vies
            PbVie1.Visible = Plateau.Vies >= 1;
            PbVie2.Visible = Plateau.Vies >= 2;

            // État des boutons
            BoutonPouvoir.Enabled = !Plateau.PouvoirUtilise;
        }

        public void ActiverControles(bool actif)
        {
            BoutonPasser.Enabled = actif && !Plateau.APasse;
            BoutonPouvoir.Enabled = actif && !Plateau.PouvoirUtilise;
            BoutonApercu.Enabled = true; // Toujours actif

            ZoneMelee.Enabled = actif;
            ZoneDistance.Enabled = actif;
            ZoneSiege.Enabled = actif;
            ZoneMain.Enabled = actif;
        }

        #endregion

        #region Gestion des cartes

        public void AjouterCarteZone(FlowLayoutPanel zone, Carte carte)
        {
            var pb = new PictureBox
            {
                Width = 60,
                Height = 90,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Tag = carte,
                Margin = new Padding(2)
            };

            ImageHelper.AppliquerImage(pb, carte.ImagePath);
            _toolTip.SetToolTip(pb, $"Nom: {carte.Nom}\nPuissance: {carte.Puissance}\nPouvoir:  {carte.Pouvoir}");
            pb.Click += (s, e) => OnCarteClicked(carte, pb, zone);
            pb.Cursor = Cursors.Hand;

            zone.Controls.Add(pb);
        }

        public void SurlignerZone(FlowLayoutPanel zone, bool actif)
        {
            if (zone is FlowLayoutPanelSurbrillance z)
            {
                z.Surbrillance = actif;
                z.Invalidate();
            }
        }

        public void ResetToutesSurbrillances()
        {
            SurlignerZone(ZoneMelee, false);
            SurlignerZone(ZoneDistance, false);
            SurlignerZone(ZoneSiege, false);
            SurlignerZone(ZoneEffetMelee, false);
            SurlignerZone(ZoneEffetDistance, false);
            SurlignerZone(ZoneEffetSiege, false);
            SurlignerZone(ZoneMeteoMelee, false);
            SurlignerZone(ZoneMeteoDistance, false);
            SurlignerZone(ZoneMeteoSiege, false);
        }

        #endregion

        #region Événements internes

        private void OnCarteClicked(Carte carte, PictureBox pb, FlowLayoutPanel zone)
        {
            CarteClicked?.Invoke(this, new CarteClickEventArgs(carte, pb, zone));
        }

        private void OnZoneClicked(FlowLayoutPanel zone, string nomZone)
        {
            ZoneClicked?.Invoke(this, new ZoneClickEventArgs(zone, nomZone));
        }

        #endregion
    }

    #region Classes d'événements

    public class CarteClickEventArgs : EventArgs
    {
        public Carte Carte { get; }
        public PictureBox PictureBox { get; }
        public FlowLayoutPanel ZoneSource { get; }

        public CarteClickEventArgs(Carte carte, PictureBox pb, FlowLayoutPanel zone)
        {
            Carte = carte;
            PictureBox = pb;
            ZoneSource = zone;
        }
    }

    public class ZoneClickEventArgs : EventArgs
    {
        public FlowLayoutPanel Zone { get; }
        public string NomZone { get; }

        public ZoneClickEventArgs(FlowLayoutPanel zone, string nomZone)
        {
            Zone = zone;
            NomZone = nomZone;
        }
    }

    #endregion
}