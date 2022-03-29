using Client.Models;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private const string fabricService = "fabric:/RecommendationApp/";


        [HttpGet]
        [Route("get")]
        //http://localhost:8819/home/get
        public async Task<IActionResult> Index()
        {
            List<Recommendation> recomendations = await GetRecommendations();

            return View("Index", recomendations);

        }

        private async Task<List<Recommendation>> GetRecommendations()
        {
            List<Recommendation> recomendations = new List<Recommendation>();
            FabricClient fabricClient = new FabricClient();
            var binding = WcfUtility.CreateTcpClientBinding();

            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(fabricService + "RecommendationService"));
            int partitionSelect = 0;

            for (int i = 0; i < partitions.Count; i++)
            {
                ServicePartitionClient<WcfCommunicationClient<IRecommendationService>> servicePartitionClient = new ServicePartitionClient<WcfCommunicationClient<IRecommendationService>>
                    (
                     new WcfCommunicationClientFactory<IRecommendationService>(clientBinding: binding),
                     new Uri(fabricService + "RecommendationService"),
                     new ServicePartitionKey(partitionSelect % partitions.Count)
                     );
                recomendations = await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.GetRecommendations());

            }

            return recomendations;
        }

        private async Task AddRecommendationInDb(Recommendation recommendation)
        {
            FabricClient fabricClient = new FabricClient();
            var binding = WcfUtility.CreateTcpClientBinding();

            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(fabricService + "RecommendationService"));
            int partitionSelect = 0;

            for (int i = 0; i < partitions.Count; i++)
            {
                ServicePartitionClient<WcfCommunicationClient<IRecommendationService>> servicePartitionClient = new ServicePartitionClient<WcfCommunicationClient<IRecommendationService>>
                    (
                     new WcfCommunicationClientFactory<IRecommendationService>(clientBinding: binding),
                     new Uri(fabricService + "RecommendationService"),
                     new ServicePartitionKey(partitionSelect % partitions.Count)
                     );
                await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.AddRecomendation(recommendation));

            }
        }

        [HttpPost]
        [Route("post")]
        public async Task<IActionResult> AddRecommendation([FromForm]string place , [FromForm] string details, [FromForm] DateTime arrangmentDate)
        {
            Recommendation recommendation = new Recommendation() { Id = Guid.NewGuid(), Place = place, Details = details, ArrangmentDate = arrangmentDate };
            string weather = await this.GetWeather(place);
            recommendation.Weather = weather;

            await AddRecommendationInDb(recommendation);

            return await Index();
        }

        [HttpGet]
        [Route("refresh")]
        public async Task<ActionResult> GetRefresh()
        {
            List<Recommendation> recomendations = await GetRecommendations();
            string weather = "";
            foreach (Recommendation rec in recomendations)
            {
                weather = await this.GetWeather(rec.Place);
                if (weather != null && weather != "")
                {
                    rec.Weather = weather;
                    Recommendation recommendation = new Recommendation() { Id = Guid.NewGuid(), Place = rec.Place, Details = rec.Details, ArrangmentDate = rec.ArrangmentDate, Weather = weather, To = rec.To };
                    await AddRecommendationInDb(recommendation);
                }
                else
                {
                    rec.Weather = "No data available";
                }
            }

            return View("Index", recomendations);
        }

        [HttpGet]
        [Route("history")]
        public async Task<IActionResult> GetHistory(string place)
        {
            List<Recommendation> recomendations = new List<Recommendation>();
            FabricClient fabricClient = new FabricClient();
            var binding = WcfUtility.CreateTcpClientBinding();

            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(fabricService + "RecommendationService"));
            int partitionSelect = 0;

            for (int i = 0; i < partitions.Count; i++)
            {
                ServicePartitionClient<WcfCommunicationClient<IRecommendationService>> servicePartitionClient = new ServicePartitionClient<WcfCommunicationClient<IRecommendationService>>
                    (
                     new WcfCommunicationClientFactory<IRecommendationService>(clientBinding: binding),
                     new Uri(fabricService + "RecommendationService"),
                     new ServicePartitionKey(partitionSelect % partitions.Count)
                     );
                recomendations = await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.GetHistoryRecommendations(place));

            }
            ViewBag.history = recomendations;

            return await Index();
        }



        private async Task<string> GetWeather(string city = "Belgrade")
        {
            string weather = "No data";
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpoint = new EndpointAddress("net.tcp://localhost:9111/WeatherServiceEndpoint");
            using (var myChannelFactory = new ChannelFactory<IWeatherService>(binding, endpoint))
            {
                try
                {
                    var client = myChannelFactory.CreateChannel();
                    weather = await client.GetWeatherForLocation(city);

                    ((ICommunicationObject)client).Close();
                    myChannelFactory.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in GetWeather controller: " + e.Message);
                }
            }

            return weather;

        }

        [HttpGet]
        [Route("db")]
        public async Task<IActionResult> Getdb()
        {
            Recommendation recommendation = new Recommendation();
            recommendation.Id = Guid.NewGuid();
            recommendation.ArrangmentDate = DateTime.Now;
            recommendation.To = recommendation.ArrangmentDate.AddDays(365);
            recommendation.Place = "Kragujevac";
            recommendation.Details = "Lep grad";

            var binding = new NetTcpBinding(SecurityMode.None);
            var endpoint = new EndpointAddress("net.tcp://localhost:9333/DatabaseServiceEndpoint");
            using (var myChannelFactory = new ChannelFactory<IDatabaseService>(binding, endpoint))
            {
                try
                {
                    var client = myChannelFactory.CreateChannel();
                    await client.AddRecommendation(recommendation);

                    var recom = await client.GetAllRecommendations();

                    ((ICommunicationObject)client).Close();
                    myChannelFactory.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in GetWeather Getdb: " + e.Message);
                }
            }

            return View("Index");

        }
    }
}
