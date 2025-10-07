using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Enums;
using IV.DX.Contracts.Common.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Contracts.Application
{
    public interface IDataService
    {
        IEnumerable<T> GetItems<T>(TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        T GetItem<T>(Guid id, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        bool IsItemExisting(Guid id, string type);

        Guid Insert(ESQLObject esqlObject);
        Guid Update(ESQLObject esqlObject);
        bool Delete(ESQLObject esqlObject);
        Guid InsertOrUpdate(ESQLObject esqlObject);

        Guid Insert(string jObject);
        Guid Update(string jObject);
        bool Delete(string typeName, Guid id);
        Guid InsertOrUpdate(string jObject);

        Guid Insert(JObject jObject);
        Guid Update(JObject jObject);
        bool Delete(JObject jObject);
        Guid InsertOrUpdate(JObject jObject);

        IEnumerable<T> GetItems<T>(EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        T GetItem<T>(Guid id, EntityHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        bool IsItemExisting(Guid id, string type, EntityHandlerBaseContext context);

        Guid Insert(ESQLObject esqlObject, EntityHandlerBaseContext context);
        Guid Update(ESQLObject esqlObject, EntityHandlerBaseContext context);
        Guid InsertOrUpdate(ESQLObject esqlObject, EntityHandlerBaseContext context);
        bool Delete(ESQLObject esqlObject, EntityHandlerBaseContext context);

        IEnumerable<ESQLModel> GetItems(string typeName, EntityHandlerBaseContext context);
        IEnumerable<ESQLModel> GetItems(string typeName, IEnumerable<Guid> ids, EntityHandlerBaseContext context);
        IEnumerable<ESQLModel> GetItems(string typeName, string esqlWhereExpression, EntityHandlerBaseContext context);
        ESQLModel GetItem(string typeName, Guid id, EntityHandlerBaseContext context);

        Guid Insert(string jObject, EntityHandlerBaseContext context);
        Guid Update(string jObject, EntityHandlerBaseContext context);
        bool Delete(string typeName, Guid id, EntityHandlerBaseContext context);
        Guid InsertOrUpdate(string jObject, EntityHandlerBaseContext context);

        Guid Insert(JObject jObject, EntityHandlerBaseContext context);
        Guid Update(JObject jObject, EntityHandlerBaseContext context);
        bool Delete(JObject jObject, EntityHandlerBaseContext context);
        Guid InsertOrUpdate(JObject jObject, EntityHandlerBaseContext context);
    }
}