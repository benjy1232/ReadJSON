using Newtonsoft.Json;
using MetaBrainz.ListenBrainz;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Net.Http;
using System.Web;

string ROOT = "api.listenbrainz.org";
string MusicHist = "music-history.json";
Console.WriteLine("Enter User ID Token: ");
token = Console.ReadLine();

// This is going to be the area that contains the actual code
string MusicHistory = File.ReadAllText(MusicHist);
List<JsonLayout> History = JsonConvert.DeserializeObject<List<JsonLayout>>(MusicHistory);

string temp = JsonConvert.SerializeObject(History, Formatting.Indented);
List<JsonLayout> YTHistory = new();
List<NeededInformation> YTMHistory = new();
List<NeededInformation> UploadedSongs = new();
Dictionary<string, string> Artist = new();
string csvInformation = File.ReadAllText("music-uploads-metadata.csv");

HttpClient client = new();
client.DefaultRequestHeaders.Authorization = new("Token", token);

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
string ToJson = @"{
    ""listen_type"": ""import"",
    ""payload"":";

File.Create("output.json").Close();
File.WriteAllText("output.json", temp);

int i = 0;
int j = 0;
foreach (JsonLayout x in History)
{
    if (x.header == "YouTube")
    {
        YTHistory.Add(x);
    }
    else
    {
        i += 1;
        NeededInformation ytmInfo = new(x);
        if (ytmInfo.track_metadata.artist_name == "Music Library Uploads")
        {
            if (ytmInfo.track_metadata.track_name != "Ghost" || ytmInfo.track_metadata.track_name != "Prelude")
            {
                string artist;
                Artist.TryGetValue(ytmInfo.track_metadata.track_name, out artist);
                ytmInfo.track_metadata.artist_name = artist;
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
        string toInsert = $"{ToJson} {temp} \n}}";
        File.Create($"JsonOut/{i}.json").Close();
        File.WriteAllText($"JsonOut/{i}.json", toInsert);
        YTMHistory.Clear();
        j += 1;
        StringContent ToSend = new(toInsert, Encoding.UTF8, "application/json");
        if (i > 2686)
        {
            Thread.Sleep(10000);
            i = 0;
        }
        var response = await client.PostAsync($"https://{ROOT}/1/submit-listens", ToSend);

        string result = response.Content.ReadAsStringAsync().Result;

        Console.WriteLine($"{result} \n{j}");
        if (j < 30)
        {
         Console.WriteLine($"{i} songs added");
        }
    }
}

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
    public track_metadata track_metadata { get; set; }
    public long listened_at { get; set; }
    public NeededInformation(JsonLayout temp)
    {
        track_metadata = new();
        track_metadata.track_name = temp.title;
        DateTime _time;
        DateTime.TryParse(temp.time, out _time);
        listened_at = ((DateTimeOffset)_time).ToUnixTimeSeconds();
        if (temp.subtitles != null)
        {
            track_metadata.artist_name = temp.subtitles[0].name;
            if (track_metadata.artist_name.Contains("- Topic"))
            {
                int numChar = track_metadata.artist_name.Count();
                track_metadata.artist_name = track_metadata.artist_name.Remove(numChar - 8, 8);
            }
        }
        else
        {
            track_metadata.artist_name = "";
        }
        if (track_metadata.track_name.Contains("Watched "))
        {
            track_metadata.track_name = track_metadata.track_name.Remove(0, 8);
        }
        if (track_metadata.artist_name == null)
        {
            track_metadata.artist_name = "Unknown";
        }
    }
}

public class track_metadata
{
    public string track_name { get; set; }
    public string artist_name { get; set; }
    public track_metadata()
    {
        track_name = "";
        artist_name = "";
    }
}
