namespace Gwent
{
    partial class FormCimetiere
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
            this.label1 = new System.Windows.Forms.Label();
            this.flpCimetiere = new System.Windows.Forms.FlowLayoutPanel();
            this.bChoisir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(128, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(542, 31);
            this.label1.TabIndex = 1;
            this.label1.Text = "Cimetière : choississez une carte à ramener";
            // 
            // flpCimetiere
            // 
            this.flpCimetiere.Location = new System.Drawing.Point(12, 59);
            this.flpCimetiere.Name = "flpCimetiere";
            this.flpCimetiere.Size = new System.Drawing.Size(773, 309);
            this.flpCimetiere.TabIndex = 3;
            // 
            // bChoisir
            // 
            this.bChoisir.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bChoisir.Location = new System.Drawing.Point(304, 385);
            this.bChoisir.Name = "bChoisir";
            this.bChoisir.Size = new System.Drawing.Size(159, 53);
            this.bChoisir.TabIndex = 2;
            this.bChoisir.Text = "Choisir";
            this.bChoisir.UseVisualStyleBackColor = true;
            this.bChoisir.Click += new System.EventHandler(this.bChoisir_Click);
            // 
            // FormCimetiere
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.flpCimetiere);
            this.Controls.Add(this.bChoisir);
            this.Controls.Add(this.label1);
            this.Name = "FormCimetiere";
            this.Text = "Cimetière";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flpCimetiere;
        private System.Windows.Forms.Button bChoisir;
    }
}