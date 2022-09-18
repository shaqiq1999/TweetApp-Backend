using com.tweetapp.api.Models;
using com.tweetapp.api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace com.tweetapp.api.Controllers
{
    [Route("api/v1/tweets")]
    [ApiController]
    public class TweetsController : ControllerBase
    {
        public static List<TweetsModel> sampleTweets = new List<TweetsModel>() { };
        public readonly IConfiguration _configuration;
        public UserDataService _db;
        public TweetDataService _dbTweet;
        
        public TweetsController(IConfiguration configuration, UserDataService db, TweetDataService dbTweet)
        {
            _configuration = configuration;
            _db=db;
            _dbTweet = dbTweet;        
        }
       

        [HttpPost("register")]
        public ActionResult UserRegister([FromBody] UsersModel user)
        {

            var response=_db.Post(user);
            if (response.Contains("User Added Successfully"))
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(false);
            }
        }

        [HttpPost("login")]
        public ActionResult UserLogin([FromBody]LoginUsersModel loginUser)
        {
            var userlist =_db.FindUser(loginUser.userName,1).FirstOrDefault();
            if (userlist == null)
                return Unauthorized(false);
            
            UsersModel user = (UsersModel)userlist;
            bool b = _db.Valid(user, loginUser.Password);
            if (b == true)
                return Ok(true);
            return Unauthorized(false);
        }

        [HttpPost("{username}/forgot")]
        public ActionResult ForgotPassword(string username,[FromBody]Security security)
        {
            string newpassword=security.NewPassword;
            security.userName = username;
            var userlist = (UsersModel)_db.FindUser(username,1).FirstOrDefault();
            if (userlist == null)            
                return BadRequest("No user of that username");

            //UsersModel user = (UsersModel)userlist.FirstOrDefault();
            if (userlist.SecurityKey == security.SecurityKey)
            {
                var filter = Builders<UsersModel>.Filter.Eq("userName", username);      
                var update = Builders<UsersModel>.Update.Set("password", newpassword);
                _db._usersCollection.UpdateOne(filter, update);
            }
            else           
                return BadRequest("Wrong Security Key!!!");
            

            return Ok("Password successfully changed!!!");
        }

        [HttpGet("all")]
        public ActionResult GetAllTweets()
        {          
            var dbList = _dbTweet._tweetsCollection.AsQueryable();            
            if (dbList.Any())            
                return Ok(dbList.OrderByDescending(tweets => tweets.TweetTime));
            
            return NotFound(null);
        }

        [HttpGet("users/all")]
        public ActionResult GetAllUsers()
        {
           
            var dbList = _db._usersCollection.AsQueryable().ToList();
            List<Object> userObList = new List<Object>();
            if (dbList.Any())
            {
                foreach(var user in dbList)
                {
                    var userOb = new
                    {
                        userName = user.userName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        ContactNumber = user.ContactNumber,
                    };
                    userObList.Add(userOb);
                }

                return Ok(userObList);
            }
            
            return NotFound("No Users");
        }

        [HttpGet("user/search/{username}")]
        public ActionResult SearchByUsername(string username)
        {
            var userlist = _db.FindUser(username,2).ToList();
            //if (type == 1)
            //{
            //    if (userlist.FirstOrDefault() == null)
            //        return NotFound("No user of that username");

            //    var user = userlist[0];

            //    var userOb = new {
            //        userName = user.userName,
            //        FirstName = user.FirstName,
            //        LastName = user.LastName,
            //        Email = user.Email,
            //        ContactNumber = user.ContactNumber, };


            //    return Ok(userOb);
            //}
            if (userlist.FirstOrDefault() == null)
                return NotFound("No user of that username");
            
                List<Object> userObs = new List<Object>();
                foreach (UsersModel user in userlist){
                    var userOb = new
                    {
                        userName = user.userName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        ContactNumber = user.ContactNumber,
                    };
                    userObs.Add(userOb);
                    
                }
                return Ok(userObs);
            
        }

        [HttpGet("{username}")]
        public ActionResult GetAllTweetsOfUser(string username)
        {
            
            var tweets = _dbTweet.FindTweet(username);
            if (tweets == null)            
                return NotFound(tweets);
            
            return Ok(tweets.OrderByDescending(tweets => tweets.TweetTime));
           
        }

        [HttpPost("{username}/add")]
        public ActionResult PostTweet(string username, [FromBody] TweetsModel tweet)
        {
            //return Ok(_dbTweet.Post(tweet,username).Value);
            var response = _dbTweet.Post(tweet,username);
            if (response.Contains("Tweet Posted successfully!!!"))
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpPut("{username}/update/{id}")]
        public ActionResult UpdateTweet(string username, int id, [FromBody] TweetsModel currentTweet)
        {

            var filter = Builders<TweetsModel>.Filter.Eq("uniqueId", id);
            filter &= Builders<TweetsModel>.Filter.Eq("userName", username);
            var update = Builders<TweetsModel>.Update.Set("TweetBody", currentTweet.TweetBody)
                                                     .Set("TweetTime", currentTweet.TweetTime);
            _dbTweet._tweetsCollection.UpdateOne(filter, update);
            
            return Ok("Updated Successfully");
        }

        [HttpDelete("{username}/delete/{id}")]
        public ActionResult DeleteTweet(string username, int id)
        {           
            var filter = Builders<TweetsModel>.Filter.Eq("uniqueId", id) & Builders<TweetsModel>.Filter.Eq("userName", username);
            //filter &= Builders<TweetsModel>.Filter.Eq("userName", username);

            if (filter != null)
            {
                _dbTweet._tweetsCollection.DeleteOne(filter);
                var dbList = _dbTweet._tweetsCollection.AsQueryable().Where(p => p.userName == username && p.uniqueId>id);
                _dbTweet.SetId(id, dbList);
               
                return Ok("Deleted Successfully");
            }
            return NotFound("Invalid Tweet");
        }

        [HttpPut("{username}/like/{id}")]
        public ActionResult LikeTweet(string username, string id )
        {
            string a = "Not Found";
            Console.WriteLine("api");
            var dbList = _dbTweet._tweetsCollection.AsQueryable().Where(p => p.id == id);
            if (dbList.Any())           
                a = _dbTweet.LikeDislike(username,id ,dbList);                                 
            return Ok(a );
        }

        [HttpPost("{username}/reply/{id}")]
        public ActionResult ReplyTweet(string username, string id, [FromBody]ReplyModel reply)
        {          
            var dbList = _dbTweet._tweetsCollection.AsQueryable().Where(p => p.id ==id);
            reply.userName = username;
            reply.ReplyTime = DateTime.Now;
            if (dbList.Any())            
                _dbTweet.Reply(username, id, reply, dbList);
                          
            else
                return NotFound("invalid");
            return Ok("Reply successful");
        }
    }
}
