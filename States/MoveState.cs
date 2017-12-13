using System.Numerics;

namespace CodeBuster
{
    class MoveState : BusterState
    {
        public static System.Random rng = null;

        public MoveState()
        {
            if(rng == null)
            {
                rng = new System.Random();
            }
        }

        public override void Enter(Buster buster)
        {
            // TODO : Change this value
            buster.TargetPosition = new Vector2(8000, 4500);
        }

        public override string Update(Buster buster)
        {
            if(buster.IsHoldingAGhost())
            {
                // TODO : From actual position get the vector to the base and calculate the point that is in radius of the base (1600)
                buster.TargetPosition = buster.BasePosition;
            }
            
            // TODO : Remove the random movement !
            /*
            if(buster.Position == buster.TargetPosition)
            {
                buster.TargetPosition = new Vector2(buster.Position.X + rng.Next(-8000, 8000), buster.Position.X + rng.Next(-3000, 3000));
                if (buster.TargetPosition.X <= 0)
                {
                    buster.TargetPosition = new Vector2(0, buster.TargetPosition.Y);
                }
                if (buster.TargetPosition.X >= 16000)
                {
                    buster.TargetPosition = new Vector2(16000, buster.TargetPosition.Y);
                }

                if (buster.TargetPosition.Y <= 0)
                {
                    buster.TargetPosition = new Vector2(buster.TargetPosition.X, 0);
                }
                if (buster.TargetPosition.Y >= 9000)
                {
                    buster.TargetPosition = new Vector2(buster.TargetPosition.X, 9000);
                }
            }
            */

            // Go to the target position
            return "MOVE " + buster.TargetPosition.X + " " + buster.TargetPosition.Y;
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

            // If we're just scouting and we can attack an enemy
            if (buster.CanAttack() && !buster.GhostCaptured)
            {
                buster.State = BusterState.StunState;
            }
        }
    }
}
 