using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SessionViewer;
using CommonCode.Modifiers;
using Microsoft.Xna.Framework.Input;
using SessionViewer.Content;
using CommonCode;
using CommonCode.UI;
using CommonCode.Drawing;

/*namespace SessionViewer
{
    /// <summary>
    /// Should be a representation of what is happening in Timeline/SessionManager
    /// </summary>
    class TimelineControl : IModifiable2D, IClickable
    {
        IModifier2D[] modifiers = new IModifier2D[4];

        SessionManager session;
        MapScreen mapScreen;
        Button background;
        Button[] eventButtons;
        Button expandButton;
        Button stopButton;
        Button pauseButton;
        Button playButton;
        Button scrollBackButton;
        Button scrollForthButton;
        Animation scratch;
        Sprite timelinePositionMarker;
        float distBetweenKeys;
        bool timelineOpen;
        int barScrollDist = 0;

        public TimelineControl(SessionManager session, MapScreen screen)
        {
            this.session = session;
            mapScreen = screen;
            Texture2D backTexture = ScreenManager.Content.Load<Texture2D>(".//UI//Scroll Timeline Back.png");
            Texture2D forthTexture = ScreenManager.Content.Load<Texture2D>(".//UI//Scroll Timeline Forth.png");
            eventButtons = new Button[session.VisibleEvents.Length];
            background = new Button(new Vector2(0, -47), Vector2.One, 0, Color.White,
                        ScreenManager.Content.Load<Texture2D>(".//UI//Timeline.png"));
            expandButton = new Button(new Rectangle(677, 0, 123, 28));
            expandButton.Clicked += OnToggleTimeline;
            stopButton = new Button(new Rectangle(0, 0, 24, 24));
            stopButton.Clicked += session.OnStop;
            pauseButton = new Button(new Rectangle(26, 0, 24, 24));
            pauseButton.Clicked += session.OnPause;
            playButton = new Button(new Rectangle(52, 0, 24, 24));
            playButton.Clicked += session.OnPlay;
            timelinePositionMarker = new Sprite(new Vector2(-6, -35), new Vector2(1), 0, Color.White, ScreenManager.Content.Load<Texture2D>(".//UI//Timeline Position Marker.png"));
            
            Texture2D timelineButtonTexture = ScreenManager.Content.Load<Texture2D>(".//UI//Timeline Key.png");
            if (session.VisibleEvents.Length > 10)
            {
                scrollBackButton = new Button(new Vector2(0, -47), Vector2.One, 0, Color.White, backTexture);
                scrollForthButton = new Button(new Vector2(0, -47), Vector2.One, 0, Color.White, forthTexture);
                scrollForthButton.Position = new Coordinate(800 - forthTexture.Width, -47);
                timelinePositionMarker.WorldPosition += new Vector2(backTexture.Width, 0);
            }
            for (int i = 0; i < session.VisibleEvents.Length; i++)
            {
                Color color = Color.White;
                foreach (var keyValue in session.Worlds)
                    if (session.Timeline[session.VisibleEvents[i]].Target == keyValue.Value.Name)
                        color = keyValue.Value.PrimaryColor;

                Vector2 buttonPos = Vector2.Zero;
                if (session.VisibleEvents.Length > 10)
                {
                    distBetweenKeys = 85 + backTexture.Width;
                    buttonPos = new Vector2(i * distBetweenKeys, -47);
                }
                else if (session.VisibleEvents.Length > 1)
                {
                    distBetweenKeys = (800 - timelineButtonTexture.Width / 2) / (session.VisibleEvents.Length - 1);
                    buttonPos = new Vector2((i * distBetweenKeys), -47);
                }
                else
                    buttonPos = new Vector2(0, -47);

                eventButtons[i] = new Button(buttonPos, Vector2.One, 0, color, new Rectangle(0, 0, timelineButtonTexture.Width, timelineButtonTexture.Height),
                                            timelineButtonTexture);
            }
            if (session.Scratched)
            {
                Vector2 scratchPos = new Vector2(eventButtons[eventButtons.Length - 1].Position.X + timelineButtonTexture.Width - 54, -47);
                scratch = new Animation(true, scratchPos, new Point(54, 46),
                    ScreenManager.Content.Load<Texture2D>(".//UI//Timeline Scratch.png"), new Frame[] { 
                        new Frame { Length = 1, Location = Point.Zero, Source = Point.Zero },
                        new Frame { Length = 1, Location = Point.Zero, Source = new Point(0, 46) },
                        new Frame { Length = 1, Location = Point.Zero, Source = new Point(0, 92) },
                        new Frame { Length = 1, Location = Point.Zero, Source = new Point(0, 138) },
                        new Frame { Length = 1, Location = Point.Zero, Source = new Point(0, 184) },
                        new Frame { Length = 1, Location = Point.Zero, Source = new Point(0, 230) }});
            }
            else
                scratch = new Animation(false, Vector2.Zero, Point.Zero, null, new Frame[] { new Frame { Delay = 0, Location = Point.Zero, Source = Point.Zero } });
            //session.MoveToTimelineEvent(currentTimelineEvent);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i] != null)
                {
                    modifiers[i].Update();
                    if (modifiers[i].RemoveIfComplete && !modifiers[i].Active)
                    {
                        modifiers[i].Remove();
                        i--;
                    }
                }
            }
            background.Update(gameTime);
            expandButton.Update(gameTime);
            scratch.Update(gameTime);

            if (session.VisibleEvents.Length > 10)
            {
                scrollBackButton.Update(gameTime);
                scrollForthButton.Update(gameTime);
            }            

            for (int i = 0; i < eventButtons.Length; i++)
                eventButtons[i].Update(gameTime);

            timelinePositionMarker.Update();
            float markerXPos = distBetweenKeys * (session.CurrentTimelineTime / 180f) - 6;
            timelinePositionMarker.WorldPosition = new Vector2(markerXPos, background.Position.Y + 12);

            stopButton.Update(gameTime);
            pauseButton.Update(gameTime);
            playButton.Update(gameTime);
        }

        public void HandleInput()
        {
            if (InputManager.IsKeyTriggered(Keys.F) && !InputManager.IsKeyTriggered(Keys.B) && session.CurrentTimelineKey < session.VisibleEvents[session.VisibleEvents.Length - 1] - 1)
                session.MoveToTimelineEvent(session.CurrentTimelineKey + 1);
            if (InputManager.IsKeyTriggered(Keys.B) && !InputManager.IsKeyTriggered(Keys.F) && session.CurrentTimelineKey > 0)
                session.MoveToTimelineEvent(session.CurrentTimelineKey - 1);

            background.HandleInput();
            expandButton.HandleInput();

            bool timelineScrolled = false;
            if (session.VisibleEvents.Length > 10)
            {
                scrollBackButton.HandleInput();
                scrollForthButton.HandleInput();
                if (scrollBackButton.IsHeld)
                {
                    if (eventButtons[0].Offset.X < 0)
                    {
                        for (int i = 0; i < eventButtons.Length; i++)
                            eventButtons[i].Offset += new Vector2(3, 0);
                        timelinePositionMarker.Offset += new Vector2(3, 0);
                        scratch.Offset += new Vector2(3, 0);
                    }
                    else
                    {
                        for (int i = 0; i < eventButtons.Length; i++)
                            eventButtons[i].Offset = Vector2.Zero;
                        timelinePositionMarker.Offset = Vector2.Zero;
                        scratch.Offset = Vector2.Zero;
                    }
                    timelineScrolled = true;
                }
                if (scrollForthButton.IsHeld)
                {
                    //If final button is not on screen
                    if (eventButtons[eventButtons.Length - 1].WorldPosition.X + eventButtons[eventButtons.Length - 1].Offset.X > 777)
                    {
                        //Allow the bar to scroll the final button towards the screen
                        for (int i = 0; i < eventButtons.Length; i++)
                            eventButtons[i].Offset += new Vector2(-3, 0);
                        timelinePositionMarker.Offset += new Vector2(-3, 0);
                        scratch.Position += new Vector2(-3, 0);
                    }
                    else
                    {

                        Vector2 newOffset = new Vector2(777 - eventButtons[eventButtons.Length - 1].WorldPosition.X, 0);
                        for (int i = 0; i < eventButtons.Length; i++)
                            eventButtons[i].Offset = newOffset;
                        timelinePositionMarker.Offset = newOffset;
                        scratch.Offset = newOffset;
                    }
                    timelineScrolled = true;
                }
            }

            for (int i = 0; i < eventButtons.Length; i++)
            {
                eventButtons[i].HandleInput();
                if (!timelineScrolled && eventButtons[i].IsHeld)
                {
                    session.MoveToTimelineEvent(i);
                    session.OnPause(this, null);
                    //currentTimelineTime = i * 180;
                    //currentTimelineKey = i;
                    //currentTimelineEvent = session.VisibleEvents[i];
                    //session.MoveToTimelineEvent(currentTimelineEvent);
                    //playing = false;
                    //mapScreen.UpdateTimeline();
                }
            }
            stopButton.HandleInput();
            pauseButton.HandleInput();
            playButton.HandleInput();
        }

        /*public void UpdateTimeline()
        {
            timelinePositionMarker.WorldPosition = eventButtons[currentTimelineKey].WorldPosition + new Vector2(-6, 12);
            currentTimelineTime = currentTimelineKey * 180;
        }*

        public void OnToggleTimeline(object sender, EventArgs e)
        {
            if (Modifiers.Length != 0 && (Math.Round(WorldPosition.Y) == -47 || Math.Round(WorldPosition.Y) == 0))
            {
                this.ClearModifiers();
                if (timelineOpen)
                {
                    this.AddModifier(new MoveToModifier2D(new Vector2(-1, -47), this, true, 20));
                    timelineOpen = false;
                }
                else
                {
                    this.AddModifier(new MoveToModifier2D(new Vector2(-1, 0), this, true, 20));
                    timelineOpen = true;
                }
            }
        }

        /*public void OnPressStop(object sender, EventArgs e)
        {
            timelinePositionMarker.ClearModifiers();
            session.OnStop(sender, e);
        }*

        public void Draw(SpriteBatch sb)
        {
            background.Draw(sb);
            expandButton.Draw(sb);
            foreach (Button button in eventButtons)
                button.Draw(sb);
            scratch.Draw(sb);
            timelinePositionMarker.Draw(sb);
            if (session.VisibleEvents.Length > 10)
            {
                scrollBackButton.Draw(sb);
                scrollForthButton.Draw(sb);
            }
        }

        #region IModifiable2D Members

        public void AddModifier(IModifier2D modifier)
        {
            modifier.owner = this;
            for (int i = 0; i <= modifiers.Length; i++)
            {
                if (i == modifiers.Length)
                {
                    IModifier2D[] newModifiersArray = new IModifier2D[modifiers.Length + 4];
                    for (int h = 0; h < modifiers.Length; h++)
                    {
                        newModifiersArray[h] = modifiers[h];
                    }
                    newModifiersArray[modifiers.Length] = modifier;
                    modifiers = newModifiersArray;
                }
                if (modifiers[i] == null)
                {
                    modifiers[i] = modifier;
                    break;
                }
            }
        }

        public void ClearModifiers()
        {
            modifiers = new IModifier2D[4];
        }

        #endregion

        #region Properties

        public Vector2 WorldPosition 
        { 
            get { return background.WorldPosition; } 
            set 
            {
                Vector2 difference = value - background.WorldPosition;
                for (int i = 0; i < eventButtons.Length; i++)
                    eventButtons[i].WorldPosition += difference;
                expandButton.WorldPosition += difference;
                playButton.WorldPosition += difference;
                pauseButton.WorldPosition += difference;
                stopButton.WorldPosition += difference;
                if (session.VisibleEvents.Length > 10)
                {
                    scrollBackButton.WorldPosition += difference;
                    scrollForthButton.WorldPosition += difference;
                }
                timelinePositionMarker.WorldPosition += difference;
                scratch.Position += difference;
                background.WorldPosition = value;
            } 
        }

        //public Vector2 Offset { get { return background.Offset; } set { background.Offset = value; } }

        public float Rotation { get { return background.Rotation; } set { background.Rotation = value; } }

        public Vector2 Scale { get { return background.Scale; } set { background.Scale = value; } }

        public Color Color { get { return background.Color; } set { background.Color = value; } }

        public IModifier2D[] Modifiers { get { return modifiers; } }

        #endregion

        #region IClickable Members

        public event EventHandler Selected;

        public event EventHandler Clicked;

        public event EventHandler Collided;

        public bool IsSelected { get { return background.IsHovered; } }

        //public bool OnCollision()
        //{
        //    if (background.OnCollision())
        //    {
        //        if (Collided != null)
        //            Collided(this, EventArgs.Empty);
        //        if (Selected != null)
        //            Selected(this, EventArgs.Empty);
        //        return true;
        //    }
        //    return false;
        //}

        public bool OnClicked()
        {
            //TODO: Potentially faulty
            if (background.IsHeld)
            {
                if (Clicked != null)
                    Clicked(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void OnSelectItem()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}*/
