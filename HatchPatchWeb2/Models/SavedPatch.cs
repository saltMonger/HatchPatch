namespace HatchPatchWeb2.Models
{
    public class SavedPatch
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PatchNotes { get; set; }
        public byte[] PatchData { get; set; }
        public string PatchFileName { get; set; }
        public List<PatchType> PatchTypes { get; set; } = new List<PatchType>();
        public DateTime PatchCreated { get; set; }
        public DateTime PatchModified { get; set; }

        public PatchSuggestion Suggestions { get; set; } = new();
    }
}
