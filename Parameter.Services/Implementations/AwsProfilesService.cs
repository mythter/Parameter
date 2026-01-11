using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Parameter.Services.Interfaces;

namespace Parameter.Services.Implementations
{
	public class AwsProfilesService : IAwsProfilesService
	{
		public string GetDefaultCredentialsPath()
		{
			return new SharedCredentialsFile().FilePath;
		}

		public List<string> GetAllSharedProfiles(string? customPath = null)
		{
			var sharedFile = customPath is null
				? new SharedCredentialsFile()
				: new SharedCredentialsFile(customPath);

			return [.. sharedFile.ListProfiles().Select(p => p.Name)];
		}

		public List<string> GetAllNetEncryptedProfiles()
		{
			var chain = new CredentialProfileStoreChain();

			return [.. chain.ListProfiles().Select(p => p.Name)];
		}

		public AWSCredentials? GetSharedProfileCredentials(string profileName, string? customPath = null)
		{
			var sharedFile = new SharedCredentialsFile();

			if (!sharedFile.TryGetProfile(profileName, out var profile))
				return null;

			return AWSCredentialsFactory.GetAWSCredentials(profile, sharedFile);
		}

		public AWSCredentials? GetNetEncryptedCredentials(string profileName)
		{
			var chain = new CredentialProfileStoreChain();

			if (chain.ListProfiles().FirstOrDefault(p => p.Name == profileName) is not { } profile)
				return null;

			return AWSCredentialsFactory.GetAWSCredentials(profile, chain);
		}
	}
}
