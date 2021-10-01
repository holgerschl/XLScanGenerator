using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLScanGenerator
{
    public static class Helpers
    {
        public static bool IsNotEqual(double[] lastPoint, double[] v, double snippingRadius)
        {
            if ((Math.Abs(v[0] - lastPoint[0]) < snippingRadius) && (Math.Abs(v[1] - lastPoint[1]) < snippingRadius))
                return false;
            return true;
        }
    }
}
