using System.Drawing;
using System.Windows.Forms;

namespace Gwent
{
    partial class FormDeck
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDeck));
            this.lChoixDeckJ1 = new System.Windows.Forms.Label();
            this.lChoixDeckJ2 = new System.Windows.Forms.Label();
            this.pbNordJ1 = new System.Windows.Forms.PictureBox();
            this.pbMonstreJ1 = new System.Windows.Forms.PictureBox();
            this.pbScoiaTelJ1 = new System.Windows.Forms.PictureBox();
            this.pbNilfgaardJ1 = new System.Windows.Forms.PictureBox();
            this.pbNilfgaardJ2 = new System.Windows.Forms.PictureBox();
            this.pbScoiaTelJ2 = new System.Windows.Forms.PictureBox();
            this.pbMonstreJ2 = new System.Windows.Forms.PictureBox();
            this.pbNordJ2 = new System.Windows.Forms.PictureBox();
            this.bValiderDeck = new System.Windows.Forms.Button();
            this.btnHost = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtHostAddress = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lHostAddress = new System.Windows.Forms.Label();
            this.lPort = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbNordJ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbMonstreJ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbScoiaTelJ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNilfgaardJ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNilfgaardJ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbScoiaTelJ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbMonstreJ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNordJ2)).BeginInit();
            this.SuspendLayout();
            // 
            // lChoixDeckJ1
            // 
            this.lChoixDeckJ1.AutoSize = true;
            this.lChoixDeckJ1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lChoixDeckJ1.Location = new System.Drawing.Point(26, 221);
            this.lChoixDeckJ1.Name = "lChoixDeckJ1";
            this.lChoixDeckJ1.Size = new System.Drawing.Size(339, 29);
            this.lChoixDeckJ1.TabIndex = 0;
            this.lChoixDeckJ1.Text = "Choix du deck pour le joueur 1";
            // 
            // lChoixDeckJ2
            // 
            this.lChoixDeckJ2.AutoSize = true;
            this.lChoixDeckJ2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lChoixDeckJ2.Location = new System.Drawing.Point(26, 9);
            this.lChoixDeckJ2.Name = "lChoixDeckJ2";
            this.lChoixDeckJ2.Size = new System.Drawing.Size(339, 29);
            this.lChoixDeckJ2.TabIndex = 1;
            this.lChoixDeckJ2.Text = "Choix du deck pour le joueur 2";
            // 
            // pbNordJ1
            // 
            this.pbNordJ1.Location = new System.Drawing.Point(31, 281);
            this.pbNordJ1.Name = "pbNordJ1";
            this.pbNordJ1.Size = new System.Drawing.Size(97, 140);
            this.pbNordJ1.TabIndex = 2;
            this.pbNordJ1.TabStop = false;
            this.pbNordJ1.Click += new System.EventHandler(this.pbNordJ1_Click);
            // 
            // pbMonstreJ1
            // 
            this.pbMonstreJ1.Location = new System.Drawing.Point(165, 281);
            this.pbMonstreJ1.Name = "pbMonstreJ1";
            this.pbMonstreJ1.Size = new System.Drawing.Size(97, 140);
            this.pbMonstreJ1.TabIndex = 3;
            this.pbMonstreJ1.TabStop = false;
            this.pbMonstreJ1.Click += new System.EventHandler(this.pbMonstreJ1_Click);
            // 
            // pbScoiaTelJ1
            // 
            this.pbScoiaTelJ1.Location = new System.Drawing.Point(308, 281);
            this.pbScoiaTelJ1.Name = "pbScoiaTelJ1";
            this.pbScoiaTelJ1.Size = new System.Drawing.Size(97, 140);
            this.pbScoiaTelJ1.TabIndex = 4;
            this.pbScoiaTelJ1.TabStop = false;
            this.pbScoiaTelJ1.Click += new System.EventHandler(this.pbScoiaTelJ1_Click);
            // 
            // pbNilfgaardJ1
            // 
            this.pbNilfgaardJ1.Location = new System.Drawing.Point(452, 281);
            this.pbNilfgaardJ1.Name = "pbNilfgaardJ1";
            this.pbNilfgaardJ1.Size = new System.Drawing.Size(97, 140);
            this.pbNilfgaardJ1.TabIndex = 5;
            this.pbNilfgaardJ1.TabStop = false;
            this.pbNilfgaardJ1.Click += new System.EventHandler(this.pbNilfgaardJ1_Click);
            // 
            // pbNilfgaardJ2
            // 
            this.pbNilfgaardJ2.Location = new System.Drawing.Point(452, 51);
            this.pbNilfgaardJ2.Name = "pbNilfgaardJ2";
            this.pbNilfgaardJ2.Size = new System.Drawing.Size(97, 140);
            this.pbNilfgaardJ2.TabIndex = 9;
            this.pbNilfgaardJ2.TabStop = false;
            this.pbNilfgaardJ2.Click += new System.EventHandler(this.pbNilfgaardJ2_Click);
            // 
            // pbScoiaTelJ2
            // 
            this.pbScoiaTelJ2.Location = new System.Drawing.Point(308, 51);
            this.pbScoiaTelJ2.Name = "pbScoiaTelJ2";
            this.pbScoiaTelJ2.Size = new System.Drawing.Size(97, 140);
            this.pbScoiaTelJ2.TabIndex = 8;
            this.pbScoiaTelJ2.TabStop = false;
            this.pbScoiaTelJ2.Click += new System.EventHandler(this.pbScoiaTelJ2_Click);
            // 
            // pbMonstreJ2
            // 
            this.pbMonstreJ2.Location = new System.Drawing.Point(165, 51);
            this.pbMonstreJ2.Name = "pbMonstreJ2";
            this.pbMonstreJ2.Size = new System.Drawing.Size(97, 140);
            this.pbMonstreJ2.TabIndex = 7;
            this.pbMonstreJ2.TabStop = false;
            this.pbMonstreJ2.Click += new System.EventHandler(this.pbMonstreJ2_Click);
            // 
            // pbNordJ2
            // 
            this.pbNordJ2.Location = new System.Drawing.Point(31, 51);
            this.pbNordJ2.Name = "pbNordJ2";
            this.pbNordJ2.Size = new System.Drawing.Size(97, 140);
            this.pbNordJ2.TabIndex = 6;
            this.pbNordJ2.TabStop = false;
            this.pbNordJ2.Click += new System.EventHandler(this.pbNordJ2_Click);
            // 
            // bValiderDeck
            // 
            this.bValiderDeck.Location = new System.Drawing.Point(634, 51);
            this.bValiderDeck.Name = "bValiderDeck";
            this.bValiderDeck.Size = new System.Drawing.Size(111, 28);
            this.bValiderDeck.TabIndex = 10;
            this.bValiderDeck.Text = "Valider";
            this.bValiderDeck.UseVisualStyleBackColor = true;
            this.bValiderDeck.Click += new System.EventHandler(this.bValiderDeck_Click);
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(634, 281);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(111, 28);
            this.btnHost.TabIndex = 11;
            this.btnHost.Text = "Héberger";
            this.btnHost.UseVisualStyleBackColor = true;
            this.btnHost.Click += new System.EventHandler(this.btnHost_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(634, 406);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(111, 28);
            this.btnConnect.TabIndex = 12;
            this.btnConnect.Text = "Se connecter";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtHostAddress
            // 
            this.txtHostAddress.Location = new System.Drawing.Point(634, 338);
            this.txtHostAddress.Name = "txtHostAddress";
            this.txtHostAddress.Size = new System.Drawing.Size(111, 20);
            this.txtHostAddress.TabIndex = 13;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(634, 378);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(111, 20);
            this.txtPort.TabIndex = 14;
            // 
            // lHostAddress
            // 
            this.lHostAddress.AutoSize = true;
            this.lHostAddress.Location = new System.Drawing.Point(631, 322);
            this.lHostAddress.Name = "lHostAddress";
            this.lHostAddress.Size = new System.Drawing.Size(94, 13);
            this.lHostAddress.TabIndex = 15;
            this.lHostAddress.Text = "Adresse de l\'hôte :";
            // 
            // lPort
            // 
            this.lPort.AutoSize = true;
            this.lPort.Location = new System.Drawing.Point(631, 362);
            this.lPort.Name = "lPort";
            this.lPort.Size = new System.Drawing.Size(32, 13);
            this.lPort.TabIndex = 16;
            this.lPort.Text = "Port :";
            // 
            // FormDeck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lPort);
            this.Controls.Add(this.lHostAddress);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtHostAddress);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnHost);
            this.Controls.Add(this.bValiderDeck);
            this.Controls.Add(this.pbNilfgaardJ2);
            this.Controls.Add(this.pbScoiaTelJ2);
            this.Controls.Add(this.pbMonstreJ2);
            this.Controls.Add(this.pbNordJ2);
            this.Controls.Add(this.pbNilfgaardJ1);
            this.Controls.Add(this.pbScoiaTelJ1);
            this.Controls.Add(this.pbMonstreJ1);
            this.Controls.Add(this.pbNordJ1);
            this.Controls.Add(this.lChoixDeckJ2);
            this.Controls.Add(this.lChoixDeckJ1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormDeck";
            this.Text = "Choix du Deck";
            ((System.ComponentModel.ISupportInitialize)(this.pbNordJ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbMonstreJ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbScoiaTelJ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNilfgaardJ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNilfgaardJ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbScoiaTelJ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbMonstreJ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbNordJ2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();


            // Bouton Charger une partie
            btnChargerPartie = new Button
            {
                Text = "Charger une partie",
                Location = new Point(634, 130),
                Size = new Size(111, 40),
                BackColor = Color.DarkOrange,
                ForeColor = Color.White
            };
            btnChargerPartie.Click += BtnChargerPartie_Click;
            this.Controls.Add(btnChargerPartie);

        }

        #endregion

        private System.Windows.Forms.Label lChoixDeckJ1;
        private System.Windows.Forms.Label lChoixDeckJ2;
        private System.Windows.Forms.PictureBox pbNordJ1;
        private System.Windows.Forms.PictureBox pbMonstreJ1;
        private System.Windows.Forms.PictureBox pbScoiaTelJ1;
        private System.Windows.Forms.PictureBox pbNilfgaardJ1;
        private System.Windows.Forms.PictureBox pbNilfgaardJ2;
        private System.Windows.Forms.PictureBox pbScoiaTelJ2;
        private System.Windows.Forms.PictureBox pbMonstreJ2;
        private System.Windows.Forms.PictureBox pbNordJ2;
        private System.Windows.Forms.Button bValiderDeck;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtHostAddress;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lHostAddress;
        private System.Windows.Forms.Label lPort;
    }
}