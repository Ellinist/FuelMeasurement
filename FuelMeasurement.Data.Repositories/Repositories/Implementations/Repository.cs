using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelMeasurement.Data.Repositories.Repositories.Implementations
{
    public class Repository : IRepository
    {
        private ProjectModelDTO _currentProject;

        public Repository()
        {

        }

        public void SetCurrentProject(ProjectModelDTO currentProject)
        {
            _currentProject = currentProject;
        }

        public ProjectModelDTO GetCurrentProject()
        {
            return _currentProject;
        }
    }
}
