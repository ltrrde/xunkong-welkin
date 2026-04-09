using System.IO;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;

namespace Xunkong.SnapMetadata;

public class SnapMetadataClient
{
    // 没有release只能直接下了
    private const string MetadataZipUrl = "https://api.github.com/repos/wangdage12/Snap.Metadata/zipball/main";
    // 需要一个好心人提供更新检查api(
    private const string MetadataCommitHashUrl = "https://api.github.com/repos/wangdage12/Snap.Metadata/git/ref/heads/main";

    private readonly HttpClient _httpClient;

    public SnapMetadataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static readonly string MetadataFolder = Path.Combine(
        AppContext.BaseDirectory,
        "Snap.Metadata");

    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private async Task EnsureMetadataDownloadedAsync()
    {
        Directory.CreateDirectory(MetadataFolder);
        string tempZipPath = Path.Combine(Path.GetTempPath(), $"Snap.Metadata.{Guid.NewGuid():N}.zip");
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, MetadataZipUrl);
            request.Headers.Add("User-Agent", "Xunkong-Welkin/1.5.1");
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using (var responseStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = File.Create(tempZipPath))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            ZipFile.ExtractToDirectory(tempZipPath, MetadataFolder, overwriteFiles: true);
        }
        finally
        {
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }
        }
    }

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

    public async Task<string> GetLatestMetadataHashAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, MetadataCommitHashUrl);
        request.Headers.Add("User-Agent", "Xunkong-Welkin/1.5.1");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var refResponse = await response.Content.ReadFromJsonAsync<RefResponse>(JsonSerializerOptions);
        return refResponse?.Object?.Sha;
    }

    public async Task<SnapMeta> GetSnapMetaAsync(bool force = false)
    {
        if (!force) await EnsureMetadataDownloadedAsync();
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

internal class RefResponse
{
    public RefObject Object { get; set; }
}

internal class RefObject
{
    public string Sha { get; set; }
}

