using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co_Loaction.Models
{
    /// <summary>
    ///  模式的星型实例 数据模型
    /// </summary>
    internal class CoLocationModel<T>
    {
        /// <summary>
        /// 模式
        /// </summary>
        public char[] Feature { get; set; }
        /// <summary>
        /// 邻居实例
        /// </summary>
        public List<DbModel> InstanceNeighbor { get; set; }
    }
}
