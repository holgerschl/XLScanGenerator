using System;

namespace XLScanGenerator
{
    public class JobFinishedExecutionCallback : ExecTimeCallback
    {
        /// <summary>
        /// Event that is invoked when the callback occurs and transfers the simulation filename to the Model
        /// </summary>
        public event EventHandler<JobExecutionFinishedEventArgs> JobExecutionFinishedOccured;
        /// <summary>
        /// The handle of the syncAXIS instance
        /// </summary>
        private uint _handle;
        /// <summary>
        /// Constructor transfers the handle to the callback class
        /// </summary>
        /// <param name="handle"></param>
        public JobFinishedExecutionCallback(uint handle) : base()
        {
            _handle = handle;
        }
        /// <summary>
        /// This is the method that is executed when the Job execution is finished. It needs to be overriden by the user.
        /// </summary>
        /// <param name="JobID">JobID is available through the interface</param>
        /// <param name="Progress">State of Progress</param>
        /// <param name="ExecTime">Execution time</param>
        public override void Run(uint JobID, ulong Progress, double ExecTime)
        {
            string simulationFilename = "";
            var retVal = syncAXIS.slsc_ctrl_get_syncaxis_simulation_filename(_handle, JobID, out simulationFilename);
            // Check if the simulation mode is chosen
            if ((retVal ^ 2048) == 0)
                OnJobExecutionFinished("Actual run executing");
            else OnJobExecutionFinished(simulationFilename);
        }
        // Invoke the JobExecutionFinishedOccured event that is handled by the Model and transfers the simulation filename to the Model.
        private void OnJobExecutionFinished(string message)
        {
            JobExecutionFinishedOccured?.Invoke(this, new JobExecutionFinishedEventArgs(message));
        }
    }
}