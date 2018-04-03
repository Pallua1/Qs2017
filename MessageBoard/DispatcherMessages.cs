using ActorSystem;

namespace MessageBoard
{ 
    // Messages sent between dispatcher and worker.

    /// <summary>
    /// Message sent from client to dispatcher to stop the system. 
    /// This message is then forwarded to all workers to stop them.
    /// </summary>
    public class Stop : Message
    {
        public Stop()
        {
        }
        public int Duration()
        {
            return 2;
        }
    }
    /// <summary>
    /// Message sent from worker to dispatcher to acknowledge the 
    /// stop message.
    /// </summary>
    public class StopAck : Message
    {
        /// <summary>
        /// The sender of this message
        /// </summary>
        public SimulatedActor Sender { get; set; }

        public StopAck(SimulatedActor sender)
        {
            Sender = sender;
        }
        public int Duration()
        {
            return 2;
        }
    }
}