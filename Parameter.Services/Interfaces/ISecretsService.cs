namespace Parameter.Services.Interfaces
{
	public interface ISecretsService
	{
		Task<string> GetSecretAsync(string secretName);
	}
}
