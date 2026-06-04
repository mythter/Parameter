using Amazon;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using Parameter.Entities.Enums;
using Parameter.Entities.Models;
using Parameter.Services.Interfaces;

namespace Parameter.Services.Implementations;

public class SsmService(AWSCredentials creds, RegionEndpoint region) : ISsmService
{
	private readonly AmazonSimpleSystemsManagementClient _ssm = new(creds, region);

	public async Task<ParameterModel> GetParameterAsync(string name, bool withDecryption = true, CancellationToken cancellationToken = default)
	{
		var response = await _ssm.GetParameterAsync(
			new GetParameterRequest
			{
				Name = name,
				WithDecryption = withDecryption
			},
			cancellationToken);

		return new ParameterModel()
		{
			Name = response.Parameter.Name,
			Value = response.Parameter.Value,
			Source = SearchSource.SSMParameterStore
		};
	}

	public async Task<List<ParameterModel>> GetParameterByPathAsync(string path, bool recursive = false, bool withDecryption = true, CancellationToken cancellationToken = default)
	{
		var result = new List<ParameterModel>();
		var nextToken = null as string;

		do
		{
			var response = await _ssm.GetParametersByPathAsync(
				new GetParametersByPathRequest
				{
					Path = path,
					Recursive = recursive,
					NextToken = nextToken,
					WithDecryption = withDecryption
				},
				cancellationToken);

			result.AddRange(response.Parameters.Select(p => new ParameterModel()
			{
				Name = p.Name,
				Value = p.Value,
				Source = SearchSource.SSMParameterStore
			}));

			nextToken = response.NextToken;
		}
		while (!string.IsNullOrEmpty(nextToken));

		return result;

	}
}
