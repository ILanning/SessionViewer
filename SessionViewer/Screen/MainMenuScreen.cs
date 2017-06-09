using CommonCode;
using CommonCode.Drawing;
using CommonCode.Modifiers;
using CommonCode.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SessionViewer
{
    class MainMenuScreen : Screen
    {
        List<Button> mainButtonList = new List<Button>();

        Button fileToStart;
        Button nextFilePage;
        Button prevFilePage;
        int fileMenuPageNumber = 0;
        List<Button> sessionButtonList = new List<Button>();
        Dictionary<Button, string> buttonsToPaths = new Dictionary<Button, string>();

        Button guideToStart;
        Button nextGuidePage;
        Button prevGuidePage;
        int guidePageNumber = 0;
        Page[] guideScreen;

        string errorMessage;
        int errorTimer = 0;
        MenuState currentState = MenuState.StartMenu;
        Sprite backgroundSession;
        Sprite titleText;
        DynamicContentManager Content;
        StringPositionColor copyrightText;

        public MainMenuScreen() : base() { }

        public override void LoadContent()
        {
            Content = new DynamicContentManager(ScreenManager.StaticGame, ScreenManager.StaticGame.Content, ScreenManager.StaticGame.Content.RootDirectory);
            //Content.LogFailures = true;

            copyrightText = new StringPositionColor("Homestuck © Andrew Hussie", new Vector2(0, 575), Color.White);
            ScreenManager.Globals.Fonts.AddFont("Consolas", ".//Fonts//Consolas.spritefont");
            ScreenManager.Globals.Fonts.AddFont("Carima", ".//Fonts//Carima.spritefont");
            Texture2D sessionTexture = Content.Load<Texture2D>(".//UI//Session.png");

            titleText = new Sprite(new Vector2(393, 111), Vector2.One, 0, Color.White, Content.Load<Texture2D>(".//UI//Title Text.png"));
            Button enterButton = new Button(null, Content.Load<Texture2D>(".//UI//Enter Button.png"), null, new Coordinate(526, 299));
            Button guideButton = new Button(null, Content.Load<Texture2D>(".//UI//Guide Button.png"), null, new Coordinate(526, 365));
            Button exitButton = new Button(null, Content.Load<Texture2D>(".//UI//Exit Button.png"), null, new Coordinate(526, 431));
            Content.Load<Texture2D>(".//UI//Blank Button.png");

            fileToStart = new Button(null, Content.Load<Texture2D>(".//UI//Up Button.png"), null, new Coordinate(650, 1160));
            prevFilePage = new Button(null, Content.Load<Texture2D>(".//UI//Left Button.png"), null, new Coordinate(580, 1160));
            nextFilePage = new Button(null, Content.Load<Texture2D>(".//UI//Right Button.png"), null, new Coordinate(720, 1160));

            guideToStart = new Button(null, Content.Load<Texture2D>(".//UI//Down Button.png"), null, new Coordinate(650, -40));
            prevGuidePage = new Button(null, Content.Load<Texture2D>(".//UI//Left Button.png"), null, new Coordinate(580, -40));
            nextGuidePage = new Button(null, Content.Load<Texture2D>(".//UI//Right Button.png"), null, new Coordinate(720, -40));

            enterButton.Clicked += OnEnter;
            mainButtonList.Add(enterButton);
            guideButton.Clicked += OnGuide;
            mainButtonList.Add(guideButton);
            exitButton.Clicked += OnExit;
            mainButtonList.Add(exitButton);

            backgroundSession = new Sprite(new Vector2(227 - sessionTexture.Width / 2, 300 - sessionTexture.Height / 2),
                                    Vector2.One, 0, Color.White, sessionTexture);

            backgroundSession.AddModifier(new RotateModifier2D(0.006f, false, -1));

            guideScreen = new Page[1];
            guideScreen[0] = new Page(new Vector2(0, -600),
                new StringPositionColor[]{
                    new StringPositionColor("LMB", new Vector2(10,0), Color.White), new StringPositionColor("-Select World", new Vector2(70,0), Color.White),
                    new StringPositionColor("RMB", new Vector2(10,20), Color.White), new StringPositionColor("-Deselect World", new Vector2(70,20), Color.White),
                    new StringPositionColor("MMB", new Vector2(10,40), Color.White), new StringPositionColor("-Rotate Camera", new Vector2(70,40), Color.White),
                    new StringPositionColor("F", new Vector2(10,60), Color.White), new StringPositionColor("-Skip forward on timeline", new Vector2(70,60), Color.White),
                    new StringPositionColor("B", new Vector2(10,80), Color.White), new StringPositionColor("-Skip backward on timeline", new Vector2(70,80), Color.White),
                    new StringPositionColor("Esc", new Vector2(10,100), Color.White), new StringPositionColor("-Quit to main menu", new Vector2(70,100), Color.White),
                    new StringPositionColor("Up", new Vector2(10,120), Color.White), new StringPositionColor("-Move forward", new Vector2(70,120), Color.White),
                    new StringPositionColor("Down", new Vector2(10,140), Color.White), new StringPositionColor("-Move backward", new Vector2(70,140), Color.White),
                    new StringPositionColor("Left", new Vector2(10,160), Color.White), new StringPositionColor("-Move left", new Vector2(70,160), Color.White),
                    new StringPositionColor("Right", new Vector2(10,180), Color.White), new StringPositionColor("-Move right", new Vector2(70,180), Color.White)
                },
                new Sprite[] { new Sprite(Vector2.Zero, Vector2.One, 0, Color.White, Content.Load<Texture2D>(".//UI//Guide Screen.png")) }
            );

            ScreenManager.StaticGame.Window.Title = "Session Viewer";

            fileToStart.Clicked += OnLeaveEnter;
            prevFilePage.Clicked += OnPrevFile;
            nextFilePage.Clicked += OnNextFile;

            guideToStart.Clicked += OnLeaveGuide;
            prevGuidePage.Clicked += OnPrevGuide;
            nextGuidePage.Clicked += OnNextGuide;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            ScreenManager.IsMouseVisible = true;
            ScreenManager.StaticGame.Window.Title = "Session Viewer";

            backgroundSession.Update();
            titleText.Update();
            foreach (Page page in guideScreen)
                page.Update();

            foreach (Button button in mainButtonList)
                button.Update(gameTime);
            foreach (Button button in sessionButtonList)
                button.Update(gameTime);

            fileToStart.Update(gameTime);
            nextFilePage.Update(gameTime);
            prevFilePage.Update(gameTime);

            guideToStart.Update(gameTime);
            nextGuidePage.Update(gameTime);
            prevGuidePage.Update(gameTime);
        }

        public override void HandleInput(GameTime gameTime)
        {
            foreach (Button button in mainButtonList)
                button.HandleInput();
            foreach (Button button in sessionButtonList)
                button.HandleInput();

            fileToStart.HandleInput();
            nextFilePage.HandleInput();
            prevFilePage.HandleInput();

            guideToStart.HandleInput();
            nextGuidePage.HandleInput();
            prevGuidePage.HandleInput();

            //if (currentState == MenuState.FileMenu)
            //{
            //    for (int i = 0; i < sessionButtonList.Count; i++)
            //    {
            //        if (sessionButtonList[i].IsHeld)
            //        {
            //            screenManager.AddScreen(new LoadScreen(sessionPathList[i], screenManager));
            //            currentState = MenuState.StartMenu;
            //            sessionPathList = new List<string>();
            //            sessionButtonList = new List<Button>();
            //            for (int h = 0; h < mainButtonList.Count; h++)
            //                mainButtonList[h].WorldPosition = new Vector2(mainButtonList[h].WorldPosition.X, 299 + h * 66);
            //            fileToStart.WorldPosition = new Vector2(fileToStart.WorldPosition.X, 1160);
            //            prevFilePage.WorldPosition = new Vector2(prevFilePage.WorldPosition.X, 1160);
            //            nextFilePage.WorldPosition = new Vector2(nextFilePage.WorldPosition.X, 1160);
            //        }
            //        else if (sessionButtonList[i].IsSelected)
            //        {
            //            errorMessage = sessionPathList[i].Replace(Content.BaseDirectory, "");
            //            errorTimer = 30;
            //        }
            //    }
            //}
            if (currentState == MenuState.MovingToFile)
            {
                if (!sessionButtonList[0].Modifiers[0].Active)
                {
                    currentState = MenuState.FileMenu;
                    foreach (Button button in sessionButtonList)
                        button.ClearModifiers();
                    foreach (Button button in mainButtonList)
                        button.ClearModifiers();
                    nextFilePage.ClearModifiers();
                    prevFilePage.ClearModifiers();
                    fileToStart.ClearModifiers();
                }
            }
            else if (currentState == MenuState.MovingFromFile)
            {
                if (!sessionButtonList[0].Modifiers[0].Active)
                {
                    fileMenuPageNumber = 0;
                    currentState = MenuState.StartMenu;
                    foreach (Button button in sessionButtonList)
                        button.ClearModifiers();
                    foreach (Button button in mainButtonList)
                        button.ClearModifiers();
                    nextFilePage.ClearModifiers();
                    prevFilePage.ClearModifiers();
                    fileToStart.ClearModifiers();
                }
            }
            else if (currentState == MenuState.FileMovingBack || currentState == MenuState.FileMovingForth)
            {
                if (!sessionButtonList[0].Modifiers[0].Active)
                {
                    currentState = MenuState.FileMenu;
                    foreach (Button button in sessionButtonList)
                        button.ClearModifiers();
                }
            }
            else if (currentState == MenuState.MovingToGuide)
            {
                if (!guideScreen[0].Modifiers[0].Active)
                {
                    currentState = MenuState.GuideMenu;
                    foreach (Button button in mainButtonList)
                        button.ClearModifiers();
                    foreach (Page page in guideScreen)
                        page.ClearModifiers();
                    backgroundSession.ClearModifiers();
                    backgroundSession.AddModifier(new RotateModifier2D(0.006f, false, -1));
                    titleText.ClearModifiers();
                    nextGuidePage.ClearModifiers();
                    prevGuidePage.ClearModifiers();
                    guideToStart.ClearModifiers();
                }
            }
            else if (currentState == MenuState.MovingFromGuide)
            {
                if (!guideScreen[0].Modifiers[0].Active)
                {
                    currentState = MenuState.StartMenu;
                    foreach (Button button in mainButtonList)
                        button.ClearModifiers();
                    foreach (Page page in guideScreen)
                        page.ClearModifiers();
                    backgroundSession.ClearModifiers();
                    backgroundSession.AddModifier(new RotateModifier2D(0.006f, false, -1));
                    titleText.ClearModifiers();
                    nextGuidePage.ClearModifiers();
                    prevGuidePage.ClearModifiers();
                    guideToStart.ClearModifiers();
                }
            }
            else if (currentState == MenuState.GuideMovingBack || currentState == MenuState.GuideMovingForth)
            {
                if (!guideToStart.Modifiers[0].Active)
                {
                    currentState = MenuState.GuideMenu;
                }
            }
        }

        #region Button Events

        public void OnEnter(object sender, EventArgs e)
        {
            if (currentState == MenuState.StartMenu)
            {
                //Opens a selection menu with all of the available sessions in the folder, moves three main buttons up.
                currentState = MenuState.MovingToFile;
                List<string> sessionFiles = new List<string>(Directory.GetFiles(Content.BaseDirectory + "\\XML Files\\Sessions", "*.xml"));
                sessionButtonList = new List<Button>();
                for (int i = 0; i < sessionFiles.Count; i++)
                {
                    Button loadSession = new Button(null, Content.Load<Texture2D>(".\\UI\\Blank Button.png"), null, new Coordinate(526 + (i / 4) * 800, 899 + (i % 4) * 66));
                    sessionButtonList.Add(loadSession);
                    sessionButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(-1, 299 + (i % 4) * 66),
                                                                                    loadSession, false, 60));
                    string buttonText = Path.GetFileNameWithoutExtension(sessionFiles[i]);
                    if (ScreenManager.Globals.Fonts["Default"].MeasureString(buttonText).X > 178)
                    {
                        int ellipsisLength = (int)ScreenManager.Globals.Fonts["Default"].MeasureString("...").X;
                        while (ScreenManager.Globals.Fonts["Default"].MeasureString(buttonText).X + ellipsisLength > 178)
                            buttonText = buttonText.Remove(buttonText.Length - 1);
                        buttonText = buttonText.Insert(buttonText.Length, "...");
                    }
                    //TODO: Repair this
                    loadSession.Text = new StringFontPositionColor(buttonText, "Consolas", new Vector2(73, 21), Color.White);
                    loadSession.Clicked += OnLoadClick;
                    buttonsToPaths[loadSession] = sessionFiles[i];
                }
                if (sessionButtonList.Count != 0)
                {
                    for (int i = 0; i < mainButtonList.Count; i++)
                        mainButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(-1, -301 + i * 66), mainButtonList[i], false, 60));
                    fileToStart.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 560), fileToStart, false, 60));
                    prevFilePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 560),  prevFilePage, false, 60));
                    nextFilePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 560), nextFilePage, false, 60));
                }
                else
                {
                    currentState = MenuState.StartMenu;
                    errorMessage = "No .xml files found in " + Content.BaseDirectory + "\\XML Files\\Sessions.";
                    errorTimer = 120;
                }
            }
        }

        void OnLoadClick(object sender, EventArgs e)
        {
            if (currentState == MenuState.FileMenu)
            {
                ScreenManager.AddScreen(new LoadScreen(buttonsToPaths[(Button)sender]));
                currentState = MenuState.StartMenu;
                sessionButtonList = new List<Button>();
                for (int h = 0; h < mainButtonList.Count; h++)
                    mainButtonList[h].WorldPosition = new Vector2(mainButtonList[h].WorldPosition.X, 299 + h * 66);
                fileToStart.WorldPosition = new Vector2(fileToStart.WorldPosition.X, 1160);
                prevFilePage.WorldPosition = new Vector2(prevFilePage.WorldPosition.X, 1160);
                nextFilePage.WorldPosition = new Vector2(nextFilePage.WorldPosition.X, 1160);
            }
        }

        public void OnNextFile(object sender, EventArgs e)
        {
            if (currentState == MenuState.FileMenu && fileMenuPageNumber < sessionButtonList.Count / 4)
            {
                currentState = MenuState.FileMovingForth;
                fileMenuPageNumber++;
                for (int i = 0; i < sessionButtonList.Count; i++)
                    sessionButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(526 + ((i / 4) - fileMenuPageNumber) * 800, -1),
                                                            sessionButtonList[i], false, 60));
            }
        }

        public void OnPrevFile(object sender, EventArgs e)
        {
            if (currentState == MenuState.FileMenu && fileMenuPageNumber > 0)
            {
                currentState = MenuState.FileMovingBack;
                fileMenuPageNumber--;
                for (int i = 0; i < sessionButtonList.Count; i++)
                    sessionButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(526 + ((i / 4) - fileMenuPageNumber) * 800, -1),
                                                            sessionButtonList[i], false, 60));
            }
        }

        public void OnLeaveEnter(object sender, EventArgs e)
        {
            if (currentState == MenuState.FileMenu)
            {
                currentState = MenuState.MovingFromFile;
                for (int i = 0; i < mainButtonList.Count; i++)
                    mainButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(-1, 299 + i * 66),
                                                                mainButtonList[i], false, 60));
                if (sessionButtonList.Count != 0)
                {
                    for (int i = 0; i < sessionButtonList.Count; i++)
                        sessionButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(-1, 899 + i * 66),
                                                                sessionButtonList[i], false, 60));
                }
                fileToStart.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 1160), fileToStart, false, 60));
                prevFilePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 1160), prevFilePage, false, 60));
                nextFilePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 1160), nextFilePage, false, 60));
            }
        }

        public void OnGuide(object sender, EventArgs e)
        {
            //Open a menu with a list of commands, moving everything else down.
            //Page 1: Keys
            //Page 2: Timeline Control
            if (currentState == MenuState.StartMenu)
            {
                currentState = MenuState.MovingToGuide;
                for (int i = 0; i < mainButtonList.Count; i++)
                    mainButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(-1, 901 + i * 66),
                                                            mainButtonList[i], false, 60));
                foreach (Page page in guideScreen)
                    page.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 0), page, false, 60));
                guideToStart.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 560), guideToStart, false, 60));
                prevGuidePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 560), prevGuidePage, false, 60));
                nextGuidePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 560), nextGuidePage, false, 60));

                backgroundSession.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 900 - backgroundSession.textureSource.Height / 2),
                                                                        backgroundSession, false, 60));
                titleText.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 711), titleText, false, 60));
            }
        }

        public void OnNextGuide(object sender, EventArgs e)
        {
            if (currentState == MenuState.GuideMenu)
            {
                currentState = MenuState.GuideMovingForth;
            }
        }

        public void OnPrevGuide(object sender, EventArgs e)
        {
            if (currentState == MenuState.GuideMenu)
            {
                currentState = MenuState.GuideMovingBack;
            }
        }

        public void OnLeaveGuide(object sender, EventArgs e)
        {
            if (currentState == MenuState.GuideMenu)
            {
                currentState = MenuState.MovingFromGuide;
                for (int i = 0; i < mainButtonList.Count; i++)
                    mainButtonList[i].AddModifier(new SmoothStepModifier2D(new Vector2(-1, 301 + i * 66),
                                                            mainButtonList[i], false, 60));
                foreach (Page page in guideScreen)
                    page.AddModifier(new SmoothStepModifier2D(new Vector2(-1, -600), page, false, 60));
                guideToStart.AddModifier(new SmoothStepModifier2D(new Vector2(-1, -40), guideToStart, false, 60));
                prevGuidePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, -40), prevGuidePage, false, 60));
                nextGuidePage.AddModifier(new SmoothStepModifier2D(new Vector2(-1, -40), nextGuidePage, false, 60));
                backgroundSession.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 300 - backgroundSession.textureSource.Height / 2), 
                                                                        backgroundSession, false, 60));
                titleText.AddModifier(new SmoothStepModifier2D(new Vector2(-1, 111), titleText, false, 60));
            }
        }

        public void OnExit(object sender, EventArgs e)
        {
            if (currentState == MenuState.StartMenu)
                ScreenManager.StaticGame.Exit();
        }

        #endregion

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sb = ScreenManager.Globals.sb;
            sb.Begin(blendState: BlendState.NonPremultiplied);

            sb.DrawString(ScreenManager.Globals.Fonts["Default"], copyrightText.Text, new Vector2(copyrightText.Position.X, copyrightText.Position.Y), copyrightText.Color);
            foreach (Button button in mainButtonList)
                button.Draw(ScreenManager.Globals.sb);

            if (sessionButtonList.Count > 0)
            {
                foreach (Button button in sessionButtonList)
                    button.Draw(sb);
                if (fileMenuPageNumber < (sessionButtonList.Count - 1) / 4)
                    nextFilePage.Draw(sb);
                if (fileMenuPageNumber > 0)
                    prevFilePage.Draw(sb);
            }
            if (currentState == MenuState.GuideMenu || currentState == MenuState.MovingFromGuide ||
                currentState == MenuState.MovingToGuide || currentState == MenuState.GuideMovingBack || currentState == MenuState.GuideMovingForth)
            {
                if (guidePageNumber > 0)
                    prevGuidePage.Draw(sb);
                if (guidePageNumber < guideScreen.Length - 1)
                    nextGuidePage.Draw(sb);
                guideScreen[guidePageNumber].Draw2D();
            }

            backgroundSession.Draw(sb);
            titleText.Draw(sb);
            fileToStart.Draw(sb);
            guideToStart.Draw(sb);

            if (errorMessage != null)
            {
               sb.DrawString(ScreenManager.Globals.Fonts["Default"], errorMessage, Vector2.Zero, Color.White);
                errorTimer--;
                if (errorTimer < 1)
                {
                    errorTimer = 0;
                    errorMessage = null;
                }
            }

            sb.End();
        }
    }

    enum MenuState
    {
        StartMenu,
        MovingToFile,
        MovingToGuide,

        FileMenu,
        FileMovingForth,
        FileMovingBack,
        MovingFromFile,

        GuideMenu,
        GuideMovingForth,
        GuideMovingBack,
        MovingFromGuide
    }
}
