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

        public bool FilterPatch(string name, List<Object> patchTypes)
        {
            if (patchTypes.Count > 0)
                return PatchTypes.Any(pt => patchTypes.Any(ptt => (string)ptt == pt.Name));

            if (string.IsNullOrEmpty(name) || name.Length < 2) return true;
            if (Name.Contains(name)) return true;
            if (PatchFileName.Contains(name)) return true;

            return false;
        }
    }
}
