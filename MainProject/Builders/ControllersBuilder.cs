using System.Collections.Generic;
using Buffer.Basic;
using Controller.Cookbook;
using Controller.Data.Cookbook;
using Controller.MainWindow;
using Model.Data.Cards;
using Model.Root;
using Controller.Helper;
using Controller.Error;

namespace MainProject.Builders
{
    class ControllersBuilder
    {
        private MainWindowController mainController;

        public ControllersBuilder()
        { }

        public void Build(DoubleBuffer doubleBuffer, RootModel rootModel, Dictionary<string, CardBasicModel.CardType> cardTypes)
        {

            List<string> cardNames = new List<string>(cardTypes.Keys);
            OutputHandler outputHandler = doubleBuffer.OutputHandler;


            //creates the controller which connects the view with everything else
            var controllerWrapper = new ControllerRecipe(new WindowGroupControllerRecipe(cardNames), doubleBuffer);

            //This is the generator that knows which windows have to be opened
            var windowGenerator = new WindowGenerator(cardNames);



            //Initializes the Variables window
            //rootModel.Data.variablesModel = new VariablesModel();
            var variablesController = new Controller.Variables.VariablesController(rootModel.Data.variablesModel, outputHandler, rootModel);

            //Connects Events to iterate and evaluate the variables between buffer and variables. Iterate performs iterate and then evaluate.
            outputHandler.IterateVariables += variablesController.IterateVariablesFromBuffer;
            outputHandler.ResetIteratorValues += variablesController.ResetIteratorValuesFromBuffer;
      
            //Initializes the controlWindow and creates it (and with it all other Windows)
            this.mainController = new MainWindowController(rootModel, doubleBuffer, outputHandler, controllerWrapper, windowGenerator, variablesController);

            doubleBuffer.OnGeneratorStateChange += mainController.OnGeneratorStateChange;
            outputHandler.OnScanChange += mainController.OnScanChange;
            outputHandler.OnOuputLoopStateChange += mainController.OnOuputLoopStateChange;
            outputHandler.BeforeIteratingVariables += mainController.MeasurementRoutineController.DetermineNextModel;
            outputHandler.AfterIteratingVariables += mainController.MeasurementRoutineController.ChangeCurrentModelIfNecessary;
        }

        public MainWindowController GetMainWindowController()
        {
            return this.mainController;
        }
    }
}
