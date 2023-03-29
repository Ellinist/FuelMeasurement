using FuelMeasurement.Common.Enums;
using FuelMeasurement.Tools.CalculationData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class FuelTankModel : ModelBase
    {
        public IEnumerable<SensorModel> Sensors { get; set; }
        public string GeometryFilePath { get; set; }
        public TankGroupModel TankGroupIn { get; set; }
        public TankGroupModel TankGroupOut { get; set; }
        public override DTOObjectType Type => DTOObjectType.FuelTank;
        public FuelTankModel()
        {

        }
    }
}
