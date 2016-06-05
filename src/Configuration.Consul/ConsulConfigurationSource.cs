using Microsoft.Extensions.Configuration;

namespace Configuration.Consul
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider();
        }
    }
}
