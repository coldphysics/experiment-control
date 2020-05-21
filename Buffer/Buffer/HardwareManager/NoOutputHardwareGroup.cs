using Communication.Interfaces.Generator;
using Communication.Interfaces.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buffer.HardwareManager
{
    /// <summary>
    /// This class is a dummy hardware group that only simulates the duration of the output cycle
    /// It is not thread safe!
    /// </summary>
    public class NoOutputHardwareGroup : IHardwareGroup
    {
        /// <summary>
        /// Indicates that the output is over
        /// </summary>
        private bool hasFinished = true;
        /// <summary>
        /// The duration of the output
        /// </summary>
        private double duration;

        public bool HasFinished()
        {
            return hasFinished;
        }

        public void Initialize(IModelOutput data)
        { 
            duration = data.OutputDurationMillis;

            // if the previous output is not over, but we still initialize
            if (!hasFinished)
            {
                Console.WriteLine("Warning: calling HW.Initilize() before output is finished!");
            }
        }

        public void Start()
        {
            // if the previous output is not over, but we still start
            if (!hasFinished)
            {
                Console.WriteLine("Warning: calling HW.Start() before output is finished!");
            }

            hasFinished = false;
            // must be at least 1!
            int durationMillis = Math.Max((int)Math.Round(duration), 1);
        
            Task t = Execute(() =>
             {
                 hasFinished = true;
             }, durationMillis);
            
        }

        public async Task Execute(Action action, int timeoutInMilliseconds)
        {
            await Task.Delay(timeoutInMilliseconds);
            action();
        }
    }
}
