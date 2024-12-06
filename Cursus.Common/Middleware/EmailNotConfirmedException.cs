using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Common.Middleware
{
    public class EmailNotConfirmedException : Exception
    {
        public EmailNotConfirmedException() : base("Email has not been confirmed.") { }

        public EmailNotConfirmedException(string message) : base(message) { }

        public EmailNotConfirmedException(string message, Exception inner) : base(message, inner) { }
    }
}
