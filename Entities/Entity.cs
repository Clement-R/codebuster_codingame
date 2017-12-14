using System.Numerics;

namespace CodeBuster
{
    class Entity
    {
        public Vector2 Position { get; set; }
        public int EntityId { get; }
        public bool IsVisible { get; set; }

        public Entity(Vector2 initialPosition, int entityId)
        {
            this.Position = initialPosition;
            this.EntityId = entityId;

            this.IsVisible = true;
        }

        public void Debug()
        {
            Player.print("Position : " + Position.ToString() + " / Id : " + EntityId.ToString() + " / Visible : " + IsVisible);
        }
    }
}