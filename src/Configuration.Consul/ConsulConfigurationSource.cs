using Microsoft.Extensions.Configuration;

namespace Configuration.Consul
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        private readonly string _ipAddress;

        public ConsulConfigurationSource(string IPAddress = "0.0.0.0")
        {
            _ipAddress = IPAddress;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(_ipAddress);
        }
    }
}
