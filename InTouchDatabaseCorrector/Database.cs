using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTouchDatabaseCorrector
{
    class Database
    {

        private List<DatabaseSection> _sections;
        private List<string> _topics;

        
        public Database(List<DatabaseSection> sections, List<string> topics)
        {
            _sections = sections;
            _topics = topics;
        }

        /// <summary>
        /// Validate the database
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            bool ret = true;

            foreach (var section in _sections)
            {
                ret = section.Validate(false, _topics) & ret;
            }

            return ret;
        }

        /// <summary>
        /// Attempt to repair the CSV file
        /// </summary>
        public void Repair()
        {
            foreach (var section in _sections)
            {
                section.Validate(true, _topics);
            }
        }

        /// <summary>
        /// Write the database back out to a CSV file
        /// </summary>
        /// <param name="file"></param>
        public void Write(string file)
        {
            TextWriter stream = File.CreateText(file);

            foreach (var section in _sections)
            {
                section.Write(stream);
            }

            stream.Close();
        }

        public void ConverAccessNamesToArchestra(Dictionary<string, string> TopicMap, string DefaultObject = "")
        {
            foreach (var section in _sections)
            {
                section.ConverAccessNamesToArchestra(TopicMap,DefaultObject);
            }
        }

        public static Database CreateFromCSV(string file)
        {
            List<DatabaseSection> sections = new List<DatabaseSection>();
            List<string> topics = new List<string>();

            string[] lines = File.ReadAllLines(file);
            int startLine = -1;
            bool access = false;

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].StartsWith(":"))
                {
                    if (access)
                    {
                        string[] copyLines = new string[i-startLine];
                        Array.Copy(lines, startLine, copyLines, 0, i-startLine);
                        topics = GenerateTopic(copyLines);
                        access = false;
                    }
                    if (lines[i].StartsWith(":IOAccess"))
                    {
                        access = true;
                    }
                    

                    if (startLine >= 0)
                    {
                        string[] copyLines = new string[i-startLine];
                        Array.Copy(lines, startLine, copyLines, 0, i-startLine);
                        DatabaseSection db = new DatabaseSection(copyLines);
                        sections.Add(db);
                        startLine = i;

                    }
                    else
                    {
                        startLine = i;
                    }
                    
                }
            }

            return new Database(sections, topics);
        }

        private static List<string> GenerateTopic(string[] copyLines)
        {
            List<string> topics = new List<string>();

            foreach (var line in copyLines)
            {
                topics.Add(line.Split(',')[0]);
            }

            return topics;
        }
    }
}
