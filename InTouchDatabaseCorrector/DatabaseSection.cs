using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTouchDatabaseCorrector
{
    class DatabaseSection
    {
        private string[][] data;

        public DatabaseSection(string[] lines)
        {
            int numCols = lines[0].Split(',').Count();
            int numRows = lines.Count();
            data = new string[numRows][];

            for(int i = 0; i < numRows; ++i)
            {
                string[] lineData = lines[i].Split(',');
                data[i] = lineData;
            }
        }

        internal void Write(TextWriter stream)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                stream.Write(string.Join(",", data[i]) + Environment.NewLine) ;
            }
            
        }

        public bool Validate(bool repair, List<string> topics)
        {
            bool valid = true;
            int accessNameIndex, maxEUIndex, minEUIndex, hiAlarmOnIndex, hihiAlarmOnIndex, hiAlarmValueIndex, hihiAlarmValueIndex, loAlarmOnIndex, loloAlarmOnIndex, loAlarmValueIndex, loloAlarmValueIndex, initialValueIndex, alarmMinorDevIndex;
            switch (data[0][0])
            {
               
                case ":IODisc":
                case ":IOMsg":
                    accessNameIndex = Array.IndexOf(data[0], "AccessName");
                    for (int i = 1; i < data.Length; ++i)
                    {
                        if (!topics.Contains(data[i][accessNameIndex]))
                        {
                            valid = false;
                            string temp = data[i][accessNameIndex];
                            string[] temp2 = data[i];
                            Console.WriteLine("Deleting " + data[i][0] + " for having an invalid Access Name");
                            if (repair)
                            {
                                data = data.Where((val, idx) => idx != i).ToArray();
                                i--;
                                
                            }
                        }
                    }
                    break;
                case ":IOReal":
                case ":IOInt":
                case ":MemoryReal":
                    accessNameIndex = Array.IndexOf(data[0], "AccessName");
                    initialValueIndex = Array.IndexOf(data[0], "InitialValue");

                    if (data[0][0] == ":MemoryReal")
                    {
                        maxEUIndex = Array.IndexOf(data[0], "MaxValue");
                        minEUIndex = Array.IndexOf(data[0], "MinValue");
                    }
                    else
                    {
                        maxEUIndex = Array.IndexOf(data[0], "MaxEU");
                        minEUIndex = Array.IndexOf(data[0], "MinEU");
                    }
                    hiAlarmOnIndex = Array.IndexOf(data[0], "HiAlarmState");
                    hihiAlarmOnIndex = Array.IndexOf(data[0], "HiHiAlarmState");
                    hiAlarmValueIndex = Array.IndexOf(data[0], "HiAlarmValue");
                    hihiAlarmValueIndex = Array.IndexOf(data[0], "HiHiAlarmValue");

                    loAlarmOnIndex = Array.IndexOf(data[0], "LoAlarmState");
                    loloAlarmOnIndex = Array.IndexOf(data[0], "LoLoAlarmState");
                    loAlarmValueIndex = Array.IndexOf(data[0], "LoAlarmValue");
                    loloAlarmValueIndex = Array.IndexOf(data[0], "LoLoAlarmValue");
                    alarmMinorDevIndex = Array.IndexOf(data[0], "MinorDevAlarmValue");

                    for (int i = 1; i < data.Length; ++i)
                    {
                        //topic test
                        if (data[0][0] != ":MemoryReal")
                        {
                            if (!topics.Contains(data[i][accessNameIndex]))
                            {
                                valid = false;
                                Console.WriteLine("Deleting " + data[i][0] + " for having an invalid Access Name");
                                if (repair)
                                {
                                    data = data.Where((val, idx) => idx != i).ToArray();
                                    i--;
                                }
                            }
                        }
                        //initial value tests
                        if (double.Parse(data[i][initialValueIndex]) > double.Parse(data[i][maxEUIndex]))
                        {
                            valid = false;
                            Console.WriteLine("Setting " + data[i][0] + " IV to max EU (too high)");
                            if (repair)
                            {
                                data[i][initialValueIndex] = data[i][maxEUIndex];
                            }
                        }
                        if (double.Parse(data[i][initialValueIndex]) < double.Parse(data[i][minEUIndex]))
                        {
                            valid = false;
                            Console.WriteLine("Setting " + data[i][0] + " IV to min EU (too low)");
                            if (repair)
                            {
                                data[i][initialValueIndex] = data[i][minEUIndex];
                            }
                        }
                        double iv = double.Parse(data[i][initialValueIndex]);
                        if (iv > 0 && iv < float.Epsilon * 100)
                        {
                            valid = false;
                            Console.WriteLine("Setting " + data[i][0] + " IV to 0 (smaller than epsilon)");
                            if (repair)
                            {
                                data[i][initialValueIndex] = "0";
                            }
                        }
                        if (iv < 0 && iv > -float.Epsilon * 100)
                        {
                            valid = false;
                            Console.WriteLine("Setting " + data[i][0] + " IV to 0 (smaller than epsilon)");
                            if (repair)
                            {
                                data[i][initialValueIndex] = "0";
                            }
                        }
                        //minor dev alarm list
                        iv = double.Parse(data[i][alarmMinorDevIndex]);
                        if (iv > 0 && iv < float.Epsilon)
                        {
                            valid = false;
                            Console.WriteLine("Setting " + data[i][0] + " Minor Alarm Dev to 0 (smaller than epsilon)");
                            if (repair)
                            {
                                data[i][alarmMinorDevIndex] = "0";
                            }
                        }
                        if (iv < 0 && iv > -float.Epsilon)
                        {
                            valid = false;
                            Console.WriteLine("Setting " + data[i][0] + " Minor Alarm Dev to 0 (smaller than epsilon)");
                            if (repair)
                            {
                                data[i][alarmMinorDevIndex] = "0";
                            }
                        }
                        //hihi alarm test
                        if (data[i][hihiAlarmOnIndex] == "On")
                        {
                            //check not above min eu
                            valid &= DisableIfAboveMaxEU(repair, ref data[i], hihiAlarmOnIndex, hihiAlarmValueIndex, maxEUIndex, "HiHi above Max EU");
                            valid &= DisableIfBelowMinEU(repair, ref data[i], hihiAlarmOnIndex, hihiAlarmValueIndex, minEUIndex, "HiHi below Min EU");

                            if (data[i][hiAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfBelow(repair, ref data[i], hihiAlarmOnIndex, hihiAlarmValueIndex, hiAlarmValueIndex, "HiHi below Hi");
                            }
                            else if (data[i][loAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfBelow(repair, ref data[i], hihiAlarmOnIndex, hihiAlarmValueIndex, loAlarmValueIndex, "HiHi below Lo");
                            }
                            else if (data[i][loloAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfBelow(repair, ref data[i], hihiAlarmOnIndex, hihiAlarmValueIndex, loloAlarmValueIndex, "HiHi below LoLo");
                            }
                            
                        }
                        //hi alarm test
                        if (data[i][hiAlarmOnIndex] == "On")
                        {
                           
                            valid &= DisableIfAboveMaxEU(repair, ref data[i], hiAlarmOnIndex, hiAlarmValueIndex, maxEUIndex, "Hi above Max EU");
                            valid &= DisableIfBelowMinEU(repair, ref data[i], hiAlarmOnIndex, hiAlarmValueIndex, minEUIndex, "Hi below Min EU");

                            if (data[i][hihiAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfAbove(repair, ref data[i], hiAlarmOnIndex, hiAlarmValueIndex, hihiAlarmValueIndex, "Hi above Hi");
                            }
                            if (data[i][loAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfBelow(repair, ref data[i], hiAlarmOnIndex, hiAlarmValueIndex, loAlarmValueIndex, "Hi below Lo");
                            }
                            else if (data[i][loloAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfBelow(repair, ref data[i], hiAlarmOnIndex, hiAlarmValueIndex, loloAlarmValueIndex, "Hi below LoLo");
                            }

                        }
                        //lo alarm tests
                        if (data[i][hihiAlarmOnIndex] == "On")
                        {
                            valid &= DisableIfAboveMaxEU(repair, ref data[i], loAlarmOnIndex, loAlarmValueIndex, maxEUIndex, "Lo above Max EU");
                            valid &= DisableIfBelowMinEU(repair, ref data[i], loAlarmOnIndex, loAlarmValueIndex, minEUIndex, "Lo below Min EU");

                            if (data[i][hiAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfAbove(repair, ref data[i], loAlarmOnIndex, loAlarmValueIndex, hiAlarmValueIndex, "Lo above Hi");
                            }
                            else if (data[i][hihiAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfAbove(repair, ref data[i], loAlarmOnIndex, loAlarmValueIndex, hihiAlarmValueIndex, "Lo above HiHi");
                            }
                            if (data[i][loloAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfBelow(repair, ref data[i], loAlarmOnIndex, loAlarmValueIndex, loloAlarmValueIndex, "Lo below LoLo");
                            }

                        }
                        //lolo alarm tests
                        if (data[i][hihiAlarmOnIndex] == "On")
                        {
                            //check not above min eu
                            valid &= DisableIfAboveMaxEU(repair, ref data[i], loloAlarmOnIndex, loloAlarmValueIndex, maxEUIndex, "LoLo above Max EU");
                            valid &= DisableIfBelowMinEU(repair, ref data[i], loloAlarmOnIndex, loloAlarmValueIndex, minEUIndex, "LoLo below Min EU");

                            if (data[i][loAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfAbove(repair, ref data[i], loloAlarmOnIndex, loloAlarmValueIndex, loAlarmValueIndex, "LoLo above Lo");
                            }
                            else if (data[i][hiAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfAbove(repair, ref data[i], loloAlarmOnIndex, loloAlarmValueIndex, hiAlarmValueIndex, "LoLo above Hi");
                            }
                            else if (data[i][hihiAlarmOnIndex] == "On")
                            {
                                valid &= DisableIfAbove(repair, ref data[i], loloAlarmOnIndex, loloAlarmValueIndex, hihiAlarmValueIndex, "LoLo above HiHi");
                            }
                            

                        }
                    }
                    break;
                case ":MemoryInt":
                case ":MemoryMsg":
                case ":MemoryDisc":
                    break;
                default:

                    break;
            }

            return valid;
        }

        internal void ConverAccessNamesToArchestra(Dictionary<string, string> topicMap, string defaultObject)
        {
            int accessNameIndex, itemUseTagnameIndex, itemNameIndex;
            switch (data[0][0])
            {
                case ":IODisc":
                case ":IOMsg":
                case ":IOReal":
                case ":IOInt":
                    accessNameIndex = Array.IndexOf(data[0], "AccessName");
                    itemUseTagnameIndex = Array.IndexOf(data[0], "ItemUseTagname");
                    itemNameIndex = Array.IndexOf(data[0], "ItemName");
                    for (int i = 1; i < data.Length; ++i)
                    {
                        if (topicMap.ContainsKey(data[i][accessNameIndex]))
                        {
                            Console.WriteLine("Updating " + data[i][0] + " access name/item name from topicmap");                           
                            data[i][itemNameIndex] = topicMap[data[i][accessNameIndex]] + "." + data[i][itemNameIndex];
                            data[i][accessNameIndex] = "Galaxy";
                        } else if (defaultObject != "")
                        {
                            Console.WriteLine("Updating " + data[i][0] + " access name/item name using default");
                            data[i][itemNameIndex] = defaultObject + "." + data[i][itemNameIndex];
                            data[i][accessNameIndex] = "Galaxy";
                        }
                    }
                    break;
                default:
                    break;
            }
    }

        public bool DisableIfAboveMaxEU(bool repair, ref string[] theData, int disableIndex, int index, int maxEUIndex, string error)
        {
            
            if (double.Parse(theData[index]) > double.Parse(theData[maxEUIndex]))
            {
                Console.WriteLine("Disabling " + theData[0] + " (" + error + ")");
                if (repair)
                {
                    theData[disableIndex] = "Off";
                }
                return false;
            }
            return true;
        }

        public bool DisableIfBelowMinEU(bool repair, ref string[] theData, int disableIndex, int index, int minEUIndex, string error)
        {
            if (double.Parse(theData[index]) < double.Parse(theData[minEUIndex]))
            {
                Console.WriteLine("Disabling " + theData[0] + " (" + error + ")");
                if (repair)
                {
                    theData[disableIndex] = "Off";
                }
                return false;
            }
            return true;
        }
        public bool DisableIfAbove(bool repair, ref string[] theData, int disableIndex, int index, int maxEUIndex, string error)
        {

            if (double.Parse(theData[index]) >= double.Parse(theData[maxEUIndex]))
            {
                Console.WriteLine("Disabling " + theData[0] + " (" + error + ")");
                if (repair)
                {
                    theData[disableIndex] = "Off";
                }
                return false;
            }
            return true;
        }

        public bool DisableIfBelow(bool repair, ref string[] theData, int disableIndex, int index, int minEUIndex, string error)
        {
            if (double.Parse(theData[index]) <= double.Parse(theData[minEUIndex]))
            {
                Console.WriteLine("Disabling " + theData[0] + " (" + error + ")");
                if (repair)
                {
                    theData[disableIndex] = "Off";
                }
                return false;
            }
            return true;
        }
    }
}
