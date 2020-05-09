using Co_Loaction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Co_Loaction
{
    internal static class API
    {


        #region 扩展方法

        /// <summary>
        /// 判断两个实例是否为邻居
        /// </summary>
        /// <param name="a">实例1</param>
        /// <param name="b">实例2</param>
        /// <param name="threshold">欧几里得距离阙值</param>
        /// <returns></returns>
        public static bool Distance(this DbModel a, DbModel b, double threshold = 200)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(a.X - b.X), 2) + Math.Pow(Math.Abs(a.Y - b.Y), 2)) < threshold;
        }

        /// <summary>
        /// 判断两个实例是否为邻居
        /// </summary>
        /// <param name="a">实例1</param>
        /// <param name="b">实例2</param>
        /// <param name="threshold">欧几里得距离阙值</param>
        /// <returns></returns>
        [Obsolete("返回迭代器对象，不好实现")]
        public static IEnumerable<bool> Distance(this DbModel a, Func<IEnumerable<DbModel>> b, int threshold = 20)
        {
            foreach (var _d in b.Invoke())
            {
                yield return Distance(a, _d, threshold);
            }

        }


        /// <summary>
        /// 模式参与度
        /// </summary>
        /// <param name="model">模式</param>
        /// <param name="i">参与组成模式的各个特征总数</param>
        /// <param name="threshold">参与度阈值</param>
        /// <returns>达到阈值则返回<code>true</code>否则返回<code>false</code></returns>
        public static bool Participation(this List<List<int>> model,int[] i,out double pt,double threshold=0.7)
        {
            pt= model.Select(l => { return (((double)l.Count / (double)i[model.IndexOf(l)])); }).Min();
            return pt > threshold;
        }


        /// <summary>
        /// 从指定索引返回数组
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="array">原数组</param>
        /// <param name="index">开始返回的位置</param>
        /// <param name="len">返回的数目</param>
        /// <returns></returns>
        public static T[] BackArray<T>(this T[] array, int index=1,int len=0)
        {
            if (array.Length == 1) return new T[array.Length];
            len = len == 0 ? array.Length - index : len;
            T[] o = new T[array.Length-1];
            Array.Copy(array,index,o,0,len);
            return o;
        }

        /// <summary>
        /// 返回数组字符串
        /// </summary>
        /// <param name="c">原数组</param>
        /// <returns>返回的字符串</returns>
        public static string BackString<T>(this T[] c)
        {
            var t = "";
            foreach (var f in c)
            {
                t += f;
            }
            return t;
        }



        /// <summary>
        /// 添加无重复元素
        /// </summary>
        /// <param name="l">要添加进的链表</param>
        /// <param name="i">添加的元素</param>
        public static List<int> AddNoRepeat(this List<int> l, int i)
        {
            if (!l.Contains(i)) l.Add(i);
            return l;
        }




        /// <summary>
        /// 笛卡尔积 两个集合的组合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">传入的要组合的变量</param>
        /// <param name="index">链表默认索引</param>
        /// <param name="data">组合后的链表</param>
        /// <returns>组合好的链表</returns>
        public static List<List<DbModel>> Descartes<T>(this IEnumerable<T> t, List<List<int>> _i, short index = 0, List<DbModel> data = null) where T : List<DbModel>
        {
            var _result = new List<List<DbModel>>();
            if (data == null) data = new List<DbModel>();
            var _t = new List<T>(t);
            foreach (var _d in _t[index])
            {
                data.Add(_d);
                _i[index+1].AddNoRepeat(_d.Id);
                if (index + 1 < _t.Count) t.Descartes(_i,(short)(index + 1),data).ToList().ForEach(l => _result.Add(l));
                else _result.Add(new List<DbModel>(data));
                data.RemoveAt(data.Count - 1);
            }
            return _result;
        }


        /// <summary>
        /// 笛卡尔积 linq方式实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_source">要组合的列表</param>
        /// <returns>组合后的列表</returns>
        [Obsolete("暂时未找到计数方法")]
        public static IEnumerable<IEnumerable<T>> Cartesian<T>(this IEnumerable<IEnumerable<T>> _source, List<List<int>> _i)
        {
            IEnumerable<IEnumerable<T>> _temp = new[] { Enumerable.Empty<T>() };
            return _source.Aggregate(_temp,(_t, _s) =>from _t_v in _t from _s_v in _s select _t_v.Concat(new[] { _s_v }));
        }

        #endregion



        #region 文件操作

        /// <summary>
        /// 从.csv文件读取数据
        /// </summary>
        /// <param name="filepath">路径</param>
        /// <returns>返回数据链表</returns>
        public static Dictionary<char, List<DbModel>> CsvRead(string filepath) => (new List<DbModel>(
            from source in File.ReadAllLines(filepath)
            let temp = source.Split(',')
            select new DbModel() { Id = Convert.ToInt16(temp[0]), Feature = Convert.ToChar(temp[1]), X = Convert.ToSingle(temp[2]), Y = Convert.ToSingle(temp[3]), })).GroupBy(l => l.Feature).OrderBy(l => l.Key).ToDictionary(l => l.Key, l => l.ToList());

        [Obsolete("数据处理方法其他写法")]
        public static Dictionary<char, List<DbModel>> _CsvRead(string filepath)
        {
            return (from source in File.ReadAllLines(filepath)
                    let temp = source.Split(',')
                    group source by temp[1] into feature
                    orderby feature.Key
                    select new List<DbModel>(
                        from _source in feature
                        select new DbModel()
                        {
                            Id = Convert.ToInt16(_source.Split(',')[0]),
                            Feature = Convert.ToChar(_source.Split(',')[1]),
                            X = Convert.ToSingle(_source.Split(',')[2]),
                            Y = Convert.ToSingle(_source.Split(',')[3]),
                        })).ToDictionary(t => t[0].Feature);
        }


        /// <summary>
        /// 读取.csv转换为xml文件
        /// </summary>
        /// <param name="filepath">路径</param>
        public static void Csv2Xml(string filepath)
        {
            string[] sources = File.ReadAllLines(filepath);
            XElement element = new XElement("root",
                from source in sources
                let temp = source.Split(',')
                select new XElement("Instance", new XAttribute("Id", temp[0]), new XElement("Feature", temp[1]), new XElement("X", temp[2]), new XElement("Y", temp[3]))
                );
            element.Save(Path.GetDirectoryName(filepath) + @"/source.xml");
        }





        public static void CsvWrite(string path,string fea,double pt)
        {
            using(StreamWriter writer=new StreamWriter(path,true))
            {
                    writer.WriteLine(fea + "," + pt);
            }
        }
        #endregion
    }
}
