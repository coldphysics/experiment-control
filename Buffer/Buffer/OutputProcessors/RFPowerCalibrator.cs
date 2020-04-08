using System;
using System.IO;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data;

namespace Buffer.OutputProcessors
{
    /// <summary>
    /// Calibrates the output for the Superatoms experiment (ask Michael Schlagmueller or Kathrin Kleinbach)
    /// </summary>
    /// <seealso cref="Buffer.OutputProcessors.OutputProcessor" />
    class RFPowerCalibrator:OutputProcessor
    {
        private const string CALIBRATION_FILES_FOLDER = @"D:\SVN\CycleData\binary\calibrationFiles\";
        private string[] filenames = new string[] { "rf_00", "rf_01", "rf_10", "rf_11" };
        private const string RF_POWER_TO_AMPLITUDE_CALIB_FILE = "rf_power2amplitude.cal";
        private const string CALIBRATED_ANAOLOG_CHANNEL_NANE = "AO3";
        private const string REFERENCE_DIGITAL_CHANNEL = "DO2";

        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static double[,] rfcalib;
        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static double[] power2amplitude;
        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static double oldRFamplitude = -1;
        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static double oldRFfrequency;
        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static bool oldRFdigital1;
        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static bool oldRFdigital2;
        /// <summary>
        /// Used within <see cref="GetCorrectedAmplitude"/> method.
        /// </summary>
        private static double oldRFoutput;



        public RFPowerCalibrator(DataModel model)
            : base(model)
        { }

        public override void Process(IModelOutput output)
        {
            //@MS: remove these hard-coded calibration definitions!
            //modify with calib curves
            AdjustRFPower((AnalogCardOutput)output.Output[CALIBRATED_ANAOLOG_CHANNEL_NANE], (DigitalCardOutput)output.Output[REFERENCE_DIGITAL_CHANNEL]); // FIXME - hard coded calibration curve
        }

        /// <summary>
        /// Code specific to Superatoms calibration
        /// </summary>
        /// <param name="a">?</param>
        /// <param name="d">?</param>
        private void AdjustRFPower(object a, object d)
        {
            AnalogCardOutput cardAnalog = (AnalogCardOutput) a;
            DigitalCardOutput cardDigital = (DigitalCardOutput)d;

            const int channelAmplitude = 4;
            const int channelFrequency = 5;
            const int switchA = 40 - 32;
            const int switchB = 41 - 32;
            const uint maskA = 1 << switchA;
            const uint maskB = 1 << switchB;
            long elements = cardAnalog.Output.GetLength(1);

            for (long i = 0; i < elements; i++)
            {
                double amplitude = cardAnalog.Output[channelAmplitude, i];
                double frequency = cardAnalog.Output[channelFrequency, i];
                uint mask = cardDigital.Output[i];
                bool stateA = false;
                bool stateB = false;

                if ((mask & maskA) != 0)
                    stateA = true;

                if ((mask & maskB) != 0)
                    stateB = true;

                double correctedAmplitude = GetCorrectedAmplitude(amplitude, frequency, stateA, stateB);
                cardAnalog.Output[channelAmplitude, i] = correctedAmplitude;
            }
        }

        /// <summary>
        /// Code specific to Superatoms calibration
        /// </summary>
        /// <param name="rfAmplitude">The rf amplitude.</param>
        /// <param name="rfFrequency">The rf frequency.</param>
        /// <param name="rfDigital1">if set to <c>true</c> [rf digital1].</param>
        /// <param name="rfDigital2">if set to <c>true</c> [rf digital2].</param>
        /// <returns>?</returns>
        /// <exception cref="System.InvalidOperationException">
        /// size should be 10001
        /// or
        /// size should be 10001
        /// </exception>
        private double GetCorrectedAmplitude(double rfAmplitude, double rfFrequency, bool rfDigital1, bool rfDigital2)
        {
            bool showoutput = false;

            if (showoutput)
            {
                Console.WriteLine("Input amplitude = " + rfAmplitude);
                Console.WriteLine("Input frequency = " + rfFrequency);
            }

            // RF AMPLITUDE
            // check if calibration file is already loaded
            if (rfcalib == null)
            {
                rfcalib = new double[4, 10001];


                for (int iFile = 0; iFile < 4; iFile++)
                {
                    var binaryInput = new BinaryReader(File.OpenRead(CALIBRATION_FILES_FOLDER + filenames[iFile] + ".cal"));
                    var byteLength = (int)binaryInput.BaseStream.Length;
                    int length = (int)(byteLength / sizeof(float));
                    if (length != 10001)
                    {
                        throw new System.InvalidOperationException("size should be 10001");
                    }

                    int pos = 0;

                    while (pos < length)
                    {
                        rfcalib[iFile, pos] = binaryInput.ReadSingle();
                        pos++;
                    }

                    binaryInput.Close();
                }
                {
                    power2amplitude = new double[10001];
                    var binaryInput = new BinaryReader(File.OpenRead(CALIBRATION_FILES_FOLDER + RF_POWER_TO_AMPLITUDE_CALIB_FILE));
                    var byteLength = (int)binaryInput.BaseStream.Length;
                    int length = (int)(byteLength / sizeof(float));

                    if (length != 10001)
                    {
                        throw new System.InvalidOperationException("size should be 10001");
                    }

                    int pos = 0;

                    while (pos < length)
                    {
                        power2amplitude[pos] = binaryInput.ReadSingle();
                        pos++;
                    }

                    binaryInput.Close();
                }
            }

            // correct rf amplitude
            double rfOutput = 0;
            // caching - if everything is the same - take the old value
            if (rfAmplitude == oldRFamplitude && rfDigital1 == oldRFdigital1 && rfDigital2 == oldRFdigital2 && rfFrequency == oldRFfrequency)
            {
                rfOutput = oldRFoutput;
            }
            else if (rfAmplitude == 0)
            {
                rfOutput = 0;
            }
            else
            {
                // calculate calibration value
                int range = 0;

                if (rfDigital2)
                {
                    range += 1;
                }
                if (rfDigital1)
                {
                    range += 2;
                }
                if (showoutput)
                {
                    System.Console.WriteLine("range: " + range);
                }
                {//WTF
                    double position = rfFrequency / 10.0 * 10000.0;
                    int posLow = (int)Math.Floor(position);
                    double diff = position - posLow;

                    if (diff == 0)
                    {
                        rfOutput = rfcalib[range, posLow];
                    }
                    else
                    {
                        rfOutput = rfcalib[range, posLow] * (1 - diff) + rfcalib[range, posLow + 1] * diff;
                    }
                }
                if (showoutput)
                {
                    System.Console.WriteLine("freq power: " + rfOutput);
                }
                // rfOutput contains the output power 0: no correction required, >0 attenuation the power

                double aquiredPower = (rfAmplitude / 10.0 - 1) * 53;

                if (showoutput)
                {
                    System.Console.WriteLine("acquired power: " + aquiredPower);
                }
                double correctedPower = aquiredPower - rfOutput;
                {
                    double position = (correctedPower + 53) / (53.0) * 10000.0;
                    if (position < 0)
                    {
                        position = 0;
                    }
                    if (position > 10000)
                    {
                        position = 10000;
                    }

                    int posLow = (int)Math.Floor(position);
                    double diff = position - posLow;

                    if (diff == 0)
                    {
                        rfOutput = power2amplitude[posLow];
                    }
                    else
                    {
                        rfOutput = power2amplitude[posLow] * (1 - diff) + power2amplitude[posLow + 1] * diff;
                    }
                }
            }
            if (showoutput)
            {
                System.Console.WriteLine("output amplitude: " + rfOutput);
            }
            oldRFamplitude = rfAmplitude;
            oldRFfrequency = rfFrequency;
            oldRFoutput = rfOutput;
            oldRFdigital1 = rfDigital1;
            oldRFdigital2 = rfDigital2;

            return rfOutput;
        }
    }
}
