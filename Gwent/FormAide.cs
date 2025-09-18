using System;
using System.Windows.Forms;

namespace Gwent
{
    public partial class FormAide : Form
    {
        public FormAide(string texte)
        {
            InitializeComponent();
            this.Text = "Aide & Règles Gwent";
            this.Width = 1000;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;

            TextBox tb = new TextBox();
            tb.Multiline = true;
            tb.ReadOnly = true;
            tb.ScrollBars = ScrollBars.Vertical;
            tb.Dock = DockStyle.Fill;
            tb.Font = new System.Drawing.Font("Segoe UI", 11);
            tb.Text = texte;

            this.Controls.Add(tb);
        }
    }
}
