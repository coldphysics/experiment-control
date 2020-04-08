using Communication.Interfaces.Generator;
using Generator.Generator.Channel;
using Generator.Generator.Concatenator;
using Generator.Generator.Step.Abstract;
using Model.Data.Steps;
using System;

namespace Generator.Generator.Step
{
    /// <summary>
    /// A generator that is capable of generating the output of a single "ramp" (single-valued) digital step
    /// </summary>
    /// <seealso cref="AbstractController.Data.Steps.AbstractStepController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class DigitalStepRampOutputGenerator : BasicStepOutputGenerator, IValueGenerator
    {
        /// <summary>
        /// The model that describes the current step.
        /// </summary>
        public StepRampModel Model
        {
            get
            {
                return (StepRampModel)_model;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalStepRampOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model that describes the current step.</param>
        /// <param name="parent">The parent.</param>
        public DigitalStepRampOutputGenerator(StepRampModel model, ChannelOutputGenerator parent)
            : base(model, parent)
        {
        }

        #region IValueGenerator Members
        //RECO extract the way of comparing doubles into a static function in the Communication project (which everybody sees)

        /// <summary>
        /// Generates the output for the current step as an array consisting of numberOfTimeSteps elements and has the same value in all cells.
        /// The values are either all 1's or all 0's depending on the value described in the model.
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        public void Generate(IConcatenator concatenator)
        {
            //determine the number of time-steps of the output
            int amountOfTimesteps = GetNumberOfTimeSteps();

            uint[] timesteps = new uint[amountOfTimesteps];
            uint outputValue = 0;

            //determine the output value
            if (Math.Abs(_model.Value.Value - 1.0) < 0.001)
            {
                outputValue = 1;
            }
            else if (Math.Abs(_model.Value.Value) < 0.001)//RECO remove the nested if - no need for it; there is no third choice!
            {
                outputValue = 0;
            }

            //fill the output array 
            for (uint iTimestep = 0; iTimestep < amountOfTimesteps; iTimestep++)
            {
                timesteps[iTimestep] = outputValue;
            }

            OutputConcatenator realConcatenator = (OutputConcatenator)concatenator;

            //add the output array to the overall output array using the concatenator
            realConcatenator.AddSteps(timesteps);
        }

        #endregion
    }
}