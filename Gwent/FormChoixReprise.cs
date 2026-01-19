using System.Drawing;
using System.Windows.Forms;

namespace Gwent
{
    public enum ModeReprise
    {
        Annuler,
        Local,
        Heberger,
        Rejoindre
    }

    public partial class FormChoixReprise : Form
    {
        public ModeReprise ModeChoisi { get; private set; } = ModeReprise.Annuler;
        public string AdresseServeur { get; private set; } = "127.0.0.1";
        public int PortServeur { get; private set; } = 12345;

        private Panel _panelChoixMode;
        private Panel _panelChoixReseau;
        private Panel _panelConnexion;

        public FormChoixReprise(string nomSauvegarde)
        {
            InitialiserComposants(nomSauvegarde);
        }

        private void InitialiserComposants(string nomSauvegarde)
        {
            this.Text = "Reprendre la partie";
            this.Size = new Size(450, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);

            // Panel principal
            var panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Titre
            var lblTitre = new Label
            {
                Text = $"📁 {nomSauvegarde}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Gold,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblSousTitre = new Label
            {
                Text = "Cette partie était en mode multijoueur.\nComment voulez-vous continuer ?",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Panel choix mode (Local / Multijoueur)
            _panelChoixMode = CreerPanelChoixMode();
            _panelChoixMode.Visible = true;

            // Panel choix réseau (Héberger / Rejoindre)
            _panelChoixReseau = CreerPanelChoixReseau();
            _panelChoixReseau.Visible = false;

            // Panel connexion (IP / Port)
            _panelConnexion = CreerPanelConnexion();
            _panelConnexion.Visible = false;

            // Ajouter dans l'ordre inverse (le dernier ajouté est en dessous)
            panelPrincipal.Controls.Add(_panelConnexion);
            panelPrincipal.Controls.Add(_panelChoixReseau);
            panelPrincipal.Controls.Add(_panelChoixMode);
            panelPrincipal.Controls.Add(lblSousTitre);
            panelPrincipal.Controls.Add(lblTitre);

            this.Controls.Add(panelPrincipal);
        }

        private Panel CreerPanelChoixMode()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(5)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            // Bouton Local
            var btnLocal = new Button
            {
                Text = "🖥️ En local\n(2 joueurs sur ce PC)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btnLocal.FlatAppearance.BorderSize = 0;
            btnLocal.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Choix:  Local");
                ModeChoisi = ModeReprise.Local;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Bouton Multijoueur
            var btnMulti = new Button
            {
                Text = "🌐 En multijoueur\n(2 joueurs en réseau)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btnMulti.FlatAppearance.BorderSize = 0;
            btnMulti.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Passage au panel réseau");
                _panelChoixMode.Visible = false;
                _panelChoixReseau.Visible = true;
                _panelChoixReseau.BringToFront();
            };

            // Bouton Annuler
            var btnAnnuler = new Button
            {
                Text = "Annuler",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
            btnAnnuler.FlatAppearance.BorderSize = 0;
            btnAnnuler.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Choix:  Annuler");
                ModeChoisi = ModeReprise.Annuler;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            layout.Controls.Add(btnLocal, 0, 0);
            layout.Controls.Add(btnMulti, 1, 0);
            layout.Controls.Add(btnAnnuler, 0, 1);
            layout.SetColumnSpan(btnAnnuler, 2);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreerPanelChoixReseau()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(5)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            // Bouton Héberger
            var btnHeberger = new Button
            {
                Text = "🏠 Héberger\n(Attendre un joueur)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Orange,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btnHeberger.FlatAppearance.BorderSize = 0;
            btnHeberger.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Choix: Heberger");
                ModeChoisi = ModeReprise.Heberger;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Bouton Rejoindre
            var btnRejoindre = new Button
            {
                Text = "🔗 Rejoindre\n(Se connecter à un hôte)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.MediumPurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btnRejoindre.FlatAppearance.BorderSize = 0;
            btnRejoindre.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Passage au panel connexion");
                _panelChoixReseau.Visible = false;
                _panelConnexion.Visible = true;
                _panelConnexion.BringToFront();
            };

            // Bouton Retour
            var btnRetour = new Button
            {
                Text = "← Retour",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
            btnRetour.FlatAppearance.BorderSize = 0;
            btnRetour.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Retour au panel mode");
                _panelChoixReseau.Visible = false;
                _panelChoixMode.Visible = true;
                _panelChoixMode.BringToFront();
            };

            layout.Controls.Add(btnHeberger, 0, 0);
            layout.Controls.Add(btnRejoindre, 1, 0);
            layout.Controls.Add(btnRetour, 0, 1);
            layout.SetColumnSpan(btnRetour, 2);

            panel.Controls.Add(layout);
            return panel;
        }

        private Panel CreerPanelConnexion()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(5)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Label IP
            var lblIP = new Label
            {
                Text = "Adresse IP :",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };

            // TextBox IP
            var txtIP = new TextBox
            {
                Text = "127.0.0.1",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(5)
            };

            // Label Port
            var lblPort = new Label
            {
                Text = "Port :",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };

            // TextBox Port
            var txtPort = new TextBox
            {
                Text = "12345",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(5)
            };

            // Panel boutons
            var panelBoutons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 5, 0, 0)
            };

            // Bouton Connecter
            var btnConnecter = new Button
            {
                Text = "Se connecter",
                Size = new Size(120, 35),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConnecter.FlatAppearance.BorderSize = 0;
            btnConnecter.Click += (s, e) =>
            {
                AdresseServeur = txtIP.Text.Trim();
                if (string.IsNullOrEmpty(AdresseServeur))
                {
                    AdresseServeur = "127.0.0.1";
                }

                if (int.TryParse(txtPort.Text.Trim(), out int port) && port > 0 && port < 65536)
                {
                    PortServeur = port;
                }
                else
                {
                    PortServeur = 12345;
                }

                System.Diagnostics.Debug.WriteLine($"[FormChoixReprise] Choix: Rejoindre {AdresseServeur}:{PortServeur}");
                ModeChoisi = ModeReprise.Rejoindre;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Bouton Retour
            var btnRetour = new Button
            {
                Text = "← Retour",
                Size = new Size(90, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnRetour.FlatAppearance.BorderSize = 0;
            btnRetour.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[FormChoixReprise] Retour au panel réseau");
                _panelConnexion.Visible = false;
                _panelChoixReseau.Visible = true;
                _panelChoixReseau.BringToFront();
            };

            panelBoutons.Controls.Add(btnConnecter);
            panelBoutons.Controls.Add(btnRetour);

            layout.Controls.Add(lblIP, 0, 0);
            layout.Controls.Add(txtIP, 1, 0);
            layout.Controls.Add(lblPort, 0, 1);
            layout.Controls.Add(txtPort, 1, 1);
            layout.Controls.Add(panelBoutons, 0, 3);
            layout.SetColumnSpan(panelBoutons, 2);

            panel.Controls.Add(layout);
            return panel;
        }
    }
}