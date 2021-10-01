using OpenVectorFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XLScanGenerator
{
    public class SyncAxis
    {
        private Timer _timer;
        public static Task<uint> SyncAxisTask { get; private set; }
        public JobFinishedExecutionCallback JobFinishedExecuting { get; private set; }

        public event EventHandler<JobExecutionFinishedEventArgs> JobExecutionFinishedOccured;

        internal OpenVectorFormat.WorkPlane LoadOVF(string filename)
        {
            WorkPlane workPlane;

            using (var input = File.OpenRead(filename))
            {

                workPlane = WorkPlane.Parser.ParseFrom(input);
                return workPlane;
            }

        }

        public SyncAxis()
        {

        }
        internal uint InitializeSyncAxis(string xmlFileName, out uint handle)
        {
            handle = 0;
            var retVal = syncAXIS.slsc_cfg_initialize_from_file(out handle, xmlFileName);
            JobFinishedExecuting = new JobFinishedExecutionCallback(handle);

            JobFinishedExecuting.JobExecutionFinishedOccured += JobFinishedExecuting_JobExecutionFinishedOccured;
            StatusChecker statusChecker = new StatusChecker();
            _timer = new Timer(statusChecker.Timer_Tick, handle, 0, 100);
            // Hook the method to the event. This is needed to transfer data to the ViewModel and is not essential for syncAXIS operation
            var ret = syncAXIS.slsc_cfg_register_callback_job_finished_executing(handle, JobFinishedExecuting);
            // Set up a timer that checks every 10 milliseconds for the ReadyForExecution status of the job queue after the job is started. 

            return retVal;
        }
        internal uint RunMarking(string fileName, uint handle, OpenVectorFormat.WorkPlane workPlane, double snippingRadius)
        {


            uint JobId = 0;
            // Register the callback occuring after the job is finished executing
            // Begin the list handling
            double[] LastPoint = new double[] { 0, 0 };

            double[] StartPosition = { 0.0, 0.0 };
            uint ret = syncAXIS.slsc_list_begin_module(handle, out JobId, StartPosition, fileName);
            int i = 0, j = 0;
            // Create the list of mark and jump commands that make up the scanlab logo
            var vectorBlocks = workPlane.VectorBlocks;
            foreach (var vectorBlock in vectorBlocks)
            {
                switch (vectorBlock.VectorDataCase)
                {
                    case OpenVectorFormat.VectorBlock.VectorDataOneofCase.LineSequence:

                        var lineSequence = vectorBlock.LineSequence;
                        if (lineSequence.Points.Count > 3)
                        {
                            if (Helpers.IsNotEqual(LastPoint, new double[] { lineSequence.Points[0], lineSequence.Points[1] }, snippingRadius))
                                ret = syncAXIS.slsc_list_jump_abs(handle, new double[] { lineSequence.Points[0], lineSequence.Points[1] });
                            for (int k = 2; k < lineSequence.Points.Count; k += 2)
                                ret = syncAXIS.slsc_list_mark_abs(handle, new double[] { lineSequence.Points[k], lineSequence.Points[k + 1] });
                            LastPoint = new double[] { lineSequence.Points[lineSequence.Points.Count - 2], lineSequence.Points[lineSequence.Points.Count - 1] };
                        }
                        break;
                    case VectorBlock.VectorDataOneofCase.Arcs:
                        var arcs = vectorBlock.Arcs;
                        double[] StartPoint = new double[] { arcs.Centers[0] + arcs.StartDx, arcs.Centers[1] + arcs.StartDy };
                        double Radius = Math.Sqrt((arcs.StartDx * arcs.StartDx) +
                            (arcs.StartDy * arcs.StartDy));
                            double[] MidPoint = new double[] { arcs.Centers[0] + Radius * Math.Cos((180-arcs.Angle/2) * Math.PI / 180.0)
                                                          , arcs.Centers[1] + Radius * Math.Sin((180-arcs.Angle/2)  * Math.PI / 180.0) };

                            double[] TargetPoint = new double[] { arcs.Centers[0] + Radius * Math.Cos((180-arcs.Angle) * Math.PI / 180.0)
                                                            , arcs.Centers[1] + Radius * Math.Sin((180-arcs.Angle) * Math.PI / 180.0) };
                            if (Helpers.IsNotEqual(LastPoint, StartPoint, snippingRadius))
                                ret = syncAXIS.slsc_list_jump_abs(handle, StartPoint);
                            //ret = syncAXIS.slsc_list_mark_abs(handle, MidPoint);
                            //ret = syncAXIS.slsc_list_mark_abs(handle, TargetPoint);
                            ret = syncAXIS.slsc_list_arc_abs(handle, MidPoint, TargetPoint);
                            LastPoint = TargetPoint;
                        break;

                    //else if (vectorBlock is DXFArc)
                    //{
                    //    DXFArc arc = vectorBlock as DXFArc;

                    //    double[] StartPoint = new double[] { arc.Point.X + arc.Radius * Math.Cos(arc.StartAngle * Math.PI / 180.0), arc.Point.Y + arc.Radius * Math.Sin(arc.StartAngle * Math.PI / 180.0) };
                    //    double[] MidPoint = new double[] { 0, 0 };
                    //    if (arc.EndAngle > arc.StartAngle)
                    //        MidPoint = new double[] { arc.Point.X + arc.Radius * Math.Cos((arc.EndAngle + arc.StartAngle) / 2 * Math.PI / 180.0), arc.Point.Y + arc.Radius * Math.Sin((arc.EndAngle + arc.StartAngle) / 2 * Math.PI / 180.0) };
                    //    else
                    //        MidPoint = new double[] { arc.Point.X - arc.Radius * Math.Cos((arc.EndAngle + arc.StartAngle) / 2 * Math.PI / 180.0), arc.Point.Y - arc.Radius * Math.Sin((arc.EndAngle + arc.StartAngle) / 2 * Math.PI / 180.0) };

                    //    double[] TargetPoint = new double[] { arc.Point.X + arc.Radius * Math.Cos(arc.EndAngle * Math.PI / 180.0), arc.Point.Y + arc.Radius * Math.Sin(arc.EndAngle * Math.PI / 180.0) };

                    //    if (Helpers.IsNotEqual(LastPoint, StartPoint, snippingRadius))
                    //        ret = syncAXIS.slsc_list_jump_abs(handle, StartPoint);
                    //    //ret = syncAXIS.slsc_list_mark_abs(handle, MidPoint);
                    //    //ret = syncAXIS.slsc_list_mark_abs(handle, TargetPoint);
                    //    ret = syncAXIS.slsc_list_arc_abs(handle, MidPoint, TargetPoint);
                    //    LastPoint = TargetPoint;
                    //}
                    //else if (vectorBlock is DXFCircle)
                    //{
                    //    DXFCircle circle = (DXFCircle)vectorBlock;

                    //    double[] StartPoint = new double[] { circle.Point.X + circle.Radius, circle.Point.Y };

                    //    if (Helpers.IsNotEqual(LastPoint, StartPoint, snippingRadius))
                    //        ret = syncAXIS.slsc_list_jump_abs(handle, StartPoint);
                    //    //ret = syncAXIS.slsc_list_mark_abs(handle, MidPoint);
                    //    //ret = syncAXIS.slsc_list_mark_abs(handle, TargetPoint);
                    //    ret = syncAXIS.slsc_list_circle_2d_abs(handle, new double[] { circle.Point.X, circle.Point.Y }, 2 * Math.PI);
                    //    LastPoint = StartPoint;
                    //}
                    //else if (vectorBlock is DXFPolyLine)
                    //{
                    //    DXFPolyLine polyLine = (DXFPolyLine)vectorBlock;
                    //    foreach (var entity in polyLine.Children)
                    //    {
                    //        if (entity is DXFLine)
                    //        {
                    //            var line = entity as DXFLine;
                    //            if (Helpers.IsNotEqual(LastPoint, new double[] { line.P1.X, line.P1.Y }, snippingRadius))
                    //                ret = syncAXIS.slsc_list_jump_abs(handle, new double[] { line.P1.X, line.P1.Y });
                    //            ret = syncAXIS.slsc_list_mark_abs(handle, new double[] { line.P2.X, line.P2.Y });
                    //            LastPoint = new double[] { line.P2.X, line.P2.Y };
                    //        }
                    //    }
                    //}
                    //else if (vectorBlock is DXFHatch)
                    //{
                    //    DXFHatch hatch = (DXFHatch)vectorBlock;
                    //    foreach (var entity in hatch.Children)
                    //    {
                    //        if (entity is DXFLine)
                    //        {
                    //            var line = entity as DXFLine;
                    //            if (Helpers.IsNotEqual(LastPoint, new double[] { line.P1.X, line.P1.Y }, snippingRadius))
                    //                ret = syncAXIS.slsc_list_jump_abs(handle, new double[] { line.P1.X, line.P1.Y });
                    //            ret = syncAXIS.slsc_list_mark_abs(handle, new double[] { line.P2.X, line.P2.Y });
                    //            LastPoint = new double[] { line.P2.X, line.P2.Y };
                    //        }
                    //    }
                    default:
                        break;
                }
            }
            // End the list handling           
            ret = syncAXIS.slsc_list_end(handle);
            // Important: The job is not started here - instead the timer event checks every millisecond wether the ReadyForExecution Status is reached.
            // As soon as that happens the execution is started. This can happen BEFORE the last mark or jump command has been added to the list.
            return ret;
        }


        private void JobFinishedExecuting_JobExecutionFinishedOccured(object sender, JobExecutionFinishedEventArgs e)
        {
            // Transfer the syncAXIS simulation filename and the info, if the system is in simulation mode to the ViewModel
            OnJobExecutionFinished(e.Message);
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

        }
        private void OnJobExecutionFinished(string message)
        {
            JobExecutionFinishedOccured?.Invoke(this, new JobExecutionFinishedEventArgs(message));
        }

    }
}
