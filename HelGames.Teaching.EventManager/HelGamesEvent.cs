// -----------------------------------------------------------------------
// <copyright file="HelGamesEvent.cs" company="HelGames Company Identifier">
// Copyright 2014 HelGames Company Identifier. All rights reserved.
// </copyright>
// <author>Paul Schulze</author>
// -----------------------------------------------------------------------
namespace HelGames.Teaching.EventManager
{
    /// <summary>
    /// Defines the HelGamesEvent. This is an implementation of IEvent, that
    /// is intended to be used for sending events and their respective data.
    /// </summary>
    public class HelGamesEvent : IEvent
    {
        /// <summary>
        /// Initializes a new instance of the HelGamesEvent class.
        /// </summary>
        /// <param name="eventType">
        /// The <see cref="System.Object"/> type of the event.
        /// </param>
        /// <param name="eventData">
        /// The <see cref="System.Object"/> context data for the event.
        /// </param>
        public HelGamesEvent(object eventType, object eventData)
        {
            this.EventType = eventType;
            this.EventData = eventData;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public object EventType { get; private set; }

        /// <summary>
        /// Gets the data, to send with the event or null, if the event doesn't require any data.
        /// </summary>
        public object EventData { get; private set; }
    }
}