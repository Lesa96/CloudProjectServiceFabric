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
            return View(recomendations);

        }

        [HttpPost]
        [Route("post")]
        public async Task AddRecommendation([FromBody] Recommendation recommendation)
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
    }
}
