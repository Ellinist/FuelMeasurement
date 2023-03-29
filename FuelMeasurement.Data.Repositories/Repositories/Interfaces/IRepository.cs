using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelMeasurement.Data.Repositories.Repositories.Interfaces
{
    public interface IRepository
    {
        void SetCurrentProject(ProjectModelDTO currentProject);
        ProjectModelDTO GetCurrentProject();
    }
}
