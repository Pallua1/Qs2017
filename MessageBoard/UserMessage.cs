using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBoard
{
    /// <summary>
    /// This class represents actual messages posted by users (do not confuse 
    /// with message passed between actors).
    /// </summary>
    public class UserMessage
    {
        /// <summary>
        /// ID for new messages
        /// </summary>
        public const long NEW = -1;

        /// <summary>
        /// Constructs a new UserMessage object.
        /// </summary>
        /// <param name="author">author of the message</param>
        /// <param name="message">posted message string</param>
        public UserMessage(string author, string message)
        {
            this.Author = author;
            this.Message = message;
            this.Likes = new List<string>();
            this.Dislikes = new List<string>();
            this.MessageId = NEW;
        }
        /// <summary>
        /// the author of the message
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// the message posted by the author
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// likes for the message (initially empty):
        /// The strings in the list
        /// are names of people who like the message.
        /// </summary>
        public List<string> Likes { get; set; }
        /// <summary>
        /// dislikes for the message (initially empty):
        /// The strings in the list
        /// are names of people who dislike the message.
        /// </summary>
        public List<string> Dislikes { get; set; }
        // invariant, only NEW and positive IDs are used 
        /// <summary>
        /// ID of the message to be able to refer to it.
        /// Only positive numbers and <c>UserMessage.NEW</c>
        /// are allowed as IDs.
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// Newly added toString()-method, which returns a string representation 
        /// of user messages.
        /// </summary>
        /// <returns>string-representation of <c>this</c></returns>
        public override string ToString()
        {
            return Author + ":" + Message + " liked by :" + String.Join(",", Likes) + " disliked by :" + String.Join(",", Dislikes);
        }
    }
}
