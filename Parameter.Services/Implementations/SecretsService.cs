using System.Text;

using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

using Parameter.Services.Interfaces;

namespace Parameter.Services.Implementations
{
	public class SecretsService : ISecretsService
	{
		private readonly IAmazonSecretsManager _secrets;

		public SecretsService(AWSCredentials creds, RegionEndpoint region)
		{
			_secrets = new AmazonSecretsManagerClient(creds, region);
		}

		public async Task<string> GetSecretAsync(string secretName)
		{
			var response = await _secrets.GetSecretValueAsync(
				new GetSecretValueRequest
				{
					SecretId = secretName,
				});

			return response.SecretString ??
				   System.Text.Encoding.UTF8.GetString(response.SecretBinary.ToArray());
		}

		public async Task<Dictionary<string, string>> GetSecretsByPrefixAsync(string prefix)
		{
			var result = new Dictionary<string, string>();
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
				});

				foreach (var secret in response.SecretList
							 .Where(s => s.Name.StartsWith(prefix))
							 .Select(s => s.Name))
				{
					var value = await _secrets.GetSecretValueAsync(
						new GetSecretValueRequest
						{
							SecretId = secret
						});

					result[secret] =
						value.SecretString ??
						Encoding.UTF8.GetString(value.SecretBinary.ToArray());
				}

				nextToken = response.NextToken;
			}
			while (!string.IsNullOrEmpty(nextToken));

			return result;
		}
	}
}
