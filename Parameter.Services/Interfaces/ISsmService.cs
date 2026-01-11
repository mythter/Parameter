namespace Parameter.Services.Interfaces
{
	public interface ISsmService
	{
		Task<string> GetParameterAsync(string name, bool withDecryption = true);

		Task<List<string>> GetParameterByPathAsync(string path, bool withDecryption = true);
	}
}
