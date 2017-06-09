using System.Collections.Generic;
using Microsoft.Xna.Framework;
using CommonCode;
using CommonCode.Drawing;
using CommonCode.Modifiers;
using System;

namespace SessionViewer
{
    abstract class Location
    {
        protected TexturedPlane plane;
        public Color PrimaryColor;
        public string Name;
        public string Description;
        protected float initialDistFromCenter;
        protected float initialAngle;
        protected float initialOrbitalSpeed;
        protected float initialOrbitHeight;
        protected bool isVisible;
        public int CursorDetectRadius;
        //Keeping this around here to ease some state transitions
        protected OrbitModifier3D planeOrbit;

        public abstract void HandleMessages(List<LocationMessage> messages);
        public abstract void Update(GameTime gameTime);
        public abstract List<DrawListItem> GetDrawables(Camera camera);
        /// <summary>
        /// If true, this world was given an orbit by the user, and will not be accepting that information from the MapScreen.
        /// </summary>
        public virtual bool UserPlaced { get; protected set; }
        public TexturedPlane WorldPlane { get { return plane; } }

        public virtual bool IsVisible { get; set; }

        protected void setisVisible(object sender, EventArgs e)
        {
            isVisible = false;
        }
    }
}
