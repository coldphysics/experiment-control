using Communication.Interfaces.Generator;
using Generator.Generator.Channel;
using Generator.Generator.Concatenator;
using Generator.Generator.Step.Abstract;
using LumenWorks.Framework.IO.Csv;
using Model.Data.Steps;
using System;
using System.IO;

namespace Generator.Generator.Step
{
    /// <summary>
    /// A generator that is capable of generating the output of a single "analog" step that is specified in a file (CSV or binary)
    /// </summary>
    /// <seealso cref="AbstractController.Data.Steps.AbstractStepController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class AnalogStepFileOutputGenerator : BasicFileStepOutputGenerator, IValueGenerator
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogStepFileOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public AnalogStepFileOutputGenerator(StepFileModel model, ChannelOutputGenerator parent)
            : base(model, parent)
        {
        }

        #region IValueGenerator Members

        /// <summary>
        /// Checks whether the file specified by the model exists. 
        /// It then reads the file (CSV or binary) and puts its content in an array and adds it to the final output array of time-steps.
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        /// <exception cref="System.Exception">File does not exist:  + _model.FileName</exception>
        /// <remarks>
        /// If the binary file format is used, then each 4 consecutive bytes in the file are considered the value of a time-step (float value).
        /// And if the CSV file format is used, then each cell value from the first column is considered the value of a time-step (double value).
        /// </remarks>
        public void Generate(IConcatenator concatenator)
        {
            //check whether the file exists
            if (!File.Exists(Model.FileName))
                throw new Exception("File does not exist: " + Model.FileName);
            //RECO file reading should be made safer using the "using" keyword or try-catch-finally

            //the file is a binary file
            if (Model.Store == StepFileModel.StoreType.Binary)
            {
                var binaryInput = new BinaryReader(File.OpenRead(Model.FileName));
                var byteLength = (int)binaryInput.BaseStream.Length;
                int length = (byteLength / sizeof(float));//RECO read doubles from the file instead of floats
                int pos = 0;
                var steps = new double[length];

                //loop through all bytes stored in the binary file
                while (pos < length)
                {
                    steps[pos] = binaryInput.ReadSingle();
                    pos++;
                }

                binaryInput.Close();

                var realConcatenator = (OutputConcatenator)concatenator;
                //add the output array to the overall output array
                realConcatenator.AddSteps(steps);
            }//the file is a CSV file
            else if (Model.Store == StepFileModel.StoreType.Csv)
            {
                var csv = new CsvReader(new StreamReader(Model.FileName), false);
                //RECO do not read the entire file and throw it away just to figure out the number of records!!!!!!! (use a list instead)
                int length = File.ReadAllLines(Model.FileName).Length;
                var steps = new double[length];
                int iLine = 0;

                while (csv.ReadNextRecord())
                {
                    double number;

                    if (double.TryParse(csv[0], out number))
                        steps[iLine] = number;

                    iLine++;
                }

                var realConcatenator = (OutputConcatenator)concatenator;
                //Adds the output array to the overall output array
                realConcatenator.AddSteps(steps);
            }
        }

        #endregion
    }
}