using System.Net.Http.Json;

namespace uniBuddyAPI.Services
{
    public class RealTimeDbService
    {
        public HttpClient Client { get; } //exposing httpclient for controllers to call firebase endpoints

        public RealTimeDbService(IConfiguration configuration, ILogger<RealTimeDbService> logger)
        {
            var firebaseDatabaseUrl = configuration["Firebase:DatabaseUrl"] //firebase url from appsettings.json
                ?? throw new InvalidOperationException("Missing Firebase:DatabaseUrl"); //error checking/handling

            Client = new HttpClient { BaseAddress = new Uri(firebaseDatabaseUrl.TrimEnd('/')) }; //making sure theres no unnecessary slash to ruin the url
           // logger.LogInformation("RealTimeDbService ready at {Url}", Client.BaseAddress);
        }
    }
}