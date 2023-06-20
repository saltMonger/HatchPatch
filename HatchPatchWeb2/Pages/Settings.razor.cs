using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using static HatchPatchWeb2.Models.DataConstants;

namespace HatchPatchWeb2.Pages;

public partial class Settings
{
    [Inject]
    private ILocalStorageService _storage { get; set; } = default!;

    public List<string> AdjectiveListUserOne { get; set; } = new();
    public List<string> AdjectiveListUserTwo { get; set; } = new();
    public List<string> SynthListUserOne { get; set; } = new();
    public List<string> SynthListUserTwo { get; set; } = new();

    private bool loading = false;

    private List<string> GetPreviewListFor(List<string> list) =>
        list.Take(5).Concat(list.Count > 5 ? new List<string>() { "..."} : new List<string>()).ToList();

    protected override async Task OnInitializedAsync()
    {
        loading = true;
        // gather current settings
        AdjectiveListUserOne = (await _storage.GetItemAsync<List<string>>($"{ADJECTIVE_LIST}{AdjectiveLists.USER_ONE}")) ?? new();
        AdjectiveListUserTwo = (await _storage.GetItemAsync<List<string>>($"{ADJECTIVE_LIST}{AdjectiveLists.USER_TWO}")) ?? new();

        SynthListUserOne = (await _storage.GetItemAsync<List<string>>($"{SYNTH_LIST}{SynthLists.USER_ONE}")) ?? new();
        SynthListUserTwo = (await _storage.GetItemAsync<List<string>>($"{SYNTH_LIST}{SynthLists.USER_TWO}")) ?? new();

        Console.WriteLine(SynthListUserOne.ToString());

        await base.OnInitializedAsync();
        loading = false;
    }

    private async Task UpdateActionForAdjectiveListsAsync(List<string> items, AdjectiveLists option)
    {

        await _storage.SetItemAsync<List<string>>($"{ADJECTIVE_LIST}{option}", items);
        switch (option)
        {
            case AdjectiveLists.USER_ONE: AdjectiveListUserOne = items; break;
            case AdjectiveLists.USER_TWO: AdjectiveListUserTwo = items; break;
        }
        loading = false;
    }

    private async Task UpdateActionForSynthListsAsync(List<string> items, SynthLists option)
    {

        await _storage.SetItemAsync<List<string>>($"{SYNTH_LIST}{option}", items);
        switch (option)
        {
            case SynthLists.USER_ONE: AdjectiveListUserOne = items; break;
            case SynthLists.USER_TWO: AdjectiveListUserTwo = items; break;
        }
        loading = false;
    }

    private async Task<List<string>> GetNLDelimitedValuesFromFile(IBrowserFile file)
    {
        string fullString;
        using (var stream = file.OpenReadStream())
        using (var sr = new StreamReader(stream))
        {
            fullString = await sr.ReadToEndAsync();
        }

        var splitList = fullString.Split(Environment.NewLine).ToList();
        return splitList;
    }
}
