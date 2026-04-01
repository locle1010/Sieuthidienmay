namespace Web_dienmay.GitOps
{
    public class CherryPickBackportRecord
    {
        public string SourceBranch { get; set; } = "main";
        public string TargetBranch { get; set; } = "develop";
        public string HotfixSha { get; set; } = string.Empty;
        public string BackportSha { get; set; } = string.Empty;
        public string Owner { get; set; } = "Duong Viet Nghia";
    }
}
