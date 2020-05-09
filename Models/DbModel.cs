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
    internal sealed class DbModel
    {
        /// <summary>
        /// 实例唯一索引
        /// </summary>
        public short Id { get; set; }
        /// <summary>
        /// 特征
        /// </summary>
        public char Feature { get; set; }
        /// <summary>
        /// X坐标
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y坐标
        /// </summary>
        public float Y { get; set; }
    }
}
