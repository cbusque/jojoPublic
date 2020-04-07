using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace jojo
{
    public sealed class GoogleCalendarManager
    {
        // TODO: jojo must add https://developers.google.com/calendar/quickstart/dotnet
        public static GoogleCalendarManager Instance { get; } = new GoogleCalendarManager();

        public Events GetCalendarInfoTutorial()
        {
            string[] Scopes = { CalendarService.Scope.CalendarReadonly };
            string ApplicationName = "Google Calendar API .NET Quickstart";

            UserCredential credential = GoogleCredentials.Instance.GetGoogleCredentials(Scopes, "user");

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");

            // TODO : remove dependacy from when we run this...
            DateTime tomorrow = DateTime.Now.AddDays(1);
            DateTime tomorrow7AM = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 7,0,0);
            request.TimeMin = tomorrow7AM; // demain a 7h

            DateTime tomorrow20 = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 20, 0, 0);
            request.TimeMax = tomorrow20; // demain a 20h
            //
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 50;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }

            return events;
            //Console.Read();

        }




    }
}
