using FuelMeasurement.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class ConfigurationModel : ModelBase
    {
        public IEnumerable<FuelTankModel> FuelTanks { get; set; }
        public IEnumerable<BranchModel> Branches { get; set; }
        public IEnumerable<InsideModelFuelTank> InsideModelFuelTanks { get; set; }
        public ConfigurationType ConfigurationType { get; set; }
        public override DTOObjectType Type => DTOObjectType.Configuration;
        public ConfigurationModel()
        {

        }
    }
}
