using Co_Loaction.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co_Loaction
{
    class Program
    {
        static void Main(string[] args)
        {

            var dbs = API.CsvRead("./data/2015_clear.csv");

            List<DbModel> list = new List<DbModel>();

            foreach (var item in dbs)
            {
                list.AddRange(item.Value);
            }
            Console.WriteLine("--X--");
            Console.WriteLine(list.Max(l=>l.X));
            Console.WriteLine(list.Min(l => l.X));
            Console.WriteLine("--Y--");
            Console.WriteLine(list.Max(l => l.Y));
            Console.WriteLine(list.Min(l => l.Y));
            //数据聚类处理，指定范围阈值内点删除，保留中心点

            Random random = new Random();

            HashSet<int> temp = new HashSet<int>();
            var crs =/*new char[]{ 'A', 'B', 'C','D','E','F','G','H','I'}*/dbs.Keys.ToArray();
            List<DbModel> _items = new List<DbModel>();
            ////筛选
            ///
            foreach (var cr in crs)
            {



                while (dbs[cr].Count!=0)
                {
                    var mainpoint = dbs[cr][random.Next(dbs[cr].Count)];
                    //范围阈值越大，去除数据量越大
                    dbs[cr].RemoveAll(l => l.Distance(mainpoint, 0.008));
                    dbs[cr].Remove(mainpoint);
                    _items.Add(mainpoint);
                }
                dbs[cr] = new List<DbModel>(_items);
                Console.WriteLine(_items.Count);
                _items.Clear();
            }


            //距离阈值越小，数据越少
            var s = new Joinless(dbs,0.03,0.5);
            s.Build();

            //Dictionary<string, int> dic = new Dictionary<string, int>();
            //for (int i = 0; i < m_Data.Length; i++)
            //{
            //    Console.WriteLine(m_Data[i]);
            //    dic.Add(m_Data[i], i);
            //}
            //GetString(dic);
            //Console.ReadLine();



            //using (StreamWriter writer=new StreamWriter(@"./back.csv"))
            //{
            //    writer.WriteLine("s,d,f,s");
            //    writer.WriteLine("s,d,f,s");
            //    writer.WriteLine("s,d,f,s");
            //    writer.WriteLine("s,d,f,s");

            //}


            //MySqlConnection sqlConnection = new MySqlConnection(@"server=localhost;port=3306;user id=root;password=79920420;database=other");
            //sqlConnection.Open();
            //MySqlCommand sqlCommand= sqlConnection.CreateCommand();
            //sqlCommand.CommandText = @"select * from maintable";
            //MySqlDataReader reader= sqlCommand.ExecuteReader();
            //while (reader.Read())
            //{
            //    Console.WriteLine(reader.GetInt32(0));
            //    Console.WriteLine(reader.GetString(1));
            //}
            //reader.Close();
            //sqlConnection.Close();
            Console.WriteLine("");
            Log.Logger.Info("----------------------完成-------------------------");
            Console.ReadLine();

        } 

    }
}
/* 实例依据特征，由小到大排序，从具有最小特征的实例开始，计算其 与 特征比他大的实例 是否为邻居，
 */
