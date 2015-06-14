using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cygnus.Nlp
{
    public class GroupExpression
    {
        public IEnumerable<string> GroupKeywords { get; set; }
        public IEnumerable<string> ResourceKeywords { get; set; }
    }
}