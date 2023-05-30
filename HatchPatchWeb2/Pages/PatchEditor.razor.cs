using HatchPatchWeb2.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using static HatchPatchWeb2.Models.DataConstants;

namespace HatchPatchWeb2.Pages;

public partial class PatchEditor
{
    [Inject]
    private HttpClient _httpClient { get; set; } = default!;
    [Inject]
    private Blazored.LocalStorage.ILocalStorageService _storage { get; set; } = default!;
    [Inject]
    private IDialogService _dialog { get; set; } = default!;
    [Inject]
    private NavigationManager _nav { get; set; } = default!;

    [Parameter]
    public string? patchId { get; set; } = default!;

    public List<AdjectiveLists> SelectedAdjectiveList = new() { AdjectiveLists.DEFAULT_ONE };
    public List<SynthLists> SelectedSynthList = new() { SynthLists.DEFAULT_ONE };

    public int numberOfAdjectives = 1;
    public int numberOfSynths = 1;
    public string generatedColor = "#FFFFFF";
    public bool timeConstraint = false;
    public bool listenToIt = false;
    public string CustomSynth = string.Empty;

    public bool randomized = false;
    public bool editMode = false;

    public List<string> GeneratedAdjectives = new();
    public string GeneratedSynths = String.Empty;
    public List<PatchType> GeneratedPatchTypes = new();
    public List<string> GeneratedConstraints = new();

    public string patchName = string.Empty;
    public string patchNotes = string.Empty;
    public byte[] patchData;
    public string patchFileName = string.Empty;
    public MudChip[] selectedPatchTypes = new MudChip[PatchTypes.Count];
    private List<Object> selectedPatchTypeValues = new();

    private bool writeInSynth = false;

    protected override async Task OnParametersSetAsync()
    {
        if (String.IsNullOrEmpty(patchId))
            return;

        var patch = await _storage.GetItemAsync<SavedPatch>($"patch-{patchId}");
        patchName = patch.Name;
        patchNotes = patch.PatchNotes;
        patchData = patch.PatchData;
        patchFileName = patch.PatchFileName;
        selectedPatchTypeValues = patch.PatchTypes.Select(pt => (Object)pt.Name).ToList();

        GeneratedAdjectives = patch.Suggestions.Adjectives;
        GeneratedSynths = patch.Suggestions.Synth;
        GeneratedConstraints = patch.Suggestions.Constraints;
        GeneratedPatchTypes = patch.Suggestions.PatchTypes;
        generatedColor = patch.Suggestions.Color;

        randomized = true;
        editMode = true;
    }

    public async Task OnChange_SelectedAdjectiveListChanged(List<AdjectiveLists> list)
    {
        var selectedList = list.First();
        if (selectedList == AdjectiveLists.USER_ONE || selectedList == AdjectiveLists.USER_TWO)
        {
            var keyExists = await _storage.ContainKeyAsync($"{ADJECTIVE_LIST}{selectedList}");
            if (!keyExists) return;// pop some junk up here 
        }

        SelectedAdjectiveList = list;
    }

    public async Task OnChange_SelectedSynthListChanged(List<SynthLists> list)
    {
        var selectedSynth = list.First();
        writeInSynth = false;
        if (selectedSynth == SynthLists.USER_ONE || selectedSynth == SynthLists.USER_TWO)
        {
            var keyExists = await _storage.ContainKeyAsync($"{SYNTH_LIST}{selectedSynth}");
            if (!keyExists) return;// pop some junk up here 
        }
        if (selectedSynth == SynthLists.CUSTOM)
            writeInSynth = true;

        SelectedSynthList = list;
    }

    private void SelectedPatchTypeValuesChanged(ICollection<object> objects)
    {
        selectedPatchTypeValues = objects.ToList();
    }

    public async Task Randomize()
    {
        GeneratedAdjectives = await GetAdjectiveList(SelectedAdjectiveList.First(), _httpClient, _storage, numberOfAdjectives);
        GeneratedSynths = SelectedSynthList.First() == SynthLists.CUSTOM ? CustomSynth
            : (await GetSynthList(SelectedSynthList.First(), _httpClient, _storage, numberOfSynths)).First();
        GeneratedPatchTypes = GetPatchTypes();
        GeneratedConstraints.Clear();


        byte[] cb = new byte[3];
        Random.Shared.NextBytes(cb);
        var str = $"#{Convert.ToHexString(cb)}";
        generatedColor = str;
        if (timeConstraint) GeneratedConstraints.Add(RandomTime());
        if (listenToIt) GeneratedConstraints.Add("Mute your synth");

        randomized = true;
    }

    private async Task FileUploaded(InputFileChangeEventArgs e)
    {
        using (var stream = e.File.OpenReadStream())
        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            ms.Position = 0;
            patchData = ms.ToArray();
        }
        patchFileName = e.File.Name;
    }

    private async Task SavePatchAsNew()
    {
        var patchGuid = Guid.NewGuid();
        await _storage.SetItemAsync("patch-" + patchGuid, new SavedPatch()
        {
            Id = patchGuid,
            Name = patchName,
            PatchNotes = patchNotes,
            PatchFileName = patchFileName,
            PatchTypes = PatchTypes.Where(pt => selectedPatchTypeValues.Any(spt => (string)spt == pt.Name)).ToList(),
            PatchData = patchData,
            Suggestions = new PatchSuggestion()
            {
                Adjectives = GeneratedAdjectives,
                PatchTypes = GeneratedPatchTypes,
                Constraints = GeneratedConstraints,
                Color = generatedColor,
                Synth = GeneratedSynths
            }
        });
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var diag = _dialog.Show<ConfirmDonePatchEditor>("Confirm", options);
        var res = await diag.Result;
        if (!res.Canceled) _nav.NavigateTo("");
    }

    private async Task OverwritePatch()
    {
        if (!editMode)
        {
            await SavePatchAsNew();
            return;
        }

        await _storage.SetItemAsync("patch-" + patchId, new SavedPatch()
        {
            Id = Guid.Parse(patchId!),
            Name = patchName,
            PatchNotes = patchNotes,
            PatchFileName = patchFileName,
            PatchTypes = PatchTypes.Where(pt => selectedPatchTypeValues.Any(spt => (string)spt == pt.Name)).ToList(),
            PatchData = patchData,
            Suggestions = new PatchSuggestion()
            {
                Adjectives = GeneratedAdjectives,
                PatchTypes = GeneratedPatchTypes,
                Constraints = GeneratedConstraints,
                Color = generatedColor,
                Synth = GeneratedSynths
            }
        });

        var options = new DialogOptions { CloseOnEscapeKey = true };
        var diag = _dialog.Show<ConfirmDonePatchEditor>("Confirm", options);
        var res = await diag.Result;
        if (!res.Canceled) _nav.NavigateTo("");
    }

    private string RandomTime()
    {
        var rand = Random.Shared.Next(4);
        switch (rand)
        {
            case 0: return "in 5 minutes";
            case 1: return "in 10 minutes";
            case 2: return "in 15 minutes";
            case 3: return "in 30 minutes";
            case 4: return "in 1 hour";
            default: return "in 5 minutes";
        }
    }
}

