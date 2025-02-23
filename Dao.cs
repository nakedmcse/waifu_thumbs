using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace Thumbnails
{
    public class Dao
    {
        public string connection;
        private readonly string baseLocation;
        
        public Dao(string baseLocation)
        {
            this.baseLocation = baseLocation;
            GetPgConfig();
        }
        
        private void GetPgConfig()
        {
            try
            {
                var configPath = Path.Combine(baseLocation, "postgres.env");
                var configLines = File.ReadAllLines(configPath);
                var username = configLines.First(l => l.StartsWith("POSTGRES_USER")).Replace("POSTGRES_USER=", "");
                var password = configLines.First(l => l.StartsWith("POSTGRES_PASSWORD")).Replace("POSTGRES_PASSWORD=", "");
                var db = configLines.First(l => l.StartsWith("POSTGRES_DB")).Replace("POSTGRES_DB=", "");
                var port = configLines.First(l => l.StartsWith("POSTGRES_PORT")).Replace("POSTGRES_PORT=", "");
                connection = $"Server=localhost;Port={port};Database={db};Username={username};Password={password}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetPgConfig: " + ex.Message);
            }
        }
        
        public List<int> GetAlbumFileIds(NpgsqlConnection context, string token)
        {
            List<int> albumIds = new List<int>();
            string query = "SELECT id FROM file_upload_model WHERE \"albumToken\" = @token";
            using (var command = new NpgsqlCommand(query, context))
            {
                command.Parameters.AddWithValue("@token", token);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        albumIds.Add(reader.GetInt32(0));
                    }
                }
            }
            return albumIds;
        }
        
        public List<int> GetCachedFileIds(NpgsqlConnection context, List<int> fileIds)
        {
            List<int> CachedFileIds = new List<int>();
            string query = "SELECT \"fileId\" FROM thumbnail_cache_model WHERE \"fileId\" = ANY(@files)";
            using (var command = new NpgsqlCommand(query, context))
            {
                command.Parameters.AddWithValue("@files", fileIds);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CachedFileIds.Add(reader.GetInt32(0));
                    }
                }
            }
            return CachedFileIds;
        }
        
        public List<Dto.FileWithMediaType> GetFilePaths(NpgsqlConnection context, List<int> fileIds)
        {
            List<Dto.FileWithMediaType> filePaths = new List<Dto.FileWithMediaType>();
            string query = "SELECT \"id\",\"fileName\",\"fileExtension\",\"mediaType\" FROM file_upload_model WHERE \"id\" = ANY(@files)";
            using (var command = new NpgsqlCommand(query, context))
            {
                command.Parameters.AddWithValue("@files", fileIds);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fileWithType = new Dto.FileWithMediaType()
                        {
                            fileId = reader.GetInt32(0),
                            filename = baseLocation + "files/" + reader.GetString(1)+"."+reader.GetString(2),
                            mediaType = reader.GetString(3)
                        };
                        filePaths.Add(fileWithType);
                    }
                }
            }
            return filePaths;
        }
        
        public void SaveThumnbails(NpgsqlConnection context, List<Tuple<int, string>> thumbnails)
        {
            thumbnails.ForEach(x =>
            {
                string query = "INSERT INTO thumbnail_cache_model (\"fileId\", \"data\") VALUES (@fileId, @data);";
                using (var command = new NpgsqlCommand(query, context))
                {
                    command.Parameters.AddWithValue("@fileId", x.Item1);
                    command.Parameters.AddWithValue("@data", x.Item2);
                    command.ExecuteNonQuery();
                }
            });
        }
    }
};