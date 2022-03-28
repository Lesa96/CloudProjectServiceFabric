using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IWeatherService
    {
        [OperationContract]
        Task<string> GetWeatherForLocation(string location);
    }
}
