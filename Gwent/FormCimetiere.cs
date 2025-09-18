using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Gwent
{
    public partial class FormCimetiere : Form
    {
        public Carte CarteChoisie { get; private set; }

        public FormCimetiere(List<Carte> cimetierre)
        {
            InitializeComponent();
            foreach (var carte in cimetierre)
            {
                PictureBox pb = new PictureBox
                {
                    Width = 80,
                    Height = 120,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Image.FromFile(carte.ImagePath),
                    Tag = carte,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(5)
                };
                pb.Click += Pb_Click;
                flpCimetiere.Controls.Add(pb);
            }
                
        }

        private void Pb_Click(object sender, EventArgs e)
        {

            // Réinitialise l'effet sur toutes les cartes
            foreach (Control ctrl in flpCimetiere.Controls)
            {
                if (ctrl is PictureBox pbReset)
                {
                    pbReset.BorderStyle = BorderStyle.None;
                    pbReset.Size = new Size(80, 120);
                    pbReset.BackColor = Color.Transparent;
                    pbReset.Padding = new Padding(0);
                }
            }

            // Applique l'effet de sélection sur la carte cliquée
            PictureBox pb = sender as PictureBox;
            pb.BorderStyle = BorderStyle.FixedSingle;
            pb.Size = new Size(85, 125);
            pb.BackColor = Color.Yellow;
            pb.Padding = new Padding(4);

            CarteChoisie = pb.Tag as Carte;
        }

        private void bChoisir_Click(object sender, EventArgs e)
        {
            if (CarteChoisie != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Sélectionnez une carte !");
            }
        }

        
    }
}
