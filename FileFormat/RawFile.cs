using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Analysis.FileFormat
{
    class RawFile
    {
        public List<List<double>> m_data;
        public string m_filename;
        public RawFile()
        {
            m_data = new List<List<double>>();
        }
        public static RawFile OpenFile(string filename, int numberOfChannels)
        {
            RawFile rf = new RawFile();
            rf.m_filename = filename;
            for (int i = 0; i < numberOfChannels; i++)
            {
                rf.m_data.Add(new List<double>());
            }
            using (FileStream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                //}
                //        using (StreamReader sReader = new StreamReader(filename))
                //        {
                BinaryReader reader = new BinaryReader(fStream);
                int channelCounter = 0;
                while (fStream.Position < fStream.Length)
                {
                    rf.m_data[channelCounter].Add(reader.ReadDouble());
                    channelCounter++;
                    if (channelCounter >= numberOfChannels)
                    {
                        channelCounter = 0;
                    }
                }
            }
            return rf;
        }
    }
}
