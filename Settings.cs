using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Reflection;

namespace FSHfiletype
{
    internal class Settings
    {
        XmlDocument xmlDocument = new XmlDocument();

        string documentPath = null;
        private void LoadSettings()
        {        
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"FSHfiletype");
            try
            {
                if (Directory.Exists(path))
                {
                    if (File.Exists(Path.Combine(path, "FSHfiletype.xml")))
                    {
                        documentPath = Path.Combine(path, "FSHfiletype.xml");
                    }
                    else
                    {
                        Assembly.GetAssembly(typeof(FshFileType)).GetManifestResourceStream("FSHfiletype.xml");
                        string filepath = Path.Combine(path, "FSHfiletype.xml");
                        using (Stream resourceStream = Assembly.GetAssembly(typeof(FshFileType)).GetManifestResourceStream("FSHfiletype.FSHfiletype.xml"))
                        {
                            // Now read s into a byte buffer.
                            byte[] bytes = new byte[resourceStream.Length];
                            int numBytesToRead = (int)resourceStream.Length;
                            int numBytesRead = 0;
                            while (numBytesToRead > 0)
                            {
                                // Read may return anything from 0 to numBytesToRead.
                                int n = resourceStream.Read(bytes, numBytesRead, numBytesToRead);
                                // The end of the file is reached.
                                if (n == 0)
                                    break;
                                numBytesRead += n;
                                numBytesToRead -= n;
                            }
                            File.WriteAllBytes(filepath, bytes);
                        }
                        if (File.Exists(Path.Combine(path, "FSHfiletype.xml")))
                        {
                            documentPath = Path.Combine(path, "FSHfiletype.xml");
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(path);
                    string filepath = Path.Combine(path, "FSHfiletype.xml");
                    using (Stream resourceStream = Assembly.GetAssembly(typeof(FshFileType)).GetManifestResourceStream("FSHfiletype.FSHfiletype.xml"))
                    { 
                        // Now read s into a byte buffer.
                        byte[] bytes = new byte[resourceStream.Length];
                        int numBytesToRead = (int)resourceStream.Length;
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            // Read may return anything from 0 to numBytesToRead.
                            int n = resourceStream.Read(bytes, numBytesRead, numBytesToRead);
                            // The end of the file is reached.
                            if (n == 0)
                                break;
                            numBytesRead += n;
                            numBytesToRead -= n;
                        }
                        File.WriteAllBytes(filepath, bytes);
                    }
                    if (File.Exists(Path.Combine(path, "FSHfiletype.xml")))
                    {
                        documentPath = Path.Combine(path, "FSHfiletype.xml");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Fsh FileType");
            }
        }      
        public Settings()
        {

            try
            {
                LoadSettings();
                xmlDocument.Load(documentPath); 
            }
            catch { xmlDocument.LoadXml("<settings></settings>"); }
        }

        public int GetSetting(string xPath, int defaultValue)
        { return Convert.ToInt16(GetSetting(xPath, Convert.ToString(defaultValue))); }

        public void PutSetting(string xPath, int value)
        { PutSetting(xPath, Convert.ToString(value)); }

        public string GetSetting(string xPath, string defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode != null) { return xmlNode.InnerText; }
            else { return defaultValue; }
        }

        public void PutSetting(string xPath, string value)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode == null) { xmlNode = createMissingNode("settings/" + xPath); }
            xmlNode.InnerText = value;
            xmlDocument.Save(documentPath);
        }

        private XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("settings");
            foreach (string xPathSection in xPathSections)
            {
                currentXPath += xPathSection;
                testNode = xmlDocument.SelectSingleNode(currentXPath);
                if (testNode == null) { currentNode.InnerXml += "<" + xPathSection + "></" + xPathSection + ">"; }
                currentNode = xmlDocument.SelectSingleNode(currentXPath);
                currentXPath += "/";
            }
            return currentNode;
        }
    }
}
