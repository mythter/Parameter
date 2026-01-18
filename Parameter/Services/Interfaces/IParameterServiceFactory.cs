using Amazon;
using Amazon.Runtime;

namespace Parameter.Services.Interfaces
{
	public interface IParameterServiceFactory
	{
		public ISsmService CreateSsmService(AWSCredentials creds, RegionEndpoint region);

		public ISecretsService CreateSecretsService(AWSCredentials creds, RegionEndpoint region);
	}
}
