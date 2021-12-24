// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using MetaBrainz;
using MetaBrainz.Common;
using MetaBrainz.ListenBrainz;
using Microsoft.VisualBasic.FileIO;
using System.Text;

string ROOT = "127.0.0.1";
// string MusicHist = Console.ReadLine();
string MusicHist = "music-history.json";

// This is going to be the area that contains the actual code
string MusicHistory = File.ReadAllText(MusicHist);
List<JsonLayout> History = JsonConvert.DeserializeObject<List<JsonLayout>>(MusicHistory);
Console.WriteLine(History.Count);

string temp = JsonConvert.SerializeObject(History, Formatting.Indented);
List<JsonLayout> YTHistory = new();
List<NeededInformation> YTMHistory = new();
List<NeededInformation> UploadedSongs = new();
Dictionary<string, string> Artist = new();
string csvInformation = File.ReadAllText("music-uploads-metadata.csv");

using (TextFieldParser parser = new("music-uploads-metadata.csv"))
{
    parser.TextFieldType = FieldType.Delimited;
    parser.SetDelimiters(",");
    // Reads first line but promptly ignores it cause it is the header
    parser.ReadFields();
    while (!parser.EndOfData)
    {
        string[] fields = parser.ReadFields();

        try
        {
            string SongTitle = fields[0].Trim();
            string artist = fields[2].Trim();

            Artist[SongTitle] = artist;
        }
        catch (Exception)
        {
            Console.WriteLine("Error");
        }

    }
}

File.Create("output.json").Close();
File.WriteAllText("output.json", temp);

int i = 0;
foreach (JsonLayout x in History)
{
    if (x.header == "YouTube")
    {
        YTHistory.Add(x);
    }
    else
    {
        NeededInformation ytmInfo = new(x);
        if (ytmInfo.artist_name == "Music Library Uploads")
        {
            if (ytmInfo.track_name != "Ghost" || ytmInfo.track_name != "Prelude")
            {
                string artist;
                Artist.TryGetValue(ytmInfo.track_name, out artist);
                ytmInfo.artist_name = artist;
                if(artist == null)
                {
                    ytmInfo.artist_name = "";
                }
                UploadedSongs.Add(ytmInfo);
                YTMHistory.Add(ytmInfo);
            }
        }
        else
        {
            YTMHistory.Add(ytmInfo);
        }
    }
    if (Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(YTMHistory)) > 10200)
    {
        temp = JsonConvert.SerializeObject(YTMHistory);
        File.Create($"JsonOut/{i}.json").Close();
        File.WriteAllText($"JsonOut/{i}.json", temp);
        i += 1;
        YTMHistory.Clear();
    }
}

temp = JsonConvert.SerializeObject(YTHistory, Formatting.Indented);

File.Create("YouTube.json").Close();
File.WriteAllText("YouTube.json", temp);

temp = JsonConvert.SerializeObject(YTMHistory, Formatting.Indented);

File.Create("YouTubeMusic.json").Close();
File.WriteAllText("YouTubeMusic.json", temp);

temp = JsonConvert.SerializeObject(UploadedSongs, Formatting.Indented);

File.Create("UploadedYTM.json").Close();
File.WriteAllText("UploadedYTM.json", temp);

public class JsonLayout
{
    public string header { get; set; }
    public string title { get; set; }
    public subtitles[] subtitles { get; set; }
    public JsonLayout()
    {
        header = "";
        title = "";
        time = "";
    }
    public string time { get; set; }
}

public class subtitles
{
    public string name { get; set; }
    public string url { get; set; }
    public subtitles()
    {
        name = "";
        url = "";
    }
}

public class NeededInformation
{
    public string track_name { get; set; }
    public string artist_name { get; set; }
    public long listened_at { get; set; }
    public NeededInformation(JsonLayout temp)
    {
        track_name = temp.title;
        DateTime _time;
        DateTime.TryParse(temp.time, out _time);
        listened_at = ((DateTimeOffset)_time).ToUnixTimeSeconds();
        if (temp.subtitles != null)
        {
            artist_name = temp.subtitles[0].name;
            if (artist_name.Contains("- Topic"))
            {
                int numChar = artist_name.Count();
                artist_name = artist_name.Remove(numChar - 8, 8);
            }
        }
        else
        {
            artist_name = "";
        }
        if (track_name.Contains("Watched "))
        {
            track_name = track_name.Remove(0, 8);
        }
    }
}