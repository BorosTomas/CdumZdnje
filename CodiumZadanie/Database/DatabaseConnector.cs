using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace CodiumZadanie.Database
{
    internal class DatabaseConnector
    {
        private string ConnectionString = @"Server=.\sqlExpress;Database=CodiumTestDb;User Id=test;Password=Test*11;MultipleActiveResultSets=True";
        private SqlConnection SqlConnector;
        private Random ApiSleeper = new Random();
        private int SleepTime;
        private bool ApiSlower =true;

        public int CreatedEventCount = 0;
        public int CreatedOddCount = 0;
        public int UpdatedEventCount = 0;
        public int UpdatedOddCount = 0;

        internal void Connect()
        {
            SqlConnector = new SqlConnection(ConnectionString);
            try
            {
                SqlConnector.Open();
                Console.WriteLine("Connected to database " + SqlConnector.Site);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            SleepTime = ApiSleeper.Next(0, 10001);
        }

        public bool CreateEvent(EventData evnt)
        {
            if (ApiSlower)
                Thread.Sleep(SleepTime);

            var eventId = evnt.ProviderEventID;

            var data = new Dictionary<string, object>()
            {
                ["ProviderEventID"] = eventId,
                ["EventName"] = evnt.EventName,
                ["EventDate"] = evnt.EventDate,
            };

            SendDataToDb("pEvent_Create", data);
            CreatedEventCount++;
            Console.WriteLine(String.Format("{0} DbConnector: New event created: {1}", DateTime.Now, eventId));

                if (evnt.OddsList != null)
                CreateOdds(evnt.OddsList, eventId);

            return true;
        }

        private void CreateOdds(IList<OddsData> oddsList, int eventId)
        {
            foreach (var odds in oddsList)
            {
                var data = new Dictionary<string, object>()
                {
                    ["ProviderEventID"] = eventId,
                    ["ProviderOddsID"] = odds.ProviderOddsID,
                    ["OddsName"] = odds.OddsName,
                    ["OddsRate"] = odds.OddsRate,
                    ["Status"] = odds.Status
                };

                SendDataToDb("pOdd_Create", data);
                CreatedOddCount++;
                Console.WriteLine(String.Format("{0} DbConnector: Odd created: {1}", DateTime.Now, odds.ProviderOddsID));
            
            }
        }

        public void UpdateEvent(EventData originalEvent, EventData newEventData)
        {
            if (ApiSlower)
                Thread.Sleep(SleepTime);

            Console.WriteLine(string.Format("Event with ID: {0} already exist, updating changes", originalEvent.ProviderEventID));

            var originalEventId = originalEvent.ProviderEventID;

            if (originalEvent.EventDate != newEventData.EventDate)
                originalEvent.EventDate = newEventData.EventDate;

            var data = new Dictionary<string, object>()
            {
                ["ProviderEventID"] = originalEventId,
                ["EventDate"] = newEventData.EventDate,
            };

            SendDataToDb("pEvent_Update", data);
            UpdatedEventCount++;
            Console.WriteLine(String.Format("{0} DbConnector: Event with ID updated: {1}", DateTime.Now, originalEventId));

            foreach (var newEventOddData in newEventData.OddsList)
            {
                var originalEventOddData = originalEvent.OddsList.FirstOrDefault(x=>x.ProviderOddsID ==  newEventOddData.ProviderOddsID);
               
                if (originalEventOddData != null)
                {
                    if (originalEventOddData.Status != newEventOddData.Status)
                        originalEventOddData.Status = newEventOddData.Status;

                    if (originalEventOddData.OddsRate != newEventOddData.OddsRate)
                        originalEventOddData.OddsRate = newEventOddData.OddsRate;

                    UpdateOdd(originalEventOddData);
                }
            }
        }

        public void UpdateOdd(OddsData odd)
        {
            var data = new Dictionary<string, object>()
            {
                ["ProviderOddsID"] = odd.ProviderOddsID,
                ["OddsRate"] = odd.OddsRate,
                ["Status"] = odd.Status
            };

            SendDataToDb("pOdd_Update", data);
            UpdatedOddCount++;
            Console.WriteLine(String.Format("{0} DbConnector: Odd updated {1}", DateTime.Now, odd.ProviderOddsID));
        }

        private void SendDataToDb(string procName, Dictionary<string, object> recordData)
        {

            using (var command = new SqlCommand(procName, SqlConnector))
            {
                command.CommandType = CommandType.StoredProcedure;
                foreach (var data in recordData)
                {
                    command.Parameters.AddWithValue(data.Key, data.Value);
                }
                try
                {
                    var result = command.ExecuteScalarAsync();
                    Console.WriteLine(DateTime.Now + "SQL Result Id: " + result.Id);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }

}
