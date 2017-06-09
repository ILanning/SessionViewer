using CommonCode.Content;
using System.Xml.Serialization;

namespace SessionViewer.Content
{
    public class TimelineBuilder : Builder<TimelineBuilder>
    {
        public TimelineEvent[] timeline;

        public TimelineBuilder() { }

        public TimelineBuilder(TimelineEvent[] timeline)
        {
            this.timeline = timeline;
        }
    }

    public struct TimelineEvent
    {
        /// <summary>
        /// The object that this event will be modifying.
        /// </summary>
        public string Target;
        /// <summary>
        /// The type of event that will be occuring.
        /// </summary>
        public EventTypes EventType;
        /// <summary>
        /// A string that is displayed once this event occurs, assuming the event is visible.
        /// </summary>
        public string DisplayedText;
        /// <summary>
        /// Optional filepath of the xml file representing [Target]'s next state.
        /// </summary>
        public string Arguments;
        /// <summary>
        /// If true, this event will create a key on the timeline.
        /// </summary>
        public bool IsVisible;

        /*Event Types:
         
         -Player Entry
         -Reload
         -Player God Tier
         -Player Death
         -Join Session
         -Change Orbit
         -Gate Level [0-7]
         -Toggle Reckoning
         -Victory*/

        /*// <summary>
        /// If true, the player of [Target] is entering on this event.
        /// </summary>
        public bool PlayerEntry;
        //8 = Keep last value.
        /// <summary>
        /// Determines how many gates the player of [Target] has passed through by this event.
        /// </summary>
        public byte GateLevel;
        /// <summary>
        /// If true, the player of [Target] reachees the god tiers on this event.
        /// </summary>
        public bool PlayerGodTier;
        /// <summary>
        /// If true, the player of [Target] dies permanently on this event.
        /// </summary>
        public bool PlayerDeath;
        /// <summary>
        /// If true, the reckoning is toggled on or off upon this event.
        /// </summary>
        public bool ToggleReckoning;
        /// <summary>
        /// If true, the players win upon this event.
        /// </summary>
        public bool Victory;*/

        public TimelineEvent(string target, EventTypes eventType, string description, string path)
        {
            Target = target;
            IsVisible = false;
            EventType = eventType;
            DisplayedText = description;
            Arguments = path;
        }

        public TimelineEvent(string target, EventTypes eventType, bool isVisible, string description, string path)
        {
            Target = target;
            IsVisible = isVisible;
            EventType = eventType;
            DisplayedText = description;
            Arguments = path;
        }

        public TimelineEvent ShallowCopy()
        {
            return new TimelineEvent(Target, EventType, IsVisible, DisplayedText, Arguments);
        }
    }
}
