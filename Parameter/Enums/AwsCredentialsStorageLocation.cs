using System.ComponentModel;

namespace Parameter.Enums
{
	public enum AwsCredentialsStorageLocation
	{
		[Description("Shared Credentials File")]
		SharedCredentialsFile,

		[Description(".NET Encrypted Store")]
		NetEncryptedStore,

		[Description("Custom")]
		Custom
	}
}
