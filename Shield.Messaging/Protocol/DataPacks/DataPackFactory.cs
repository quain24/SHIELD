using Shield.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class DataPackFactory
    {
        private readonly IDictionary<Type, Delegate> _dataPackFactories = new Dictionary<Type, Delegate>();

        public DataPackFactory()
        {
            CreateCompiledDataPackExpressions(); 
        }

        private void CreateCompiledDataPackExpressions()
        {
            foreach (var dataPackType in AllDataPackTypesFromAssembly())
            {
                var ctors = dataPackType?.GetConstructors();
                if (dataPackType is null || ctors.IsNullOrEmpty())
                    continue;

                var ctorParamType = ctors.First()?.GetParameters().First()?.ParameterType;
                if (ctorParamType is null)
                    continue;

                var ctorInfo = dataPackType.GetConstructor(new[] { ctorParamType });
                var returnType = typeof(IDataPack);

                var parameter = Expression.Parameter(ctorParamType);
                var funcType = Expression.GetFuncType(ctorParamType, returnType);
                var lambda = Expression.Lambda(funcType, Expression.New(ctorInfo, parameter), parameter);
                var dataPackCreator = lambda.Compile();

                _dataPackFactories.Add(ctorParamType, dataPackCreator);
            }
        }

        private IEnumerable<Type> AllDataPackTypesFromAssembly()
        {
            return Assembly
                .GetAssembly(typeof(IDataPack))
                .GetTypes()
                .Where(t =>
                    typeof(IDataPack).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
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