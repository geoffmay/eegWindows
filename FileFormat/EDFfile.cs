using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Analysis;


namespace Analysis
{
    public class EDFfile
    {
        public EDFHeader m_header;
        private List<double> m_continuousRecordStartTimes;
        private List<int> m_continuousRecordStartIndices;
        private List<string> m_annotations;
        private List<double> m_annotationOnsets;
        private List<double> m_annotationDurations;
        private string m_fileName;
        private int bytesPerSample;
        private double[] m_sampleRates;

        private int[][] m_allSamples;
        private bool m_fullyInMemory = false;
        private double m_memoryStartTime = -1;
        private double m_memoryEndTime = -1;
        private int[] m_readFunctionOffset;
        private int[] m_readFunctionCount;
        private double[] m_autoGainRangeReciprocals;
        private double[] m_autoGainMidRanges;

        private bool m_drawModeAntiAlias = false;
        private bool m_drawModeGridVisible = false;


        #region constructors
        private EDFfile()
        {
            m_fileName = "";
            m_continuousRecordStartTimes = new List<double>();
            m_continuousRecordStartIndices = new List<int>();
            m_annotations = new List<string>();
            m_annotationOnsets = new List<double>();
            m_annotationDurations = new List<double>();
            m_header = new EDFHeader();
        }
        public static EDFfile CreateNewEDF(string fileName)
        {
            EDFfile retVal = new EDFfile();
            retVal.m_fileName = fileName;
            if (File.Exists(fileName))
            {
                throw new ArgumentException("EDF file " + fileName + " already exists.");
            }
            //todo: make a file
            return retVal;
        }
        #endregion constructors

        #region serialization
        public static EDFHeader FillGapsInHeader(EDFHeader header)
        {
            if (header.NumberOfSignals == 0)
            {
                string[] labels = Labels.antLabelsEegoRt32();
                foreach (string label in labels)
                {
                    header.AddSignal(label, "EEG", "uV", -1, 1, -32767, 32767, "", 2048, "");
                }
            }
            if (header.PtInfoBirthdate.CompareTo(DateTime.MinValue) == 0)
            {
                header.PtInfoBirthdate = new DateTime(1983, 11, 1);
            }
            if (header.RecInfoTechnician.Equals(""))
            {
                header.RecInfoTechnician = "salvaged";
            }
            return header;
        }
        public static void Convert(string eegoRawOutputFilename, Header header, string outputFilename, bool rereferenceToLinkedEars)
        {
            EDFHeader edfHeader = new EDFHeader();
            edfHeader.PtInfoBirthdate = header.patientBirthDate;
            edfHeader.PtInfoName = header.patientID;
            edfHeader.PtInfoHospitalID = "VISN17CoE";
            edfHeader.RecInfoEquipment = "eego rt";
            edfHeader.RecInfoStartDate = header.recordingStart;
            edfHeader.RecInfoTechnician = header.technicianName;
            Convert(eegoRawOutputFilename, edfHeader, outputFilename, rereferenceToLinkedEars);
        }
        public static void Convert(string eegoRawOutputFilename, EDFHeader inputEdfHeader, string outputFilename, bool rerefernceToLinkedEars)
        {
            EDFHeader.SampleStyle sampleStyle = EDFHeader.SampleStyle.Continuous;
            int digimin = -32767; //2^15-1
            int digimax = 32767;
            int bytesPerSample = inputEdfHeader.bytesPerSample;
            if (bytesPerSample == 0)
            {
                bytesPerSample = 2;
            }
            if (outputFilename.ToLower().EndsWith("bdf"))
            {
                sampleStyle = EDFHeader.SampleStyle.Biosemi24Bit;
                digimin = -8388607; //2^23-1
                digimax = 8388607;
                bytesPerSample = 3;
                inputEdfHeader.bytesPerSample = 3;
                inputEdfHeader.Version = (char)255 + "BIOSEMI";
            }
            else
            {
                sampleStyle = EDFHeader.SampleStyle.Continuous;
                inputEdfHeader.Version = "0";
            }
            //EDFfile file = new EDFfile();
            inputEdfHeader = FillGapsInHeader(inputEdfHeader);
            int channelCount = inputEdfHeader.NumberOfSignals;
            string rootName = eegoRawOutputFilename;
            if (rootName.ToLower().EndsWith(".txt") || rootName.ToLower().EndsWith(".eegdata") || rootName.ToLower().EndsWith(".json"))
            {
                rootName = rootName.Substring(0, rootName.LastIndexOf('.'));
            }
            string dataFilename = rootName + ".eegData";
            string eventFilename = rootName + "Events.txt";
            string headerInfoFileName = rootName + ".json";
            Header headerInfo;
            DateTime waitFromHere = DateTime.Now;
            bool ready = false;
            while (!ready && waitFromHere.AddSeconds(5).CompareTo(DateTime.Now) > 0)
            {
                if (File.Exists(headerInfoFileName))
                {
                    ready = true;
                }
            }
            int a1SignalNumber = -1;
            int a2SignalNumber = -1;
            int cpzSignalNumber = -1;

            using (StreamReader sReader = new StreamReader(headerInfoFileName))
            {
                headerInfo = Header.Deserialize(sReader.ReadToEnd());
                inputEdfHeader.RecInfoStartDate = headerInfo.recordingStart;
                for (int i = 0; i < headerInfo.numberOfChannels; i++)
                {
                    inputEdfHeader.SetSignalLocationLabel(i, headerInfo.channelLabels[i]);
                    if (headerInfo.dataType.Equals("Impedance"))
                    {
                        inputEdfHeader.SetSignalUnitLabel(i, "ohm");
                        inputEdfHeader.SetSignalType(i, "impedance");
                    }
                    else
                    {
                        inputEdfHeader.SetSignalType(i, "EEG");
                        inputEdfHeader.SetSignalType(i, "uV");
                    }
                    inputEdfHeader.SetSignalSamplesPerSecond(i, headerInfo.sampleRate);
                }
                inputEdfHeader.PtInfoBirthdate = headerInfo.patientBirthDate;
                inputEdfHeader.PtInfoName = headerInfo.patientID;
                inputEdfHeader.RecInfoStartDate = headerInfo.recordingStart;
                inputEdfHeader.RecInfoTechnician = headerInfo.technicianName;
                if (rerefernceToLinkedEars)
                {
                    for (int i = 0; i < inputEdfHeader.NumberOfSignals; i++)
                    {
                        if (inputEdfHeader.SignalLabel(i).Equals("M1"))
                        {
                            inputEdfHeader.SetSignalLocationLabel(i, "A1");
                            a1SignalNumber = i;
                        }
                        else if (inputEdfHeader.SignalLabel(i).Equals("M2"))
                        {
                            inputEdfHeader.SetSignalLocationLabel(i, "A2");
                            a2SignalNumber = i;
                        }
                        else if (inputEdfHeader.SignalLabel(i).Equals("CPz"))
                        {
                            cpzSignalNumber = i;
                        }
                    }
                }

                //not transcribed:
                //headerInfo.neurofeedbackType;
                //headerInfo.recordingEnd;
            }

            using (FileStream inStream = new FileStream(dataFilename, FileMode.Open, FileAccess.Read))
            {
                long sampleCount = inStream.Length / sizeof(double) / inputEdfHeader.NumberOfSignals;
                double seconds = Math.Ceiling((double)sampleCount
                    / (double)inputEdfHeader.SignalSamplesPerRecord(0));
                //double seconds = Math.Ceiling((double)sampleCount
                //    / (double)inputEdfHeader.SignalSamplesPerRecord(0)) * 2 / 3;
                inputEdfHeader.RecordDuration = 1;
                inputEdfHeader.NumberOfRecords = (int)(Math.Ceiling(seconds));
                long inPosition = 0;
                if (channelCount < 1)
                {
                    //todo: handle variable channels (with eego this could be done by figuring out how long the space is between increments
                    channelCount = 34;
                }
                byte[] buffer = new byte[channelCount * sizeof(double)];
                double[] samples = new double[channelCount];
                #region rangeFinding
                double[] min = new double[channelCount];
                double[] max = new double[channelCount];
                double[] range = new double[channelCount];
                for (int i = 0; i < channelCount; i++)
                {
                    min[i] = double.MaxValue;
                    max[i] = double.MinValue;
                }
                while (inPosition < inStream.Length)
                {
                    int bytesRead = inStream.Read(buffer, 0, buffer.Length);
                    Buffer.BlockCopy(buffer, 0, samples, 0, buffer.Length);
                    if (rerefernceToLinkedEars)
                    {
                        double avgEar = (samples[a1SignalNumber] + samples[a2SignalNumber]) / 2;
                        for (int rereferenceChannelIndex = 0; rereferenceChannelIndex < samples.Length; rereferenceChannelIndex++)
                        {
                            samples[rereferenceChannelIndex] -= avgEar;
                        }
                    }

                    for (int i = 0; i < samples.Length; i++)
                    {
                        if (samples[i] > max[i]) { max[i] = samples[i]; }
                        if (samples[i] < min[i]) { min[i] = samples[i]; }
                    }
                    inPosition += bytesRead;
                }
                double globalMax = double.MinValue;
                double globalMin = double.MaxValue;
                double maxRange = double.MinValue;
                for (int i = 0; i < channelCount - 1; i++)
                {
                    range[i] = max[i] - min[i];
                    if (range[i] > maxRange) { maxRange = range[i]; }
                    if (globalMax < max[i]) { globalMax = max[i]; }
                    if (globalMin > min[i]) { globalMin = min[i]; }
                }
                double globalRange = globalMax - globalMin;



                #endregion rangeFinding

                //set channel "physical" ranges
                double[] physMins = new double[channelCount];
                double[] physMaxs = new double[channelCount];
                for (int i = 0; i < channelCount; i++)
                {
                    double physMin = min[i] * 1000000; //volts to microvolts
                    double physMax = max[i] * 1000000; //volts to microvolts
                    physMins[i] = physMin;
                    physMaxs[i] = physMax;
                    //double average = (max[i] + min[i]) / 2;
                    //double halfRange = maxRange / 2;
                    //double physMin = average - halfRange;
                    //double physMax = average + halfRange;
                    //physMins[i] = physMin;
                    //physMaxs[i] = physMax;
                    //double fakeHalfRange = 32767; //evaluated using a simulator
                    //physMin = -fakeHalfRange;
                    //physMax = fakeHalfRange;
                    if (i >= inputEdfHeader.NumberOfSignals)
                    {
                        inputEdfHeader.AddSignal(Labels.antLabelsEegoRt32()[i], "EEG", "uV",
                            physMin, physMax, digimin, digimax,
                            "none", 2048, "");
                    }
                    else
                    {
                        inputEdfHeader.SetMinMax(i, digimin, digimax, physMin, physMax);
                    }
                }
                int samplesPerRecord = inputEdfHeader.SignalSamplesPerRecord(0);
                for (int i = 1; i < inputEdfHeader.NumberOfSignals; i++)
                {
                    if (samplesPerRecord != inputEdfHeader.SignalSamplesPerRecord(i))
                    {
                        throw new ApplicationException("currently unable to handle variable sample rates");
                    }
                }

                //write the header
                using (FileStream fstream = new FileStream(outputFilename, FileMode.Create, FileAccess.Write))
                {
                    inputEdfHeader.Write(fstream);
                }
                //unfudge the physical minima/maxima
                for (int i = 0; i < channelCount; i++)
                {
                    inputEdfHeader.SetMinMax(i, digimin, digimax, physMins[i], physMaxs[i]);
                }

                ////debug
                //double[] physRanges = new double[physMins.Length];
                //List<double> uniqueRanges = new List<double>();
                //List<int> rangeCounts = new List<int>();
                //for (int i = 0; i < physMins.Length; i++)
                //{
                //    physRanges[i] = physMaxs[i] - physMins[i];
                //    bool found = false;
                //    for (int j = 0; j < uniqueRanges.Count; j++)
                //    {
                //        if (physRanges[i] == uniqueRanges[j])
                //        {
                //            rangeCounts[j]++;
                //            found = true;
                //        }
                //    }
                //    if (!found)
                //    {
                //        uniqueRanges.Add(physRanges[i]);
                //        rangeCounts.Add(1);
                //    }
                //}
                //double modeRange = -1;
                //double modeCount = 0;
                //for (int i = 0; i < uniqueRanges.Count; i++)
                //{
                //    if (rangeCounts[i] > modeCount)
                //    {
                //        modeCount = rangeCounts[i];
                //        modeRange = uniqueRanges[i];
                //    }
                //}
                //double scaledRange = modeRange * 1000; //we assume that 

                ////end debug


                //write the data
                inPosition = 0;
                inStream.Seek(inPosition, SeekOrigin.Begin);
                int[] signalByteOffsetInRecord = new int[channelCount];
                double[] conversionFactors = new double[channelCount];
                for (int i = 0; i < channelCount; i++)
                {
                    signalByteOffsetInRecord[i] = inputEdfHeader.SignalByteOffsetInRecord(i);
                    // D = (P - Pmin) * conv + Dmin;   where conv = (Dmax - Dmin) / (Pmax - Pmin);
                    conversionFactors[i] = (inputEdfHeader.SignalDigitalMax(i) - inputEdfHeader.SignalDigitalMin(i)) / (inputEdfHeader.SignalPhysicalMax(i) - inputEdfHeader.SignalPhysicalMin(i));
                }
                byte[] record = new byte[inputEdfHeader.BytesPerRecord];
                using (FileStream outStream = new FileStream(outputFilename, FileMode.Append, FileAccess.Write))
                {
                    int recordNumber = 0;
                    while (inPosition < inStream.Length)
                    {
                        Array.Clear(record, 0, record.Length);
                        int writeOffset = 0;
                        while (writeOffset < signalByteOffsetInRecord[1])
                        {
                            int bytesRead = inStream.Read(buffer, 0, buffer.Length);
                            int channelNumber = 0;
                            for (int readCursor = 0; readCursor < buffer.Length; readCursor += sizeof(double))
                            {
                                int writeLocation = signalByteOffsetInRecord[channelNumber] + writeOffset;
                                // D = (P - Pmin) * conv + Dmin;   where conv = (Dmax - Dmin) / (Pmax - Pmin);
                                double phys = BitConverter.ToDouble(buffer, readCursor);
                                byte[] output;
                                if (channelNumber < channelCount - 1)
                                {
                                    double pMin = inputEdfHeader.SignalPhysicalMin(channelNumber);
                                    double dMin = inputEdfHeader.SignalDigitalMin(channelNumber);
                                    int dig = (int)((phys * 1000000 - pMin) * conversionFactors[channelNumber] + dMin);
                                    output = BitConverter.GetBytes(dig);
                                }
                                else
                                {
                                    output = BitConverter.GetBytes((int)phys);
                                }
                                Buffer.BlockCopy(output, 0, record, writeLocation, bytesPerSample);
                                channelNumber++;
                            }
                            inPosition += bytesRead;
                            writeOffset += bytesPerSample;
                        }
                        outStream.Write(record, 0, record.Length);
                        recordNumber++;
                    }
                }
            }
        }
        private void WriteHeader()
        {
            m_header.NormalizeRecordDuration();
            using (FileStream fs = new FileStream(m_fileName, FileMode.Create, FileAccess.Write))
            {
                m_header.Write(fs);
            }
        }
        private void WriteRecord(byte[] Record)
        {
            using (StreamWriter sWriter = new StreamWriter(m_fileName, true))
            {
                sWriter.Write(Record);
            }
        }
        public static EDFfile OpenFile(string fileName)
        {
            return OpenFile(fileName, false);
        }
        public static EDFfile OpenFile(string fileName, bool openFully)
        {
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("EDF file " + fileName + " doesn't exist.");
            }
            EDFfile retVal = new EDFfile();
            retVal.m_fileName = fileName;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                retVal.m_header.Parse(fs);
                if (retVal.m_header.Reserved.StartsWith("24BIT"))
                {
                    retVal.bytesPerSample = 3;
                }
                else
                {
                    retVal.bytesPerSample = 2;
                }
                retVal.m_sampleRates = new double[retVal.m_header.NumberOfSignals];
                retVal.m_readFunctionCount = new int[retVal.m_header.NumberOfSignals];
                retVal.m_readFunctionOffset = new int[retVal.m_header.NumberOfSignals];
                retVal.m_autoGainRangeReciprocals = new double[retVal.m_header.NumberOfSignals];
                retVal.m_autoGainMidRanges = new double[retVal.m_header.NumberOfSignals];
                for (int i = 0; i < retVal.m_header.NumberOfSignals; i++)
                {
                    retVal.m_sampleRates[i] = retVal.m_header.RecordDuration * retVal.m_header.SignalSamplesPerRecord(i);
                    retVal.m_autoGainMidRanges[i] = 0.0;
                    retVal.m_autoGainRangeReciprocals[i] = 1.0;
                }

                List<int> annotationSignals = new List<int>();
                try
                {
                    //analyze every annotation signal
                    for (int signalNumber = 0; signalNumber < retVal.m_header.NumberOfSignals; signalNumber++)
                    {
                        if (retVal.m_header.IsSignalAnnotation(signalNumber))
                        {
                            annotationSignals.Add(signalNumber);
                        }
                    }
                    for (int signalIndex = 0; signalIndex < annotationSignals.Count; signalIndex++)
                    {
                        int signalNumber = annotationSignals[signalIndex];
                        double lastRecordStartTime = 0;
                        for (int i = 0; i < retVal.m_header.NumberOfRecords; i++)
                        {
                            byte[] record = retVal.ReadSingleChannelSingleRecordFromDisk(i, signalNumber);
                            //find annotations
                            double recordStartTime = retVal.ParseAnnotations(record);
                            //find contiguous segments
                            if (signalIndex == 0)
                            {
                                //add the first record
                                if (i == 0)
                                {
                                    retVal.m_continuousRecordStartIndices.Add(i);
                                    retVal.m_continuousRecordStartTimes.Add(recordStartTime);
                                }
                                else
                                {
                                    double difference = recordStartTime - lastRecordStartTime - retVal.m_header.RecordDuration;
                                    if (difference > 0.01)
                                    {
                                        retVal.m_continuousRecordStartIndices.Add(i);
                                        retVal.m_continuousRecordStartTimes.Add(recordStartTime);
                                    }
                                }
                                lastRecordStartTime = recordStartTime;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //todo: log error
                }
            }
            if (openFully)
            {
                int[] offset, count;
                double endTime = retVal.m_header.NumberOfRecords * retVal.m_header.RecordDuration;
                retVal.m_allSamples = retVal.ReadRecord(0.0, endTime, out offset, out count);
                retVal.m_memoryStartTime = 0.0;
                retVal.m_memoryEndTime = endTime;

                retVal.m_fullyInMemory = true;
            }
            return retVal;
        }
        public void convertToRaw(string outputFilename)
        {
            using (FileStream fStream = new FileStream(outputFilename, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter writer = new BinaryWriter(fStream);
                for (int i = 0; i < m_allSamples.Length; i++)
                {
                    for (int j = 0; j < m_allSamples[i].Length; j++)
                    {
                        int dig = m_allSamples[i][j];
                        double phys = convertDigitalToPhysical(dig, i);
                        writer.Write(phys);
                    }
                }
                writer.Close();
            }
        }

        #endregion serialization

        #region accessors
        public int[][] ReadRecord(double startTime, double endTime, out int[] offset, out int[] count)
        {
            double duration = endTime - startTime;
            offset = m_readFunctionOffset;
            count = m_readFunctionCount;
            if (m_fullyInMemory)
            {
                for (int i = 0; i < m_header.NumberOfSignals; i++)
                {
                    offset[i] = (int)(m_sampleRates[i] * startTime);
                    count[i] = (int)(m_sampleRates[i] * duration);
                }
                return m_allSamples;
            }
            else
            {
                for (int i = 0; i < m_header.NumberOfSignals; i++)
                {
                    offset[i] = 0;
                    count[i] = (int)(m_sampleRates[i] * duration);
                }

                if (startTime >= endTime) { throw new ArgumentException("Start time must be less than end time."); }
                int startRecordIndex = (int)(startTime / m_header.RecordDuration);
                int endRecordIndex = (int)(endTime / m_header.RecordDuration);
                double[] samplesPerSecond = new double[m_header.NumberOfSignals];
                long[] startSample = new long[m_header.NumberOfSignals];
                long[] endSample = new long[m_header.NumberOfSignals];
                long[] numberOfSamplesToReturn = new long[m_header.NumberOfSignals];
                int[][] retShorts = new int[m_header.NumberOfSignals][];
                int[] bytesPerRecordForSignal = new int[m_header.NumberOfSignals];
                long[] writeIndex = new long[m_header.NumberOfSignals];
                long[] readIndex = new long[m_header.NumberOfSignals];
                long[] lastSampleIndexInRecord = new long[m_header.NumberOfSignals];


                for (int i = 0; i < m_header.NumberOfSignals; i++)
                {
                    samplesPerSecond[i] = m_header.SignalSamplesPerRecord(i) / m_header.RecordDuration;
                    startSample[i] = (long)(startTime * samplesPerSecond[i]);
                    endSample[i] = (long)(endTime * samplesPerSecond[i]);
                    numberOfSamplesToReturn[i] = endSample[i] - startSample[i];
                    retShorts[i] = new int[numberOfSamplesToReturn[i]];
                    bytesPerRecordForSignal[i] = m_header.SignalSamplesPerRecord(i) * bytesPerSample;
                    writeIndex[i] = 0;
                    readIndex[i] = m_header.SignalByteOffsetInRecord(i)
                        + (startSample[i] % m_header.SignalSamplesPerRecord(i)) * bytesPerSample;
                    lastSampleIndexInRecord[i] = endSample[i] % m_header.SignalSamplesPerRecord(i);
                }

                long recordNumber = startRecordIndex;
                byte[] theRecords = ReadRecordsFromDisk(startRecordIndex, endRecordIndex);
                //read the first record
                {
                    int signalIndex = 0;
                    while (signalIndex < m_header.NumberOfSignals)
                    {
                        while (writeIndex[signalIndex] < retShorts[signalIndex].Length &&
                            (readIndex[signalIndex] - m_header.SignalByteOffsetInRecord(signalIndex)) % m_header.BytesPerRecord
                            < bytesPerRecordForSignal[signalIndex])
                        {
                            int nextInt = 0;
                            for (int i = 0; i < bytesPerSample; i++)
                            {
                                int byteIndex = bytesPerSample - i - 1;
                                nextInt += theRecords[readIndex[signalIndex] + byteIndex];
                                if (i < bytesPerSample - 1)
                                {
                                    nextInt = nextInt << 8;
                                }
                            }
                            retShorts[signalIndex][writeIndex[signalIndex]] = nextInt;
                            writeIndex[signalIndex] = writeIndex[signalIndex] + 1;
                            readIndex[signalIndex] = readIndex[signalIndex] + bytesPerSample;
                        }
                        //move the cursor for the current signal forward to the next record
                        readIndex[signalIndex] = readIndex[signalIndex]
                            + m_header.BytesPerRecord
                            - m_header.SignalSamplesPerRecord(signalIndex) * bytesPerSample;
                        signalIndex++;
                    }
                    recordNumber++;
                }
                //read the middle records
                while (recordNumber < endRecordIndex)
                {
                    int signalIndex = 0;
                    while (signalIndex < m_header.NumberOfSignals)
                    {
                        //while (readIndex[signalIndex] < bytesPerRecordForSignal[signalIndex])
                        while ((readIndex[signalIndex] - m_header.SignalByteOffsetInRecord(signalIndex)) % m_header.BytesPerRecord
                            < bytesPerRecordForSignal[signalIndex])
                        {
                            int nextInt = 0;
                            for (int i = 0; i < bytesPerSample; i++)
                            {
                                int byteIndex = bytesPerSample - i - 1;
                                nextInt += theRecords[readIndex[signalIndex] + byteIndex];
                                if (i < bytesPerSample - 1)
                                {
                                    nextInt = nextInt << 8;
                                }
                            }
                            retShorts[signalIndex][writeIndex[signalIndex]] = nextInt;
                            writeIndex[signalIndex] = writeIndex[signalIndex] + 1;
                            readIndex[signalIndex] = readIndex[signalIndex] + bytesPerSample;
                        }
                        //move the cursor for the current signal forward to the next record
                        readIndex[signalIndex] = readIndex[signalIndex]
                            + m_header.BytesPerRecord
                            - m_header.SignalSamplesPerRecord(signalIndex) * bytesPerSample;
                        signalIndex++;
                    }
                    recordNumber++;
                }
                //read the last record
                if (startRecordIndex < endRecordIndex)
                {
                    int signalIndex = 0;
                    while (signalIndex < m_header.NumberOfSignals)
                    {
                        while ((readIndex[signalIndex] - m_header.SignalByteOffsetInRecord(signalIndex)) % m_header.BytesPerRecord
                            < lastSampleIndexInRecord[signalIndex])
                        {
                            int nextInt = 0;
                            for (int i = 0; i < bytesPerSample; i++)
                            {
                                int byteIndex = bytesPerSample - i - 1;
                                nextInt += theRecords[readIndex[signalIndex] + byteIndex];
                                if (i < bytesPerSample - 1)
                                {
                                    nextInt = nextInt << 8;
                                }
                            }
                            retShorts[signalIndex][writeIndex[signalIndex]] = nextInt;
                            writeIndex[signalIndex] = writeIndex[signalIndex] + 1;
                            readIndex[signalIndex] = readIndex[signalIndex] + bytesPerSample;
                        }
                        signalIndex++;
                    }
                    recordNumber++;

                }
                return retShorts;
            }
        }
        public int[] ReadSingleChannelFromDisk(int signalIndex, double startTime, double endTime)
        {
            if (m_header.IsSignalAnnotation(signalIndex)) { throw new ArgumentException("Attempted to read annotation singal as normal signal."); }
            if (startTime >= endTime) { throw new ArgumentException("Start time must be less than end time."); }
            long startRecordIndex = (long)(startTime / m_header.RecordDuration);
            long endRecordIndex = (long)(endTime / m_header.RecordDuration);
            //samples / second = samples / record / (seconds / record)
            double samplesPerSecond = m_header.SignalSamplesPerRecord(signalIndex) / m_header.RecordDuration;
            long startSample = (long)(startTime * samplesPerSecond);
            long endSample = (long)(endTime * samplesPerSecond);
            long numberOfSamplesToReturn = endSample - startSample;
            int[] returnInts = new int[numberOfSamplesToReturn];
            int bytesPerRecordForSignal = m_header.SignalSamplesPerRecord(signalIndex) * bytesPerSample;

            long writeIndex = 0;
            long recordNumber = startRecordIndex;
            //read the first record
            {
                byte[] firstRecord = ReadSingleChannelSingleRecordFromDisk(startRecordIndex, signalIndex);
                long readIndex = (startSample % m_header.SignalSamplesPerRecord(signalIndex)) * bytesPerSample;
                while (readIndex < bytesPerRecordForSignal)
                {
                    int nextInt = 0;
                    for (int i = 0; i < bytesPerSample; i++)
                    {
                        int byteIndex = bytesPerSample - i - 1;
                        nextInt += firstRecord[readIndex + byteIndex];
                        if (i < bytesPerSample - 1)
                        {
                            nextInt = nextInt << 8;
                        }
                    }
                    returnInts[writeIndex] = nextInt;
                    writeIndex++;
                    readIndex += bytesPerSample;
                }
                recordNumber++;
            }
            //read the middle records
            while (recordNumber < endRecordIndex)
            {
                int readIndex = 0;
                byte[] middleRecord = ReadSingleChannelSingleRecordFromDisk(recordNumber, signalIndex);
                while (readIndex < bytesPerRecordForSignal)
                {
                    int nextInt = 0;
                    for (int i = 0; i < bytesPerSample; i++)
                    {
                        int byteIndex = bytesPerSample - i - 1;
                        nextInt += middleRecord[readIndex + byteIndex];
                        if (i < bytesPerSample - 1)
                        {
                            nextInt = nextInt << 8;
                        }
                    }
                    returnInts[writeIndex] = nextInt;
                    writeIndex++;
                    readIndex += bytesPerSample;
                }
                recordNumber++;
            }
            //read the last record
            if (startRecordIndex < endRecordIndex)
            {
                int readIndex = 0;
                long lastSampleIndexInRecord = endSample % m_header.SignalSamplesPerRecord(signalIndex);
                byte[] lastRecord = ReadSingleChannelSingleRecordFromDisk(recordNumber, signalIndex);
                while (readIndex < lastSampleIndexInRecord)
                {
                    int nextInt = 0;
                    for (int i = 0; i < bytesPerSample; i++)
                    {
                        int byteIndex = bytesPerSample - i - 1;
                        nextInt += lastRecord[readIndex + byteIndex];
                        if (i < bytesPerSample - 1)
                        {
                            nextInt = nextInt << 8;
                        }
                    }
                    returnInts[writeIndex] = nextInt;
                    writeIndex++;
                    readIndex += bytesPerSample;
                }
            }
            return returnInts;
        }
        protected byte[] ReadSingleChannelSingleRecordFromDisk(long recordIndex, int signalIndex)
        {
            long readStart = m_header.BytesInHeader +
                recordIndex * (long)(m_header.BytesPerRecord) +
                m_header.SignalByteOffsetInRecord(signalIndex);
            int readLength = bytesPerSample * m_header.SignalSamplesPerRecord(signalIndex);
            byte[] retBytes = new byte[readLength];
            using (FileStream fs = new FileStream(m_fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Position = readStart;
                fs.Read(retBytes, 0, readLength);
            }
            return retBytes;
        }
        //protected byte[] ReadRecord(int recordIndex)
        //{
        //    int bytesPerRecord = (int)(m_header.BytesPerRecord);
        //    long readStart = m_header.BytesInHeader +
        //        recordIndex * bytesPerRecord;
        //    byte[] retBytes = new byte[0];
        //    using (FileStream fs = new FileStream(m_fileName, FileMode.Open, FileAccess.Read))
        //    {
        //        fs.Position = readStart;
        //        fs.Read(retBytes, 0, bytesPerRecord);
        //    }
        //    return retBytes;
        //}

        /*
         * Realize that the formula to compute the physical value, P, 
         * from the digitally stored (16-bit integer) value, D, reads: 
         * P = Pmin   +  (Pmax - Pmin) * (D - Dmin) / (Dmax - Dmin). 
         * In this formula, Pmin and Pmax are the physimin and physimax values, 
         * while Dmin and Dmax are the digimin and digimax values, all from the EDF header. 
         */
        public float convertDigitalToPhysical(int digitalSignal, int signalIndex)
        {
            double pMax = m_header.SignalPhysicalMax(signalIndex);
            double pMin = m_header.SignalPhysicalMin(signalIndex);
            double dMax = m_header.SignalDigitalMax(signalIndex);
            double dMin = m_header.SignalDigitalMin(signalIndex);
            if (this.bytesPerSample == 2)
            {
                Int16 d = (Int16)digitalSignal;
                return (float)(pMin + (pMax - pMin) * (d - dMin) / (dMax - dMin));
            }
            else if (this.bytesPerSample == 3)
            {
                int d = getSignedInt24(digitalSignal);
                return (float)(pMin + (pMax - pMin) * (d - dMin) / (dMax - dMin));
            }
            else { throw new ApplicationException("unhandled number of bytes"); }
        }

        // P = Pmin   +  (Pmax - Pmin) * (D - Dmin) / (Dmax - Dmin)
        // (P - Pmin) / (Pmax - Pmin) * (Dmax - Dmin) + Dmin = D.

        // D = (P - Pmin) * conv + Dmin;   where conv = (Dmax - Dmin) / (Pmax - Pmin);
        public double convertPhysicalToDigital(double physicalSignal, int signalIndex)
        {
            double pMax = m_header.SignalPhysicalMax(signalIndex);
            double pMin = m_header.SignalPhysicalMin(signalIndex);
            double dMax = m_header.SignalDigitalMax(signalIndex);
            double dMin = m_header.SignalDigitalMin(signalIndex);
            return (physicalSignal - pMin) * (dMax - dMin) / (pMax - pMin);
        }


        private static int getSignedInt24(int input)
        {
            if (input > 0xFFFFFF)
            {
                throw new ArgumentException("input is greater than max 24 bit integer");
            }
            if ((input & 0x800000) > 0)  // this is the sign bit
            {
                input = input ^ 0xFFFFFF; //flip the bits
                input = input + 1;        //add one per two's complement
                input = -input;           //preserve sign in the 32 bit int
            }
            return input;
        }
        protected byte[] ReadRecordsFromDisk(int recordStartIndex, int recordEndIndex)
        {
            int bytesPerRecord = (int)(m_header.BytesPerRecord);
            long readStart = m_header.BytesInHeader +
                recordStartIndex * bytesPerRecord;
            int readLength = (recordEndIndex - recordStartIndex + 1) * bytesPerRecord;
            byte[] retBytes = new byte[readLength];
            using (FileStream fs = new FileStream(m_fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Position = readStart;
                fs.Read(retBytes, 0, readLength);
            }
            return retBytes;
        }
        /// <summary>
        /// Parses annotation signal, storing them in memory.
        /// </summary>
        /// <param name="annotationSignal">The annotation bytes as stored in the edf file</param>
        /// <returns>The timestamp of the record start, to be used for determining continuity.</returns>
        //protected double ParseAnnotations(byte[] annotationSignal)
        public double ParseAnnotations(byte[] annotationSignal)
        {
            //first annotation is the record start time
            //Each TAL starts with a time stamp Onset[21]Duration[20] in which [21] and [20] are single bytes with value 21 and 20, respectively
            //byte values 43, 45, 46 and 48-57 (the '+', '-',  '.' and '0'-'9' characters, respectively). 
            //Onset must start with a '+' or a '-' character and specifies the amount of seconds by which the onset 
            //of the annotated event follows ('+') or precedes ('-') the startdate/time of the file, that is specified in the header. 
            //Duration must not contain any '+' or '-' and specifies the duration of the annotated event in seconds. 
            //If such a specification is not relevant, Duration can be skipped in which case its preceding [21] must also be skipped. 
            //Both Onset and Duration can contain a dot ('.') but only if the fraction of a second is specified (up to arbitrary accuracy). 
            int i = 0;

            //oh boy, a list of lists. 
            //The first list points out all the \0's which denotes completely separate 
            //notations, which can each have their own onset and duration.
            //The second list is a list of all notations at that same onset.
            List<List<int>> annotationLists = new List<List<int>>();
            annotationLists.Add(new List<int>());

            #region find delimeters
            while (i < annotationSignal.Length)
            {
                if (annotationSignal[i] == (byte)20)
                {
                    //annotationMarkers.Add(i);
                    annotationLists[annotationLists.Count - 1].Add(i);
                }
                else if (annotationSignal[i] == (byte)21)
                {
                    //annotationMarkers.Add(i);
                    annotationLists[annotationLists.Count - 1].Add(i);
                }
                else if (annotationSignal[i] == (byte)0)
                {
                    //Subsequent TALs in the same data record must follow immediately after the trailing  of the preceding TAL. 
                    //so two zeros in a row means there are no more annotations in this record
                    if ((i > 0) && (annotationSignal[i - 1] == (byte)0))
                    {
                        //this line was taken out because sometimes two nulls in a row were in the garbage preceeding
                        //the onset.
                        //i = annotationSignal.Length;
                    }
                    else
                    {
                        //listMarkers.Add(i);
                        annotationLists.Add(new List<int>());
                    }
                }
                i++;
            }
            #endregion find delimeters
            int annotationCursor = 0;
            double recordOnset = 0;
            //if the first TAL in a data record reads '+567[20][20]', then that data record starts 567s after the startdate/time of the file. 
            //If the data records contain 'ordinary signals', then the starttime of each data record must be the starttime of its signals. 
            //If there are no 'ordinary signals', then a non-empty annotation immediately following the time-keeping annotation 
            //(in the same TAL) must specify what event defines the starttime of this data record. 
            //For example, '+3456.789[20][20]R-wave[20]' indicates that this data record starts at the occurrence of an R-wave, 
            //which is 3456.789s after file start. 
            for (int listNumber = 0; listNumber < annotationLists.Count; listNumber++)
            {
                bool onsetDetermined = false;
                if (annotationLists[listNumber].Count > 0)
                {
                    int annotationNumber = 0;
                    int nextAnnotationCursor;
                    //This pickiness was commented out because I wanted to handle some garbage that appears in some files.
                    //if (annotationNumber + 1 >= annotationLists[listNumber].Count)
                    //{
                    //    throw new ArgumentException("Annotations were improperly formatted. No byte 20 or 21 follows onset.");
                    //}

                    nextAnnotationCursor = annotationLists[listNumber][annotationNumber];
                    //parse onset
                    string onsetString = ParseBytes(annotationSignal, annotationCursor, nextAnnotationCursor);
                    //debug: this was put here to handle some "garbage" input in a test file.
                    bool thisNotationIsShot = false;
                    double onset;
                    //All this tryparse stuff handles what appears to be corruption discovered in 
                    //a few sample files.  The problem was that some garbage was popping up before
                    //the duration, and the duration should be first.  This block ensures that.
                    while (!double.TryParse(onsetString, out onset) && !thisNotationIsShot)
                    {
                        if (onsetString.Contains("+"))
                        {
                            //advance one character, incase the first character is a + sign followed by garbage.
                            onsetString = onsetString.Substring(1);
                            //advance to the next +.  Maybe there's a number that follows it, and we'll treat that as the duration.
                            onsetString = onsetString.Substring(onsetString.IndexOf('+'));
                        }
                        else if (onsetString.Contains("-"))
                        {
                            onsetString = onsetString.Substring(1);
                            onsetString = onsetString.Substring(onsetString.IndexOf('-'));
                        }
                        else
                        {
                            thisNotationIsShot = true;
                            //debug
                            using (StreamWriter sWriter = new StreamWriter("debug annotations.txt", true))
                            {
                                sWriter.Write("ABN ");
                            }
                            //end debug

                        }
                    }
                    if (!thisNotationIsShot)
                    {
                        //end debug
                        if (!double.TryParse(onsetString, out onset))
                        {
                            int dummy = 0;
                        }


                        double duration = 0;
                        if (!onsetDetermined)
                        {
                            recordOnset = onset;
                            onsetDetermined = true;
                        }
                        //parse duration, if it exists.
                        //if (annotationSignal[annotationLists[listNumber][annotationNumber]] == (byte)21)
                        if (annotationSignal[nextAnnotationCursor] == (byte)21)
                        {
                            annotationNumber++;
                            annotationCursor = nextAnnotationCursor + 1;
                            if (annotationNumber + 1 >= annotationLists[listNumber].Count) { throw new ArgumentException("Annotations were improperly formatted. No byte 20 follows duration."); }
                            //nextAnnotationCursor = annotationLists[listNumber][annotationNumber + 1];
                            nextAnnotationCursor = annotationLists[listNumber][annotationNumber];
                            string durationString = ParseBytes(annotationSignal, annotationCursor, nextAnnotationCursor);
                            duration = double.Parse(durationString);
                        }

                        annotationNumber++;

                        //parse each remaining annotation
                        while (annotationNumber < annotationLists[listNumber].Count)
                        {
                            annotationCursor = nextAnnotationCursor + 1;
                            //nextAnnotationCursor = annotationLists[listNumber][annotationNumber + 1];
                            nextAnnotationCursor = annotationLists[listNumber][annotationNumber];
                            string annotationString = ParseBytes(annotationSignal, annotationCursor, nextAnnotationCursor);

                            if (annotationString != null && !annotationString.Equals(""))
                            {
                                m_annotations.Add(annotationString);
                                m_annotationOnsets.Add(onset);
                                m_annotationDurations.Add(duration);
                            }
                            annotationNumber++;
                        }
                        //burn the [20] and the [0] that indicates the end of the list
                        annotationCursor = nextAnnotationCursor + bytesPerSample;
                    }// <---debug, thisNotationIsShot
                }
            }
            return recordOnset;
        }
        protected string ParseBytes(byte[] toCopy, int startIndexInclusive, int endIndexExclusive)
        {
            char[] retVal = new char[endIndexExclusive - startIndexInclusive];
            for (int i = 0; i < retVal.Length; i++)
            {
                retVal[i] = (char)(toCopy[i + startIndexInclusive]);
            }
            return new string(retVal);
        }
        #endregion accessors

        #region IEEG interface implementations
        public int NumberOfChannels()
        {
            return m_header.NumberOfSignals;
        }
        public string ChannelLabel(int channelNumber)
        {
            string label, transducerType, dimensionUnit, prefilter, reserved;
            double pmin, pmax, dmin, dmax, samplesPerSecond;
            m_header.GetSignalParameters(channelNumber, out label, out  transducerType, out  dimensionUnit,
                out  pmin, out  pmax, out  dmin, out  dmax, out  prefilter, out  samplesPerSecond,
                out  reserved);
            return label;
        }
        public double FramesPerSecond()
        {
            int channelNumber = 0;
            string label, transducerType, dimensionUnit, prefilter, reserved;
            double pmin, pmax, dmin, dmax, samplesPerSecond;
            m_header.GetSignalParameters(channelNumber, out label, out  transducerType, out  dimensionUnit,
                out  pmin, out  pmax, out  dmin, out  dmax, out  prefilter, out  samplesPerSecond,
                out  reserved);
            return samplesPerSecond;
        }
        public long FramesInRecording()
        {
            long framesPerRecord = (long)(FramesPerSecond() * m_header.RecordDuration);
            return (long)m_header.NumberOfRecords * framesPerRecord;
        }
        #region IEEG interface (commented out)
        //public double[] Voltage(int channelNumber, double startTime, double endTime)
        //{
        //    int[] digitals = ReadRecord(channelNumber, startTime, endTime);
        //    double[] Voltages = new double[digitals.Length];
        //    double rangeReciprocal = 1.0/((double)Int16.MaxValue - (double)Int16.MinValue);
        //    double physRange = m_header.SignalPhysicalMax(channelNumber) - m_header.SignalPhysicalMin(channelNumber);
        //    for (int i = 0; i < digitals.Length; i++)
        //    {
        //        double ratio = ((double)digitals[i] - (double)Int16.MinValue) * rangeReciprocal;
        //        Voltages[i] = m_header.SignalPhysicalMin(channelNumber) + ratio * physRange;
        //    }
        //    return Voltages;
        //}
        //public double[][] Voltage(double startTime, double endTime)
        //{
        //    int offset, count;
        //    int[][] digitals = ReadRecord(startTime, endTime, out offset, out count);
        //    double[][] Voltages = new double[m_header.NumberOfSignals][];
        //    for (int h = 0; h < Voltages.Length; h++)
        //    {
        //        Voltages[h] = new double[digitals[h].Length];
        //        double rangeReciprocal = 1.0 / ((double)Int16.MaxValue - (double)Int16.MinValue);
        //        double physRange = m_header.SignalPhysicalMax(h) - m_header.SignalPhysicalMin(h);
        //        for (int i = 0; i < digitals[h].Length; i++)
        //        {
        //            double ratio = ((double)digitals[h][i] - (double)Int16.MinValue) * rangeReciprocal;
        //            Voltages[h][i] = m_header.SignalPhysicalMin(h) + ratio * physRange;
        //        }
        //    }
        //    return Voltages;
        //}
        //public int[] DigitalSignal(int channelNumber, double startTime, double endTime)
        //{
        //    return ReadRecord(channelNumber, startTime, endTime);
        //}
        //public int[][] DigitalSignal(double startTime, double endTime)
        //{
        //    return ReadRecord(startTime, endTime);
        //}
        #endregion IEEG interface (commented out)

        public string FileName()
        {
            return m_fileName;
        }

        #endregion IEEG interface implementations

        #region annotations
        protected void ReadAnnotations()
        {
            throw new ApplicationException("not implemented yet.");
        }
        public int NumberOfAnnotations
        {
            get
            {
                return m_annotations.Count;
            }
        }
        public string Annotation(int index)
        {
            throw new ApplicationException("not implemented yet.");
            return m_annotations[index];
        }
        public double AnnotationTime(int index)
        {
            throw new ApplicationException("not implemented yet.");
        }
        public double AnnotationDuration(int index)
        {
            throw new ApplicationException("not implemented yet.");
        }
        #endregion annotations

        #region drawing

        public Bitmap GenerateBitmap(int picWidth, int picHeight, bool DrawGrid, double startTime, double endTime, double gain, bool[] display)
        {
            if (picWidth < 1) { picWidth = 1; }
            if (picHeight < 1) { picHeight = 1; }
            Bitmap retBmp = new Bitmap(picWidth, picHeight);
            Graphics g = Graphics.FromImage(retBmp);
            g.FillRectangle((Brushes.White), 0, 0, picWidth, picHeight);
            return DrawSignal(retBmp, DrawGrid, startTime, endTime, gain, display);
        }
        public Bitmap DrawSignal(Bitmap picToModify, bool DrawGrid, double startTime, double endTime)
        {
            return DrawSignal(picToModify, DrawGrid, startTime, endTime, 0);
        }
        public Bitmap DrawSignal(Bitmap picToModify, bool DrawGrid, double startTime, double endTime, double gain)
        {
            bool[] display = new bool[m_header.NumberOfSignals];
            for (int i = 0; i < display.Length; i++)
            {
                display[i] = true;
            }
            return DrawSignal(picToModify, DrawGrid, startTime, endTime, gain, display);
        }
        private void setAutoGain(int[][] samples)
        {
            //int nChannels = m_header.NumberOfSignals;
            int nChannels = samples.Length;
            for (int i = 0; i < nChannels; i++)
            {
                m_autoGainMidRanges[i] = 0;
                m_autoGainRangeReciprocals[i] = 1;
            }

            double[] minValues = new double[nChannels];
            double[] maxValues = new double[nChannels];
            for (int channelIndex = 0; channelIndex < nChannels; channelIndex++)
            {
                minValues[channelIndex] = double.NaN;
                maxValues[channelIndex] = double.NaN;
                int endIndex = samples[0].Length;

                //for (int sampleIndex = offset[channelIndex];
                //    sampleIndex < m_allSamples[channelIndex].Length && sampleIndex < endIndex;
                //    sampleIndex++)
                for (int sampleIndex = 0; sampleIndex < samples[channelIndex].Length; sampleIndex++)
                {
                    //awkward ! > handles nans correctly
                    if(!(samples[channelIndex][sampleIndex] >= minValues[channelIndex]))
                    //if (!(m_allSamples[channelIndex][sampleIndex] >= minValues[channelIndex]))
                    {
                        minValues[channelIndex] = samples[channelIndex][sampleIndex];
                    }
                    if (!(samples[channelIndex][sampleIndex] <= maxValues[channelIndex]))
                    //if (!(m_allSamples[channelIndex][sampleIndex] <= maxValues[channelIndex]))
                    {
                        maxValues[channelIndex] = samples[channelIndex][sampleIndex];
                    }
                }
            }
            for (int i = 0; i < nChannels; i++)
            {
                m_autoGainMidRanges[i] = (maxValues[i] + minValues[i]) * 0.5;
                m_autoGainRangeReciprocals[i] = 1.0 / (maxValues[i] - minValues[i]);
                if (m_autoGainRangeReciprocals[i] == double.NaN ||
                    m_autoGainRangeReciprocals[i] == double.PositiveInfinity ||
                    m_autoGainRangeReciprocals[i] == double.NegativeInfinity)
                {
                    m_autoGainRangeReciprocals[i] = 1.0;
                }
            }
        }
        private void setAutoGain(int[] offset, int[] count)
        {
            int nChannels = m_header.NumberOfSignals;
            //double[] rangeReciprocals = new double[display.Length];
            //double[] midRanges = new double[display.Length];
            for (int i = 0; i < nChannels; i++)
            {
                m_autoGainMidRanges[i] = 0;
                m_autoGainRangeReciprocals[i] = 1;
                //rangeReciprocals[i] = double.NaN;
                //midRanges[i] = double.NaN;
            }

            double[] minValues = new double[nChannels];
            double[] maxValues = new double[nChannels];
            for (int channelIndex = 0; channelIndex < nChannels; channelIndex++)
            {
                minValues[channelIndex] = double.NaN;
                maxValues[channelIndex] = double.NaN;
                int endIndex = offset[channelIndex] + count[channelIndex];

                for (int sampleIndex = offset[channelIndex];
                    sampleIndex < m_allSamples[channelIndex].Length && sampleIndex < endIndex;
                    sampleIndex++)
                {
                    if (!(m_allSamples[channelIndex][sampleIndex] >= minValues[channelIndex]))
                    {
                        minValues[channelIndex] = m_allSamples[channelIndex][sampleIndex];
                    }
                    if (!(m_allSamples[channelIndex][sampleIndex] <= maxValues[channelIndex]))
                    {
                        maxValues[channelIndex] = m_allSamples[channelIndex][sampleIndex];
                    }
                }
            }
            for (int i = 0; i < nChannels; i++)
            {
                m_autoGainMidRanges[i] = (maxValues[i] + minValues[i]) * 0.5;
                m_autoGainRangeReciprocals[i] = 1.0 / (maxValues[i] - minValues[i]);
                if (m_autoGainRangeReciprocals[i] == double.NaN ||
                    m_autoGainRangeReciprocals[i] == double.PositiveInfinity ||
                    m_autoGainRangeReciprocals[i] == double.NegativeInfinity)
                {
                    m_autoGainRangeReciprocals[i] = 1.0;
                }
            }
        }
        public Bitmap DrawSignal(Bitmap picToModify, bool DrawGrid, double startTime, double endTime, double gain, bool[] display)
        {
            //debug
            //debug
            List<long> splits = new List<long>();
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            long linesDrawn = 0;
            stopWatch.Start();
            //end debug
            //end debug
            #region initialization
            int[][] samples = ReadRecord(startTime, endTime, out m_readFunctionOffset, out m_readFunctionCount);
            int numberOfDisplayedChannels = 0;
            for (int i = 0; i < display.Length; i++)
            {
                if (display[i])
                {
                    numberOfDisplayedChannels++;
                }
            }
            //debug
            splits.Add(stopWatch.ElapsedMilliseconds);
            //end debug

            //setAutoGain(m_readFunctionOffset, m_readFunctionCount);
            if (gain == 0)
            {
                setAutoGain(samples);
            }
            //debug
            splits.Add(stopWatch.ElapsedMilliseconds);
            //end debug

            //Bitmap picToModify = new Bitmap(picWidth, picHeight);
            Graphics g = Graphics.FromImage(picToModify);
            if (m_drawModeAntiAlias)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
            else
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }
            #endregion initialization

            #region drawgridlines
            //draw gridlines
            if (DrawGrid)
            {
                {
                    Pen minorPen = new Pen(Color.Pink);
                    Pen majorPen = new Pen(Color.Pink, Math.Max(2.0f, (float)picToModify.Width / 500.0f));
                    //start at a point where the frame is clearly at a second marker
                    double timeCursor = startTime;
                    timeCursor = (Math.Floor(timeCursor * 5.0) + 1.0) / 5.0;
                    double lastTic = endTime;
                    lastTic = (Math.Floor(lastTic * 5.0) + 1.0) / 5.0;
                    Pen drawPen = new Pen(Color.WhiteSmoke);
                    while (timeCursor < lastTic)
                    {
                        //major tic
                        if (Math.Floor(timeCursor) == timeCursor)
                        {
                            drawPen = majorPen;
                        }
                        //minor tic
                        else
                        {
                            drawPen = minorPen;
                        }
                        float x = (float)((timeCursor - startTime) / (endTime - startTime) * picToModify.Width);
                        g.DrawLine(drawPen, x, 0, x, picToModify.Height);

                        timeCursor += 0.2;
                        timeCursor = (Math.Floor(timeCursor * 5.0 + 0.5)) / 5.0;
                    }
                }
            }
            #endregion drawgridlines

            #region draw annotations
            //draw annotations
            int annotationIndex = 0;
            while (annotationIndex < m_annotationOnsets.Count && m_annotationOnsets[annotationIndex] < endTime)
            {
                if (m_annotationOnsets[annotationIndex] >= startTime)
                {
                    Pen p = new Pen(Color.Black);
                    float x;
                    if (m_annotationDurations[annotationIndex] > 0)
                    {
                        x = (float)((m_annotationOnsets[annotationIndex] - startTime) / (endTime - startTime) * picToModify.Width);
                        float width = (float)((m_annotationDurations[annotationIndex]) / (endTime - startTime) * picToModify.Width);
                        g.FillRectangle(Brushes.LightBlue, x, 0, width, picToModify.Height);
                    }
                    else
                    {
                        x = (float)((m_annotationOnsets[annotationIndex] - startTime) / (endTime - startTime) * picToModify.Width);
                        g.DrawLine(p, x, 0, x, picToModify.Height);
                    }
                    float fontHeight = picToModify.Height / (numberOfDisplayedChannels * 3);
                    g.DrawString(m_annotations[annotationIndex], new Font("Arial", fontHeight),
                        Brushes.Black, x, picToModify.Height - fontHeight - 10);
                }
                annotationIndex++;
            }
            #endregion draw annotations

            //debug
            splits.Add(stopWatch.ElapsedMilliseconds);
            //end debug

            #region draw signals


            float x1, x2, y1, y2;
            x1 = x2 = y1 = y2 = 0;
            double WidthReciprocal = 1.0 / (endTime - startTime);
            double HeightReciprocal = 1.0 / (gain * (2 * numberOfDisplayedChannels + 1));
            //iterate through all channels that are toggled to be displayed.
            int[] displayedChannelIndices = new int[numberOfDisplayedChannels];
            for (int i = 0; i < numberOfDisplayedChannels; i++)
            {
                int channelIndex = 0;
                int displayedChannelCounter = 0;
                while (displayedChannelCounter <= i)
                {
                    if (display[channelIndex])
                    {
                        displayedChannelIndices[displayedChannelCounter] = channelIndex;
                        displayedChannelCounter++;
                    }
                    channelIndex++;
                }
                channelIndex--;

                //draw for each frame of the eeg, even if they have to squish 
                //into the width of the bitmap's pixels
                double samplesPerSecond = (double)m_header.SignalSamplesPerRecord(channelIndex) / m_header.RecordDuration;
                double sampleNumberD = startTime * samplesPerSecond;
                if (Math.Floor(sampleNumberD) != sampleNumberD)
                {
                    sampleNumberD = Math.Floor(sampleNumberD) + 1;
                }
                long sampleNumber = (long)sampleNumberD;

                Pen p = new Pen(GetColor(channelIndex));
                double newGain = gain;
                double heightScalar;
                if (newGain == 0)
                {
                    heightScalar = (double)picToModify.Height
                        // / (maxValues[channelIndex]-minValues[channelIndex])
                        / (double)numberOfDisplayedChannels;
                    newGain = 1;
                }
                else
                {
                    heightScalar = (double)picToModify.Height * gain
                        / (m_header.SignalDigitalMax(channelIndex) - m_header.SignalDigitalMin(channelIndex))
                        / (double)numberOfDisplayedChannels;
                }

                double channelY = (0.5 + i) * picToModify.Height / (numberOfDisplayedChannels + 1);
                //double channelY = (0.5 + (double)channelIndex) * picToModify.Height / (numberOfDisplayedChannels + 1);
                double xDenom = 1.0 / (double)samples[channelIndex].Length;
                int sampleIncrement = 1;
                if (!m_drawModeAntiAlias)
                {
                    sampleIncrement = (int)(0.5 * (double)m_readFunctionCount[channelIndex] / (double)picToModify.Width);
                    if (sampleIncrement < 1) { sampleIncrement = 1; }
                }

                //debug
                int temp4 = m_readFunctionCount[channelIndex];
                int temp5 = samples[channelIndex].Length;
                //end debug
                int endIndex = m_readFunctionCount[i] + m_readFunctionOffset[i];
                for (long j = m_readFunctionOffset[channelIndex]; j < samples[i].Length && j < endIndex; j += sampleIncrement)
                {
                    //double sampleTime = sampleNumber / samplesPerSecond;
                    double sampleTime = j / samplesPerSecond;
                    //set the x value for the next segment of the curve
                    //x2 = (float)((sampleTime - startTime) * WidthReciprocal * picToModify.Width);
                    x2 = (float)(sampleTime * WidthReciprocal * picToModify.Width);
                    //set the y value for the next segment of the 
                    if (gain == 0)
                    {
                        double temp = (samples[channelIndex][j] - m_autoGainMidRanges[channelIndex]) * m_autoGainRangeReciprocals[channelIndex];
                        temp *= heightScalar;
                        temp *= newGain;
                        temp += channelY;
                        y2 = (float)temp;
                        //y2 = (float)(((bytes[channelIndex][j] - middle) * heightScalar * newGain + channelY));
                    }
                    else
                    {
                        y2 = (float)((samples[channelIndex][j] * heightScalar * newGain + channelY));
                    }
                    if (y2 > picToModify.Height) y2 = picToModify.Height - 1;
                    if (y2 < 0) y2 = 0;
                    if (j != 0          //need two points.  there's only one when j==0
                        && x1 <= x2)    //prevents erroneous connections between channels in the raw tracing
                    {
                        //draw the line.
                        g.DrawLine(p, x1, y1, x2, y2);

                        //debug
                        linesDrawn++;
                        //end debug

                    }
                    //move new to old.
                    x1 = x2;
                    y1 = y2;
                    sampleNumber++;
                }
                float fontSize = (float)picToModify.Height / (float)(numberOfDisplayedChannels * 3);
                g.DrawString(m_header.SignalLabel(channelIndex),
                    new Font("Arial", fontSize),
                    new SolidBrush(Color.Black), 10,
                    (float)(channelY));
            }
            #endregion draw signals
            //debug
            splits.Add(stopWatch.ElapsedMilliseconds);
            //end debug

            //debug
            stopWatch.Stop();
            long milliSeconds = stopWatch.ElapsedMilliseconds;
            if (milliSeconds > 0)
            {
                double linesPerMillisecond = linesDrawn / milliSeconds;
            }
            //end debug
            return picToModify;
        }
        protected Color GetColor(int i)
        {
            //if (!IsMonochrome)
            {
                i %= 20;
                switch (i)
                {
                    case 0:
                        return Color.Black;
                    case 1:
                        return Color.Red;
                    case 2:
                        return Color.Blue;
                    case 3:
                        return Color.Green;
                    case 4:
                        return Color.DarkGoldenrod;
                    case 5:
                        return Color.DarkMagenta;
                    case 6:
                        return Color.DarkSeaGreen;
                    case 7:
                        return Color.DarkViolet;
                    case 8:
                        return Color.DeepPink;
                    case 9:
                        return Color.DarkSalmon;
                    case 10:
                        return Color.DarkRed;
                    case 11:
                        return Color.DarkOrange;
                    case 12:
                        return Color.DarkKhaki;
                    case 13:
                        return Color.DarkGreen;
                    case 14:
                        return Color.DarkCyan;
                    case 15:
                        return Color.Crimson;
                    case 16:
                        return Color.DarkGray;
                    case 17:
                        return Color.DeepSkyBlue;
                    case 18:
                        return Color.BurlyWood;
                    case 19:
                        return Color.MidnightBlue;
                }
                return Color.Black;
            }
            //else
            {
                return Color.Black;
            }
        }
        #endregion drawing



    }
}
