using System.Reflection;
using System.Text.Json;
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

    [Theory]
    [InlineData("i18n/default.json", "backpack", "manual gift suggestion menu")]
    [InlineData("i18n/zh.json", "背包", "手动送礼建议界面")]
    public void Tooltips_ExplainAutoGiftAndStorageBoundaries(
        string path,
        string backpackText,
        string manualMenuText)
    {
        string sourcePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../", path));
        Dictionary<string, string>? translations = JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(sourcePath));

        Assert.NotNull(translations);
        Assert.Contains(backpackText, translations["gmcm.autoGift.tooltip"]);
        Assert.Contains(manualMenuText, translations["gmcm.showStorageItems.tooltip"]);
    }
}
