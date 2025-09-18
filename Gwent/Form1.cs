using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Gwent.Jeu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Media;


namespace Gwent
{
    public partial class FPrincipal : Form
    {

        private Jeu jeu;
        private Carte choix = null;
        private PictureBox carteSelectionnee = null;
        private bool enPleinEcran = true;
        private ToolTip toolTipCarte = new ToolTip();
        private bool joueur1Passe = false;
        private bool joueur2Passe = false;
        private int viesJoueur1 = 2;
        private int viesJoueur2 = 2;
        private Joueur joueurCourant;
        private Random random = new Random();
        private Joueur perdantDerniereManche = null;
        private string dosCarteJ1;
        private string dosCarteJ2;
        private bool modeLeurre = false;
        private Carte carteLeurre = null;
        private bool chargeActiveMeleeJ1 = false;
        private bool chargeActiveDistanceJ1 = false;
        private bool chargeActiveSiegeJ1 = false;
        private bool chargeActiveMeleeJ2 = false;
        private bool chargeActiveDistanceJ2 = false;
        private bool chargeActiveSiegeJ2 = false;
        private bool meteoFroidMordantJ1 = false;
        private bool meteoFroidMordantJ2 = false;
        private bool meteoBrouillardJ1 = false;
        private bool meteoBrouillardJ2 = false;
        private bool meteoPluieJ1 = false;
        private bool meteoPluieJ2 = false;
        private bool pouvoirUtiliseJ1 = false;
        private bool pouvoirUtiliseJ2 = false;
        private SoundPlayer player;
        private FlowLayoutPanel zoneLeurre = null;
        private List<Carte> deckInitialJ1;
        private List<Carte> deckInitialJ2;




        public FPrincipal(List<Carte> deckJ1, List<Carte> deckJ2, int indexDeckJ1, int indexDeckJ2)
        {
            InitializeComponent();

            // Initialise le jeu avec les decks des joueurs et les indices des decks
            jeu = new Jeu(deckJ1, deckJ2, indexDeckJ1, indexDeckJ2);

            this.DoubleBuffered = true;

            deckInitialJ1 = new List<Carte>(deckJ1);
            deckInitialJ2 = new List<Carte>(deckJ2);


            // Pouvoir passif Scoia'Tael : choisir qui commence
            if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel && jeu.Joueur2.PouvoirPassif != Jeu.PouvoirPassifDeck.ScoiaTel)
            {
                var choixDeck = MessageBox.Show("Voulez-vous que Joueur 1 commence ?", "Pouvoir Scoia'Tael", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                joueurCourant = (choixDeck == DialogResult.Yes) ? jeu.Joueur1 : jeu.Joueur2;
                MessageBox.Show($"{(joueurCourant == jeu.Joueur1 ? "Joueur 1" : "Joueur 2")} commence !");
            }
            else if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel && jeu.Joueur1.PouvoirPassif != Jeu.PouvoirPassifDeck.ScoiaTel)
            {
                var choixDeck = MessageBox.Show("Voulez-vous que Joueur 2 commence ?", "Pouvoir Scoia'Tael", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                joueurCourant = (choixDeck == DialogResult.Yes) ? jeu.Joueur2 : jeu.Joueur1;
                MessageBox.Show($"{(joueurCourant == jeu.Joueur1 ? "Joueur 1" : "Joueur 2")} commence !");
            }
            else
            {
                // Cas normal ou les deux ont Scoia'Tael : tirage au sort
                if (random.Next(2) == 0)
                {
                    joueurCourant = jeu.Joueur1;
                    MessageBox.Show($"Le joueur 1 commence !");
                }
                else
                {
                    joueurCourant = jeu.Joueur2;
                    MessageBox.Show($"Le joueur 2 commence !");
                }
            }

            // Chargement et lecture de la musique de fond en boucle
            string cheminMusique = Path.Combine(Application.StartupPath, "Musique", "gwent.wav");
            player = new SoundPlayer(cheminMusique);
            try
            {
                player.PlayLooping();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la lecture de la musique : " + ex.Message);
            }

            // Chargement de l'image des points de vie
            string imageVie = Path.Combine(Application.StartupPath, "Images", "VieGwent.png");
            pbVieJ1_1.Image = Image.FromFile(imageVie);
            pbVieJ1_2.Image = Image.FromFile(imageVie);
            pbVieJ2_1.Image = Image.FromFile(imageVie);
            pbVieJ2_2.Image = Image.FromFile(imageVie);

            // Affichage des points de vie
            pbVieJ1_1.Visible = true;
            pbVieJ1_2.Visible = true;
            pbVieJ2_1.Visible = true;
            pbVieJ2_2.Visible = true;

            // Donne le dos de carte correspondant au deck
            dosCarteJ1 = FormDeck.DosCartesDecks[indexDeckJ1];
            dosCarteJ2 = FormDeck.DosCartesDecks[indexDeckJ2];

            // Active les zones en fonction du joueur courant
            ActiverZones();

            // Chargement des cartes dans les panels
            ChargerCarte(flpJoueur1, jeu.Joueur1);
            ChargerCarte(flpJoueur2, jeu.Joueur2);

            // Mise à jour des labels de deck et cimetière
            MettreAJourDeckEtCimetiere();

            // Configuration de la fenêtre en plein écran sans bordure
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // Chargement de l’icône d’aide
            string imageHelp = Path.Combine(Application.StartupPath, "Images", "help.png");
            bAide.BackgroundImage = Image.FromFile(imageHelp);
            this.BackgroundImageLayout = ImageLayout.Center;
            bAide.Click += bAide_Click;

            // Chargement des images de fond pour toutes les zones du plateau de jeu
            ChargerImagePanel(flpSiegeJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_siege1_Cartes.png"));
            ChargerImagePanel(flpDistanceJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_archer1_Cartes.png"));
            ChargerImagePanel(flpMeleeJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_melee1_Cartes.png"));
            ChargerImagePanel(flpMeleeJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_melee2_Cartes.png"));
            ChargerImagePanel(flpDistanceJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_archer2_Cartes.png"));
            ChargerImagePanel(flpSiegeJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_siege2_Cartes.png"));


            ChargerImagePanel(flpCimetierre, Path.Combine(Application.StartupPath, "Images", "cimetierre.png"));
            ChargerImagePanel(flpCimetierre2, Path.Combine(Application.StartupPath, "Images", "cimetierre.png"));


            ChargerImagePanel(flpPiocheJ1, Path.Combine(Application.StartupPath, "Images", "Pioche.png"));
            ChargerImagePanel(flpPiocheJ2, Path.Combine(Application.StartupPath, "Images", "Pioche.png"));

            
            ChargerImagePanel(flpEffetArcherJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpEffetArcherJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpEffetMeleeJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpEffetMeleeJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpEffetSiegeJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpEffetSiegeJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpMeteoMeleeJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpMeteoDistanceJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpMeteoSiegeJ1, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpMeteoMeleeJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpMeteoDistanceJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));
            ChargerImagePanel(flpMeteoSiegeJ2, Path.Combine(Application.StartupPath, "Images", "Plateau_Effet.png"));


            // Couleur de fond des panels
            this.BackColor = Color.FromArgb(101, 67, 33);
            pJoueur1.BackColor = Color.FromArgb(101, 67, 33);
            pJoueur2.BackColor = Color.FromArgb(101, 67, 33);


            // Réinitialisation des états des pouvoirs spéciaux
            pouvoirUtiliseJ1 = false;
            pouvoirUtiliseJ2 = false;
            bPouvoirJ1.Enabled = true;
            bPouvoirJ2.Enabled = true;

            // Événements de clic pour les zones
            var panels = new[] {
                flpMeleeJ1, flpMeleeJ2, flpDistanceJ1, flpDistanceJ2, flpSiegeJ1, flpSiegeJ2,
                flpEffetArcherJ1, flpEffetArcherJ2, flpEffetMeleeJ1, flpEffetMeleeJ2,
                flpEffetSiegeJ1, flpEffetSiegeJ2, flpMeteoMeleeJ1, flpMeteoDistanceJ1, flpMeteoSiegeJ1,
                flpMeteoMeleeJ2, flpMeteoDistanceJ2, flpMeteoSiegeJ2
            };
            foreach (var panel in panels)
                panel.Click += ZoneCible_Click;

        }

        private void MettreAJourDeckEtCimetiere()
        {
            lPiocheJ1.Text = $"{jeu.Joueur1.Deck.Count}";
            lPiocheJ2.Text = $"{jeu.Joueur2.Deck.Count}";
            lCimetiereJ1.Text = $"{jeu.Joueur1.Cimetiere.Count}";
            lCimetiereJ2.Text = $"{jeu.Joueur2.Cimetiere.Count}";
        }

        private void ActiverZones()
        {
            // Désactive toutes les zones d'abord
            flpMeleeJ1.Enabled = false;
            flpDistanceJ1.Enabled = false;
            flpSiegeJ1.Enabled = false;
            flpMeleeJ2.Enabled = false;
            flpDistanceJ2.Enabled = false;
            flpSiegeJ2.Enabled = false;

            if (choix != null && choix.Pouvoir == PouvoirSpecial.Espion)
            {
                // On active la zone de l'adversaire (même si le joueur a passé)
                Joueur adversaire = (joueurCourant == jeu.Joueur1) ? jeu.Joueur2 : jeu.Joueur1;
                if (adversaire == jeu.Joueur1)
                {
                    if (choix.Type == TypeCarte.Melee) flpMeleeJ1.Enabled = true;
                    else if (choix.Type == TypeCarte.Distance) flpDistanceJ1.Enabled = true;
                    else if (choix.Type == TypeCarte.Siege) flpSiegeJ1.Enabled = true;
                }
                else
                {
                    if (choix.Type == TypeCarte.Melee) flpMeleeJ2.Enabled = true;
                    else if (choix.Type == TypeCarte.Distance) flpDistanceJ2.Enabled = true;
                    else if (choix.Type == TypeCarte.Siege) flpSiegeJ2.Enabled = true;
                }
            }

            else
            {
                // Comportement normal
                flpMeleeJ1.Enabled = (joueurCourant == jeu.Joueur1 && !joueur1Passe);
                flpDistanceJ1.Enabled = (joueurCourant == jeu.Joueur1 && !joueur1Passe);
                flpSiegeJ1.Enabled = (joueurCourant == jeu.Joueur1 && !joueur1Passe);

                flpMeleeJ2.Enabled = (joueurCourant == jeu.Joueur2 && !joueur2Passe);
                flpDistanceJ2.Enabled = (joueurCourant == jeu.Joueur2 && !joueur2Passe);
                flpSiegeJ2.Enabled = (joueurCourant == jeu.Joueur2 && !joueur2Passe);
            }

            // Active ou désactive le bouton "Passer".
            bPasserJ1.Enabled = (joueurCourant == jeu.Joueur1 && !joueur1Passe);
            bPasserJ2.Enabled = (joueurCourant == jeu.Joueur2 && !joueur2Passe);

            // Désactive les boutons Aperçu et Pouvoir de l'adversaire
            bApercuJ1.Enabled = (joueurCourant == jeu.Joueur1 && !joueur1Passe);
            bPouvoirJ1.Enabled = (joueurCourant == jeu.Joueur1 && !pouvoirUtiliseJ1);

            bApercuJ2.Enabled = (joueurCourant == jeu.Joueur2 && !joueur2Passe);
            bPouvoirJ2.Enabled = (joueurCourant == jeu.Joueur2 && !pouvoirUtiliseJ2);
        }


        private void ChargerCarte(FlowLayoutPanel panel, Joueur joueur)
        {
            // On vide le panel pour ne pas empiler les cartes à chaque appel.
            panel.Controls.Clear();

            // On parcourt toutes les cartes dans la main du joueur.
            foreach (var carte in joueur.Main)
            {
                // Création d’un contrôle PictureBox pour représenter visuellement une carte.
                PictureBox pb = new PictureBox
                {
                    Width = 80,
                    Height = 120,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Image.FromFile(carte.ImagePath),
                    Tag = carte,

                };

                // Si le joueur affiché n’est PAS le joueur courant (donc un adversaire) 
                if (joueur != joueurCourant)
                {
                    // On détermine l’image de dos de carte à utiliser selon le joueur.
                    // On affiche le dos de la carte au lieu de l’image réelle.
                    // On affiche une infobulle "Carte cachée" en survol.
                    // L’adversaire ne peut pas cliquer sur ses cartes.

                    string dosCartePath = (joueur == jeu.Joueur1) ? dosCarteJ1 : dosCarteJ2;
                    pb.Image = Image.FromFile(dosCartePath);
                    toolTipCarte.SetToolTip(pb, "Carte cachée");
                    pb.Enabled = false;
                }
                else
                {
                    // Si c’est le joueur courant, on tente de charger son image réelle.
                    // Affiche une erreur si l’image est introuvable ou invalide.
                    try
                    {
                        pb.Image = Image.FromFile(carte.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors du chargement de l'image de carte : " + carte.ImagePath + "\n" + ex.Message);
                    }
                    // Affichage d'une infobulle avec les infos de la carte.
                    // Le joueur courant peut cliquer sur ses cartes.
                    toolTipCarte.SetToolTip(pb, $"Nom : {carte.Nom}\nPuissance : {carte.Puissance}\nPouvoir : {carte.Pouvoir}");
                    pb.Enabled = true;
                }

                // Gestionnaire d'événement pour le clic sur la carte.
                // Ajoute la carte (PictureBox) au panel fourni.
                pb.Click += Pb_Click;
                panel.Controls.Add(pb);

            }
            // Met à jour l’affichage du deck et du cimetière après avoir chargé les cartes.
            MettreAJourDeckEtCimetiere();
        }

        private void Pb_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb == null || pb.Parent == null)
                return;

            // Autoriser le clic uniquement si la carte est dans la main du joueur courant
            if (pb.Parent != flpJoueur1 && pb.Parent != flpJoueur2 && !modeLeurre)
            {
                // Sauf si on est en mode Leurre (cas déjà géré plus bas)
                
                    return;
            }
            //Si le mode Leurre est activé, on cherche à remplacer une carte déjà posée par un Leurre

            if (modeLeurre)
            {
                PictureBox picturebox = sender as PictureBox;
                if (picturebox != null && picturebox.Tag is Carte c)
                {
                    // Vérifie que la carte appartient au joueur courant, n'est pas un Leurre, et n'est pas déjà la carte Leurre
                    FlowLayoutPanel zoneCarte = picturebox.Parent as FlowLayoutPanel;
                    bool estZoneJoueur =
                        (joueurCourant == jeu.Joueur1 && (zoneCarte == flpMeleeJ1 || zoneCarte == flpDistanceJ1 || zoneCarte == flpSiegeJ1)) ||
                        (joueurCourant == jeu.Joueur2 && (zoneCarte == flpMeleeJ2 || zoneCarte == flpDistanceJ2 || zoneCarte == flpSiegeJ2));
                    if (estZoneJoueur && c.Pouvoir != PouvoirSpecial.Leurre && c != carteLeurre && zoneCarte == zoneLeurre)
                    {
                        // Retirer le Leurre déjà posé sur la zone (il y en a forcément un, c'est le dernier ajouté)
                        PictureBox leurreExistant = null;
                        foreach (Control ctrl in zoneCarte.Controls)
                        {
                            if (ctrl is PictureBox pictureboite && pictureboite.Tag is Carte carte && carte == carteLeurre)
                            {
                                leurreExistant = pictureboite;
                                break;
                            }
                        }
                        // Si on en trouve un, on le retire
                        if (leurreExistant != null)
                            zoneCarte.Controls.Remove(leurreExistant);

                        // Trouver l'index de la carte à remplacer
                        int index = zoneCarte.Controls.GetChildIndex(picturebox);

                        // Retirer la carte à remplacer et la remettre dans la main
                        zoneCarte.Controls.Remove(picturebox);
                        joueurCourant.Main.Add(c);

                        // Créer le PictureBox du Leurre et l'insérer à la bonne position
                        PictureBox pbLeurre = new PictureBox
                        {
                            Width = 100,
                            Height = 150,
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Image = Image.FromFile(carteLeurre.ImagePath),
                            Tag = carteLeurre
                        };
                        //pbLeurre.Click += Pb_Click;

                        // Ajouter le Leurre dans la zone à la même position que la carte remplacée
                        zoneCarte.Controls.Add(pbLeurre);
                        zoneCarte.Controls.SetChildIndex(pbLeurre, index);

                        // Mettre à jour le score de la zone (points affichés)
                        MettreAJourScoreZone(zoneCarte, LienLabelZone(zoneCarte));
                        ChargerCarte((joueurCourant == jeu.Joueur1) ? flpJoueur1 : flpJoueur2, joueurCourant);

                        // Recharger les cartes dans la main du joueur
                        MessageBox.Show($"Leurre : {c.Nom} retourne dans votre main !");

                        // On sort du mode Leurre et on libère la mémoire de la carteLeurre
                        modeLeurre = false;
                        carteLeurre = null;
                        zoneLeurre = null;

                        // Mise à jour de l’affichage des decks/cimetières et passage au tour suivant
                        MettreAJourDeckEtCimetiere();
                        PasserAuTourSuivant();
                        return;
                    }
                    else
                    {
                        // Si la carte cliquée ne peut pas être remplacée
                        MessageBox.Show("Vous ne pouvez remplacer qu'une carte sur la même zone par le Leurre.");
                        return;
                    }
                }
            }

            // Si on n'est pas en mode Leurre, il s'agit d'une simple sélection de carte dans la main
            //PictureBox pb = sender as PictureBox;
            if (pb != null)
            {
                // Si on reclique sur la même carte, on la désélectionne
                if (carteSelectionnee == pb)
                {
                    carteSelectionnee.BorderStyle = BorderStyle.None;
                    carteSelectionnee.Size = new Size(80, 120);
                    carteSelectionnee.BackColor = Color.Transparent;
                    carteSelectionnee.Padding = new Padding(0);
                    carteSelectionnee = null;
                    choix = null;

                    // Réinitialiser les zones de jeu (retire le surlignage)
                    ResetZones(flpMeleeJ1);
                    ResetZones(flpMeleeJ2);
                    ResetZones(flpDistanceJ1);
                    ResetZones(flpDistanceJ2);
                    ResetZones(flpSiegeJ1);
                    ResetZones(flpSiegeJ2);

                    return;
                }

                // Si une autre carte était déjà sélectionnée, on enlève l’effet visuel
                if (carteSelectionnee != null)
                {
                    carteSelectionnee.BorderStyle = BorderStyle.None;
                    carteSelectionnee.Size = new Size(80, 120);
                    carteSelectionnee.BackColor = Color.Transparent;
                    carteSelectionnee.Padding = new Padding(0);

                }

                // Appliquer l'effet visuel à la nouvelle sélection
                carteSelectionnee = pb;
                carteSelectionnee.BorderStyle = BorderStyle.FixedSingle; // Effet de sélection
                carteSelectionnee.Size = new Size(85, 125);
                carteSelectionnee.BackColor = Color.Yellow; // Fond légèrement coloré
                carteSelectionnee.Padding = new Padding(4); // Effet d'ombre


                // Mettre à jour la carte sélectionnée
                choix = pb.Tag as Carte;

                // Surligner les zones valides pour poser cette carte
                HighlightZones(choix);

                ActiverZones();
            }
        }

        private void HighlightZones(Carte carte)
        {
            // Réinitialise toutes les zones de jeu (enlève les surbrillances éventuelles)
            FlowLayoutPanel[] toutesLesZones = new[]
            {
                flpMeleeJ1, flpMeleeJ2,
                flpDistanceJ1, flpDistanceJ2,
                flpSiegeJ1, flpSiegeJ2,
                flpEffetArcherJ1, flpEffetArcherJ2,
                flpEffetMeleeJ1, flpEffetMeleeJ2,
                flpEffetSiegeJ1, flpEffetSiegeJ2,
                flpMeteoMeleeJ1, flpMeteoMeleeJ2,
                flpMeteoDistanceJ1, flpMeteoDistanceJ2,
                flpMeteoSiegeJ1, flpMeteoSiegeJ2,
            };

            foreach (var zone in toutesLesZones)
                ResetZones(zone);

            if (carte.Pouvoir == PouvoirSpecial.Espion)
            {
                // On surligne la zone de l'adversaire (pas du joueur courant)
                Joueur adversaire = (joueurCourant == jeu.Joueur1) ? jeu.Joueur2 : jeu.Joueur1;
                if (adversaire == jeu.Joueur1)
                {
                    if (carte.Type == TypeCarte.Melee)
                        SurbrillanceZone(flpMeleeJ1);
                    else if (carte.Type == TypeCarte.Distance)
                        SurbrillanceZone(flpDistanceJ1);
                    else if (carte.Type == TypeCarte.Siege)
                        SurbrillanceZone(flpSiegeJ1);
                }
                else
                {
                    if (carte.Type == TypeCarte.Melee)
                        SurbrillanceZone(flpMeleeJ2);
                    else if (carte.Type == TypeCarte.Distance)
                        SurbrillanceZone(flpDistanceJ2);
                    else if (carte.Type == TypeCarte.Siege)
                        SurbrillanceZone(flpSiegeJ2);
                }
                return;
            }

            // --- Reste du comportement standard ---
            if (carte.Pouvoir == PouvoirSpecial.Agile)
            {
                if (joueurCourant == jeu.Joueur1)
                {
                    SurbrillanceZone(flpMeleeJ1);
                    SurbrillanceZone(flpDistanceJ1);
                }
                else
                {
                    SurbrillanceZone(flpMeleeJ2);
                    SurbrillanceZone(flpDistanceJ2);
                }
            }
            else if (carte.Type == TypeCarte.Melee)
            {
                if (joueurCourant == jeu.Joueur1)
                    SurbrillanceZone(flpMeleeJ1);
                else
                    SurbrillanceZone(flpMeleeJ2);
            }
            else if (carte.Type == TypeCarte.Distance)
            {
                if (joueurCourant == jeu.Joueur1)
                    SurbrillanceZone(flpDistanceJ1);
                else
                    SurbrillanceZone(flpDistanceJ2);
            }
            else if (carte.Type == TypeCarte.Siege)
            {
                if (joueurCourant == jeu.Joueur1)
                    SurbrillanceZone(flpSiegeJ1);
                else
                    SurbrillanceZone(flpSiegeJ2);
            }
            else if (carte.Type == TypeCarte.Effet)
            {
                if (joueurCourant == jeu.Joueur1)
                {
                    SurbrillanceZone(flpEffetMeleeJ1);
                    SurbrillanceZone(flpEffetArcherJ1);
                    SurbrillanceZone(flpEffetSiegeJ1);
                }
                else
                {
                    SurbrillanceZone(flpEffetMeleeJ2);
                    SurbrillanceZone(flpEffetArcherJ2);
                    SurbrillanceZone(flpEffetSiegeJ2);
                }
            }
            else if (carte.Pouvoir == PouvoirSpecial.Leurre)
            {
                if (joueurCourant == jeu.Joueur1)
                {
                    SurbrillanceZone(flpMeleeJ1);
                    SurbrillanceZone(flpDistanceJ1);
                    SurbrillanceZone(flpSiegeJ1);
                }
                else
                {
                    SurbrillanceZone(flpMeleeJ2);
                    SurbrillanceZone(flpDistanceJ2);
                    SurbrillanceZone(flpSiegeJ2);
                }
            }
            else if (carte.Type == TypeCarte.Meteo)
            {
                if (joueurCourant == jeu.Joueur1)
                {
                    if (carte.Pouvoir == PouvoirSpecial.Gel)
                        SurbrillanceZone(flpMeteoMeleeJ2);
                    else if (carte.Pouvoir == PouvoirSpecial.Brouillard)
                        SurbrillanceZone(flpMeteoDistanceJ2);
                    else if (carte.Pouvoir == PouvoirSpecial.Pluie)
                        SurbrillanceZone(flpMeteoSiegeJ2);
                }
                else
                {
                    if (carte.Pouvoir == PouvoirSpecial.Gel)
                        SurbrillanceZone(flpMeteoMeleeJ1);
                    else if (carte.Pouvoir == PouvoirSpecial.Brouillard)
                        SurbrillanceZone(flpMeteoDistanceJ1);
                    else if (carte.Pouvoir == PouvoirSpecial.Pluie)
                        SurbrillanceZone(flpMeteoSiegeJ1);
                }
            }

        }


        // Méthode qui active la surbrillance visuelle d'une zone donnée (si compatible)
        private void SurbrillanceZone(FlowLayoutPanel zone)
        {
            // On tente de convertir la zone en FlowLayoutPanelSurbrillance (classe personnalisée)
            var z = zone as FlowLayoutPanelSurbrillance;

            // Si la conversion a réussi (donc la zone supporte la surbrillance)
            if (z != null)
            {
                z.Surbrillance = true;
                z.Invalidate(); // Redessiner la zone pour appliquer la surbrillance
            }
        }


        // Méthode qui désactive la surbrillance visuelle d'une zone donnée (si compatible)
        private void ResetZones(FlowLayoutPanel zone)
        {
            var z = zone as FlowLayoutPanelSurbrillance;
            if (z != null)
            {
                z.Surbrillance = false; // Désactiver la surbrillance
                z.Invalidate(); // Redessiner la zone pour enlever la surbrillance
            }
        }

        // Méthode qui tente de placer la carte actuellement sélectionnée (choix) dans une zone donnée du plateau
        private void PlacerCarteDansZone(FlowLayoutPanel zoneCible)
        {
            if (choix != null)
            {
                bool zoneAutorisee = false;

                // --- Cas Espion : la carte se pose sur la zone du joueur courant ---
                if (choix.Pouvoir == PouvoirSpecial.Espion)
                {
                    // La carte se pose sur la zone de l'adversaire
                    Joueur adversaire = (joueurCourant == jeu.Joueur1) ? jeu.Joueur2 : jeu.Joueur1;
                    if (adversaire == jeu.Joueur1)
                    {
                        if (choix.Type == TypeCarte.Melee && zoneCible == flpMeleeJ1) zoneAutorisee = true;
                        else if (choix.Type == TypeCarte.Distance && zoneCible == flpDistanceJ1) zoneAutorisee = true;
                        else if (choix.Type == TypeCarte.Siege && zoneCible == flpSiegeJ1) zoneAutorisee = true;
                    }
                    else
                    {
                        if (choix.Type == TypeCarte.Melee && zoneCible == flpMeleeJ2) zoneAutorisee = true;
                        else if (choix.Type == TypeCarte.Distance && zoneCible == flpDistanceJ2) zoneAutorisee = true;
                        else if (choix.Type == TypeCarte.Siege && zoneCible == flpSiegeJ2) zoneAutorisee = true;
                    }
                }

                else
                {
                    // ... (le reste du code standard inchangé pour les autres cartes)
                    if (joueurCourant == jeu.Joueur1)
                    {
                        if ((choix.Type == TypeCarte.Melee && zoneCible == flpMeleeJ1) ||
                            (choix.Type == TypeCarte.Distance && zoneCible == flpDistanceJ1) ||
                            (choix.Type == TypeCarte.Siege && zoneCible == flpSiegeJ1) ||
                            (choix.Type == TypeCarte.Effet && zoneCible == flpEffetMeleeJ1) ||
                            (choix.Type == TypeCarte.Effet && zoneCible == flpEffetArcherJ1) ||
                            (choix.Type == TypeCarte.Effet && zoneCible == flpEffetSiegeJ1) ||
                            ((choix.Type == TypeCarte.Meteo && choix.Pouvoir == PouvoirSpecial.Gel && zoneCible == flpMeteoMeleeJ2) ||
                            (choix.Type == TypeCarte.Meteo && choix.Pouvoir == PouvoirSpecial.Brouillard && zoneCible == flpMeteoDistanceJ2) ||
                            (choix.Type == TypeCarte.Meteo && choix.Pouvoir == PouvoirSpecial.Pluie && zoneCible == flpMeteoSiegeJ2)) ||
                            (choix.Pouvoir == PouvoirSpecial.Leurre && (zoneCible == flpMeleeJ1 || zoneCible == flpDistanceJ1 || zoneCible == flpSiegeJ1)) ||
                            (choix.Pouvoir == PouvoirSpecial.Agile && (zoneCible == flpMeleeJ1 || zoneCible == flpDistanceJ1)))
                        {
                            zoneAutorisee = true;
                        }
                    }
                    else
                    {
                        if ((choix.Type == TypeCarte.Melee && zoneCible == flpMeleeJ2) ||
                            (choix.Type == TypeCarte.Distance && zoneCible == flpDistanceJ2) ||
                            (choix.Type == TypeCarte.Siege && zoneCible == flpSiegeJ2) ||
                            (choix.Type == TypeCarte.Effet && zoneCible == flpEffetMeleeJ2) ||
                            (choix.Type == TypeCarte.Effet && zoneCible == flpEffetArcherJ2) ||
                            (choix.Type == TypeCarte.Effet && zoneCible == flpEffetSiegeJ2) ||
                            ((choix.Type == TypeCarte.Meteo && choix.Pouvoir == PouvoirSpecial.Gel && zoneCible == flpMeteoMeleeJ1) ||
                            (choix.Type == TypeCarte.Meteo && choix.Pouvoir == PouvoirSpecial.Brouillard && zoneCible == flpMeteoDistanceJ1) ||
                            (choix.Type == TypeCarte.Meteo && choix.Pouvoir == PouvoirSpecial.Pluie && zoneCible == flpMeteoSiegeJ1)) ||
                            (choix.Pouvoir == PouvoirSpecial.Leurre && (zoneCible == flpMeleeJ2 || zoneCible == flpDistanceJ2 || zoneCible == flpSiegeJ2)) ||
                            (choix.Pouvoir == PouvoirSpecial.Agile && (zoneCible == flpMeleeJ2 || zoneCible == flpDistanceJ2)))
                        {
                            zoneAutorisee = true;
                        }
                    }
                }

                if (zoneAutorisee)
                {
                    var pouvoirAvant = choix.Pouvoir;

                    // Cas météo 
                    if (choix.Type == TypeCarte.Meteo)
                    {
                        // Place la carte météo dans la zone adverse (zoneCible)
                        Carte carteEffet = new Carte(choix.Nom, choix.Puissance, choix.ImagePath, choix.Type, choix.Pouvoir);
                        PictureBox pbEffet = new PictureBox
                        {
                            Width = 100,
                            Height = 150,
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Image = Image.FromFile(carteEffet.ImagePath),
                            Tag = carteEffet
                        };
                        toolTipCarte.SetToolTip(pbEffet, $"Nom : {carteEffet.Nom}\nPuissance : {carteEffet.Puissance}\nPouvoir : {carteEffet.Pouvoir}");
                        pbEffet.Click += Pb_Click;
                        zoneCible.Controls.Add(pbEffet);


                        // Place une copie dans la zone météo du joueur
                        FlowLayoutPanel zoneMeteoJoueur = null;
                        if (joueurCourant == jeu.Joueur1)
                        {
                            if (choix.Pouvoir == PouvoirSpecial.Gel)
                                zoneMeteoJoueur = flpMeteoMeleeJ1;
                            else if (choix.Pouvoir == PouvoirSpecial.Brouillard)
                                zoneMeteoJoueur = flpMeteoDistanceJ1;
                            else if (choix.Pouvoir == PouvoirSpecial.Pluie)
                                zoneMeteoJoueur = flpMeteoSiegeJ1;
                        }
                        else
                        {
                            if (choix.Pouvoir == PouvoirSpecial.Gel)
                                zoneMeteoJoueur = flpMeteoMeleeJ2;
                            else if (choix.Pouvoir == PouvoirSpecial.Brouillard)
                                zoneMeteoJoueur = flpMeteoDistanceJ2;
                            else if (choix.Pouvoir == PouvoirSpecial.Pluie)
                                zoneMeteoJoueur = flpMeteoSiegeJ2;
                        }
                        if (zoneMeteoJoueur != null)
                        {
                            Carte carteCopie = new Carte(choix.Nom, choix.Puissance, choix.ImagePath, choix.Type, choix.Pouvoir);
                            PictureBox pbCopie = new PictureBox
                            {
                                Width = 100,
                                Height = 150,
                                SizeMode = PictureBoxSizeMode.StretchImage,
                                Image = Image.FromFile(carteCopie.ImagePath),
                                Tag = carteCopie
                            };
                            toolTipCarte.SetToolTip(pbCopie, $"Nom : {carteCopie.Nom}\nPuissance : {carteCopie.Puissance}\nPouvoir : {carteCopie.Pouvoir}");
                            pbCopie.Click += Pb_Click;
                            zoneMeteoJoueur.Controls.Add(pbCopie);

                        }

                        joueurCourant.Main.Remove(choix);
                        ChargerCarte((joueurCourant == jeu.Joueur1) ? flpJoueur1 : flpJoueur2, joueurCourant);

                        lancerPouvoirSpeciale(choix, zoneCible, joueurCourant);

                        choix = null;
                        ActiverZones();

                        FlowLayoutPanel[] toutesLesZones = new[]
                        {
                            flpMeleeJ1, flpMeleeJ2,
                            flpDistanceJ1, flpDistanceJ2,
                            flpSiegeJ1, flpSiegeJ2,
                            flpEffetArcherJ1, flpEffetArcherJ2,
                            flpEffetMeleeJ1, flpEffetMeleeJ2,
                            flpEffetSiegeJ1, flpEffetSiegeJ2,
                            flpMeteoMeleeJ1, flpMeteoMeleeJ2,
                            flpMeteoDistanceJ1, flpMeteoDistanceJ2,
                            flpMeteoSiegeJ1, flpMeteoSiegeJ2,
                        };
                        foreach (var zone in toutesLesZones)
                            ResetZones(zone);

                        PasserAuTourSuivant();
                        return;
                    }

                    // comportement standard
                    PictureBox pb = new PictureBox
                    {
                        Width = 100,
                        Height = 150,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = Image.FromFile(choix.ImagePath),
                        Tag = choix
                    };
                    toolTipCarte.SetToolTip(pb, $"Nom : {choix.Nom}\nPuissance : {choix.Puissance}\nPouvoir : {choix.Pouvoir}");

                    pb.Click += Pb_Click;
                    zoneCible.Controls.Add(pb);


                    Label labelZone = LienLabelZone(zoneCible);
                    if (labelZone != null)
                        MettreAJourScoreZone(zoneCible, labelZone);

                    joueurCourant.Main.Remove(choix);
                    ChargerCarte((joueurCourant == jeu.Joueur1) ? flpJoueur1 : flpJoueur2, joueurCourant);

                    lancerPouvoirSpeciale(choix, zoneCible, joueurCourant);

                    choix = null;
                    ActiverZones();

                    FlowLayoutPanel[] toutesLesZones2 = new[]
                    {
                        flpMeleeJ1, flpMeleeJ2,
                        flpDistanceJ1, flpDistanceJ2,
                        flpSiegeJ1, flpSiegeJ2,
                        flpEffetArcherJ1, flpEffetArcherJ2,
                        flpEffetMeleeJ1, flpEffetMeleeJ2,
                        flpEffetSiegeJ1, flpEffetSiegeJ2,
                        flpMeteoMeleeJ1, flpMeteoMeleeJ2,
                        flpMeteoDistanceJ1, flpMeteoDistanceJ2,
                        flpMeteoSiegeJ1, flpMeteoSiegeJ2,
                    };
                    foreach (var zone in toutesLesZones2)
                        ResetZones(zone);

                    if (pouvoirAvant != PouvoirSpecial.Leurre)
                        PasserAuTourSuivant();

                }
                else
                {
                    MessageBox.Show("Vous ne pouvez pas jouer ici");
                }
            }
        }

        //  zone de jeu (FlowLayoutPanel) est cliquée par le joueur
        private void ZoneCible_Click(object sender, EventArgs e)
        {
            // Convertit l'expéditeur de l'événement en FlowLayoutPanel (zone cliquée)
            FlowLayoutPanel zoneCible = sender as FlowLayoutPanel;

            // Vérifie que la zone est bien cliquée et qu'une carte est actuellement sélectionnée (choix)
            if (zoneCible != null && carteSelectionnee != null)
            {
                // Tente de placer la carte sélectionnée dans la zone cliquée
                PlacerCarteDansZone(zoneCible);
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Si la touche appuyée est Échap (Escape)
            if (keyData == Keys.Escape)
            {
                BasculerPleinEcran();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Méthode pour basculer entre le mode plein écran et le mode fenêtré
        private void BasculerPleinEcran()
        {
            if (enPleinEcran)
            {

                this.FormBorderStyle = FormBorderStyle.Sizable; 
                this.WindowState = FormWindowState.Normal; 
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None; 
                this.WindowState = FormWindowState.Maximized; 
            }

            // Inverser l'état du booléen pour refléter le nouveau mode d'affichage
            enPleinEcran = !enPleinEcran;
        }


        // Met à jour le score d'une zone donnée (FlowLayoutPanel) et l'affiche dans un label
        private void MettreAJourScoreZone(FlowLayoutPanel zone, Label label)
        {
            // Regroupe les cartes par nom pour appliquer Lien Etroits
            var cartes = new List<Carte>();
            foreach (Control ctrl in zone.Controls)
            {
                if (ctrl is PictureBox pb && pb.Tag is Carte carte)
                {
                    cartes.Add(carte);
                }
            }

            int score = 0;

            // On regroupe par nom pour appliquer le bonus
            var groupes = cartes.GroupBy(c => c.Nom);

            foreach (var groupe in groupes)
            {
                // Nombre de cartes dans le groupe
                int count = groupe.Count();

                // Présence du pouvoir "Lien Etroits"
                bool lienEtroits = groupe.Any(c => c.Pouvoir == PouvoirSpecial.LienEtroits);

                // Vérifie si un effet météo est actif sur la zone
                bool meteoActive = false;
                if ((zone == flpMeleeJ1 && meteoFroidMordantJ1) || (zone == flpMeleeJ2 && meteoFroidMordantJ2))
                    meteoActive = true;
                if ((zone == flpDistanceJ1 && meteoBrouillardJ1) || (zone == flpDistanceJ2 && meteoBrouillardJ2))
                    meteoActive = true;
                if ((zone == flpSiegeJ1 && meteoPluieJ1) || (zone == flpSiegeJ2 && meteoPluieJ2))
                    meteoActive = true;

                // Calcul du score selon le pouvoir "Lien Etroits" et la météo
                if (lienEtroits && count > 1)
                {
                    if (meteoActive)
                    {
                        // Leurre doit toujours rester à 0, même sous météo
                        int nbCartesNonLeurre = groupe.Count(c => c.Pouvoir != PouvoirSpecial.Leurre);
                        score += nbCartesNonLeurre * count; // chaque carte non-Leurre puissance 1, multipliée par le nombre de cartes identiques
                                                            // Les Leurre n'ajoutent rien
                    }
                    else
                    {
                        score += groupe.Where(c => c.Pouvoir != PouvoirSpecial.Leurre).Sum(c => c.Puissance) * count;
                        // Les Leurre n'ajoutent rien
                    }
                }
                else
                {
                    if (meteoActive)
                    {
                        // Chaque carte non-Leurre puissance 1, Leurre = 0
                        score += groupe.Count(c => c.Pouvoir != PouvoirSpecial.Leurre);
                    }
                    else
                    {
                        score += groupe.Where(c => c.Pouvoir != PouvoirSpecial.Leurre).Sum(c => c.Puissance);
                    }
                }

            }


            // Appliquer le bonus Boost Morale (+1 à chaque carte sauf Boost Morale) si au moins une carte Boost Morale est présente
            bool boostMoraleActif = cartes.Any(c => c.Pouvoir == PouvoirSpecial.BoostMorale);
            if (boostMoraleActif)
            {
                int nbCartesBoostees = cartes.Count(c => c.Pouvoir != PouvoirSpecial.BoostMorale);
                score += nbCartesBoostees;// Ajoute 1 par carte boostée
            }

            // Appliquer l'effet Charge si actif sur la zone
            bool chargeActive = false;
            if (zone == flpMeleeJ1) chargeActive = chargeActiveMeleeJ1;
            else if (zone == flpDistanceJ1) chargeActive = chargeActiveDistanceJ1;
            else if (zone == flpSiegeJ1) chargeActive = chargeActiveSiegeJ1;
            else if (zone == flpMeleeJ2) chargeActive = chargeActiveMeleeJ2;
            else if (zone == flpDistanceJ2) chargeActive = chargeActiveDistanceJ2;
            else if (zone == flpSiegeJ2) chargeActive = chargeActiveSiegeJ2;

            if (chargeActive)
                score *= 2;// Double le score

            // Affiche le score dans le label associé à la zone
            label.Text = score.ToString();

            // Met à jour le score total du joueur
            MettreAJourScore();
        }


        // Retourne le label associé à une zone donnée (FlowLayoutPanel)
        private Label LienLabelZone(FlowLayoutPanel zone)
        {
            if (zone == flpMeleeJ1) return lScoreMeleeJ1;
            if (zone == flpMeleeJ2) return lScoreMeleeJ2;
            if (zone == flpDistanceJ1) return lScoreDistanceJ1;
            if (zone == flpDistanceJ2) return lScoreDistanceJ2;
            if (zone == flpSiegeJ1) return lScoreSiegeJ1;
            if (zone == flpSiegeJ2) return lScoreSiegeJ2;
            return null; // Si aucune zone ne correspond
        }

        

        // Met à jour le score total affiché pour chaque joueur
        private void MettreAJourScore()
        {
            // Calcule le score total du Joueur 1 en additionnant les scores des zones Mêlée, Distance et Siège
            int scoreJ1 = int.Parse(lScoreMeleeJ1.Text) + int.Parse(lScoreDistanceJ1.Text) + int.Parse(lScoreSiegeJ1.Text);

            // Met à jour le label affichant le score total du Joueur 1
            lScoreJoueur1.Text = scoreJ1.ToString();

            int scoreJ2 = int.Parse(lScoreMeleeJ2.Text) + int.Parse(lScoreDistanceJ2.Text) + int.Parse(lScoreSiegeJ2.Text);
            lScoreJoueur2.Text = scoreJ2.ToString();
        }

        private void PasserAuTourSuivant()
        {
            // Si les deux joueurs ont passé leur tour, la manche se termine
            if (joueur1Passe && joueur2Passe)
            {
                FinDeManche();
                return;
            }

            // Si un joueur a passé, l'autre peut continuer à jouer tant qu'il veut
            if (joueur1Passe && !joueur2Passe)
                joueurCourant = jeu.Joueur2;
            else if (joueur2Passe && !joueur1Passe)
                joueurCourant = jeu.Joueur1;
            else
                joueurCourant = (joueurCourant == jeu.Joueur1) ? jeu.Joueur2 : jeu.Joueur1;

            ActiverZones();
            ChargerCarte(flpJoueur1, jeu.Joueur1);
            ChargerCarte(flpJoueur2, jeu.Joueur2);
        }

        private void bPasserJ1_Click(object sender, EventArgs e)
        {
            joueur1Passe = true;
            PasserAuTourSuivant();

        }

        private void bPasserJ2_Click(object sender, EventArgs e)
        {
            joueur2Passe = true;
            PasserAuTourSuivant();

        }

        private void FinDeManche()
        {
            // Récupération des scores actuels affichés pour chaque joueur
            int scoreJ1 = int.Parse(lScoreJoueur1.Text);
            int scoreJ2 = int.Parse(lScoreJoueur2.Text);

            string message;
            perdantDerniereManche = null;

            // Détermine quel joueur a gagné la manche
            if (scoreJ1 > scoreJ2)
            {
                viesJoueur2--;
                message = "Joueur 1 gagne la manche !";
                perdantDerniereManche = jeu.Joueur2;
            }
            else if (scoreJ2 > scoreJ1)
            {
                viesJoueur1--;
                message = "Joueur 2 gagne la manche !";
                perdantDerniereManche = jeu.Joueur1;
            }
            else
            {
                // Pouvoir passif Nilfgaard : gagne la manche en cas d'égalité
                if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.Nilfgaard && jeu.Joueur2.PouvoirPassif != Jeu.PouvoirPassifDeck.Nilfgaard)
                {
                    viesJoueur2--;
                    message = "Égalité ! Pouvoir Nilfgaard : Joueur 1 (Nilfgaard) gagne la manche !";
                    perdantDerniereManche = jeu.Joueur2;
                }
                else if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.Nilfgaard && jeu.Joueur1.PouvoirPassif != Jeu.PouvoirPassifDeck.Nilfgaard)
                {
                    viesJoueur1--;
                    message = "Égalité ! Pouvoir Nilfgaard : Joueur 2 (Nilfgaard) gagne la manche !";
                    perdantDerniereManche = jeu.Joueur1;
                }
                else
                {
                    viesJoueur1--;
                    viesJoueur2--;
                    message = "Égalité ! Les deux joueurs perdent une vie.";
                    perdantDerniereManche = (random.Next(2) == 0) ? jeu.Joueur1 : jeu.Joueur2;
                }
            }

            MessageBox.Show($"{message}\nVies Joueur 1 : {viesJoueur1} | Vies Joueur 2 : {viesJoueur2}");

            // Pouvoir passif Royaumes du Nord : pioche une carte en cas de victoire de manche
            if (scoreJ1 > scoreJ2 && jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.RoyaumesDuNord)
            {
                jeu.Joueur1.Piocher();
                MessageBox.Show("Pouvoir Royaumes du Nord : Vous piochez une carte !");
                ChargerCarte(flpJoueur1, jeu.Joueur1);
                MettreAJourDeckEtCimetiere();
            }
            else if (scoreJ2 > scoreJ1 && jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.RoyaumesDuNord)
            {
                jeu.Joueur2.Piocher();
                MessageBox.Show("Pouvoir Royaumes du Nord : Vous piochez une carte !");
                ChargerCarte(flpJoueur2, jeu.Joueur2);
                MettreAJourDeckEtCimetiere();
            }

            // Pour le joueur 1
            if (viesJoueur1 == 1)
                pbVieJ1_2.Visible = false;
            else if (viesJoueur1 == 0)
                pbVieJ1_1.Visible = false;

            // Pour le joueur 2
            if (viesJoueur2 == 1)
                pbVieJ2_2.Visible = false;
            else if (viesJoueur2 == 0)
                pbVieJ2_1.Visible = false;

            // Si un joueur n'a plus de vie, la partie est terminée
            if (viesJoueur1 == 0 || viesJoueur2 == 0)
            {
                string gagnant = (viesJoueur1 > viesJoueur2) ? "Joueur 1" : "Joueur 2";
                MessageBox.Show($"{gagnant} remporte la partie !");
                this.Close();
                return;
            }

            // Envoie les cartes du plateau au cimetière (fin de manche)
            // Vide les zones des effets actifs
            EnvoyerPlateauAuCimetiere();
            ViderZonesEffet();

            // Remettre à zéro les scores de zones et totaux
            lScoreMeleeJ1.Text = "0";
            lScoreDistanceJ1.Text = "0";
            lScoreSiegeJ1.Text = "0";
            lScoreMeleeJ2.Text = "0";
            lScoreDistanceJ2.Text = "0";
            lScoreSiegeJ2.Text = "0";
            lScoreJoueur1.Text = "0";
            lScoreJoueur2.Text = "0";

            chargeActiveMeleeJ1 = false;
            chargeActiveDistanceJ1 = false;
            chargeActiveSiegeJ1 = false;
            chargeActiveMeleeJ2 = false;
            chargeActiveDistanceJ2 = false;
            chargeActiveSiegeJ2 = false;

            // Préparer la manche suivante
            joueur1Passe = false;
            joueur2Passe = false;

            // Le joueur qui a perdu la dernière manche commence la manche suivante
            joueurCourant = perdantDerniereManche;

            MessageBox.Show($"{(joueurCourant == jeu.Joueur1 ? "Joueur 1" : "Joueur 2")} commence la nouvelle manche !");

            // Réactive les zones de jeu et recharge les cartes des deux joueurs dans l'interface
            ActiverZones();
            ChargerCarte(flpJoueur1, jeu.Joueur1);
            ChargerCarte(flpJoueur2, jeu.Joueur2);
        }


        private void lancerPouvoirSpeciale(Carte carte, FlowLayoutPanel zone, Joueur joueur)
        {
            if (carte.Pouvoir == PouvoirSpecial.Aucun)
            {
                return; // Aucun pouvoir spécial à lancer
            }

            // En fonction du pouvoir spécial de la carte, on exécute un comportement spécifique
            switch (carte.Pouvoir)
            {

                // Le pouvoir "Medic" permet de ramener une carte du cimetière à la main du joueur
                case PouvoirSpecial.Medic:
                    if (joueur.Cimetiere.Count > 0)
                    {
                        using (var formCimetiere = new FormCimetiere(joueur.Cimetiere))
                        {
                            if (formCimetiere.ShowDialog() == DialogResult.OK && formCimetiere.CarteChoisie != null)
                            {
                                Carte carteRessuscitee = formCimetiere.CarteChoisie;
                                joueur.Cimetiere.Remove(carteRessuscitee);

                                // Trouver la zone cible selon le type de la carte
                                FlowLayoutPanel zoneCible = null;
                                if (carteRessuscitee.Type == TypeCarte.Melee)
                                    zoneCible = (joueur == jeu.Joueur1) ? flpMeleeJ1 : flpMeleeJ2;
                                else if (carteRessuscitee.Type == TypeCarte.Distance)
                                    zoneCible = (joueur == jeu.Joueur1) ? flpDistanceJ1 : flpDistanceJ2;
                                else if (carteRessuscitee.Type == TypeCarte.Siege)
                                    zoneCible = (joueur == jeu.Joueur1) ? flpSiegeJ1 : flpSiegeJ2;
                                else if (carteRessuscitee.Type == TypeCarte.Effet)
                                    zoneCible = (joueur == jeu.Joueur1) ? flpEffetMeleeJ1 : flpEffetMeleeJ2; // À adapter si besoin

                                if (zoneCible != null)
                                {
                                    PictureBox pb = new PictureBox
                                    {
                                        Width = 100,
                                        Height = 150,
                                        SizeMode = PictureBoxSizeMode.StretchImage,
                                        Image = Image.FromFile(carteRessuscitee.ImagePath),
                                        Tag = carteRessuscitee
                                    };
                                    pb.Click += Pb_Click;
                                    zoneCible.Controls.Add(pb);


                                    // Met à jour le score de la zone
                                    Label labelZone = LienLabelZone(zoneCible);
                                    if (labelZone != null)
                                        MettreAJourScoreZone(zoneCible, labelZone);

                                    MessageBox.Show($"{carteRessuscitee.Nom} a été ressuscitée et placée sur le plateau !");
                                }
                                else
                                {
                                    MessageBox.Show("Type de carte non pris en charge pour la résurrection.");
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cimetière vide.");
                    }
                    MettreAJourDeckEtCimetiere();
                    break;


                case PouvoirSpecial.Espion:
                    // Le joueur pioche 2 cartes de son propre deck
                    int nbAPiocher = Math.Min(2, joueur.Deck.Count);
                    if (nbAPiocher == 0)
                    {
                        MessageBox.Show("Votre deck est vide, vous ne pouvez pas piocher de carte.");
                    }
                    else
                    {
                        for (int i = 0; i < nbAPiocher; i++)
                        {
                            joueur.Piocher();
                        }
                        MessageBox.Show($"Vous piochez {nbAPiocher} carte{(nbAPiocher > 1 ? "s" : "")} grâce à l'effet Espion !");
                        ChargerCarte((joueur == jeu.Joueur1) ? flpJoueur1 : flpJoueur2, joueur);
                        MettreAJourDeckEtCimetiere();
                    }
                    break;


                case PouvoirSpecial.Rassembler:
                    // Le pouvoir "Rassembler" permet de poser toutes les cartes identiques à celle-ci du deck et de la main sur le plateau
                    // Cherche les cartes identiques dans la main et le deck du joueur
                    var cartesIdentiquesMain = joueur.Main
                        .Where(c => c.Nom == carte.Nom && c != carte)
                        .ToList();
                    var cartesIdentiquesDeck = joueur.Deck
                        .Where(c => c.Nom == carte.Nom)
                        .ToList();

                    // Ajoute les cartes de la main sur le plateau
                    foreach (var c in cartesIdentiquesMain)
                    {
                        joueur.Main.Remove(c);
                        PictureBox pb = new PictureBox
                        {
                            Width = 100,
                            Height = 150,
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Image = Image.FromFile(c.ImagePath),
                            Tag = c
                        };
                        zone.Controls.Add(pb);

                    }

                    // Ajoute les cartes du deck sur le plateau
                    foreach (var c in cartesIdentiquesDeck)
                    {
                        joueur.Deck.Remove(c);
                        PictureBox pb = new PictureBox
                        {
                            Width = 100,
                            Height = 150,
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Image = Image.FromFile(c.ImagePath),
                            Tag = c
                        };
                        zone.Controls.Add(pb);

                    }

                    // Met à jour le score de la zone et la main
                    MettreAJourScoreZone(zone, LienLabelZone(zone));
                    ChargerCarte((joueur == jeu.Joueur1) ? flpJoueur1 : flpJoueur2, joueur);

                    if (cartesIdentiquesMain.Count + cartesIdentiquesDeck.Count > 0)
                        MessageBox.Show($"Rassembler : {cartesIdentiquesMain.Count + cartesIdentiquesDeck.Count} carte(s) identique(s) ajoutée(s) au plateau !");
                    else
                        MessageBox.Show("Rassembler : aucune autre carte identique trouvée.");
                    MettreAJourDeckEtCimetiere();
                    break;

                case PouvoirSpecial.Brulure:
                    // Liste de toutes les zones de combat
                    var zonesBrulure = new List<FlowLayoutPanel>
                    {
                        flpMeleeJ1, flpDistanceJ1, flpSiegeJ1,
                        flpMeleeJ2, flpDistanceJ2, flpSiegeJ2
                    };

                    // Récupère toutes les cartes sur le plateau (hors la carte qui active le pouvoir)
                    var cartesPlateau = new List<(FlowLayoutPanel zone, Carte carte, PictureBox pb)>();
                    foreach (var z in zonesBrulure)
                    {
                        foreach (Control ctrl in z.Controls)
                        {
                            if (ctrl is PictureBox pb && pb.Tag is Carte c && c != carte)
                                cartesPlateau.Add((z, c, pb));
                        }
                    }

                    if (cartesPlateau.Count == 0)
                    {
                        MessageBox.Show("Aucune carte à brûler !");
                        break;
                    }

                    // Trouve la puissance maximale
                    int maxPuissance = cartesPlateau.Max(t => t.carte.Puissance);

                    // Sélectionne toutes les cartes à brûler (puissance max)
                    var aBruler = cartesPlateau.Where(t => t.carte.Puissance == maxPuissance).ToList();

                    // Retire les cartes du plateau et les envoie au cimetière du bon joueur
                    foreach (var tuple in aBruler)
                    {
                        tuple.zone.Controls.Remove(tuple.pb);

                        Joueur proprio = null;
                        if (tuple.zone == flpMeleeJ1 || tuple.zone == flpDistanceJ1 || tuple.zone == flpSiegeJ1)
                            proprio = jeu.Joueur1;
                        else if (tuple.zone == flpMeleeJ2 || tuple.zone == flpDistanceJ2 || tuple.zone == flpSiegeJ2)
                            proprio = jeu.Joueur2;

                        if (proprio != null)
                            proprio.Cimetiere.Add(tuple.carte);
                    }

                    // Met à jour les scores de toutes les zones
                    MettreAJourScoreZone(flpMeleeJ1, LienLabelZone(flpMeleeJ1));
                    MettreAJourScoreZone(flpDistanceJ1, LienLabelZone(flpDistanceJ1));
                    MettreAJourScoreZone(flpSiegeJ1, LienLabelZone(flpSiegeJ1));
                    MettreAJourScoreZone(flpMeleeJ2, LienLabelZone(flpMeleeJ2));
                    MettreAJourScoreZone(flpDistanceJ2, LienLabelZone(flpDistanceJ2));
                    MettreAJourScoreZone(flpSiegeJ2, LienLabelZone(flpSiegeJ2));

                    MessageBox.Show($"Brûlure : {aBruler.Count} carte(s) la plus puissante détruite(s) !");
                    MettreAJourDeckEtCimetiere();
                    break;

                case PouvoirSpecial.Leurre:
                    // Active le mode Leurre
                    modeLeurre = true;
                    carteLeurre = carte;
                    zoneLeurre = zone;
                    MessageBox.Show("Cliquez sur une de vos cartes sur le plateau pour la remplacer par le Leurre.");
                    break;

                case PouvoirSpecial.Charge:
                    // Si la carte est posée dans un panel d'effet, on double la puissance de la zone de combat associée
                    FlowLayoutPanel zoneCombat = ZoneCombatAssociee(zone);
                    if (zoneCombat == flpMeleeJ1) chargeActiveMeleeJ1 = true;
                    else if (zoneCombat == flpDistanceJ1) chargeActiveDistanceJ1 = true;
                    else if (zoneCombat == flpSiegeJ1) chargeActiveSiegeJ1 = true;
                    else if (zoneCombat == flpMeleeJ2) chargeActiveMeleeJ2 = true;
                    else if (zoneCombat == flpDistanceJ2) chargeActiveDistanceJ2 = true;
                    else if (zoneCombat == flpSiegeJ2) chargeActiveSiegeJ2 = true;

                    if (zoneCombat != null)
                    {
                        Label labelZone = LienLabelZone(zoneCombat);
                        if (labelZone != null)
                        {
                            int scoreZone = int.Parse(labelZone.Text);
                            scoreZone *= 2;
                            labelZone.Text = scoreZone.ToString();
                            MettreAJourScore();
                        }
                    }

                    // Place la carte dans le FlowLayoutPanel d'effet correspondant
                    FlowLayoutPanel flpCharge = null;
                    if (zone == flpMeleeJ1) flpCharge = flpEffetMeleeJ1;
                    else if (zone == flpDistanceJ1) flpCharge = flpEffetArcherJ1;
                    else if (zone == flpSiegeJ1) flpCharge = flpEffetSiegeJ1;
                    else if (zone == flpMeleeJ2) flpCharge = flpEffetMeleeJ2;
                    else if (zone == flpDistanceJ2) flpCharge = flpEffetArcherJ2;
                    else if (zone == flpSiegeJ2) flpCharge = flpEffetSiegeJ2;

                    if (flpCharge != null)
                    {
                        PictureBox pbCharge = new PictureBox
                        {
                            Width = 60,
                            Height = 90,
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Image = Image.FromFile(carte.ImagePath),
                            Tag = carte
                        };
                        flpCharge.Controls.Add(pbCharge);
                    }

                    MessageBox.Show("Charge : la puissance de la zone est doublée !");
                    break;

                case PouvoirSpecial.Gel:
                    meteoFroidMordantJ1 = true;
                    meteoFroidMordantJ2 = true;
                    MettreAJourScoreZone(flpMeleeJ1, LienLabelZone(flpMeleeJ1));
                    MettreAJourScoreZone(flpMeleeJ2, LienLabelZone(flpMeleeJ2));
                    MessageBox.Show("Froid Mordant : toutes les cartes de mêlée sont réduites à 1 !");
                    break;


                case PouvoirSpecial.Brouillard:
                    meteoBrouillardJ1 = true;
                    meteoBrouillardJ2 = true;
                    MettreAJourScoreZone(flpDistanceJ1, LienLabelZone(flpDistanceJ1));
                    MettreAJourScoreZone(flpDistanceJ2, LienLabelZone(flpDistanceJ2));
                    MessageBox.Show("Brouillard : toutes les cartes de distance sont réduites à 1 !");
                    break;

                case PouvoirSpecial.Pluie:
                    meteoPluieJ1 = true;
                    meteoPluieJ2 = true;
                    MettreAJourScoreZone(flpSiegeJ1, LienLabelZone(flpSiegeJ1));
                    MettreAJourScoreZone(flpSiegeJ2, LienLabelZone(flpSiegeJ2));
                    MessageBox.Show("Pluie : toutes les cartes de siège sont réduites à 1 !");
                    break;

                case PouvoirSpecial.Soleil:
                    // On vide toutes les zones météo
                    EnvoyerZoneAuCimetiere(flpMeteoMeleeJ1, joueur);
                    EnvoyerZoneAuCimetiere(flpMeteoMeleeJ2, joueur);
                    EnvoyerZoneAuCimetiere(flpMeteoDistanceJ1, joueur);
                    EnvoyerZoneAuCimetiere(flpMeteoDistanceJ2, joueur);
                    EnvoyerZoneAuCimetiere(flpMeteoSiegeJ1, joueur);
                    EnvoyerZoneAuCimetiere(flpMeteoSiegeJ2, joueur);

                    // Désactive tous les effets météo
                    meteoFroidMordantJ1 = false;
                    meteoFroidMordantJ2 = false;
                    meteoBrouillardJ1 = false;
                    meteoBrouillardJ2 = false;
                    meteoPluieJ1 = false;
                    meteoPluieJ2 = false;

                    // Met à jour les scores des zones de combat
                    MettreAJourScoreZone(flpMeleeJ1, LienLabelZone(flpMeleeJ1));
                    MettreAJourScoreZone(flpMeleeJ2, LienLabelZone(flpMeleeJ2));
                    MettreAJourScoreZone(flpDistanceJ1, LienLabelZone(flpDistanceJ1));
                    MettreAJourScoreZone(flpDistanceJ2, LienLabelZone(flpDistanceJ2));
                    MettreAJourScoreZone(flpSiegeJ1, LienLabelZone(flpSiegeJ1));
                    MettreAJourScoreZone(flpSiegeJ2, LienLabelZone(flpSiegeJ2));

                    MessageBox.Show("Soleil : toutes les cartes météo sont retirées du plateau !");
                    break;
            }
        }

        private void EnvoyerPlateauAuCimetiere()
        {
            // Sélectionne la carte à garder pour chaque joueur Monstres 
            // On stocke la carte à garder dans le Tag de la zone correspondante
            if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.Monstres)
                GarderCarteAleatoireSurPlateau(new[] { flpMeleeJ1, flpDistanceJ1, flpSiegeJ1 });
            if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.Monstres)
                GarderCarteAleatoireSurPlateau(new[] { flpMeleeJ2, flpDistanceJ2, flpSiegeJ2 });

            // Vide toutes les zones de combat (sauf la carte à garder) 
            EnvoyerZoneAuCimetiere(flpMeleeJ1, jeu.Joueur1);
            EnvoyerZoneAuCimetiere(flpDistanceJ1, jeu.Joueur1);
            EnvoyerZoneAuCimetiere(flpSiegeJ1, jeu.Joueur1);

            EnvoyerZoneAuCimetiere(flpMeleeJ2, jeu.Joueur2);
            EnvoyerZoneAuCimetiere(flpDistanceJ2, jeu.Joueur2);
            EnvoyerZoneAuCimetiere(flpSiegeJ2, jeu.Joueur2);

            //  Remet la carte à garder sur le plateau 
            foreach (var zone in new[] { flpMeleeJ1, flpDistanceJ1, flpSiegeJ1, flpMeleeJ2, flpDistanceJ2, flpSiegeJ2 })
            {
                if (zone.Tag is PictureBox pbAGarder)
                {
                    zone.Controls.Add(pbAGarder);
                    zone.Tag = null;
                }
            }

            // --- 4. Vide les zones météo ---
            EnvoyerZoneAuCimetiere(flpMeteoMeleeJ1, jeu.Joueur1);
            EnvoyerZoneAuCimetiere(flpMeteoMeleeJ2, jeu.Joueur2);
            EnvoyerZoneAuCimetiere(flpMeteoDistanceJ1, jeu.Joueur1);
            EnvoyerZoneAuCimetiere(flpMeteoDistanceJ2, jeu.Joueur2);
            EnvoyerZoneAuCimetiere(flpMeteoSiegeJ1, jeu.Joueur1);
            EnvoyerZoneAuCimetiere(flpMeteoSiegeJ2, jeu.Joueur2);

            MettreAJourDeckEtCimetiere();
        }


        private void GarderCarteAleatoireSurPlateau(FlowLayoutPanel[] zones)
        {
            // Récupère toutes les cartes du plateau du joueur
            var cartesPlateau = new List<(FlowLayoutPanel zone, PictureBox pb)>();
            foreach (var zone in zones)
            {
                foreach (Control ctrl in zone.Controls)
                {
                    if (ctrl is PictureBox pb)
                        cartesPlateau.Add((zone, pb));
                }
            }
            if (cartesPlateau.Count == 0)
                return;

            // Tire une carte au hasard à garder
            var aGarder = cartesPlateau[random.Next(cartesPlateau.Count)];
            // Retire la carte à garder de la zone AVANT de vider la zone
            aGarder.zone.Controls.Remove(aGarder.pb);

            // Stocke la carte à garder pour la remettre après le cimetière
            aGarder.zone.Tag = aGarder.pb; // Utilise Tag pour la retrouver
        }




        private void EnvoyerZoneAuCimetiere(FlowLayoutPanel zone, Joueur joueur)
        {

            // Désactive l'effet météo Froid Mordant si on vide la zone météo mêlée
            if (zone == flpMeteoMeleeJ1)
                meteoFroidMordantJ1 = false;
            else if (zone == flpMeteoMeleeJ2)
                meteoFroidMordantJ2 = false;

            // Si c'est une zone météo mêlée, on met à jour les scores des deux zones mêlée
            if (zone == flpMeteoMeleeJ1 || zone == flpMeteoMeleeJ2)
            {
                MettreAJourScoreZone(flpMeleeJ1, LienLabelZone(flpMeleeJ1));
                MettreAJourScoreZone(flpMeleeJ2, LienLabelZone(flpMeleeJ2));
            }

            // Désactive l'effet météo Brouillard si on vide la zone météo distance
            if (zone == flpMeteoDistanceJ1)
                meteoBrouillardJ1 = false;
            else if (zone == flpMeteoDistanceJ2)
                meteoBrouillardJ2 = false;

            // Si c'est une zone météo distance, on met à jour les scores des deux zones distance
            if (zone == flpMeteoDistanceJ1 || zone == flpMeteoDistanceJ2)
            {
                MettreAJourScoreZone(flpDistanceJ1, LienLabelZone(flpDistanceJ1));
                MettreAJourScoreZone(flpDistanceJ2, LienLabelZone(flpDistanceJ2));
            }

            // Désactive l'effet météo Pluie si on vide la zone météo siège
            if (zone == flpMeteoSiegeJ1)
                meteoPluieJ1 = false;
            else if (zone == flpMeteoSiegeJ2)
                meteoPluieJ2 = false;

            // Si c'est une zone météo siège, on met à jour les scores des deux zones siège
            if (zone == flpMeteoSiegeJ1 || zone == flpMeteoSiegeJ2)
            {
                MettreAJourScoreZone(flpSiegeJ1, LienLabelZone(flpSiegeJ1));
                MettreAJourScoreZone(flpSiegeJ2, LienLabelZone(flpSiegeJ2));
            }

            var cartes = new List<Carte>();
            foreach (Control ctrl in zone.Controls)
            {
                if (ctrl is PictureBox pb && pb.Tag is Carte carte)
                {
                    cartes.Add(carte);
                }
            }
            foreach (var carte in cartes)
            {
                joueur.Cimetiere.Add(carte);
            }
            zone.Controls.Clear();
            MettreAJourDeckEtCimetiere();
        }

        private FlowLayoutPanel ZoneCombatAssociee(FlowLayoutPanel panelEffet)
        {
            if (panelEffet == flpEffetMeleeJ1) return flpMeleeJ1;
            if (panelEffet == flpEffetArcherJ1) return flpDistanceJ1;
            if (panelEffet == flpEffetSiegeJ1) return flpSiegeJ1;
            if (panelEffet == flpEffetMeleeJ2) return flpMeleeJ2;
            if (panelEffet == flpEffetArcherJ2) return flpDistanceJ2;
            if (panelEffet == flpEffetSiegeJ2) return flpSiegeJ2;
            return null;
        }

        private void ViderZonesEffet()
        {
            flpEffetMeleeJ1.Controls.Clear();
            flpEffetArcherJ1.Controls.Clear();
            flpEffetSiegeJ1.Controls.Clear();
            flpEffetMeleeJ2.Controls.Clear();
            flpEffetArcherJ2.Controls.Clear();
            flpEffetSiegeJ2.Controls.Clear();
        }

        private void bAide_Click(object sender, EventArgs e)
        {
            string message =
                @"Déroulement d'une partie :
                - Chaque joueur pioche ses cartes et joue à tour de rôle.
                - À chaque tour, posez une carte ou passez.
                - La manche se termine quand les deux joueurs passent.
                - Le joueur avec le plus de points remporte la manche.

                Effets passifs des decks :
                - Royaumes du Nord : Pioche une carte supplémentaire à chaque manche gagnée.
                - Monstres : Garde une carte aléatoire sur le plateau à la fin de chaque manche.
                - Scoia'Tael : Choisit qui commence la partie (si un seul joueur a ce deck).
                - Nilfgaard : Remporte la manche en cas d'égalité de score.

                Pouvoirs activables des decks (utilisation unique par partie) :
                - Royaumes du Nord : Retire tous les effets météo du plateau (effet Soleil).
                - Monstres : Permet de récupérer une carte du cimetière (effet Medic).
                - Scoia'Tael : Détruit la/les carte(s) la/les plus puissante(s) sur la ligne mêlée adverse (effet Brûlure sur mêlée adverse), si la puissance de la zone dépasse 10.
                - Nilfgaard : Double la puissance de votre ligne mêlée (effet Charge sur mêlée).

                Pouvoirs spéciaux des cartes :
                - Medic : Récupère une carte du cimetière.
                - Espion : Copie une carte de la main adverse.
                - Rassembler : Place toutes les cartes identiques sur le plateau.
                - Brûlure : Détruit la/les carte(s) la/les plus puissante(s) sur le plateau.
                - Leurre : Remplace une de vos cartes sur le plateau par le Leurre.
                - Charge : Double la puissance d'une zone.
                - Boost Morale : +1 à chaque carte de la zone (sauf Boost Morale).
                - Lien Etroits : Multiplie la puissance des cartes identiques dans la même zone.
                - Agile : Peut être jouée en mêlée ou distance.

                Effets météo :
                - Froid Mordant : Toutes les cartes de mêlée sont réduites à 1.
                - Brouillard : Toutes les cartes de distance sont réduites à 1.
                - Pluie : Toutes les cartes de siège sont réduites à 1.
                - Soleil : Retire tous les effets météo du plateau.

                But du jeu :
                - Remporter 2 manches en totalisant le plus de points sur le plateau.";

            using (var f = new FormAide(message))
            {
                f.ShowDialog(this);
            }
        }




        private void bPouvoirJ1_Click(object sender, EventArgs e)
        {
            // Sécurité : seul le joueur 1 peut activer ce bouton à son tour et s'il n'a pas passé
            if (joueurCourant != jeu.Joueur1 || joueur1Passe)
                return;

            if (pouvoirUtiliseJ1)
            {
                MessageBox.Show("Pouvoir déjà utilisé.");
                return;
            }
            if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.RoyaumesDuNord)
            {
                ActiverPouvoirSoleil(jeu.Joueur1);
                pouvoirUtiliseJ1 = true;
                bPouvoirJ1.Enabled = false;
            }
            else if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.Monstres)
            {
                ActiverPouvoirMedic(jeu.Joueur1);
                pouvoirUtiliseJ1 = true;
                bPouvoirJ1.Enabled = false;
            }
            else if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel)
            {
                ActiverPouvoirBrulureMeleeAdverse(jeu.Joueur1);
                pouvoirUtiliseJ1 = true;
                bPouvoirJ1.Enabled = false;
            }
            else if (jeu.Joueur1.PouvoirPassif == Jeu.PouvoirPassifDeck.Nilfgaard)
            {
                ActiverPouvoirChargeMelee(jeu.Joueur1);
                pouvoirUtiliseJ1 = true;
                bPouvoirJ1.Enabled = false;
            }
            else
            {
                MessageBox.Show("Ce deck n'a pas de pouvoir activable.");
            }
        }

        private void bPouvoirJ2_Click(object sender, EventArgs e)
        {
            // Sécurité : seul le joueur 2 peut activer ce bouton à son tour et s'il n'a pas passé
            if (joueurCourant != jeu.Joueur2 || joueur2Passe)
                return;

            if (pouvoirUtiliseJ2)
            {
                MessageBox.Show("Pouvoir déjà utilisé.");
                return;
            }
            if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.RoyaumesDuNord)
            {
                ActiverPouvoirSoleil(jeu.Joueur2);
                pouvoirUtiliseJ2 = true;
                bPouvoirJ2.Enabled = false;
            }
            else if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.Monstres)
            {
                ActiverPouvoirMedic(jeu.Joueur2);
                pouvoirUtiliseJ2 = true;
                bPouvoirJ2.Enabled = false;
            }
            else if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.ScoiaTel)
            {
                ActiverPouvoirBrulureMeleeAdverse(jeu.Joueur2);
                pouvoirUtiliseJ2 = true;
                bPouvoirJ2.Enabled = false;
            }
            else if (jeu.Joueur2.PouvoirPassif == Jeu.PouvoirPassifDeck.Nilfgaard)
            {
                ActiverPouvoirChargeMelee(jeu.Joueur2);
                pouvoirUtiliseJ2 = true;
                bPouvoirJ2.Enabled = false;
            }
            else
            {
                MessageBox.Show("Ce deck n'a pas de pouvoir activable.");
            }
        }



        private void ActiverPouvoirChargeMelee(Joueur joueur)
        {
            // Applique l'effet Charge uniquement sur la zone mêlée du joueur
            if (joueur == jeu.Joueur1)
                chargeActiveMeleeJ1 = true;
            else
                chargeActiveMeleeJ2 = true;

            // Met à jour le score de la zone mêlée
            FlowLayoutPanel zoneMelee = (joueur == jeu.Joueur1) ? flpMeleeJ1 : flpMeleeJ2;
            Label labelZone = LienLabelZone(zoneMelee);
            if (labelZone != null)
            {
                MettreAJourScoreZone(zoneMelee, labelZone);
            }

            MessageBox.Show("Pouvoir Nilfgaard : la puissance de votre ligne mêlée est doublée !");
        }


        private void ActiverPouvoirBrulureMeleeAdverse(Joueur joueur)
        {
            // Détermine la ligne mêlée de l'adversaire
            Joueur adversaire = (joueur == jeu.Joueur1) ? jeu.Joueur2 : jeu.Joueur1;
            FlowLayoutPanel flpMeleeAdverse = (adversaire == jeu.Joueur1) ? flpMeleeJ1 : flpMeleeJ2;

            // Récupère le score actuel de la zone mêlée adverse
            Label labelZone = LienLabelZone(flpMeleeAdverse);
            int scoreZone = 0;
            if (labelZone != null)
                int.TryParse(labelZone.Text, out scoreZone);

            // Vérifie que le score dépasse 10
            if (scoreZone <= 10)
            {
                MessageBox.Show("Le pouvoir Scoia'Tael ne peut être utilisé que si la zone mêlée adverse a un score strictement supérieur à 10.");
                return;
            }

            // Récupère toutes les cartes de la ligne mêlée adverse
            var cartesPlateau = new List<(Carte carte, PictureBox pb)>();
            foreach (Control ctrl in flpMeleeAdverse.Controls)
            {
                if (ctrl is PictureBox pb && pb.Tag is Carte c)
                    cartesPlateau.Add((c, pb));
            }

            if (cartesPlateau.Count == 0)
            {
                MessageBox.Show("Aucune carte à brûler sur la ligne mêlée adverse !");
                return;
            }

            // Trouve la puissance maximale
            int maxPuissance = cartesPlateau.Max(t => t.carte.Puissance);

            // Sélectionne toutes les cartes à brûler (puissance max)
            var aBruler = cartesPlateau.Where(t => t.carte.Puissance == maxPuissance).ToList();

            // Retire les cartes du plateau et les envoie au cimetière de l'adversaire
            foreach (var tuple in aBruler)
            {
                flpMeleeAdverse.Controls.Remove(tuple.pb);
                adversaire.Cimetiere.Add(tuple.carte);
            }

            // Met à jour le score de la zone mêlée adverse
            MettreAJourScoreZone(flpMeleeAdverse, LienLabelZone(flpMeleeAdverse));

            MessageBox.Show($"Pouvoir Scoia'Tael : {aBruler.Count} carte(s) la plus puissante brûlée(s) sur la ligne mêlée adverse !");
            MettreAJourDeckEtCimetiere();
        }




        private void ActiverPouvoirSoleil(Joueur joueur)
        {
            // Vide toutes les zones météo
            EnvoyerZoneAuCimetiere(flpMeteoMeleeJ1, joueur);
            EnvoyerZoneAuCimetiere(flpMeteoMeleeJ2, joueur);
            EnvoyerZoneAuCimetiere(flpMeteoDistanceJ1, joueur);
            EnvoyerZoneAuCimetiere(flpMeteoDistanceJ2, joueur);
            EnvoyerZoneAuCimetiere(flpMeteoSiegeJ1, joueur);
            EnvoyerZoneAuCimetiere(flpMeteoSiegeJ2, joueur);

            // Désactive tous les effets météo
            meteoFroidMordantJ1 = false;
            meteoFroidMordantJ2 = false;
            meteoBrouillardJ1 = false;
            meteoBrouillardJ2 = false;
            meteoPluieJ1 = false;
            meteoPluieJ2 = false;

            // Met à jour les scores des zones de combat
            MettreAJourScoreZone(flpMeleeJ1, LienLabelZone(flpMeleeJ1));
            MettreAJourScoreZone(flpMeleeJ2, LienLabelZone(flpMeleeJ2));
            MettreAJourScoreZone(flpDistanceJ1, LienLabelZone(flpDistanceJ1));
            MettreAJourScoreZone(flpDistanceJ2, LienLabelZone(flpDistanceJ2));
            MettreAJourScoreZone(flpSiegeJ1, LienLabelZone(flpSiegeJ1));
            MettreAJourScoreZone(flpSiegeJ2, LienLabelZone(flpSiegeJ2));

            MessageBox.Show("Pouvoir Royaumes du Nord : Toutes les cartes météo sont retirées du plateau !");
        }

        private void ActiverPouvoirMedic(Joueur joueur)
        {
            if (joueur.Cimetiere.Count > 0)
            {
                using (var formCimetiere = new FormCimetiere(joueur.Cimetiere))
                {
                    if (formCimetiere.ShowDialog() == DialogResult.OK && formCimetiere.CarteChoisie != null)
                    {
                        joueur.Main.Add(formCimetiere.CarteChoisie);
                        joueur.Cimetiere.Remove(formCimetiere.CarteChoisie);
                        MessageBox.Show($"{formCimetiere.CarteChoisie.Nom} a été ramenée du cimetière !");
                        ChargerCarte((joueur == jeu.Joueur1) ? flpJoueur1 : flpJoueur2, joueur);
                    }
                }
            }
            else
            {
                MessageBox.Show("Cimetière vide.");
            }
            MettreAJourDeckEtCimetiere();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Libère les images des PictureBox dans tous les panels
            var allPanels = new[] {
        flpJoueur1, flpJoueur2, flpMeleeJ1, flpMeleeJ2, flpDistanceJ1, flpDistanceJ2, flpSiegeJ1, flpSiegeJ2,
        flpEffetArcherJ1, flpEffetArcherJ2, flpEffetMeleeJ1, flpEffetMeleeJ2, flpEffetSiegeJ1, flpEffetSiegeJ2,
        flpMeteoMeleeJ1, flpMeteoMeleeJ2, flpMeteoDistanceJ1, flpMeteoDistanceJ2, flpMeteoSiegeJ1, flpMeteoSiegeJ2,
        flpCimetierre, flpCimetierre2, flpPiocheJ1, flpPiocheJ2
    };

            foreach (var panel in allPanels)
            {
                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl is PictureBox pb && pb.Image != null)
                    {
                        pb.Image.Dispose();
                        pb.Image = null;
                    }
                }
                // Libère l'image de fond du panel
                if (panel.BackgroundImage != null)
                {
                    panel.BackgroundImage.Dispose();
                    panel.BackgroundImage = null;
                }
            }

            // Libère les images des points de vie
            if (pbVieJ1_1.Image != null) { pbVieJ1_1.Image.Dispose(); pbVieJ1_1.Image = null; }
            if (pbVieJ1_2.Image != null) { pbVieJ1_2.Image.Dispose(); pbVieJ1_2.Image = null; }
            if (pbVieJ2_1.Image != null) { pbVieJ2_1.Image.Dispose(); pbVieJ2_1.Image = null; }
            if (pbVieJ2_2.Image != null) { pbVieJ2_2.Image.Dispose(); pbVieJ2_2.Image = null; }

            // Libère la musique
            if (player != null)
            {
                player.Stop();
                player.Dispose();
            }

            base.OnFormClosing(e);
        }


        private void ChargerImagePanel(FlowLayoutPanel panel, string cheminImage)
        {
            try
            {
                panel.BackgroundImage = Image.FromFile(cheminImage);
                panel.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement de l'image : " + cheminImage + "\n" + ex.Message);
            }
        }

        private void bApercuJ1_Click(object sender, EventArgs e)
        {
            if (joueurCourant != jeu.Joueur1 || joueur1Passe)
                return;

            var form = new FormDeckApercu(jeu.Joueur2.Nom, deckInitialJ2);
            form.ShowDialog(this);
        }

        private void bApercuJ2_Click(object sender, EventArgs e)
        {
            if (joueurCourant != jeu.Joueur2 || joueur2Passe)
                return;

            var form = new FormDeckApercu(jeu.Joueur1.Nom, deckInitialJ1);
            form.ShowDialog(this);
        }


        


    }
}
