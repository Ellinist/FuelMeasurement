using FuelMeasurement.Model.Models.GeometryModels;
using System;
using System.Collections.Generic;

namespace FuelMeasurement.Data.Repositories.Repositories.Interfaces
{
    public interface IGeometryRepository
    {
        bool AddTankGeometry(string id, FuelTankGeometryModel geometry);
        bool TryGetGeometryById(string id, out FuelTankGeometryModel geometry);
        bool RemoveGeometryById(string id);
        void RemoveAllGeometry();
        IEnumerable<FuelTankGeometryModel> GetAllGeometry();
    }
}
