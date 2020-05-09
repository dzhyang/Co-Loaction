using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace Co_Loaction.Models
{
    class MysqlHelper
    {
        static string conStr = "server=localhost;port=3306;user=root;password=79920420; database=colocation;";

        static string sqlcon = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=colocation;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        MySqlConnection connection;
        public MysqlHelper()
        {
            connection = new MySqlConnection(conStr);
            connection.Open();
        }
       public void Insert(Dictionary<char, List<DbModel>> dbs)
        {
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            //string sql = $"insert into model values(?id,?feature,?x,?y);";
            
            foreach (var items in dbs)
            {
                foreach (var item in items.Value)
                {
                    cmd.CommandText = $"insert into model(feature,x,y) values('{item.Feature}',{item.X},{item.Y});";

                    cmd.ExecuteNonQuery();
                }
            }
            transaction.Commit();
            connection.Close();
        }

        public void InsertMode(string mode)
        {
            var sql = $"insert delayed into `{mode.Length}` values('{mode}')";
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        public List<string> SelectMode(int index)
        {
            var temp = new List<string>();
            var sql = $"select * from `{index-1}`";
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            var reader =cmd.ExecuteReader();
            while (reader.Read())
            {
                temp.Add(reader[0].ToString());
            }
            reader.Close();
            return temp;
        }
        public void InsertModeInstence(string mode,List<List<DbModel>> dbs)
        {
            var sql = $"insert delayed into {mode} value ";
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SET UNIQUE_CHECKS=0;set foreign_key_checks=0;";
            cmd.ExecuteNonQuery();
            for (int i=0;i<dbs.Count;i++)
            {
                var temp = "(";
                foreach (var item in dbs[i].OrderBy(_ => _.Feature))
                {
                    temp += item.Id + ",";
                }
                sql+=  new string(temp.Take(temp.Length - 1).ToArray()) + "),";

                if (i%5000==0)
                {

                    cmd.CommandText = sql.Substring(0, sql.LastIndexOf(","));
                    cmd.ExecuteNonQuery();
                    sql= $"insert delayed into {mode} value ";
                }
            }
            cmd.CommandText = sql.Substring(0, sql.LastIndexOf(","));
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SET UNIQUE_CHECKS=1;set foreign_key_checks=1;";
            cmd.ExecuteNonQuery();

        }

        public bool InstenceIsExits(string mode,List<DbModel> models)
        {
            var sql = $"select isnull((select {mode[0]} from `{mode}` where ";
            var end = "))";
            var cmd = connection.CreateCommand();

            cmd.CommandText = sql + string.Join(" and ", models.OrderBy(_ => _.Feature).Select(l => string.Join("=", l.Feature, l.Id))) + end;

            using (var reader= cmd.ExecuteReader())
            {
                reader.Read();
                return reader.GetBoolean(0);
            }
        }

        public void CreateModeTable(string mode)
        {

            var cmd = connection.CreateCommand();
            var sql = $"CREATE TABLE IF NOT EXISTS `colocation`.`{mode}` (";
            var k = "`{0}` INT NOT NULL,";


            var pk = "PRIMARY KEY(";//`A`, `B`, `C`
                                
            var end= ")) ENGINE = InnoDB";
            var temp = "";

            foreach (var item in mode.OrderBy(_=>_))
            {
                sql += string.Format(k, item);
                temp += $"`{item}`"+',';
            }

            cmd.CommandText = sql + pk + new string(temp.Take(temp.Length - 1).ToArray()) + end;
            cmd.ExecuteNonQuery();

            cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS `colocation`.`{mode.Length}` (`mode` VARCHAR(50) NOT NULL , PRIMARY KEY(`mode`)) ENGINE = InnoDB";
            cmd.ExecuteNonQuery();

        }
    }
}
