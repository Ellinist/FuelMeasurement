using FuelMeasurement.Common.Enums;
using FuelMeasurement.Tools.CalculationData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class BranchModel : ModelBase
    {
        public DateTime Creation { get; set; }
        public DateTime? Updated { get; set; }
        public IEnumerable<FuelTankModel> FuelTanks { get; set; }
        public IEnumerable<TankGroupModel> TankInGroups { get; set; }
        public IEnumerable<TankGroupModel> TankOutGroups { get; set; }
        public BranchConfigurationModel Configuration { get; set; }
        public BranchType BranchType { get; set; }
        public override DTOObjectType Type => DTOObjectType.Branch;
        public BranchModel()
        {

        }
    }
}
