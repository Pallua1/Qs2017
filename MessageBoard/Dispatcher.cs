using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActorSystem;

namespace MessageBoard
{
    /// <summary>
    /// Dispatcher class, which acts as an interface to new clients.
    /// Upon communication initialization it selects a worker and forwards
    /// the communication request to it. It is also responsible for stopping
    /// the system.
    /// </summary>
    public class Dispatcher : SimulatedActor
    {
        /// <summary>
        /// Dispatcher mode which can either be normal or stopping, 
        /// which means that new communication requests are ignored.
        /// </summary>
        public enum Mode { NORMAL, STOPPING };
        /// <summary>
        /// mode property defining the mode currently active <see cref="Dispatcher.Mode"/>
        /// </summary>
        protected Mode mode { get; set; }

        /// <summary>
        /// Worker actors, which are managed by this actor
        /// </summary>
        protected List<Worker> workers { get; set; }
        /// <summary>
        /// The system, which is used to spawn actors.
        /// </summary>
        protected SimulatedActorSystem system { get; set; }
        /// <summary>
        /// List of acknowledgement messages to collect, which is only non-empty
        /// in stopping mode- The list elements correspond to the actor-IDs of workers,
        /// which have not yet acknowledged the stop messages sent to them.
        /// </summary>
        protected List<long> acksToCollect { get; set; }
        /// <summary>
        /// message store, which is used by workers to persist application data.
        /// </summary>
        protected MessageStore messageStore { get; set; }

        
        /// <summary>
        /// Constructs a new Dispatcher object
        /// </summary>
        /// <param name="system">system used to spawn actors</param>
        /// <param name="numberOfWorkers">number of workers to be spawned</param>
        public Dispatcher(SimulatedActorSystem system, int numberOfWorkers)
        {
            this.system = system;
            this.workers = new List<Worker>(numberOfWorkers);
            this.mode = Mode.NORMAL;
            this.acksToCollect = new List<long>();
        }
        /// <summary>
        /// Creates all Workers and the message store
        /// </summary>
        public override void atStartUp()
        {
            messageStore = new MessageStore();
            base.atStartUp();
            for (int i = 0; i < workers.Capacity; i++)
            {
                Worker w = new Worker(this,messageStore,system);
                system.Spawn(w);
                workers.Add(w);
            }
            system.Spawn(messageStore);
        }
        /// <summary>
        /// Depending on messages sent and the mode, different actions are performed.
        /// </summary>
        /// <param name="m">non-null message received</param>
        public override void Receive(Message m)
        {
            if (mode == Mode.NORMAL)
            {
                NormalOperation(m);
            }
            if (mode == Mode.STOPPING)
            {
                Stopping(m);
            }
        }
        /// <summary>
        /// In stopping mode, InitCommunication always fail, which is signal 
        /// using an OperationFailed message sent to the client. 
        /// In this mode, only StopAck-messages are expected and if all stop acknowledgements
        /// have been collected, the Dispatcher stop itself.
        /// </summary>
        /// <param name="m">non-null message received</param>
        private void Stopping(Message m)
        {
            if (m is InitCommunication)
            {
                InitCommunication initM = ((InitCommunication)m);
                initM.Client.Tell(new OperationFailed(initM.CommunicationId));
            }
            else if (m is StopAck)
            {
                SimulatedActor actor = ((StopAck)m).Sender;
                acksToCollect.Remove(actor.Id);
                system.Stop(actor);
                if (acksToCollect.Count == 0)
                {
                    system.Stop(messageStore);
                    system.Stop(this);
                }
            }
        }
        /// <summary>
        /// In normal operation messages are forwarded to workers.
        /// A InitCommunication-messages are forwarded to one worker
        /// which is selected based on the communication id set in the message.
        /// The selection scheme is (if workers are numbered from 0 to n - 1) 
        /// selectworkernumber = communication % n, where a % b is the non-negative
        /// remainder of the integer division a/b.
        /// If a Stop message is sent, it is broadcast to all workers and the mode 
        /// is switched to STOPPING. 
        /// </summary>
        /// <param name="m">non-null message received</param>
        private void NormalOperation(Message m)
        {
            if (m is Stop)
            {
                foreach (Worker w in workers)
                {
                    acksToCollect.Add(w.Id);
                    w.Tell(new Stop());
                }
                mode = Mode.STOPPING;
            }
            else if (m is InitCommunication)
            {
                // decide upon id for now, maybe switch to login credentials TODO
                InitCommunication initC = ((InitCommunication)m);
                int rnd = new Random((int)initC.CommunicationId).Next();
                int index = (((rnd % workers.Count) + workers.Count) % workers.Count);
                Worker w = workers[index];
                w.Tell(m);
            }
        }

    }
}
