using Common;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DatabaseService : StatelessService, IDatabaseService
    {
        private const string connectionString = "mongodb://127.0.0.1:27017";
        private const string databaseName = "recommendation_db";
        private const string recommendationCollectionName = "recommendations";

        private static IMongoCollection<Recommendation> RE_Collection;
        public DatabaseService(StatelessServiceContext context)
            : base(context)
        {
            RE_Collection = ConnectToMongo<Recommendation>(recommendationCollectionName);
        }

        private IMongoCollection<T> ConnectToMongo<T>(in string collection)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            return db.GetCollection<T>(collection);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>(1)
            {
                new ServiceInstanceListener(context=> this.CreateWcfCommunicationListener(context), "DatabaseServiceEndpoint")
            };
        }

        private ICommunicationListener CreateWcfCommunicationListener(StatelessServiceContext context)
        {
            string host = context.NodeContext.IPAddressOrFQDN;
            var endpointConfig = context.CodePackageActivationContext.GetEndpoint("DatabaseServiceEndpoint");
            int port = endpointConfig.Port;
            var scheme = endpointConfig.Protocol.ToString();
            string uri = string.Format(CultureInfo.InvariantCulture, "net.{0}://{1}:{2}/DatabaseServiceEndpoint", scheme, host, port);

            var listener = new WcfCommunicationListener<IDatabaseService>(
                serviceContext: context,
                wcfServiceObject: this,
                listenerBinding: WcfUtility.CreateTcpListenerBinding(),
                address: new EndpointAddress(uri)
            );

            return listener;
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            }
        }



        public async Task<List<Recommendation>> GetAllRecommendations()
        {
            var results = await RE_Collection.FindAsync(_ => true);
            List<Recommendation> returnRecom = new List<Recommendation>();
            var res = results.ToList();

            foreach (Recommendation recommendation in res)
            {
                var available = returnRecom.Where(x => x.Place.Equals(recommendation.Place)).FirstOrDefault();
                if (available != null)
                {
                    returnRecom.Remove(available);                                      
                }
                returnRecom.Add(recommendation);

            }
            return returnRecom;
        }

        public async Task<List<Recommendation>> GetHistoryRecommendation(string place)
        {
            var results = await RE_Collection.FindAsync(x => x.Place.Equals(place));
            return results.ToList();
        }

        public async Task AddRecommendation(Recommendation recommendation)
        {
            try
            {
                await RE_Collection.InsertOneAsync(recommendation);
            }
            catch (Exception e)
            {

                ServiceEventSource.Current.ServiceMessage(this.Context, "Error in adding into db: " + e.Message);
            }
            
        }

        public async Task RemoveRecommendation(Recommendation recommendation)
        {
            await RE_Collection.DeleteOneAsync(Builders<Recommendation>.Filter.Eq("Place", recommendation.Place));
        }
    }
}
