using Microsoft.Xna.Framework;
using CommonCode.Content;


namespace SessionViewer.Content
{
    public class CollisionDataBuilder : Builder<CollisionDataBuilder>
    {
        /// <summary>
        /// A counter-clockwise list of points representing the collision map.
        /// </summary>
        public Vector2Builder[] PointsInPolygon;
        /// <summary>
        /// A point near the center of the collision map.
        /// </summary>
        public Vector2Builder CenterPoint;
        /// <summary>
        /// Represents what the collision map will draw in front of if debug is enabled.
        /// </summary>
        public sbyte DrawOrder;
        /// <summary>
        /// If false, all collision checks with this object will immediately return false.
        /// </summary>
        public bool CollisionOn;

        public CollisionDataBuilder()
        { }

        public CollisionDataBuilder(Vector2[] points, Vector2 center, bool collide)
        {
            PointsInPolygon = new Vector2Builder[points.Length];
            for (int i = 0; i < points.Length; i++ ) 
                PointsInPolygon[i] = points[i];
            CenterPoint = center;
            CollisionOn = collide;
        }
    }
}
