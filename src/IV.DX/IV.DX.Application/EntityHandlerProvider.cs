using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.DataHandlers;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application
{
    internal class EntityHandlerProvider
    {
        public static Dictionary<string, Tuple<Type, Object>> Handlers
        {
            get
            {
                return _handlers;
            }
        }

        private static readonly Dictionary<string, Tuple<Type, Object>> _handlers;
        private static readonly Dictionary<string, Type> _modelTypes;
        private static Tuple<Type, Object> _baseHandler;
        private static bool IsInitCore;
        private static readonly bool IsInit;

        public static ICoreModelHandler CoreModelHandler { get; private set; }

        private static readonly object obj = new object();

        static EntityHandlerProvider()
        {
            _handlers = new Dictionary<string, Tuple<Type, Object>>();
            _modelTypes = new Dictionary<string, Type>();
        }

        public static void InitCore(IServiceProvider serviceProvider)
        {
            if (!IsInitCore)
            {
                _baseHandler =
                  Tuple.Create(
                       typeof(ESQLObject),
                      (object)(new BaseEntityHandler<ESQLObject>(serviceProvider)));

                Register<DXElementDefinitionUnit>(new DXElementDefinitionUnitHandler(serviceProvider));
                Register<DXUnitDefinitionUnit>(new DXUnitDefinitionUnitHandler(serviceProvider));
                Register<DXEnumDefinitionUnit>(new DXEnumDefinitionUnitHandler(serviceProvider));
                Register<DPRelationObject>(new DPRelationObjectHandler(serviceProvider));
                Register<DPInheritanceInitCore>(new DPInheritanceInitCoreHandler(serviceProvider));
                Register<DPRelationItemObject>(new DPRelationItemObjectHandler(serviceProvider));

                CoreModelHandler = serviceProvider.GetService<ICoreModelHandler>();

                IsInitCore = true;
            }
        }

        public static void Init()
        {
            if (!IsInitCore)
                throw new Exception("Please call InitCore method before Init");

            if (!IsInit)
            {

            }
        }

        public static void Register<T>(IEntityHandler<T> handler) where T : ESQLObject, new()
        {
            lock (obj)
            {
                var typeName = typeof(T).Name;// AttributeReader.GetESQLObjectTypeName(typeof(T));

                if (!_handlers.ContainsKey(typeName))
                {
                    _handlers.Add(typeName, Tuple.Create(typeof(T), (object)(handler)));
                }

                if (!_modelTypes.ContainsKey(typeName))
                {
                    _modelTypes.Add(typeName, typeof(T));
                }
            }
        }

        public static IEntityHandler<ESQLObject> GetHandler(string entityName)
        {
            Tuple<Type, object> existinghandler = null;

            if (IsCustomHandlerExisting(entityName))
            {
                existinghandler = _handlers[entityName];
            }
            else
            {
                existinghandler = _baseHandler;
            }

            return new CompositeEntityBehavior(existinghandler);
        }

        public static Type GetHandlerType(string entityName)
        {
            if (IsCustomHandlerExisting(entityName))
            {
                return _handlers[entityName].Item1;
            }
            else
            {
                return null;
            }
        }

        public static bool IsCustomHandlerExisting(string entityName)
        {
            return _handlers.ContainsKey(entityName);
        }

        public static IEntityHandler<ESQLObject> GetHandler(ESQLObject esqlObject)
        {
            var typeName = AttributeReader.GetESQLObjectTypeName(esqlObject.GetType());

            return GetHandler(typeName);
        }

        private class CompositeEntityBehavior : IEntityHandler<ESQLObject>
        {
            private readonly Tuple<Type, object> _handler;

            public CompositeEntityBehavior(Tuple<Type, object> handler)
            {
                this._handler = handler;
            }

            public bool IsItemExisting(Guid id, EntityHandlerBaseContext context)
            {
                return (this._handler.Item2 as dynamic).IsItemExisting(id, context);
            }

            public void OnDeleted(Guid id, EntityHandlerBaseContext context)
            {
                (this._handler.Item2 as dynamic).OnDeleted(id, context);
            }

            public bool OnDeleting(Guid id, EntityHandlerBaseContext context)
            {
                return (this._handler.Item2 as dynamic).OnDeleting(id, context);
            }

            public void OnGetting(ESQLModel model, EntityHandlerBaseContext context)
            {
                (this._handler.Item2 as dynamic).OnGetting(model, context);
            }

            public void OnInserted(ESQLObject entity, EntityHandlerBaseContext context)
            {
                (this._handler.Item2 as dynamic).OnInserted(VerifyRecord(this._handler.Item1, entity), context);
            }

            public Guid OnInserting(ESQLObject entity, EntityHandlerBaseContext context)
            {
                return (this._handler.Item2 as dynamic).OnInserting(VerifyRecord(this._handler.Item1, entity), context);
            }

            public void OnUpdated(ESQLObject entity, EntityHandlerBaseContext context)
            {
                (this._handler.Item2 as dynamic).OnUpdated(VerifyRecord(this._handler.Item1, entity), context);
            }

            public Guid OnUpdating(ESQLObject entity, EntityHandlerBaseContext context)
            {
                return (this._handler.Item2 as dynamic).OnUpdating(VerifyRecord(this._handler.Item1, entity), context);
            }

            private dynamic VerifyRecord(Type recordType, ESQLObject dbRecord)
            {
                if (!recordType.IsInstanceOfType(dbRecord))
                {
                    throw new ArgumentException(string.Format("Cannot cast record type {1} to handler type {0}", recordType.FullName,
                        dbRecord.GetType().FullName));
                }
                return dbRecord;
            }
        }
    }
}