using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XLScanGenerator
{
    class Program
    {
        public static string Message { get; private set; }

        static void Main(string[] args)
        {
            Message = "";
            SyncAxis syncAxisInstance = new SyncAxis();
            syncAxisInstance.JobExecutionFinishedOccured += Program_JobExecutionFinishedOccured;
            OpenVectorFormat.WorkPlane workPlane = new OpenVectorFormat.WorkPlane();
            uint handle = 0;
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: XLScanGenerator XLScan-XML-Filename Workplane-Filename Module-Filename" + System.Environment.NewLine + "Press any key to continue");
                Console.ReadKey();
                return;
            }
            var retVal = syncAxisInstance.InitializeSyncAxis(args[0], out handle);
            //TODO: Error handling
            workPlane = syncAxisInstance.LoadOVF(args[1]);
            syncAxisInstance.RunMarking(args[2], handle, workPlane, 1e-2);
            while (Message == "") ;
            syncAXIS.slsc_cfg_delete(handle);
        }

        private static void Program_JobExecutionFinishedOccured(object sender, JobExecutionFinishedEventArgs e)
        {
            Message = e.Message;
        }
    }
}
