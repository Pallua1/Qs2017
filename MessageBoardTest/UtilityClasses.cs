using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActorSystem;
using MessageBoard;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulatedActorUnitTests
{
    /// <summary>
    /// Simple actor, which can be used in tests, e.g. to check if
    /// the correct messages are sent by workers. This actor can be sent 
    /// to workers as client.
    /// </summary>
    public class TestClient : SimulatedActor
    {
        /// <summary>
        /// messages received by this actor.
        /// </summary>
        public Queue<Message> ReceivedMessages { get; private set; }
        public TestClient()
        {
            ReceivedMessages = new Queue<Message>();
        }
        /// <summary>
        /// does not implement any logic, only saves the received messages
        /// </summary>
        /// <param name="m"></param>
        public override void Receive(Message m)
        {
            ReceivedMessages.Enqueue(m);
        }

    }
}
