using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Buffer.OutputProcessors;
using Buffer.OutputProcessors.CalibrationUnit;
using Buffer.OutputProcessors.ValidationUnit;
using Communication.Interfaces.Buffer;
using Communication.Interfaces.Generator;
using Communication.Interfaces.Model;
using Errors.Error;
using Errors.Error.ErrorItems;
using Generator.Generator.Step;
using Model.Root;

namespace Buffer.Basic
{
    /// <summary>
    /// Event arguments to be used with the "FinishedModelGeneration" event
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class FinishedModelGenerationEventArgs : EventArgs
    {
        public bool IsSuccessful;
    }


    /// <summary>
    /// Receives a <see cref=" RootModel"/> from the GUI, and then generates the corresponding output and converts it (if AdWin)
    /// , and applies some optional modifiers to the output signal (if NI), and finally gives the output to the <see cref=" OutputHandler"/>.
    /// </summary>
    /// <remarks>
    /// This class uses its own thread to perform its functionality.
    /// </remarks>
    /// <seealso cref="IBuffer" />
    public class DoubleBuffer : IBuffer
    {
        // ******************** Enums ******************** 
        class LastValidOutput
        {
            public RootModel model { set; get; }
            public double CycleDuration { set; get; }
            public IModelOutput RawOutput { set; get; }
            public bool IsCurrentModel { set; get; }
        }

        /// <summary>
        /// Describes the states in which the output-generating thread could be.
        /// </summary>
        public enum GeneratorState
        {
            /// <summary>
            /// The model has changed, but the output-generating thread is still working on a previous model.
            /// </summary>
            GeneratingPendingChanges,

            /// <summary>
            /// The output is being generated.
            /// </summary>
            Generating,

            /// <summary>
            /// The output-generating thread has finished its work.
            /// </summary>
            Waiting
        }

        // ******************** Variables/Objects ******************** 

        #region Attributes

        /// <summary>
        /// Event handler for the [finished model generation] event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="FinishedModelGenerationEventArgs"/> instance containing the event data.</param>
        public delegate void FinishedModelGenerationEventHandler(object sender, FinishedModelGenerationEventArgs args);


        /// <summary>
        /// Occurs when the current state of the generator changes. Used for the UI
        /// </summary>
        public event EventHandler OnGeneratorStateChange;

        /// <summary>
        /// Occurs when [finished model generation].
        /// </summary>
        public event FinishedModelGenerationEventHandler FinishedModelGeneration;

        public static bool ModelIsWrong;

        /// <summary>
        /// The duration of the sequence in seconds
        /// </summary>
        public double DurationSeconds { private set; get; } = 0;


        ////Ebaa        
        ///// <summary>
        ///// Gets or sets the visualizer output.
        ///// </summary>
        ///// <value>
        ///// The visualizer output.
        ///// </value>
        ///// /// <remarks>
        ///// To save the output for the Outputvisualizer before quantization and calibration.
        ///// </remarks>
        //public IModelOutput visualizerOutput { set; get; }

        /// <summary>
        /// The error item that is shown on the <see cref=" ErrorCollector"/> if the current output is not the same as the model shown on the screen.
        /// </summary>
        private StickyErrorItem outputNotCurrentModelError;

        /// <summary>
        /// The recipe used to cook the generator.
        /// </summary>
        private readonly IGeneratorRecipe _generatorRecipe;


        /// <summary>
        /// Needed to pass the generated output to the output handler by using the method <see cref="OutputHandler.SetNewCycleData"/>
        /// </summary>
        private OutputHandler _outputHandler;


        /// <summary>
        /// Stores the current state of the generator.
        /// </summary>
        private GeneratorState _generatorState;

        /// <summary>
        /// Synchronizes the work between the output-generating thread, and the GUI thread.
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// A copy of the root model given by the GUI for the purpose of output.
        /// </summary>
        private RootModel _localCopy;

        /// <summary>
        /// The number of times to replicate output.
        /// </summary>
        public int TimesToReplicateOutput { private set; get; }

        private LastValidOutput lastValidOutput;

        /// <summary>
        /// Indicates that the GUI has given a new root model which not yet processed.
        /// </summary>
        private bool hasNewData;

        /// <summary>
        /// Indicates that the output-generating thread is running.
        /// </summary>
        private bool generateThreadRunning;

        #endregion

        // ******************** Properties ********************
        /// <summary>
        /// Gets the output handler.
        /// </summary>
        /// <value>
        /// The output handler.
        /// </value>
        public OutputHandler OutputHandler
        {
            get { return _outputHandler; }
        }

        /// <summary>
        /// Gets or sets the current state of the generator.
        /// </summary>
        /// <value>
        /// The current state of the generator.
        /// </value>
        /// <remarks>
        /// Changing the state using this property is thread-safe and it triggers the <see cref=" OnGeneratorStateChange"/> event.
        /// </remarks>
        public GeneratorState CurrentGeneratorState
        {
            get { return _generatorState; }

            private set
            {
                lock (lockObj)
                {
                    _generatorState = value;
                }

                UpdateGeneratorState();
            }
        }


        //************** Constructor*************

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleBuffer"/> class.
        /// </summary>
        /// <param name="generatorRecipe">The generator recipe.</param>
        /// <param name="outputHandler">The output handler.</param>
        public DoubleBuffer(IGeneratorRecipe generatorRecipe, OutputHandler outputHandler)
        {
            _generatorRecipe = generatorRecipe;
            _outputHandler = outputHandler;
            TimesToReplicateOutput = 1; //The default value
        }


        //******************** Methods ********************        
        /// <summary>
        /// Works on <see cref=" _localCopy"/> and generates the output, converts the output (if AdWin), 
        /// applies modifiers(offset, multiplicator, invert, calibration file) to the output(if NI), and gives the result to the <see cref=" OutputHandler"/>
        /// </summary>
        private void GenerateThread()
        {
            ModelIsWrong = false;
            bool quit;
            RootModel generateThreadLocalCopy;

            do
            {
                Console.WriteLine("Generation started!");

                quit = true;

                lock (lockObj)
                {
                    CurrentGeneratorState = GeneratorState.Generating;
                    generateThreadLocalCopy = _localCopy;
                    hasNewData = false;
                }

                if (lastValidOutput != null)
                    lastValidOutput.IsCurrentModel = false;


                try
                {
                    //Generating the output

                    IDataGenerator outputGenerator = _generatorRecipe.Cook(generateThreadLocalCopy);
                    IModelOutput rawOutput = outputGenerator.Generate();
                    ICollection<OutputProcessor> processors = ProcessorListManager.GetInstance()
                        .GetOutputProcessorList(generateThreadLocalCopy, TimesToReplicateOutput);

                    foreach (OutputProcessor processor in processors)
                    {
                        processor.Process(rawOutput);
                    }

                    DurationSeconds = rawOutput.OutputDurationMillis / 1000.0;

                    lastValidOutput = new LastValidOutput
                    {
                        CycleDuration = DurationSeconds,
                        model = generateThreadLocalCopy,
                        RawOutput = rawOutput,
                        IsCurrentModel = true
                    };
                }
                catch (CalibrationException e)
                {
                    ErrorCollector.Instance.AddError(e.Message /*+ "\nThe last valid model is used instead!"*/,
                        ErrorCategory.MainHardware, false, ErrorTypes.DynamicCompileError);
                }
                catch (ValidationException e)
                {
                    ErrorCollector.Instance.AddError(e.Message /*+ "\nThe last valid model is used instead!"*/,
                        ErrorCategory.Basic, false, ErrorTypes.OutOfRange);
                }
                catch (OutOfMemoryException)
                {
                    ErrorCollector.Instance.AddError(
                        "Cannot allocate the amount of RAM required for the generation of the model!",
                        ErrorCategory.Basic, false, ErrorTypes.ProgramError);
                }
                catch (PythonStepException e)
                {
                    ErrorCollector.Instance.AddError(e.Message, ErrorCategory.MainHardware, false,
                        ErrorTypes.DynamicCompileError);
                }
                catch (Exception e)
                {
                    ProcessUnexpectedException(e);
                }

                if (!lastValidOutput.IsCurrentModel && outputNotCurrentModelError == null)
                {
                    outputNotCurrentModelError = ErrorCollector.Instance.AddStickyError(
                        "Due to errors in the current model, it is not used for Output. The last valid model is used instead!",
                        ErrorCategory.Basic, true, ErrorTypes.Other);
                    ModelIsWrong = true;
                }
                else if (lastValidOutput.IsCurrentModel && outputNotCurrentModelError != null)
                {
                    ErrorCollector.Instance.RemoveSingleError(outputNotCurrentModelError);
                    outputNotCurrentModelError = null;
                }


                //Give the results to the output handler
                _outputHandler.SetNewCycleData(lastValidOutput.model, lastValidOutput.CycleDuration,
                    lastValidOutput.RawOutput, TimesToReplicateOutput);


                //Raise the event
                FinishedModelGenerationEventArgs args = new FinishedModelGenerationEventArgs();

                if (outputNotCurrentModelError != null)
                    args.IsSuccessful = false;
                else
                    args.IsSuccessful = true;

                OnFinishedModelGeneration(args);


                lock (lockObj)
                {
                    //If new root model has arrived during the execution of this loop do not quit
                    if (hasNewData)
                    {
                        quit = false;
                    }
                    else
                    {
                        //ensures that a new thread will be created if a new root model is given.
                        generateThreadRunning = false;
                    }
                }
            } while (!quit);

            CurrentGeneratorState = GeneratorState.Waiting;
        }

        private void ProcessUnexpectedException(Exception e)
        {
            string DEBUG_SAVE_PATH = "/Computer Control Debugging/";
            string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + DEBUG_SAVE_PATH;

            // save variables
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            string path = string.Format("{0}Debug at {1}.txt", saveFolder, DateTime.Now.Ticks);
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("Exception Type:");
                sw.WriteLine(e.GetType().ToString());
                sw.WriteLine("");
                sw.WriteLine("Exception Message:");
                sw.WriteLine(e.Message);
                sw.WriteLine("");
                sw.WriteLine("Exception Stacktrace:");
                sw.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Creates an exact copy of a serializable object.
        /// </summary>
        /// <param name="original">The original object to copy.</param>
        /// <returns>An exact copy of the input object.</returns>
        /// <remarks>
        /// This method uses the serialize-deserialize approach.
        /// </remarks>
        private Object DeepClone(Object original)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter {Context = new StreamingContext(StreamingContextStates.Clone)};
            formatter.Serialize(stream, original);
            stream.Position = 0;
            return formatter.Deserialize(stream);
        }

        /// <summary>
        /// Copies a new and valid model hierarchy to the buffer in order to send it eventually to the hardware system.
        /// </summary>
        /// <param name="copyJob">A new and a valid model hierarchy.</param>
        /// <param name="timesToReplicateOutput">The number of times to replicate output.</param>
        /// <remarks>
        /// Calling this method changes the state of the generator, and starts the generating thread if it has not already been started.
        /// </remarks>
        public void CopyData(IModel copyJob, int timesToReplicateOutput) //copies the data tree
        {
            lock (lockObj)
            {
                //RECO no need for this assignment.
                GeneratorState generatorState = CurrentGeneratorState;

                if (generatorState == GeneratorState.Generating)
                {
                    CurrentGeneratorState = GeneratorState.GeneratingPendingChanges;
                }

                hasNewData = true;
                _localCopy = (RootModel) DeepClone(copyJob);
                TimesToReplicateOutput = timesToReplicateOutput;

                if (!generateThreadRunning)
                {
                    Thread thread = new Thread(GenerateThread);
                    generateThreadRunning = true;
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// Triggers the <see cref=" OnGeneratorStateChange"/> event.
        /// </summary>
        private void UpdateGeneratorState()
        {
            OnGeneratorStateChange?.Invoke(this, null);
        }


        /// <summary>
        /// Raises the <see cref="E:FinishedModelGeneration" /> event.
        /// </summary>
        /// <param name="args">The <see cref="FinishedModelGenerationEventArgs"/> instance containing the event data.</param>
        protected void OnFinishedModelGeneration(FinishedModelGenerationEventArgs args)
        {
            if (FinishedModelGeneration != null)
                FinishedModelGeneration(this, args);
        }
    }
}