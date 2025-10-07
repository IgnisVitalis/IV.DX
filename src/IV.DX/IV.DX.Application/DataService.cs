using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application
{
    internal class DataService : IDXUnitDataService
    {
        private readonly IDXCoreRepository _coreRepo;

        public DataService(IDXCoreRepository coreRepo)
        {
            this._coreRepo = coreRepo;
        }

        public T GetItem<T>(Guid id, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            var modelDefinition = DXModelConverter.GetESQLModelDefinition<T>();

            var esqlModel = this.GetItem(modelDefinition, id, context, typeOfLoading);

            var esqlObject = DXUnitHelper.CreateInstance<T>(esqlModel);

            return esqlObject;
        }

        public IEnumerable<T> GetItems<T>(DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            var modelDefinition = DXModelConverter.GetESQLModelDefinition<T>();

            var result = this.GetItems(modelDefinition, context, typeOfLoading).Select(x => DXUnitHelper.CreateInstance<T>(x));

            return result;
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            var modelDefinition = DXModelConverter.GetESQLModelDefinition<T>();

            var result = this.GetItems(modelDefinition, ids, context, typeOfLoading).Select(x => DXUnitHelper.CreateInstance<T>(x));

            return result;
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            var modelDefinition = DXModelConverter.GetESQLModelDefinition<T>();

            var result = this.GetItems(modelDefinition, esqlWhereExpression, context, typeOfLoading).Select(x => DXUnitHelper.CreateInstance<T>(x));

            return result;
        }

        public Guid Insert(DXUnit esqlObject, DXUnitHandlerBaseContext context)
        {
            var handler = EntityHandlerProvider.GetHandler(esqlObject);

            var result = handler.OnInserting(esqlObject, context);

            handler.OnInserted(esqlObject, context);

            return result;
        }

        public Guid InsertOrUpdate(DXUnit esqlObject, DXUnitHandlerBaseContext context)
        {
            var typeName = AttributeReader.GetESQLObjectTypeName(esqlObject.GetType());

            var itemIsExisting = this._coreRepo.IsItemExisting(typeName, esqlObject.ID);

            if (itemIsExisting)
            {
                return this.Update(esqlObject, context);
            }
            else
            {
                return this.Insert(esqlObject, context);
            }
        }

        public Guid Update(DXUnit esqlObject, DXUnitHandlerBaseContext context)
        {
            var handler = EntityHandlerProvider.GetHandler(esqlObject);

            var result = handler.OnUpdating(esqlObject, context);

            handler.OnUpdated(esqlObject, context);

            return result;
        }

        public bool Delete(string typeName, Guid id)
        {
            return this.Delete(typeName, id, new DXUnitHandlerBaseContext());
        }

        public bool Delete(DXUnit esqlObject)
        {
            return this.Delete(esqlObject, new DXUnitHandlerBaseContext());
        }

        public Guid Insert(string json, DXUnitHandlerBaseContext context)
        {
            var jObject = JObject.Parse(json);
           
            return this.Insert(jObject, context);
        }

        public Guid Update(string json, DXUnitHandlerBaseContext context)
        {
            var jObject = JObject.Parse(json);

            return this.Update(jObject, context);
        }

        public bool Delete(string typeName, Guid id, DXUnitHandlerBaseContext context)
        {
            bool result;

            if (EntityHandlerProvider.IsCustomHandlerExisting(typeName))
            {
                var handler = EntityHandlerProvider.GetHandler(typeName);

                result = handler.OnDeleting(id, context);

                handler.OnDeleted(id, context);
            }
            else
            {
                result = EntityHandlerProvider.CoreModelHandler.OnDeleting(typeName, id, context);

                EntityHandlerProvider.CoreModelHandler.OnDeleted(typeName, id, context);
            }

            return result;
        }

        public Guid InsertOrUpdate(string json, DXUnitHandlerBaseContext context)
        {
            var jObject = JObject.Parse(json);

            return this.InsertOrUpdate(jObject, context);
        }

        public bool IsItemExisting(Guid id, string type, DXUnitHandlerBaseContext context)
        {
            var entityType = type;

            if (EntityHandlerProvider.IsCustomHandlerExisting(entityType))
            {
                var handler = EntityHandlerProvider.GetHandler(entityType);

                return handler.IsItemExisting(id, context);
            }
            else
            {
                return EntityHandlerProvider.CoreModelHandler.IsItemExisting(entityType, id, context);
            }
        }

        public IEnumerable<T> GetItems<T>(DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            return this.GetItems<T>(new DXUnitHandlerBaseContext());
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            return this.GetItems<T>(ids, new DXUnitHandlerBaseContext());
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            return this.GetItems<T>(esqlWhereExpression, new DXUnitHandlerBaseContext());
        }

        public T GetItem<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new()
        {
            return this.GetItem<T>(id, new DXUnitHandlerBaseContext());
        }

        public bool IsItemExisting(Guid id, string type)
        {
            return this.IsItemExisting(id, type, new DXUnitHandlerBaseContext());
        }

        public Guid Insert(DXUnit esqlObject)
        {
            return this.Insert(esqlObject, new DXUnitHandlerBaseContext());
        }

        public Guid Update(DXUnit esqlObject)
        {
            return this.Update(esqlObject, new DXUnitHandlerBaseContext());
        }

        public Guid InsertOrUpdate(DXUnit esqlObject)
        {
            return this.InsertOrUpdate(esqlObject, new DXUnitHandlerBaseContext());
        }

        public Guid Insert(string jObject)
        {
            return this.Insert(jObject, new DXUnitHandlerBaseContext());
        }

        public Guid Update(string jObject)
        {
            return this.Update(jObject, new DXUnitHandlerBaseContext());
        }

        public Guid InsertOrUpdate(string jObject)
        {
            return this.InsertOrUpdate(jObject, new DXUnitHandlerBaseContext());
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, DXUnitHandlerBaseContext context)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(typeName);

            this.HandleItems(items, typeName, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition modelDefinition, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(modelDefinition, typeOfLoading);

            this.HandleItems(items, modelDefinition.OwnSingleItem.Type, context);

            return items;
        }

        private void HandleItems(IEnumerable<ESQLModel> items, string typeName, DXUnitHandlerBaseContext context)
        {
            if (EntityHandlerProvider.IsCustomHandlerExisting(typeName))
            {
                var handler = EntityHandlerProvider.GetHandler(typeName);

                foreach (var item in items)
                {
                    handler.OnGetting(item, context);
                }
            }
            else
            {
                foreach (var item in items)
                {
                    EntityHandlerProvider.CoreModelHandler.OnGetting(item, context);
                }
            }
        }

        private void HandleItem(ESQLModel item, string typeName, DXUnitHandlerBaseContext context)
        {
            if (EntityHandlerProvider.IsCustomHandlerExisting(typeName))
            {
                var handler = EntityHandlerProvider.GetHandler(typeName);

                handler.OnGetting(item, context);
            }
            else
            {
                EntityHandlerProvider.CoreModelHandler.OnGetting(item, context);
            }
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition modelDefinition, IEnumerable<Guid> ids, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(modelDefinition, ids, typeOfLoading);

            this.HandleItems(items, modelDefinition.OwnSingleItem.Type, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, IEnumerable<Guid> ids, DXUnitHandlerBaseContext context)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(typeName, ids);

            this.HandleItems(items, typeName, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, string esqlWhereExpression, DXUnitHandlerBaseContext context)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(typeName, esqlWhereExpression);

            this.HandleItems(items, typeName, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition modelDefinition, string esqlWhereExpression, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(modelDefinition, esqlWhereExpression, typeOfLoading);

            this.HandleItems(items, modelDefinition.OwnSingleItem.Type, context);

            return items;
        }

        public ESQLModel GetItem(string typeName, Guid id, DXUnitHandlerBaseContext context)
        {
            ESQLModel item = this._coreRepo.GetItem(typeName, id);

            this.HandleItem(item, typeName, context);

            return item;
        }

        public ESQLModel GetItem(ESQLModelDefinition modelDefinition, Guid id, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full)
        {
            ESQLModel item = this._coreRepo.GetItem(modelDefinition, id, typeOfLoading);

            this.HandleItem(item, modelDefinition.OwnSingleItem.Type, context);

            return item;
        }

        public Guid Insert(JObject jObject)
        {
            return this.Insert(jObject, new DXUnitHandlerBaseContext());
        }

        public Guid Update(JObject jObject)
        {
            return this.Update(jObject, new DXUnitHandlerBaseContext());
        }

        public bool Delete(JObject jObject)
        {
            return this.Delete(jObject, new DXUnitHandlerBaseContext());
        }

        public Guid InsertOrUpdate(JObject jObject)
        {
            return this.InsertOrUpdate(jObject, new DXUnitHandlerBaseContext());
        }

        public bool Delete(DXUnit esqlObject, DXUnitHandlerBaseContext context)
        {
            return this.Delete(esqlObject.GetTypeName(), esqlObject.ID, context);
        }

        public Guid Insert(JObject jObject, DXUnitHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);

            return this.Insert(esqlModel, context);
        }

        private Guid Insert(ESQLModel esqlModel, DXUnitHandlerBaseContext context)
        {            
            var entityType = esqlModel.OwnSingleItem.ObjectInfo.ObjectName;

            Guid result;

            if (EntityHandlerProvider.IsCustomHandlerExisting(entityType))
            {
                var handlerType = EntityHandlerProvider.GetHandlerType(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);
                var handler = EntityHandlerProvider.GetHandler(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);

                var obj = DXUnitHelper.CreateInstance(esqlModel, handlerType);

                result = handler.OnInserting(obj, context);

                handler.OnInserted(obj, context);
            }
            else
            {
                result = EntityHandlerProvider.CoreModelHandler.OnInserting(esqlModel, context);

                EntityHandlerProvider.CoreModelHandler.OnInserted(esqlModel, context);
            }

            return result;
        }

        public Guid Update(JObject jObject, DXUnitHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);
            return this.Update(esqlModel, context);
        }

        private Guid Update(ESQLModel esqlModel, DXUnitHandlerBaseContext context)
        {           
            var entityType = esqlModel.OwnSingleItem.ObjectInfo.ObjectName;

            Guid result;

            if (EntityHandlerProvider.IsCustomHandlerExisting(entityType))
            {
                var handlerType = EntityHandlerProvider.GetHandlerType(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);
                var handler = EntityHandlerProvider.GetHandler(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);

                var obj = DXUnitHelper.CreateInstance(esqlModel, handlerType);

                result = handler.OnUpdating(obj, context);

                handler.OnUpdated(obj, context);
            }
            else
            {
                result = EntityHandlerProvider.CoreModelHandler.OnUpdating(esqlModel, context);

                EntityHandlerProvider.CoreModelHandler.OnUpdated(esqlModel, context);
            }

            return result;
        }

        public bool Delete(JObject jObject, DXUnitHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);
            
            return this.Delete(esqlModel.OwnSingleItem.ObjectInfo.ObjectName, esqlModel.OwnSingleItem.Item.ID.Value, context);
        }

        public Guid InsertOrUpdate(JObject jObject, DXUnitHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);

            var objId = esqlModel.OwnSingleItem.Item.ID;

            if (objId.HasValue
                && this.IsItemExisting(objId.Value, esqlModel.OwnSingleItem.ObjectInfo.ObjectName, context))
            {
                return this.Update(esqlModel, context);
            }
            else
            {
                return this.Insert(esqlModel, context);
            }
        }
    }
}