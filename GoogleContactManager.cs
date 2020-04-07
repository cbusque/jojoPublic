using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
//using Google.Apis.People.v1;
//using Google.Apis.People.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace jojo
{
    public sealed class GoogleContactManager
    {
        private List<string> _allAdress = null;
        private IList<Person> _contactList = null;
        private PeopleServiceService _peopleService = null;

        // TODO: jojo must add https://developers.google.com/calendar/quickstart/dotnet
        public static GoogleContactManager Instance { get; } = new GoogleContactManager();
        public IList<Person> ContactList
        {
            get
            {
                if (_contactList == null)
                {
                    _contactList = GoogleContactManager.Instance.GetContact();
                    if (_contactList == null)
                    {
                        _contactList = new List<Person>();
                    }
                    GoogleContactManager.Instance.FixContact();
                    return _contactList;
                }
                else
                    return _contactList;
            }
        }

        public PeopleServiceService Service
        {
            get
            {
                if (_peopleService == null)
                {
                    // Create the service.
                    _peopleService = new PeopleServiceService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GoogleCredentials.Instance.GetGoogleCredentials(new[] { "profile", "https://www.googleapis.com/auth/contacts" }, "me"),
                        ApplicationName = "jojo",
                    });
                    return _peopleService;
                }
                else
                    return _peopleService;
            }
        }

        public List<string> AllContactEmailAdress
        {
            get
            {
                if (_allAdress == null)
                {
                    _allAdress = new List<string>();
                    foreach (Person client in GoogleContactManager.Instance.ContactList)
                    {
                        if (client?.EmailAddresses == null)
                            continue;

                        foreach (EmailAddress emailAdress in client?.EmailAddresses)
                        {
                            _allAdress.Add(emailAdress.Value.ToLower());
                        }
                    }
                    return _allAdress;
                }
                return _allAdress;
            }
        }


        private IList<Person> GetContact()
        {
            PeopleResource.ConnectionsResource.ListRequest peopleRequest = GoogleContactManager.Instance.Service.People.Connections.List("people/me");
            peopleRequest.RequestMaskIncludeField = "person.names,person.emailAddresses,person.PhoneNumbers";
            ListConnectionsResponse connectionsResponse = peopleRequest.Execute();
            IList<Person> contact = connectionsResponse.Connections;
            IList<Person> allContacts = contact;
            while (connectionsResponse.NextPageToken != null)
            {
                peopleRequest.PageToken = connectionsResponse.NextPageToken;
                connectionsResponse = peopleRequest.Execute();
                allContacts = allContacts.Union(connectionsResponse.Connections).ToList();
            }
            return allContacts;
        }

        public string GetContactPhoneNumber(string emailAdress)
        {
            foreach (Person client in GoogleContactManager.Instance.ContactList)
            {
                Person matchingContact = null;
                foreach (var emailadress in client?.EmailAddresses)
                {
                    if (emailadress.Value == emailAdress)
                    {
                        matchingContact = client;
                    }
                }
                if (matchingContact != null)
                {
                    if (client?.PhoneNumbers?.Count > 0)
                    {
                        return client.PhoneNumbers.First().Value.Replace("-",String.Empty).Replace(" ",String.Empty).Replace("(", String.Empty).Replace(")", String.Empty);
                    }
                }
            }

            return "";
        }

        public string GetContactName(string email)
        {
            if (!AllContactEmailAdress.Contains(email.ToLower()))
                return String.Empty;

            foreach (Person client in GoogleContactManager.Instance.ContactList)
            {
                Person matchingContact = null;
                foreach (var emailadress in client?.EmailAddresses)
                {
                    if (emailadress.Value == email)
                    {
                        matchingContact = client;
                    }
                }
                if (matchingContact != null)
                {
                    if (client?.Names?.Count > 0)
                    {
                        return client.Names.First().DisplayName;
                    }
                }
            }

            // How?!
            return String.Empty;
        }

        // We need to parse all contact from jojo, if there is a email, keep it, else add email bidon 
        // so she can find the user in google calendar app
        // email bidon -> maratob445@emailnube.com
        // must be unique !!!! so be sure it is not already include in contact list
        private void FixContact()
        {
            foreach (Person contact in GoogleContactManager.Instance.ContactList)
            {
                if (contact?.EmailAddresses == null || contact?.EmailAddresses?.Count == 0)
                {
                    //fix contact to be able to see it in calendar 
                    // need to push some info
                    List<EmailAddress> emailAddresses = new List<EmailAddress>();
                    EmailAddress emailAdress = new EmailAddress();
                    emailAdress.Value = GenerateRandomUniqueEmail();
                    emailAddresses.Add(emailAdress);
                    contact.EmailAddresses = emailAddresses;

                    PeopleResource.UpdateContactRequest peopleRequest = new PeopleResource.UpdateContactRequest(Service, contact, contact.ResourceName);
                    peopleRequest.UpdatePersonFields = "emailAddresses";
                    var updateContact = peopleRequest.Execute();
                }
            }
        }

        private string GenerateRandomUniqueEmail()
        {
            string newEmail = "";
            do
            {
                // TODO : make them smaller like 5-6 digit each(will be less scary)
                Guid rancomAccount = Guid.NewGuid();
                Guid randomDomain = Guid.NewGuid();
                newEmail = $"{rancomAccount.ToString()}@{randomDomain.ToString()}.com";

            } while (AllContactEmailAdress.Contains(newEmail));

            return newEmail;
        }
    }
}
