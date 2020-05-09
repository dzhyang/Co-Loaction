using System.Collections.Generic;
using System.Linq;

namespace Co_Loaction.Models
{
    /// <summary>
    /// 组合后不同模式的星型实例 数据模型
    /// </summary>
    internal sealed class CombinationModel
    {

        /// <summary>
        /// 模式
        /// </summary>
        public char[] Feature { get; set; }
        /// <summary>
        /// 邻居实例数组，数组内每个元素是一个DbModel的集合
        /// </summary>
        public List<List<DbModel>> InstanceNeighbor { get; set; }

        public string[] InstanceNeighborStr { get
            {
                return (from _i in InstanceNeighbor select (from _ in _i select _.Id).ToArray().BackString()).ToArray();
            }}
    }
}
