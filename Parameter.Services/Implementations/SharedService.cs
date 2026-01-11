using Amazon.Runtime.CredentialManagement;

namespace Parameter.Services.Implementations
{
	public class SharedService
	{
		public List<string> GetAllProfiles()
		{
			var sharedFile = new SharedCredentialsFile();

			return [.. sharedFile.ListProfiles().Select(p => p.Name)];
		}
	}
}