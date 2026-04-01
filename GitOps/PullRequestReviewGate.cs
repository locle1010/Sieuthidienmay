namespace Web_dienmay.GitOps
{
    public class PullRequestReviewGate
    {
        public bool BuildPassed { get; set; }
        public bool ScopeChecked { get; set; }
        public bool MigrationChecked { get; set; }
        public bool SecurityChecked { get; set; }

        public bool CanApprove()
        {
            return BuildPassed && ScopeChecked && MigrationChecked && SecurityChecked;
        }
    }
}
