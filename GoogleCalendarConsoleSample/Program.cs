using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace CalendarQuickstart
{
    class Program
    {
        static string[] Scopes = { CalendarService.Scope.Calendar };

        static string clientId = "217994279280-lnq6di8dn6n5g899rhr77stthoi882oo.apps.googleusercontent.com";
        static string clientSecret = "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";

        static string scope = "https://www.googleapis.com/auth/calendar";
        static string refreshToken = "1//0ecsnocj5cCw5CgYIARAAGA4SNwF-L9Irq-cMgF2SzilSvX4sejOr1RxnD8g61Ag0Cfzs-CwDDp7XAJR_iAeHCpCRKQjKVVW1L7g";

        static string url = "https://oauth2.googleapis.com/token";

        static async Task Main(string[] args)
        {
            // 1.
            /*InitAccessToken();*/

            // 2.
            /*string accessToken = "ya29.A0AVA9y1tEvVu4Mmce7aqegvetPlmeBngFpR0mmPF91zHzqVAGSDKGVH-023B4iqW979NirA9IEhToEDJgElointcTQsj7bSwFLwODzE3-H9OMsIM0foKbEvGx4lIdb0imGJjZi0dyS7sQagAIaRUO4yeOOlN9YUNnWUtBVEFTQVRBU0ZRRTY1ZHI4YjdGbjRNSENTWjV4bk9WMjQxdU4tUQ0163";
            GetGoogleCalendarEvents(accessToken);*/

            // 3.
            var accessToken = UpdateAccessToken().Result;
            GetGoogleCalendarEvents(accessToken);

        }

        static void InitAccessToken()
        {
            // Load client secrets.
            ClientSecrets clientSecrets = new ClientSecrets()
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            /* The file token.json stores the user's access and refresh tokens, and is created
             automatically when the authorization flow completes for the first time. */
            string credPath = "token.json";

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                Scopes,
                "accountName",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);
        }

        static async Task<string> UpdateAccessToken()
        {
            HttpClient client = new HttpClient();

            var refreshContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            });

            var refreshResponse = await client.PostAsync(url, refreshContent, CancellationToken.None);
            refreshResponse.EnsureSuccessStatusCode();
            var payload = JObject.Parse(await refreshResponse.Content.ReadAsStringAsync());

            Console.WriteLine(payload.ToString());

            var refreshedAccessToken = payload.Value<string>("access_token");
            var refreshedRefreshToken = payload.Value<string>("refresh_token");
            var refreshedExpiresIn = payload.Value<string>("expires_in");
            var refreshedIdToken = payload.Value<string>("id_token");

            Console.WriteLine("refreshedAccessToken: " + refreshedAccessToken);
            Console.WriteLine("refreshedRefreshToken: " + refreshedRefreshToken);
            Console.WriteLine("refreshedExpiresIn: " + refreshedExpiresIn);
            Console.WriteLine("refreshedIdToken: " + refreshedIdToken);

            if (int.TryParse(refreshedExpiresIn, out int expiresInSeconds))
            {
                var refreshedExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
                Console.WriteLine(refreshedExpiresAt);
            }

            return refreshedAccessToken;
        }

        static void GetGoogleCalendarEvents(string accessToken)
        {
            GoogleCredential cred = GoogleCredential.FromAccessToken(accessToken);

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items == null || events.Items.Count == 0)
            {
                Console.WriteLine("No upcoming events found.");
                return;
            }
            foreach (var eventItem in events.Items)
            {
                string when = eventItem.Start.DateTime.ToString();
                if (String.IsNullOrEmpty(when))
                {
                    when = eventItem.Start.Date;
                }
                string whenTimeZone = eventItem.Start.TimeZone.ToString();
                Console.WriteLine("{0} ({1}) ({2})", eventItem.Summary, when, whenTimeZone);
            }
        }
    }
}