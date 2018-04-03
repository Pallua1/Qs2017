using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Abstract base class for CommunicationChannel class. It is abstract rather than 
    /// an interface to alleviate exercise 3.
    /// 
    /// <para>
    /// The class simulates a communication channel existing between two actors, which
    /// causes some delay for the transmission of messages. This corresponds e.g. to 
    /// TCP/IP connections in actor applications.
    /// </para>
    /// </summary>
    public abstract class ICommunicationChannel
    {
        /// <summary>
        /// The messages which are currently in transit on this channel.
        /// The first tuple element defines the ticks left until, the message
        /// (the second tuple element) reaches it destination.
        /// </summary>
        protected List<Tuple<int, Message>> messagesInDelivery { get; set; }
        /// <summary>
        /// This method is used to send messages via this channel.
        /// Depending on the actual implementation varying delays can be added, or 
        /// messages can be dropped (e.g. to test parts of the application).
        /// </summary>
        /// <param name="m">non-null message to be sent</param>
        public abstract void Send(Message m);

        /// <summary>
        /// This method is used to signal to the channel object that one time unit 
        /// has passed. The ticks left for all messages in transit should be decremented,
        /// except for those having zero ticks left, those messages should be returned, 
        /// because they reached their destinations.
        /// </summary>
        /// <returns>all messages having zero ticks left (upon entering the method)</returns>
        public abstract List<Message> Tick();

        /// <summary>
        /// Constructs a new ICommunicationChannel object.
        /// </summary>
        protected ICommunicationChannel()
        {
            this.messagesInDelivery = new List<Tuple<int, Message>>();
        }
    }
}
