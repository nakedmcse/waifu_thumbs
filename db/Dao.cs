using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace Thumbnails
{
    public class Dao
    {
        public enum DbType
        {
            Postgres,
            SQLite
        }
        
        public string connection;
        public DbType dbType;
        public string redisUri;
        public readonly DbContext context;
        private readonly string baseLocation;
        
        public Dao(string baseLocation)
        {
            this.baseLocation = baseLocation;
            GetConfig();
            this.context = new DBContext(this.connection, this.dbType);
        }

        private void GetConfig()
        {
            try
            {
                var pgenv = Path.Combine(baseLocation, "postgres.env");
                var env = Path.Combine(baseLocation, ".env");
                var config = DotEnv.Read(options: new DotEnvOptions(
                    envFilePaths: new[] { env, pgenv },
                    ignoreExceptions: false));
                
                if (config["DATABASE_TYPE"] == "postgres")
                {
                    connection = $"Server=localhost;Port={config["POSTGRES_PORT"]};Database={config["POSTGRES_DB"]};Username={config["POSTGRES_USER"]};Password={config["POSTGRES_PASSWORD"]}";
                    dbType = DbType.Postgres;
                }
                else
                {
                    connection = $"Data Source={baseLocation}main.sqlite";
                    dbType = DbType.SQLite;
                }
                
                redisUri = config["REDIS_URI"].Replace("redis://", "");
            }
            catch (Exception ex)
            {
                throw new Exception("Configuration file could not be loaded. " + ex.Message);
            }
        }
        
        public List<int> GetAlbumFileIds(string token)
        {
            return this.context.Set<Model.file_upload_model>().Where(x => x.albumToken == token)
                .Select(x => x.id).ToList();
        }
        
        public List<int> GetCachedFileIds(List<int> fileIds)
        {
            return this.context.Set<Model.thumbnail_cache_model>().Where(x => fileIds.Contains(x.fileId))
                .Select(x => x.fileId).ToList();
        }
        
        public List<Dto.FileWithMediaType> GetFilePaths(List<int> fileIds)
        {
            return this.context.Set<Model.file_upload_model>().Where(f => fileIds.Contains(f.id))
                .Select(x => new Dto.FileWithMediaType()
                {
                    fileId = x.id,
                    filename = baseLocation + "files/" + x.fileName + "." + x.fileExtension,
                    mediaType = x.mediaType
                }).ToList();
        }
        
        public async Task<bool> SaveThumnbails(List<Tuple<int, string>> thumbnails)
        {
            var redis = ConnectionMultiplexer.Connect(redisUri);
            var db = redis.GetDatabase();
            Utils.Log("Connected to Redis");
            
            var now = DateTime.UtcNow;
            await this.context.Set<Model.thumbnail_cache_model>().AddRangeAsync(
                thumbnails.Select(x => new Model.thumbnail_cache_model()
                {
                    fileId = x.Item1,
                    createdAt = now,
                    updatedAt = now,
                    data = x.Item2
                })
            );
            await this.context.SaveChangesAsync();
            Utils.Log("Saved thumbnails to database");
            
            db.StringSet(thumbnails.Select(x => 
                new KeyValuePair<RedisKey,RedisValue>($"thumbnail:{x.Item1.ToString()}",Convert.FromBase64String(x.Item2))).ToArray());
            Utils.Log("Saved thumbnails to Redis");
            return true;
        }
    }
};