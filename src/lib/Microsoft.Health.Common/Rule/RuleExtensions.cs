// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;

namespace Microsoft.Health.Common.Rule
{
    /// <summary>
    /// Collection of extension methods for supporting dynamic creation for rules
    /// </summary>
    public static class RuleExtensions
    {
        /// <summary>
        /// Combines two rules with an And comparsion
        /// </summary>
        /// <typeparam name="T">Type to evaluate</typeparam>
        /// <param name="rule1">First rule to evaluate</param>
        /// <param name="rule2">Second rule to evaluate</param>
        /// <param name="shortCircuit">Default is true.  If true a short circuit And comparison will be used and rule2.IsTrue will not be evaluated if rule1.IsTrue returns false.</param>
        /// <returns>A composite rule</returns>
        public static IRule<T> And<T>(this IRule<T> rule1, IRule<T> rule2, bool shortCircuit = true)
        {
            return shortCircuit ?
                ToRule<T>(e => rule1.IsTrue(e) && rule2.IsTrue(e)) :
                ToRule<T>(e => rule1.IsTrue(e) & rule2.IsTrue(e));
        }

        /// <summary>
        /// Combines two rules with an Or comparsion
        /// </summary>
        /// <typeparam name="T">Type to evaluate</typeparam>
        /// <param name="rule1">First rule to evaluate</param>
        /// <param name="rule2">Second rule to evaluate</param>
        /// <param name="shortCircuit">Default is true.  If true a short circuit Or comparison will be used and rule2.IsTrue will not be evaluated if rule1.IsTrue returns true.</param>
        /// <returns>A composite rule</returns>
        public static IRule<T> Or<T>(this IRule<T> rule1, IRule<T> rule2, bool shortCircuit = true)
        {
            return shortCircuit ?
                ToRule<T>(e => rule1.IsTrue(e) || rule2.IsTrue(e)) :
                ToRule<T>(e => rule1.IsTrue(e) | rule2.IsTrue(e));
        }

        public static IRule<T> Not<T>(this IRule<T> rule)
        {
            return ToRule<T>(e => !rule.IsTrue(e));
        }

        /// <summary>
        /// Creates a conditional branch using one rule as the anchor.  The anchor rule result isn't returned instead used to direct the branch.
        /// </summary>
        /// <typeparam name="T">Type to evaluate</typeparam>
        /// <param name="rule">Anchor rule to determine how the branch is resolved</param>
        /// <param name="ifRule">The rule executed and result returned if the anchor rule is true</param>
        /// <param name="elseRule">The rule executed and result returned if the anchor rule is false</param>
        /// <returns>Result of the if or else rule depending on the anchor rule result</returns>
        public static IRule<T> IfThenElse<T>(this IRule<T> rule, IRule<T> ifRule, IRule<T> elseRule)
        {
            Func<T, bool> ifThenElse = e => rule.IsTrue(e) ? ifRule.IsTrue(e) : elseRule.IsTrue(e);
            return ifThenElse.ToRule();
        }

        public static IRule<T> ToRule<T>(this Func<T, bool> func)
        {
            EnsureArg.IsNotNull(func, nameof(func));
            return new FuncRule<T>(func);
        }

        private class FuncRule<T> : IRule<T>
        {
            private readonly Func<T, bool> _internalRule;

            public FuncRule(Func<T, bool> rule)
            {
                _internalRule = rule;
            }

            public bool IsTrue(T entity)
            {
                return _internalRule(entity);
            }
        }
    }
}
