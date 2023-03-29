using FuelMeasurement.Model.DTO.Models.ProjectModels;
using FuelMeasurement.Tools.FileManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FuelMeasurement.Tools.FileManager.Implementations
{
    public class XMLManager : IXMLManager
    {
        public XMLManager()
        {

        }

        public (ProjectModelDTO, List<string>) Load(string filePath)
        {
            ProjectModelDTO model = null;
            List<string> errors = new ();
            using var stream = File.OpenRead(filePath);
            XmlSerializer deserializer = new(typeof(ProjectModelDTO));
            
            try 
            {
                var deserializeModel = deserializer.Deserialize(stream);

                if(deserializeModel is ProjectModelDTO projectModel)
                {
                    model = (ProjectModelDTO)deserializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
            }

            stream.Dispose();
            return (model, errors);
        }

        public (bool, List<string>) Save(ProjectModelDTO model, string filePath)
        {
            bool result = true;
            List<string> errors = new();
            XmlSerializer serializer = new(typeof(ProjectModelDTO));
            using var stream = File.Create(filePath);
            try
            {
                serializer.Serialize(stream, model);
            }
            catch (Exception e)
            {
                result = false;
                errors.Add(e.Message);
            }

            stream.Dispose();
            return (result, errors);
        }
    }
}
