using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VSAddInLib
{
    /// <summary>
    /// Extremely basic static class containing a couple of functions to save
    /// and load configs without too much hassle.
    /// </summary>
    public static class AddInConfig
    {
        //////////////////////////////////////////////////////////////////////////

        private static string GetConfigFileName(string addInName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), addInName + ".config.xml");
        }

        //////////////////////////////////////////////////////////////////////////

        public static void LoadConfig<T>(string addInName, out T config) where T : class, new()
        {
            config = null;

            try
            {
                string configFileName = GetConfigFileName(addInName);

                using (XmlReader reader = XmlReader.Create(configFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    config = serializer.Deserialize(reader) as T;
                }
            }
            catch (FileNotFoundException)
            {
                // file not found (oddly enough)
            }

            if (config == null)
                config = new T();
        }

        //////////////////////////////////////////////////////////////////////////

        public static void SaveConfig<T>(string addInName, T config)
        {
            try
            {
                string configFileName = GetConfigFileName(addInName);

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();

                xmlWriterSettings.Encoding = new UTF8Encoding(false);//false=no BOM
                xmlWriterSettings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(configFileName, xmlWriterSettings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, config);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // file is not writeable (probably)
            }
        }

        //////////////////////////////////////////////////////////////////////////
    }
}
