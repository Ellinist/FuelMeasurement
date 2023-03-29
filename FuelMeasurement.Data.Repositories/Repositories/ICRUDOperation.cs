using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelMeasurement.Data.Repositories.Repositories
{
    public interface ICRUDOperation<T, Tdata>
    {
        Task<T> Create(Tdata data);
        Task<T> Get(string id);
        Task<IEnumerable<T>> GetAll();
        Task<bool> RemoveById(string id);
        Task<bool> Remove(T model);
        Task Update(string id, Tdata data);
        Task Update(T model);
    }
}
