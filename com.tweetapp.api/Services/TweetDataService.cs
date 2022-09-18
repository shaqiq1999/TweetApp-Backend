using com.tweetapp.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.tweetapp.api.Services
{
    public class TweetDataService : ITweetDataService
    {
        public readonly IConfiguration _configuration;
        public readonly IMongoCollection<TweetsModel> _tweetsCollection;


        public TweetDataService(IConfiguration configuration)
        {
            _configuration = configuration;
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("TweetappCon"));
            IMongoDatabase mongoDatabase = dbClient.GetDatabase(_configuration.GetConnectionString("DatabaseName"));
            _tweetsCollection = mongoDatabase.GetCollection<TweetsModel>("Tweets");

        }


        public string Post(TweetsModel tweets, string username)
        {

            tweets.uniqueId = (_tweetsCollection.AsQueryable().Where(p => p.userName == username).Count()) + 1;
            tweets.userName = username;
            _tweetsCollection.InsertOne(tweets);
            var Response = "Tweet Posted successfully!!!";
            return Response;
        }

        public IQueryable<TweetsModel> FindTweet(string username)
        {
            var dbList = _tweetsCollection.AsQueryable().Where(p => p.userName == username);
            if (dbList.Any())
                return (dbList);
           
            return null;

        }

        public void SetId(int id, IQueryable<TweetsModel> dbList)
        {
            foreach (TweetsModel tweets in dbList)
            {              
                    var filterId = Builders<TweetsModel>.Filter.Eq("_id", ObjectId.Parse(tweets.id));
                    int? temp = tweets.uniqueId-1;
                    var updateId = Builders<TweetsModel>.Update.Set("uniqueId", temp);
                    _tweetsCollection.UpdateOne(filterId, updateId);
               
            }
        }

        public string LikeDislike(string username,string id, IQueryable<TweetsModel> dbList)
        {                                
            TweetsModel tweet = dbList.FirstOrDefault();
            var filter = Builders<TweetsModel>.Filter.Eq("_id", ObjectId.Parse(id));            
            string response= "";

            //List<string> likedUsers = tweet.LikedUser;

            if (tweet.LikedUser == null)
            {
                tweet.LikesCount++;
                //likedUsers = new List<string>();
                tweet.LikedUser = new List<string>();
                tweet.LikedUser.Add(username);
                response = "Liked";
            }
            else if (tweet.LikedUser.Contains(username))
            {
                tweet.LikesCount--;
                tweet.LikedUser.Remove(username);
                response = "Disliked";
            }
            else
            {
                tweet.LikesCount++;
                tweet.LikedUser.Add(username);
                response = "Liked";
            }
            var update = Builders<TweetsModel>.Update.Set("LikesCount", tweet.LikesCount)
                                                          .Set("LikedUser", tweet.LikedUser);
            _tweetsCollection.UpdateOne(filter, update);
            
            return response;
        }

        public void Reply(string username, string id, ReplyModel reply, IQueryable<TweetsModel> dbList)
        {
            var filter = Builders<TweetsModel>.Filter.Eq("_id", ObjectId.Parse(id));
            TweetsModel tweet = dbList.FirstOrDefault();
            if (tweet.TweetReply == null)
                tweet.TweetReply = new List<ReplyModel>();
            tweet.TweetReply.Add(reply);

            var update = Builders<TweetsModel>.Update.Set("TweetReply", tweet.TweetReply);
            _tweetsCollection.UpdateOne(filter, update);
        }
    }
}
