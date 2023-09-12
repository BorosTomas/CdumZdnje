using CodiumZadanie.Database;
using CodiumZadanie.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CodiumZadanie
{
    internal class Program
    {
        private static DatabaseConnector DbConnector;
        private static string Path = "c:\\Backup\\zdrojovy_dokument.json";

        static void Main(string[] args)
        {
            Console.WriteLine("This program gets all data from file and put them to DB, do you want to start? Press any key...");
            Console.ReadLine();

            Console.WriteLine("Program started...\nConnecting to database");
            DbConnector = new DatabaseConnector();
            DbConnector.Connect();

            var startTime = DateTime.Now;
            var data = GetAndCompressJsonDocument();
            CreateOrUpdateEvent(data);
            FinishMessage(startTime);

            Console.ReadLine();
        }

        private static void FinishMessage(DateTime startTime)
        {
            var endTime = DateTime.Now;

            Console.WriteLine("Program ended, press any button");
            Console.WriteLine(
                string.Format("Started at {0}," +
                "Ended at {1}" +
                "Created events: {2}, " +
                "added odds under events: {3}, " +
                "updated events: {4}, " +
                "updated odds under events: {5}"
                , startTime
                , endTime
                , DbConnector.CreatedEventCount
                , DbConnector.CreatedOddCount
                , DbConnector.UpdatedEventCount
                , DbConnector.UpdatedOddCount));
        }

        private static void CreateOrUpdateEvent(List<Message> data)
        {
            CreateEvents(data);
            UpdateEvents(data);
        }


        private static void UpdateEvents(List<Message> data)
        {
            var tasks = new List<Task>();
            Semaphore semaphore = new Semaphore(1, 1);
            Parallel.ForEach(data, d =>
            {
                semaphore.WaitOne();
                try
                {
                    var itemExists = EventItemsCache.GetAvailableEvent(d.Event);

                    if (itemExists != null)
                    {
                        if (itemExists != null)
                        {
                            Task task = Task.Run(() => DbConnector.UpdateEvent(itemExists, d.Event));
                            tasks.Add(task);
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            Task.WaitAll(tasks.ToArray());
        }

        private static void CreateEvents(List<Message> data)
        {
            var tasks = new List<Task>();
            Semaphore semaphore = new Semaphore(1, Environment.ProcessorCount);

            Parallel.ForEach(data, d =>
            {
                semaphore.WaitOne();
                try
                {
                    var itemExists = EventItemsCache.GetAvailableEvent(d.Event);

                    if (itemExists == null)
                    {
                        var task = Task.Run(() => DbConnector.CreateEvent(d.Event));
                        tasks.Add(task);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
            Task.WaitAll(tasks.ToArray());
        }


        private static List<Message> GetAndCompressJsonDocument()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var fileReader = File.ReadAllText(Path);

            Console.WriteLine("Decerializing started");
            var option = new JsonSerializerOptions();
            option.Converters.Add(new JsonStringEnumConverter());
            var data = JsonSerializer.Deserialize<List<Message>>(fileReader, option);
            sw.Stop();
            Console.WriteLine($"Deserialized in {sw.ElapsedMilliseconds} ms");
            Console.WriteLine("Getting data..");

            return data;
        }

    }
}

