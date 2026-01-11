using System.ComponentModel;

namespace Parameter.Enums
{
	public enum SearchSource
	{
		[Description("Everywhere")]
		Everywhere,

		[Description("SSM parameter store")]
		SSMParameterStore,

		[Description("Secrets manager")]
		SecretsManager
	}
}
