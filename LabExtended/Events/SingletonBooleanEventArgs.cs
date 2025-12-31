namespace LabExtended.Events
{
    /// <summary>
    /// Provides event data for singleton event scenarios, using a single shared instance of the event type.
    /// </summary>
    /// <remarks>This class is intended for use in event patterns where a single, reusable instance of event
    /// data is sufficient. The static singleton instance is created using the default constructor of the specified
    /// event type. This can help reduce allocations in high-frequency event scenarios. The ResetEvent method can be
    /// overridden to reset the state of the singleton event data before it is reused.</remarks>
    /// <typeparam name="TEvent">The type of the event data to be used as a singleton instance.</typeparam>
    public class SingletonBooleanEventArgs<TEvent> : BooleanEventArgs
    {
        /// <summary>
        /// Gets a singleton instance of the event type.
        /// </summary>
        /// <remarks>The singleton instance is created using the default constructor of the event type.
        /// This property is useful when a single, shared instance of the event is sufficient for application logic. The
        /// instance is created once and reused for all subsequent accesses.</remarks>
        public static TEvent Singleton { get; } = Activator.CreateInstance<TEvent>();

        /// <summary>
        /// Resets the event to its initial state.
        /// </summary>
        public virtual void ResetEvent()
        {
            IsAllowed = true;
        }
    }
}