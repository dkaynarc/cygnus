using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cygnus.Managers
{
    public class ConditionalExpression
    {
        public ConditionalConstructType ConstructType { get; set; }
        public BooleanCondition Condition { get; set; }
        public string Consequant { get; set; }

        public void SetConstructType(string s)
        {
            if (IsValidConstructType(s))
            {
                ConstructType = ConstructTypeMap()[s];
            }
        }

        #region static methods
        public static Dictionary<string, ConditionalConstructType> ConstructTypeMap()
        {
            var map = new Dictionary<string, ConditionalConstructType>()
            {
                { "if", ConditionalConstructType.If }
            };
            
            return map;
        }

        public static bool IsValidConstructType(string s)
        {
            return ConstructTypeMap().ContainsKey(s.ToLowerInvariant());
        }
        #endregion
    }

    public class BooleanCondition
    {
        public ConditionalOperatorType OperatorType { get; set; }
        public string Lhs { get; set; }
        public string Rhs { get; set; }

        public void SetOperatorType(string s)
        {
            if (IsValidOperatorType(s))
            {
                OperatorType = OperatorTypeMap()[s];
            }
        }

        #region static methods
        public static Dictionary<string, ConditionalOperatorType> OperatorTypeMap()
        {
            var map = new Dictionary<string, ConditionalOperatorType>()
            {
                { "equal", ConditionalOperatorType.Equal },
                { "same", ConditionalOperatorType.Equal },
                { "identical", ConditionalOperatorType.Equal },

                { "lessthan", ConditionalOperatorType.LessThan },
                { "under", ConditionalOperatorType.LessThan },
                { "below", ConditionalOperatorType.LessThan },
                { "less", ConditionalOperatorType.LessThan },

                { "greaterthan", ConditionalOperatorType.LessThan },
                { "over", ConditionalOperatorType.LessThan },
                { "above", ConditionalOperatorType.LessThan },
                { "more", ConditionalOperatorType.LessThan },
            };
            return map;
        }

        public static bool IsValidOperatorType(string s)
        {
            return OperatorTypeMap().ContainsKey(s.ToLowerInvariant());
        }
        #endregion
    }

    public enum ConditionalConstructType
    {
        Unknown, If
    }

    public enum ConditionalOperatorType
    {
        Unknown, LessThan, GreaterThan, Equal
    }
}