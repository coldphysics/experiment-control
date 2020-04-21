using Communication.Interfaces.Generator;
using Generator.Generator.Channel;
using Generator.Generator.Concatenator;
using Generator.Generator.Step.Abstract;
using Model.Data.Steps;
using System;

namespace Generator.Generator.Step
{
    //RECO Use the strategy design pattern to do the of output; a factory (cookbook) should choose the pattern

    /// <summary>
    /// Represents a generator that is capable of generating the output of a single "analog" ramp step (constant or linear)
    /// </summary>
    /// <seealso cref="AbstractController.Data.Steps.AbstractStepController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class AnalogStepRampOutputGenerator : BasicStepOutputGenerator, IValueGenerator
    {
        /// <summary>
        /// The model that describes the step
        /// </summary>
        public StepRampModel Model
        {
            get
            {
                return (StepRampModel)_model;
            }
        }
        

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogStepRampOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public AnalogStepRampOutputGenerator(StepRampModel model, ChannelOutputGenerator parent)
            : base(model, parent)
        {
        }

        #region IValueGenerator Members
       

        /// <summary>
        /// Generates the output time-steps for the current step, so that their values correspond to either a constant, linear, or exponential curves
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        /// <remarks>
        /// The number of time-steps T in the output is calculated based on the duration of the related step model.
        /// A constant step just holds the value V_NEW (which is retrieved from the step model) for T time-steps.
        /// A linear step goes from V_OLD (which is the value of the previous step if one exists, or the init value if a previous step does not exist) to V_NEW linearly during the T time-steps.
        /// </remarks>
        public void Generate(IConcatenator concatenator)
        {
            //The number of time-steps this step has
            int amountOfTimesteps = GetNumberOfTimeSteps();
            double[] timesteps = new double[amountOfTimesteps];
            double previousValue = GetLastValueOfPreviousStep();
            double slope;//Used for linear and exponential steps

            switch (Model.Store)
            {
                case StepRampModel.StoreType.Constant://A constant steps holds the same value for all time-steps
                    for (int iTimestep = 0; iTimestep < amountOfTimesteps; iTimestep++)
                    {
                        timesteps[iTimestep] = _model.Value.Value;
                    }
                    break;
                case StepRampModel.StoreType.Linear://A linear step - see the method remarks for details
                    slope = (_model.Value.Value - previousValue) / amountOfTimesteps;

                    for (int iTimestep = 0; iTimestep < amountOfTimesteps; iTimestep++)
                    {
                        timesteps[iTimestep] = previousValue + slope * (iTimestep + 1);//We add 1 to the current timestep, so that we ensure reaching the desired value at the end
                    }

                    break;
                default:
                    Console.WriteLine("Usage of an unsupported step type: {0}", Model.Store);
                    break;
            }

            OutputConcatenator realConcatenator = (OutputConcatenator)concatenator;

            //Add the generated time-steps to the overall output array
            realConcatenator.AddSteps(timesteps);
        }

        #endregion
    }
}