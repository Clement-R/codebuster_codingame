﻿namespace CodeBuster
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
            // If we've drop our ghost and a ghost is in range
            if (!buster.IsHoldingAGhost() && buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }

            // If we've drop our ghost we can now move
            if (!buster.IsHoldingAGhost())
            {
                buster.State = BusterState.MoveState;
            }
        }
    }
}