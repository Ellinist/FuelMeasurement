using FuelMeasurement.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Models
{
    public class TankGroupModel : ModelBase
    {
        public IEnumerable<string> FuelTanksInGroup { get; set; }
        public override DTOObjectType Type => DTOObjectType.TankGroup;
        public TankGroupModel()
        {

        }
    }
}
