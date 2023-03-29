using FuelMeasurement.Data.Repositories.Repositories.Interfaces;
using FuelMeasurement.Model.Models.GeometryModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelMeasurement.Data.Repositories.Repositories.Implementations
{
    public class GeometryRepository : IGeometryRepository
    {
        protected Dictionary<string, FuelTankGeometryModel> Geometry;
        private readonly object _lock = new object();

        public GeometryRepository()
        {
            Geometry = new();
        }

        public bool AddTankGeometry(string id, FuelTankGeometryModel geometry)
        {
            lock(_lock)
            {
                if (!Geometry.TryGetValue(id, out var findGeometry))
                {
                    Geometry.Add(id, geometry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void RemoveAllGeometry()
        {
            lock (_lock)
            {
                Geometry.Clear();
            }
        }

        public bool RemoveGeometryById(string id)
        {
            lock (_lock)
            {
                return Geometry.Remove(id);
            }
        }

        public bool TryGetGeometryById(string id, out FuelTankGeometryModel geometry)
        {
            lock (_lock)
            {
                if (Geometry.TryGetValue(id, out geometry))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerable<FuelTankGeometryModel> GetAllGeometry()
        {
            lock (_lock)
            {
                foreach (var geometry in Geometry.Values)
                {
                    yield return geometry;
                }
            }
        }
    }
}
