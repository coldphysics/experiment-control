using System.Collections.Generic;
using Buffer.Basic;
using Controller.MainWindow;
using Model.Builder;
using Model.Data.Cards;
using Model.Root;

namespace MainProject.Builders
{
    class MasterBuilder
    {
        private MainWindowController mainController;

        public MasterBuilder()
        { }

        public void Build()
        {
            ModelBuilder modelBuilder = new ModelBuilder();
            DoubleBufferBuilder bufferBuilder = new DoubleBufferBuilder();
            ControllersBuilder controllersBuilder = new ControllersBuilder();

            modelBuilder.Build();
            RootModel root = modelBuilder.GetRootModel();
            Dictionary<string, CardBasicModel.CardType> cardTypes = modelBuilder.GetCardTypes();

            bufferBuilder.Build(root, cardTypes);
            DoubleBuffer doubleBuffer = bufferBuilder.GetDoubleBuffer();

            controllersBuilder.Build(doubleBuffer, root, cardTypes);
            this.mainController = controllersBuilder.GetMainWindowController();

        }

        public MainWindowController GetMainController()
        {
            return this.mainController;
        }
    }
}
