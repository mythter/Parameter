using Amazon.Runtime;

namespace Parameter.Services.Interfaces
{
	public interface IAwsProfilesService
	{
		string GetDefaultCredentialsPath();

		List<string> GetAllSharedProfiles(string? customPath = null);

		List<string> GetAllNetEncryptedProfiles();

		public AWSCredentials? GetSharedProfileCredentials(string profileName, string? customPath = null);

		public AWSCredentials? GetNetEncryptedCredentials(string profileName);

	}
}
