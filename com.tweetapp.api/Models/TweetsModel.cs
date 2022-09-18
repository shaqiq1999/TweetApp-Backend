using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace com.tweetapp.api.Models
{
    public class TweetsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
       
        
        public int? uniqueId { get; set; }
        public string userName { get; set; }
        public string TweetBody { get; set; }
        public string TweetTag { get; set; }
        public DateTime TweetTime { get; set; }
        public List<ReplyModel> TweetReply { get; set; }
        public List<string> LikedUser { get; set; }
        public int LikesCount { get; set; }
    }
}
