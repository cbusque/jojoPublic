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
    public sealed class GoogleCredentials
    {
        // TODO: jojo must add https://developers.google.com/calendar/quickstart/dotnet
        public static GoogleCredentials Instance { get; } = new GoogleCredentials();

        public UserCredential GetGoogleCredentials(string[] scopes, string user)
        {

            UserCredential credential;
            using (var stream =
                 new FileStream($"{user}_credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = $"{user}_token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    user, //user for calendar // me for contact but dont work
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            return credential;
        }

    }
}
