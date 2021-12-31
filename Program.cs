using Newtonsoft.Json;
using Microsoft.VisualBasic.FileIO;
using System.Text;

string ROOT = "api.listenbrainz.org";
string MusicHist = "music-history.json";
Console.WriteLine("Enter User ID Token: ");
string? token = Console.ReadLine();

if(token == null)
{
    Console.WriteLine("No token input, exiting");
    return;
}

string MusicHistory = File.ReadAllText(MusicHist);
List<FromYTJson>? History = JsonConvert.DeserializeObject<List<FromYTJson>>(MusicHistory);

if(History == null)
{
    Console.WriteLine("File not present or empty");
    Console.WriteLine("Exiting Program");
    return;
}

string temp = JsonConvert.SerializeObject(History, Formatting.Indented);
List<ToListenBrainz> YTMHistory = new();
Dictionary<string, string> Artist = new();

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
        string[]? fields = parser.ReadFields();

        if(fields != null)
        {
            string SongTitle = fields[0].Trim();
            string SongArtist = fields[2].Trim();

            if(!Artist.TryAdd(SongTitle, SongArtist))
            {
                Artist.Remove(SongArtist);
            }

        }

    }
}

string ToJson = @"{
    ""listen_type"": ""import"",
    ""payload"":";

int i = 0;
int j = 0;
foreach (FromYTJson x in History)
{
    if(x.header != "YouTube")
    {
        i += 1;
        ToListenBrainz ytmInfo = new(x);
        if (ytmInfo.track_metadata.artist_name == "Music Library Uploads")
        {
            string? artist;
            Artist.TryGetValue(ytmInfo.track_metadata.track_name, out artist);
            if(artist != null)
            {
                ytmInfo.track_metadata.artist_name = artist;
            }
            else
            {
                ytmInfo.track_metadata.artist_name = "Unknown";
            }
            YTMHistory.Add(ytmInfo);
        }
        // This check is to prevent songs for from where the artist is no longer known
        // from being added to the list as to prevent errors during import.
        else if(!ytmInfo.track_metadata.track_name.Contains("https"))
        {
            YTMHistory.Add(ytmInfo);
        }
    }

    if (Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(YTMHistory)) > 10200)
    {
        temp = JsonConvert.SerializeObject(YTMHistory);
        string toInsert = $"{ToJson} {temp} \n}}";
        File.Create($"JsonOut/{j}.json").Close();
        File.WriteAllText($"JsonOut/{j}.json", toInsert);
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

// Should I take in the URL for the song?
public class FromYTJson
{
    public string header { get; set; }
    public string title { get; set; }
    public List<subtitles> subtitles { get; set; }
    public string time { get; set; }
    public FromYTJson()
    {
        header = "";
        title = "";
        time = "";
        subtitles = new();
    }
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

public class ToListenBrainz
{
    public track_metadata track_metadata { get; set; }
    public long listened_at { get; set; }
    public ToListenBrainz(FromYTJson temp)
    {
        track_metadata = new();
        track_metadata.track_name = temp.title;
        DateTime _time;
        DateTime.TryParse(temp.time, out _time);
        listened_at = ((DateTimeOffset)_time).ToUnixTimeSeconds();
        if (temp.subtitles != null)
        {
            try
            {
                track_metadata.artist_name = temp.subtitles.ElementAt(0).name;
                if (track_metadata.artist_name.Contains("- Topic"))
                {
                    int numChar = track_metadata.artist_name.Count();
                    track_metadata.artist_name = track_metadata.artist_name.Remove(numChar - 8, 8);
                }
            }
            catch(Exception)
            {
                Console.WriteLine(track_metadata.track_name);
            }
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
