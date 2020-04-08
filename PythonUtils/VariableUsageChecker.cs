using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.Sequences;
using Model.Data.Steps;
using Model.Root;
using Model.Variables;
using PythonUtils.ScriptAnalysis;
using System;
using System.Collections.Generic;

namespace PythonUtils
{
    /// <summary>
    /// The possible in which places a variable could be used.
    /// </summary>
    public enum VariableUsageType
    {
        /// <summary>
        /// The variable is used in a dynamic variable
        /// </summary>
        DynamicVariable,
        /// <summary>
        /// The variable is used in a python step
        /// </summary>
        PythonStep,
        /// <summary>
        /// The variable is used in a calibration script
        /// </summary>
        CalibrationScript,
        /// <summary>
        /// The variable is used as a value for a step
        /// </summary>
        Value,
        /// <summary>
        /// The variable is used as a duration for a step
        /// </summary>
        Duration
    }

    /// <summary>
    /// An object that holds the information related to one usage of the variable
    /// </summary>
    public class VariableUsage
    {
        /// <summary>
        /// Gets or sets the script location.
        /// </summary>
        /// <value>
        /// The script location.
        /// </value>
        public IScriptLocation ScriptLocation { set; get; }
        /// <summary>
        /// Gets or sets the type of the usage.
        /// </summary>
        /// <value>
        /// The type of the usage.
        /// </value>
        public VariableUsageType UsageType { set; get; }
        /// <summary>
        /// Gets or sets the usage context.
        /// </summary>
        /// <value>
        /// The usage context.
        /// </value>
        public object UsageContext { set; get; }
    }

    /// <summary>
    /// Iterates the usages of a variable
    /// </summary>
    public class VariableUsageChecker
    {
        /// <summary>
        /// The root model
        /// </summary>
        private RootModel rootModel;


        /// <summary>
        /// Initializes a new instance of the <see cref="VariableUsageChecker"/> class.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        public VariableUsageChecker(RootModel rootModel)
        {
            this.rootModel = rootModel;
        }


        /// <summary>
        /// Gets the usages of variable in all used python scripts.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>The usages of a variable</returns>
        public IEnumerable<VariableUsage> GetUsagesOfVariable(string variableName)
        {
            foreach (CardBasicModel card in rootModel.Data.group.Cards)
            {
                //Check the calibration script
                foreach (ChannelSettingsModel setting in card.Settings)
                {
                    if (setting.UseCalibration && !String.IsNullOrEmpty(setting.CalibrationScript))
                    {

                        if (IsVariableUsedInScript(variableName, setting.CalibrationScript))
                        {
                            //The variable is used as part of a calibration script
                            yield return new VariableUsage()
                            {
                                UsageContext = setting,
                                UsageType = VariableUsageType.CalibrationScript,
                                ScriptLocation = new ChannelBasedScriptLocation() { CardName = card.Name, ChannelIndex = setting.Index() }
                            };
                        }
                    }
                }

                foreach (SequenceModel sequence in card.Sequences)
                {
                    foreach (ChannelModel channel in sequence.Channels)
                    {
                        //Check the steps
                        foreach (StepBasicModel step in channel.Steps)
                        {

                            StepBasedScriptLocation stepLocation = new StepBasedScriptLocation() { CardName = card.Name, ChannelIndex = channel.Index(), SequenceName = sequence.Name, StepIndex = step.Index() };
                            if (step is StepPythonModel)
                            {


                                if (IsVariableUsedInScript(variableName, ((StepPythonModel)step).Script))
                                {
                                    //The variable is used as part of a python step
                                    yield return new VariableUsage()
                                    {
                                        UsageContext = step,
                                        UsageType = VariableUsageType.PythonStep,
                                        ScriptLocation = stepLocation
                                    };
                                }
                            }

                            if (!String.IsNullOrEmpty(step.ValueVariableName) && variableName.Equals(step.ValueVariableName))
                            {
                                //The variable is used as a value of a step
                                yield return new VariableUsage()
                                {
                                    UsageContext = step,
                                    UsageType = VariableUsageType.Value,
                                    ScriptLocation = stepLocation
                                };
                            }

                            if (!String.IsNullOrEmpty(step.DurationVariableName) && variableName.Equals(step.DurationVariableName))
                            {
                                //The variable is used as a duration of a step
                                yield return new VariableUsage()
                                {
                                    UsageContext = step,
                                    UsageType = VariableUsageType.Duration,
                                    ScriptLocation = stepLocation
                                };
                            }
                        }
                    }
                }
            }

            //Check usage in dynamic variables
            foreach (VariableModel dynamic in rootModel.Data.variablesModel.VariablesList)
            {

                if (dynamic.TypeOfVariable == Communication.VariableType.VariableTypeDynamic &&
                    IsVariableUsedInScript(variableName, dynamic.VariableCode))
                {
                    //The variable is used within a dynamic variable
                    yield return new VariableUsage()
                    {
                        UsageContext = dynamic,
                        UsageType = VariableUsageType.DynamicVariable,
                        ScriptLocation = new VariableBasedScriptLocation() { VariableName = dynamic.VariableName }
                    };
                }

            }
        }


        /// <summary>
        /// Determines whether a variable is used in a specific python script.
        /// </summary>
        /// <param name="variableName">The variable name.</param>
        /// <param name="script">The script.</param>
        /// <returns><c>true</c> when the variable is used in the script, <c>false</c> otherwise.</returns>
        /// <remarks>This method checks the existence of the variable name as a separate word within the text of the script (even in the comments)</remarks>
        private bool IsVariableUsedInScript(string variableName, string script)
        {
            return ScriptExecution.PythonScriptVariablesAnalyzer.IsVariableUsedInScript(variableName, script);
        }



        
    }
}
