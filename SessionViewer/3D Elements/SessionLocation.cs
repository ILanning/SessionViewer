using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using SessionViewer.Content;
using CommonCode.Modifiers;
using CommonCode;
using CommonCode.Drawing;

namespace SessionViewer
{
    class SessionLocation : Location, ICopyable<SessionLocation>
    {
        public static SessionLocation BuildSessionLocation(string path, DynamicContentManager content)
        {
            return new SessionLocation(path, content);
        }

        public SessionLocation() { }

        public SessionLocation(string filePath, DynamicContentManager Content)
        {
            SessionLocationBuilder builder = SessionLocationBuilder.BuilderRead(filePath);
            plane = new TexturedPlane(builder.Image);
            plane.billboard = true;
            isVisible = builder.IsVisible;
            PrimaryColor = new Color((byte)builder.PrimaryColor.X, (byte)builder.PrimaryColor.Y, (byte)builder.PrimaryColor.Z, (byte)builder.PrimaryColor.W);
            Name = builder.Name;
            Description = builder.Description;
            CursorDetectRadius = builder.CursorDetectRadius;
            if (builder.InitialAngle != 0 || builder.OrbitalSpeed != 0 || builder.OrbitHeight != 0 || builder.DistFromCenter != 0)
            {
                UserPlaced = true;
                initialDistFromCenter = builder.DistFromCenter;
                initialAngle = builder.InitialAngle;
                initialOrbitalSpeed = builder.OrbitalSpeed;
                initialOrbitHeight = builder.OrbitHeight;
                plane.AddModifier(new OrbitModifier3D(new Vector3(0, builder.OrbitHeight, 0), Vector3.Up, builder.DistFromCenter, builder.InitialAngle, builder.OrbitalSpeed, plane, false, -1));
            }
            else
                UserPlaced = false;
        }

        private void reload(string filePath, DynamicContentManager Content)
        {
            SessionLocation newState = Content.Load<SessionLocation>(filePath);
            newState.plane.ClearModifiers();
            TexturedPlane oldPlane = plane;
            plane = newState.plane;
            plane.Color = oldPlane.Color;
            plane.WorldPosition = oldPlane.WorldPosition;
            IsVisible = newState.isVisible;
            PrimaryColor = newState.PrimaryColor;
            Name = newState.Name;
            Description = newState.Description;
            CursorDetectRadius = newState.CursorDetectRadius;
            for (int i = 0; i < oldPlane.Modifiers.Length; i++)
            {
                if (oldPlane.Modifiers[i] != null)
                    plane.AddModifier(oldPlane.Modifiers[i].DeepCopy(plane));
            }
        }

        public override void HandleMessages(List<LocationMessage> messages)
        {
            //Throw exceptions for messages this object can't handle
            //Messages valid for this object:
            //
            //Filter out useless/overridden messages
            //Handle any remaining messages sequentially
            LocationMessage?[] foundMessages = new LocationMessage?[] { null, null, null, null };
            for (int i = 0; i < messages.Count; i++)
            {
                //Reload replaces
                //SetOrbit goes with first
                //ChangeOrbit replaces
                //SetVisibility replaces
                switch (messages[i].EventType)
                {
                    case EventTypes.Reload:
                        foundMessages[0] = messages[i];
                        break;
                    case EventTypes.SetOrbit:
                        if (foundMessages[1] == null)
                            foundMessages[1] = messages[i];
                        break;
                    case EventTypes.ChangeOrbit:
                        foundMessages[2] = messages[i];
                        break;
                    case EventTypes.SetVisibility:
                        foundMessages[3] = messages[i];
                        break;
                    default:
                        throw new ArgumentException("Message" + i.ToString() + " of type " + messages[i].EventType.ToString() + " is not valid for SessionLocation " + this.Name);
                }
            }
            if (foundMessages[0] != null)
                reload(foundMessages[0].Value.Message, ScreenManager.Content);
            if (foundMessages[1] != null)
            {
                string dist, angle, speed, height;
                string message = foundMessages[1].Value.Message;
                dist = message.Substring(0, message.IndexOf(' ') - 1);
                angle = message.Substring(1, message.IndexOf(' ', 1) - 1);
                speed = message.Substring(2, message.IndexOf(' ', 2) - 1);
                height = message.Substring(3, message.IndexOf(' ', 3) - 1);
                initialDistFromCenter = float.Parse(dist);
                initialAngle = float.Parse(angle);
                initialOrbitalSpeed = float.Parse(speed);
                initialOrbitHeight = float.Parse(height);
            }
            if (foundMessages[2] != null)
            {
                float dist, angle, speed, height;
                string message = foundMessages[1].Value.Message;
                dist = float.Parse(message.Substring(0, message.IndexOf(' ') - 1));
                angle = float.Parse(message.Substring(1, message.IndexOf(' ', 1) - 1));
                speed = float.Parse(message.Substring(2, message.IndexOf(' ', 2) - 1));
                height = float.Parse(message.Substring(3, message.IndexOf(' ', 3) - 1));
                //OrbitInterpolator
            }
            if (foundMessages[3] != null)
                IsVisible = bool.Parse(foundMessages[3].Value.Message);
        }

        public override void Update(GameTime gameTime)
        {
            plane.Update(gameTime);
        }

        public override bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (isVisible == value)
                    return;
                if (!value)
                {
                    ColorModifier3D fadeModifier = new ColorModifier3D(Color.TransparentBlack, true, plane, 90);
                    fadeModifier.Complete += setisVisible;
                    plane.AddModifier(fadeModifier);
                }
                else
                {
                    isVisible = true;
                    plane.AddModifier(new ColorModifier3D(Color.White, true, plane, 90));
                }
            }
        }

        /// <summary>
        /// Creates a list of all drawable objects this location contains.
        /// </summary>
        /// <param name="camera">The camera used to construct the DrawListItems.</param>
        /// <returns>A list of all drawable things this object contains.</returns>
        public override List<DrawListItem> GetDrawables(Camera camera)
        {
            if (IsVisible)
                return new List<DrawListItem>(new DrawListItem[] { new DrawListItem(camera, plane) });
            else
                return new List<DrawListItem>();
        }

        #region ICopyable<SessionLocation> Members

        public SessionLocation ShallowCopy()
        {
            var clone = new SessionLocation();
            clone.CursorDetectRadius = CursorDetectRadius;
            clone.Description = Description;
            clone.isVisible = IsVisible;
            clone.Name = Name;
            clone.plane = plane;
            clone.PrimaryColor = PrimaryColor;
            clone.initialAngle = initialAngle;
            clone.initialDistFromCenter = initialDistFromCenter;
            clone.initialOrbitalSpeed = initialOrbitalSpeed;
            clone.initialOrbitHeight = initialOrbitHeight;
            clone.UserPlaced = UserPlaced;
            return clone;
        }

        public SessionLocation ShallowCopy(LoadArgs l)
        {
            return ShallowCopy();
        }

        public SessionLocation DeepCopy()
        {
            var clone = new SessionLocation();
            clone.plane = plane.DeepCopy();
            clone.CursorDetectRadius = CursorDetectRadius;
            clone.Description = Description;
            clone.isVisible = IsVisible;
            clone.Name = Name;
            clone.PrimaryColor = PrimaryColor;
            clone.initialAngle = initialAngle;
            clone.initialDistFromCenter = initialDistFromCenter;
            clone.initialOrbitalSpeed = initialOrbitalSpeed;
            clone.initialOrbitHeight = initialOrbitHeight;
            clone.UserPlaced = UserPlaced;
            return clone;
        }

        public SessionLocation DeepCopy(LoadArgs l)
        {
            return DeepCopy();
        }

        #endregion
    }
}
