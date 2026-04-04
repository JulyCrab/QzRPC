using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace QobuzRPC.Services;

public class MusicMetadataService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, (string? AlbumArt, int? Duration)> _cache = new();
    private string? _lastFmApiKey;

    public MusicMetadataService()
    {
        _httpClient = new HttpClient();
    }

    public void SetLastFmApiKey(string? apiKey)
    {
        _lastFmApiKey = apiKey;
    }

    public async Task<(string? AlbumArt, int? Duration)> GetTrackInfoAsync(string trackName, string artistName)
    {
        var cacheKey = $"{artistName}|{trackName}".ToLower();
        
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        // try last.fm first
        var result = await GetLastFmInfoAsync(trackName, artistName);
        
        // if no album art, try itunes
        if (result.AlbumArt == null)
        {
            var itunesArt = await GetITunesAlbumArtAsync(trackName, artistName);
            result = (itunesArt, result.Duration);
        }
        
        // still nothing? try searching just the artist name
        if (result.AlbumArt == null)
        {
            var artistOnlyArt = await GetITunesAlbumArtAsync("", artistName);
            result = (artistOnlyArt, result.Duration);
        }

        _cache[cacheKey] = result;
        return result;
    }

    private async Task<(string? AlbumArt, int? Duration)> GetLastFmInfoAsync(string trackName, string artistName)
    {
        if (string.IsNullOrEmpty(_lastFmApiKey))
            return (null, null);

        try
        {
            // clean up the names a bit - remove featuring artists, etc
            var cleanTrack = CleanTrackName(trackName);
            var cleanArtist = CleanArtistName(artistName);
            
            var url = $"http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key={_lastFmApiKey}&artist={Uri.EscapeDataString(cleanArtist)}&track={Uri.EscapeDataString(cleanTrack)}&format=json";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            string? albumArt = null;
            int? duration = null;

            if (json.RootElement.TryGetProperty("track", out var track))
            {
                // Get album art
                if (track.TryGetProperty("album", out var album) && 
                    album.TryGetProperty("image", out var images))
                {
                    var imageArray = images.EnumerateArray().ToList();
                    for (int i = imageArray.Count - 1; i >= 0; i--)
                    {
                        var image = imageArray[i];
                        if (image.TryGetProperty("#text", out var imageUrl) && !string.IsNullOrEmpty(imageUrl.GetString()))
                        {
                            albumArt = imageUrl.GetString();
                            break;
                        }
                    }
                }

                // Get duration
                if (track.TryGetProperty("duration", out var durationMs))
                {
                    var ms = durationMs.GetInt32();
                    if (ms > 0)
                        duration = ms / 1000;
                }
            }

            return (albumArt, duration);
        }
        catch
        {
            return (null, null);
        }
    }

    private async Task<string?> GetITunesAlbumArtAsync(string trackName, string artistName)
    {
        try
        {
            // clean up names
            var cleanTrack = CleanTrackName(trackName);
            var cleanArtist = CleanArtistName(artistName);
            
            var searchTerm = string.IsNullOrEmpty(cleanTrack) 
                ? cleanArtist 
                : $"{cleanArtist} {cleanTrack}";
            
            var url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(searchTerm)}&media=music&entity=song&limit=3";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            if (json.RootElement.TryGetProperty("resultCount", out var count) && count.GetInt32() > 0)
            {
                if (json.RootElement.TryGetProperty("results", out var results))
                {
                    var resultsArray = results.EnumerateArray().ToList();
                    if (resultsArray.Count > 0)
                    {
                        // try to find the best match
                        foreach (var result in resultsArray)
                        {
                            if (result.TryGetProperty("artworkUrl100", out var artwork))
                            {
                                var artUrl = artwork.GetString()?.Replace("100x100", "600x600");
                                if (!string.IsNullOrEmpty(artUrl))
                                    return artUrl;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // oh well
        }

        return null;
    }
    
    private string CleanTrackName(string trackName)
    {
        if (string.IsNullOrEmpty(trackName))
            return trackName;
        
        // remove stuff in parentheses or brackets (remixes, versions, etc)
        var cleaned = System.Text.RegularExpressions.Regex.Replace(trackName, @"\s*[\(\[].*?[\)\]]", "");
        
        // remove "feat.", "ft.", etc
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+(feat\.|ft\.|featuring).*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return cleaned.Trim();
    }
    
    private string CleanArtistName(string artistName)
    {
        if (string.IsNullOrEmpty(artistName))
            return artistName;
        
        // remove featuring artists
        var cleaned = System.Text.RegularExpressions.Regex.Replace(artistName, @"\s+(feat\.|ft\.|featuring|&|,).*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return cleaned.Trim();
    }
}
