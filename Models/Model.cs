using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co_Loaction.Models
{
    /// <summary>
    /// 基本数据模型
    /// </summary>
    internal struct Model
    {
        /// <summary>
        /// 实例唯一索引
        /// </summary>
        public UInt32 Id { get; set; }
        /// <summary>
        /// 特征
        /// </summary>
        public Char Feature { get; set; }
        /// <summary>
        /// X坐标
        /// </summary>
        public Single X { get; set; }
        /// <summary>
        /// Y坐标
        /// </summary>
        public Single Y { get; set; }
    }
}
