using System.Numerics;

namespace CodeBuster
{
    class Buster
    {
        public Vector2 Position { get; set; }
        public Vector2 GhostPosition { get; set; }
        public Vector2 EnemyPosition { get; set; }
        public Vector2 TargetPosition { get; set; }
        public Vector2 BasePosition { get; set; }
        public int EntityId { get; }
        public bool IsInDropZone { get; set; }
        public Ghost GhostCaptured { get; set; }
        public Ghost GhostInRange { get; set; }
        public BusterState State { get; set; }
        public BusterState LastState { get; set; }
        public int EnemyInRange { get; set; }
        public int LastTurnStun { get; set; }
        public bool CanStun { get; set; }

        public Buster(int entityId, Vector2 initialPosition, Vector2 basePosition)
        {
            Position = initialPosition;
            EntityId = entityId;

            // Initialize values
            IsInDropZone = false;
            GhostCaptured = null;
            BasePosition = basePosition;
            GhostInRange = null;
            EnemyInRange = -1;

            // Initialize MoveToPosition
            TargetPosition = initialPosition;

            // Initialize default state
            State = BusterState.MoveState;
        }

        public void ComputeInformations()
        {
            Player.print(State.ToString());
            State.ComputeInformations(this);

            if(State != LastState)
            {
                State.Enter(this);
            }

            LastState = State;
        }

        public string ComputeNextOrder()
        {
            return State.Update(this);
        }

        public bool CanRelease()
        {
            if (GhostCaptured != null && IsInDropZone)
            {
                return true;
            }

            return false;
        }

        public bool CanCapture()
        {
            if (GhostCaptured == null && GhostInRange != null)
            {
                return true;
            }

            return false;
        }

        public bool IsHoldingAGhost()
        {
            if(GhostCaptured == null)
            {
                return false;
            }

            return true;
        }

        public bool CanAttack()
        {
            if(EnemyInRange != -1 && CanStun)
            {
                return true;
            }

            return false;
        }

        public void Debug()
        {
            Player.print("Buster " + EntityId + " / position : " + Position.ToString() + " / target : " + TargetPosition.ToString() + " / can capture : " + CanCapture().ToString() + " / is holding : " + IsHoldingAGhost().ToString() + " / can release : " + CanRelease().ToString() + " / can attack : " + CanAttack().ToString() + " / last turn stun : " + LastTurnStun.ToString());
        }

        public void MarkGhostAsCaptured()
        {
            if(GhostInRange != null)
            {
                GhostInRange.Captured = true;
            }
        }

        public bool IsBusy()
        {
            if(State == BusterState.MoveState && !CanCapture())
            {
                return false;
            }

            return true;
        }
    }
}