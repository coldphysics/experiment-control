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
    //RECO have one FileGenerator per file type. Avoid using enums!

    /// <summary>
    /// A generator that is capable of generating the output of a "digital" step whose details are read from a file (CSV or binary).
    /// </summary>
    /// <seealso cref="AbstractController.Data.Steps.AbstractStepController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class DigitalStepFileOutputGenerator : BasicFileStepOutputGenerator, IValueGenerator
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalStepRampOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model that describes the current step.</param>
        /// <param name="parent">The parent.</param>
        public DigitalStepFileOutputGenerator(StepFileModel model, ChannelOutputGenerator parent)
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
        /// If the binary file format is used, then each byte in the file is considered the value of a time-step.
        /// And if the CSV file format is used, then each cell value from the first column is considered the value of a time-step
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
                BinaryReader binaryInput = new BinaryReader(File.OpenRead(Model.FileName));
                int byteLength = (int)binaryInput.BaseStream.Length;
                int length = (byteLength / sizeof(byte));//WTF?
                int pos = 0;
                uint[] steps = new uint[length];

                //loop through all bytes stored in the binary file
                while (pos < length)
                {
                    //RECO ensure that the value is either 0 or 1, otherwise funny things could happen within the Concatenator.DigitalCard class!
                    steps[pos] = binaryInput.ReadByte();//copy the current value in the file to the output array
                    pos++;
                }

                binaryInput.Close();

                OutputConcatenator realConcatenator = (OutputConcatenator)concatenator;
                //add the output array to the overall output array
                realConcatenator.AddSteps(steps);
            }//the file is a CSV file
            else if (Model.Store == StepFileModel.StoreType.Csv)
            {
                CsvReader csv = new CsvReader(new StreamReader(Model.FileName), false);
                //RECO do not read the entire file and throw it away just to figure out the number of records!!!!!!! (use a list instead)
                int fieldCount = File.ReadAllLines(Model.FileName).Length;
                var steps = new uint[fieldCount];
                int iLine = 0;

                //Loops all rows
                while (csv.ReadNextRecord())
                {
                    uint number;

                    if (uint.TryParse(csv[0], out number))
                        steps[iLine] = number;

                    iLine++;
                }

                //RECO put addsteps after the if-else  
                OutputConcatenator realConcatenator = (OutputConcatenator)concatenator;
                //Adds the output array to the overall output array
                realConcatenator.AddSteps(steps);
            }
        }

        #endregion
    }
}