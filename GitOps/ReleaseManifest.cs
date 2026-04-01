namespace Web_dienmay.GitOps
{
    public class ReleaseManifest
    {
        public string Version { get; set; } = "1.0.0";
        public string SourceBranch { get; set; } = "develop";
        public string TargetBranch { get; set; } = "main";
        public string TagName { get; set; } = "v1.0.0";
    }
}
