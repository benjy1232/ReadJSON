// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using MetaBrainz;
using MetaBrainz.Common;
using MetaBrainz.ListenBrainz;

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

File.Create("output.json").Close();
File.WriteAllText("output.json", temp);

foreach(JsonLayout x in History)
{
    if(x.header == "YouTube")
    {
        YTHistory.Add(x);
    }
    else
    {
        NeededInformation ytmInfo = new(x);
        if(ytmInfo.artist == "Music Library Uploads")
        {
            UploadedSongs.Add(ytmInfo);
        }
        else
        {
            YTMHistory.Add(ytmInfo);
        }
        
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
    public string title { get; set; }
    public string artist { get; set; }
    public string time { get; set; }

    public NeededInformation(JsonLayout temp)
    {
        char[] charArr = {'-',' ', 'T', 'o', 'p', 'i','c'};
        title = temp.title;
        time = temp.time;
        if(temp.subtitles != null)
        {
            artist = temp.subtitles[0].name;
            if(artist.Contains("- Topic"))
            {
                artist = artist.TrimEnd(charArr);
            }
        }
        else
        {
            artist = "";
        }
        if(title.Contains("Watched "))
        {
            title = title.Remove(0,8);
        }
    }

}