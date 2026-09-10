using Microsoft.Extensions.Configuration;
using Xunit;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Services;

namespace Tests.Unit
{
    public class AiProviderResolverTests
    {
        private static IConfiguration BuildConfig(string? aiProvider) =>
            new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["AI_PROVIDER"] = aiProvider
            }).Build();

        [Fact]
        public void GetConfiguredProvider_Openai_ReturnsOpenai()
        {
            Assert.Equal("OPENAI", AiProviderResolver.GetConfiguredProvider(BuildConfig("OPENAI")));
        }

        [Fact]
        public void GetConfiguredProvider_LowerCase_IsNormalizedToUpperInvariant()
        {
            Assert.Equal("OPENAI", AiProviderResolver.GetConfiguredProvider(BuildConfig("openai")));
        }

        [Fact]
        public void GetConfiguredProvider_NotSet_DefaultsToAzureOpenai()
        {
            Assert.Equal("AZURE_OPENAI", AiProviderResolver.GetConfiguredProvider(BuildConfig(null)));
        }

        [Fact]
        public void GetSuccessOrigin_Openai_ReturnsOpenaiOrigin()
        {
            Assert.Equal(OrigenAnalisis.OPENAI, AiProviderResolver.GetSuccessOrigin(BuildConfig("OPENAI")));
        }

        [Fact]
        public void GetSuccessOrigin_AzureOpenai_ReturnsAzureOpenaiOrigin()
        {
            Assert.Equal(OrigenAnalisis.AZURE_OPENAI, AiProviderResolver.GetSuccessOrigin(BuildConfig("AZURE_OPENAI")));
        }
    }
}
