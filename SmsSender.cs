using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace jojo
{
    public sealed class SmsSender
    {
        const string PathToCheckpoint = "RendezVousCheckpoint";

        public static SmsSender Instance { get; } = new SmsSender();

        // Find your Account Sid and Token at twilio.com/console
        // DANGER! This is insecure. See http://twil.io/secure
        private const string accountSid = "ACe2899cad6b68f439baf06841d786a4e4";
        private const string authToken = "7e4915c6b374af62fd765b9070cd6e1f";
        private const string SenderPhone = "+14502329713";


        public void SendSMS(string phoneNomber,string message)
        {
            TwilioClient.Init(accountSid, authToken);

            var text = MessageResource.Create(
                body: $"{message}",
                from: new Twilio.Types.PhoneNumber(SenderPhone),
                to: new Twilio.Types.PhoneNumber(phoneNomber)
            );

            Console.WriteLine(text.Sid);
        }

        internal void SendSMS(string phoneNumber, string clientName, Event rendezVous)
        {
            if (AlreadyNotified(rendezVous) || phoneNumber == string.Empty)
            {
                Writelog($"not sending sms because phone number is '{phoneNumber}' or alreadynotified is '{AlreadyNotified(rendezVous)}'");
                return;
            }

            Writelog(phoneNumber);
            string message = CreateSMS(clientName, rendezVous);
            TwilioClient.Init(accountSid, authToken);

            var text = MessageResource.Create(
                body: $"{message}",
                from: new Twilio.Types.PhoneNumber(SenderPhone),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            Console.WriteLine(text.Sid);
        }

        private bool AlreadyNotified(Event rendezVous)
        {
            //write in a file
            string rendezVousID = rendezVous.Id + rendezVous.Start.DateTime.ToString();
            List<string> rendezVousNotified = File.Exists(PathToCheckpoint) ? File.ReadAllLines(PathToCheckpoint).ToList<string>() : new List<string>();
            if (rendezVousNotified.Contains(rendezVousID))
            {
                return true;
            }
            //add it 
            using (StreamWriter streamWriter = File.AppendText(PathToCheckpoint))
            {
                streamWriter.WriteLine(rendezVousID);
            }
            return false;
        }

        private string CreateSMS(string ClientName, Event rendezVous)
        {
            EventDateTime startTime = rendezVous.Start;
            string rendezVousStartTime = GetRendezVousStartTime(rendezVous.Start.DateTime);
            string HostName = "Johanne Préville";
            string canceledMessage = "'annulé'";
            string HostPhoneNumber = "450-271-8528";
            string message = $"Bonjour {ClientName},\nVotre Rendez-vous avec {HostName} est demain à {rendezVousStartTime}.\nSi vous désirez annuler, veuillez contacter {HostName} au numéro suivant : {HostPhoneNumber}.";
            Writelog(message.Replace("\n", string.Empty));
            return message;
        }

        private string GetRendezVousStartTime(DateTime? dateTime)
        {
            string minute = null;
            string hour = null;

            // Minute 
            if (dateTime?.Minute == null)
            {
                minute = "00";
            }
            else if (dateTime?.Minute < 10)
            {
                minute = $"0{dateTime?.Minute}";
            }
            else
            {
                minute = dateTime?.Minute.ToString();
            }

            // Hour
            if (dateTime?.Hour == null)
            {
                hour = "00";
            }
            else
            {
                hour = dateTime?.Hour.ToString();
            }
            return $"{hour}:{minute}";
        }

        public void Writelog(string log)
        {
            string logpath = "log.txt";
            using (StreamWriter streamWriter = File.AppendText(logpath))
            {
                streamWriter.WriteLine(log);
            }
        }
    }
}
