using IdentityServer.Web.Models;
using Newtonsoft.Json;

namespace IdentityServer.Web.Helpers
{
    public static class ThirdPartyIdentityHelper
    {
        public static IdentityConfigurationModel GetConfigurationModel(string thirdPartyIdentityName)
        {
            
            using (StreamReader r = new StreamReader(Directory.GetCurrentDirectory() + "/Constants/thirdPartyIdentityConfiguration.json"))
            {
                string json = r.ReadToEnd();
                List<IdentityConfigurationModel> configurationModels = JsonConvert.DeserializeObject<List<IdentityConfigurationModel>>(json);
                return configurationModels.First(x => x.ThirdPartyIdentityName == thirdPartyIdentityName);
            }

        }
    }
}
