using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActorSystem
{
    /// <summary>
    /// Abstract SimulatedActor class implementing methods common to all actors. 
    /// The Receive method is not implemented, because it should contain 
    /// actor-specific logic.
    /// </summary>
    public abstract class SimulatedActor : ISimulatedActor
    {
        /// <summary>
        /// Contructs a new SimulatedActor 
        /// </summary>
        public SimulatedActor()
        {

        }
        /// <summary>
        /// Implementation of <c>Tell</c>-method.
        /// </summary>
        /// <param name="m">message to be sent</param>
        public override void Tell(Message m)
        {
            channel.Send(m);
            MessageLog.Add(m);
        }

        /// <summary>
        /// Implementation of <c>Tick()</c>-method
        /// </summary>
        public override void Tick()
        {
            TimeSinceSystemStart++;
            List<Message> newlyDelivered = channel.Tick();
            for (int i = 0; i < newlyDelivered.Count; i++)
                messageBox.Enqueue(newlyDelivered[i]);

            if (busyFor > 0)
            {
                busyFor--;
                return;
            }
            Message messageToProcess = null;
            // busyFor is zero, so if there is an activeMessage, we are
            // finished processing it, so we can use Receive for changes
            // to take effect

            if (activeMessage != null)
            {
                messageToProcess = activeMessage;
                activeMessage = null;
            }
            else if (messageBox.Count > 0)
            {
                activeMessage = messageBox.Dequeue();
                busyFor = activeMessage.Duration();
            }
            // might throw an exception, but all the other code should still be executed,
            // but not in a finally block
            // so we use this variable for intermediately storing the message
            if (messageToProcess != null)
                Receive(messageToProcess);
        }

        /// <summary>
        /// Standard implementation of <c>atStartUp()</c> doing nothing.
        /// </summary>
        public override void atStartUp()
        {
        }

    }
}
