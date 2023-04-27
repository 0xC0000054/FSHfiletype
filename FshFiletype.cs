/*
*  This file is part of fsh-filetype, a filetype plug-in for Paint.NET
*  that loads and saves FSH images.
*
*  Copyright (C) 2009, 2010, 2011, 2012, 2014, 2015, 2023 Nicholas Hayes
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*/

using System.Collections.Generic;
using System.IO;
using FSHfiletype.Properties;
using PaintDotNet;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace FSHfiletype
{

    public sealed class FshFileType : PropertyBasedFileType, IFileTypeFactory
    {
        public FshFileType()
            : base("EA Fsh", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving | FileTypeFlags.SupportsLayers, new string[] { ".fsh" })
        {
        }

        public override PaintDotNet.PropertySystem.PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(StaticListChoiceProperty.CreateForEnum<FshFileFormat>(PropertyNames.FileType, FshFileFormat.DXT1, false));
            props.Add(new StringProperty(PropertyNames.DirectoryName, "0000", 4));
            props.Add(new BooleanProperty(PropertyNames.FshWriteCompression, false));

            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();
            
            Pair<object, object>[] values = new Pair<object, object>[]{ Pair.Create<object, object>(PropertyNames.FileType, FshFileFormat.DXT1), 
                Pair.Create<object, object>(PropertyNames.FileType, FshFileFormat.DXT3)};
            
            rules.Add(new ReadOnlyBoundToNameValuesRule(PropertyNames.FshWriteCompression, true, values));

            return new PropertyCollection(props, rules);
        }

        public override PaintDotNet.IndirectUI.ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {

            ControlInfo info = PropertyBasedFileType.CreateDefaultSaveConfigUI(props);
            info.SetPropertyControlValue(PropertyNames.FileType, ControlInfoPropertyNames.DisplayName, Resources.FshtypeText);
            PropertyControlInfo fileTypePCI = info.FindControlForPropertyName(PropertyNames.FileType);
            fileTypePCI.SetValueDisplayName(FshFileFormat.TwentyFourBit, Resources.FshFileFormat24Bit);
            fileTypePCI.SetValueDisplayName(FshFileFormat.ThirtyTwoBit, Resources.FshFileFormat32Bit);
            fileTypePCI.SetValueDisplayName(FshFileFormat.SixteenBit, Resources.FshFileFormat16Bit);
            fileTypePCI.SetValueDisplayName(FshFileFormat.SixteenBitAlpha, Resources.FshFileFormat16BitAlpha);
            fileTypePCI.SetValueDisplayName(FshFileFormat.SixteenBit4x4, Resources.FshFileFormat16Bit4x4);
            fileTypePCI.SetValueDisplayName(FshFileFormat.DXT1, Resources.FshFileFormatDXT1);
            fileTypePCI.SetValueDisplayName(FshFileFormat.DXT3, Resources.FshFileFormatDXT3);
            
            info.SetPropertyControlType(PropertyNames.DirectoryName, PropertyControlType.TextBox);
            info.SetPropertyControlValue(PropertyNames.DirectoryName, ControlInfoPropertyNames.DisplayName, Resources.DirNameText);

            info.SetPropertyControlType(PropertyNames.FshWriteCompression, PropertyControlType.CheckBox);
            info.SetPropertyControlValue(PropertyNames.FshWriteCompression, ControlInfoPropertyNames.DisplayName, string.Empty);
            info.SetPropertyControlValue(PropertyNames.FshWriteCompression, ControlInfoPropertyNames.Description, Resources.FshWriteText);

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