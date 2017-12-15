﻿using System.Numerics;

namespace CodeBuster
{
    class MoveState : BusterState
    {
        public static System.Random rng = null;

        public MoveState()
        {
        }

        public override void Enter(Buster buster)
        {
        }

        public override string Update(Buster buster)
        {
            if(buster.IsHoldingAGhost())
            {
                // TODO : From actual position get the vector to the base and calculate the point that is in radius of the base (1600)
                buster.TargetPosition = buster.BasePosition;
            }

            // Go to the target position
            return "MOVE " + buster.TargetPosition.X + " " + buster.TargetPosition.Y;
        }

        public override void ComputeInformations(Buster buster)
        {
            // If we're just scouting and we can attack an enemy
            if (buster.CanAttack() && buster.GhostCaptured == null)
            {
                buster.State = BusterState.StunState;
            }

            // If we've moved to a ghost and we're in range to capture it
            if (buster.CanCapture())
            {
                buster.State = BusterState.CaptureState;
            }

            // If we were running to the base with a ghost and that we can now drop it
            if (buster.CanRelease())
            {
                buster.State = BusterState.ReleaseState;
            }
        }
    }
}
 