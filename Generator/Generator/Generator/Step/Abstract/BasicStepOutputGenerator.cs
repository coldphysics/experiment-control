using AbstractController.Data.Steps;
using Generator.Generator.Channel;
using Model.Data.Steps;
using System;

namespace Generator.Generator.Step.Abstract
{
    public abstract class BasicStepOutputGenerator:AbstractStepController
    {
        /// <summary>
        /// The model that describes the step
        /// </summary>
        protected StepBasicModel _model;

        public BasicStepOutputGenerator(StepBasicModel model, ChannelOutputGenerator parent)
            : base(parent)
        {
            this._model = model;
        }

        public double GetLastValueOfPreviousStep()
        {
            double previousValue = 0.0;

            //RECO this could be redundant
            if (_model.Index() == 0 && _model.Sequence().Index() == 0)
            {
                previousValue = _model.Setting.InitValue;
            }
            else
            {
                //If this is the first step of this channel among all sequences then the previous value is the initial value specified in the Settings.
                if (_model.PreviousStep() == null)
                {
                    previousValue = _model.Setting.InitValue;
                }
                else//If there exists a previous step then its value is the previous value (this doesn't consider a previous file-step; see the RECO above)
                {
                    previousValue = _model.PreviousStep().Value.Value;
                }
            }

            return previousValue;
        }

        //TODO implement method
        public double GetFirstValueOfNextStep()
        {
            throw new NotImplementedException();
        }

        public int GetNumberOfTimeSteps()
        {
            int amountOfTimesteps = (int)(Math.Round(_model.Duration.Value / _model.SmallestStepSize()));

            return amountOfTimesteps;
        }

        
    }
}
