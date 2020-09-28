using System.Collections.Generic;
using Controller.Data.Channels;
using Controller.Data.Steps;
using Controller.Data.Tabs;
using Controller.Data.WindowGroups;
using Controller.Data.Windows;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;
using Model.Data.Steps;

namespace Controller.Data.Cookbook
{
    public class WindowGroupControllerRecipe
    {
        // ******************** variables ********************
        private readonly List<string> _windows = new List<string>();

        // ******************** constructor ********************
        public WindowGroupControllerRecipe(List<string> windows)
        {
            _windows = windows;
        }

        public WindowGroupControllerRecipe()
        {}

        public void CookWindowGroup(SequenceGroupModel model, DataController dataController)
        {
            var windowGroupController = new WindowGroupController(model, dataController);
            foreach (CardBasicModel card in model.Cards)
            {
                //$ Settings ask user what to do
                if (_windows.Contains(card.Name))
                    CookWindow(card, windowGroupController);
            }
            dataController.SequenceGroup = windowGroupController;
        }

        public WindowBasicController CookWindow(CardBasicModel model, WindowGroupController windowGroupController)
        {
            WindowBasicController windowController;

            if (model.Type == CardBasicModel.CardType.Analog)
            {
                windowController = new AnalogWindowController(model, windowGroupController);
            }
            else
            {
                windowController = new DigitalWindowController(model, windowGroupController);
            }

            foreach (SequenceModel sequence in model.Sequences)
            {
                CookTab(sequence, windowController);
            }

            windowGroupController.Windows.Add(windowController);

            return windowController;
        }

        public TabController CookTab(SequenceModel sequence, WindowBasicController windowController)
        {
            var tab = new TabController(sequence, windowController);
            int chanNum = 0;

            foreach (ChannelModel channel in sequence.Channels)
            {
                ChannelBasicController channelController;
                ChannelSettingsController channelSettings;

                if (channel.Card().Type == CardBasicModel.CardType.Analog)
                {
                    channelController = new ChannelAnalogController(channel, tab);
                    channelSettings = new AnalogChannelSettingsController(channel.Setting, tab, chanNum);
                }
                else
                {
                    channelController = new ChannelDigitalController(channel, tab);
                    channelSettings = new DigitalChannelSettingsController(channel.Setting, tab, chanNum);
                }

                chanNum++;

                //RECO A channel's header is a step! That is not logical!
                channelController.Steps.Add(channelSettings);
                tab.Channels.Add(channelController);

                StepBasicController stepController = null;

                foreach (StepBasicModel step in channel.Steps)
                {
                    if (step is StepFileModel)
                    {
                        if (channel.Card().Type == CardBasicModel.CardType.Analog)
                        {
                            stepController = new AnalogStepFileController((StepFileModel)step, channelController);
                        }
                        else
                        {
                            stepController = new DigitalStepFileController((StepFileModel)step, channelController);
                        }
                        
                    }
                    else if(step is StepRampModel)
                    {
                        if (channel.Card().Type == CardBasicModel.CardType.Analog)
                        {
                            stepController = new AnalogStepRampController((StepRampModel) step, channelController);
                        }
                        else
                        {
                            stepController = new DigitalStepRampController((StepRampModel)step, channelController);
                        }
                    }
                    else if (step is StepPythonModel)
                    {
                        stepController = new StepPythonController((StepPythonModel)step, channelController);
                    }

                    channelController.Steps.Add(stepController);
                }
            }
            windowController.Tabs.Add(tab);
            return tab;
        }

    }
}