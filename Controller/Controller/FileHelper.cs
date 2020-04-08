using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace Controller
{
    public class FileHelper
    {
        public static string SaveFile(object modelToSave, string defaultExtension = ".xml.gz", string filter = "Sequence (.xml.gz)|*.xml.gz")
        {
            var fileDialog = new SaveFileDialog { DefaultExt = defaultExtension, Filter = filter };

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                SaveFile(fileDialog.FileName, modelToSave);
                return fileDialog.FileName;
            }
            return null;

        }

        public static void SaveFile(string filePath, object modelToSave)
        {
            using (var writer = new FileStream(filePath, FileMode.Create))
            {
                using (var gz = new GZipStream(writer, CompressionMode.Compress, false))
                {
                    Type type = modelToSave.GetType();
                    var serializer = new DataContractSerializer(type);
                    serializer.WriteObject(gz, modelToSave);
                }
            }
        }
    }
}
