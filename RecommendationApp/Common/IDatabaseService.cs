﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IDatabaseService
    {
        [OperationContract]
        Task<List<Recommendation>> GetAllRecommendations();
        [OperationContract]
        Task AddRecommendation(Recommendation recommendation);
    }
}
