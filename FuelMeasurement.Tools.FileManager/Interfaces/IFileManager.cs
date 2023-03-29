using FuelMeasurement.Model.DTO.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Tools.FileManager.Interfaces
{
    public interface IFileManager
    {
        /// <summary>
        /// Сохранение проекта в архив 
        /// </summary>
        /// <param name="project">Проект</param>
        bool SaveCurrentProject(ProjectModelDTO project);

        /// <summary>
        /// Загрузка проекта из архива
        /// </summary>
        /// <returns>Проект, список ошибок</returns>
        public (ProjectModelDTO, List<string>) LoadProjectZIP();

        /// <summary>
        /// Загрузка проекта из XML
        /// </summary>
        /// <returns>Проект, список ошибо</returns>
        public (ProjectModelDTO, List<string>) LoadProjectXML();
    }
}
