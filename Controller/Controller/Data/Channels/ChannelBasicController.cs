using System;
using System.ComponentModel;
using System.Windows.Input;
using AbstractController.Data.Channels;
using Communication.Commands;
using Controller.Data.Steps;
using Controller.Data.Tabs;
using Model.Data.Channels;
using Model.Data.Steps;

namespace Controller.Data.Channels
{
    public abstract class ChannelBasicController : AbstractChannelController, INotifyPropertyChanged
    {
        // ******************** enums ********************
        public enum AnalogTypes
        {
            Constant,
            Linear,
            Binary,
            Csv,
            Python
        }

        public enum DigitalTypes
        {
            Constant,
            Binary,
            Csv
        }

        public enum LeftRightEnum
        {
            Left,
            Right
        }


        // ******************** Properties ********************
        public Root.RootController _rootController
        {
            get { return _parent._rootController; }
        }

        public RelayCommand MouseDownCommand
        {
            private set;
            get;
        }


        // ******************** events ********************
        public event PropertyChangedEventHandler PropertyChanged;


        // ******************** variables ********************
        private TabController _parent;


        // ******************** constructor ********************
        protected ChannelBasicController(ChannelModel model, TabController parent)
            : base(model, parent)
        {
            _parent = parent;
            MouseDownCommand = new RelayCommand(OnMoudeDown);
        }

        internal void ChangeStep(StepBasicController step, String type)
        {
            int indexController = Steps.IndexOf(step);
            int indexModel = step.IndexOfModel();
            RemoveStep(step, true);
            StepBasicModel stepModelToAdd = null;
            StepBasicController stepControllerToAdd = null;

            //Create the model and the controller
            if (AnalogTypes.Python.ToString().Equals(type))
            {
                stepModelToAdd = new StepPythonModel(Model);
                stepControllerToAdd = new StepPythonController((StepPythonModel)stepModelToAdd, this);
            }
            else
            {
                StepRampModel.StoreType rampStore;

                if (Enum.TryParse(type, out rampStore))
                {
                    stepModelToAdd = new StepRampModel(Model, rampStore);
                    stepControllerToAdd = new StepRampController((StepRampModel)stepModelToAdd, this);
                }
                else
                {
                    StepFileModel.StoreType fileStore;

                    if (Enum.TryParse(type, out fileStore))
                    {
                        stepModelToAdd = new StepFileModel(Model, fileStore);
                        stepControllerToAdd = new StepFileController((StepFileModel)stepModelToAdd, this);
                    }
                }
            }

            //Add the model and the controller to their parents
            Model.Steps.Insert(indexModel, stepModelToAdd);
            Steps.Insert(indexController, stepControllerToAdd);
        }

        ///A function that moves a step to the left or to the right.
        public void MoveStep(StepBasicController step, LeftRightEnum leftRight)
        {
            int index = Steps.IndexOf(step);

            // We need to move the controllers, the models, recalculate the start time of the effected steps, and notify UI of change
            if (index > 1 && leftRight == LeftRightEnum.Left)
            {
                StepBasicController previousStep = (StepBasicController)Steps[index - 1];
                Steps.Move(index, index - 1);
                Model.Steps.Move(index - 1, index - 2);
                Model.Steps[index - 2].SetStartTime(StartTimeOf(step));
                Model.Steps[index - 1].SetStartTime(StartTimeOf(previousStep));
                step.UpdateProperty("StartTime");
                previousStep.UpdateProperty("StartTime");
                CopyToBuffer();
            }

            if (index < Steps.Count - 1 && leftRight == LeftRightEnum.Right)
            {
                StepBasicController nextStep = (StepBasicController)Steps[index + 1];
                Steps.Move(index, index + 1);
                Model.Steps.Move(index - 1, index);
                Model.Steps[index].SetStartTime(StartTimeOf(step));
                Model.Steps[index - 1].SetStartTime(StartTimeOf(nextStep));
                step.UpdateProperty("StartTime");
                nextStep.UpdateProperty("StartTime");
                CopyToBuffer();
            }
        }

        public void InsertStep(StepBasicController step)
        {
            int index = Steps.IndexOf(step);
            var newStepModel = new StepRampModel(Model, StepRampModel.StoreType.Constant);
            Model.StepAdd(newStepModel);

            Model.Steps.Move(Model.Steps.IndexOf(newStepModel), index - 1);

            var newStepController = new StepRampController(newStepModel, this);
            Steps.Add(newStepController);


            Steps.Move(Steps.IndexOf(newStepController), index);
            CopyToBuffer();
        }

        public void RemoveStep(StepBasicController step, bool exchange = false)
        {
            if (Steps.Count > 1 || exchange)
            {
                int index = Steps.IndexOf(step);
                Model.Steps.RemoveAt(index - 1);
                Steps.RemoveAt(index);
                CopyToBuffer();
                for (int i = 1; i < Steps.Count && index >= 0; i++)
                {
                    ((StepBasicController)Steps[i]).UpdateProperty("StartTime");
                }
            }
        }

        public void AddStep()
        {
            var newStepModel = new StepRampModel(Model, StepRampModel.StoreType.Constant);
            Model.StepAdd(newStepModel);
            var newStepController = new StepRampController(newStepModel, this);
            Steps.Add(newStepController);
        }

        public void InsertConstantStepAtBeginning(double value, double duration)
        {
            StepRampModel model = new StepRampModel(Model, StepRampModel.StoreType.Constant);
            model.Duration = new Model.BaseTypes.ValueDoubleModel() { Value = duration };
            model.Value = new Model.BaseTypes.ValueDoubleModel() { Value = value };
            Model.Steps.Insert(0, model);
            StepRampController controller = new StepRampController(model, this);
            Steps.Insert(1, controller);
        }

        public void CopyToBuffer()
        {
            ((TabController)Parent).CopyToBuffer();
        }

        public double StartTimeOf(StepBasicController step)
        {
            var index = Steps.IndexOf(step);
            double startTime = 0;
            for (int i = 1; i < index && index >= 0; i++)
            {
                startTime += ((StepBasicController)Steps[i]).Duration;
            }
            return startTime;
        }

        public void UpdateSteps(StepBasicController step)
        {
            var index = Steps.IndexOf(step);
            var total = Steps.Count;
            for (var i = index; i < total && index >= 0; i++)
            {
                ((StepBasicController)Steps[i]).UpdateProperty("StartTime");
            }
        }

        private void OnMoudeDown(object parameter)
        {
            MouseButtonEventArgs e = (MouseButtonEventArgs)parameter;

            if (e.ClickCount >= 2)
            {
                AddStep();
            }
        }

        public override string ToString()
        {
            return Model.Setting.Name;
        }
    }
}