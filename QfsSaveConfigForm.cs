using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using FSHLib;
using PaintDotNet;


namespace FSHfiletype
{
    public class QfsSaveConfigDialog : SaveConfigWidget
    {
        internal ComboBox Fshtype;
        internal RadioButton genmapRadio;
        internal RadioButton origAlpha;
        private Label label1;
        internal TextBox dirnameBox;
        private Label fshtypelbl;
        internal RadioButton imgtransalphaRadio;
        internal CheckBox genmipBox;
        private GroupBox Alphagb;
        private CheckBox Fshwritecompcb;
        private OpenFileDialog openFileDialog1;

        public QfsSaveConfigDialog()
        {
            InitializeComponent();
        }
        protected override void InitFileType()
        {
            this.fileType = new QfsFileType();
        }
        protected override void InitWidgetFromToken(SaveConfigToken Savetoken)
        {
            if (Savetoken is FshSaveConfigToken)
            {
                FshSaveConfigToken token = (FshSaveConfigToken)Savetoken;
                Fshtype.SelectedIndex = token.Fshtype;
                genmapRadio.Checked = token.Genmap;
                origAlpha.Checked = token.OrigAlpha;
                imgtransalphaRadio.Checked = token.Alphatrans;
                dirnameBox.Text = token.Dirname;
                genmipBox.Checked = token.Genmip;
                genmipBox.Enabled = token.GenmipEnabled;
                Fshwritecompcb.Checked = token.FshwriteComp;
            }
            else
            {
                Fshtype.SelectedIndex = 2;
                genmapRadio.Checked = false;
                origAlpha.Checked = false;
                imgtransalphaRadio.Checked = true;
                dirnameBox.Text = "FiSH";
                genmipBox.Checked = true;
                genmipBox.Enabled = false;
                Fshwritecompcb.Checked = true;
            }
        }
        protected override void InitTokenFromWidget()
        {
            ((FshSaveConfigToken)token).Alphatrans = imgtransalphaRadio.Checked;
            ((FshSaveConfigToken)token).Dirname = dirnameBox.Text;
            ((FshSaveConfigToken)token).Genmap = genmapRadio.Checked;
            ((FshSaveConfigToken)token).OrigAlpha = origAlpha.Checked;
            ((FshSaveConfigToken)token).Genmip = genmipBox.Checked;
            ((FshSaveConfigToken)token).Fshtype = Fshtype.SelectedIndex;
            ((FshSaveConfigToken)token).FshwriteComp = Fshwritecompcb.Checked;
        }

        private void InitializeComponent()
        {
            this.Fshtype = new System.Windows.Forms.ComboBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.imgtransalphaRadio = new System.Windows.Forms.RadioButton();
            this.genmapRadio = new System.Windows.Forms.RadioButton();
            this.origAlpha = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.dirnameBox = new System.Windows.Forms.TextBox();
            this.fshtypelbl = new System.Windows.Forms.Label();
            this.genmipBox = new System.Windows.Forms.CheckBox();
            this.Alphagb = new System.Windows.Forms.GroupBox();
            this.Fshwritecompcb = new System.Windows.Forms.CheckBox();
            this.Alphagb.SuspendLayout();
            this.SuspendLayout();
            // 
            // Fshtype
            // 
            this.Fshtype.FormattingEnabled = true;
            this.Fshtype.Items.AddRange(new object[] {
            "24 Bit RGB",
            "32 Bit ARGB",
            "DXT1 Compressed, no Alpha",
            "DXT3 Compressed, with Alpha"});
            this.Fshtype.Location = new System.Drawing.Point(76, 39);
            this.Fshtype.Name = "Fshtype";
            this.Fshtype.Size = new System.Drawing.Size(193, 21);
            this.Fshtype.TabIndex = 5;
            this.Fshtype.SelectedIndexChanged += new System.EventHandler(this.Fshtype_SelectedIndexChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // imgtransalphaRadio
            // 
            this.imgtransalphaRadio.AutoSize = true;
            this.imgtransalphaRadio.Checked = true;
            this.imgtransalphaRadio.Location = new System.Drawing.Point(17, 70);
            this.imgtransalphaRadio.Name = "imgtransalphaRadio";
            this.imgtransalphaRadio.Size = new System.Drawing.Size(170, 17);
            this.imgtransalphaRadio.TabIndex = 17;
            this.imgtransalphaRadio.TabStop = true;
            this.imgtransalphaRadio.Text = "Alpha from image transparency";
            this.imgtransalphaRadio.UseVisualStyleBackColor = true;
            this.imgtransalphaRadio.CheckedChanged += new System.EventHandler(this.AlphaRadios_CheckedChanged);
            // 
            // genmapRadio
            // 
            this.genmapRadio.AutoSize = true;
            this.genmapRadio.Location = new System.Drawing.Point(17, 47);
            this.genmapRadio.Name = "genmapRadio";
            this.genmapRadio.Size = new System.Drawing.Size(115, 17);
            this.genmapRadio.TabIndex = 16;
            this.genmapRadio.Text = "Generate new map";
            this.genmapRadio.UseVisualStyleBackColor = true;
            this.genmapRadio.CheckedChanged += new System.EventHandler(this.AlphaRadios_CheckedChanged);
            // 
            // origAlpha
            // 
            this.origAlpha.AutoSize = true;
            this.origAlpha.Location = new System.Drawing.Point(17, 24);
            this.origAlpha.Name = "origAlpha";
            this.origAlpha.Size = new System.Drawing.Size(110, 17);
            this.origAlpha.TabIndex = 15;
            this.origAlpha.Text = "Use original Alpha";
            this.origAlpha.UseVisualStyleBackColor = true;
            this.origAlpha.CheckedChanged += new System.EventHandler(this.AlphaRadios_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Directory name";
            // 
            // dirnameBox
            // 
            this.dirnameBox.Location = new System.Drawing.Point(107, 66);
            this.dirnameBox.Name = "dirnameBox";
            this.dirnameBox.Size = new System.Drawing.Size(43, 20);
            this.dirnameBox.TabIndex = 18;
            this.dirnameBox.Text = "FiSH";
            this.dirnameBox.TextChanged += new System.EventHandler(this.dirnameBox_TextChanged);
            // 
            // fshtypelbl
            // 
            this.fshtypelbl.AutoSize = true;
            this.fshtypelbl.Location = new System.Drawing.Point(20, 42);
            this.fshtypelbl.Name = "fshtypelbl";
            this.fshtypelbl.Size = new System.Drawing.Size(50, 13);
            this.fshtypelbl.TabIndex = 19;
            this.fshtypelbl.Text = "Fsh type:";
            // 
            // genmipBox
            // 
            this.genmipBox.AutoSize = true;
            this.genmipBox.Location = new System.Drawing.Point(23, 92);
            this.genmipBox.Name = "genmipBox";
            this.genmipBox.Size = new System.Drawing.Size(115, 17);
            this.genmipBox.TabIndex = 23;
            this.genmipBox.Text = "Generate Mipmaps";
            this.genmipBox.UseVisualStyleBackColor = true;
            this.genmipBox.CheckedChanged += new System.EventHandler(this.genmipBox_CheckedChanged);
            // 
            // Alphagb
            // 
            this.Alphagb.Controls.Add(this.origAlpha);
            this.Alphagb.Controls.Add(this.imgtransalphaRadio);
            this.Alphagb.Controls.Add(this.genmapRadio);
            this.Alphagb.Location = new System.Drawing.Point(23, 138);
            this.Alphagb.Name = "Alphagb";
            this.Alphagb.Size = new System.Drawing.Size(200, 100);
            this.Alphagb.TabIndex = 24;
            this.Alphagb.TabStop = false;
            this.Alphagb.Text = "Alpha map";
            // 
            // Fshwritecompcb
            // 
            this.Fshwritecompcb.AutoSize = true;
            this.Fshwritecompcb.Location = new System.Drawing.Point(23, 115);
            this.Fshwritecompcb.Name = "Fshwritecompcb";
            this.Fshwritecompcb.Size = new System.Drawing.Size(127, 17);
            this.Fshwritecompcb.TabIndex = 33;
            this.Fshwritecompcb.Text = "Fshwrite compression";
            this.Fshwritecompcb.UseVisualStyleBackColor = true;
            this.Fshwritecompcb.CheckedChanged += new System.EventHandler(this.Fshwritecompcb_CheckedChanged);
            // 
            // QfsSaveConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.Fshwritecompcb);
            this.Controls.Add(this.Alphagb);
            this.Controls.Add(this.genmipBox);
            this.Controls.Add(this.fshtypelbl);
            this.Controls.Add(this.dirnameBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Fshtype);
            this.Name = "QfsSaveConfigDialog";
            this.Size = new System.Drawing.Size(281, 324);
            this.Alphagb.ResumeLayout(false);
            this.Alphagb.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
       
        private void AlphaRadios_CheckedChanged(object sender, EventArgs e)
        {
            if (origAlpha.Checked)
            {
                Fshtype.SelectedIndex = 3;
            }
            else if (genmapRadio.Checked)
            {
                Fshtype.SelectedIndex = 2;
            }
            else if (imgtransalphaRadio.Checked)
            {
                Fshtype.SelectedIndex = 3;
            }
            UpdateToken();
        }
       

        internal string savebmptype = null;
        protected override void OnLoad(EventArgs e)
        {
            if (!string.IsNullOrEmpty(savebmptype))
            {
                switch (savebmptype)
                {
                    case "TwentyFourBit":
                        Fshtype.SelectedIndex = 0;
                        break;
                    case "ThirtyTwoBit":
                        Fshtype.SelectedIndex = 1;
                        break;
                    case "DXT1":
                        Fshtype.SelectedIndex = 2;
                        break;
                    case "DXT3":
                        Fshtype.SelectedIndex = 3;
                        break;
                }
            }
            base.OnLoad(e);
        }
        
        private void dirnameBox_TextChanged(object sender, EventArgs e)
        {
            if ((dirnameBox.Text.Length > 0) && dirnameBox.Text.Length == 4)
            {
                this.UpdateToken();
            }
        }

        private void genmipBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }

        private void Fshtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }

        private void Fshwritecompcb_CheckedChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }
           
        
        
    }
}