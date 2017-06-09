using SessionViewer.Content;
using System.IO;
using System.Collections.Generic;
using System;
using CommonCode;

namespace SessionViewer
{
    struct LocationMessage
    {
        public EventTypes EventType;
        public string Message;
    }


    class SessionManager : ICopyable<SessionManager>
    {
        public GenericMeteor[] GenericMeteors;
        public Dictionary<string, Location> Worlds;
        public int PlayerWorldCount;
        /// <summary>
        /// The last message to appear on the screen.
        /// </summary>
        public string LatestMessage = "";
        ///// <summary>
        ///// The first event that will draw a key on the timeline.  this event and all previous are automatically generated.
        ///// </summary>
        public int FirstKeyedEvent;
        /// <summary>
        /// Array representing indices of keyed events in Timeline.
        /// </summary>
        public int[] VisibleEvents;
        public TimelineEvent[] Timeline;
        public string Description;
        /// <summary>
        /// Set to true if the players scratch their session.
        /// </summary>
        public bool Scratched;
        /// <summary>
        /// Set to true if no pre-entry prototypings occur.
        /// </summary>
        public bool VoidSession;
        /// <summary>
        /// Set to true if the session fails to result in a universe.
        /// </summary>
        public bool NullSession;
        public bool ReckoningActive;

        public int currentTimelineKey;
        private int currentTimelineTime = 0;
        private bool playing = false;

        public static SessionManager BuildSession(string path, DynamicContentManager content)
        {
            return new SessionManager(path, content);
        }

        /*  Original thoughts on timeline structure:
         Timeline:
         * Loads in new files for each planet at a certain time.
         * Triggers for point on timeline:
         * * Planet changes texture/description
         * * Player enters session
         * One point on the timeline for each entry/Skaia evolution
         * If scratched, timeline will not let you move beyond a certain point
         */

        public SessionManager() { }

        public SessionManager(string filePath, DynamicContentManager Content)
        {
            SessionBuilder builder = SessionBuilder.BuilderRead(filePath);
            if (builder.OtherLocations == null)
                builder.OtherLocations = new string[0];
            Worlds = new Dictionary<string, Location>(3 + builder.Lands.Length + builder.OtherLocations.Length);
            GenericMeteors = new GenericMeteor[builder.GenericMeteors.Length];
            Description = builder.Description;
            PlayerWorldCount = builder.Lands.Length;

            if (builder.Scratched)
                Timeline = new TimelineEvent[builder.Timeline.Length + PlayerWorldCount + builder.OtherLocations.Length + 4];
            else
                Timeline = new TimelineEvent[builder.Timeline.Length + PlayerWorldCount + builder.OtherLocations.Length + 3];

            Skaia tempSkaia = Content.Load<Skaia>(builder.Skaia);
            Worlds.Add("Skaia", tempSkaia);
            SessionLocation tempProspit = Content.Load<SessionLocation>(builder.Prospit);
            Worlds.Add("Prospit", tempProspit);
            SessionLocation tempDerse = Content.Load<SessionLocation>(builder.Derse);
            Worlds.Add("Derse", tempDerse);
            PlayerWorld[] tempLands = new PlayerWorld[PlayerWorldCount];
            for (int i = 0; i < builder.Lands.Length; i++)
            {
                tempLands[i] = Content.Load<PlayerWorld>(builder.Lands[i]);
                Timeline[i + 2] = new TimelineEvent(tempLands[i].Name, EventTypes.Reload, "None", builder.Lands[i]);
                Worlds.Add(tempLands[i].Name, tempLands[i]);
            }
            SessionLocation[] tempMeteors = new SessionLocation[builder.OtherLocations.Length];
            for (int i = 0; i < tempMeteors.Length; i++)
            {
                tempMeteors[i] = Content.Load<SessionLocation>(builder.OtherLocations[i]);
                Timeline[i + 2 + tempLands.Length] = new TimelineEvent(tempMeteors[i].Name, EventTypes.Reload, "None", builder.OtherLocations[i]);
                Worlds.Add(tempMeteors[i].Name, tempMeteors[i]);
            }
            for (int i = 0; i < GenericMeteors.Length; i++)
                GenericMeteors[i] = new GenericMeteor(Path.Combine(Content.BaseDirectory, builder.GenericMeteors[i]), Content);

            Timeline[0] = new TimelineEvent("Prospit", EventTypes.Reload, "None", builder.Prospit);
            Timeline[1] = new TimelineEvent("Derse", EventTypes.Reload, "None", builder.Derse);

            Timeline[tempLands.Length + tempMeteors.Length + 2] = new TimelineEvent("Skaia", EventTypes.Reload, true, "None", builder.Skaia);
            if (builder.Scratched)
            {
                for (int i = tempLands.Length + tempMeteors.Length + 3; i < Timeline.Length - 1; i++)
                    Timeline[i] = builder.Timeline[i - (tempLands.Length + tempMeteors.Length + 3)];
                Timeline[Timeline.Length - 1] = new TimelineEvent("Session", EventTypes.ScratchComplete, true, "", "");
            }
            else
            {
                for (int i = tempLands.Length + tempMeteors.Length + 3; i < Timeline.Length; i++)
                    Timeline[i] = builder.Timeline[i - (tempLands.Length + tempMeteors.Length + 3)];
            }

            List<int> visibleEvents = new List<int>();
            for (int i = 0; i < Timeline.Length; i++)
                if (Timeline[i].IsVisible)
                    visibleEvents.Add(i);
            VisibleEvents = visibleEvents.ToArray();
            Scratched = builder.Scratched;
            VoidSession = builder.VoidSession;
            NullSession = builder.NullSession;
            ReckoningActive = false;
        }

        public void ConstructTimeline()
        {

        }

        public void LoadContent()
        {
            for (int i = 0; i < Timeline.Length - 1; i++)
            {
                if (!string.IsNullOrWhiteSpace(Timeline[i].Arguments))
                {
                    //if()
                    ScreenManager.Content.Load<Location>(Timeline[i].Arguments);
                }
            }
        }

        public void UpdateTimeline()
        {
            if (playing)
            {
                currentTimelineTime++;
                if (currentTimelineTime % 180 == 0)
                {
                    MoveToTimelineEvent(currentTimelineTime / 180);
                    if (currentTimelineKey == VisibleEvents.Length - 1)
                        playing = false;
                }
            }
        }

        /// <summary>
        /// Sends out messages that update the various session objects to represent the state of the session at <eventKey>.
        /// </summary>
        /// <param name="eventKey">The event in VisibleEvents to move to.</param>
        public void MoveToTimelineEvent(int eventKey)
        {
            if (eventKey == currentTimelineKey)
                return;

            var worldMessageQueues = new Dictionary<string, List<LocationMessage>>(Worlds.Count);
            foreach (var keyValue in Worlds)
                worldMessageQueues.Add(keyValue.Key, new List<LocationMessage>());
            //If it's moving to a future event, only send the intervening changes.  Otherwise, rebuild everything's state up to the passed in key.
            int MessageReadStart = 0;
            if (eventKey > currentTimelineKey)
                MessageReadStart = VisibleEvents[currentTimelineKey];
            for (int i = MessageReadStart + 1; i <= VisibleEvents[eventKey]; i++)
            {
                if (Timeline[i].Target == "Session")
                { }
                else if (Timeline[i].EventType == EventTypes.PlayerEntry)
                {
                    var skaiaMessage = new LocationMessage { EventType = Timeline[i].EventType, Message = Timeline[i].Arguments };
                    var planetMessage = new LocationMessage { EventType = Timeline[i].EventType, Message = "true" };
                    worldMessageQueues["Skaia"].Add(skaiaMessage);
                    worldMessageQueues[Timeline[i].Target].Add(planetMessage);
                }
                else
                {
                    var newMessage = new LocationMessage { EventType = Timeline[i].EventType, Message = Timeline[i].Arguments };
                    worldMessageQueues[Timeline[i].Target].Add(newMessage);
                }
            }

            if (Timeline[VisibleEvents[eventKey]].DisplayedText != null)
                LatestMessage = Timeline[VisibleEvents[eventKey]].DisplayedText;
            else
                LatestMessage = "";

            List<LocationMessage> sessionMessages;
            if (worldMessageQueues.TryGetValue("Session", out sessionMessages))
            {
                HandleMessages(sessionMessages);
                worldMessageQueues.Remove("Session");
            }

            foreach (var keyValue in Worlds)
                keyValue.Value.HandleMessages(worldMessageQueues[keyValue.Key]);

            currentTimelineKey = eventKey;
            currentTimelineTime = currentTimelineKey * 180;

            //Build a list of messages for each planet
            //Send all messages
            //The worlds can decide what ones need to be followed
        }

        public void HandleMessages(List<LocationMessage> messages)
        { 
            
        }

        public void OnStop(object sender, EventArgs e)
        {
            playing = false;
            currentTimelineTime = 0;
            currentTimelineKey = 0;
            MoveToTimelineEvent(0);
        }

        public void OnPause(object sender, EventArgs e)
        {
            playing = false;
        }

        public void OnPlay(object sender, EventArgs e)
        {
            playing = true;
        }

        // TODO: Change to message system
        /// <summary>
        /// Fires when the next timeline key is reached, or when the user selects a key to skip to.
        /// </summary>
        /*public void UpdateTimeline()//This updated each of the worlds
        {
            timelineControl.UpdateTimeline();

            if (currentSession.Scratched && timelineControl.currentTimelineKey == currentSession.VisibleEvents.Length - 1)
                sessionScratched = true;
            else
                sessionScratched = false;

            if (cursorTarget != null && focused && ScreenManager.Globals.Camera.Modifiers[0].GetType() != typeof(MoveToModifier3D))
            {
                cursorTarget = currentSession.Worlds[cursorTarget.Name];

                ScreenManager.Globals.Camera.ClearModifiers();
                ScreenManager.Globals.Camera.AddModifier(new FollowModifier3D(cursorTarget.WorldPlane, false, false, -1));
            }
            for (int i = 0; i < currentSession.Lands.Length; i++)
            {
                if (currentSession.Lands[i].playerEntered != prevLandStates[i])
                {
                    prevLandStates[i] = !prevLandStates[i];
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
                        planetarySpirographs[i].AddModifier(new ColorModifier3D(currentSession.Lands[i].playerColor, true, planetarySpirographs[i], 60));

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
                        planetarySpirographs[i].AddModifier(new ColorModifier3D(Color.Lerp(currentSession.Lands[i].playerColor, Color.Black, 0.5f), true, planetarySpirographs[i], 60));

                        for (int h = 0; h < 7; h++)
                            gateSpirographs[i, h].AddModifier(new ColorModifier3D(Color.TransparentBlack, true, gateSpirographs[i, h], 60));
                    }
                }
            }
            if (prevMessage != currentSession.lastMessage && currentSession.lastMessage != null)
            {
                timeToMessageDecay = 120;
                prevMessage = currentSession.lastMessage;
            }
        }*/

        /// <summary>
        /// The current position on the timeline in ticks.
        /// </summary>
        public int CurrentTimelineTime { get { return currentTimelineTime; } }
        /// <summary>
        /// The latest visible timeline key hit.
        /// </summary>
        public int CurrentTimelineKey { get { return currentTimelineKey; } }

        public SessionManager ShallowCopy()
        {
            var clone = new SessionManager();
            clone.currentTimelineKey = currentTimelineKey;
            clone.currentTimelineTime = currentTimelineTime;
            clone.Description = Description;
            clone.FirstKeyedEvent = FirstKeyedEvent;
            clone.GenericMeteors = GenericMeteors;
            clone.LatestMessage = LatestMessage;
            clone.NullSession = NullSession;
            clone.PlayerWorldCount = PlayerWorldCount;
            clone.playing = playing;
            clone.ReckoningActive = ReckoningActive;
            clone.Scratched = Scratched;
            clone.Timeline = Timeline;
            clone.VisibleEvents = VisibleEvents;
            clone.VoidSession = VoidSession;
            clone.Worlds = Worlds;
            return clone;
        }

        public SessionManager ShallowCopy(LoadArgs l)
        {
            return ShallowCopy();
        }

        public SessionManager DeepCopy()
        {
            var clone = new SessionManager();
            clone.currentTimelineKey = currentTimelineKey;
            clone.currentTimelineTime = currentTimelineTime;
            clone.Description = Description;
            clone.FirstKeyedEvent = FirstKeyedEvent;
            var cloneGenericMeteors = new GenericMeteor[GenericMeteors.Length];
            for (int i = 0; i < GenericMeteors.Length; i++)
                cloneGenericMeteors[i] = GenericMeteors[i].DeepCopy();
            clone.GenericMeteors = cloneGenericMeteors;
            clone.LatestMessage = LatestMessage;
            clone.NullSession = NullSession;
            clone.PlayerWorldCount = PlayerWorldCount;
            clone.playing = playing;
            clone.ReckoningActive = ReckoningActive;
            clone.Scratched = Scratched;
            var cloneTimeline = new TimelineEvent[Timeline.Length];
            for (int i = 0; i < Timeline.Length; i++)
                cloneTimeline[i] = Timeline[i].ShallowCopy();
            clone.Timeline = cloneTimeline;
            var cloneVisibleEvents = new int[VisibleEvents.Length];
            for (int i = 0; i < VisibleEvents.Length; i++)
                cloneVisibleEvents[i] = VisibleEvents[i];
            clone.VisibleEvents = cloneVisibleEvents;
            clone.VoidSession = VoidSession;
            var cloneWorlds = new Dictionary<string, Location>(Worlds.Count);
            foreach (var keyValue in Worlds)
            {
                if (keyValue.Value is SessionLocation)
                    cloneWorlds.Add(keyValue.Key, (keyValue.Value as SessionLocation).DeepCopy());
                else if (keyValue.Value is PlayerWorld)
                    cloneWorlds.Add(keyValue.Key, (keyValue.Value as PlayerWorld).DeepCopy());
                else if (keyValue.Value is Skaia)
                    cloneWorlds.Add(keyValue.Key, (keyValue.Value as Skaia).DeepCopy());
                else
                    throw new NotImplementedException("The type " + keyValue.Value.GetType().Name + " is not implemented in SessionManager");
            }
            clone.Worlds = cloneWorlds;
            return clone;
        }

        public SessionManager DeepCopy(LoadArgs l)
        {
            return DeepCopy();
        }
    }
}
