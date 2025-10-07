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
    internal class DataService : IDataService
    {
        private readonly ICoreRepository _coreRepo;

        public DataService(ICoreRepository coreRepo)
        {
            this._coreRepo = coreRepo;
        }

        public T GetItem<T>(Guid id, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            var modelDefinition = ModelConverter.GetESQLModelDefinition<T>();

            var esqlModel = this.GetItem(modelDefinition, id, context, typeOfLoading);

            var esqlObject = ESQLObjectHelper.CreateInstance<T>(esqlModel);

            return esqlObject;
        }

        public IEnumerable<T> GetItems<T>(EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            var modelDefinition = ModelConverter.GetESQLModelDefinition<T>();

            var result = this.GetItems(modelDefinition, context, typeOfLoading).Select(x => ESQLObjectHelper.CreateInstance<T>(x));

            return result;
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            var modelDefinition = ModelConverter.GetESQLModelDefinition<T>();

            var result = this.GetItems(modelDefinition, ids, context, typeOfLoading).Select(x => ESQLObjectHelper.CreateInstance<T>(x));

            return result;
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            var modelDefinition = ModelConverter.GetESQLModelDefinition<T>();

            var result = this.GetItems(modelDefinition, esqlWhereExpression, context, typeOfLoading).Select(x => ESQLObjectHelper.CreateInstance<T>(x));

            return result;
        }

        public Guid Insert(ESQLObject esqlObject, EntityHandlerBaseContext context)
        {
            var handler = EntityHandlerProvider.GetHandler(esqlObject);

            var result = handler.OnInserting(esqlObject, context);

            handler.OnInserted(esqlObject, context);

            return result;
        }

        public Guid InsertOrUpdate(ESQLObject esqlObject, EntityHandlerBaseContext context)
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

        public Guid Update(ESQLObject esqlObject, EntityHandlerBaseContext context)
        {
            var handler = EntityHandlerProvider.GetHandler(esqlObject);

            var result = handler.OnUpdating(esqlObject, context);

            handler.OnUpdated(esqlObject, context);

            return result;
        }

        public bool Delete(string typeName, Guid id)
        {
            return this.Delete(typeName, id, new EntityHandlerBaseContext());
        }

        public bool Delete(ESQLObject esqlObject)
        {
            return this.Delete(esqlObject, new EntityHandlerBaseContext());
        }

        public Guid Insert(string json, EntityHandlerBaseContext context)
        {
            var jObject = JObject.Parse(json);
           
            return this.Insert(jObject, context);
        }

        public Guid Update(string json, EntityHandlerBaseContext context)
        {
            var jObject = JObject.Parse(json);

            return this.Update(jObject, context);
        }

        public bool Delete(string typeName, Guid id, EntityHandlerBaseContext context)
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

        public Guid InsertOrUpdate(string json, EntityHandlerBaseContext context)
        {
            var jObject = JObject.Parse(json);

            return this.InsertOrUpdate(jObject, context);
        }

        public bool IsItemExisting(Guid id, string type, EntityHandlerBaseContext context)
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

        public IEnumerable<T> GetItems<T>(TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            return this.GetItems<T>(new EntityHandlerBaseContext());
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            return this.GetItems<T>(ids, new EntityHandlerBaseContext());
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            return this.GetItems<T>(esqlWhereExpression, new EntityHandlerBaseContext());
        }

        public T GetItem<T>(Guid id, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new()
        {
            return this.GetItem<T>(id, new EntityHandlerBaseContext());
        }

        public bool IsItemExisting(Guid id, string type)
        {
            return this.IsItemExisting(id, type, new EntityHandlerBaseContext());
        }

        public Guid Insert(ESQLObject esqlObject)
        {
            return this.Insert(esqlObject, new EntityHandlerBaseContext());
        }

        public Guid Update(ESQLObject esqlObject)
        {
            return this.Update(esqlObject, new EntityHandlerBaseContext());
        }

        public Guid InsertOrUpdate(ESQLObject esqlObject)
        {
            return this.InsertOrUpdate(esqlObject, new EntityHandlerBaseContext());
        }

        public Guid Insert(string jObject)
        {
            return this.Insert(jObject, new EntityHandlerBaseContext());
        }

        public Guid Update(string jObject)
        {
            return this.Update(jObject, new EntityHandlerBaseContext());
        }

        public Guid InsertOrUpdate(string jObject)
        {
            return this.InsertOrUpdate(jObject, new EntityHandlerBaseContext());
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, EntityHandlerBaseContext context)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(typeName);

            this.HandleItems(items, typeName, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition modelDefinition, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(modelDefinition, typeOfLoading);

            this.HandleItems(items, modelDefinition.OwnSingleItem.Type, context);

            return items;
        }

        private void HandleItems(IEnumerable<ESQLModel> items, string typeName, EntityHandlerBaseContext context)
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

        private void HandleItem(ESQLModel item, string typeName, EntityHandlerBaseContext context)
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

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition modelDefinition, IEnumerable<Guid> ids, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(modelDefinition, ids, typeOfLoading);

            this.HandleItems(items, modelDefinition.OwnSingleItem.Type, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, IEnumerable<Guid> ids, EntityHandlerBaseContext context)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(typeName, ids);

            this.HandleItems(items, typeName, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(string typeName, string esqlWhereExpression, EntityHandlerBaseContext context)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(typeName, esqlWhereExpression);

            this.HandleItems(items, typeName, context);

            return items;
        }

        public IEnumerable<ESQLModel> GetItems(ESQLModelDefinition modelDefinition, string esqlWhereExpression, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full)
        {
            IEnumerable<ESQLModel> items = this._coreRepo.GetItems(modelDefinition, esqlWhereExpression, typeOfLoading);

            this.HandleItems(items, modelDefinition.OwnSingleItem.Type, context);

            return items;
        }

        public ESQLModel GetItem(string typeName, Guid id, EntityHandlerBaseContext context)
        {
            ESQLModel item = this._coreRepo.GetItem(typeName, id);

            this.HandleItem(item, typeName, context);

            return item;
        }

        public ESQLModel GetItem(ESQLModelDefinition modelDefinition, Guid id, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full)
        {
            ESQLModel item = this._coreRepo.GetItem(modelDefinition, id, typeOfLoading);

            this.HandleItem(item, modelDefinition.OwnSingleItem.Type, context);

            return item;
        }

        public Guid Insert(JObject jObject)
        {
            return this.Insert(jObject, new EntityHandlerBaseContext());
        }

        public Guid Update(JObject jObject)
        {
            return this.Update(jObject, new EntityHandlerBaseContext());
        }

        public bool Delete(JObject jObject)
        {
            return this.Delete(jObject, new EntityHandlerBaseContext());
        }

        public Guid InsertOrUpdate(JObject jObject)
        {
            return this.InsertOrUpdate(jObject, new EntityHandlerBaseContext());
        }

        public bool Delete(ESQLObject esqlObject, EntityHandlerBaseContext context)
        {
            return this.Delete(esqlObject.GetTypeName(), esqlObject.ID, context);
        }

        public Guid Insert(JObject jObject, EntityHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);

            return this.Insert(esqlModel, context);
        }

        private Guid Insert(ESQLModel esqlModel, EntityHandlerBaseContext context)
        {            
            var entityType = esqlModel.OwnSingleItem.ObjectInfo.ObjectName;

            Guid result;

            if (EntityHandlerProvider.IsCustomHandlerExisting(entityType))
            {
                var handlerType = EntityHandlerProvider.GetHandlerType(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);
                var handler = EntityHandlerProvider.GetHandler(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);

                var obj = ESQLObjectHelper.CreateInstance(esqlModel, handlerType);

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

        public Guid Update(JObject jObject, EntityHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);
            return this.Update(esqlModel, context);
        }

        private Guid Update(ESQLModel esqlModel, EntityHandlerBaseContext context)
        {           
            var entityType = esqlModel.OwnSingleItem.ObjectInfo.ObjectName;

            Guid result;

            if (EntityHandlerProvider.IsCustomHandlerExisting(entityType))
            {
                var handlerType = EntityHandlerProvider.GetHandlerType(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);
                var handler = EntityHandlerProvider.GetHandler(esqlModel.OwnSingleItem.ObjectInfo.ObjectName);

                var obj = ESQLObjectHelper.CreateInstance(esqlModel, handlerType);

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

        public bool Delete(JObject jObject, EntityHandlerBaseContext context)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);
            
            return this.Delete(esqlModel.OwnSingleItem.ObjectInfo.ObjectName, esqlModel.OwnSingleItem.Item.ID.Value, context);
        }

        public Guid InsertOrUpdate(JObject jObject, EntityHandlerBaseContext context)
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