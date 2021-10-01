using System;
using System.Collections.Generic;
using System.Text;

namespace XLScanGenerator
{
    public class StatusChecker
    {
        public void Timer_Tick(object handle)
        {
            uint? h = handle as uint?;
            if (h != null)
            {
                var state = slsc_ExecState.slsc_ExecState_NotInitOrError;
                syncAXIS.slsc_ctrl_get_exec_state((uint)h, out state);
                // Check every millisecond if the ReadyForExecution state has been reached.
                if (state == slsc_ExecState.slsc_ExecState_ReadyForExecution)
                {
                    var retVal = syncAXIS.slsc_ctrl_start_execution((uint)h);
                }
            }
        }
    }

}
