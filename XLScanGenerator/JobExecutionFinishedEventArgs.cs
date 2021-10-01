namespace XLScanGenerator
{
    public class JobExecutionFinishedEventArgs
    {
        /// <summary>
        /// The simulation filename if syncAXIS is in simulation mode
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The simulation filename if syncAXIS is in simulation mode</param>
        /// <param name="simulationFilenamePresent">Is true when simulation mode is chosen</param>
        public JobExecutionFinishedEventArgs(string message)
        {
            Message = message;
        }
    }

}