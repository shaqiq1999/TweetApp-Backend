using com.tweetapp.api.Controllers;
using com.tweetapp.api.Models;
using com.tweetapp.api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace com.tweetapp.api.UnitTests
{
    public class TweetsTests
    {
        TweetsController _tweetsController;
        private readonly IMongoCollection<UsersModel> _usersCollection;
        private readonly IMongoCollection<TweetsModel> _tweetsCollection;

        private readonly FilterDefinitionBuilder<TweetsModel> _tweetsFilterBuilder = Builders<TweetsModel>.Filter;
        private readonly FilterDefinitionBuilder<UsersModel> _usersFilterBuilder = Builders<UsersModel>.Filter;

        private readonly UpdateDefinitionBuilder<TweetsModel> _tweetsUpdateBuilder = Builders<TweetsModel>.Update;
        private readonly UpdateDefinitionBuilder<UsersModel> _usersUpdateBuilder = Builders<UsersModel>.Update;

        string testDBConnectionURI = "mongodb://localhost:27017";
        string testDBName = "tweetapptest";
        public TweetsTests()
        {
            MongoClient mongoClient = new MongoClient(testDBConnectionURI);
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(testDBName);
            _usersCollection = mongoDatabase.GetCollection<UsersModel>("Users");
            _tweetsCollection = mongoDatabase.GetCollection<TweetsModel>("Tweets");
        }


        [SetUp]
        public void Setup()
        {
            var testConfigSettings = new Dictionary<string, string> {
                                  { "ConnectionStrings:TweetappCon", testDBConnectionURI},
                                  { "ConnectionStrings:DatabaseName",  testDBName}
                        };
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(testConfigSettings).Build();
           
            UserDataService _userServices = new UserDataService(configuration);
            TweetDataService _tweetServices = new TweetDataService(configuration);
            _tweetsController = new TweetsController(configuration,_userServices, _tweetServices);

        }

        [Test]
        public void UserRegister_NewUserRegister_ShouldGetRegistered()
        {
            UsersModel user = new UsersModel()
            {
                userName = "abirdas",
                FirstName = "Abir",
                LastName = "Das",
                Email = "abirdas@gmail.com",
                Password = "abirdas@1",
                SecurityKey = "abirismyself",
                ContactNumber = "9865745235"
            };
            var response = (OkObjectResult)_tweetsController.UserRegister(user);
            
            Assert.IsNotNull(response);           
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }
        [Test]
        public void UserRegister_NewUserRegisterExistingUsername_ShouldGetError()
        {
            UsersModel user = new UsersModel()
            {
                userName = "sha",
                FirstName = "Abir",
                LastName = "Das",
                Email = "abirdas@gmail.com",
                Password = "abird1",
                SecurityKey = "abirismys",
                ContactNumber = "9865745235"
            };
            var response = (BadRequestObjectResult)_tweetsController.UserRegister(user);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.BadRequest);
        }
        [Test]
        public void UserRegister_NewUserRegisterExistingEmail_ShouldGetError()
        {
            UsersModel user = new UsersModel()
            {
                userName = "abirdas",
                FirstName = "Abir",
                LastName = "Das",
                Email = "raihan@gmail.com",
                Password = "abird1",
                SecurityKey = "abirismys",
                ContactNumber = "9865745235"
            };
            var response = (BadRequestObjectResult)_tweetsController.UserRegister(user);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.BadRequest);
        }
        [Test]
        public void UserRegister_NewUserRegisterExistingContactNumber_ShouldGetError()
        {
            UsersModel user = new UsersModel()
            {
                userName = "abirda",
                FirstName = "Abir",
                LastName = "Das",
                Email = "raiha@gmail.com",
                Password = "abird1",
                SecurityKey = "abirismys",
                ContactNumber = "9595857888"
            };
            var response = (BadRequestObjectResult)_tweetsController.UserRegister(user);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.BadRequest);
        }
        [Test]
        public void UserLogin_ExistingUserTryLoginCorrectCredentials_ShouldLogin()
        {
            LoginUsersModel user = new LoginUsersModel()
            {
                userName = "shaqiq",
                Password = "raihan",
            };
            var response = (OkObjectResult)_tweetsController.UserLogin(user);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
            
        }
        [Test]
        public void UserLogin_ExistingUserTryLoginWrongUsername_ShouldGetError()
        {
            LoginUsersModel user = new LoginUsersModel()
            {
                userName = "shaqi",
                Password = "raih",
            };
            var response = (UnauthorizedObjectResult)_tweetsController.UserLogin(user);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.Unauthorized);
            
        }
        [Test]
        public void UserLogin_ExistingUserTryLoginWrongPassword_ShouldGetError()
        {
            LoginUsersModel user = new LoginUsersModel()
            {
                userName = "shaqiq",
                Password = "raih",
            };
            var response = (UnauthorizedObjectResult)_tweetsController.UserLogin(user);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.Unauthorized);
            
        }
        [Test]
        public void ForgotPassword_ExistingUserTryForgotPasswordValidSecurityKey_ShouldGetPasswordReseted()
        {
            Security security = new Security();
            security.userName = "shaqiq";
            security.SecurityKey = "string@1";
            security.NewPassword = "New@Password";
           
            var response = (OkObjectResult)_tweetsController.ForgotPassword(security.userName,security);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
            Assert.IsTrue(response.Value.Equals("Password successfully changed!!!"));
        }
        [Test]
        public void ForgotPassword_ExistingUserTryForgotPasswordWrongSecurityKey_ShouldGetError()
        {
            Security security = new Security();
            security.userName = "shaqiq";
            security.SecurityKey = "string";
            security.NewPassword = "New@Password";
            var response = (BadRequestObjectResult)_tweetsController.ForgotPassword(security.userName, security);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.BadRequest);
            Assert.IsTrue(response.Value.Equals("Wrong Security Key!!!"));
        }

        [Test]
        public void GetAllTweets_UserTryGetAllTweets_ShouldGetAllTweets()
        {
            var response = (OkObjectResult)_tweetsController.GetAllTweets();
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }

        [Test]
        public void GetAllUsers_UserTryGetAllUsers_ShouldGetAllUsers()
        {
            var response = (OkObjectResult)_tweetsController.GetAllUsers();
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }

        [Test]
        public void SearchByUsername_UserTrySearchByUsername_ShouldGetSearchedUser()
        {
            var response = (OkObjectResult)_tweetsController.SearchByUsername("sh");
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }

        [Test]
        public void GetAllTweetsOfUser_UserTryGetAllTweetsOfUser_ShouldGetAllTweetsOfUser()
        {
            var response = (OkObjectResult)_tweetsController.GetAllTweetsOfUser("shaqiq");
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }
        [Test]
        public void PostTweet_UserTryToPostTweet_ShouldAbleToPostTweet()
        {
            TweetsModel tweetBody = new TweetsModel()
            {
                TweetBody = "Creating new test tweet!",
                TweetTag = "#testTweet",
                TweetTime = DateTime.UtcNow
            };
            var response = (OkObjectResult)_tweetsController.PostTweet("shaqiq", tweetBody);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }
        [Test]
        public void UpdateTweet_UserTryToUpdateTweet_ShouldAbleToUpdateTweet()
        {
            int tweetId = 1;
            TweetsModel tweetBody = new TweetsModel()
            {
                TweetBody = "Updating new test tweet!",
                TweetTime = DateTime.UtcNow
            };
            var response = (OkObjectResult)_tweetsController.UpdateTweet("shaqiq", tweetId,  tweetBody);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }
        [Test]
        public void DeleteTweet_UserTryToDeleteTweet_ShouldAbleToDeleteTweet()
        {
            TweetsModel testTweet = new TweetsModel()
            {
                uniqueId=999,
                userName = "abid",
                TweetBody = "test tweet body for delete check",
                TweetTag = "#testTweet",
                TweetTime = DateTime.UtcNow
            };

            _tweetsCollection.InsertOne(testTweet);


            int tweetId = 999;
            var response = (OkObjectResult)_tweetsController.DeleteTweet("abid",tweetId);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
        }
        [Test]
        public void LikeTweet_UserTryToLikeTweet_ShouldAbleToLikeTweet()
        {
            string tweetId = "62f00b9b9138066b08abdb58";
            var response = (OkObjectResult)_tweetsController.LikeTweet("sha",tweetId);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
            Assert.IsTrue(response.Value.Equals("Liked"));
        }

        [Test]
        public void ReplyTweet_UserTryToReplyTweet_ShouldAbleToReplyTweet()
        {
            string tweetId = "62f00b9b9138066b08abdb58";
            ReplyModel reply = new ReplyModel();
            reply.Reply = "New reply from test data!";
            var response = (OkObjectResult)_tweetsController.ReplyTweet("sha", tweetId,  reply);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.OK);
            Assert.IsTrue(response.Value.Equals("Reply successful"));
        }









        [TearDown]
        public void TearDown()
        {
            // Delete testUser
            var userFilter = _usersFilterBuilder.Where(tweet => tweet.userName == "abirdas");
            _usersCollection.DeleteOne(userFilter);

            // Reset test data user password
            userFilter = _usersFilterBuilder.Where(tweet => tweet.userName == "shaqiq");
            var userUpdate = _usersUpdateBuilder.Set("Password", "raihan");
            _usersCollection.UpdateOne(userFilter, userUpdate);

            // Delete testTweet
            var tweetFilter = _tweetsFilterBuilder.Where(tweet => tweet.userName == "shaqiq" && tweet.TweetTag.Contains("#testTweet"));
            _tweetsCollection.DeleteOne(tweetFilter);

            // Reset tweet body
            tweetFilter = _tweetsFilterBuilder.Where(tweet => tweet.id == "62f00b9b9138066b08abdb58");
            var tweetUpdate = _tweetsUpdateBuilder.Set("TweetBody", "test tweet 001");
            _tweetsCollection.UpdateOne(tweetFilter, tweetUpdate);


            // Reset like user
            var likedUser = _tweetsCollection.AsQueryable()
                                             .Where(tweet => tweet.id == "62f00b9b9138066b08abdb58")
                                             .ToList().ElementAt(0).LikedUser;
            ;
            if (likedUser.Contains("sha"))
            {
                likedUser.Remove("sha");
            }
            tweetFilter = _tweetsFilterBuilder.Where(tweet => tweet.id == "62f00b9b9138066b08abdb58");
            tweetUpdate = _tweetsUpdateBuilder.Set("LikedUser", likedUser);
            _tweetsCollection.UpdateOne(tweetFilter, tweetUpdate);

            // Reset user reply 
            var tweetReply = _tweetsCollection.AsQueryable()
                                              .Where(tweet => tweet.id == "62f00b9b9138066b08abdb58")
                                              .ToList().ElementAt(0).TweetReply;
            foreach (var reply in tweetReply)
            {
                if ( reply.Reply == "New reply from test data!")
                {
                    tweetReply.Remove(reply);
                    break;
                }
            }
            tweetFilter = _tweetsFilterBuilder.Where(tweet => tweet.id == "62f00b9b9138066b08abdb58");
            tweetUpdate = _tweetsUpdateBuilder.Set("TweetReply", tweetReply);
            _tweetsCollection.UpdateOne(tweetFilter, tweetUpdate);
        }


    }

}

