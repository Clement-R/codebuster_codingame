using System;

namespace CodeBuster
{
    class Vector2
    {
        public int x { get; set; }
        public int y { get; set; }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static int Distance(Vector2 from, Vector2 to)
        {
            return (int)Math.Ceiling(Math.Sqrt(Math.Pow((to.x - from.x), 2) + Math.Pow((to.y - from.y), 2)));
        }

        public override string ToString()
        {
            return "X: " + x.ToString() + " / Y: " + y.ToString();
        }

        public override bool Equals(object obj)
        {
            Vector2 item = obj as Vector2;

            if (item == null)
            {
                return false;
            }

            if (this.x == item.x && this.y == item.y)
            {
                return true;
            }

            return false;
        }

        public static bool operator ==(Vector2 vec1, Vector2 vec2)
        {
            if (object.ReferenceEquals(vec1, null))
            {
                return object.ReferenceEquals(vec2, null);
            }

            return vec1.Equals(vec2);
        }

        public static bool operator !=(Vector2 vec1, Vector2 vec2)
        {
            if (object.ReferenceEquals(vec1, null))
            {
                return object.ReferenceEquals(vec2, null);
            }

            return vec1.Equals(vec2);
        }

        public override int GetHashCode()
        {
            return x + y;
        }
    }
}