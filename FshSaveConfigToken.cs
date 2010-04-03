using System;
using PaintDotNet;

namespace FSHfiletype
{
	[Serializable]
	public class FshSaveConfigToken : SaveConfigToken
	{
        private bool useorigalpha;
        private bool alphatrans;
        private bool genmap;
        private int fshtype;
        private string dirname;
        private bool genmipenabled;
        private bool genmip;

        public bool OrigAlpha
        {
            get
            {
                return useorigalpha;
            }
            set
            {
                useorigalpha = value;
            }
        }
        public bool Alphatrans
        {
            get
            {
                return alphatrans;
            }
            set
            {
                alphatrans = value;
            }
        }
        public bool Genmap
        {

            get
            {
                return genmap;
            }
            set
            {
                genmap = value;
            }
        }
        public int Fshtype
        {

            get
            {
                return fshtype;
            }
            set
            {
                fshtype = value;
            }
        }
        public string Dirname
        {

            get
            {
                return dirname;
            }
            set
            {
                dirname = value;
            }
        }
        public bool Genmip
        {

            get
            {
                return genmip;
            }
            set
            {
                genmip = value;
            }
        }
        public bool GenmipEnabled
        {

            get
            {
                return genmipenabled;
            }
            set
            {
                genmipenabled = value;
            }
        }

        public FshSaveConfigToken(bool useorigalpha, bool genmap, bool alphatrans, int Fshtype, string dirname, bool genmipenabled, bool genmip)
            : base()
        {
            this.useorigalpha = useorigalpha;
            this.genmap = genmap;
            this.alphatrans = alphatrans;
            this.Fshtype = Fshtype;
            this.dirname = dirname;
            this.genmipenabled = genmipenabled;
            this.genmip = genmip;
        }



        protected FshSaveConfigToken(FshSaveConfigToken copyMe)
		{
            this.useorigalpha = copyMe.useorigalpha;
            this.genmap = copyMe.genmap;
            this.alphatrans = copyMe.alphatrans;
            this.Fshtype = copyMe.Fshtype;
            this.dirname = copyMe.dirname;
            this.genmipenabled = copyMe.genmipenabled;
            this.genmip = copyMe.genmip;
		}

		public override void Validate()
		{
            if (fshtype < 0 || fshtype > 3)
            {
                throw new ArgumentOutOfRangeException("fshtype", "The value of fshtype must be between 0 and 3");
            }
		}
        public override object Clone()
		{
			return new FshSaveConfigToken(this);
		}

		
	}
}
