using MudBlazor;
using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace HatchPatchWeb2.Models;

public static class DataConstants
{
    public static int CURRENT_VERSION = 1;

    public static readonly string ADJECTIVE_LIST = "AdjectiveList-";
    public static readonly string SYNTH_LIST = "SynthList-";
    public enum AdjectiveLists
    {
        DEFAULT_ONE,
        USER_ONE,
        USER_TWO,
    }

    public enum SynthLists
    {
        DEFAULT_ONE,
        USER_ONE,
        USER_TWO,
        CUSTOM
    }

    public static List<PatchType> PatchTypes = new List<PatchType>
    {
        new PatchType(){ Name = "PLUCK", Color = Color.Secondary },
        new PatchType(){ Name = "LEAD", Color = Color.Primary },
        new PatchType(){ Name = "STRING", Color = Color.Tertiary },
        new PatchType(){ Name = "PAD", Color = Color.Info },
        new PatchType(){ Name = "HORN", Color = Color.Secondary },
        new PatchType(){ Name = "BELL", Color = Color.Primary },
        new PatchType(){ Name = "VOICE", Color = Color.Tertiary },
        new PatchType(){ Name = "SFX", Color = Color.Info },
        new PatchType(){ Name = "AMBIENT", Color = Color.Tertiary },
        new PatchType(){ Name = "KEYBOARD", Color = Color.Info },
    };

    public async static Task<List<string>> GetAdjectiveList(AdjectiveLists adjType, HttpClient httpContext, ILocalStorageService storage, int number)
    {
        List<string> ret = new();
        if (adjType == AdjectiveLists.USER_ONE || adjType == AdjectiveLists.USER_TWO)
        {
            // TODO: do special shit
            return await storage.GetItemAsync<List<string>>($"{ADJECTIVE_LIST}{adjType}");
        }

        var resp = await httpContext.GetStringAsync("default-data/adjlists/def-adj-1.txt");
        return GetRandomNumberOf(number, 
            resp.Split(Environment.NewLine).ToList());
    }

    public async static Task<List<string>> GetSynthList(SynthLists synType, HttpClient httpContext, ILocalStorageService storage, int number)
    {
        List<string> ret = new();
        if (synType == SynthLists.USER_ONE || synType == SynthLists.USER_TWO)
        {
            // TODO: do special shit
            return await storage.GetItemAsync<List<string>>($"{SYNTH_LIST}{synType}");
        }

        if (synType == SynthLists.CUSTOM) return ret;

        var resp = await httpContext.GetStringAsync("default-data/synlists/def-syn-vsts.txt");
        return GetRandomNumberOf<string>(number,
            resp.Split(Environment.NewLine).ToList());
    }

    public static List<PatchType> GetPatchTypes()
    {
        int rand = Random.Shared.Next(PatchTypes.Count);
        if (rand <= 0) rand = 1;
        return GetRandomNumberOfNonRemoving(rand, PatchTypes);
    }

    public async static Task<List<string>> GetTutorialsForVersion(int version, HttpClient httpContext)
    {
        List<string> ret = new();
        for (int i = version; i < CURRENT_VERSION; i++)
        {
            var resp = await httpContext.GetStringAsync($"default-data/tutorials/version{version}.txt");
            ret.Add(resp);
        }
        return ret;
    }

    private static List<T> GetRandomNumberOf<T>(int number, List<T> source)
    {
        if (number < 0) throw new InvalidDataException("Invalid number of items requested! Please choose a non zero number");
        if (number >= source.Count) throw new InvalidDataException($"Too many items requested for list, max is: {source.Count}");

        int rand;
        List<T> ret = new();
        for (int i = 0; i < number; i++)
        {
            rand = Random.Shared.Next(source.Count);
            ret.Add(source[rand]);
            source.RemoveAt(rand);
        }

        return ret;
    }

    private static List<T> GetRandomNumberOfNonRemoving<T>(int number, List<T> source)
    {
        if (number < 0) throw new InvalidDataException("Invalid number of items requested! Please choose a non zero number");
        if (number >= source.Count) throw new InvalidDataException($"Too many items requested for list, max is: {source.Count}");

        int rand;
        List<T> ret = new();
        List<T> filtered = new();
        for (int i = 0; i < number; i++)
        {
            filtered = source.Except(ret).ToList();
            rand = Random.Shared.Next(filtered.Count());
            ret.Add(filtered[rand]);
        }
        return ret;
    }
}
