using Parameter.Entities.Models;

namespace Parameter.Services.Interfaces
{
	public interface ISsmService
	{
		Task<ParameterModel> GetParameterAsync(string name, bool withDecryption = true, CancellationToken cancellationToken = default);

		Task<List<ParameterModel>> GetParameterByPathAsync(string path, bool withDecryption = true, CancellationToken cancellationToken = default);
	}
}
