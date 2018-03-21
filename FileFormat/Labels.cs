using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis
{
    public static class Labels
    {
        public static string[] antLabelsEegoRt32()
        {
            return new string[] {   
                "Fp1", "Fpz", "Fp2",      //0
            "F7","F3", "Fz", "F4", "F8",  //3 
             "FC5","FC1", "FC2", "FC6",   //8
       "M1", "T7", "C3", "Cz", "C4", "T8", "M2",  //13 
            "CP5", "CP1","CP2", "CP6",  //20 
           "P7", "P3", "Pz", "P4", "P8",  //24
                       "POz",               //29
                  "O1", "Oz", "O2",         //30
                    "CPz", "counter" }; ;   //33
        }
    }
}
