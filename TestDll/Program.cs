using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
//using System.Threading.Tasks;
//using MathWorks.MATLAB.NET.Arrays;

namespace TestDll
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            double a = mlxTestFunction1(2, 3);

        }

        [DllImport("test.dll", EntryPoint = "mlxTestFunction1")]
        public static extern double mlxTestFunction1(int nargout, int value);
        

    }
}
