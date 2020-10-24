using Shield.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class ReflectionBasedDataPackFactory : IDataPackFactory
    {
        private readonly IDictionary<Type, Delegate> _dataPackFactories = new Dictionary<Type, Delegate>();

        public ReflectionBasedDataPackFactory()
        {
            CreateCompiledDataPackExpressions();
        }

        private void CreateCompiledDataPackExpressions()
        {
            foreach (var dataPackType in AllDataPackTypesFromAssembly())
            {
                if (dataPackType is null)
                    continue;

                var ctor = dataPackType.GetConstructors().First(ci => ci.GetParameters().Length == 1);
                var ctorParamType = ctor.GetParameters()[0].ParameterType;
                var returnType = typeof(IDataPack);

                var parameter = Expression.Parameter(ctorParamType);
                var funcType = Expression.GetFuncType(ctorParamType, returnType);
                var lambda = Expression.Lambda(funcType, Expression.New(ctor, parameter), parameter);
                var dataPackCreator = lambda.Compile();

                try
                {
                    _dataPackFactories.Add(ctorParamType, dataPackCreator);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"One or more DataPack classes handles the same type of input variable - {ctorParamType.Name}", e);
                }
            }
        }

        private IEnumerable<Type> AllDataPackTypesFromAssembly()
        {
            return Assembly
                .GetAssembly(typeof(IDataPack))
                .GetTypes()
                .Where(t =>
                    typeof(IDataPack).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    t.IsClass &&
                    t.GetConstructors().Any(ci => ci.GetParameters().Length == 1));
        }

        public IDataPack CreateFrom<TType>(TType data)
        {
            if (HasNoData(data))
                return EmptyDataPackSingleton.GetInstance();

            if (HasNoDataPackPreciselyForThisType(typeof(TType)))
                return new JsonDataPack(data);

            return CreateDataPack(data);
        }

        public IDataPack CreateFrom<TType>(params TType[] data)
        {
            if (HasNoData(data))
                return EmptyDataPackSingleton.GetInstance();

            if (HasNoDataPackPreciselyForThisType(typeof(TType[])))
                return new JsonDataPack(data);

            return CreateDataPack(data);
        }

        private bool HasNoData<TType>(TType data)
        {
            return data == null
                   || (data is string s && string.IsNullOrEmpty(s))
                   || (data is string[] arr && arr.All(string.IsNullOrEmpty))
                   || (data is TType[] array && array.IsNullOrEmpty());
        }

        private bool HasNoDataPackPreciselyForThisType(Type type)
        {
            return !_dataPackFactories.ContainsKey(type);
        }

        private IDataPack CreateDataPack<TType>(TType data)
        {
            var del = _dataPackFactories[typeof(TType)] as Func<TType, IDataPack>;
            return del?.Invoke(data) ?? throw new ArgumentOutOfRangeException(nameof(data),
                $"There is no DataPack for given data (type: {typeof(TType).Name}) and it cannot be pushed into {nameof(JsonDataPack)}.");
        }
    }
}