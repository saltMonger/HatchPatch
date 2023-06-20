using HatchPatchWeb2.Models;
using HatchPatchWeb2.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace HatchPatchWeb2.Pages
{
    public partial class Index
    {
        [Inject]
        private Blazored.LocalStorage.ILocalStorageService _storage { get; set; } = default!;
        [Inject]
        private IJSRuntime _jsRuntime { get; set; } = default!;
        [Inject]
        private IDialogService _dialog { get; set; } = default!;

        public List<SavedPatch> Patches { get; set; } = new List<SavedPatch>();

        private string patchSearchString = string.Empty;
        private List<Object> selectedPatchTypeValues = new();

        protected override async Task OnInitializedAsync()
        {
            var keys = (await _storage.KeysAsync()).Where(k => k.Contains("patch")).ToList();
            foreach (var key in keys)
            {
                var patch = await _storage.GetItemAsync<SavedPatch>(key);
                Patches.Add(patch);
            }

            var lastVersionTutorial = await _storage.GetItemAsync<int?>("finishedTutorial");
            if ((lastVersionTutorial ?? 0) < DataConstants.CURRENT_VERSION)
            {
                // pop over tutorial dialog
                var parameters = new DialogParameters();
                parameters.Add(nameof(StartupDialog.version), lastVersionTutorial ?? 0);
                var res = await _dialog.ShowAsync<StartupDialog>("Version Notes/Intro", parameters);
                var ret = await res.Result;
                Console.WriteLine(ret);
                if ((bool)ret.Data)
                    await _storage.SetItemAsStringAsync("finishedTutorial", DataConstants.CURRENT_VERSION.ToString());
            }

            await base.OnInitializedAsync();
        }

        private void NewPatch() => Navigation.NavigateTo("PatchEditor");

        private void DownloadPatch(SavedPatch patch)
        {
            if (_jsRuntime is IJSUnmarshalledRuntime wasmJsRuntime)
            {
                wasmJsRuntime.InvokeUnmarshalled<string, string, byte[], bool>
                    ("BlazorDownloadFileFast", patch.PatchFileName, "application/octet-stream", patch.PatchData);
            }
        }

        private void EditPatch(string id) => Navigation.NavigateTo($"patcheditor/{id}");

        private void SelectedPatchTypeValuesChanged(ICollection<object> objects)
        {
            selectedPatchTypeValues = objects.ToList();
        }

        private bool PatchFilter(SavedPatch patch) => patch.FilterPatch(patchSearchString, selectedPatchTypeValues);
    }
}
