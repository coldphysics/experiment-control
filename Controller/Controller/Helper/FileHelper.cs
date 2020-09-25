using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace Controller.Helper
{
    public class FileHelper
    {
        public static string PickFilePath(string defaultExtension = ".xml.gz", string filter = "Sequence (.xml.gz)|*.xml.gz")
        {
            var fileDialog = new SaveFileDialog { DefaultExt = defaultExtension, Filter = filter };

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                return fileDialog.FileName;
            }

            return null;
        }

        public static string SaveFile(object modelToSave, string defaultExtension = ".xml.gz", string filter = "Sequence (.xml.gz)|*.xml.gz")
        {
            string filePath = PickFilePath(defaultExtension, filter);

            if (filePath != null)
            {
                SaveFile(filePath, modelToSave);

                return filePath;
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

        public static string GenerateTemporaryFilePath(string fileExtension)
        {
            return Path.GetTempPath() + Guid.NewGuid().ToString() + fileExtension;
        }

        /// <summary>
        /// Creates a temporary file (i.e., that resides in the temp directory of the current user) with a random name, and the specified
        /// extension, and populates with the contents of the specified string.
        /// </summary>
        /// <param name="fileContent">The content of the file</param>
        /// <param name="fileExtension">The extension of the file</param>
        /// <returns>The full path of the created file</returns>
        public static string CreateTemporaryFile(string fileContent, string fileExtension)
        {
            string temporaryFilePath = GenerateTemporaryFilePath(fileExtension);
            File.WriteAllText(temporaryFilePath, fileContent);

            return temporaryFilePath;
        }
    }
}
