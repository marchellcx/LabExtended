namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate starts idling.
    /// </summary>
    public class TeslaGateStartedIdlingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the tesla gate that started idling.
        /// </summary>
        public ExTeslaGate Gate { get; }

        /// <summary>
        /// Creates a new <see cref="TeslaGateStartedIdlingEventArgs"/> instance.
        /// </summary>
        /// <param name="gate">The gate that started idling.</param>
        public TeslaGateStartedIdlingEventArgs(ExTeslaGate gate)
        {
            Gate = gate;
        }
    }
}