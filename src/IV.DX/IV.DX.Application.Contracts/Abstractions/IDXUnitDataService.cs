using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataService
    {

        Task<T> InsertAsync<T>(T esqlObject, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> UpdateAsync<T>(T esqlObject, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> InsertOrUpdateAsync<T>(T esqlObject, CancellationToken ct = default) where T : DXUnit, new();
        Task<bool> DeleteAsync<T>(T esqlObject, CancellationToken ct = default) where T : DXUnit, new();


        Task<T> InsertAsync<T>(T esqlObject, IDXHandlerContext context, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> UpdateAsync<T>(T esqlObject, IDXHandlerContext context, CancellationToken ct = default) where T : DXUnit, new();
        Task<T> InsertOrUpdateAsync<T>(T esqlObject, IDXHandlerContext context, CancellationToken ct = default) where T : DXUnit, new();
        Task<bool> DeleteAsync<T>(T esqlObject, IDXHandlerContext context, CancellationToken ct = default) where T : DXUnit, new();

        T GetItem<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();



        IEnumerable<T> GetItems<T>(DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
       
        bool IsItemExisting(Guid id, string type);





    

        Guid Insert(string jObject);
        Guid Update(string jObject);
        bool Delete(string typeName, Guid id);
        Guid InsertOrUpdate(string jObject);

        Guid Insert(JObject jObject);
        Guid Update(JObject jObject);
        bool Delete(JObject jObject);
        Guid InsertOrUpdate(JObject jObject);

        IEnumerable<T> GetItems<T>(IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        T GetItem<T>(Guid id, IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        bool IsItemExisting(Guid id, string type, IDXHandlerContext context);

      
     
    
      
     

        IEnumerable<DXModel> GetItems(string typeName, IDXHandlerContext context);
        IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> ids, IDXHandlerContext context);
        IEnumerable<DXModel> GetItems(string typeName, string esqlWhereExpression, IDXHandlerContext context);
        DXModel GetItem(string typeName, Guid id, IDXHandlerContext context);

        Guid Insert(string jObject, IDXHandlerContext context);
        Guid Update(string jObject, IDXHandlerContext context);
        bool Delete(string typeName, Guid id, IDXHandlerContext context);
        Guid InsertOrUpdate(string jObject, IDXHandlerContext context);

        Guid Insert(JObject jObject, IDXHandlerContext context);
        Guid Update(JObject jObject, IDXHandlerContext context);
        bool Delete(JObject jObject, IDXHandlerContext context);
        Guid InsertOrUpdate(JObject jObject, IDXHandlerContext context);
    }
}