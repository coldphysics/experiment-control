using System;
using Controller.MainWindow;
using Controller.Variables;
using MainProject.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GeneratorUT
{
    [TestClass]
    public class VariablesControllerUT: ParentUT
    {
        [DataRow("Dy4thFloor")]
        [TestMethod]
        public void TestChangeVariableTypeToIterator(string profileName)
        {
            const int START = 0;
            const int STOP = 10;
            const int STEP = 1;
            int steps = (STOP - START + 1) / STEP;

            SelectProfile(profileName);
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController mainWindowController = builder.GetMainController();
            VariablesController controller = mainWindowController.GetRootController().Variables;
            Assert.AreEqual(0, controller.VariablesStatic.Count);
            Assert.AreEqual(0, controller.VariablesDynamic.Count);
            Assert.AreEqual(0, controller.VariablesIterator.Count);
            Assert.AreEqual(1, controller.NumberOfIterations);
            controller.AddStaticCommand.Execute(null);
            // group header + variable
            Assert.AreEqual(2, controller.VariablesStatic.Count);
            Assert.AreEqual(0, controller.VariablesDynamic.Count);
            Assert.AreEqual(0, controller.VariablesIterator.Count);
            Assert.AreEqual(1, controller.NumberOfIterations);
            VariableController variable = controller.VariablesStatic
                .Where(value => value is VariableStaticController)
                .First();
            Assert.IsNotNull(variable);
            Assert.IsInstanceOfType(variable, typeof(VariableStaticController));
            // prepare the iterator-specific values before changing type to iterator (RIP OOP :))
            variable.VariableStartValue = 0;
            variable.VariableStepValue = 1;
            variable.VariableEndValue = 10;
            Assert.AreEqual(1, controller.NumberOfIterations);
            variable = controller.ChangeVariableType(variable, Communication.VariableType.VariableTypeIterator);
            Assert.IsNotNull(variable);
            Assert.IsInstanceOfType(variable, typeof(VariableIteratorController));
            // group header
            Assert.AreEqual(1, controller.VariablesStatic.Count);
            Assert.AreEqual(0, controller.VariablesDynamic.Count);
            // variable
            Assert.AreEqual(1, controller.VariablesIterator.Count);
            Assert.AreEqual(steps, controller.NumberOfIterations);

            variable = controller.ChangeVariableType(variable, Communication.VariableType.VariableTypeStatic);            Assert.IsNotNull(variable);
            Assert.IsInstanceOfType(variable, typeof(VariableStaticController));
            // group header + variable
            Assert.AreEqual(2, controller.VariablesStatic.Count);
            Assert.AreEqual(0, controller.VariablesDynamic.Count);
            Assert.AreEqual(0, controller.VariablesIterator.Count);
            Assert.AreEqual(1, controller.NumberOfIterations);


        }
    }
}
