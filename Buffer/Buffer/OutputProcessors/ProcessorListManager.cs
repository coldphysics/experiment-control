using System.Collections.Generic;
using Buffer.OutputProcessors.CalibrationUnit;
using Buffer.OutputProcessors.Compression;
using Buffer.OutputProcessors.Quantization;
using Buffer.OutputProcessors.ValidationUnit;
using Model.Root;
using Model.Settings;
using Buffer.OutputProcessors.SaveOutputAfterReplication;

namespace Buffer.OutputProcessors
{
    /// <summary>
    /// Creates a list of <see cref=" OutputProcessor"/>'s based on the current settings values.
    /// </summary>
    public class ProcessorListManager
    {

        //Ebaa
        public OutputSaver saver { get; set; }
        /// <summary>
        /// Prevents a default instance of the <see cref="ProcessorListManager"/> class from being created.
        /// </summary>
        private ProcessorListManager()
        { }

        private static ProcessorListManager instance;

        public static ProcessorListManager GetInstance()
        {
            if (instance == null)
                instance = new ProcessorListManager();

            return instance;
        }

        public ICollection<OutputProcessor> GetOutputProcessorList(RootModel model, int timesToReplicateOutput)
        {
            List<OutputProcessor> result = new List<OutputProcessor>();
            ProfilesManager manager = ProfilesManager.GetInstance();
            DigitalChannelInverter inverter = new DigitalChannelInverter(model.Data);
            OutputCalibrator calibrator = new OutputCalibrator(model.Data);
            OutputValidator validator = new OutputValidator(model.Data);
            OutputReplicator replicator = new OutputReplicator(model.Data, timesToReplicateOutput);
            //Ebaa
            saver = new OutputSaver(model.Data);
            result.Add(inverter);
            result.Add(calibrator);


            HW_TYPES hwType = Global.GetHardwareType();
            //RECO This tightly couples the RFPowerCalibration with any experiment with NI_CHASSIS hardware
            //TODO This tightly couples the RFPowerCalibration with any experiment with NI_CHASSIS hardware
            //FIXME This tightly couples the RFPowerCalibration with any experiment with NI_CHASSIS hardware
            //$ Setting This tightly couples the RFPowerCalibration with any experiment with NI_CHASSIS hardware
            if (hwType == HW_TYPES.NI_CHASSIS)
            {
                RFPowerCalibrator rfCalibrator = new RFPowerCalibrator(model.Data);
                result.Add(rfCalibrator);
            }

            result.Add(validator);
            result.Add(replicator);//Replication should be done before compression!
            //Ebaa- save the output before the quantization and compression steps.
            result.Add(saver);

            //Here all hardware types expecting quantization should be listed
            if (hwType == HW_TYPES.AdWin_Simulator || hwType == HW_TYPES.AdWin_T11 || hwType == HW_TYPES.AdWin_T12)
            {
                OutputQuantizer quantizer = new OutputQuantizer(model.Data);
                result.Add(quantizer);

                if (manager.ActiveProfile.DoesSettingExist(SettingNames.COMPRESSION))
                    if (manager.ActiveProfile.GetSettingValueByName<bool>(SettingNames.COMPRESSION))
                    {
                        OutputCompressor compressor = new OutputCompressor(model.Data);
                        result.Add(compressor);
                    }

            }

            return result;
        }
    }
}
