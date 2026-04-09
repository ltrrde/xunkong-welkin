using System.IO;
using System.Text.Json;

namespace Xunkong.SnapMetadata;

public class SnapMetadataClient
{
    public SnapMetadataClient(HttpClient httpClient)
    {
        _ = httpClient;
    }

    private static readonly string MetadataFolder = Path.Combine(
        AppContext.BaseDirectory,
        "Snap.Metadata");

    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private static string GetPackageFilePath(string relativePath)
    {
        string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(MetadataFolder, normalizedPath);
    }

    private static async Task<T> ReadLocalJsonAsync<T>(string relativePath)
    {
        string filePath = GetPackageFilePath(relativePath);
        await using FileStream stream = File.OpenRead(filePath);
        T? value = await JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerOptions);
        return value ?? throw new JsonException($"Failed to deserialize metadata file: {filePath}");
    }


    public async Task<SnapMeta> GetSnapMetaAsync()
    {
        const string relativePath = "Genshin/CHS/Meta.json";
        return await ReadLocalJsonAsync<SnapMeta>(relativePath);
    }


    public async Task<SnapAvatarInfo> GetAvatarInfoAsync(string id)
    {
        string relativePath = $"Genshin/CHS/{id}.json";
        return await ReadLocalJsonAsync<SnapAvatarInfo>(relativePath);
    }


    public async Task<List<SnapWeaponInfo>> GetWeaponInfosAsync()
    {
        const string relativePath = "Genshin/CHS/Weapon.json";
        return await ReadLocalJsonAsync<List<SnapWeaponInfo>>(relativePath);
    }




    public async Task<List<SnapGachaEventInfo>> GetGachaEventInfosAsync()
    {
        const string relativePath = "Genshin/CHS/GachaEvent.json";
        return await ReadLocalJsonAsync<List<SnapGachaEventInfo>>(relativePath);
    }



    public async Task<List<SnapAchievementItem>> GetAchievementItemsAsync()
    {
        const string relativePath = "Genshin/CHS/Achievement.json";
        return await ReadLocalJsonAsync<List<SnapAchievementItem>>(relativePath);
    }


    public async Task<List<SnapAchievementGoal>> GetAchievementGoalsAsync()
    {
        const string relativePath = "Genshin/CHS/AchievementGoal.json";
        return await ReadLocalJsonAsync<List<SnapAchievementGoal>>(relativePath);
    }


    public async Task<List<SnapDisplayItem>> GetDisplayItemsAsync()
    {
        const string relativePath = "Genshin/CHS/DisplayItem.json";
        return await ReadLocalJsonAsync<List<SnapDisplayItem>>(relativePath);
    }



    public async Task<List<SnapPromote>> GetPromotesAsync()
    {
        const string relativePath1 = "Genshin/CHS/AvatarPromote.json";
        var list1 = await ReadLocalJsonAsync<List<SnapPromote>>(relativePath1);
        const string relativePath2 = "Genshin/CHS/WeaponPromote.json";
        var list2 = await ReadLocalJsonAsync<List<SnapPromote>>(relativePath2);
        return list1.Concat(list2).ToList();
    }


    public async Task<List<SnapMaterial>> GetMaterialsAsync()
    {
        const string relativePath = "Genshin/CHS/Material.json";
        return await ReadLocalJsonAsync<List<SnapMaterial>>(relativePath);
    }


}



