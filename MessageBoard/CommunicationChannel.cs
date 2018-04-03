using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Abstract CommunicationChannel class implementing the Tick method, which 
    /// is common to all concrete implementations of communication channels.
    /// </summary>
    public abstract class CommunicationChannel : ICommunicationChannel
    {
        /// <summary>
        ///  Constructs a CommuncationChannel object.
        /// </summary>
        protected CommunicationChannel()
        {
        }
        /// <summary>
        /// Method to signal that time has passed and thus messages have travelled on 
        /// this channel. 
        /// <see cref="ICommunicationChannel.Tick()"/>
        /// </summary>
        /// <returns>messages which arrived at their destination</returns>
        public override List<Message> Tick()
        {
            List<Message> messagesDelivered = new List<Message>();
            List<Tuple<int, Message>> newMessagesInDelivery = new List<Tuple<int, Message>>();
      
            for (int i = 0; i < messagesInDelivery.Count; i++)
            {
                Tuple<int, Message> current = messagesInDelivery[i];
                if (current.Item1 == 0)
                {
                    messagesDelivered.Add(current.Item2);
                }
                else
                {
                    newMessagesInDelivery.Add(new Tuple<int, Message>(current.Item1 - 1, current.Item2));
                }
            }
            messagesInDelivery = newMessagesInDelivery;
            return messagesDelivered;
        }
    }

 
}
