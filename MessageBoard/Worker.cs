using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActorSystem;


namespace MessageBoard
{

    /// <summary>
    /// Worker class which contains most of the actual application logic.
    /// </summary>
    public class Worker : SimulatedActor
    {
        /// <summary>
        /// actor responsible for persistence-related tasks
        /// </summary>
        protected SimulatedActor messageStore { get; set; }
        /// <summary>
        /// dispatcher actor, which manages all workers
        /// </summary>
        protected SimulatedActor dispatcher { get; set; }
        /// <summary>
        /// currently active communications with clients, the key of 
        /// the dictionary is a communication ID and the value a reference to the client
        /// </summary>
        protected Dictionary<long, SimulatedActor> ongoingCommunications { get; set; }
        /// <summary>
        /// system used to spawn actors
        /// </summary>
        protected SimulatedActorSystem system { get; set; }
        /// <summary>
        /// flag which is set if the worker is about to be stopped
        /// </summary>
        protected bool stopping { get; set; }

        /// <summary>
        /// Constructs a new Worker object
        /// </summary>
        /// <param name="disp">the dispatcher</param>
        /// <param name="messageStore">the message store responsible for persistence</param>
        /// <param name="system">the actor system simulation</param>
        public Worker(SimulatedActor disp, SimulatedActor messageStore, SimulatedActorSystem system)
        {
            this.dispatcher = disp;
            this.messageStore = messageStore;
            this.ongoingCommunications = new Dictionary<long, SimulatedActor>();
            this.system = system;
            this.stopping = false;
        }
        /// <summary>
        /// Receive method which chooses the actions to perform depending on the message type.
        /// Accepts the Stop message from the dispatcher and all ClientMessage messages except 
        /// the reply message OperationAck, InitAck,FinishAck and OperationFailed. 
        /// It does not accept any messages while stopping and responds with back 
        /// OperationFailed messages during stopping. 
        /// If an unknown communication ID is used for ClientMessage messages, an UnknownClientException-
        /// exception is thrown. Further documentation can be found above helper methods named processMessageType.
        /// </summary>
        /// <param name="m">non-null message</param>
        public override void Receive(Message m)
        {
            if (stopping && m is ClientMessage)
            {
                // all operations while stopping fail
                ClientMessage clientMessage = (ClientMessage)m;
                if (!ongoingCommunications.ContainsKey(clientMessage.CommunicationId))
                    throw new UnknownClientException("Unknown communication ID");
                ongoingCommunications[clientMessage.CommunicationId].Tell(new OperationFailed(clientMessage.CommunicationId));
            }
            else if (m is InitCommunication)
            {
                processInitCommunication(m);
            }
            else if (m is FinishCommunication)
            {
                processFinishCommunication(m);
            }
            else if (m is Stop)
            {
                processStop();
            }
            else if (m is Publish)
                processPublish(m);
            else if (m is RetrieveMessages)
            {
                processRetrieveMessages(m);
            }
            else if (m is Like)
            {
                processLike(m);
            }
            else if (m is Dislike)
            {
                processDislike(m);
            }
        }
        /// <summary>
        /// Initiates communication with a client and sends an InitAck message to it, 
        /// which contains a reference to <c>this</c>.
        /// After that other ClientMessage messages can be sent to this worker 
        /// using the communication ID given in the received message.
        /// </summary>
        /// <param name="m">non-null message of type InitCommunication</param>
        protected void processInitCommunication(Message m)
        {
            InitCommunication initC = (InitCommunication)m;
            ongoingCommunications[initC.CommunicationId] = initC.Client;
            initC.Client.Tell(new InitAck(this, initC.CommunicationId));
        }
        /// <summary>
        /// Finishes communication with a client and sends a FinishAck message to it.
        /// After that other ClientMessage messages, using the communication ID given 
        /// in the received message, cannot be sent to this worker anymore.
        /// </summary>
        /// 
        /// <param name="m">non-null message of type FinishCommunication</param>
        protected void processFinishCommunication(Message m)
        {
            FinishCommunication finC = (FinishCommunication)m;

            if (!ongoingCommunications.ContainsKey(finC.CommunicationId))
                throw new UnknownClientException("Unknown communication ID");
            SimulatedActor client = ongoingCommunications[finC.CommunicationId];
            ongoingCommunications.Remove(finC.CommunicationId);
            client.Tell(new FinishAck(finC.CommunicationId));
        }
        /// <summary>
        /// Changes into stopping mode and acknowledges stopping to the dispatcher.
        /// </summary>
        protected void processStop()
        {
            dispatcher.Tell(new StopAck(this));
            stopping = true;
        }

        /// <summary>
        /// Spawns a worker helper which communicates with the message store to retrieve
        /// messages of the author given in the message passed as parameter.
        /// </summary>
        /// <param name="m">non-null message of type RetrieveMessages</param>
        protected void processRetrieveMessages(Message m)
        {
            RetrieveMessages retrMessages = (RetrieveMessages)m;
            if (!ongoingCommunications.ContainsKey(retrMessages.CommunicationId))
                throw new UnknownClientException("Unknown communication ID");
            SimulatedActor client = ongoingCommunications[retrMessages.CommunicationId];

            MessageStoreMessage message = new RetrieveFromStore(retrMessages.Author, retrMessages.CommunicationId);
            WorkerHelper helper = new WorkerHelper(messageStore, client, message, system);
            system.Spawn(helper);
        }
        /// <summary>
        /// Spawns a worker helper which communicates with the message store to add a like
        /// to a user message given in the message passed as parameter.
        /// </summary>
        /// <param name="m">non-null message of type Like</param>
        protected void processLike(Message m)
        {
            Like like = (Like)m;
            if (!ongoingCommunications.ContainsKey(like.CommunicationId))
                throw new UnknownClientException("Unknown communication ID");
            SimulatedActor client = ongoingCommunications[like.CommunicationId];
            MessageStoreMessage message = new AddLike(like.ClientName, like.MessageId, like.CommunicationId);
            WorkerHelper helper = new WorkerHelper(messageStore, client, message, system);
            system.Spawn(helper);
        }
        /// <summary>
        /// Spawns a worker helper which communicates with the message store to add a dislike
        /// to a user message given in the message passed as parameter.
        /// </summary>
        /// <param name="m">non-null message of type Dislike</param>
        protected void processDislike(Message m)
        {
            Dislike like = (Dislike)m;
            if (!ongoingCommunications.ContainsKey(like.CommunicationId))
                throw new UnknownClientException("Unknown communication ID");
            SimulatedActor client = ongoingCommunications[like.CommunicationId];
            MessageStoreMessage message = new AddDislike(like.ClientName, like.MessageId, like.CommunicationId);
            WorkerHelper helper = new WorkerHelper(messageStore, client, message, system);
            system.Spawn(helper);
        }
        /// <summary>
        /// Performs checks on a user message, which should be published. If the
        /// checks are passed, a worker helper is spawned, which communicates with 
        /// the message store to store the new user message.
        /// New messages must have zero likes, must not have a message ID assigned 
        /// and must not be (strictly) longer than 10 characters.
        /// Only 10 characters are allowed to to alleviate exercise 4.
        /// </summary>
        /// <param name="m">non-null message of type Publish</param>
        protected void processPublish(Message m)
        {
            Publish publish = (Publish)m;
            if (!ongoingCommunications.ContainsKey(publish.CommunicationId))
                throw new UnknownClientException("Unknown communication ID");
            SimulatedActor client = ongoingCommunications[publish.CommunicationId];
            UserMessage userMessage = publish.Message;
            if (userMessage.Likes.Count > 0 || userMessage.Dislikes.Count > 0 || userMessage.MessageId != UserMessage.NEW
                || userMessage.Message.Length > 10)
            {
                client.Tell(new OperationFailed(publish.CommunicationId));
            }
            else
            {
                MessageStoreMessage message = new UpdateMessageStore(userMessage, publish.CommunicationId);
                WorkerHelper helper = new WorkerHelper(messageStore, client, message, system);
                system.Spawn(helper);
            }
        }
    }
}
