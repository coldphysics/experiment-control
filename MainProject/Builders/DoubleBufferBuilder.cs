using System.Collections.Generic;
using Buffer.Basic;
using Buffer.HardwareManager;
using Communication.Interfaces.Hardware;
using Generator.Cookbook;
using HardwareAdWin.HardwareAdWin;
using HardwareDAQmx.Dy4thFloor;
using HardwareDAQmx.SuperAtoms5thFloor;
using Model.Data.Cards;
using Model.Root;
using Model.Settings;

namespace MainProject.Builders
{
    class DoubleBufferBuilder
    {
        private DoubleBuffer doubleBuffer;

        public DoubleBufferBuilder()
        { }

        private IHardwareGroup GetHardwareGroup(Dictionary<string, CardBasicModel.CardType> cardTypes)
        {
            HW_TYPES hardware = Global.GetHardwareType();

            switch (hardware)
            {
                case HW_TYPES.AdWin_T11:
                case HW_TYPES.AdWin_T12:
                case HW_TYPES.AdWin_Simulator:
                    ControlAdwinProcess.StartAdwin();
                    return new AdWinHwGroup(cardTypes);
                case HW_TYPES.NI_CHASSIS:
                    return new SuperAtomsHwGroup();
                case HW_TYPES.NI_PCI:
                    return new DyHwGroup();
                case HW_TYPES.NO_OUTPUT:
                    return new NoOutputHardwareGroup();
            }

            return null;
        }
        public void Build(RootModel rootModel, Dictionary<string, CardBasicModel.CardType> cardTypes)
        {
            //List<string> cardNames = new List<string>(cardTypes.Keys);

            //This is the generator, which will be used in the Buffer to generate the output
            GeneratorRecipe generatorRecipe = new GeneratorRecipe(new SequenceGroupGeneratorRecipe());

            var experiment = GetHardwareGroup(cardTypes); 

            //Creates the Hardware manager which will be responsible for the communication with the Hardware
            HardwareManager hardwareManager = new HardwareManager(experiment);

            OutputHandler outputHandler = new OutputHandler(hardwareManager);
            this.doubleBuffer = new DoubleBuffer(generatorRecipe, hardwareManager, outputHandler);
        }

        public DoubleBuffer GetDoubleBuffer()
        {
            return this.doubleBuffer;
        }
    }
}
