using CommonCode.Content;
using Microsoft.Xna.Framework;

namespace SessionViewer.Content
{
    public class SessionLocationBuilder : Builder<SessionLocationBuilder>
    {
        public float InitialAngle;
        public float DistFromCenter;
        public float OrbitalSpeed;
        public float OrbitHeight;
        public bool IsVisible;
        public int CursorDetectRadius;
        public string Name;
        public string Description;
        public Vector4Builder PrimaryColor;
        public TexturedPlaneBuilder Image;

        public SessionLocationBuilder() { }
    }

    public class PlayerWorldBuilder : Builder<PlayerWorldBuilder>
    {
        public float InitialAngle;
        public float DistFromCenter;
        public float OrbitalSpeed;
        public float OrbitHeight;
        public bool IsVisible;
        public int CursorDetectRadius;
        public string Name;
        public string Description;
        public int GateLevel;
        public string PlayerName;
        public bool PlayerEntered;
        public Vector4Builder PrimaryColor;
        public Vector4Builder SecondaryColor;
        public TexturedPlaneBuilder Image;

        public PlayerWorldBuilder() { }

        public PlayerWorldBuilder(string name, string description, int gate, string playerName, bool playerEntered, Vector4 primaryColor, 
            Vector4 secondaryColor, TexturedPlaneBuilder image)
        {
            Name = name;
            Description = description;
            GateLevel = gate;
            PlayerName = playerName;
            PlayerEntered = playerEntered;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            Image = image;
        }
    }

    public class SkaiaBuilder : Builder<SkaiaBuilder>
    {
        public bool IsVisible;
        public int CursorDetectRadius;
        public string Name;
        public string Description;
        public Vector4Builder PrimaryColor;
        public TexturedPlaneBuilder Image;
        public TexturedPlaneBuilder Battlefield;

        public SkaiaBuilder() { }
    }
}
