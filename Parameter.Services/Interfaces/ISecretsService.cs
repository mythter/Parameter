using Parameter.Entites.Models;

namespace Parameter.Services.Interfaces
{
	public interface ISecretsService
	{
		Task<ParameterModel> GetSecretAsync(string secretName);

		public Task<List<ParameterModel>> GetSecretsByPrefixAsync(string prefix);
	}
}
