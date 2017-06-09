using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CommonCode.Modifiers;
using CommonCode;
using CommonCode.Drawing;

namespace SessionViewer
{
    class LoadScreen : Screen
    {
        Spirograph spirograph;
        Camera camera;
        bool flashComplete = false;
        bool shrinkComplete = false;
        bool growComplete = false;
        bool errorThrown = false;
        bool skip = false;
        string sessionPath;
        Thread preloadContentThread;

        public LoadScreen(string sessionFile) : base()
        {
            sessionPath = sessionFile;
        }

        public override void LoadContent()
        {
            ScreenManager.Content = new DynamicContentManager(ScreenManager.StaticGame, ScreenManager.StaticGame.Content.RootDirectory);
            base.LoadContent();
            spirograph = new Spirograph(1, 10f / 7f, 7, 1, 1, 300, new Color(0, 30, 0));
            camera = new SphericalCamera(Vector3.Zero, new Vector2(0, 90), 12);
            ScreenManager.Globals.Camera = camera;
            camera.InputIndependent = true;
            spirograph.Generate(600);
            spirograph.AddModifier(new RotateModifier3D(Quaternion.CreateFromYawPitchRoll(0, -0.01f, 0), false, -1));
            spirograph.AddModifier(new SpirographLerpModifier(-1, -1, -1, -1, 1, false, spirograph, 300));
            spirograph.AddModifier(new ColorModifier3D(new Color(100, 255, 100), false, spirograph, 300));
            flashComplete = false;
            shrinkComplete = false;
            growComplete = false;
            preloadContentThread = new Thread(new ThreadStart(Preload));
            preloadContentThread.Start();
            ScreenManager.IsMouseVisible = false;
        }

        public void Preload()
        {
            //try
            //{
                SessionManager tempSession = ScreenManager.Content.Load<SessionManager>(sessionPath);
                //tempSession.LoadContent();
            //}
            //catch (NotSupportedException) { 
            //    errorThrown = true; }
            //catch (ArgumentException) { 
            //    errorThrown = true; }
            //catch (FileNotFoundException) { 
            //    errorThrown = true; }

            string[] preloadImages = new string[] {
                ".//UI//Scroll Bar.png",
                ".//UI//Cursor.png",
                ".//UI//Description Box.png",
                ".//Miscellaneous//Inner Rim.png",
                ".//Miscellaneous//Outer Rim.png",
                ".//UI//Timeline Scratch.png",
                ".//UI//Timeline Key.png",
                ".//UI//Timeline Position Marker.png",
                ".//UI//Timeline.png",
                ".//UI//Scroll Timeline Back.png",
                ".//UI//Scroll Timeline Forth.png" };

            for (int i = 0; i < preloadImages.Length; i++)
            {
                try
                { 
                    ScreenManager.Content.Load<Texture2D>(preloadImages[i]);
                }
                catch (FileNotFoundException)
                { 
                    errorThrown = true;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            ScreenManager.Globals.Camera.HandleInput();
            ScreenManager.Globals.Camera.Update();
            //ScreenManager.Globals.Camera.WorldPosition = camera.WorldPosition;
            ScreenManager.Globals.DefaultEffect.View = Matrix.CreateLookAt(ScreenManager.Globals.Camera.CameraPosition, ScreenManager.Globals.Camera.LookAtPosition, Vector3.Up);

            spirograph.Update(gameTime);

            if (errorThrown && preloadContentThread.ThreadState == ThreadState.Stopped)
            {
                //screenManager.AddScreen(new ErrorScreen(ScreenManager.Content.ThrownErrors.ToArray(), screenManager));
                //ScreenManager.Content.ThrownErrors = new List<string>();
                RemoveSelf();
            }

            if (spirograph.DoneSlowDrawing)
            {
                if (!flashComplete)
                {
                    (spirograph.Modifiers[0] as RotateModifier3D).Reset(Quaternion.CreateFromYawPitchRoll(0, -0.06f, 0), false, -1);
                    (spirograph.Modifiers[1] as SpirographLerpModifier).Reset(1f, 10f / 7f, -1f, 1f, 1.2f, 20);
                    (spirograph.Modifiers[2] as ColorModifier3D).Reset(Color.White, false, 20);
                    flashComplete = true;
                }
                else if (!spirograph.Modifiers[1].Active && !shrinkComplete)
                {
                    (spirograph.Modifiers[0] as RotateModifier3D).Reset(Quaternion.CreateFromYawPitchRoll(0, 0.12f, 0), false, -1);
                    (spirograph.Modifiers[1] as SpirographLerpModifier).Reset(1f, 10f / 7f, -1f, 10f, 0.1f, 80);
                    (spirograph.Modifiers[2] as ColorModifier3D).Reset(Color.Lime, false, 80);
                    shrinkComplete = true;
                }
                else if (!spirograph.Modifiers[1].Active && !growComplete)
                {
                    (spirograph.Modifiers[0] as RotateModifier3D).Reset(Quaternion.CreateFromYawPitchRoll(0, -0.24f, 0), false, -1);
                    (spirograph.Modifiers[1] as SpirographLerpModifier).Reset(1f, 10f / 7f, -1f, 0.1f, 5f, 20);
                    (spirograph.Modifiers[2] as ColorModifier3D).Reset(Color.Black, false, 20);
                    growComplete = true;
                }
                else if (!spirograph.Modifiers[1].Active)
                    skip = true;
            }

            if (InputManager.IsKeyTriggered(Keys.Escape))
                skip = true;

            if(skip && preloadContentThread.ThreadState == ThreadState.Stopped)
            {
                preloadContentThread.Join();
                ScreenManager.AddScreen(new MapScreen(sessionPath));
                RemoveSelf();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (EffectPass pass in ScreenManager.Globals.DefaultEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                spirograph.Draw(ScreenManager.Globals.DefaultEffect, ScreenManager.Globals.Graphics);
            }
        }
    }
}
