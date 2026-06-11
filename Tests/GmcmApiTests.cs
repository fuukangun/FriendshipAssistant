using System.Reflection;
using Xunit;

namespace FriendshipAssistant.Tests;

public sealed class GmcmApiTests
{
    [Fact]
    public void Register_UsesSmapiManifestParameterForApiMapping()
    {
        MethodInfo? register = typeof(IGenericModConfigMenuApi).GetMethod(nameof(IGenericModConfigMenuApi.Register));

        Assert.NotNull(register);
        Assert.Equal("StardewModdingAPI.IManifest", register.GetParameters()[0].ParameterType.FullName);
    }

    [Fact]
    public void AddBoolOption_UsesSmapiManifestParameterForApiMapping()
    {
        MethodInfo? addBoolOption = typeof(IGenericModConfigMenuApi).GetMethod(nameof(IGenericModConfigMenuApi.AddBoolOption));

        Assert.NotNull(addBoolOption);
        Assert.Equal("StardewModdingAPI.IManifest", addBoolOption.GetParameters()[0].ParameterType.FullName);
    }
}
