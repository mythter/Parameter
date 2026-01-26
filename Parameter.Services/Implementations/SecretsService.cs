using System.Text;

using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

using Parameter.Entities.Enums;
using Parameter.Entities.Models;
using Parameter.Services.Interfaces;

namespace Parameter.Services.Implementations
{
	public class SecretsService(AWSCredentials creds, RegionEndpoint region) : ISecretsService
	{
		private readonly AmazonSecretsManagerClient _secrets = new(creds, region);

		public async Task<ParameterModel> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
		{
			var response = await _secrets.GetSecretValueAsync(
				new GetSecretValueRequest
				{
					SecretId = secretName,
				},
				cancellationToken);

			return new ParameterModel()
			{
				Name = response.Name,
				Value = response.SecretString ?? Encoding.UTF8.GetString(response.SecretBinary.ToArray()),
				Source = SearchSource.SecretsManager,
			};
		}

		public async Task<List<ParameterModel>> GetSecretsByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
		{
			var result = new List<ParameterModel>();
			var nextToken = null as string;

			do
			{
				var response = await _secrets.ListSecretsAsync(new ListSecretsRequest
				{
					Filters =
					[
						new Filter() {
							Key = FilterNameStringType.Name,
							Values = [prefix]
						}
					],
					NextToken = nextToken
				},
				cancellationToken);

				foreach (var secret in response.SecretList
							 .Where(s => s.Name.StartsWith(prefix))
							 .Select(s => s.Name))
				{
					var value = await _secrets.GetSecretValueAsync(
						new GetSecretValueRequest
						{
							SecretId = secret
						},
						cancellationToken);

					result.Add(new ParameterModel()
					{
						Name = secret,
						Value = value.SecretString ?? Encoding.UTF8.GetString(value.SecretBinary.ToArray()),
						Source = SearchSource.SecretsManager,
					});
				}

				nextToken = response.NextToken;
			}
			while (!string.IsNullOrEmpty(nextToken));

			return result;
		}
	}
}
