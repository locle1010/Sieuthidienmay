namespace Web_dienmay.GitOps
{
    public static class GitFlowGuardPolicy
    {
        public const string MainBranch = "main";
        public const string DevelopBranch = "develop";

        public static readonly string[] AllowedFeaturePrefixes =
        {
            "feature/",
            "hotfix/"
        };

        public static bool IsProtectedBranch(string branchName)
        {
            return branchName == MainBranch || branchName == DevelopBranch;
        }
    }
}
