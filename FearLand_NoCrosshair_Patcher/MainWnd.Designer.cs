namespace FearLand_NoCrosshair_Patcher
{
    partial class MainWnd
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWnd));
            this.Btn_Open = new System.Windows.Forms.Button();
            this.Txt_Log = new System.Windows.Forms.RichTextBox();
            this.Txt_File = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Btn_Patch = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.Btn_PatchCrosshair = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Btn_Open
            // 
            this.Btn_Open.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_Open.Location = new System.Drawing.Point(484, 27);
            this.Btn_Open.Name = "Btn_Open";
            this.Btn_Open.Size = new System.Drawing.Size(29, 22);
            this.Btn_Open.TabIndex = 2;
            this.Btn_Open.Text = "...";
            this.Btn_Open.UseVisualStyleBackColor = true;
            this.Btn_Open.Click += new System.EventHandler(this.Btn_Open_Click);
            // 
            // Txt_Log
            // 
            this.Txt_Log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Txt_Log.HideSelection = false;
            this.Txt_Log.Location = new System.Drawing.Point(8, 114);
            this.Txt_Log.Name = "Txt_Log";
            this.Txt_Log.ReadOnly = true;
            this.Txt_Log.Size = new System.Drawing.Size(505, 200);
            this.Txt_Log.TabIndex = 1;
            this.Txt_Log.TabStop = false;
            this.Txt_Log.Text = "";
            // 
            // Txt_File
            // 
            this.Txt_File.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Txt_File.Location = new System.Drawing.Point(8, 29);
            this.Txt_File.Name = "Txt_File";
            this.Txt_File.Size = new System.Drawing.Size(470, 20);
            this.Txt_File.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose your \"game.exe\" :";
            // 
            // Btn_Patch
            // 
            this.Btn_Patch.Enabled = false;
            this.Btn_Patch.Location = new System.Drawing.Point(8, 65);
            this.Btn_Patch.Name = "Btn_Patch";
            this.Btn_Patch.Size = new System.Drawing.Size(141, 35);
            this.Btn_Patch.TabIndex = 3;
            this.Btn_Patch.Text = "Patch Header Only";
            this.Btn_Patch.UseVisualStyleBackColor = true;
            this.Btn_Patch.Click += new System.EventHandler(this.Btn_Patch_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Btn_PatchCrosshair
            // 
            this.Btn_PatchCrosshair.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_PatchCrosshair.Enabled = false;
            this.Btn_PatchCrosshair.Location = new System.Drawing.Point(337, 65);
            this.Btn_PatchCrosshair.Name = "Btn_PatchCrosshair";
            this.Btn_PatchCrosshair.Size = new System.Drawing.Size(141, 35);
            this.Btn_PatchCrosshair.TabIndex = 4;
            this.Btn_PatchCrosshair.Text = "Header + No Crosshair";
            this.Btn_PatchCrosshair.UseVisualStyleBackColor = true;
            this.Btn_PatchCrosshair.Click += new System.EventHandler(this.Btn_PatchCrosshair_Click);
            // 
            // MainWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 324);
            this.Controls.Add(this.Btn_PatchCrosshair);
            this.Controls.Add(this.Btn_Patch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Txt_File);
            this.Controls.Add(this.Txt_Log);
            this.Controls.Add(this.Btn_Open);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWnd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fright Fear Land - Patcher v2.0";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Btn_Open;
        private System.Windows.Forms.RichTextBox Txt_Log;
        private System.Windows.Forms.TextBox Txt_File;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Btn_Patch;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button Btn_PatchCrosshair;
    }
}

