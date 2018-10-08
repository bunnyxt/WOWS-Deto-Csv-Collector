using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WOWSDetoCsvCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("server=59.110.222.86;User Id=deto;password=WOWSdr2018;Database=wows_detonation;SslMode=None");
                conn.Open();

                MySqlCommand sql;
                MySqlDataReader reader;

                List<Week> weekList = new List<Week>();
                sql = new MySqlCommand("select date from wows_detonation.asia_date where finished = 1;", conn);
                reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    weekList.Add(new Week(reader.GetString(0)));
                }
                reader.Close();

                //foreach (var item in weekList)
                //{
                //    Console.WriteLine(item.StampYear + " " + item.StampWeek + " " + item.Year + " " + item.Month + " " + item.Day);
                //}

                List<Data> dataList = new List<Data>();
                sql = new MySqlCommand("select * from wows_detonation.asia_deto_total order by Y" + weekList.Last<Week>().StampYear + "W" + weekList.Last<Week>().StampWeek + " desc limit 40", conn);
                reader = sql.ExecuteReader();
                MySqlDataReader reader2;
                MySqlConnection conn2 = new MySqlConnection("server=59.110.222.86;User Id=deto;password=WOWSdr2018;Database=wows_detonation;SslMode=None");
                conn2.Open();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string user_name = "undefined";
                    reader2 = new MySqlCommand("select user_name from wows_detonation.asia_player where id = " + id, conn2).ExecuteReader();
                    while (reader2.Read())
                    {
                        user_name = reader2.GetString(0);
                    }
                    reader2.Close();
                    int lastValidValue = 0;
                    for (int i = 0; i < weekList.Count; i++)
                    {
                        int value = reader.GetInt32(i + 1);
                        if (value == 0)
                        {
                            value = lastValidValue;
                        }
                        else
                        {
                            lastValidValue = value;
                        }
                        dataList.Add(new Data(user_name, "", value, weekList[i].ToDate()));
                    }
                }
                reader.Close();

                conn2.Close();
                conn.Close();

                AddLgoToTXT("name,type,value,date");
                foreach (var data in dataList)
                {
                    AddLgoToTXT(data.Name + "," + data.Type + "," + data.Value + "," + data.Date);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static void AddLgoToTXT(string logstring)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "result.csv";
            if (!System.IO.File.Exists(path))
            {
                FileStream stream = System.IO.File.Create(path);
                stream.Close();
                stream.Dispose();
            }
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(logstring);
            }
        }
    }

    class Week
    {
        public int StampYear { get; set; }
        public int StampWeek { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public Week(string stamp)
        {
            StampYear = (stamp[1] - '0') * 10 + (stamp[2] - '0');
            StampWeek = (stamp[4] - '0') * 10 + (stamp[5] - '0');

            int yy = 2017;
            int mm = 11;
            int dd = 25;
            int sy = 17;
            int sw = 46;

            while (sy < StampYear || (sy == StampYear && sw < StampWeek))
            {
                dd += 7;
                if (dd > getDayLimit(mm, yy))
                {
                    dd = (dd - getDayLimit(mm, yy));
                    mm++;
                    if (mm > 12)
                    {
                        mm = 1;
                        yy++;
                        sy++;
                        sw = -1;
                    }
                }
                sw++;
            }

            Year = yy;
            Month = mm;
            Day = dd;
        }

        public string ToDate()
        {
            return Year + "-" + Month + "-" + Day;
        }

        private int getDayLimit(int mm, int yy)
        {
            int[] notLeapLimit = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int[] LeapLimit = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (yy % 100 != 0 && yy % 4 == 0 || yy % 100 == 0 && yy % 400 != 0)
            {
                return LeapLimit[mm - 1];
            }
            else
            {
                return notLeapLimit[mm - 1];
            }
        }
    }

    class Data
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Value { get; set; }
        public string Date { get; set; }

        public Data(string name, string type, int value, string date)
        {
            Name = name;
            Type = type;
            Value = value;
            Date = date;
        }
    }
}
