using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Cygnus.Nlp
{
    public class ConditionalExpression
    {
        public ConditionalConstructType ConstructType { get; set; }
        public BooleanCondition Condition { get; set; }
        public Consequent Consequent { get; set; }

        public ConditionalExpression()
        {
            Condition = new BooleanCondition();
            Consequent = new Consequent();
        }

        public void SetConstructType(string s)
        {
            var typeCandidate = s.ToLowerInvariant();
            if (IsValidConstructType(typeCandidate))
            {
                ConstructType = ConstructTypeMap()[typeCandidate];
            }
        }

        public void CoercePredicateActions()
        {
            Consequent.CoercePredicateAction();
            Condition.CoercePredicateAction();
        }

        public virtual bool IsValid()
        {
            var valid = this.ConstructType != ConditionalConstructType.Unknown &&
                    this.Condition.IsValid() &&
                    this.Consequent.Predicate.IsValid();
            return valid;
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
            return ConstructTypeMap().ContainsKey(s);
        }
        #endregion
    }

    public class Consequent
    {
        public Predicate Predicate { get; set; }
        public List<string> ObjectKeywords { get; set; }

        public Consequent()
        {
            this.Predicate = new Predicate();
            this.ObjectKeywords = new List<string>();
        }

        public void CoercePredicateAction()
        {
            this.Predicate.ResetActionType();
        }
    }

    public class BooleanCondition
    {
        public ConditionalOperatorType OperatorType { get; set; }
        public List<string> ObjectKeywords { get; set; }
        public Predicate Predicate { get; set; }

        public BooleanCondition()
        {
            OperatorType = ConditionalOperatorType.Unknown;
            ObjectKeywords = new List<string>();
            Predicate = new Predicate();
        }

        public void SetOperatorType(string s)
        {
            var typeCandidate = s.ToLowerInvariant();
            if (IsValidOperatorType(typeCandidate))
            {
                OperatorType = OperatorTypeMap()[typeCandidate];
            }
        }

        public void CoerceOperatorFromPredicate()
        {
            if (this.Predicate == null) { throw new NullReferenceException("Predicate was null"); }
            this.SetOperatorType(Predicate.Governor);
        }

        public void CoercePredicateAction()
        {
            this.Predicate.ResetActionType();
        }

        public bool IsValid()
        {
            return OperatorType != ConditionalOperatorType.Unknown;
        }

        #region static methods
        public static Dictionary<string, ConditionalOperatorType> OperatorTypeMap()
        {
            var map = new Dictionary<string, ConditionalOperatorType>()
            {
                { "equal", ConditionalOperatorType.Equal },
                { "same", ConditionalOperatorType.Equal },
                { "identical", ConditionalOperatorType.Equal },
                { "is", ConditionalOperatorType.Equal },

                { "lessthan", ConditionalOperatorType.LessThan },
                { "under", ConditionalOperatorType.LessThan },
                { "below", ConditionalOperatorType.LessThan },
                { "less", ConditionalOperatorType.LessThan },
                { "fewer", ConditionalOperatorType.LessThan },

                { "greaterthan", ConditionalOperatorType.GreaterThan },
                { "over", ConditionalOperatorType.GreaterThan },
                { "above", ConditionalOperatorType.GreaterThan },
                { "more", ConditionalOperatorType.GreaterThan },
                { "greater", ConditionalOperatorType.GreaterThan }
            };
            return map;
        }

        private static Dictionary<ConditionalOperatorType, Func<IComparable, IComparable, bool>> ExpressionActionMap()
        {
            var map = new Dictionary<ConditionalOperatorType, Func<IComparable, IComparable, bool>>()
            {
                { ConditionalOperatorType.Unknown, (x,y) => false },
                { ConditionalOperatorType.Equal, (x,y) => x.Equals(y) },
                { ConditionalOperatorType.GreaterThan, (x,y) => x.CompareTo(y) > 0 },
                { ConditionalOperatorType.LessThan, (x,y) => x.CompareTo(y) < 0 }
            };
            return map;
        }

        public static bool IsValidOperatorType(string s)
        {
            return OperatorTypeMap().ContainsKey(s);
        }

        public bool Evaluate(string arg)
        {
            bool result = false;
            decimal dArg = 0;
            bool bArg = false;
            var handler = ExpressionActionMap()[this.OperatorType];
            if (Decimal.TryParse(arg, out dArg))
            {
                decimal dPred;
                if (Decimal.TryParse(this.Predicate.Dependent, out dPred))
                {
                    result = handler(dArg, dPred);
                }
            }
            else if (Boolean.TryParse(arg, out bArg))
            {
                bool bPred = false;
                if (Boolean.TryParse(this.Predicate.Dependent, out bPred))
                {
                    result = handler(bPred, bArg);
                }
            }
            return result;
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