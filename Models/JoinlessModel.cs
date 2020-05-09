using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co_Loaction.Models
{
    /// <summary>
    /// Joinless第一阶段 实例的邻居 数据模型
    /// </summary>
    internal sealed class JoinlessModel
    {
        /// <summary>
        /// 实例
        /// </summary>
        public DbModel Instance { get; set; }
        /// <summary>
        /// 带特征邻居
        /// </summary>
        public Dictionary<char,List<DbModel>> NeighborWithFeature { get; set; }

    }
}
