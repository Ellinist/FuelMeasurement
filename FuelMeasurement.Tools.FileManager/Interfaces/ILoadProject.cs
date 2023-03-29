using FuelMeasurement.Model.DTO.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.FileManager.Interfaces
{
    public interface ILoadProject
    {
        (ProjectModelDTO, List<string>) Load(string filePath);
    }
}
