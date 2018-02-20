using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.DAL
{

    public class RedisSortedSetObjectValue
    {
        /// <summary>
        /// The identity of the object set in the sorted set
        /// <para>Make sure the object set is the IDENTITY of the object and does not change over time.</para>
        /// </summary>
        public string value { get; set; }
        public double score { get; set; }

        public RedisSortedSetObjectValue() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">
        /// The identity of the object set in the sorted set
        /// <para>Make sure the object set is the IDENTITY of the object and does not change over time.</para>
        /// </param>
        /// <param name="score"></param>
        public RedisSortedSetObjectValue(string value, double score)
        {
            this.score = score;
            this.value = value;
        }
    }
}
