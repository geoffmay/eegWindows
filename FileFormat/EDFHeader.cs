using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Analysis
{
    /// <summary>
    /// Written by Geoff May
    /// A class capable of reading and writing the header of an EDF+ file and exposes the relevant data.
    /// Compliant with standards published on edfplus.info
    /// </summary>
    public class EDFHeader
    {
        #region fields
        #region specified in file
        //8 ascii : version of this data format (0) 
        private string m_version;
        //80 ascii : local patient identification (mind item 3 of the additional EDF+ specs)
        //The 'local patient identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
        //- the code by which the patient is known in the hospital administration. 
        //- sex (English, so F or M). 
        //- birthdate in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 02-AUG-1951 is OK, while 2-AUG-1951 is not. 
        //- the patients name. 
        //Any space inside the hospital code or the name of the patient must be replaced by a different character, for instance an underscore. 
        //For instance, the 'local patient identification' field could start with: MCH-0234567 F 02-MAY-1951 Haagse_Harry. 
        //Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. 
        //Additional subfields may follow the ones described here. 
        private string m_patient_info;
        private string m_PI_hospitalPatientIdentifier;
        private string m_PI_sex;
        private DateTime m_PI_birthdate;
        private string m_PI_name;
        private bool m_PI_successful_parse;
        //80 ascii : local recording identification (mind item 4 of the additional EDF+ specs)
        //The 'local recording identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
        //- The text 'Startdate'. 
        //- The startdate itself in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 
        //- The hospital administration code of the investigation, i.e. EEG number or PSG number. 
        //- A code specifying the responsible investigator or technician. 
        //- A code specifying the used equipment. 
        //Any space inside any of these codes must be replaced by a different character, for instance an underscore. 
        //The 'local recording identification' field could contain: Startdate 02-MAR-2002 PSG-1234/2002 NN Telemetry03. 
        //Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. 
        //So, if everything is unknown then the 'local recording identification' field would start with: Startdate X X X X. 
        //Additional subfields may follow the ones described here. 
        private string m_recording_info;
        //private string m_RI_startDate;
        private DateTime m_RI_startDate;
        private string m_RI_hospitalEEGNumber;
        private string m_RI_technician;
        private string m_RI_equipment;
        private bool m_RI_successful_parse;
        //8 ascii : startdate of recording (dd.mm.yy) (mind item 2 of the additional EDF+ specs)
        //The 'startdate' and 'starttime' fields in the header should contain only characters 0-9, and the period (.) as a separator, 
        //for example "02.08.51". In the 'startdate', use 1985 as a clipping date in order to avoid the Y2K problem. 
        //So, the years 1985-1999 must be represented by yy=85-99 and the years 2000-2084 by yy=00-84. 
        //After 2084, yy must be 'yy' and only m_patient_info defines the date. 
        private string m_record_start_date;
        //8 ascii : starttime of recording (hh.mm.ss) 
        private string m_record_start_time;
        //8 ascii : number of bytes in header record 
        //private string m_bytes_in_header;
        private int m_bytes_in_header;
        //44 ascii : reserved 
        private string m_reserved;
        //8 ascii : number of data records (-1 if unknown, obey item 10 of the additional EDF+ specs) 
        // The 'number of data records' can only be -1 during recording. As soon as the file is closed, the correct number is known and must be entered. 
        //private string m_number_of_records;
        private int m_number_of_records;
        //8 ascii : duration of a data record, in seconds 
        //private string m_duration;
        private double m_record_duration;
        //4 ascii : number of signals (ns) in data record 
        //private string m_number_of_signals;
        private int m_number_of_signals;
        //ns * 16 ascii : ns * label (e.g. EEG Fpz-Cz or Body temp) (mind item 9 of the additional EDF+ specs)
        private string[] m_signal_labels;
        //ns * 80 ascii : ns * transducer type (e.g. AgAgCl electrode) 
        private string[] m_signal_transducer_types;
        //ns * 8 ascii : ns * physical dimension (e.g. uV or degreeC) 
        private string[] m_signal_dimension_units;
        //ns * 8 ascii : ns * physical minimum (e.g. -500 or 34)
        //private string[] m_signal_physical_minima;
        private double[] m_signal_physical_minima;
        //ns * 8 ascii : ns * physical maximum (e.g. 500 or 40) 
        //private string[] m_signal_physical_maxima;
        private double[] m_signal_physical_maxima;
        //ns * 8 ascii : ns * digital minimum (e.g. -2048) 
        //private string[] m_signal_digital_minima;
        private double[] m_signal_digital_minima;
        //ns * 8 ascii : ns * digital maximum (e.g. 2047) 
        //private string[] m_signal_digital_maxima;
        private double[] m_signal_digital_maxima;
        //ns * 80 ascii : ns * prefiltering (e.g. HP:0.1Hz LP:75Hz) 
        private string[] m_signal_prefilterings;
        //ns * 8 ascii : ns * nr of samples in each data record 
        //private string[] m_signal_samples_per_second;
        private double[] m_signal_samples_per_record;
        //ns * 32 ascii : ns * reserved
        private string[] m_signal_reserved;
        //1. In the header, use only printable US-ASCII characters with byte values 32..126. 
        //2. The 'startdate' and 'starttime' fields in the header should contain only characters 0-9, and the period (.) as a separator, for example "02.08.51". In the 'startdate', use 1985 as a clipping date in order to avoid the Y2K problem. So, the years 1985-1999 must be represented by yy=85-99 and the years 2000-2084 by yy=00-84. After 2084, yy must be 'yy' and only item 4 of this paragraph defines the date. 
        //3. The 'local patient identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
        //- the code by which the patient is known in the hospital administration. 
        //- sex (English, so F or M). 
        //- birthdate in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 02-AUG-1951 is OK, while 2-AUG-1951 is not. 
        //- the patients name. 
        //Any space inside the hospital code or the name of the patient must be replaced by a different character, for instance an underscore. For instance, the 'local patient identification' field could start with: MCH-0234567 F 02-MAY-1951 Haagse_Harry. Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. Additional subfields may follow the ones described here. 
        //4. The 'local recording identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
        //- The text 'Startdate'. 
        //- The startdate itself in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 
        //- The hospital administration code of the investigation, i.e. EEG number or PSG number. 
        //- A code specifying the responsible investigator or technician. 
        //- A code specifying the used equipment. 
        //Any space inside any of these codes must be replaced by a different character, for instance an underscore. The 'local recording identification' field could contain: Startdate 02-MAR-2002 PSG-1234/2002 NN Telemetry03. Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. So, if everything is unknown then the 'local recording identification' field would start with: Startdate X X X X. Additional subfields may follow the ones described here. 
        //5. 'Digital maximum' must be larger than 'Digital minimum'. In case of a negative amplifier gain the corresponding 'Physical maximum' is smaller than the 'Physical minimum'. Check item 9 on how to apply the 'negativity upward' rule in Clinical Neurophysiology to the physical ordinary signal. 'Physical maximum' must differ from 'Physical minimum'. In case of uncalibrated signals, physical dimension is left empty (that is 8 spaces), while 'Physical maximum' and 'Physical minimum' must still contain different values (this is to avoid 'division by 0' errors by some viewers). 
        //6.  Never use any digit grouping symbol in numbers. Never use a comma "," for a for a decimal separator. When a decimal separator is required, use a dot ("."). 
        //7. The ordinary signal samples (2-byte two's complement integers) must be stored in 'little-endian' format, that is the least significant byte first. This is the default format in PC applications.
        //8. The 'starttime' should be local time at the patients location when the recording was started. 
        //9. Use the standard texts and polarity rules at http://www.edfplus.info/specs/edftexts.html. These standard texts may in the future be extended with further texts, a.o. for Sleep scorings, ENG and various evoked potentials. 
        //10. The 'number of data records' can only be -1 during recording. As soon as the file is closed, the correct number is known and must be entered. 
        //11. If filters (such as HighPass, LowPass or Notch) were applied to the ordinary signals then, preferably automatically, specify them like "HP:0.1Hz LP:75Hz N:50Hz" in the "prefiltering" field of the header. If the file contains an analysis result, the prefiltering field should mention the relevant analysis parameters. 
        //12. The "transducertype" field should specify the applied sensor, such as "AgAgCl electrode" or "thermistor". 
        #endregion specified in file
        #region dervied
        public int bytesPerSample = 2;
        #endregion derived
        #endregion //fields
        public enum SampleStyle { Continuous, Discontinuous, Biosemi24Bit, Unspecified };

        private SampleStyle m_sampleStyle;
        public SampleStyle sampleStyle
        {
            get
            {
                return m_sampleStyle;
            }
            set
            {
                m_sampleStyle = value;
                if (value == SampleStyle.Biosemi24Bit)
                {
                    bytesPerSample = 3;
                }
                else
                {
                    bytesPerSample = 2;
                }
            }
        }
        public EDFHeader()
        {
            m_version = "0";
            m_patient_info = "";
            m_PI_hospitalPatientIdentifier = "";
            m_PI_sex = "";
            m_PI_birthdate = new DateTime();
            m_PI_name = "";
            m_recording_info = "";
            m_RI_startDate = new DateTime();
            m_RI_hospitalEEGNumber = "";
            m_RI_technician = "";
            m_RI_equipment = "";
            m_record_start_date = "";
            m_record_start_time = "";
            m_bytes_in_header = 256;
            m_reserved = "";
            m_number_of_records = 0;
            m_record_duration = 1;
            m_number_of_signals = 0;
            m_signal_digital_maxima = new double[0];
            m_signal_digital_minima = new double[0];
            m_signal_dimension_units = new string[0];
            m_signal_labels = new string[0];
            m_signal_physical_maxima = new double[0];
            m_signal_physical_minima = new double[0];
            m_signal_prefilterings = new string[0];
            m_signal_reserved = new string[0];
            m_signal_samples_per_record = new double[0];
            m_signal_transducer_types = new string[0];
        }

        #region set
        public void AddSignal(string label,
            string transducerType,
            string dimenstionUnit,
            double physicalMinimum,
            double physicalMaximum,
            double digitalMinimum,
            double dignitalMaximum,
            string preFilter,
            double samplesPerRecord,
            string reserved)
        {
            {
                List<string> filters = new List<string>(m_signal_prefilterings);
                filters.Add(preFilter);
                m_signal_prefilterings = filters.ToArray();
            }
            {
                List<double> hertzes = new List<double>(m_signal_samples_per_record);
                hertzes.Add(samplesPerRecord);
                m_signal_samples_per_record = hertzes.ToArray();
            }
            {
                List<string> reserveds = new List<string>(m_signal_reserved);
                reserveds.Add(reserved);
                m_signal_reserved = reserveds.ToArray();
            }



            {
                List<double> physMax = new List<double>(m_signal_physical_maxima);
                physMax.Add(physicalMaximum);
                m_signal_physical_maxima = physMax.ToArray();
            }
            {
                List<double> digMin = new List<double>(m_signal_digital_minima);
                digMin.Add(digitalMinimum);
                m_signal_digital_minima = digMin.ToArray();
            }
            {
                List<double> digMax = new List<double>(m_signal_digital_maxima);
                digMax.Add(dignitalMaximum);
                m_signal_digital_maxima = digMax.ToArray();
            }

            {
                List<string> labels = new List<string>(m_signal_labels);
                labels.Add(label);
                m_signal_labels = labels.ToArray();
            }
            {
                List<string> transducers = new List<string>(m_signal_transducer_types);
                transducers.Add(transducerType);
                m_signal_transducer_types = transducers.ToArray();
            }
            {
                List<string> dimesnions = new List<string>(m_signal_dimension_units);
                dimesnions.Add(dimenstionUnit);
                m_signal_dimension_units = dimesnions.ToArray();
            }
            {
                List<double> physMin = new List<double>(m_signal_physical_minima);
                physMin.Add(physicalMinimum);
                m_signal_physical_minima = physMin.ToArray();
            }
            m_bytes_in_header += 256;
            m_number_of_signals += 1;
        }
        public void AddAnnotationSignal(
            double samplesPerRecord)
        {
            {
                List<string> filters = new List<string>(m_signal_prefilterings);
                filters.Add("EDF Annotations");
                m_signal_prefilterings = filters.ToArray();
            }
            {
                List<double> hertzes = new List<double>(m_signal_samples_per_record);
                hertzes.Add(samplesPerRecord);
                m_signal_samples_per_record = hertzes.ToArray();
            }
            {
                List<string> reserveds = new List<string>(m_signal_reserved);
                reserveds.Add("EDF Annotations");
                m_signal_reserved = reserveds.ToArray();
            }



            {
                List<double> physMax = new List<double>(m_signal_physical_maxima);
                physMax.Add(1);
                m_signal_physical_maxima = physMax.ToArray();
            }
            {
                List<double> digMin = new List<double>(m_signal_digital_minima);
                digMin.Add(-32768);
                m_signal_digital_minima = digMin.ToArray();
            }
            {
                List<double> digMax = new List<double>(m_signal_digital_maxima);
                digMax.Add(32767);
                m_signal_digital_maxima = digMax.ToArray();
            }

            {
                List<string> labels = new List<string>(m_signal_labels);
                labels.Add("EDF Annotations");
                m_signal_labels = labels.ToArray();
            }
            {
                List<string> transducers = new List<string>(m_signal_transducer_types);
                transducers.Add("EDF Annotations");
                m_signal_transducer_types = transducers.ToArray();
            }
            {
                List<string> dimesnions = new List<string>(m_signal_dimension_units);
                dimesnions.Add("EDF Annotations");
                m_signal_dimension_units = dimesnions.ToArray();
            }
            {
                List<double> physMin = new List<double>(m_signal_physical_minima);
                physMin.Add(-1);
                m_signal_physical_minima = physMin.ToArray();
            }
            m_bytes_in_header += 256;
            m_number_of_signals += 1;
        }
        public void RemoveSignal(int Index)
        {
            {
                List<string> filters = new List<string>(m_signal_prefilterings);
                filters.RemoveAt(Index);
                m_signal_prefilterings = filters.ToArray();
            }
            {
                List<double> hertzes = new List<double>(m_signal_samples_per_record);
                hertzes.RemoveAt(Index);
                m_signal_samples_per_record = hertzes.ToArray();
            }
            {
                List<string> reserveds = new List<string>(m_signal_reserved);
                reserveds.RemoveAt(Index);
                m_signal_reserved = reserveds.ToArray();
            }
            {
                List<double> physMax = new List<double>(m_signal_physical_maxima);
                physMax.RemoveAt(Index);
                m_signal_physical_maxima = physMax.ToArray();
            }
            {
                List<double> digMin = new List<double>(m_signal_digital_minima);
                digMin.RemoveAt(Index);
                m_signal_digital_minima = digMin.ToArray();
            }
            {
                List<double> digMax = new List<double>(m_signal_digital_maxima);
                digMax.RemoveAt(Index);
                m_signal_digital_maxima = digMax.ToArray();
            }

            {
                List<string> labels = new List<string>(m_signal_labels);
                labels.RemoveAt(Index);
                m_signal_labels = labels.ToArray();
            }
            {
                List<string> transducers = new List<string>(m_signal_transducer_types);
                transducers.RemoveAt(Index);
                m_signal_transducer_types = transducers.ToArray();
            }
            {
                List<string> dimesnions = new List<string>(m_signal_dimension_units);
                dimesnions.RemoveAt(Index);
                m_signal_dimension_units = dimesnions.ToArray();
            }
            {
                List<double> physMin = new List<double>(m_signal_physical_minima);
                physMin.RemoveAt(Index);
                m_signal_physical_minima = physMin.ToArray();
            }
            m_bytes_in_header -= 256;
            m_number_of_signals -= 1;
        }
        public string Version
        {
            get { return m_version; }
            set { m_version = value; }
        }
        public string PatientInfoString
        {
            get { return m_patient_info; }
            set { m_patient_info = value; }
        }
        public string PtInfoHospitalID
        {
            get { return m_PI_hospitalPatientIdentifier; }
            set
            {
                m_PI_hospitalPatientIdentifier = value;
                m_patient_info = PatientInfo;
            }
        }
        public string PtInfoSex
        {
            get { return m_PI_sex; }
            set
            {
                m_PI_sex = value;
                m_patient_info = PatientInfo;
            }
        }
        public DateTime PtInfoBirthdate
        {
            get { return m_PI_birthdate; }
            set
            {
                m_PI_birthdate = value;
                m_patient_info = PatientInfo;
            }
        }

        public string PtInfoName
        {
            get { return m_PI_name; }
            set
            {
                m_PI_name = value;
                m_patient_info = PatientInfo;
            }
        }
        public string RecordingInfoString
        {
            get { return m_recording_info; }
            set { m_recording_info = value; }
        }
        public DateTime RecInfoStartDate
        {
            get { return m_RI_startDate; }
            set
            {
                m_RI_startDate = value;
                m_recording_info = RecordingInfo;
                m_record_start_date =
                    string.Format("{0:00}", value.Day) + "." +
                    string.Format("{0:00}", value.Month) + "." +
                    string.Format("{0:00}", value.Year % 100);
                m_record_start_time =
                    string.Format("{0:00}", value.Hour) + "." +
                    string.Format("{0:00}", value.Minute) + "." +
                    string.Format("{0:00}", value.Second);
            }
        }
        public string RecInfoHospitalEEGNumber
        {
            get { return m_RI_hospitalEEGNumber; }
            set
            {
                m_RI_hospitalEEGNumber = value;
                m_recording_info = RecordingInfo;
            }
        }
        public string RecInfoTechnician
        {
            get { return m_RI_technician; }
            set
            {
                m_RI_technician = value;
                m_recording_info = RecordingInfo;
            }
        }
        public string RecInfoEquipment
        {
            get { return m_RI_equipment; }
            set
            {
                m_RI_equipment = value;
                m_recording_info = RecordingInfo;
            }
        }
        public string RecordStartDate
        {
            get { return m_record_start_date; }
            set { m_record_start_date = value; }
        }
        public string RecordStartTime
        {
            get { return m_record_start_time; }
            set { m_record_start_time = value; }
        }
        public int BytesInHeader
        {
            get { return m_bytes_in_header; }
            set { m_bytes_in_header = value; }
        }
        public string Reserved
        {
            get { return m_reserved; }
            set { m_reserved = value; }
        }
        public int NumberOfRecords
        {
            get { return m_number_of_records; }
            set { m_number_of_records = value; }
        }
        public double RecordDuration
        {
            get { return m_record_duration; }
            set { m_record_duration = value; }
        }
        public int NumberOfSignals
        {
            get { return m_number_of_signals; }
            //set { m_number_of_signals = value; }
        }
        public void SetSignalUnitLabel(int signalNumber, string label)
        {
            m_signal_dimension_units[signalNumber] = label;
        }
        public void SetSignalLocationLabel(int signalNumber, string label)
        {
            m_signal_labels[signalNumber] = label;
        }
        public void SetSignalType(int signalNumber, string label)
        {
            m_signal_transducer_types[signalNumber] = label;
        }
        public void SetSignalSamplesPerSecond(int signalNumber, double samplesPerSecond)
        {
            m_signal_samples_per_record[signalNumber] = samplesPerSecond * m_record_duration;
        }
        #endregion set
        #region get
        public void GetSignalParameters(int index,
            out string label,
            out string transducerType,
            out string dimenstionUnit,
            out double physicalMinimum,
            out double physicalMaximum,
            out double digitalMinimum,
            out double digitalMaximum,
            out string preFilter,
            out double samplesPerRecord,
            out string reserved)
        {
            digitalMaximum = m_signal_digital_maxima[index];
            digitalMinimum = m_signal_digital_minima[index];
            dimenstionUnit = m_signal_dimension_units[index];
            label = m_signal_labels[index];
            physicalMaximum = m_signal_physical_maxima[index];
            physicalMinimum = m_signal_physical_minima[index];
            preFilter = m_signal_prefilterings[index];
            reserved = m_signal_reserved[index];
            samplesPerRecord = m_signal_samples_per_record[index];
            transducerType = m_signal_transducer_types[index];
        }
        public double GetSampleRate(int index)
        {
            return m_signal_samples_per_record[index] * m_record_duration;
        }
        public string GetChannelUnits(int index)
        {
            return m_signal_dimension_units[index];
        }
        public bool IsSignalAnnotation(int index)
        {
            if (m_signal_labels[index].StartsWith("EDF Annotations"))
            {
                return true;
            }
            else return false;
        }

        public long BytesPerRecord
        {
            get
            {
                long retVal = 0;
                for (int i = 0; i < m_signal_samples_per_record.Length; i++)
                {
                    retVal += (long)m_signal_samples_per_record[i];
                }
                return retVal * bytesPerSample;
            }
        }
        public int SignalByteOffsetInRecord(int index)
        {
            int retVal = 0;
            for (int i = 0; i < index; i++)
            {
                retVal += bytesPerSample * (int)(m_signal_samples_per_record[i]);

            }
            return (int)retVal;
        }
        public int SignalSamplesPerRecord(int index)
        {
            return (int)m_signal_samples_per_record[index];
        }
        public string SignalLabel(int index)
        {
            return m_signal_labels[index];
        }
        public string[] SignalLabels
        {
            get
            {
                return m_signal_labels;
            }
        }
        public double SignalDigitalMax(int index)
        {
            return m_signal_digital_maxima[index];
        }
        public double SignalDigitalMin(int index)
        {
            return m_signal_digital_minima[index];
        }
        public double SignalPhysicalMax(int index)
        {
            return m_signal_physical_maxima[index];
        }
        public double SignalPhysicalMin(int index)
        {
            return m_signal_physical_minima[index];
        }
        public void SetMinMax(int signalIndex, double digitalMin, double digitalMax, double physicalMin, double physicalMax)
        {
            m_signal_digital_maxima[signalIndex] = digitalMax;
            m_signal_digital_minima[signalIndex] = digitalMin;
            m_signal_physical_maxima[signalIndex] = physicalMax;
            m_signal_physical_minima[signalIndex] = physicalMin;
        }
        public int HeaderLength
        {
            get
            {
                return 256 + 256 * m_number_of_signals;
            }
        }
        public override string ToString()
        {
            MemoryStream ms = new MemoryStream();
            Write(ms);
            ms.Position = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ms.Length; i++)
            {
                sb.Append((char)ms.ReadByte());
            }
            return sb.ToString();
        }
        public TimeSpan Duration
        {
            get
            {
                double seconds = m_record_duration * m_number_of_records;
                return new TimeSpan(0, 0, 0, (int)seconds, (int)(seconds * 1000 % 1000));
            }
        }
        #endregion get
        #region read functions
        public Stream Parse(Stream input)
        {
            //debug
            long position = input.Position;
            long peekLength = Math.Min(input.Length, 100000);

            byte[] peekBytes = new byte[peekLength];
            int bytesRead = input.Read(peekBytes, 0, peekBytes.Length);
            string peek = Encoding.UTF8.GetString(peekBytes);
            input.Seek(position, SeekOrigin.Begin);
            //end debug

            if (peek.StartsWith("SR_RESEARCH"))
            {
                string[] lines = peek.Split('\n');
                return input;
            }

            //8 ascii : version of this data format (0) 
            m_version = ReadFixedLength(input, 8);

            //80 ascii : local patient identification (mind item 3 of the additional EDF+ specs)
            //The 'local patient identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
            //- the code by which the patient is known in the hospital administration. 
            //- sex (English, so F or M). 
            //- birthdate in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 02-AUG-1951 is OK, while 2-AUG-1951 is not. 
            //- the patients name. 
            //Any space inside the hospital code or the name of the patient must be replaced by a different character, for instance an underscore. 
            //For instance, the 'local patient identification' field could start with: MCH-0234567 F 02-MAY-1951 Haagse_Harry. 
            //Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. 
            //Additional subfields may follow the ones described here. 
            m_patient_info = ReadFixedLength(input, 80);
            string[] patientInfo = m_patient_info.Split(' ');
            m_PI_hospitalPatientIdentifier = patientInfo[0];
            m_PI_sex = patientInfo[1];
            string birthDate = patientInfo[2];
            if (DateTime.TryParse(birthDate, out m_PI_birthdate))
            {
                m_PI_successful_parse = true;
            }
            else
            {
                m_PI_birthdate = DateTime.MaxValue;
                m_PI_successful_parse = false;
            }
            m_PI_name = patientInfo[3];

            //80 ascii : local recording identification (mind item 4 of the additional EDF+ specs)
            //The 'local recording identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
            //- The text 'Startdate'. 
            //- The startdate itself in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 
            //- The hospital administration code of the investigation, i.e. EEG number or PSG number. 
            //- A code specifying the responsible investigator or technician. 
            //- A code specifying the used equipment. 
            //Any space inside any of these codes must be replaced by a different character, for instance an underscore. 
            //The 'local recording identification' field could contain: Startdate 02-MAR-2002 PSG-1234/2002 NN Telemetry03. 
            //Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. 
            //So, if everything is unknown then the 'local recording identification' field would start with: Startdate X X X X. 
            //Additional subfields may follow the ones described here. 
            m_recording_info = ReadFixedLength(input, 80);
            string[] recordingInfo = m_recording_info.Split(' ');
            if (DateTime.TryParse(recordingInfo[1], out m_RI_startDate))
            {
                m_RI_successful_parse = true;
            }
            else
            {
                m_RI_startDate = DateTime.MaxValue;
                m_RI_successful_parse = false;
            }
            //m_RI_startDate = DateTime.Parse(recordingInfo[1]);
            m_RI_hospitalEEGNumber = recordingInfo[2];
            m_RI_technician = recordingInfo[3];
            m_RI_equipment = recordingInfo[4];

            //8 ascii : startdate of recording (dd.mm.yy) (mind item 2 of the additional EDF+ specs)
            //The 'startdate' and 'starttime' fields in the header should contain only characters 0-9, and the period (.) as a separator, 
            //for example "02.08.51". In the 'startdate', use 1985 as a clipping date in order to avoid the Y2K problem. 
            //So, the years 1985-1999 must be represented by yy=85-99 and the years 2000-2084 by yy=00-84. 
            //After 2084, yy must be 'yy' and only m_patient_info defines the date. 
            m_record_start_date = ReadFixedLength(input, 8);
            string[] dateElements = m_record_start_date.Split('.');
            if(dateElements.Length == 3)
            {
                int currentYear = DateTime.Now.Year;
                int year = currentYear / 100 * 100 + int.Parse(dateElements[2]);
                while (year > currentYear) { year -= 100; }
                m_RI_startDate = new DateTime(year, int.Parse(dateElements[1]), int.Parse(dateElements[0]));
            }
            //8 ascii : starttime of recording (hh.mm.ss) 
            m_record_start_time = ReadFixedLength(input, 8);
            string hours = m_record_start_time.Substring(0, 2);
            string mins = m_record_start_time.Substring(3, 2);
            string secs = m_record_start_time.Substring(6, 2);
            m_RI_startDate = m_RI_startDate.AddHours(double.Parse(hours));
            m_RI_startDate = m_RI_startDate.AddMinutes(double.Parse(mins));
            m_RI_startDate = m_RI_startDate.AddSeconds(double.Parse(secs));

            //8 ascii : number of bytes in header record 
            string bytesInHeader = ReadFixedLength(input, 8);
            m_bytes_in_header = int.Parse(bytesInHeader);
            //44 ascii : reserved 
            //The EDF+ header record identifies the patient and specifies the technical characteristics of the recorded signals according to the EDF specs, 
            //except for the first 'reserved' field (44 characters) which must start with 'EDF+C' if the recording is uninterrupted, 
            //thus having contiguous data records, i.e. the starttime of each data record coincides with the end (starttime + duration) 
            //of the preceding one. In this case, the file is EDF compatible and the recording ends (number x duration) seconds after its 
            //startdate/time. The 'reserved' field must start with 'EDF+D' if the recording is interrupted, so not all data records are contiguous. 
            m_reserved = ReadFixedLength(input, 44);
            bytesPerSample = 2;
            if (m_reserved.StartsWith("EDF+C"))
            {
                m_sampleStyle = SampleStyle.Continuous;
            }
            else if (m_reserved.StartsWith("EDF+D"))
            {
                m_sampleStyle = SampleStyle.Discontinuous;
            }
            else if (m_reserved.StartsWith("24BIT"))
            {
                m_sampleStyle = SampleStyle.Biosemi24Bit;
                bytesPerSample = 3;
            }
            else
            {
                m_sampleStyle = SampleStyle.Unspecified;
            }
            //8 ascii : number of data records (-1 if unknown, obey item 10 of the additional EDF+ specs) 
            // The 'number of data records' can only be -1 during recording. As soon as the file is closed, the correct number is known and must be entered. 
            string numberOfRecords = ReadFixedLength(input, 8);
            m_number_of_records = int.Parse(numberOfRecords);
            //8 ascii : duration of a data record, in seconds 
            string Duration = ReadFixedLength(input, 8);
            m_record_duration = double.Parse(Duration);

            //4 ascii : number of signals (ns) in data record 

            string numberOfSignals = ReadFixedLength(input, 4);
            m_number_of_signals = int.Parse(numberOfSignals);

            //ns * 16 ascii : ns * label (e.g. EEG Fpz-Cz or Body temp) (mind item 9 of the additional EDF+ specs)
            m_signal_labels = ReadMultipleFixedLength(input, 16, m_number_of_signals);

            //ns * 80 ascii : ns * transducer type (e.g. AgAgCl electrode) 
            m_signal_transducer_types = ReadMultipleFixedLength(input, 80, m_number_of_signals);

            //ns * 8 ascii : ns * physical dimension (e.g. uV or degreeC) 
            m_signal_dimension_units = ReadMultipleFixedLength(input, 8, m_number_of_signals);

            //ns * 8 ascii : ns * physical minimum (e.g. -500 or 34)
            m_signal_physical_minima = ReadMultipleFixedLengthDoubles(input, 8, m_number_of_signals);

            //ns * 8 ascii : ns * physical maximum (e.g. 500 or 40) 
            m_signal_physical_maxima = ReadMultipleFixedLengthDoubles(input, 8, m_number_of_signals);
            for (int i = 0; i < m_signal_physical_minima.Length; i++)
            {
                if (m_signal_physical_minima[i] == m_signal_physical_maxima[i])
                {
                    throw new ArgumentException("Physical maximum and minimum must be different.");
                }
            }

            //ns * 8 ascii : ns * digital minimum (e.g. -2048) 
            m_signal_digital_minima = ReadMultipleFixedLengthDoubles(input, 8, m_number_of_signals);

            //ns * 8 ascii : ns * digital maximum (e.g. 2047) 
            m_signal_digital_maxima = ReadMultipleFixedLengthDoubles(input, 8, m_number_of_signals);
            for (int i = 0; i < m_signal_digital_maxima.Length; i++)
            {
                if (m_signal_digital_maxima[i] <= m_signal_digital_minima[i])
                {
                    throw new ArgumentException("Digital maximum must be less than digital minimum." +
                        "  If gain is negative, set phyical maximum less than physical minimum.");
                }
            }

            //ns * 80 ascii : ns * prefiltering (e.g. HP:0.1Hz LP:75Hz) 
            m_signal_prefilterings = ReadMultipleFixedLength(input, 80, m_number_of_signals);

            //ns * 8 ascii : ns * nr of samples in each data record 
            m_signal_samples_per_record = ReadMultipleFixedLengthDoubles(input, 8, m_number_of_signals);

            //ns * 32 ascii : ns * reserved
            m_signal_reserved = ReadMultipleFixedLength(input, 32, m_number_of_signals);

            return input;
        }

        #endregion read functions
        #region write functions
        protected string PatientInfo
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(NotNull(m_PI_hospitalPatientIdentifier));
                sb.Append(' ');
                sb.Append(NotNull(m_PI_sex));
                sb.Append(' ');
                sb.Append(m_PI_birthdate.Day.ToString("00"));
                sb.Append('-');
                sb.Append(MonthLetters(m_PI_birthdate.Month));
                sb.Append('-');
                sb.Append(m_PI_birthdate.Year.ToString("00"));
                sb.Append(' ');
                sb.Append(NotNull(m_PI_name));
                return sb.ToString();
            }
        }
        protected string RecordingInfo
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Startdate ");
                sb.Append(m_RI_startDate.Day.ToString("00"));
                sb.Append('-');
                sb.Append(MonthLetters(m_RI_startDate.Month));
                sb.Append('-');
                sb.Append(m_RI_startDate.Year.ToString("0000"));
                sb.Append(' ');
                sb.Append(NotNull(m_RI_hospitalEEGNumber));
                sb.Append(' ');
                sb.Append(NotNull(m_RI_technician));
                sb.Append(' ');
                sb.Append(NotNull(m_RI_equipment));
                return sb.ToString();
            }
        }
        public void NormalizeRecordDuration()
        {
            //choose a record duration such that
            //the sum for each i (signal[i] * samplesPerRecord[i] * bytesPerSample) < 61440
            //So for 124 channels each sampled at 1006 Hz,
            //61440 / (124 * 1006) / 2 = 0.24626434938754569358045276726736
            //the Duration must be smaller than 0.2462643 in order to fit the sizelimit of 61440 bytes.
            //and, it must be expressed as 8 characters.
            #region interval determination
            //find the lcm of the fractional components.
            //every frequency is multiplied by this much to ensure an integer number of samples
            // per record.
            int fractionalLCM = 1;
            for (int i = 0; i < m_signal_samples_per_record.Length; i++)
            {
                fractionalLCM = LeastCommonMultiple(fractionalLCM, ReciprocalOfFraction(m_signal_samples_per_record[i]));
            }
            double DurationInterval;
            if (fractionalLCM > 1)
            {
                //we have to advance an integer number of seconds
                DurationInterval = fractionalLCM;
            }
            else
            {
                //we can advance by a second or less
                int greatestCommonFactor = 1;
                if (m_signal_samples_per_record.Length > 0)
                {
                    greatestCommonFactor = (int)m_signal_samples_per_record[0];
                    for (int i = 1; i < m_signal_samples_per_record.Length; i++)
                    {
                        greatestCommonFactor = GreatestCommonFactor((int)m_signal_samples_per_record[i], greatestCommonFactor);
                    }
                }
                DurationInterval = 1.0 / ((double)(greatestCommonFactor));
            }
            #endregion //interval determination
            double totalSamplesPerSecond = 0;
            for (int i = 0; i < m_signal_samples_per_record.Length; i++)
            {
                totalSamplesPerSecond += m_signal_samples_per_record[i];
            }
            //leave at least 256 bytes per record for annotations
            //double maxDuration = 61440.0 / bytesPerSample / totalSamplesPerSecond;            
            double maxDuration = (61440.0 - 256) / (double)bytesPerSample / (double)totalSamplesPerSecond;
            
            //the number of samples per record must be an integer.
            //duration and samples per second should be chosen to minimize the relative error.
            //This best Duration = 0.221670s. For this Duration, NrSamples = 223.00002.
            //However, in EDF, we must specify that NrSamples is 223.
            //The error is 0.00002 samples per datarecord. The RelativeError = 0.0000000896.
            //So, in a full 24h recording, we have accumulated an error of less than 0.008 seconds.
            //No ADC in this universe has that accuracy.
            double durationWithLeastError = -1;
            double minError = double.MaxValue;
            int DurationCoefficient = 1;
            double Duration = DurationInterval;
            while (DurationInterval * DurationCoefficient < maxDuration)
            {
                Duration = DurationInterval * DurationCoefficient;
                double roundedDuration = 0;
                //count the number of records in the current duration
                double totalError = 0;
//                for (int i = 0; i < m_signal_samples_per_record.Length; i++)
                int i = 0;
                {
                    double totalSamples = m_signal_samples_per_record[i] * Duration;
                    string DurationString = Duration.ToString();
                    if (DurationString.Length > 8)
                    {
                        DurationString = DurationString.Substring(0, 8);
                    }
                    roundedDuration = double.Parse(DurationString);
                    double roundedSamplesPerRecord = m_signal_samples_per_record[i] * roundedDuration;

                    totalError += Math.Abs((totalSamples - roundedSamplesPerRecord) / totalSamples);
                    //totalError += Math.Abs(Duration - roundedDuration);

                }
                //compare to the duration as represented by 8 ascii characters
                //if it's minimized error, it's the new duration
                if (totalError <= minError)
                {
                    minError = totalError;
                    durationWithLeastError = roundedDuration;
                }
                //advance by one "duration"
                DurationCoefficient++;
            }
            double multiplier = durationWithLeastError / m_record_duration;
            for (int i = 0; i < m_signal_samples_per_record.Length; i++)
            {
                m_signal_samples_per_record[i] *= multiplier;
                m_signal_samples_per_record[i] = Math.Floor(m_signal_samples_per_record[i] + 0.5);
            }
            m_record_duration = durationWithLeastError;
        }
        public void Write(Stream output)
        {
            //8 ascii : version of this data format (0) 
            WriteFixedLength(output, m_version, 8);

            //80 ascii : local patient identification (mind item 3 of the additional EDF+ specs)
            //The 'local patient identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
            //- the code by which the patient is known in the hospital administration. 
            //- sex (English, so F or M). 
            //- birthdate in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 02-AUG-1951 is OK, while 2-AUG-1951 is not. 
            //- the patients name. 
            //Any space inside the hospital code or the name of the patient must be replaced by a different character, for instance an underscore. 
            //For instance, the 'local patient identification' field could start with: MCH-0234567 F 02-MAY-1951 Haagse_Harry. 
            //Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. 
            //Additional subfields may follow the ones described here. 

            WriteFixedLength(output, PatientInfo, 80);

            //80 ascii : local recording identification (mind item 4 of the additional EDF+ specs)
            //The 'local recording identification' field must start with the subfields (subfields do not contain, but are separated by, spaces): 
            //- The text 'Startdate'. 
            //- The startdate itself in dd-MMM-yyyy format using the English 3-character abbreviations of the month in capitals. 
            //- The hospital administration code of the investigation, i.e. EEG number or PSG number. 
            //- A code specifying the responsible investigator or technician. 
            //- A code specifying the used equipment. 
            //Any space inside any of these codes must be replaced by a different character, for instance an underscore. 
            //The 'local recording identification' field could contain: Startdate 02-MAR-2002 PSG-1234/2002 NN Telemetry03. 
            //Subfields whose contents are unknown, not applicable or must be made anonymous are replaced by a single character 'X'. 
            //So, if everything is unknown then the 'local recording identification' field would start with: Startdate X X X X. 
            //Additional subfields may follow the ones described here. 

            WriteFixedLength(output, RecordingInfo, 80);

            //8 ascii : startdate of recording (dd.mm.yy) (mind item 2 of the additional EDF+ specs)
            //The 'startdate' and 'starttime' fields in the header should contain only characters 0-9, and the period (.) as a separator, 
            //for example "02.08.51". In the 'startdate', use 1985 as a clipping date in order to avoid the Y2K problem. 
            //So, the years 1985-1999 must be represented by yy=85-99 and the years 2000-2084 by yy=00-84. 
            //After 2084, yy must be 'yy' and only m_patient_info defines the date. 
            StringBuilder startDateBuilder = new StringBuilder();
            startDateBuilder.Append(m_RI_startDate.Day.ToString("00"));
            startDateBuilder.Append('.');
            startDateBuilder.Append(m_RI_startDate.Month.ToString("00"));
            startDateBuilder.Append('.');
            startDateBuilder.Append((m_RI_startDate.Year % 100).ToString("00"));
            WriteFixedLength(output, startDateBuilder.ToString(), 8);

            //8 ascii : starttime of recording (hh.mm.ss) 
            StringBuilder startTimeBuilder = new StringBuilder();
            startTimeBuilder.Append(m_RI_startDate.Hour.ToString("00"));
            startTimeBuilder.Append('.');
            startTimeBuilder.Append(m_RI_startDate.Minute.ToString("00"));
            startTimeBuilder.Append('.');
            startTimeBuilder.Append(m_RI_startDate.Second.ToString("00"));
            WriteFixedLength(output, startTimeBuilder.ToString(), 8);

            //8 ascii : number of bytes in header record 
            WriteFixedLength(output, m_bytes_in_header, 8);

            //44 ascii : reserved 
            if (m_sampleStyle == SampleStyle.Continuous)
            {
                if (!m_reserved.StartsWith("EDF"))
                {
                    m_reserved = "EDF " + m_reserved;
                }
            }
            else if (m_sampleStyle == SampleStyle.Discontinuous)
            {
                if (!m_reserved.StartsWith("EDF+D"))
                {
                    m_reserved = "EDF+D " + m_reserved;
                }
            }
            WriteFixedLength(output, m_reserved, 44);

            //8 ascii : number of data records (-1 if unknown, obey item 10 of the additional EDF+ specs) 
            // The 'number of data records' can only be -1 during recording. As soon as the file is closed, the correct number is known and must be entered. 
            WriteFixedLength(output, m_number_of_records, 8);
            //8 ascii : duration of a data record, in seconds 
            WriteFixedLength(output, m_record_duration, 8);

            //4 ascii : number of signals (ns) in data record 
            WriteFixedLength(output, m_number_of_signals, 4);

            //ns * 16 ascii : ns * label (e.g. EEG Fpz-Cz or Body temp) (mind item 9 of the additional EDF+ specs)
            WriteFixedLength(output, m_signal_labels, 16);

            //ns * 80 ascii : ns * transducer type (e.g. AgAgCl electrode) 
            WriteFixedLength(output, m_signal_transducer_types, 80);

            //ns * 8 ascii : ns * physical dimension (e.g. uV or degreeC) 
            WriteFixedLength(output, m_signal_dimension_units, 8);

            //ns * 8 ascii : ns * physical minimum (e.g. -500 or 34)
            WriteFixedLength(output, m_signal_physical_minima, 8);

            //ns * 8 ascii : ns * physical maximum (e.g. 500 or 40) 
            WriteFixedLength(output, m_signal_physical_maxima, 8);

            //ns * 8 ascii : ns * digital minimum (e.g. -2048) 
            WriteFixedLength(output, m_signal_digital_minima, 8);

            //ns * 8 ascii : ns * digital maximum (e.g. 2047) 
            WriteFixedLength(output, m_signal_digital_maxima, 8);

            //ns * 80 ascii : ns * prefiltering (e.g. HP:0.1Hz LP:75Hz) 
            WriteFixedLength(output, m_signal_prefilterings, 80);

            //ns * 8 ascii : ns * nr of samples in each data record 
            WriteFixedLength(output, m_signal_samples_per_record, 8);

            //ns * 32 ascii : ns * reserved
            WriteFixedLength(output, m_signal_reserved, 32);
        }
        public static bool ContainsIllegalHeaderCharacters(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if ((byte)input[i] < 32 ||
                    (byte)input[i] > 126)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion write functions
        #region utilities
        protected string NotNull(string input)
        {
            if (input == null || input.Equals(""))
            {
                return "X";
            }
            else return input;
        }

        public static int ReciprocalOfFraction(double input)
        {
            int whole = (int)input;
            double remainder = whole - input;
            if (remainder == 0.0)
            {
                return 1;
            }
            else
            {
                return Math.Abs((int)(1.0 / remainder));
            }
        }
        public static int GreatestCommonFactor(int num1, int num2)
        {
            if (num1 == num2)
            {
                return num1;
            }
            //factor each number
            List<int> factors1 = Factor(num1);
            List<int> factors2 = Factor(num2);
            //multiply common factors into product
            int i, j;
            i = j = 0;
            int product = 1;
            while (i < factors1.Count)
            {
                while (j < factors2.Count)
                {
                    if (i < factors1.Count && factors1[i] == factors2[j])
                    {
                        product *= factors2[j];
                        factors1.RemoveAt(i);
                        factors2.RemoveAt(j);
                        j--;
                    }
                    j++;
                }
                j = 0;
                i++;
            }
            return product;
        }
        public static int LeastCommonMultiple(int num1, int num2)
        {
            if (num1 == num2)
            {
                return num1;
            }
            //factor each number
            List<int> factors1 = Factor(num1);
            List<int> factors2 = Factor(num2);
            //cancel out common factors
            int i, j;
            i = j = 0;
            int product = 1;
            while (i < factors1.Count)
            {
                while (j < factors2.Count)
                {
                    if (i < factors1.Count && factors1[i] == factors2[j])
                    {
                        product *= factors2[j];
                        factors1.RemoveAt(i);
                        factors2.RemoveAt(j);
                        j--;
                    }
                    j++;
                }
                j = 0;
                i++;
            }
            //multiply remaining factors

            for (int k = 0; k < factors1.Count; k++)
            {
                product *= factors1[k];
            }
            for (int k = 0; k < factors2.Count; k++)
            {
                product *= factors2[k];
            }
            return product;
        }
        public static List<int> Factor(int product)
        {
            List<int> retVal = new List<int>();
            bool goOn = true;
            while (goOn)
            {
                bool dividedSuccessfully = false;
                int i = 2;
                double max = Math.Sqrt(product);
                while (goOn && !dividedSuccessfully)
                {
                    if (product % i == 0)
                    {
                        retVal.Add(i);
                        product /= i;
                        dividedSuccessfully = true;
                    }
                    else if (i >= max)
                    {
                        retVal.Add(product);
                        goOn = false;
                    }
                    i++;
                }
            }
            return retVal;
        }
        #region banal month conversion
        protected static string MonthLetters(int monthNumber)
        {
            switch (monthNumber)
            {
                case 1:
                    return "JAN";
                    break;
                case 2:
                    return "FEB";
                    break;
                case 3:
                    return "MAR";
                    break;
                case 4:
                    return "APR";
                    break;
                case 5:
                    return "MAY";
                    break;
                case 6:
                    return "JUN";
                    break;
                case 7:
                    return "JUL";
                    break;
                case 8:
                    return "AUG";
                    break;
                case 9:
                    return "SEP";
                    break;
                case 10:
                    return "OCT";
                    break;
                case 11:
                    return "NOV";
                    break;
                case 12:
                    return "DEC";
                    break;
                default:
                    throw new ArgumentException("invalid month number " + monthNumber);
                    break;
            }
        }

        #endregion
        protected static string ReadFixedLength(Stream input, int numberOfCharacters)
        {
            byte[] bytes = new byte[numberOfCharacters];
            input.Read(bytes, 0, numberOfCharacters);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append((char)bytes[i]);
            }
            return sb.ToString();
        }
        protected static string[] ReadMultipleFixedLength(Stream input, int numberOfCharacters, int numberOfReads)
        {
            List<string> retVal = new List<string>();
            for (int i = 0; i < numberOfReads; i++)
            {
                retVal.Add(ReadFixedLength(input, numberOfCharacters));
            }
            return retVal.ToArray();
        }
        protected static double[] ReadMultipleFixedLengthDoubles(Stream input, int numberOfCharacters, int numberOfReads)
        {
            double[] retVal = new double[numberOfReads];
            string[] values = ReadMultipleFixedLength(input, numberOfCharacters, numberOfReads);

            for (int i = 0; i < numberOfReads; i++)
            {
                retVal[i] = double.Parse(values[i]);
            }
            return retVal;
        }
        protected static void WriteFixedLength(Stream output, string valueToWrite, int numberOfCharacters)
        {
            char[] valueChars = valueToWrite.ToCharArray();
            byte[] valueBytes = new byte[valueChars.Length];
            for (int i = 0; i < valueChars.Length; i++)
            {
                valueBytes[i] = (byte)valueChars[i];
            }
            if (valueChars.Length == numberOfCharacters)
            {
                output.Write(valueBytes, 0, numberOfCharacters);
            }
            else if (valueChars.Length < numberOfCharacters)
            {
                output.Write(valueBytes, 0, valueBytes.Length);
                for (int i = valueBytes.Length; i < numberOfCharacters; i++)
                {
                    byte b = (byte)' ';
                    output.WriteByte(b);
                }
            }
            else //(valueChars.Length > numberOfCharacters)
            {
                for (int i = 0; i < numberOfCharacters; i++)
                {
                    output.WriteByte(valueBytes[i]);
                }
            }
        }
        protected static void WriteFixedLength(Stream output, double valueToWrite, int numberOfCharacters)
        {
            WriteFixedLength(output, valueToWrite.ToString(), numberOfCharacters);
        }
        protected static void WriteFixedLength(Stream output, string[] valueToWrite, int numberOfCharacters)
        {
            for (int i = 0; i < valueToWrite.Length; i++)
            {
                WriteFixedLength(output, valueToWrite[i], numberOfCharacters);
            }
        }
        protected static void WriteFixedLength(Stream output, double[] valueToWrite, int numberOfCharacters)
        {
            for (int i = 0; i < valueToWrite.Length; i++)
            {
                WriteFixedLength(output, valueToWrite[i].ToString(), numberOfCharacters);
            }
        }
        #endregion utilities
    }
}
