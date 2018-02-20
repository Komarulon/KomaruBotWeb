using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.DAL
{
    public class RedisObjectValue
    {
        public string key { get; set; }
        public object value { get; set; }
        public DateTime? expiration { get; set; }

        public RedisObjectValue() { }
        public RedisObjectValue(string key, object value)
        {
            this.key = key;
            this.value = value;
        }
        public RedisObjectValue(string key, object value, DateTime expiration)
        {
            this.key = key;
            this.value = value;
            this.expiration = expiration;
        }
    }
}
