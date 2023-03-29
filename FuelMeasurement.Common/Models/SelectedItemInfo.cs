using FuelMeasurement.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Common.Models
{
    public  class SelectedItemInfo
    {
        public string Id { get; set; }
        public DTOObjectType Type { get; set; }

        public SelectedItemInfo(string id, DTOObjectType type)
        {
            Type = type;
            Id = id;
        }
    }
}
