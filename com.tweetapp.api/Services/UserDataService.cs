using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using com.tweetapp.api.Models;

namespace com.tweetapp.api.Services
{
    public class UserDataService :IUserDataService
    {
        public readonly IConfiguration _configuration;
        public readonly IMongoCollection<UsersModel> _usersCollection;


        public UserDataService(IConfiguration configuration)
        {
            _configuration = configuration;
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("TweetappCon"));
            IMongoDatabase mongoDatabase = dbClient.GetDatabase(_configuration.GetConnectionString("DatabaseName"));
            _usersCollection = mongoDatabase.GetCollection<UsersModel>("Users");

        }


        public string Post(UsersModel user)
        {
            var userList = _usersCollection.AsQueryable().ToList();

            if (user.userName == user.Email)
            {
                return "Username and email id can't be same!";
            }
            else if (userList.Any(userInfo => userInfo.userName == user.userName ||
                                              userInfo.Email == user.Email ||
                                              userInfo.ContactNumber == user.ContactNumber))
            {
                return "User already present!";
            }
            else
            {
                _usersCollection.InsertOne(user);
                return ("User Added Successfully");
            }
        }
             
        public IQueryable<UsersModel> FindUser(string username,int type)
        {
            IQueryable<UsersModel> dbList=Enumerable.Empty<UsersModel>().AsQueryable();
            if (type == 1)
            {
                dbList = _usersCollection.AsQueryable().Where(p => p.userName == username);
            }
            else
            {
                dbList = _usersCollection.AsQueryable().Where(p => p.userName.Contains(username));
            }

            return  dbList;          
            
        }
        
        public bool Valid(Object user,string password)
        {
            bool response=false;                       
            UsersModel u = (UsersModel)user;
            if (u.Password == password)
                response = true;
            
            return (response);
        }


        
    }
}
