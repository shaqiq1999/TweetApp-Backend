using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.tweetapp.api.Models
{
    public class ReplyModel
    {
        public string userName { get; set; }
        public string Reply { get; set; }
        public DateTime ReplyTime { get; set; }
    }
}
