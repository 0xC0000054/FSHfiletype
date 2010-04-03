using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PaintDotNet;

namespace FSHfiletype
{
    public partial class FshSaveConfigDialog : SaveConfigWidget
    {
        public FshSaveConfigDialog()
        {
            InitializeComponent();
        }
        protected override void InitFileType()
        {
            this.fileType = new FshFileType();
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
        }

        private void Alpha_CheckedChanged(object sender, EventArgs e)
        {
            if (origAlpha.Checked)
            {
                if (Fshtype.SelectedIndex == 0 || Fshtype.SelectedIndex == 1)
                {
                    Fshtype.SelectedIndex = 1;
                }
                else
                {
                    Fshtype.SelectedIndex = 3;
                }
            }
            else if (genmapRadio.Checked)
            {
                if (Fshtype.SelectedIndex == 0 || Fshtype.SelectedIndex == 1)
                {
                    Fshtype.SelectedIndex = 0;
                }
                else
                {
                    Fshtype.SelectedIndex = 2;
                }
            }
            else if (imgtransalphaRadio.Checked)
            {
                if (Fshtype.SelectedIndex == 0 || Fshtype.SelectedIndex == 1)
                {
                    Fshtype.SelectedIndex = 1;
                }
                else
                {
                    Fshtype.SelectedIndex = 3;
                }
            }
            this.UpdateToken();
        }

        private void Fshtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (origAlpha.Checked)
            {
                if (Fshtype.SelectedIndex == 0 || Fshtype.SelectedIndex == 1)
                {
                    Fshtype.SelectedIndex = 1;
                }
                else
                {
                    Fshtype.SelectedIndex = 3;
                }
            }
            else if (genmapRadio.Checked)
            {
                if (Fshtype.SelectedIndex == 0 || Fshtype.SelectedIndex == 1)
                {
                    Fshtype.SelectedIndex = 0;
                }
                else
                {
                    Fshtype.SelectedIndex = 2;
                }
            }
            else if (imgtransalphaRadio.Checked)
            {
                if (Fshtype.SelectedIndex == 0 || Fshtype.SelectedIndex == 1)
                {
                    Fshtype.SelectedIndex = 1;
                }
                else
                {
                    Fshtype.SelectedIndex = 3;
                }
            }
            this.UpdateToken();
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

        internal string savebmptype = string.Empty;
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


    }
}
