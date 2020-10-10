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
        private readonly IDictionary<string, Delegate> _dataPackFactories = new Dictionary<string, Delegate>();
        private readonly string _dataPackSuffix = "DataPack";

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
                if(ctorParamType is null)
                    continue;

                var ctorInfo = dataPackType.GetConstructor(new[] { ctorParamType });
                var returnType = typeof(IDataPack);

                var parameter = Expression.Parameter(ctorParamType);
                var funcType = Expression.GetFuncType(ctorParamType, returnType);
                var lambda = Expression.Lambda(funcType, Expression.New(ctorInfo, parameter), parameter);
                var dataPackCreator = lambda.Compile();

                _dataPackFactories.Add(TypeName(dataPackType), dataPackCreator);
            }
        }

        private string TypeName(Type type)
        {
            return type.Name.Replace(_dataPackSuffix, "");
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

            var del = _dataPackFactories[typeof(TType).Name] as Func<TType, IDataPack>;
            return del.Invoke(data);
        }

        private bool HasNoData<TType>(TType data)
        {
            return data == null || (data is string s && string.IsNullOrEmpty(s));
        }

        private bool HasNoDataPackPreciselyForThisType(Type type)
        {
            return !_dataPackFactories.ContainsKey(type.Name);
        }
    }
}