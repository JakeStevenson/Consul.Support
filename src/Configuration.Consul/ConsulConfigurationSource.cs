using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Configuration.Consul
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        private readonly string _ipAddress;
        private readonly ILoggerFactory _loggerFactory;

        public ConsulConfigurationSource(string IPAddress = "0.0.0.0", ILoggerFactory loggerFactory = null)
        {
            _ipAddress = IPAddress;
            _loggerFactory = loggerFactory;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(_ipAddress, _loggerFactory);
        }
    }
}
