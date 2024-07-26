using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Utility.Extend.DelegateTool
{
    public static class DelegateTool
    {
        public static Action<object[]> CreateDelegate(MethodInfo method, object target)
        {
            var parameter = Expression.Parameter(typeof(object[]), "args");
            //参数表达式
            var methodParameters = method.GetParameters();
            var arguments = new Expression[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var index = Expression.Constant(i);
                var parameterType = methodParameters[i].ParameterType;
                var parameterAccessor = Expression.ArrayIndex(parameter, index);
                var parameterCast = Expression.Convert(parameterAccessor, parameterType);
                arguments[i] = parameterCast;
            }

            //实例表达式
            var instance = Expression.Constant(target);
            //方法调用表达式
            var methodCall = Expression.Call(instance, method, arguments);
            //创建并编译lambda表达式
            var lambda = Expression.Lambda<Action<object[]>>(methodCall, parameter);
            return lambda.Compile();
        }
    }
}