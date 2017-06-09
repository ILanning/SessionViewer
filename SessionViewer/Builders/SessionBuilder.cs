using CommonCode.Content;

namespace SessionViewer.Content
{
    public class SessionBuilder : Builder<SessionBuilder>
    {
        /// <summary>
        /// Set to true if the session fails to result in a universe.
        /// </summary>
        public bool NullSession;
        /// <summary>
        /// Set to true if no pre-entry prototypings occur.
        /// </summary>
        public bool VoidSession;
        /// <summary>
        /// Set to true if the players scratch their session.
        /// </summary>
        public bool Scratched;
        /// <summary>
        /// A description of the session as a whole.
        /// </summary>
        public string Description;
        /// <summary>
        /// Initial Skaia file.
        /// </summary>
        public string Skaia;
        /// <summary>
        /// Initial Prospit file.
        /// </summary>
        public string Prospit;
        /// <summary>
        /// Initial Derse file.
        /// </summary>
        public string Derse;
        /// <summary>
        /// Initial Land files.
        /// </summary>
        public string[] Lands;
        /// <summary>
        /// Meteors that will be copied several times and scattered about the outer rim.
        /// </summary>
        public string[] GenericMeteors;
        /// <summary>
        /// Any other locations in the session that can be interacted with.
        /// </summary>
        public string[] OtherLocations;
        /// <summary>
        /// Array containing all events that occur within this session's timeline.
        /// </summary>
        public TimelineEvent[] Timeline;

        public SessionBuilder() { }

        public SessionBuilder(string skaia, string prospit, string derse, string[] lands, TimelineEvent[] events, bool nullSession, bool voidSession, bool scratched)
        {
            Skaia = skaia;
            Prospit = prospit;
            Derse = derse;
            Lands = lands;
            Timeline = events;
            VoidSession = voidSession;
            NullSession = nullSession;
            Scratched = scratched;
        }
    }
}
