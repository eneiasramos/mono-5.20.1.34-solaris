//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Linq.Expressions;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotMatchKeywords,
        Justification = "Optimizing for XAML naming. VB imperative users will [] qualify (e.g. New [And])")]
    public sealed class And<TLeft, TRight, TResult> : CodeActivity<TResult>
    {
        //Lock is not needed for operationFunction here. The reason is that delegates for a given And<TLeft, TRight, TResult> are the same.
        //It's possible that 2 threads are assigning the operationFucntion at the same time. But it's okay because the compiled codes are the same.
        static Func<TLeft, TRight, TResult> operationFunction;

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TLeft> Left
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TRight> Right
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            BinaryExpressionHelper.OnGetArguments(metadata, this.Left, this.Right);

            if (operationFunction == null)
            {
                ValidationError validationError;
                if (!BinaryExpressionHelper.TryGenerateLinqDelegate(ExpressionType.And, out operationFunction, out validationError))
                {
                    metadata.AddValidationError(validationError);
                }
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            Fx.Assert(operationFunction != null, "OperationFunction must exist.");
            TLeft leftValue = this.Left.Get(context);
            TRight rightValue = this.Right.Get(context);
            return operationFunction(leftValue, rightValue);
        }
    }
}
