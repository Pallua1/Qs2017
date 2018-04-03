using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Interface which all messages need to implement.
    /// Message instances are used for communication between actors.
    /// </summary>
    public interface Message
    {
        /// <summary>
        /// To simulate that the processing of messages takes a certain amount 
        /// of time, we use this method to return the number of ticks
        /// it should take an actor to process the message. As a simplification
        /// all implementing classes return a constant value. However, the only 
        /// restriction is that return value must be greater or equal to zero.
        /// </summary>
        /// <returns>number of ticks it take to process this message</returns>
        int Duration();
    }
}
