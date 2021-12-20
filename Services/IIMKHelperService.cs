using IMK_web.Models.ModelHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Services
{
    public interface IIMKHelperService
    {
        Task<AzureCountryResultModel> geCountryFromAzureMaps(string latitude, string longtiude);
    }
}
