namespace CodeBuster
{
    class CaptureState : BusterState
    {
        public CaptureState()
        {
        }

        public override void Enter(Buster buster)
        {
        }

        public override string Update(Buster buster)
        {
            // BUST id
            return "BUST " + buster.GhostInRange.EntityId.ToString();
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we were capturing and the ghost flew away, or we capture a ghost but we're not in range to drop it
            if(!buster.CanCapture() || (buster.GhostCaptured != null && !buster.IsHoldingAGhost()))
            {
                buster.State = BusterState.MoveState;
            }

            // If we were capturing and we're already in range for drop
            if (buster.CanRelease())
            {
                buster.State = BusterState.ReleaseState;
            }
        }
    }
}
