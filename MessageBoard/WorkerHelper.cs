using ActorSystem;

namespace MessageBoard
{
    /// <summary>
    /// Helper which should only send one message to the message store 
    /// and then forward the response to the client. As all workers share
    /// one message store, messages could get dropped (this can be simulated
    /// using different channel implementations for the message store), so
    /// this actor will resend messages, if it does not receive a response 
    /// for a predefined amount of time.
    /// 
    /// Such simple actors are common in programs using the actor model.
    /// </summary>
    public class WorkerHelper : SimulatedActor
    {
        /// <summary>
        /// The message which should be sent to the message store
        /// </summary>
        protected MessageStoreMessage message { get; set; }
        /// <summary>
        /// the message store actor
        /// </summary>
        protected SimulatedActor messageStore { get; set; }
        /// <summary>
        /// the client to which the response should be forwarded
        /// </summary>
        protected SimulatedActor client { get; set; }
        /// <summary>
        /// the actor system which is used for stopping after
        /// forwarding the response
        /// </summary>
        protected SimulatedActorSystem system { get; set; }
        /// <summary>
        /// counts the number of ticks since the message 
        /// was sent to the message store
        /// </summary>
        protected int timeSinceLastSent { get; set; }
        /// <summary>
        /// Used to mark that the actor is stopping and should 
        /// not try resending the message anymore
        /// </summary>
        protected bool stopping { get; set; }
        /// <summary>
        /// count how often the message was resent
        /// </summary>
        protected int retries { get; set; }

        /// <summary>
        /// maximum number of resends
        /// </summary>
        protected const int MAX_RETRIES = 2;

        /// <summary>
        /// Constructs a new WorkerHelper object.
        /// </summary>
        /// <param name="messageStore">message store which receives messages from helper</param>
        /// <param name="client">client to which the message from the store gets forwarded</param>
        /// <param name="message">the message to be sent to the message store</param>
        /// <param name="system">actor system used to stop the helper</param>
        public WorkerHelper(SimulatedActor messageStore, SimulatedActor client, MessageStoreMessage message, SimulatedActorSystem system)
        {
            this.message = message;
            this.message.StoreClient = this;
            this.messageStore = messageStore;
            this.client = client;
            this.system = system;
            this.timeSinceLastSent = 0;
            this.stopping = false;
            this.retries = 0;

            // good connection between WorkerHelper and MessageStore -> no delay
            this.channel = new DeterministicChannel(0);
        }
        /// <summary>
        /// After spawning the message should be sent for the first time to the message store.
        /// </summary>
        public override void atStartUp()
        {
            base.atStartUp();
            messageStore.Tell(message);
            timeSinceLastSent = 0;
        }
        /// <summary>
        /// We assume that the helper only receives reply messages from the message store,
        /// which it must forward to clients. 
        /// </summary>
        /// <param name="m"></param>
        public override void Receive(Message m)
        {
            // only forwarding to client
            client.Tell(m);
            system.Stop(this);
            stopping = true; // mark as stopping, in order to avoid accidental resending in tick below
        }
        /// <summary>
        /// Overriden Tick()-method, which counts the time units passed since
        /// the message was sent the last time and the number of sending 
        /// retries.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            // as all workers share one MessageStore instance, it might happen that messages are dropped
            if (!stopping && timeSinceLastSent++ >= 3) 
            {
                if (retries == MAX_RETRIES)
                {
                    client.Tell(new OperationFailed(message.CommunicationId));
                    system.Stop(this);
                }
                else
                {
                    messageStore.Tell(message);
                    timeSinceLastSent = 0;
                    retries++;
                }
            }
        }
    }
}