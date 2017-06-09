using System.Collections.Generic;
using Microsoft.Xna.Framework;

using SessionViewer.Content;
using CommonCode.Modifiers;
using System;
using CommonCode.Drawing;
using CommonCode;

namespace SessionViewer
{
    class PlayerWorld : Location, ICopyable<PlayerWorld>
    {
        public Color SecondaryColor;
        /// <summary>
        /// The number of gates between the world and Skaia that the player has gone through.
        /// </summary>
        private int gateLevel;
        /// <summary>
        /// The name of this world's player.
        /// </summary>
        public string PlayerName;
        /// <summary>
        /// If true, the player for this world has entered the session.
        /// </summary>
        public bool PlayerEntered 
        { 
            get { return playerEntered; } 
            set 
            {
                if (value == playerEntered)
                    return;
                playerEntered = value;
                if (playerEntered)
                {
                    //planeOrbit = (OrbitModifier3D)planeOrbit.DeepCopy(plane);
                    plane.ClearModifiers();
                    plane.AddModifier(planeOrbit);
                    plane.AddModifier(new ColorModifier3D(Color.White, true, plane, 60));
                    plane.AddModifier(new MoveToModifier3D(new Vector3(-1, 8, -1), plane, true, 60));

                    lowerSpiroOrbit = (OrbitModifier3D)lowerSpiroOrbit.DeepCopy(lowerSpirograph);
                    lowerSpirograph.ClearModifiers();
                    lowerSpirograph.AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(0.02f, 0, 0), false, -1));
                    lowerSpirograph.AddModifier(lowerSpiroOrbit);
                    lowerSpirograph.AddModifier(new SpirographLerpModifier(-1, -1, -1, 3, -1, true, lowerSpirograph, 60));

                    lowerSpirograph.AddModifier(new ColorModifier3D(PrimaryColor, true, lowerSpirograph, 60));
                    setGates(gateLevel);
                }
                else
                {
                    //planeOrbit = (OrbitModifier3D)planeOrbit.DeepCopy(plane);
                    plane.ClearModifiers();
                    plane.AddModifier(planeOrbit);
                    plane.AddModifier(new ColorModifier3D(new Color(50, 50, 50), true, plane, 60));
                    plane.AddModifier(new MoveToModifier3D(new Vector3(-1, 0, -1), plane, true, 60));

                    lowerSpiroOrbit = (OrbitModifier3D)lowerSpiroOrbit.DeepCopy(lowerSpirograph);
                    lowerSpirograph.ClearModifiers();
                    lowerSpirograph.AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(-0.01f, 0, 0), false, -1));
                    lowerSpirograph.AddModifier(lowerSpiroOrbit);
                    lowerSpirograph.AddModifier(new SpirographLerpModifier(-1, -1, -1, 1.2f, -1, true, lowerSpirograph, 60));
                    lowerSpirograph.AddModifier(new ColorModifier3D(Color.Lerp(PrimaryColor, Color.Black, 0.5f), true, lowerSpirograph, 60));
                    setGates(0);
                }
            } 
        }
        private bool playerEntered;
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
                    lowerSpirograph.AddModifier(new ColorModifier3D(Color.TransparentBlack, true, lowerSpirograph, 90));
                    setGates(0);
                }
                else
                {
                    isVisible = true;
                    if (PlayerEntered)
                    {
                        plane.AddModifier(new ColorModifier3D(Color.White, true, plane, 90));
                        lowerSpirograph.AddModifier(new ColorModifier3D(PrimaryColor, true, lowerSpirograph, 90));
                        setGates(gateLevel);
                    }
                    else
                    {
                        plane.AddModifier(new ColorModifier3D(new Color(50, 50, 50), true, plane, 90));
                        lowerSpirograph.AddModifier(new ColorModifier3D(Color.Lerp(PrimaryColor, Color.Black, 0.8f), true, lowerSpirograph, 90));
                        setGates(0);
                    }
                }
            }
        }
        /// <summary>
        /// Spirographs leading from the player world to Skaia.
        /// </summary>
        private Spirograph[] gates;
        /// <summary>
        /// Spirograph underneath the player world.
        /// </summary>
        private Spirograph lowerSpirograph;
        //Keeping this around here to ease some state transitions
        private OrbitModifier3D lowerSpiroOrbit;

        public static PlayerWorld BuildPlayerWorld(string path, DynamicContentManager content)
        {
            return new PlayerWorld(path, content);
        }

        public PlayerWorld() { }

        public PlayerWorld(string filePath, DynamicContentManager Content)
        {
            PlayerWorldBuilder builder = PlayerWorldBuilder.BuilderRead(filePath);
            plane = new TexturedPlane(builder.Image);
            plane.billboard = true;
            PrimaryColor = new Color((byte)builder.PrimaryColor.X, (byte)builder.PrimaryColor.Y, (byte)builder.PrimaryColor.Z, (byte)builder.PrimaryColor.W);
            SecondaryColor = new Color((byte)builder.SecondaryColor.X, (byte)builder.SecondaryColor.Y, (byte)builder.SecondaryColor.Z, (byte)builder.SecondaryColor.W);
            Name = builder.Name;
            Description = builder.Description;
            gateLevel = builder.GateLevel;
            PlayerName = builder.PlayerName;
            PlayerEntered = builder.PlayerEntered;
            isVisible = builder.IsVisible;
            CursorDetectRadius = builder.CursorDetectRadius;
            if (builder.InitialAngle != 0 || builder.OrbitalSpeed != 0 || builder.OrbitHeight != 0 || builder.DistFromCenter != 0)
            {
                UserPlaced = true;
                initialDistFromCenter = builder.DistFromCenter;
                initialAngle = builder.InitialAngle;
                initialOrbitalSpeed = builder.OrbitalSpeed;
                initialOrbitHeight = builder.OrbitHeight;
                planeOrbit = new OrbitModifier3D(new Vector3(0, builder.OrbitHeight, 0), Vector3.Up, builder.DistFromCenter, builder.InitialAngle, builder.OrbitalSpeed, plane, false, -1);
                plane.AddModifier(planeOrbit);
            }
            else
                UserPlaced = false;
        }

        private PlayerWorld(TexturedPlane image, Color primaryColor, Color secondaryColor, string name, string description, int gate, string playerName, bool playerEntered, bool isVisible, float initAngle, float initDist, float initSpeed, float initHeight)
        {
            plane = image;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            Name = name;
            Description = description;
            gateLevel = gate;
            PlayerName = playerName;
            this.playerEntered = playerEntered;
            this.isVisible = isVisible;
            if (initAngle != 0 || initSpeed != 0 || initHeight != 0 || initDist != 0)
            {
                UserPlaced = true;
                initialDistFromCenter = initDist;
                initialAngle = initAngle;
                initialOrbitalSpeed = initSpeed;
                initialOrbitHeight = initHeight;
                planeOrbit = new OrbitModifier3D(new Vector3(0, initHeight, 0), Vector3.Up, initDist, initAngle, initSpeed, plane, false, -1);
                plane.AddModifier(planeOrbit);
            }
            else
                UserPlaced = false;
        }

        public void Initialize(float distFromCenter, float orbitAngle, float orbitSpeed)
        {
            gates = new Spirograph[7];

            if (PlayerEntered == true)
            {
                plane.Color = Color.White;
                planeOrbit = new OrbitModifier3D(new Vector3(0, 8, 0), Vector3.Up, distFromCenter, orbitAngle, orbitSpeed, plane, false, -1);
                plane.AddModifier(planeOrbit);
                lowerSpirograph = new Spirograph(1f, 10f / 7f, 7, 1, 3, 0, PrimaryColor);
                lowerSpirograph.AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(0.02f, 0, 0), false, -1));
            }
            else
            {
                plane.Color = new Color(50, 50, 50);
                planeOrbit = new OrbitModifier3D(Vector3.Zero, Vector3.Up, distFromCenter, orbitAngle, orbitSpeed, plane, false, -1);
                plane.AddModifier(planeOrbit);
                lowerSpirograph = new Spirograph(1f, 10f / 7f, 7, 0.4f, 3, 0, Color.Lerp(PrimaryColor, Color.Black, 0.8f));
                lowerSpirograph.AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(-0.01f, 0, 0), false, -1));
            }
            lowerSpirograph.Generate(300);
            lowerSpiroOrbit = new OrbitModifier3D(Vector3.Zero, Vector3.Up, distFromCenter, orbitAngle, orbitSpeed, lowerSpirograph, false, -1);
            lowerSpirograph.AddModifier(lowerSpiroOrbit);
            lowerSpirograph.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.PiOver2);

            for (int i = 0; i < 7; i++)
            {
                float gateDist = ((distFromCenter - 20) / 7) * (6 - i) + 20;
                if (PlayerEntered == true)
                {
                    gates[i] = new Spirograph(1f, 10f / 7f, 7, 1, 0.2f, 0, SecondaryColor);
                    gates[i].Generate(140);
                }
                else
                {
                    gates[i] = new Spirograph(1f, 10f / 7f, 7, 1, 0.2f, 0, Color.TransparentBlack);
                    gates[i].Generate(140);
                }
                gates[i].Rotation = Quaternion.CreateFromYawPitchRoll(orbitAngle, 0, 0);//(MathHelper.TwoPi - (MathHelper.TwoPi / currentSession.Lands.Length)) * i
                gates[i].AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(-orbitSpeed, 0, 0), false, -1));
                gates[i].WorldPosition = new Vector3(0, 8, 0);
                gates[i].AddModifier(new OrbitModifier3D(new Vector3(0, 8, 0), Vector3.Up, gateDist, orbitAngle, orbitSpeed, gates[i], false, -1));
            }
        }

        public override void HandleMessages(List<LocationMessage> messages)
        {
            //Throw exceptions for messages this object can't handle
            //Messages valid for this object:
            //PlayerEntry, Reload, PlayerGodTier, PlayerDeath, SetVisibility, ChangeOrbit, SetOrbit, ChangeGateLevel,
            //Filter out useless/overridden messages
            //Handle any remaining messages sequentially
            LocationMessage?[] foundMessages = new LocationMessage?[] { null, null, null, null, null, null };
            for (int i = 0; i < messages.Count; i++)
            {
                //SetOrbit goes with first
                //Reload replaces
                //ChangeOrbit replaces
                //SetVisibility replaces
                //ChangeGateLevel replaces
                //PlayerEntry goes with first
                //PlayerDeath goes with first
                switch (messages[i].EventType)
                {
                    case EventTypes.SetOrbit:
                        if (foundMessages[0] == null)
                            foundMessages[0] = messages[i];
                        break;
                    case EventTypes.Reload:
                        foundMessages[1] = messages[i];
                        break;
                    case EventTypes.ChangeOrbit:
                        foundMessages[2] = messages[i];
                        break;
                    case EventTypes.SetVisibility:
                        foundMessages[3] = messages[i];
                        break;
                    case EventTypes.ChangeGateLevel:
                        foundMessages[4] = messages[i];
                        break;
                    case EventTypes.PlayerEntry:
                        if (foundMessages[5] == null)
                            foundMessages[5] = messages[i];
                        break;
                    default:
                        throw new ArgumentException("Message" + i.ToString() + " of type " + messages[i].EventType.ToString() + " is not valid for PlayerWorld " + this.Name);
                }
            }
            if (foundMessages[0] != null)
            {
                string dist, angle, speed, height;
                string message = foundMessages[0].Value.Message;
                dist = message.Substring(0, message.IndexOf(' ') - 1);
                angle = message.Substring(1, message.IndexOf(' ', 1) - 1);
                speed = message.Substring(2, message.IndexOf(' ', 2) - 1);
                height = message.Substring(3, message.IndexOf(' ', 3) - 1);
                initialDistFromCenter = float.Parse(dist);
                initialAngle = float.Parse(angle);
                initialOrbitalSpeed = float.Parse(speed);
                initialOrbitHeight = float.Parse(height);
            }
            if (foundMessages[1] != null)
                reload(foundMessages[1].Value.Message, ScreenManager.Content);
            if (foundMessages[2] != null)
            {
                float dist, angle, speed, height;
                string message = foundMessages[1].Value.Message;
                dist = float.Parse(message.Substring(0, message.IndexOf(' ') - 1));
                angle = float.Parse(message.Substring(1, message.IndexOf(' ', 1) - 1));
                speed = float.Parse(message.Substring(2, message.IndexOf(' ', 2) - 1));
                height = float.Parse(message.Substring(3, message.IndexOf(' ', 3) - 1));
                //OrbitInterpolator

                //Plan:
                //  Add Inactive() Event to modifiers
                //  Use to properly implement IsVisible
            }
            if (foundMessages[3] != null)
                IsVisible = bool.Parse(foundMessages[3].Value.Message);
            if (foundMessages[4] != null)
            {
                gateLevel = int.Parse(foundMessages[4].Value.Message);
                if (PlayerEntered)
                    setGates(gateLevel);
            }
            if (foundMessages[5] != null)
            {
                PlayerEntered = bool.Parse(foundMessages[5].Value.Message);
            }
        }
        /*
            if (prevLandStates[i] == true)
            {
                OrbitModifier3D temp = (OrbitModifier3D)currentSession.Lands[i].Modifiers[0];
                currentSession.Lands[i].ClearModifiers();
                currentSession.Lands[i].AddModifier(temp.DeepCopy(currentSession.Lands[i]));
                currentSession.Lands[i].AddModifier(new ColorModifier3D(Color.White, true, currentSession.Lands[i], 60));
                currentSession.Lands[i].AddModifier(new MoveToModifier3D(new Vector3(-1, 8, -1), currentSession.Lands[i], true, 60));

                temp = (OrbitModifier3D)planetarySpirographs[i].Modifiers[0];
                planetarySpirographs[i].ClearModifiers();
                planetarySpirographs[i].AddModifier(temp.DeepCopy(planetarySpirographs[i]));
                planetarySpirographs[i].AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(0.02f, 0, 0), false, -1));
                planetarySpirographs[i].AddModifier(new SpirographLerpModifier(-1, -1, -1, 3, -1, true, planetarySpirographs[i], 60));
                planetarySpirographs[i].AddModifier(new ColorModifier3D(currentSession.Lands[i].primaryColor, true, planetarySpirographs[i], 60));

                for (int h = 0; h < 7; h++)
                    gateSpirographs[i, h].AddModifier(new ColorModifier3D(currentSession.Lands[i].secondaryColor, true, gateSpirographs[i, h], 60));
            }
            else
            {
                OrbitModifier3D temp = (OrbitModifier3D)currentSession.Lands[i].Modifiers[0];
                currentSession.Lands[i].ClearModifiers();
                currentSession.Lands[i].AddModifier(temp.DeepCopy(currentSession.Lands[i]));
                currentSession.Lands[i].AddModifier(new ColorModifier3D(new Color(50, 50, 50), true, currentSession.Lands[i], 60));
                currentSession.Lands[i].AddModifier(new MoveToModifier3D(new Vector3(-1, 0, -1), currentSession.Lands[i], true, 60));

                temp = (OrbitModifier3D)planetarySpirographs[i].Modifiers[0];
                planetarySpirographs[i].ClearModifiers();
                planetarySpirographs[i].AddModifier(temp.DeepCopy(planetarySpirographs[i]));
                planetarySpirographs[i].AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(-0.01f, 0, 0), false, -1));
                planetarySpirographs[i].AddModifier(new SpirographLerpModifier(-1, -1, -1, 1.2f, -1, true, planetarySpirographs[i], 60));
                planetarySpirographs[i].AddModifier(new ColorModifier3D(Color.Lerp(currentSession.Lands[i].primaryColor, Color.Black, 0.5f), true, planetarySpirographs[i], 60));

                for (int h = 0; h < 7; h++)
                    gateSpirographs[i, h].AddModifier(new ColorModifier3D(Color.TransparentBlack, true, gateSpirographs[i, h], 60));
            }
         */

        public override void Update(GameTime gameTime)
        {
            plane.Update(gameTime);
            foreach (Spirograph spirograph in gates)
                spirograph.Update(gameTime);
            lowerSpirograph.Update(gameTime);
        }
        
        public override List<DrawListItem> GetDrawables(Camera camera)
        {
            if (IsVisible)
            {
                List<DrawListItem> drawableList = new List<DrawListItem>(gateLevel + 2);
                drawableList.Add(new DrawListItem(camera, plane));
                drawableList.Add(new DrawListItem(camera, lowerSpirograph));
                for (int i = 0; i < gateLevel; i++)
                    drawableList.Add(new DrawListItem(camera, gates[i]));
                return drawableList;
            }
            else
                return new List<DrawListItem>();
        }

        private void reload(string filePath, DynamicContentManager Content)
        {
            PlayerWorld newState = Content.Load<PlayerWorld>(filePath);
            TexturedPlane oldPlane = plane;
            plane = newState.plane;
            plane.Color = oldPlane.Color;
            plane.WorldPosition = oldPlane.WorldPosition;
            //planeOrbit = (OrbitModifier3D)planeOrbit.DeepCopy(plane);
            //plane.AddModifier(planeOrbit);
            IsVisible = newState.isVisible;
            PrimaryColor = newState.PrimaryColor;
            SecondaryColor = newState.SecondaryColor;
            Name = newState.Name;
            Description = newState.Description;
            PlayerName = newState.PlayerName;
            if (newState.CursorDetectRadius > 0)
                CursorDetectRadius = newState.CursorDetectRadius;

            if (gateLevel != newState.gateLevel && PlayerEntered)
                setGates(gateLevel);
            gateLevel = newState.gateLevel;

            for (int i = 0; i < oldPlane.Modifiers.Length; i++)
            {
                if (oldPlane.Modifiers[i] != null)
                {
                    IModifier3D newMod = oldPlane.Modifiers[i].DeepCopy(plane);
                    if (oldPlane.Modifiers[i] == planeOrbit)
                        planeOrbit = (OrbitModifier3D)newMod;
                    plane.AddModifier(newMod);
                }
            }
        
            if (PlayerEntered != newState.PlayerEntered)
            {
                PlayerEntered = newState.PlayerEntered;
                for (int i = 0; i < lowerSpirograph.Modifiers.Length; i++)
                {
                    if (lowerSpirograph.Modifiers[i] != null && lowerSpirograph.Modifiers[i] is ColorModifier3D)
                    {
                        lowerSpirograph.Modifiers[i].Remove();
                        i--;
                    }
                }
                for (int i = 0; i < plane.Modifiers.Length; i++)
                {
                    if (plane.Modifiers[i] != null && plane.Modifiers[i] is ColorModifier3D)
                    {
                        plane.Modifiers[i].Remove();
                        i--;
                    }
                }
                if (PlayerEntered)
                {
                    lowerSpirograph.AddModifier(new ColorModifier3D(PrimaryColor, true, lowerSpirograph, 90));
                    plane.AddModifier(new ColorModifier3D(Color.White, true, plane, 90));
                    setGates(gateLevel);
                }
                else
                {
                    lowerSpirograph.AddModifier(new ColorModifier3D(Color.Lerp(PrimaryColor, Color.Black, 0.8f), true, lowerSpirograph, 90));
                    plane.AddModifier(new ColorModifier3D(new Color(50, 50, 50), true, plane, 90));
                    setGates(0);
                }
            }
            //else if (!PlayerEntered)
            //    setGates(0);
        }

        /// <summary>
        /// Sets the given number of gates to be visible.  Remaining gates will be set as invisible.
        /// </summary>
        /// <param name="newGateLevel">Number of gates that will be visible.</param>
        private void setGates(int newGateLevel)
        {
            for (int i = 0; i < 7; i++)
            {
                if (i < newGateLevel)
                {
                    if (gates[i].Color != SecondaryColor)
                    {
                        for (int h = 0; h < gates[i].Modifiers.Length; h++)
                        {
                            if (gates[i].Modifiers[h] != null && gates[i].Modifiers[h] is ColorModifier3D)
                            {
                                gates[i].Modifiers[h].Remove();
                                h--;
                            }
                        }
                        gates[i].AddModifier(new ColorModifier3D(Color.TransparentBlack, true, gates[i], 90));
                    }
                }
                else if (gates[i].Color != Color.TransparentBlack)
                {
                    for (int h = 0; h < gates[i].Modifiers.Length; h++)
                    {
                        if (gates[i].Modifiers[h] != null && gates[i].Modifiers[h] is ColorModifier3D)
                        {
                            gates[i].Modifiers[h].Remove();
                            h--;
                        }
                    }
                    gates[i].AddModifier(new ColorModifier3D(Color.TransparentBlack, true, gates[i], 90));
                }
            }
        }

        #region ICopyable<Planet> Members

        public PlayerWorld ShallowCopy()
        {
            PlayerWorld clone = new PlayerWorld();
            clone.PlayerEntered = PlayerEntered;
            clone.Name = Name;
            clone.Description = Description;
            clone.gateLevel = gateLevel;
            clone.plane = plane;
            clone.PrimaryColor = PrimaryColor;
            clone.SecondaryColor = SecondaryColor;
            clone.IsVisible = IsVisible;
            return clone;
        }

        public PlayerWorld DeepCopy()
        {
            PlayerWorld clone = new PlayerWorld(plane.DeepCopy(), PrimaryColor, SecondaryColor, Name, 
                Description, gateLevel, PlayerName, PlayerEntered, IsVisible, initialAngle, 
                initialDistFromCenter, initialOrbitalSpeed, initialOrbitHeight);
            clone.CursorDetectRadius = CursorDetectRadius;
            return clone;
        }

        #endregion
    }
}
