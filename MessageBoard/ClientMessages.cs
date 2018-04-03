using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActorSystem;

namespace MessageBoard
{
    /// <summary>
    /// The abstract base class for all messages sent between clients of 
    /// the message board (people who want to post messages) and the dispatcher
    /// respectively worker. 
    /// 
    /// All communications have a communication ID, which is defined in this 
    /// class. For simplicity, we assume that clients choose this number
    /// and that it will be unique across all clients.
    /// 
    /// Client messages are generally requests from clients to which the workers
    /// react with an appropriate response (e.g. operation acknowledge/failure).
    /// </summary>
    public abstract class ClientMessage : Message
    {
        /// <summary>
        /// some unique ID, identifies one communication/session
        /// </summary>
        public long CommunicationId { get; set; }
        public abstract int Duration();
    }
    /// <summary>
    /// This message is sent from clients to the dispatcher and then forwarded
    /// to workers to initiate communication.
    /// </summary>
    public class InitCommunication : ClientMessage
    {
        /// <summary>
        /// the client trying to set up the communication
        /// </summary>
        public SimulatedActor Client { get; set; }
        public InitCommunication(SimulatedActor client, long communicationId)
        {
            this.Client = client;
            this.CommunicationId = communicationId;
        }
        public override int Duration()
        {
            return 2;
        }
    }
    /// <summary>
    /// Message sent from worker to client to tell the client 
    /// that the communication initiation was successful
    /// </summary>
    public class InitAck : ClientMessage
    {
        /// <summary>
        /// the worker serving the client during this communcation/session
        /// this worker reference can be used to send messages to
        /// </summary>
        public SimulatedActor Worker { get; set; }

        public InitAck(SimulatedActor worker, long communicationId)
        {
            this.Worker = worker;
            this.CommunicationId = communicationId;
        }
        public override int Duration()
        {
            return 1;
        }
    }
    /// <summary>
    /// Message sent from client to worker to end the communication/session.
    /// </summary>
    public class FinishCommunication : ClientMessage
    {
        public FinishCommunication(long communicationId)
        {
            this.CommunicationId = communicationId;
        }
        public override int Duration()
        {
            return 3;
        }
    }
    /// <summary>
    /// Message sent from worker to client to show that the communication 
    /// teardown was successful.
    /// </summary>
    public class FinishAck : ClientMessage
    {
        public FinishAck(long communicationId)
        {
            this.CommunicationId = communicationId;
        }
        public override int Duration()
        {
            return 1;
        }
    }
    /// <summary>
    /// Message sent from client to worker to publish new user messages.
    /// </summary>
    public class Publish : ClientMessage
    {
        /// <summary>
        /// the actual user message to be posted
        /// </summary>
        public UserMessage Message { get; set; }
        public override int Duration()
        {
            return 3;
        }
        public Publish(UserMessage message, long communicationId)
        {
            this.Message = message;
            this.CommunicationId = communicationId;
        }
    }
    /// <summary>
    /// Message sent from client to worker to retrieve all user messages
    /// written by a given author.
    /// </summary>
    public class RetrieveMessages : ClientMessage
    {
        /// <summary>
        /// the author of whom the messages should be looked up
        /// </summary>
        public string Author { get; set; }
        public RetrieveMessages(string author, long communicationId)
        {
            this.Author = author;
            this.CommunicationId = communicationId;
        }
        public override int Duration()
        {
            return 3;
        }
    }
    /// <summary>
    /// The response to the RetrieveMessages message sent from worker to 
    /// client containing all user messages written by the author defined
    /// in the message above.
    /// </summary>
    public class FoundMessages : ClientMessage
    {
        /// <summary>
        /// list of user messages written by one author
        /// </summary>
        public List<UserMessage> Messages { get; set; }
        public FoundMessages(List<UserMessage> messages, long communicationId)
        {
            this.Messages = messages;
            this.CommunicationId = communicationId;
        }
        public override int Duration()
        {
            return 1;
        }
    }
    /// <summary>
    /// Message sent from client to worker to signal that a like should be 
    /// added to a given user message.
    /// </summary>
    public class Like : ClientMessage
    {
        /// <summary>
        /// the user message id (UserMessage.MessageId) of the message
        /// to be liked
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// the name of the person who likes the message
        /// </summary>
        public string ClientName { get; set; }
        public Like(string clientName, long communicationId, long mId)
        {
            this.ClientName = clientName;
            this.CommunicationId = communicationId;
            this.MessageId = mId;
        }
        public override int Duration()
        {
            return 1;
        }
    }
    /// <summary>
    /// Message sent from client to worker to signal that a dislike should be 
    /// added to a given user message.
    /// </summary>
    public class Dislike : ClientMessage
    {
        /// <summary>
        /// the user message id (UserMessage.MessageId) of the message
        /// to be disliked
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// the name of the person who dislikes the message
        /// </summary>
        public string ClientName { get; set; }
        public Dislike(string clientName, long communicationId, long mId)
        {
            this.ClientName = clientName;
            this.CommunicationId = communicationId;
            this.MessageId = mId;
        }
        public override int Duration()
        {
            return 1;
        }
    }
    /// <summary>
    /// Reply message base class sent from worker to client to show 
    /// that a request succeeded or failed.
    /// </summary>
    public abstract class Reply : ClientMessage
    {
        public override int Duration()
        {
            return 1;
        }
    }
    /// <summary>
    /// Reply message sent from worker to client to that a request succeeded.
    /// </summary>
    public class OperationAck : Reply
    {
        public OperationAck(long communicationId)
        {
            this.CommunicationId = communicationId;
        }
    }

    /// <summary>
    /// Reply message sent from worker to client to that a request failed.
    /// </summary>
    public class OperationFailed : Reply
    {
        public OperationFailed(long communicationId)
        {
            this.CommunicationId = communicationId;
        }
    }
}
