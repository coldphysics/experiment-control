using Controller;
using Controller.Helper;
using Controller.MainWindow;
using Controller.MainWindow.MeasurementRoutine;
using MainProject.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.MeasurementRoutine;
using Model.Root;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;


namespace GeneratorUT
{
    [TestClass]
    public class MeasurementRoutineUT : ParentUT
    {
        /// <summary>
        /// Test the loading of a primary model and a secondary model via the measurement routine controller
        /// </summary>
        /// <param name="primaryModel">the path to the primary model.</param>
        /// <param name="secondaryModel">the path to the secondary model.</param>
        /// <param name="profileName">the name of the user profile to be used.</param>
        [DataRow("Resources\\0.2ms.xml.gz", "Resources\\0.0ms.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestLoadingModels(string primaryModel, string secondaryModel, string profileName)
        {
            SelectProfile(profileName);
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController mainWindowController = builder.GetMainController();
            MeasurementRoutineManagerController manager = new MeasurementRoutineManagerController(mainWindowController,
                mainWindowController.GetRootController().returnModel);

            manager.LoadModelAsync(primaryModel, true, true).Wait();

            Assert.AreNotEqual(null, manager.PrimaryModel);
            Assert.AreNotEqual(null, manager.SecondaryModels);
            Assert.AreEqual(0, manager.SecondaryModels.Count);
            Assert.AreEqual(manager.PrimaryModel, manager.CurrentRoutineModel);
            Assert.AreEqual(0, manager.CurrentRoutineModelIndex);
            Assert.AreEqual(primaryModel, manager.PrimaryModel.FilePath);
            Assert.AreNotEqual(null, manager.PrimaryModel.ActualModel);
            manager.IsAdvancedMode = true;

            manager.LoadModelAsync(secondaryModel, false, true).Wait();

            Assert.AreNotEqual(null, manager.PrimaryModel);
            Assert.AreNotEqual(null, manager.SecondaryModels);
            Assert.AreEqual(manager.PrimaryModel, manager.CurrentRoutineModel);
            Assert.AreEqual(0, manager.CurrentRoutineModelIndex);
            Assert.AreEqual(primaryModel, manager.PrimaryModel.FilePath);
            Assert.AreNotEqual(null, manager.PrimaryModel.ActualModel);
            Assert.AreEqual(1, manager.SecondaryModels.Count);
            RoutineModelController secondaryController = manager.SecondaryModels.First();
            Assert.AreEqual(secondaryModel, secondaryController.FilePath);
            Assert.AreNotEqual(null, secondaryController.ActualModel);
        }

        [DataRow("Resources\\0.2ms.xml.gz", "Resources\\0.0ms.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestSavingAndLoadingMeasurementRoutine(string primaryModel, string secondaryModel, string profileName)
        {
            // initialize
            SelectProfile(profileName);
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController mainWindowController = builder.GetMainController();
            RootModel initialRootModel = mainWindowController.GetRootController().returnModel;
            MeasurementRoutineManagerController manager = new MeasurementRoutineManagerController(mainWindowController, initialRootModel);
            RoutineBasedRootModel initialPrimaryMRM = manager.ConstructMeasurementRoutineModel().PrimaryModel;

            manager.IsAdvancedMode = true;
            string temporaryFilePath = FileHelper.GenerateTemporaryFilePath(".routine.gz");

            // load primary and secondary models
            manager.LoadModelAsync(primaryModel, true, true).Wait();
            manager.LoadModelAsync(secondaryModel, false, true).Wait();

            // create routine model object and set scripts
            MeasurementRoutineModel routineModel = manager.ConstructMeasurementRoutineModel();
            routineModel.RoutineControlScript = "A";
            routineModel.RoutineInitializationScript = "B";
            
            // save model
            FileHelper.SaveFile(temporaryFilePath, routineModel);
            Assert.IsTrue(File.Exists(temporaryFilePath));

            // reset
            MeasurementRoutineModel empty = new MeasurementRoutineModel { PrimaryModel = initialPrimaryMRM,
                SecondaryModels = new ObservableCollection<RoutineBasedRootModel>(), RoutineControlScript = null, RoutineInitializationScript = null };

            // sanity checks
            manager.ConnectControllerToModel(empty, true);
            Assert.AreEqual(initialRootModel, manager.PrimaryModel.ActualModel);
            Assert.AreEqual(0, manager.SecondaryModels.Count);
            Assert.AreEqual(0, manager.CurrentRoutineModelIndex);
            Assert.AreEqual(null, manager.Script);
            Assert.AreEqual(null, manager.InitializationScript);

            // load routine model
            manager.LoadMeasurementRoutineAsync(temporaryFilePath, true).Wait();

            Assert.AreNotEqual(null, manager.PrimaryModel);
            Assert.AreNotEqual(null, manager.SecondaryModels);
            Assert.AreEqual(manager.PrimaryModel, manager.CurrentRoutineModel);
            Assert.AreEqual(0, manager.CurrentRoutineModelIndex);
            Assert.AreEqual(primaryModel, manager.PrimaryModel.FilePath);
            Assert.AreNotEqual(null, manager.PrimaryModel.ActualModel);
            Assert.AreEqual(1, manager.SecondaryModels.Count);
            RoutineModelController secondaryController = manager.SecondaryModels.First();
            Assert.AreEqual(secondaryModel, secondaryController.FilePath);
            Assert.AreNotEqual(null, secondaryController.ActualModel);
            Assert.AreEqual("A", manager.Script);
            Assert.AreEqual("B", manager.InitializationScript);

            // Cleanup
            File.Delete(temporaryFilePath);
        }
    }
}
