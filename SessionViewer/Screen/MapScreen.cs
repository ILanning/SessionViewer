using CommonCode;
using CommonCode.Collision;
using CommonCode.Drawing;
using CommonCode.Modifiers;
using CommonCode.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SessionViewer
{
    class MapScreen : Screen
    {

        TexturedPlane cursor;
        Location cursorTarget;
        /// <summary>
        /// If true, the camera is currently zoomed in on a world.
        /// </summary>
        bool focused = false;

        string sessionPath;
        SessionManager currentSession;
        /// <summary>
        /// The graphical interface the user uses to manipulate the timeline.
        /// </summary>
        //TimelineControl timelineControl;

        /// <summary>
        /// The button that opens and closes the description at the bottom of the screen.
        /// </summary>
        Button descriptionControl;
        /// <summary>
        /// The graphical box around the description.
        /// </summary>
        Button descriptionBox;
        TextBox descriptionText;
        bool descriptionOpen = false;

        /// <summary>
        /// Array representing the previous PlayerEntry states.
        /// </summary>
        //bool[] prevLandStates;
        //bool prevReckoningState = false;

        /// <summary>
        /// The ring between Skaia and the player worlds.
        /// </summary>
        TexturedPlane innerRim;
        /// <summary>
        /// The ring between the player worlds and Derse.
        /// </summary>
        TexturedPlane outerRim;
        /// <summary>
        /// The un-interactable meteors beyond the outer rim.
        /// </summary>
        GenericMeteor[] genericMeteors;

        /// <summary>
        /// If true, the timeline has reached the scratch event, which overrides anything else on the screen.
        /// </summary>
        public bool ShowingScratchText = false;
        int scratchedTextTimer = 0;
        int timeToMessageDecay = 0;
        string lastMessage = "";
        /// <summary>
        /// The maximum distance the point the camera focuses on can be from the center of the map on the x-z plane.
        /// </summary>
        float cameraRange;
        AlphaTestEffect alphaTester;

        public MapScreen(string sessionFile) : base()
        {
            sessionPath = sessionFile;
            Draw3DFirst = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            cursor = new TexturedPlane(Vector3.Zero, Quaternion.Identity, new Vector2(7), Color.White, false,
                ScreenManager.Content.Load<Texture2D>(".//UI//Cursor.png"));

            currentSession = ScreenManager.Content.Load<SessionManager>(sessionPath);
            float innerRimSize = 90;
            innerRim = new TexturedPlane(new Vector3(0, 5, 0), Quaternion.Identity, new Vector2(innerRimSize), Color.White,
                false, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//Inner Rim.png"));
            float outerRimSize = currentSession.PlayerWorldCount * 25 + innerRimSize;
            outerRim = new TexturedPlane(new Vector3(0, 5, 0), Quaternion.Identity, new Vector2(outerRimSize), Color.White,
                false, ScreenManager.Content.Load<Texture2D>(".//Miscellaneous//Outer Rim.png"));

            ScreenManager.Globals.Camera = new SphericalCamera(new Vector3(0, 4, 0), new Vector2(0, 60), 125);
            (ScreenManager.Globals.Camera as SphericalCamera).MaxZoomLevel = 600;
            cameraRange = (outerRimSize / 2) + 45f;

            descriptionControl = new Button(new AABox(new Rectangle(0, 0, 136, 23)), position: new Coordinate(0, 578));
            descriptionBox = new Button(null, ScreenManager.Content.Load<Texture2D>(".//UI//Description Box.png"), null, new Coordinate(0, 574));
            descriptionText = new TextBox("", new Rectangle(5, 605, 790, 186), "Default");
            descriptionControl.Clicked += OnOpenDescription;

            float outerRimCircumference = (float)(outerRimSize * Math.PI);
            Random rand = new Random(4);
            if (outerRimCircumference < 1000)
                genericMeteors = new GenericMeteor[(int)outerRimCircumference];
            else
                genericMeteors = new GenericMeteor[1000];
            for (int i = 0; i < genericMeteors.Length; i++)
            {
                float angleOffset = (float)(rand.NextDouble() - 0.5);
                float distOffset = (float)(rand.NextDouble() - 0.5) * 10f;
                float yOffset = (float)(rand.NextDouble() - 0.5) * 3f;
                genericMeteors[i] = currentSession.GenericMeteors[rand.Next(currentSession.GenericMeteors.Length)].DeepCopy();
                genericMeteors[i].AddModifier(new OrbitModifier3D(new Vector3(0, 8 + yOffset, 0), Vector3.Up,
                    (outerRimSize / 2) + 7.5f + distOffset, (float)((Math.PI * 2) * ((float)i / genericMeteors.Length)) + angleOffset,
                    -0.002f, genericMeteors[i], false, -1));
            }
            //foreach (var keyValue in currentSession.Worlds)
            //    keyValue.Value.Initialize();
            if(currentSession.Worlds["Prospit"].UserPlaced == false)
                currentSession.Worlds["Prospit"].WorldPlane.AddModifier(new OrbitModifier3D(new Vector3(0, 12, 0), 
                    Vector3.Up, 15, 0, -0.002f, currentSession.Worlds["Prospit"].WorldPlane, false, -1));
            if (currentSession.Worlds["Derse"].UserPlaced == false)
                currentSession.Worlds["Derse"].WorldPlane.AddModifier(new OrbitModifier3D(new Vector3(0, 12, 0),
                    Vector3.Up, outerRimSize / 2 + 15, 1, 0.002f, currentSession.Worlds["Derse"].WorldPlane, false, -1));
            //Prospit: new Vector3(0, 12, 0), Vector3.Up, 15, 0, -0.002f  Derse: new Vector3(0, 12, 0), Vector3.Up, outerRimSize / 2 + 15, 1, 0.002f
            int h = 0;
            float planetDist = (outerRimSize / 2 - innerRimSize / 2) / 2 + innerRimSize / 2;
            foreach (var keyValue in currentSession.Worlds)
            {
                if (keyValue.Value is PlayerWorld)
                {
                    PlayerWorld world = keyValue.Value as PlayerWorld;
                    world.Initialize(planetDist, (MathHelper.TwoPi / currentSession.PlayerWorldCount) * h, 0.003f);
                    h++;
                }
            }
            //timelineControl = new TimelineControl(currentSession, this);
            alphaTester = new AlphaTestEffect(ScreenManager.Globals.Graphics);
        }

        public override void UnloadContent()
        {
            //ScreenManager.Content.UnloadContent();
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            currentSession.UpdateTimeline();
            cursor.Update(gameTime);
            ScreenManager.Globals.Camera.HandleInput();

            Vector2 tempCameraPos = new Vector2(ScreenManager.Globals.Camera.LookAtPosition.X, ScreenManager.Globals.Camera.LookAtPosition.Z);
            if (tempCameraPos.LengthSquared() > cameraRange * cameraRange)
            {
                tempCameraPos.Normalize();
                tempCameraPos *= cameraRange;
                ScreenManager.Globals.Camera.AddModifier(new MoveToModifier3D(new Vector3(tempCameraPos.X, ScreenManager.Globals.Camera.LookAtPosition.Y, tempCameraPos.Y), ScreenManager.Globals.Camera, true, 1));
            }
            if (!focused && cursorTarget != null && InputManager.IsMouseButtonTriggered(MouseButtons.LMB))
            {
                focused = true;
                ScreenManager.Globals.Camera.InputIndependent = true;
                ScreenManager.Globals.Camera.AddModifier(new MoveToModifier3D(cursorTarget.WorldPlane, ScreenManager.Globals.Camera, false, 30));
                ScreenManager.Globals.Camera.AddModifier(new ScaleToModifier3D(new Vector3(20, -1, -1), ScreenManager.Globals.Camera, false, 30));
                Vector3 rotationVector = ScreenManager.Globals.Camera.CameraPosition - ScreenManager.Globals.Camera.LookAtPosition;
                Vector3 rightNormal = Vector3.Cross(rotationVector, Vector3.Down);
                rightNormal.Normalize();
                ScreenManager.Globals.Camera.AddModifier(new RotateToModifier3D(Quaternion.CreateFromAxisAngle(rightNormal, MathHelper.Pi / 3f), ScreenManager.Globals.Camera, true, 30));
                if (descriptionOpen)
                    descriptionText.Text = cursorTarget.Description;
            }

            if (focused)
            {
                if (ScreenManager.Globals.Camera.Modifiers[0].GetType() == typeof(MoveToModifier3D) && !ScreenManager.Globals.Camera.Modifiers[0].Active)
                {
                    ScreenManager.Globals.Camera.ClearModifiers();
                    ScreenManager.Globals.Camera.AddModifier(new FollowModifier3D(cursorTarget.WorldPlane, false, false, -1));
                    if (descriptionOpen)
                        ScreenManager.Globals.Camera.AddModifier(new MoveToModifier3D(new Vector3(-1, ScreenManager.Globals.Camera.LookAtPosition.Y - 2.25f, -1), ScreenManager.Globals.Camera, false, 20));
                    //ScreenManager.Globals.Camera.AddModifier(new RotateModifier3D(Quaternion.CreateFromAxisAngle(Vector3.Up, 0.005f), false, -1));
                }
                if (InputManager.IsMouseButtonTriggered(MouseButtons.RMB))
                {
                    focused = false;
                    ScreenManager.Globals.Camera.InputIndependent = false;
                    ScreenManager.Globals.Camera.ClearModifiers();
                    if (descriptionOpen)
                        descriptionText.Text = currentSession.Description;
                }
            }
            if (lastMessage != currentSession.LatestMessage)
            {
                lastMessage = currentSession.LatestMessage;
                timeToMessageDecay = 120;
            }

            descriptionControl.Update(gameTime);
            descriptionBox.Update(gameTime);
            descriptionText.Update();

            foreach (var keyValue in currentSession.Worlds)
                keyValue.Value.Update(gameTime);
            foreach (GenericMeteor meteor in genericMeteors)
                meteor.Update(gameTime);

            ScreenManager.Globals.Camera.Update();
            ScreenManager.Globals.DefaultEffect.View = Matrix.CreateLookAt(ScreenManager.Globals.Camera.CameraPosition, ScreenManager.Globals.Camera.LookAtPosition, Vector3.Up);

            if (ShowingScratchText)
            {
                scratchedTextTimer++;
                if (scratchedTextTimer == 90)
                    scratchedTextTimer -= 90;
            }
            //timelineControl.Update();

            if (focused /*|| timelineControl.IsSelected*/ || descriptionBox.IsHovered || ShowingScratchText)
                ScreenManager.IsMouseVisible = true;
            else 
                ScreenManager.IsMouseVisible = false;
        }

        public override void HandleInput(GameTime gameTime)
        {
            Vector3 nearsource = new Vector3(InputManager.MousePosition, 0f);
            Vector3 farsource = new Vector3(InputManager.MousePosition, 1f);
            Matrix worldMatrix = Matrix.CreateTranslation(0, 0, 0);
            Vector3 nearPoint = ScreenManager.Globals.Graphics.Viewport.Unproject(nearsource, ScreenManager.Globals.DefaultEffect.Projection, ScreenManager.Globals.DefaultEffect.View, worldMatrix);
            Vector3 farPoint = ScreenManager.Globals.Graphics.Viewport.Unproject(farsource, ScreenManager.Globals.DefaultEffect.Projection, ScreenManager.Globals.DefaultEffect.View, worldMatrix);
            Vector3 mouseRayDirection = farPoint - nearPoint;

            float t = (0.1f - nearPoint.Y) / mouseRayDirection.Y;
            Vector3 mousePlanePos = new Vector3(nearPoint.X + mouseRayDirection.X * t, 0.1f, nearPoint.Z + mouseRayDirection.Z * t);

            cursor.WorldPosition = mousePlanePos;

            if (!focused && !ScreenManager.IsMouseVisible)
            {
                bool foundTarget = false;
                foreach (var keyValue in currentSession.Worlds)
                {
                    Location location = keyValue.Value;
                    Vector3 distance = new Vector3(cursor.WorldPosition.X - location.WorldPlane.WorldPosition.X, 0, cursor.WorldPosition.Z - location.WorldPlane.WorldPosition.Z);
                    if (distance.LengthSquared() < location.CursorDetectRadius * location.CursorDetectRadius &&
                        location.IsVisible)
                    {
                        if (keyValue.Key == "Skaia")
                        { }
                        cursorTarget = location;
                        if (!(cursor.Modifiers[0] is ColorModifier3D))
                            cursor.AddModifier(new ColorModifier3D(location.PrimaryColor, true, cursor, 10));
                        foundTarget = true;
                        break;
                    }
                }
                if (!foundTarget)
                {
                    cursorTarget = null;
                    if (!(cursor.Modifiers[0] is ColorModifier3D))
                        cursor.AddModifier(new ColorModifier3D(Color.White, true, cursor, 10));
                }
            }

            descriptionControl.HandleInput();
            descriptionBox.HandleInput();
            descriptionText.HandleInput();

            //timelineControl.HandleInput();

            if (InputManager.IsKeyTriggered(Keys.Escape))
                RemoveSelf();
        }

        public void OnOpenDescription(object sender, EventArgs e)
        {
            if (descriptionBox.Modifiers[0] == null)
            {
                if (!descriptionOpen)
                {
                    if (focused)
                    {
                        descriptionText = new TextBox(cursorTarget.Description, new Rectangle(5, 605, 790, 186), "Default");
                        ScreenManager.Globals.Camera.AddModifier(new MoveToModifier3D(new Vector3(-1, ScreenManager.Globals.Camera.LookAtPosition.Y - 2.25f, -1), ScreenManager.Globals.Camera, false, 20));
                    }
                    else
                        descriptionText = new TextBox(currentSession.Description, new Rectangle(5, 605, 790, 186), "Default");
                    descriptionBox.AddModifier(new MoveToModifier2D(new Vector2(0, 377), descriptionBox, true, 20));
                    descriptionControl.AddModifier(new MoveToModifier2D(new Vector2(0, 381), descriptionControl, true, 20));
                    descriptionText.AddModifier(new MoveToModifier2D(new Vector2(5, 408), descriptionText, true, 20));
                    descriptionOpen = true;
                }
                else
                {
                    if (focused)
                        ScreenManager.Globals.Camera.AddModifier(new MoveToModifier3D(new Vector3(-1, ScreenManager.Globals.Camera.LookAtPosition.Y + 2.25f, -1), ScreenManager.Globals.Camera, false, 20));
                    descriptionBox.AddModifier(new MoveToModifier2D(new Vector2(0, 574), descriptionBox, true, 20));
                    descriptionControl.AddModifier(new MoveToModifier2D(new Vector2(0, 578), descriptionControl, true, 20));
                    descriptionText.AddModifier(new MoveToModifier2D(new Vector2(5, 605), descriptionText, true, 20));
                    descriptionOpen = false;
                }
            }
        }

        List<DrawListItem> depthDrawList = new List<DrawListItem>();
        List<DrawListItem> paintersDrawList = new List<DrawListItem>();

        public void Draw3D()
        {
            if (!ShowingScratchText)
            {
                GraphicsDevice graphics = ScreenManager.Globals.Graphics;

                ScreenManager.Globals.DefaultEffect.VertexColorEnabled = true;
                alphaTester.Projection = ScreenManager.Globals.DefaultEffect.Projection;
                alphaTester.View = ScreenManager.Globals.DefaultEffect.View;
                graphics.DepthStencilState = DepthStencilState.Default;

                BlendState newBlendState = new BlendState();
                newBlendState.ColorSourceBlend = Blend.SourceAlpha;
                newBlendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                graphics.BlendState = newBlendState;

                RasterizerState newRasterizer = new RasterizerState();
                newRasterizer.CullMode = CullMode.None;
                graphics.RasterizerState = newRasterizer;
                //ScreenManager.Globals.Graphics.BlendState.ColorSourceBlend = Blend.SourceAlpha;
                //ScreenManager.Globals.Graphics.BlendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                //ScreenManager.Globals.Graphics.RasterizerState.CullMode = CullMode.None;

                #region Sorting


                depthDrawList.Clear();
                paintersDrawList.Clear();

                foreach (var keyValue in currentSession.Worlds)
                    depthDrawList.AddRange(keyValue.Value.GetDrawables(ScreenManager.Globals.Camera));
                //depthDrawList.Add(new DrawListItem(ScreenManager.Globals.Camera, cursor));
                foreach (GenericMeteor meteor in genericMeteors)
                    depthDrawList.Add(new DrawListItem(ScreenManager.Globals.Camera, meteor));

                if (focused && cursorTarget.Name == "Skaia")
                    depthDrawList.Add(new DrawListItem(ScreenManager.Globals.Camera, ((Skaia)currentSession.Worlds["Skaia"]).Battlefield));

                paintersDrawList.AddRange(depthDrawList);

                if (!ScreenManager.IsMouseVisible)
                    depthDrawList.Add(new DrawListItem(ScreenManager.Globals.Camera, cursor));


                depthDrawList.Sort(DrawListItem.DepthCompare);
                paintersDrawList.Sort(DrawListItem.DistanceCompare);

                #endregion
                graphics.DepthStencilState = DepthStencilState.DepthRead;
                foreach (DrawListItem item in paintersDrawList)
                {
                    item.ListItem.Draw(alphaTester, graphics);
                }
                innerRim.Draw(alphaTester, graphics);
                outerRim.Draw(alphaTester, graphics);

                DepthStencilState newDepthState = new DepthStencilState();
                newDepthState.DepthBufferEnable = true;
                //newDepthState.DepthBufferFunction = CompareFunction.Greater;
                newDepthState.DepthBufferWriteEnable = true;
                graphics.DepthStencilState = newDepthState;
                innerRim.Draw(alphaTester, graphics);
                outerRim.Draw(alphaTester, graphics);
                foreach (DrawListItem item in depthDrawList)
                {
                    if (item.ListItem is Spirograph)
                        item.ListItem.Draw(ScreenManager.Globals.DefaultEffect, graphics);
                    else
                        item.ListItem.Draw(alphaTester, graphics);
                }
            }
        }

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        public override void Draw(GameTime gameTime)
        {
            Draw3D();
            Draw2D(gameTime);
        }

        public void Draw2D(GameTime gameTime)
        {
            SpriteBatch sb = ScreenManager.Globals.sb;
            sb.Begin(blendState: BlendState.NonPremultiplied);

            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
            frameCounter++;

            //timelineControl.Draw(ScreenManager.Globals.sb);
            if (!ShowingScratchText && cursorTarget != null)
            {
                float nameWidth = ScreenManager.Globals.Fonts["Carima"].MeasureString(cursorTarget.Name).X;
                sb.DrawString(ScreenManager.Globals.Fonts["Carima"], cursorTarget.Name, new Vector2((float)Math.Round(ScreenManager.Globals.Graphics.Viewport.Width / 2 - nameWidth / 2), (float)Math.Round(52f/* + timelineControl.WorldPosition.Y*/)), Color.White);
            }

            if (ShowingScratchText)
            {
                ScreenManager.IsMouseVisible = true;
                if (scratchedTextTimer / 45 == 1)
                {
                    SpriteFont font = ScreenManager.Globals.Fonts["Consolas"];
                    sb.DrawString(font, "-- [Contact Lost] --",
                        new Vector2(ScreenManager.Globals.Graphics.Viewport.Width / 2 - font.MeasureString("-- [Contact Lost] --").X / 2,
                            ScreenManager.Globals.Graphics.Viewport.Height / 2 - font.MeasureString("-- [Contact Lost] --").Y / 2), Color.Lime);
                }
            }

            float drawPosOffset = 74;// + timelineControl.WorldPosition.Y;
            if (timeToMessageDecay > -1)
            {
                sb.DrawString(ScreenManager.Globals.Fonts["Default"], currentSession.LatestMessage, new Vector2(0, drawPosOffset), Color.White);
                drawPosOffset += 20;
                timeToMessageDecay--;
            }
            descriptionBox.Draw(sb);
            descriptionText.Draw(sb);
            string fps = string.Format("FPS: {0}", frameRate);
            for (int i = 0, j = 0; i < ScreenManager.Globals.Camera.Modifiers.Length; i++)//each (IModifier3D modifier in ScreenManager.Globals.Camera.Modifiers)
            {
                if (ScreenManager.Globals.Camera.Modifiers[i] != null)
                {
                    string modName = "Mod " + j.ToString() + " Type: " + ScreenManager.Globals.Camera.Modifiers[i].GetType().Name;
                    sb.DrawString(ScreenManager.Globals.Fonts.FontLibrary["Default"], modName, new Vector2(797 - ScreenManager.Globals.Fonts.FontLibrary["Default"].MeasureString(modName).X, j * 20 + 90/* + timelineControl.WorldPosition.Y*/), Color.White);
                    j++;
                }
            }
            //Debug text displays

            Vector3 cameraPos = ScreenManager.Globals.Camera.CameraPosition - ScreenManager.Globals.Camera.LookAtPosition;
            cameraPos.Normalize();
            //Vector3 planeVector = new Vector3(cameraPos.X, 0, cameraPos.Z))
            //planeVector.normalize();
            //float cameraAngle = (float)Math.Acos(Vector3.Dot(cameraPos, planeVector)) * (180/MathHelper.Pi);

            //Simplified way of doing things: We don't need the whole dot product in this case, since the x and z vectors are meaningless to us.
            //Instead, we just have to take the arcsine of the normalized y component to get our angle
            float cameraAngle = (float)Math.Asin(cameraPos.Y) * (180 / MathHelper.Pi);

            //sb.DrawString(ScreenManager.Globals.Fonts.FontLibrary["Default"], fps, new Vector2(797 - ScreenManager.Globals.Fonts.FontLibrary["Default"].MeasureString(fps).X, 70 + timelineControl.WorldPosition.Y), Color.White);
            sb.DrawString(ScreenManager.Globals.Fonts.FontLibrary["Default"], "Look At: " + cameraAngle.ToString(), new Vector2(0, 70/* + timelineControl.WorldPosition.Y*/), Color.White);
            //sb.DrawString(ScreenManager.Globals.Fonts.FontLibrary["Default"], "Position: " + ScreenManager.Globals.Camera.CameraPosition.ToString(), new Vector2(0, 90 + timelineControl.WorldPosition.Y), Color.White);
            sb.DrawString(ScreenManager.Globals.Fonts.FontLibrary["Default"], "Movement: " + ((SphericalCamera)ScreenManager.Globals.Camera).Movement.ToString(), new Vector2(0, 110/* + timelineControl.WorldPosition.Y*/), Color.White);
            //sb.DrawString(ScreenManager.Globals.Fonts.FontLibrary["Default"], "Angle: " + ((SphericalCamera)ScreenManager.Globals.Camera).CameraAngle.ToString(), new Vector2(0, 130/* + timelineControl.WorldPosition.Y*/), Color.White);
            //sb.DrawString(ScreenManager.Globals.fonts.FontLibrary["Default"], "Timeline: " + expandTimeline.WorldPosition.ToString(), new Vector2(0, 48 + timelineControl.WorldPosition.Y), Color.Blue);
            //sb.DrawString(ScreenManager.Globals.fonts.FontLibrary["Default"], "Size: " + expandTimeline.textureSource.ToString(), new Vector2(0, 68 + timelineControl.WorldPosition.Y), Color.Blue);

            sb.End();
        }
    }
}