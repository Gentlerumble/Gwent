using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gwent.Net;

namespace Gwent
{
    public partial class FormChargerPartie : Form
    {
        public GameSaveDto PartieSelectionnee { get; private set; }
        public string FichierSelectionne { get; private set; }

        private ListView _listeSauvegardes;
        private Button _btnCharger;
        private Button _btnSupprimer;
        private Button _btnAnnuler;
        private Button _btnRefresh;
        private Label _lblDetails;
        private Label _lblChemin;

        public FormChargerPartie()
        {
            InitialiserComposants();
            ChargerListeSauvegardes();
        }

        private void InitialiserComposants()
        {
            this.Text = "Charger une partie";
            this.Size = new Size(750, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Layout principal
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // Titre
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));   // Chemin
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Liste
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // Détails
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // Boutons

            // Titre
            var lblTitre = new Label
            {
                Text = "Sélectionnez une partie à charger :",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Chemin du dossier
            _lblChemin = new Label
            {
                Text = $"Dossier: {GameSaveManager.GetSaveDirectory()}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Liste des sauvegardes
            _listeSauvegardes = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            _listeSauvegardes.Columns.Add("Nom", 180);
            _listeSauvegardes.Columns.Add("Date", 140);
            _listeSauvegardes.Columns.Add("Joueurs", 180);
            _listeSauvegardes.Columns.Add("Manche", 60);
            _listeSauvegardes.Columns.Add("Type", 70);

            _listeSauvegardes.SelectedIndexChanged += ListeSauvegardes_SelectedIndexChanged;
            _listeSauvegardes.DoubleClick += BtnCharger_Click;

            // Détails
            _lblDetails = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DarkGray,
                Text = "Sélectionnez une sauvegarde pour voir les détails.. .",
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            // Panel boutons
            var panelBoutons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            _btnAnnuler = new Button
            {
                Text = "Annuler",
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            _btnSupprimer = new Button
            {
                Text = "Supprimer",
                Size = new Size(100, 35),
                Enabled = false,
                BackColor = Color.IndianRed,
                ForeColor = Color.White
            };
            _btnSupprimer.Click += BtnSupprimer_Click;

            _btnCharger = new Button
            {
                Text = "Charger",
                Size = new Size(100, 35),
                Enabled = false,
                BackColor = Color.ForestGreen,
                ForeColor = Color.White
            };
            _btnCharger.Click += BtnCharger_Click;

            _btnRefresh = new Button
            {
                Text = "Actualiser",
                Size = new Size(100, 35),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White
            };
            _btnRefresh.Click += (s, e) => ChargerListeSauvegardes();

            panelBoutons.Controls.Add(_btnAnnuler);
            panelBoutons.Controls.Add(_btnSupprimer);
            panelBoutons.Controls.Add(_btnCharger);
            panelBoutons.Controls.Add(_btnRefresh);

            // Ajouter au layout
            layout.Controls.Add(lblTitre, 0, 0);
            layout.Controls.Add(_lblChemin, 0, 1);
            layout.Controls.Add(_listeSauvegardes, 0, 2);
            layout.Controls.Add(_lblDetails, 0, 3);
            layout.Controls.Add(panelBoutons, 0, 4);

            this.Controls.Add(layout);
            this.AcceptButton = _btnCharger;
            this.CancelButton = _btnAnnuler;
        }

        private void ChargerListeSauvegardes()
        {
            _listeSauvegardes.Items.Clear();

            System.Diagnostics.Debug.WriteLine("[FormChargerPartie] Chargement de la liste des sauvegardes.. .");

            var sauvegardes = GameSaveManager.ListerSauvegardes();

            System.Diagnostics.Debug.WriteLine($"[FormChargerPartie] Sauvegardes trouvées:  {sauvegardes.Count}");

            if (sauvegardes.Count == 0)
            {
                _lblDetails.Text = "Aucune sauvegarde trouvée dans le dossier:\n" + GameSaveManager.GetSaveDirectory();
                _lblDetails.ForeColor = Color.OrangeRed;
                return;
            }

            _lblDetails.ForeColor = Color.DarkGray;

            foreach (var save in sauvegardes)
            {
                var item = new ListViewItem(save.SaveName);
                item.SubItems.Add(save.SaveDate.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add($"{save.Joueur1Nom} vs {save.Joueur2Nom}");
                item.SubItems.Add(save.NumeroManche.ToString());
                item.SubItems.Add(save.EstPartieReseau ? "Réseau" : "Local");
                item.Tag = save;

                _listeSauvegardes.Items.Add(item);

                System.Diagnostics.Debug.WriteLine($"[FormChargerPartie] Ajouté: {save.SaveName}");
            }

            // Sélectionner le premier élément
            if (_listeSauvegardes.Items.Count > 0)
            {
                _listeSauvegardes.Items[0].Selected = true;
                _listeSauvegardes.Select();
            }
        }

        private void ListeSauvegardes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_listeSauvegardes.SelectedItems.Count > 0)
            {
                var saveInfo = _listeSauvegardes.SelectedItems[0].Tag as GameSaveInfo;
                if (saveInfo != null)
                {
                    _lblDetails.Text = $"📅 Sauvegardé le {saveInfo.SaveDate: dddd dd MMMM yyyy à HH:mm}\n" +
                                       $"🎮 {saveInfo.Joueur1Nom} ({saveInfo.Joueur1Vies}♥) vs {saveInfo.Joueur2Nom} ({saveInfo.Joueur2Vies}♥) - Manche {saveInfo.NumeroManche}";
                    _lblDetails.ForeColor = Color.DarkGreen;
                    _btnCharger.Enabled = true;
                    _btnSupprimer.Enabled = true;
                    FichierSelectionne = saveInfo.FilePath;

                    System.Diagnostics.Debug.WriteLine($"[FormChargerPartie] Sélectionné: {saveInfo.SaveName} ({saveInfo.FilePath})");
                }
            }
            else
            {
                _lblDetails.Text = "Sélectionnez une sauvegarde pour voir les détails...";
                _lblDetails.ForeColor = Color.DarkGray;
                _btnCharger.Enabled = false;
                _btnSupprimer.Enabled = false;
                FichierSelectionne = null;
            }
        }

        private void BtnCharger_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FichierSelectionne))
            {
                MessageBox.Show("Veuillez sélectionner une sauvegarde.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[FormChargerPartie] Chargement de:  {FichierSelectionne}");

            PartieSelectionnee = GameSaveManager.ChargerPartie(FichierSelectionne);

            if (PartieSelectionnee != null)
            {
                System.Diagnostics.Debug.WriteLine($"[FormChargerPartie] Partie chargée avec succès");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Erreur lors du chargement de la sauvegarde.\nLe fichier est peut-être corrompu.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FichierSelectionne)) return;

            var saveInfo = _listeSauvegardes.SelectedItems[0]?.Tag as GameSaveInfo;
            string saveName = saveInfo?.SaveName ?? "cette sauvegarde";

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer \"{saveName}\" ?",
                "Confirmation de suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (GameSaveManager.SupprimerSauvegarde(FichierSelectionne))
                {
                    MessageBox.Show("Sauvegarde supprimée.", "Succès",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ChargerListeSauvegardes();
                    _btnCharger.Enabled = false;
                    _btnSupprimer.Enabled = false;
                    FichierSelectionne = null;
                }
                else
                {
                    MessageBox.Show("Erreur lors de la suppression.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}