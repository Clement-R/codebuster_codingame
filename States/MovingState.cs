namespace CodeBuster
{
    class MovingState : BusterState
    {
        public MovingState()
        {
        }

        public override void Enter(Buster buster)
        {

        }

        public override string Update(Buster buster)
        {
            if(buster.IsHoldingAGhost())
            {
                // Go to base
                return "MOVE " + buster.BasePosition.x + " " + buster.BasePosition.y;
            }

            // Go to the middle of the map
            return "MOVE 8000 4500";
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we were running to the base with a ghost and that we can now drop it
            if(buster.CanRelease())
            {
                buster.State = BusterState.ReleaseState;
            }

            // If we've moved to a ghost and we're in range to capture it
            if(buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }
        }
    }
}