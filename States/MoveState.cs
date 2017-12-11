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
            
        }

        public override string Update(Buster buster)
        {
            if(buster.IsHoldingAGhost())
            {
                // Go to base
                Player.print(buster.EntityId + " is going to base with a ghost");
                buster.TargetPosition = buster.BasePosition;
            }
            
            if(buster.Position == buster.TargetPosition)
            {
                buster.TargetPosition = new Vector2(buster.Position.x + rng.Next(-4000, 4000), buster.Position.x + rng.Next(-2000, 2000));
                if (buster.TargetPosition.x <= 0)
                {
                    buster.TargetPosition.x = 0;
                }
                if (buster.TargetPosition.x >= 16000)
                {
                    buster.TargetPosition.x = 16000;
                }

                if (buster.TargetPosition.y <= 0)
                {
                    buster.TargetPosition.y = 0;
                }
                if (buster.TargetPosition.y >= 9000)
                {
                    buster.TargetPosition.y = 9000;
                }

                // buster.TargetPosition = new Vector2(8000, 4500);
                // buster.TargetPosition = new Vector2(4000, 2250);
            }

            // Go to the target position
            Player.print(buster.TargetPosition.ToString());
            return "MOVE " + buster.TargetPosition.x + " " + buster.TargetPosition.y;
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
                Player.print(buster.EntityId + " is going to capture");
                buster.State = BusterState.CaptureState;
            }
        }
    }
}
 