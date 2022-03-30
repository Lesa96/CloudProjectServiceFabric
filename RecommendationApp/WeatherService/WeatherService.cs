using Common;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace WeatherService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WeatherService : StatelessService , IWeatherService
    {
        //https://weatherstack.com/
        private const string api = "http://api.weatherstack.com/current?access_key=93602e2d7324bc707a86b984e679c60e&query=";
        public WeatherService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>(1)
            {
                new ServiceInstanceListener(context=> this.CreateWcfCommunicationListener(context), "WeatherServiceEndpoint")
            };
        }

        private ICommunicationListener CreateWcfCommunicationListener(StatelessServiceContext context)
        {
            string host = context.NodeContext.IPAddressOrFQDN;
            var endpointConfig = context.CodePackageActivationContext.GetEndpoint("WeatherServiceEndpoint");
            int port = endpointConfig.Port;
            var scheme = endpointConfig.Protocol.ToString();
            string uri = string.Format(CultureInfo.InvariantCulture, "net.{0}://{1}:{2}/WeatherServiceEndpoint", scheme, host, port);

            var listener = new WcfCommunicationListener<IWeatherService>(
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

        public async Task<string> GetWeatherForLocation(string location)
        {
            //https://weatherstack.com/documentation
            //http://api.weatherstack.com/current?access_key=93602e2d7324bc707a86b984e679c60e&query=Novi%20Sad
            location = location.Replace(" ", "%20");
            //Belgrade
            HttpClient httpClient = new HttpClient();
            string returnString = "";

            HttpResponseMessage response = await httpClient.GetAsync(api + location);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    returnString = await response.Content.ReadAsStringAsync();
                    WeatherResposne weatherResposne = WeatherResposne.FromJson(returnString);
                    returnString = "";
                    foreach (string item in weatherResposne.Current.WeatherDescriptions)
                    {
                        returnString += item + ", ";
                    }
                    returnString +=  "Temperature: " +  weatherResposne.Current.Temperature.ToString() + " C" + ", Time: " + weatherResposne.Location.Localtime;
                }
                catch (Exception e)
                {

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Error in GetWeatherForLocation: " + e.Message);
                    returnString = "";
                }
                

            }

            return returnString;
        }
    }
}
