using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBoard
{
    /// <summary>
    /// Exception class used to signal that a worker does 
    /// not know a client given the communication ID
    /// </summary>
    public class UnknownClientException : Exception
    {
       /// <summary>
       /// Contructs a new UnknownClientException object. 
       /// </summary>
       /// <param name="message">message corresponding to the exception</param>
        public UnknownClientException(string message) : base(message)
        {

        }
    }
}
