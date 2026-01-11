using Amazon;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Parameter.Services.Interfaces;

namespace Parameter.Services.Implementations
{
	public class SsmService : ISsmService
	{
		private readonly IAmazonSimpleSystemsManagement _ssm;

		public SsmService(AWSCredentials creds, RegionEndpoint region)
		{
			_ssm = new AmazonSimpleSystemsManagementClient(creds, region);
		}

		public async Task<string> GetParameterAsync(string name, bool withDecryption = true)
		{
			try
			{
				var response = await _ssm.GetParameterAsync(
						new GetParameterRequest
						{
							Name = name,
							WithDecryption = withDecryption
						});
				return response.Parameter.Value;
			}
			catch (Exception ex)
			{

				throw;
			}
		}

		public async Task<List<string>> GetParameterByPathAsync(string path, bool withDecryption = true)
		{
			try
			{
				var response = await _ssm.GetParametersByPathAsync(
						new GetParametersByPathRequest
						{
							Path = path,
							WithDecryption = withDecryption
						});
				return [.. response.Parameters.Select(p => p.Value)];
			}
			catch (Exception ex)
			{

				throw;
			}
		}
	}
}
