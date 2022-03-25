using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IRecommendationService
    {
        [OperationContract]
        Task<List<Recommendation>> GetRecommendations();
        [OperationContract]
        Task AddRecomendation(Recommendation recommendation);
    }
}
