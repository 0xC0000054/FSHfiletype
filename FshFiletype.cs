using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using PaintDotNet;
using PaintDotNet.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PaintDotNet.PropertySystem;
using PaintDotNet.IndirectUI;
using FSHfiletype.Properties;

namespace FSHfiletype
{

    public sealed class FshFileType : PropertyBasedFileType, IFileTypeFactory
    {
        public FshFileType()
            : base("Fsh", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving | FileTypeFlags.SupportsLayers, new string[] { ".fsh" })
        {
        }

        public override PaintDotNet.PropertySystem.PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(StaticListChoiceProperty.CreateForEnum<FshFileFormat>(PropertyNames.FileType, FshFileFormat.DXT1, false));
            props.Add(new StringProperty(PropertyNames.DirectoryName, "FiSH", 4));
            props.Add(new BooleanProperty(PropertyNames.FshWriteCompression, false));
            props.Add(new BooleanProperty(PropertyNames.AlphaFromTransparency, true));

            return new PropertyCollection(props);
        }

        public override PaintDotNet.IndirectUI.ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {

            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.FileType, ControlInfoPropertyNames.DisplayName, Resources.FshtypeText);
            info.FindControlForPropertyName(PropertyNames.FileType).SetValueDisplayName(FshFileFormat.TwentyFourBit, Resources.FshFileFormat24Bit);
            info.FindControlForPropertyName(PropertyNames.FileType).SetValueDisplayName(FshFileFormat.ThirtyTwoBit, Resources.FshFileFormat32Bit);
            info.FindControlForPropertyName(PropertyNames.FileType).SetValueDisplayName(FshFileFormat.DXT1, Resources.FshFileFormatDXT1);
            info.FindControlForPropertyName(PropertyNames.FileType).SetValueDisplayName(FshFileFormat.DXT3, Resources.FshFileFormatDXT3);
            
            info.SetPropertyControlType(PropertyNames.DirectoryName, PropertyControlType.TextBox);
            info.SetPropertyControlValue(PropertyNames.DirectoryName, ControlInfoPropertyNames.DisplayName, Resources.DirNameText);

            info.SetPropertyControlType(PropertyNames.FshWriteCompression, PropertyControlType.CheckBox);
            info.SetPropertyControlValue(PropertyNames.FshWriteCompression, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(PropertyNames.FshWriteCompression, ControlInfoPropertyNames.Description, Resources.FshWriteText);

            info.SetPropertyControlType(PropertyNames.AlphaFromTransparency, PropertyControlType.CheckBox);
            info.SetPropertyControlValue(PropertyNames.AlphaFromTransparency, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(PropertyNames.AlphaFromTransparency, ControlInfoPropertyNames.Description, Resources.AlphatransText);

            return info;
        }

        public FileType[] GetFileTypeInstances()
        {
            return new FileType[] { new FshFileType() };
        }

        protected override Document OnLoad(Stream input)
        {
            FshFile fsh = new FshFile();
            return fsh.Load(input);
        }


        protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            FshFile fsh = new FshFile();
            fsh.Save(input, output, token, scratchSurface);
        }
    }
}