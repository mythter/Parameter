using Parameter.Entites.Models;

namespace Parameter.Services.Interfaces
{
	public interface ISsmService
	{
		Task<ParameterModel> GetParameterAsync(string name, bool withDecryption = true);

		Task<List<ParameterModel>> GetParameterByPathAsync(string path, bool withDecryption = true);
	}
}
