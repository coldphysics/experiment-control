using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Xml;
using Model.MeasurementRoutine;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class ModelFileNotFoundEventArgs : EventArgs
    {
        public string ModelFileName
        { get; set; }

        public string NewModelFileName
        { set; get; }

        public bool Cancel
        { set; get; }

        public bool Retry
        { set; get; }

    }

    public delegate void ModelFileNotFoundDelegate(object sender, ModelFileNotFoundEventArgs e);

    public class MeasurementRoutineLoader
    {
        private ModelLoader loader;
        public event ModelFileNotFoundDelegate ModelFileNotFound;
        /// <summary>
        /// Occurs when the structure of the model currently being loaded does not match with the settings.
        /// </summary>
        public event ModelStructureMismatchDelegate ModelStructureMismatchDetected;
       

        /// <summary>
        /// Occurs when the version of the model currently being loaded does not match with the current model version.
        /// </summary>
        public event ModelVersionMismatchDelegate ModelVersionMismatchDetected;

        public MeasurementRoutineLoader()
        {
            loader = new ModelLoader();
            loader.ModelStructureMismatchDetected +=
                (sender, args) =>
                {
                    if (ModelStructureMismatchDetected != null)
                        ModelStructureMismatchDetected(sender, args);
                };
            loader.ModelVersionMismatchDetected +=
                (sender, args) =>
                {
                    if (ModelVersionMismatchDetected != null)
                        ModelVersionMismatchDetected(sender, args);
                };
        }

        private bool LoadRoutineBasedRootModel(RoutineBasedRootModel model)
        {
            while (!File.Exists(model.FilePath))
            {
                if (ModelFileNotFound != null)
                {
                    ModelFileNotFoundEventArgs args = new ModelFileNotFoundEventArgs
                    {
                        Cancel = false,
                        Retry = false,
                        ModelFileName = model.FilePath,
                        NewModelFileName = ""
                    };

                    ModelFileNotFound(this, args);

                    if (args.Cancel)
                        return false;

                    if (args.Retry)
                        continue;

                    if (String.IsNullOrEmpty(args.NewModelFileName))
                        model.FilePath = args.NewModelFileName;
                }
                else
                {
                    return false;
                }
            }

            if (File.Exists(model.FilePath))//This should be always true here!
            {
                model.Counters = new ModelSpecificCounters();
                model.ActualModel = loader.LoadModel(model.FilePath);

                if(model.ActualModel != null)//Loading the file can still produce errors!
                    return true;

                return false;
            }
            else
            {
                return false;//We should not encounter this
            }
        }

        public MeasurementRoutineModel LoadMeasurementRoutine(string filepath)
        {
            MeasurementRoutineModel result = null;

            using (FileStream stream = new FileStream(filepath, FileMode.Open))
            {
                using (GZipStream gz = new GZipStream(stream, CompressionMode.Decompress, false))
                {
                    var deserializer = new DataContractSerializer(typeof(MeasurementRoutineModel));
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(gz, new XmlDictionaryReaderQuotas());
                    result = (MeasurementRoutineModel)deserializer.ReadObject(reader, true);
                }
            }

            if (result != null)
            {
                if (LoadRoutineBasedRootModel(result.PrimaryModel))
                {
                    foreach (RoutineBasedRootModel secondary in result.SecondaryModels)
                    {
                        if (!LoadRoutineBasedRootModel(secondary))//All secondary models have to load correctly
                        {
                            result = null;
                            break;
                        }

                    }
                }
                else
                {
                    result = null;//Actual primary root model is missing
                }
            }


            return result;
        }
    }
}
