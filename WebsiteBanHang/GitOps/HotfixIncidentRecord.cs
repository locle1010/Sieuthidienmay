namespace Web_dienmay.GitOps
{
    public class HotfixIncidentRecord
    {
        public string IncidentCode { get; set; } = string.Empty;
        public string MainBranchSha { get; set; } = string.Empty;
        public string HotfixBranch { get; set; } = string.Empty;
        public string Owner { get; set; } = "Le Tan Loc";
        public string Status { get; set; } = "Open";
    }
}
