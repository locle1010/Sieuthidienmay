namespace Web_dienmay.GitOps
{
    public class SubmoduleSubtreeLink
    {
        public string Name { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty; // submodule | subtree
        public string RemoteUrl { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
    }
}
