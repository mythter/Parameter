using Amazon;
using Amazon.Runtime;

using Parameter.Services.Implementations;
using Parameter.Services.Interfaces;

namespace Parameter.Services
{
	public class ParameterServiceFactory : IParameterServiceFactory
	{
		public ISsmService CreateSsmService(AWSCredentials creds, RegionEndpoint region)
		{
			return new SsmService(creds, region);
		}

		public ISecretsService CreateSecretsService(AWSCredentials creds, RegionEndpoint region)
		{
			return new SecretsService(creds, region);
		}
	}
}
