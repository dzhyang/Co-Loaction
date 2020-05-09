using Co_Loaction.Log;
using Co_Loaction.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Co_Loaction
{
    class Joinless
    {
        /// <summary>
        /// 源数据
        /// </summary>
        private readonly Dictionary<char, List<DbModel>> _source_dic;

        /// <summary>
        /// 特征列表
        /// </summary>
        private readonly List<char> _feature;
        private readonly double td;
        private readonly double pd;

        /// <summary>
        /// 各个实例的星型邻居
        /// </summary>
        private readonly Dictionary<char, List<JoinlessModel>> _first = new Dictionary<char, List<JoinlessModel>>();


        //private MysqlHelper MysqlHelper = new MysqlHelper();

        /// <summary>
        /// 处理传入数据
        /// </summary>
        /// <param name="source_dic">传入数据</param>
        public Joinless(Dictionary<char, List<DbModel>> source_dic, double td,double pd)
        {
            _source_dic = source_dic;
            _feature = source_dic.Keys.ToList();
            this.td = td;
            this.pd = pd;
            StageOne();
        }

        /// <summary>
        /// 第一阶段
        /// </summary>
        private void StageOne()
        {
            var t1 = DateTime.Now;
            //较小特征
            foreach (var _f in _feature)
            {
                //TODO 数据量过大时，时间太长，之后可根据较小特征来开多线程
                var _temp = new List<JoinlessModel>();
                Logger.Info($"开始进行特征{_f}的星型邻居的计算");
                //得到较小特征的实例
                foreach (var _i in _source_dic[_f])
                {
                    var _d = new Dictionary<char, List<DbModel>>();
                    foreach (var _l in _feature)
                    {
                        var _n = new List<DbModel>();
                        if (_l > _f)//较大特征
                        {
                            //实例
                            _source_dic[_l].ForEach(l =>
                            {
                                if (l.Distance(_i, threshold: td)) _n.Add(l);
                            });
                            /*if(_n.Count!=0&&_n!=null)*/
                            _d.Add(_l, _n);
                        }
                    }
                    //if (_d.Count == 0||_d==null) continue;
                    _temp.Add(new JoinlessModel()
                    {
                        Instance = _i,
                        NeighborWithFeature = _d
                    });
                }

                _first.Add(_f, _temp);
            }
            var t2 = DateTime.Now;
            Logger.Info($"第一阶段计算共{_feature.Count}个特征，耗时：{(t2 - t1).TotalSeconds}秒\n");
        }

        //效率换空间
        private IEnumerable<JoinlessModel> Run(char feature)
        {
            foreach (var _i in _source_dic[feature])
            {
                var _d = new Dictionary<char, List<DbModel>>();
                foreach (var _l in _feature)
                {
                    var _n = new List<DbModel>();
                    if (_l > feature)//较大特征
                    {
                        //实例
                        _source_dic[_l].ForEach(l =>
                        {
                            if (l.Distance(_i, threshold: td)) _n.Add(l);
                        });
                        /*if(_n.Count!=0&&_n!=null)*/
                        _d.Add(_l, _n);
                    }
                }
                //if (_d.Count == 0||_d==null) continue;
                yield return new JoinlessModel()
                {
                    Instance = _i,
                    NeighborWithFeature = _d
                };
            }

        }


        /// <summary>
        /// 第二阶段
        /// </summary>
        private Dictionary<string, HashSet<List<DbModel>>> StageTwo(Dictionary<string, HashSet<List<DbModel>>> _baseData, short index = 2)
        {
            Dictionary<string, HashSet<List<DbModel>>> _temp = new Dictionary<string, HashSet<List<DbModel>>>();
            HashSet<string> keys;
            if (index != 2)
            {
                //keys = MysqlHelper.SelectMode(index);
                keys = new HashSet<string>(_baseData.Keys);
            }
            else
            {
                keys = new HashSet<string>(from _c in _feature select _c.ToString());
            }
            //取出一种新模式，index是阶数
            foreach (var pattern in CombinationNoRecursive(keys))
            {

                //中转集合
                var _i = new List<List<DbModel>>();

                //参与的实例集合
                var usedInstanceSet = new List<List<int>>(from l in pattern select new List<int>());

                //获取最小特征的各个实例邻居
                foreach (var instanceWithNeighbor in /*Run(pattern.Min())*/_first[pattern.Min()])
                {
                    //在最小特征的各个实例邻居中，获取除开最小特征的这一组特征的笛卡尔积
                    var descartesWithoutMinFeature = (from _m in pattern.Substring(1) select instanceWithNeighbor.NeighborWithFeature[_m]).Descartes(usedInstanceSet);
                    foreach (var instances in descartesWithoutMinFeature)
                    {
                        if (index > 2)
                        {
                            if (keys.Contains(pattern.Substring(1)))
                            {
                                if (/*MysqlHelper.InstenceIsExits(_c, instances)*/_baseData[pattern.Substring(1)].Add(instances))
                                {
                                    instances.Insert(0, instanceWithNeighbor.Instance); usedInstanceSet[0].AddNoRepeat(instanceWithNeighbor.Instance.Id);
                                    _i.Add(instances);
                                }
                            }
                        }
                        else
                        {
                            instances.Insert(0, instanceWithNeighbor.Instance); usedInstanceSet[0].AddNoRepeat(instanceWithNeighbor.Instance.Id); _i.Add(instances);

                        }
                    }
                }

                if (usedInstanceSet.Participation((from _c in pattern select _source_dic[_c].Count).ToArray(), out double pt,threshold:pd))
                {
                    Logger.Info("---" + pattern + " : " + pt + "\t---\t" + _i.Count);
                    //Logger.Info("------开始存储数据------");
                    //var t1 = DateTime.Now;
                    //MysqlHelper.CreateModeTable(pattern);
                    //MysqlHelper.InsertModeInstence(pattern, _i);
                    //MysqlHelper.InsertMode(pattern);
                    //Logger.Info($"---存储{_i.Count}条数据，耗时{(DateTime.Now - t1).TotalSeconds}---");
                    API.CsvWrite(@"./result/db.csv", pattern, pt);

                    _temp.Add(pattern, new HashSet<List<DbModel>>(_i));
                }
                else Logger.Error("---" + pattern + " : " + pt + "\t---\t" + _i.Count);

                _i.Clear();

            }




            return _temp;

        }


        //public IEnumerable<List<DbModel>> Tp()
        //{
        //    foreach (var instanceWithNeighbor in /*Run(pattern.Min())*/_first[pattern.Min()])
        //    {
        //        //在最小特征的各个实例邻居中，获取除开最小特征的这一组特征的笛卡尔积
        //        var descartesWithoutMinFeature = (from _m in pattern.BackArray(1) select instanceWithNeighbor.NeighborWithFeature[_m]).Descartes(usedInstanceSet);
        //        foreach (var instances in descartesWithoutMinFeature)
        //        {
        //            if (index > 2)
        //            {
        //                foreach (var _c in _baseData.Keys)
        //                {
        //                    if (pattern.BackArray().BackString() == _c.BackString())
        //                    {
        //                        if (_baseData[_c].Add(instances))
        //                        {
        //                            instances.Insert(0, instanceWithNeighbor.Instance); usedInstanceSet[0].AddNoRepeat(instanceWithNeighbor.Instance.Id); yield return instances ;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                instances.Insert(0, instanceWithNeighbor.Instance); usedInstanceSet[0].AddNoRepeat(instanceWithNeighbor.Instance.Id); yield return instances;
        //            }
        //        }
        //    }
        //}



        public void Build()
        {


            var t1 = StageTwo(StageTwo(null), 3);
            var t2 = StageTwo(t1, 4);
            var t3 = StageTwo(t2, 5);
            var t4 = StageTwo(t3, 6);
            StageTwo(t4, 7);

        }


        #region 生成模式

        /// <summary>
        /// 根据k阶模式生成k+1阶模式
        /// </summary>
        /// <param name="index">阶</param>
        /// <returns>k+1模式</returns>
        private IEnumerable<string> CombinationNoRecursive(IEnumerable<string> _baseData)
        {
            //if (index > _feature.Count) return new List<char[]>();
            //var _lc = new List<char[]>();
            var s = (from _c in _baseData select _c);
            foreach (var _f in s)
            {
                foreach (var _l in s)
                {
                    if ((_l.Last() > _f.Last()) && (_l.Substring(0, _l.Length - 1) == _f.Substring(0, _f.Length - 1)))
                    {

                        yield return _f + _l.Last();
                        //_lc.Add(_temp);
                    }
                }
            }
            //return _lc;
        }
        #endregion
    }
}
