using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Channel class simulating a reliable communication channel with a fixed
    /// delay between two actors. This channel is used as default for 
    /// simulated actors, because it is simple to use.
    /// </summary>
    public class DeterministicChannel : CommunicationChannel
    {
        /// <summary>
        /// Fixed delay for this channel. All messages transmitted via this 
        /// channel take (delay + 1) calls to <c>Tick()</c> to send.
        /// </summary>
        private int delay;
        /// <summary>
        /// Constructs a new DeterministicChannel object.
        /// </summary>
        /// <param name="delay">fixed delay for each message, 
        /// set to zero for instant transmission of messages (arrive at 
        /// next call of <c>Tick()</c>)</param>
        public DeterministicChannel(int delay)
        {
            this.delay = delay;
        }
        /// <summary>
        /// Adds a new message to the messages currently transmitted via this 
        /// channel.
        /// </summary>
        /// <param name="m">the message to be sent</param>
        public override void Send(Message m)
        {
            messagesInDelivery.Add(new Tuple<int, Message>(delay, m));
        }
    }
}
