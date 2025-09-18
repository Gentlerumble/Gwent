using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Gwent
{
    public partial class FormDeckApercu : Form
    {
        public FormDeckApercu(string nomJoueur, List<Carte> deck)
        {
            InitializeComponent();
            this.Text = $"Aperçu du deck de {nomJoueur}";

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            this.Controls.Add(flow);

            foreach (var carte in deck)
                flow.Controls.Add(CreerPanelCarte(carte));
        }


        private Panel CreerPanelCarte(Carte carte)
        {
            var panel = new Panel
            {
                Width = 120,
                Height = 220,
                Margin = new Padding(30),
                BackColor = Color.LightGray
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // image
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // texte

            var pb = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Image.FromFile(carte.ImagePath),
                Margin = new Padding(0)
            };

            var lbl = new Label
            {
                Text = $"{carte.Nom}\nPuissance : {carte.Puissance}\nPouvoir : {carte.Pouvoir}",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSize = false
            };

            table.Controls.Add(pb, 0, 0);
            table.Controls.Add(lbl, 0, 1);

            panel.Controls.Add(table);
            return panel;
        }




    }
}
