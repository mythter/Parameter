using Parameter.Entities.Models;

namespace Parameter.Services.Interfaces
{
	public interface ISecretsService
	{
		Task<ParameterModel> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);

		Task<List<ParameterModel>> GetSecretsByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
	}
}
