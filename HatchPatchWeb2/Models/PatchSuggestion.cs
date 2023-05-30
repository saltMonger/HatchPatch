namespace HatchPatchWeb2.Models;

public class PatchSuggestion
{
    public List<string> Adjectives { get; set; } = new();
    public string Color { get; set; } = "#FFFFFF";
    public string Synth { get; set; } = String.Empty;
    public List<PatchType> PatchTypes { get; set; } = new();
    public List<string> Constraints { get; set; } = new();

}
