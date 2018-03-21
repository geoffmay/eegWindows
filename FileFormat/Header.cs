using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Analysis
{
    public class Header
    {
        public string patientID;
        public DateTime patientBirthDate;
        public string technicianName;

        public DateTime recordingStart;
        public DateTime recordingEnd;
        public int numberOfChannels;
        public double sampleRate;
        //public Amplifier.DataType dataType;
        public string dataType;
        public string neurofeedbackType;
        public string[] channelLabels;

        public Header()
        {
        }
        public static Header Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Header>(json);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
