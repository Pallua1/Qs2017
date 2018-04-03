using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Abstract base class for SimulatedActor class. It is abstract rather than 
    /// an interface to alleviate writing CodeContracts.
    /// 
    /// <para>
    /// It simulates actors known from other programming languages/frameworks like
    /// Erlang or Akka, which is available for Java and Scala. 
    /// </para>
    /// <para>
    /// Actors are basically concurrently running entities, which communicate via 
    /// asynchronous message-passing. In our model, we will simulate
    /// time, respectively concurrency, by using a Tick(), which essentially
    /// signals that one time unit has passed. This method is called on all currently 
    /// active actors at each time step and by that we simulate concurrency. 
    /// 
    /// In general actors should be loosely coupled and their should only be seen 
    /// and manipulated by the actor itself. Hence, it is also suggested to test 
    /// actors mainly by sending messages and checking the responses. However,
    /// this is not a requirement for the exercises, unless otherwise specified.
    /// 
    /// </para>
    /// 
    /// </summary>
    public abstract class ISimulatedActor
    {
        /// <summary>
        /// Channel, which simulates the communication channel built-up when 
        /// messages are sent to an actors. A more accurate approximation
        /// of the real world would include one channel per communication/pair
        /// of actors, but for the sake of simplicity we use only one per actor.
        /// 
        /// It is protected to be able to set it to another channel in deriving classes.
        /// </summary>
        protected CommunicationChannel channel { get; set; }

        /// <summary>
        /// Unique id assigned to each actor
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Remaining number of ticks, for which this actor is busy processing a message. 
        /// This way we simulate that messages take time to process.
        /// </summary>
        protected int busyFor { get; set; }
        /// <summary>
        /// The message currently being processed.
        /// </summary>
        protected Message activeMessage {get;set;}
        /// <summary>
        /// All messages, which already been sent via <c>channel</c>, but have been processed
        /// yet.
        /// </summary>
        protected Queue<Message> messageBox {get;set;}
        /// <summary>
        /// All messages sent to this actor, this includes messages 
        /// in transit, already processed messages and messages in the <c>messageBox</c>. 
        /// It is used to alleviate debugging and testing.
        /// </summary>
        public List<Message> MessageLog { get; protected set; }

        /// <summary>
        /// Property logging the time since the system was started, initially (after the
        /// construction) it is -1, shall be set to the current system time 
        /// right after atStartUp is called and shall be incremented when Tick() 
        /// is called. 
        /// 
        /// If SimulatedActor.Receive throws an exception, it might not correctly
        /// reflect the current system time, but for the sake of simplicity, we 
        /// ignore this fact.
        /// </summary>
        public int TimeSinceSystemStart { get; set; }

        /// <summary>
        /// This abstract method shall be implemnted by all concrete actors
        /// inidividually and shall contain all actor-specific logic (e.g. state updates). 
        /// </summary>
        /// <param name="m">non-null message received</param>
        public abstract void Receive(Message m);

        /// <summary>
        /// Constructs and ISimulatedActor instance
        /// </summary>
        public ISimulatedActor()
        {
            channel = new DeterministicChannel(1);
            Id = SimulatedActorSystem.NEW_ACTOR;
            busyFor = 0;
            activeMessage = null;
            messageBox = new Queue<Message>();
            MessageLog = new List<Message>();
            TimeSinceSystemStart = -1;
        }
        /// <summary>
        /// This is method is used to send messages to the actor
        /// represented by <c>this</c>. Messages should also be
        /// logged.
        /// </summary>
        /// <param name="m">non-null message to be sent</param>
        public abstract void Tell(Message m);

        /// <summary>
        /// Method to signal to the actor that one time-unit has passed.
        /// If the actor is currently busy, the busyFor-time shall be decreased, 
        /// if busyFor reaches zero, it means that message processing is finished
        /// and the application logic corresponding to the active message can be 
        /// triggered using a call to <c>Receive</c>. 
        /// 
        /// If the actor can process a new message, it shall take a new message from 
        /// the <c>messageBox</c> and set the <c>busyFor</c>-time appropriately.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Method which is called when the actor is spawned.
        /// </summary>
        public abstract void atStartUp();
    }
}
