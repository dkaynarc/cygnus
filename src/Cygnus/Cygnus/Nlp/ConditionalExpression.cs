using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Cygnus.Nlp
{
    public class ConditionalExpression<TCond>
    {
        public ConditionalConstructType ConstructType { get; set; }
        public BooleanCondition<TCond> Condition { get; set; }
        public Consequant Consequant { get; set; }

        public ConditionalExpression()
        {
            Condition = new BooleanCondition<TCond>();
            Consequant = new Consequant();
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
            Consequant.CoercePredicateAction();
            Condition.CoercePredicateAction();
        }

        public virtual bool IsValid()
        {
            return 
            (
                this.ConstructType != ConditionalConstructType.Unknown &&
                this.Condition.IsValid() &&
                this.Consequant.Predicate.IsValid()
            );
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

    public class Consequant
    {
        public Predicate Predicate { get; set; }
        public List<string> ObjectKeywords { get; set; }

        public Consequant()
        {
            this.Predicate = new Predicate();
            this.ObjectKeywords = new List<string>();
        }

        public void CoercePredicateAction()
        {
            this.Predicate.ResetActionType();
        }
    }

    public class BooleanCondition<T>
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
            return OperatorType != ConditionalOperatorType.Unknown && this.Predicate.IsValid();
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

                { "greaterthan", ConditionalOperatorType.LessThan },
                { "over", ConditionalOperatorType.LessThan },
                { "above", ConditionalOperatorType.LessThan },
                { "more", ConditionalOperatorType.LessThan },
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

        public bool Result(object arg, Type t)
        {
            var convertedArg = Convert.ChangeType(arg, t, CultureInfo.InvariantCulture);
            var convertedDep = Convert.ChangeType(this.Predicate.Dependent, t, CultureInfo.InvariantCulture);

            return false;
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