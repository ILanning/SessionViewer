using CommonCode.Content;

namespace SessionViewer.Content
{
    public class GenericMeteorBuilder : Builder<GenericMeteorBuilder>
    {
        public string Name;
        public TexturedPlaneBuilder Image;

        public GenericMeteorBuilder() { }

        public GenericMeteorBuilder(string name, TexturedPlaneBuilder image)
        {
            Name = name;
            Image = image;
        }
    }

    /*public class SpecialMeteorBuilder : Builder<SpecialMeteorBuilder>
    {
        public float InitialAngle;
        public float DistFromCenter;
        public string Meteor;

        public SpecialMeteorBuilder() { }

        public SpecialMeteorBuilder(float angle, float distFromCenter, string meteor)
        {
            InitialAngle = angle;
            DistFromCenter = distFromCenter;
            Meteor = meteor;
        }
    }*/
}
