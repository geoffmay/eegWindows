using System;
using System.Collections.Generic;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Analysis
{
    class TestPython
    {
        public static string testCall()
        {
            var ipy = Python.CreateRuntime();
            //dynamic test = ipy.UseFile("Test.py");
            //dynamic test = ipy.UseFile(@"C:\Users\Eeg\source\repos\PyWrapper.py");
            dynamic test = ipy.UseFile("PyWrapper.py");
            dynamic result = test.Simple();
            return result as string;
        }
    }
}
