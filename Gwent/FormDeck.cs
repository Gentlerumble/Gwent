using Gwent.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gwent
{
    public partial class FormDeck : Form
    {
        public bool HostSelected { get; set; } = false;
        public bool ClientSelected { get; set; } = false;

        public string HostAddress { get; private set; } = " 172.20.10.2";
        public int HostPort { get; private set; } = 12345;

        public int LocalDeckIndex { get; private set; } = -1;
        public List<Carte> DeckJ1 { get; private set; }
        public List<Carte> DeckJ2 { get; private set; }

        public int IndexDeckJ1 { get; private set; }
        public int IndexDeckJ2 { get; private set; }

        private PictureBox pbSelectionneeJ1 = null;
        private PictureBox pbSelectionneeJ2 = null;

        // Dans la classe FormDeck, ajoutez :
        public bool ChargerPartieSelected { get; private set; } = false;
        public GameSaveDto PartieSauvegardee { get; private set; }

        private Button btnChargerPartie;

        private ToolTip toolTipDeck = new ToolTip();

        public FormDeck()
        {
            InitializeComponent();
            pbNordJ1.Image = Image.FromFile("Images\\dos_carte_J1.jpg");
            pbNordJ1.Tag = 0;

            pbMonstreJ1.Image = Image.FromFile("Images\\dos_carte_J2.jpg");
            pbMonstreJ1.Tag = 1;

            pbScoiaTelJ1.Image = Image.FromFile("Images\\dos_carte_J4.jpg");
            pbScoiaTelJ1.Tag = 2;

            pbNilfgaardJ1.Image = Image.FromFile("Images\\dos_carte_J3.jpg");
            pbNilfgaardJ1.Tag = 3;



            pbNordJ2.Image = Image.FromFile("Images\\dos_carte_J1.jpg");
            pbNordJ2.Tag = 0;

            pbMonstreJ2.Image = Image.FromFile("Images\\dos_carte_J2.jpg");
            pbMonstreJ2.Tag = 1;

            pbScoiaTelJ2.Image = Image.FromFile("Images\\dos_carte_J4.jpg");
            pbScoiaTelJ2.Tag = 2;

            pbNilfgaardJ2.Image = Image.FromFile("Images\\dos_carte_J3.jpg");
            pbNilfgaardJ2.Tag = 3;

            pbNordJ1.SizeMode = PictureBoxSizeMode.StretchImage;
            pbMonstreJ1.SizeMode = PictureBoxSizeMode.StretchImage;
            pbScoiaTelJ1.SizeMode = PictureBoxSizeMode.StretchImage;
            pbNilfgaardJ1.SizeMode = PictureBoxSizeMode.StretchImage;

            pbNordJ2.SizeMode = PictureBoxSizeMode.StretchImage;
            pbMonstreJ2.SizeMode = PictureBoxSizeMode.StretchImage;
            pbScoiaTelJ2.SizeMode = PictureBoxSizeMode.StretchImage;
            pbNilfgaardJ2.SizeMode = PictureBoxSizeMode.StretchImage;

            toolTipDeck.SetToolTip(pbNordJ1, InfosDecks[0]);
            toolTipDeck.SetToolTip(pbMonstreJ1, InfosDecks[1]);
            toolTipDeck.SetToolTip(pbScoiaTelJ1, InfosDecks[2]);
            toolTipDeck.SetToolTip(pbNilfgaardJ1, InfosDecks[3]);

            toolTipDeck.SetToolTip(pbNordJ2, InfosDecks[0]);
            toolTipDeck.SetToolTip(pbMonstreJ2, InfosDecks[1]);
            toolTipDeck.SetToolTip(pbScoiaTelJ2, InfosDecks[2]);
            toolTipDeck.SetToolTip(pbNilfgaardJ2, InfosDecks[3]);
        }

        private void bValiderDeck_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ1 == null || pbSelectionneeJ2 == null)
            {
                MessageBox.Show("Chaque joueur doit choisir un deck !");
                return;
            }

            int indexDeckJ1 = (int)pbSelectionneeJ1.Tag;
            int indexDeckJ2 = (int)pbSelectionneeJ2.Tag;

            var decks = Jeu.AvoirDeckDispo();
            DeckJ1 = new List<Carte>(decks[indexDeckJ1]);
            DeckJ2 = new List<Carte>(decks[indexDeckJ2]);

            IndexDeckJ1 = indexDeckJ1;
            IndexDeckJ2 = indexDeckJ2;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void pbNordJ1_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ1 != null)
            {
                pbSelectionneeJ1.BorderStyle = BorderStyle.None;
                pbSelectionneeJ1.Padding = new Padding(0);
                pbSelectionneeJ1.BackColor = Color.Transparent;
            }

            pbSelectionneeJ1 = (PictureBox)sender;
            pbSelectionneeJ1.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ1.Padding = new Padding(4);
            pbSelectionneeJ1.BackColor = Color.Yellow;
        }

        private void pbMonstreJ1_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ1 != null)
            {
                pbSelectionneeJ1.BorderStyle = BorderStyle.None;
                pbSelectionneeJ1.Padding = new Padding(0);
                pbSelectionneeJ1.BackColor = Color.Transparent;
            }

            pbSelectionneeJ1 = (PictureBox)sender;
            pbSelectionneeJ1.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ1.Padding = new Padding(4);
            pbSelectionneeJ1.BackColor = Color.Yellow;
        }

        private void pbScoiaTelJ1_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ1 != null)
            {
                pbSelectionneeJ1.BorderStyle = BorderStyle.None;
                pbSelectionneeJ1.Padding = new Padding(0);
                pbSelectionneeJ1.BackColor = Color.Transparent;
            }

            pbSelectionneeJ1 = (PictureBox)sender;
            pbSelectionneeJ1.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ1.Padding = new Padding(4);
            pbSelectionneeJ1.BackColor = Color.Yellow;
        }

        private void pbNilfgaardJ1_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ1 != null)
            {
                pbSelectionneeJ1.BorderStyle = BorderStyle.None;
                pbSelectionneeJ1.Padding = new Padding(0);
                pbSelectionneeJ1.BackColor = Color.Transparent;
            }

            pbSelectionneeJ1 = (PictureBox)sender;
            pbSelectionneeJ1.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ1.Padding = new Padding(4);
            pbSelectionneeJ1.BackColor = Color.Yellow;
        }

        private void pbNordJ2_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ2 != null)
            {
                pbSelectionneeJ2.BorderStyle = BorderStyle.None;
                pbSelectionneeJ2.Padding = new Padding(0);
                pbSelectionneeJ2.BackColor = Color.Transparent;
            }

            pbSelectionneeJ2 = (PictureBox)sender;
            pbSelectionneeJ2.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ2.Padding = new Padding(4);
            pbSelectionneeJ2.BackColor = Color.Yellow;
        }

        private void pbMonstreJ2_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ2 != null)
            {
                pbSelectionneeJ2.BorderStyle = BorderStyle.None;
                pbSelectionneeJ2.Padding = new Padding(0);
                pbSelectionneeJ2.BackColor = Color.Transparent;
            }

            pbSelectionneeJ2 = (PictureBox)sender;
            pbSelectionneeJ2.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ2.Padding = new Padding(4);
            pbSelectionneeJ2.BackColor = Color.Yellow;
        }

        private void pbScoiaTelJ2_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ2 != null)
            {
                pbSelectionneeJ2.BorderStyle = BorderStyle.None;
                pbSelectionneeJ2.Padding = new Padding(0);
                pbSelectionneeJ2.BackColor = Color.Transparent;
            }

            pbSelectionneeJ2 = (PictureBox)sender;
            pbSelectionneeJ2.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ2.Padding = new Padding(4);
            pbSelectionneeJ2.BackColor = Color.Yellow;
        }

        private void pbNilfgaardJ2_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ2 != null)
            {
                pbSelectionneeJ2.BorderStyle = BorderStyle.None;
                pbSelectionneeJ2.Padding = new Padding(0);
                pbSelectionneeJ2.BackColor = Color.Transparent;
            }

            pbSelectionneeJ2 = (PictureBox)sender;
            pbSelectionneeJ2.BorderStyle = BorderStyle.FixedSingle;
            pbSelectionneeJ2.Padding = new Padding(4);
            pbSelectionneeJ2.BackColor = Color.Yellow;
        }

        public static readonly string[] DosCartesDecks = new string[]
        {
            Path.Combine(Application.StartupPath, "Images", "dos_carte_J1.jpg"), // Royaumes du Nord
            Path.Combine(Application.StartupPath, "Images", "dos_carte_J2.jpg"), // Monstres
            Path.Combine(Application.StartupPath, "Images", "dos_carte_J4.jpg"), // Scoia'tael
            Path.Combine(Application.StartupPath, "Images", "dos_carte_J3.jpg"), // Nilfgaard
        };

        private static readonly string[] InfosDecks = new string[]
        {
            "Royaumes du Nord\nForces : Polyvalence,\nFaiblesses : Besoin de synergie,\nPouvoir passif : Pioche une carte après une manche gagnée,\nPouvoir activable : Retire tous les effets Météo actifs",
            "Monstres\nForces : Beaucoup d'invocations\nFaiblesses : Peu de contrôle, pas beaucoup de contrôle de zone,\nPouvoir passif : À la fin de la manche garde une carte sur le plateau,\nPouvoir activable : Ramnène une carte depuis le cimetière",
            "Scoia'tael\nForces : Agilité, cartes puissantes\nFaiblesses : Peu de synergie,\nPouvoir passif : Choisit qui commence,\nPouvoir activable : Détruit la carte de mélée la plus forte adverse si le score de la zone Mélée de l'adversaire dépasse 10",
            "Nilfgaard\nForces : Espions, contrôle\nFaiblesses : Besoin de synergie,\nPouvoir passif : Gagne forcément les égalités,\nPouvoir activable : double le score des unités de Mélée"
        };

        // Héberger : utilise la sélection J1 (gauche)
        private void btnHost_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ1 == null)
            {
                MessageBox.Show("Sélectionnez d'abord votre deck (gauche).");
                return;
            }

            int port = HostPort;
            if (this.Controls.ContainsKey("txtPort") && this.Controls["txtPort"] is TextBox tbPort && int.TryParse(tbPort.Text, out var p))
                port = p;

            HostSelected = true;
            ClientSelected = false;
            LocalDeckIndex = (int)pbSelectionneeJ1.Tag;
            HostPort = port;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Se connecter : utilise la sélection J2 (droite)
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (pbSelectionneeJ2 == null)
            {
                MessageBox.Show("Sélectionnez d'abord votre deck (droite).");
                return;
            }

            string host = HostAddress;
            int port = HostPort;
            if (this.Controls.ContainsKey("txtHostAddress") && this.Controls["txtHostAddress"] is TextBox tbHost && !string.IsNullOrWhiteSpace(tbHost.Text))
                host = tbHost.Text.Trim();
            if (this.Controls.ContainsKey("txtPort") && this.Controls["txtPort"] is TextBox tbPort && int.TryParse(tbPort.Text, out var p))
                port = p;

            HostSelected = false;
            ClientSelected = true;
            LocalDeckIndex = (int)pbSelectionneeJ2.Tag;
            HostAddress = host;
            HostPort = port;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private bool ValiderSelectionDecks()
        {
            if (pbSelectionneeJ1 == null || pbSelectionneeJ2 == null)
            {
                MessageBox.Show("Chaque joueur doit choisir un deck !");
                return false;
            }
            return true;
        }

        private void BtnChargerPartie_Click(object sender, EventArgs e)
        {
            using (var formCharger = new FormChargerPartie())
            {
                if (formCharger.ShowDialog() == DialogResult.OK && formCharger.PartieSelectionnee != null)
                {
                    ChargerPartieSelected = true;
                    PartieSauvegardee = formCharger.PartieSelectionnee;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void PrepareDecksAndClose()
        {
            int indexDeckJ1 = (int)pbSelectionneeJ1.Tag;
            int indexDeckJ2 = (int)pbSelectionneeJ2.Tag;
            var decks = Jeu.AvoirDeckDispo();
            DeckJ1 = new List<Carte>(decks[indexDeckJ1]);
            DeckJ2 = new List<Carte>(decks[indexDeckJ2]);
            IndexDeckJ1 = indexDeckJ1;
            IndexDeckJ2 = indexDeckJ2;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
