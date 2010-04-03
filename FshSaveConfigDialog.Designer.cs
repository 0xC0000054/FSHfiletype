using FSHfiletype.Properties;
namespace FSHfiletype
{
    partial class FshSaveConfigDialog
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Alphagb = new System.Windows.Forms.GroupBox();
            this.origAlpha = new System.Windows.Forms.RadioButton();
            this.imgtransalphaRadio = new System.Windows.Forms.RadioButton();
            this.genmapRadio = new System.Windows.Forms.RadioButton();
            this.dirnameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Fshtype = new System.Windows.Forms.ComboBox();
            this.genmipBox = new System.Windows.Forms.CheckBox();
            this.Fshtypegb = new System.Windows.Forms.GroupBox();
            this.Alphagb.SuspendLayout();
            this.Fshtypegb.SuspendLayout();
            this.SuspendLayout();
            // 
            // Alphagb
            // 
            this.Alphagb.Controls.Add(this.origAlpha);
            this.Alphagb.Controls.Add(this.imgtransalphaRadio);
            this.Alphagb.Controls.Add(this.genmapRadio);
            this.Alphagb.Location = new System.Drawing.Point(3, 140);
            this.Alphagb.Name = "Alphagb";
            this.Alphagb.Size = new System.Drawing.Size(200, 100);
            this.Alphagb.TabIndex = 29;
            this.Alphagb.TabStop = false;
            this.Alphagb.Text = Properties.Resources.AlphagbText;
            // 
            // origAlpha
            // 
            this.origAlpha.AutoSize = true;
            this.origAlpha.Location = new System.Drawing.Point(10, 23);
            this.origAlpha.Name = "origAlpha";
            this.origAlpha.Size = new System.Drawing.Size(110, 17);
            this.origAlpha.TabIndex = 15;
            this.origAlpha.Text = global::FSHfiletype.Properties.Resources.OrigAlphaText;
            this.origAlpha.UseVisualStyleBackColor = true;
            this.origAlpha.CheckedChanged += new System.EventHandler(this.Alpha_CheckedChanged);
            // 
            // imgtransalphaRadio
            // 
            this.imgtransalphaRadio.AutoSize = true;
            this.imgtransalphaRadio.Checked = true;
            this.imgtransalphaRadio.Location = new System.Drawing.Point(10, 69);
            this.imgtransalphaRadio.Name = "imgtransalphaRadio";
            this.imgtransalphaRadio.Size = new System.Drawing.Size(170, 17);
            this.imgtransalphaRadio.TabIndex = 17;
            this.imgtransalphaRadio.TabStop = true;
            this.imgtransalphaRadio.Text = global::FSHfiletype.Properties.Resources.AlphatransText;
            this.imgtransalphaRadio.UseVisualStyleBackColor = true;
            this.imgtransalphaRadio.CheckedChanged += new System.EventHandler(this.Alpha_CheckedChanged);
            // 
            // genmapRadio
            // 
            this.genmapRadio.AutoSize = true;
            this.genmapRadio.Location = new System.Drawing.Point(10, 46);
            this.genmapRadio.Name = "genmapRadio";
            this.genmapRadio.Size = new System.Drawing.Size(115, 17);
            this.genmapRadio.TabIndex = 16;
            this.genmapRadio.Text = global::FSHfiletype.Properties.Resources.GenmapText;
            this.genmapRadio.UseVisualStyleBackColor = true;
            this.genmapRadio.CheckedChanged += new System.EventHandler(this.Alpha_CheckedChanged);
            // 
            // dirnameBox
            // 
            this.dirnameBox.Location = new System.Drawing.Point(94, 91);
            this.dirnameBox.Name = "dirnameBox";
            this.dirnameBox.Size = new System.Drawing.Size(43, 20);
            this.dirnameBox.TabIndex = 27;
            this.dirnameBox.Text = "FiSH";
            this.dirnameBox.TextChanged += new System.EventHandler(this.dirnameBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Directory name";
            // 
            // Fshtype
            // 
            this.Fshtype.FormattingEnabled = true;
            this.Fshtype.Items.AddRange(new object[] {
            "24 Bit RGB",
            "32 Bit ARGB",
            "DXT1 Compressed, no Alpha",
            "DXT3 Compressed, with Alpha"});
            this.Fshtype.Location = new System.Drawing.Point(1, 23);
            this.Fshtype.Name = "Fshtype";
            this.Fshtype.Size = new System.Drawing.Size(179, 21);
            this.Fshtype.TabIndex = 25;
            this.Fshtype.SelectedIndexChanged += new System.EventHandler(this.Fshtype_SelectedIndexChanged);
            // 
            // genmipBox
            // 
            this.genmipBox.AutoSize = true;
            this.genmipBox.Location = new System.Drawing.Point(13, 117);
            this.genmipBox.Name = "genmipBox";
            this.genmipBox.Size = new System.Drawing.Size(115, 17);
            this.genmipBox.TabIndex = 30;
            this.genmipBox.Text = global::FSHfiletype.Properties.Resources.GenmipText;
            this.genmipBox.UseVisualStyleBackColor = true;
            this.genmipBox.CheckedChanged += new System.EventHandler(this.genmipBox_CheckedChanged);
            // 
            // Fshtypegb
            // 
            this.Fshtypegb.Controls.Add(this.Fshtype);
            this.Fshtypegb.Location = new System.Drawing.Point(3, 35);
            this.Fshtypegb.Name = "Fshtypegb";
            this.Fshtypegb.Size = new System.Drawing.Size(200, 50);
            this.Fshtypegb.TabIndex = 31;
            this.Fshtypegb.TabStop = false;
            this.Fshtypegb.Text = "Fsh Type";
            // 
            // FshSaveConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Fshtypegb);
            this.Controls.Add(this.genmipBox);
            this.Controls.Add(this.Alphagb);
            this.Controls.Add(this.dirnameBox);
            this.Controls.Add(this.label1);
            this.Name = "FshSaveConfigDialog";
            this.Size = new System.Drawing.Size(262, 305);
            this.Alphagb.ResumeLayout(false);
            this.Alphagb.PerformLayout();
            this.Fshtypegb.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox Alphagb;
        internal System.Windows.Forms.RadioButton origAlpha;
        internal System.Windows.Forms.RadioButton imgtransalphaRadio;
        internal System.Windows.Forms.RadioButton genmapRadio;
        internal System.Windows.Forms.TextBox dirnameBox;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.ComboBox Fshtype;
        internal System.Windows.Forms.CheckBox genmipBox;
        private System.Windows.Forms.GroupBox Fshtypegb;
    }
}
