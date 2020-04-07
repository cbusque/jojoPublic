// Install the C# / .NET helper library from twilio.com/docs/csharp/install

using jojo;
using System;
using System.Collections.Generic;
using Google.Apis.Calendar.v3.Data;

class Program
{
    // args can be gmail account + password
    // whats next for jojoapp
    // 1. automate deployment -> script 
    // 2. find a way to have multiple client easely
        // 2.1 change hardcoded variable in code
    // 3. port it to jojoapi to receive sms ! 
    // 4. find a better way to run it each day

    static void Main(string[] args)
    {
        string myEmailAdress = "jostxei@hotmail.com";
        string myPhoneNumber = "+15149933101";

        // return only 50 meeting from tomorrow 7am to 20pm
        Events rendezVousList = GoogleCalendarManager.Instance.GetCalendarInfoTutorial();

        foreach (var rendezVous in rendezVousList?.Items)
        {
            if (rendezVous?.Attendees == null)
                continue;

            foreach (var client in rendezVous?.Attendees)
            {
                if (client.Email != myEmailAdress)
                {
                    string ClientPhoneNumber = GoogleContactManager.Instance.GetContactPhoneNumber(client.Email);
                    string ClientName = GoogleContactManager.Instance.GetContactName(client.Email);
                    if (ClientName != string.Empty)
                        SmsSender.Instance.SendSMS(ClientPhoneNumber, ClientName, rendezVous); 
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
