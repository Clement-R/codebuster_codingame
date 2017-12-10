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
    }
}