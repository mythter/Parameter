using Amazon;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Parameter.Entites.Enums;
using Parameter.Entites.Models;
using Parameter.Services.Interfaces;

namespace Parameter.Services.Implementations
{
	public class SsmService(AWSCredentials creds, RegionEndpoint region) : ISsmService
	{
		private readonly AmazonSimpleSystemsManagementClient _ssm = new(creds, region);

		public async Task<ParameterModel> GetParameterAsync(string name, bool withDecryption = true)
		{
			var response = await _ssm.GetParameterAsync(
				new GetParameterRequest
				{
					Name = name,
					WithDecryption = withDecryption
				});

			return new ParameterModel()
			{
				Name = response.Parameter.Name,
				Value = response.Parameter.Value,
				Source = SearchSource.SSMParameterStore
			};
		}

		public async Task<List<ParameterModel>> GetParameterByPathAsync(string path, bool withDecryption = true)
		{
			var response = await _ssm.GetParametersByPathAsync(
				new GetParametersByPathRequest
				{
					Path = path,
					WithDecryption = withDecryption
				});

			return [.. response.Parameters.Select(p => new ParameterModel()
				{
					Name = p.Name,
					Value = p.Value,
					Source = SearchSource.SSMParameterStore
				})];
		}
	}
}
