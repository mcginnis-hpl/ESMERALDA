namespace ESMERALDAClasses
{
    using System;

    public class Point : Ordered<Point>
    {
        private static readonly System.Random rnd = new System.Random();
        public double x;
        public double y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static double Area2(Point p0, Point p1, Point p2)
        {
            return (((p0.x * (p1.y - p2.y)) + (p1.x * (p2.y - p0.y))) + (p2.x * (p0.y - p1.y)));
        }

        public bool Equals(Point p2)
        {
            return ((this.x == p2.x) && (this.y == p2.y));
        }

        public override bool Less(Ordered<Point> o2)
        {
            Point p2 = (Point) o2;
            return ((this.x < p2.x) || ((this.x == p2.x) && (this.y < p2.y)));
        }

        public static Point Random(int w, int h)
        {
            return new Point((double) rnd.Next(w), (double) rnd.Next(h));
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "(", this.x, ", ", this.y, ")" });
        }
    }
}

