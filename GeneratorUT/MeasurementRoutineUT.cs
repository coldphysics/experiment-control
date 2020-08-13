using Controller;
using Controller.MainWindow;
using Controller.MainWindow.MeasurementRoutine;
using MainProject.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.MeasurementRoutine;
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
            bool primaryAlreadyLoaded = false;

            manager.FinishedLoadingModel += (sender, args) =>
            {
                if (!primaryAlreadyLoaded)
                {
                    Assert.AreNotEqual(null, manager.PrimaryModel);
                    Assert.AreNotEqual(null, manager.SecondaryModels);
                    Assert.AreEqual(0, manager.SecondaryModels.Count);
                    Assert.AreEqual(manager.PrimaryModel, manager.CurrentRoutineModel);
                    Assert.AreEqual(0, manager.CurrentRoutineModelIndex);
                    Assert.AreEqual(primaryModel, manager.PrimaryModel.FilePath);
                    Assert.AreNotEqual(null, manager.PrimaryModel.ActualModel);
                    manager.IsAdvancedMode = true;
                    primaryAlreadyLoaded = true;
                    manager.LoadModel(secondaryModel, false, true);
                } else
                {
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

            };

            manager.LoadModel(primaryModel, true, true);
        }

        [DataRow("Resources\\0.2ms.xml.gz", "Resources\\0.0ms.xml.gz", "Dy4thFloor")]
        [DataTestMethod]
        public void TestSavingAndLoadingMeasurementRoutine(string primaryModel, string secondaryModel, string profileName)
        {
            SelectProfile(profileName);
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController mainWindowController = builder.GetMainController();
            MeasurementRoutineManagerController manager = new MeasurementRoutineManagerController(mainWindowController,
                mainWindowController.GetRootController().returnModel);
            bool primaryAlreadyLoaded = false;
            manager.IsAdvancedMode = true;
            string temporaryFilePath = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".routine.gz";

            manager.FinishedLoadingModel += (sender, args) =>
            {
                if (!primaryAlreadyLoaded)
                {   
                    primaryAlreadyLoaded = true;
                    manager.LoadModel(secondaryModel, false, true);
                }
                else
                {
                    
                    MeasurementRoutineModel routineModel = manager.ConstructMeasurementRoutineModel();
                    FileHelper.SaveFile(temporaryFilePath, routineModel);
                    Assert.IsTrue(File.Exists(temporaryFilePath));
                    manager.LoadMeasurementRoutine(temporaryFilePath, true);
                }

            };

            manager.FinishedLoadingRoutineModel += (sender, args) => 
            {
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
            };

            manager.LoadModel(primaryModel, true, true);
        }
    }
}
