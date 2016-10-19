using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AsyncDolls.Expressions
{
    public class Chain
    {
        readonly ILinkElement[] executingElements;
        private Func<IncomingContext, Task> pipeline;

        public Chain(ILinkElement[] elements)
        {
            executingElements = elements;

            pipeline = executingElements.CreatePipelineExecutionFuncFor<IncomingContext>();
        }

        public Task Invoke(IncomingContext context)
        {
            return pipeline(context);
        }
    }

    static class PipelineExecutionExtensions
    {
        private static Type LinkElementInterfaceType = typeof(ILinkElement<,>);

        internal static Type GetLinkElementInterfaceType(this Type type)
        {
            return type.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == LinkElementInterfaceType);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public static Func<TRootContext, Task> CreatePipelineExecutionFuncFor<TRootContext>(this ILinkElement[] behaviors)
            where TRootContext : Context
        {
            return (Func<TRootContext, Task>)behaviors.CreatePipelineExecutionExpression();
        }

        /// <code>
        /// rootContext
        ///    => behavior1.Invoke(rootContext,
        ///       context1 => behavior2.Invoke(context1,
        ///        ...
        ///          context{N} => behavior{N}.Invoke(context{N},
        ///             context{N+1} => TaskEx.Completed))
        /// </code>
        public static Delegate CreatePipelineExecutionExpression(this ILinkElement[] links, List<Expression> expressions = null)
        {
            Delegate lambdaExpression = null;
            var linkCount = links.Length - 1;
            // We start from the end of the list know the lambda expressions deeper in the call stack in advance
            for (var i = linkCount; i >= 0; i--)
            {
                var currentLink = links[i];
                var linkElementInterfaceType = currentLink.GetType().GetLinkElementInterfaceType();
                if (linkElementInterfaceType == null)
                {
                    throw new InvalidOperationException("Links must implement ILinkElement<TInContext, TOutContext>");
                }
                // Select the method on the type which was implemented from the link interface.
                var methodInfo = currentLink.GetType().GetInterfaceMap(linkElementInterfaceType).TargetMethods.FirstOrDefault();
                if (methodInfo == null)
                {
                    throw new InvalidOperationException("Links must implement ILinkElement<TInContext, TOutContext> and provide an invocation method.");
                }

                var genericArguments = linkElementInterfaceType.GetGenericArguments();
                var inContextType = genericArguments[0];

                var inContextParameter = Expression.Parameter(inContextType, $"context{i}");

                if (i == linkCount)
                {
                    var doneDelegate = CreateDoneDelegate(inContextType, i);
                    lambdaExpression = CreateLinkCallDelegate(currentLink, methodInfo, inContextParameter, doneDelegate, expressions);
                    continue;
                }

                lambdaExpression = CreateLinkCallDelegate(currentLink, methodInfo, inContextParameter, lambdaExpression, expressions);
            }

            return lambdaExpression;
        }

        // ReSharper disable once SuggestBaseTypeForParameter

        /// <code>
        /// context{i} => link.Invoke(context{i}, context{i+1} => previous)
        /// </code>>
        static Delegate CreateLinkCallDelegate(ILinkElement currentBehavior, MethodInfo methodInfo, ParameterExpression outerContextParam, Delegate previous, List<Expression> expressions = null)
        {
            Expression body = Expression.Call(Expression.Constant(currentBehavior), methodInfo, outerContextParam, Expression.Constant(previous));
            var lambdaExpression = Expression.Lambda(body, outerContextParam);
            expressions?.Add(lambdaExpression);
            return lambdaExpression.Compile();
        }

        /// <code>
        /// context{i} => return Task.CompletedTask;
        /// </code>>
        static Delegate CreateDoneDelegate(Type inContextType, int i)
        {
            var innerContextParam = Expression.Parameter(inContextType, $"context{i + 1}");
            return Expression.Lambda(typeof(Func<,>).MakeGenericType(inContextType, typeof(Task)), Expression.Constant(Task.CompletedTask), innerContextParam).Compile();
        }
    }
}