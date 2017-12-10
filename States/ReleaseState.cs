namespace CodeBuster
{
    class ReleaseState : BusterState
    {
        public ReleaseState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            return "RELEASE";
        }

        public override void ComputeInformations(Buster buster)
        {
            // TODO : define possible transitions
            // If buster has no ghost switch to move
            if (!buster.GhostCaptured)
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}