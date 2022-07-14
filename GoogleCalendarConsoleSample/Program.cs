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
    /* Class to demonstrate the use of Calendar events list API */
    class Program
    {
        /* Global instance of the scopes required by this quickstart.
         If modifying these scopes, delete your previously saved token.json/ folder. */
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        static async Task Main(string[] args)
        {
            try
            {
                UserCredential credential;
                // Load client secrets.
                using (var stream =
                       new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    /* The file token.json stores the user's access and refresh tokens, and is created
                     automatically when the authorization flow completes for the first time. */
                    string credPath = "token.json";
                    var clentSecrets = GoogleClientSecrets.FromStream(stream).Secrets;

                    /*credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        clentSecrets,
                        Scopes,
                        "accountName",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;*/
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                //credential.Flow.
                string oldToken = "ya29.A0AVA9y1u2sfcHCD3hgmRGS3hnzWlBgAWTeuUGMQU4EWWlpZ8oyk5FENMYYrRHz91nOqhHQtXVf6EcSbdjtQ30buHeKss6pFWu9Ud5g2u3JDbX5ENSOD_rl7nO4YmZh_IKPD8xjGIwFToC5DoiZ5RIxLcnXCfRYUNnWUtBVEFTQVRBU0ZRRTY1ZHI4NDJpcXZ4WHdpVmZzRF9oM2xTQmJZQQ0163";



                string clientId = "217994279280-lnq6di8dn6n5g899rhr77stthoi882oo.apps.googleusercontent.com";
                string clientSecret = "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";
                string refreshToken = "1//0eCFCkEHkxT1gCgYIARAAGA4SNwF-L9Ir1SrfmsO1k95NY0_0nqL-G-ZekU5y2ljqbZazUpAfNr0_BuwNK7MiqvwXGtC_2kQXD7k";
                string url = "https://oauth2.googleapis.com/token";

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


                var refreshedAccessToken = payload.Value<string>("access_token");
                var refreshedRefreshToken = payload.Value<string>("refresh_token");
                var refreshedExpiresIn = payload.Value<string>("expires_in");
                var refreshedIdToken = payload.Value<string>("id_token");

                var accessToken = refreshedAccessToken;
                GoogleCredential cred = GoogleCredential.FromAccessToken(accessToken);
                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = cred
                });

                // Create Google Calendar API service.
                /*var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });*/

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
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task<string> GetRefreshToken()
        {
            string clientId = "217994279280-lnq6di8dn6n5g899rhr77stthoi882oo.apps.googleusercontent.com";
            string clientSecret = "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";
            string refreshToken = "1//0eCFCkEHkxT1gCgYIARAAGA4SNwF-L9Ir1SrfmsO1k95NY0_0nqL-G-ZekU5y2ljqbZazUpAfNr0_BuwNK7MiqvwXGtC_2kQXD7k";
            string url = "https://oauth2.googleapis.com/token";

            HttpClient client = new HttpClient();

            RefreshTokenRequest request = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };

            request.ClientId = clientId;
            request.ClientSecret = clientSecret;

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
            var refreshedAccessToken = payload.Value<string>("access_token");
            var refreshedRefreshToken = payload.Value<string>("refresh_token");
            var refreshedExpiresIn = payload.Value<string>("expires_in");
            var refreshedIdToken = payload.Value<string>("id_token");

            return refreshedAccessToken;
        }
    }
}