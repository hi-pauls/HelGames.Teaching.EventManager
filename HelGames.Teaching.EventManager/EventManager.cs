// -----------------------------------------------------------------------
// <copyright file="EventManager.cs" company="HelGames Company Identifier">
// Copyright 2014 HelGames Company Identifier. All rights reserved.
// </copyright>
// <author>Paul Schulze</author>
// -----------------------------------------------------------------------
namespace HelGames.Teaching.EventManager
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the signature for an event handler delegate. This signature has to be implemented
    /// by any method, that needs to work as an event handler and therefor needs to be registered
    /// using <see cref="EventManager.RegisterListener" />.
    /// </summary>
    /// <param name="evt">
    /// The <see cref="IEvent"/> event, the event manager received.
    /// </param>
    public delegate void EventHandlerDelegate(IEvent evt);

    /// <summary>
    /// Defines the EventManager, responsible for notifying registered handlers of events.
    /// <para>
    /// The implementation of this event manager class uses a queing approach to events,
    /// meaning that events normally are queued using <see cref="EventManager.QueueEvent" />
    /// and only fired when calling <see cref="EventManager.ProcessEvents" />. It will also
    /// not process any new events, that are queued during processing, those will be processed
    /// during the next call to <see cref="EventManager.ProcessEvents" />. This is a safe
    /// approach, as it will not result in an endless loop during processing and will limit
    /// the number of events, processed during one frame. It will prevent event handling and
    /// event loops from starving other parts of the game. However, it will also result in
    /// slight latency when the game depends on long chains of events.
    /// </para>
    /// <para>
    /// For events, that need to be processed with as little latency as possible, the event
    /// manager provides the method <see cref="EventManager.FireEvent" />. This method should
    /// however be used sparingly, as it doesn't offer the same safety net as calling the
    /// <see cref="EventManager.QueueEvent" /> method. Also note, that any events, that are queued
    /// during execution of the fired event will only be processed during the next call to the
    /// <see cref="EventManager.ProcessEvents" /> method, adding latency again.
    /// </para>
    /// <para>
    /// Please note, that this class does not include any safety-checks for invalid arguments
    /// (like null values for things, that may not be null). This they were intentionally left
    /// out to keep the code as short and concise as possible. For that reason, this implementation
    /// is not fit for production use.
    /// </para>
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// Hosts the Dictionary of event types to <see cref="EventHandlerDelegate" /> event handler delegates for that type.
        /// When processing an event, this dictionary is used to retreive all registered for the specific type of the event.
        /// Handlers are added to this dictionary by using <see cref="EventManager.RegisterListener" /> and can be removed
        /// using <see cref="EventManager.RemoveListener" />.
        /// </summary>
        private Dictionary<object, EventHandlerDelegate> handlers = new Dictionary<object, EventHandlerDelegate>();

        /// <summary>
        /// Hosts the List of events, queued since the last time,
        /// <see cref="EventManager.ProcessEvents" /> was called.
        /// </summary>
        private List<IEvent> queuedEvents = new List<IEvent>();

        /// <summary>
        /// Register an event handler.
        /// </summary>
        /// <param name="eventType">
        /// The <see cref="System.Object"/> type of event to register the listener for.
        /// This value may not be null.
        /// </param>
        /// <param name="eventHandler">
        /// The <see cref="EventHandlerDelegate"/> delegate, to call, whenever the given event happens.
        /// This value may not be null.
        /// </param>
        public void RegisterListener(object eventType, EventHandlerDelegate eventHandler)
        {
            EventHandlerDelegate handler;
            if (this.handlers.TryGetValue(eventType, out handler))
            {
                // Remove the handler first, before adding it again. This prevents the
                // handler from being registered twice in the multi-cast delegate.
                handler -= eventHandler;
                handler += eventHandler;

                // Don't forget to re-assign the handler to, as delegates have overloaded
                // + and - operators, making them essentially behave like immutable objects.
                // They may also change state from being a Delegate object to becoming a
                // MulticastDelegate thingy, which requires those overloads.
                this.handlers[eventType] = handler;
            }
            else
            {
                // The event type does not have a handler yet, add a new entry
                // to the dictionary for the given event type.
                handler = eventHandler;
                this.handlers.Add(eventType, handler);
            }
        }

        /// <summary>
        /// Remove a previously registered event handler from handling the given type of event.
        /// Should the handler not be registered for the given event, the call is ignored.
        /// </summary>
        /// <param name="eventType">
        /// The <see cref="System.Object"/> type of event to remove  the listener for.
        /// This value may not be null.
        /// </param>
        /// <param name="eventHandler">
        /// The <see cref="EventHandlerDelegate"/> delegate, to call, whenever the given event happens.
        /// This value may not be null.
        /// </param>
        public void RemoveListener(object eventType, EventHandlerDelegate eventHandler)
        {
            EventHandlerDelegate handler;
            if (this.handlers.TryGetValue(eventType, out handler))
            {
                // Remove the specified handler from the multi-cast delegate.
                handler -= eventHandler;
            }
        }

        /// <summary>
        /// Queue an event for processing. The handlers for the given event will only
        /// be executed once <see cref="EventManager.ProcessEvents" /> is called.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="IEvent"/> event to send to all registered listeners during the
        /// next call to <see cref="EventManager.ProcessEvents" />.
        /// </param>
        public void QueueEvent(IEvent evt)
        {
            // Simply add the event to the queue.
            this.queuedEvents.Add(evt);
        }

        /// <summary>
        /// Fire an event immediately. This will only execute the event handlers for the
        /// given event type immediately. Any events, being fired from those handlers
        /// using <see cref="EventManager.QueueEvent" /> will still be queued as usual.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="IEvent"/> event to send to all registered listeners immediately.
        /// </param>
        public void FireEvent(IEvent evt)
        {
            this.ProcessEvent(evt);
        }

        /// <summary>
        /// Process all events, that were queued. Usually, this should be called once per frame. It
        /// will cause the event manager to process its list of currently queued events, which will
        /// result in event handlers being called once for each queued event, it is registered for.
        /// </summary>
        public void ProcessEvents()
        {
            // Use the copy constructor, so the new list knows how long it needs to be.
            List<IEvent> currentEvents = new List<IEvent>(this.queuedEvents);

            // Now clear the list of queued events, so the handlers of the current events
            // can queue new events. Those events will be executed during the next time, this
            // method is called.
            this.queuedEvents.Clear();

            // Now loop over the current events and process them one by one.
            foreach (IEvent evt in currentEvents)
            {
                this.ProcessEvent(evt);
            }
        }

        /// <summary>
        /// Process the given event. This is a utility method, that is called by both,
        /// <see cref="EventManager.FireEvent" /> and <see cref="EventManager.ProcessEvents" />. It
        /// will try to get the multi-cast delegate from the list of registered handlers and upon
        /// success, will execute that delegate, calling all the individual delegates, it consists of.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="IEvent"/> event to send to all registered listeners.
        /// </param>
        private void ProcessEvent(IEvent evt)
        {
            EventHandlerDelegate handler;
            if (this.handlers.TryGetValue(evt.EventType, out handler))
            {
                handler(evt);
            }
        }
    }
}