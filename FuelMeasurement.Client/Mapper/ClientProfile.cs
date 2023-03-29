using AutoMapper;
using FuelMeasurement.Client.Models;
using FuelMeasurement.Common.Enums;
using FuelMeasurement.Model.DTO.Models.AirplaneModels;
using FuelMeasurement.Model.DTO.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelMeasurement.Client.Mapper
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<SensorModelDTO, SensorModel>();

            CreateMap<TankGroupModelDTO, TankGroupModel>()
                .ForMember(x => x.Id, x => x.MapFrom(src => src.Id))
                .ForMember(x => x.Name, x => x.MapFrom(src => src.Name))
                .ForMember(x => x.FuelTanksInGroup, x => x.MapFrom(src => src.FuelTanksInGroup))
                ;

            CreateMap<FuelTankModelDTO, FuelTankModel>()
                .ForMember(x => x.Id, x => x.MapFrom(src => src.Id))
                .ForMember(x => x.Name, x => x.MapFrom(src => src.Name))
                .ForMember(x => x.GeometryFilePath, x => x.MapFrom(src => src.GeometryFilePath))
                .ForMember(x => x.TankGroupIn, x => x.MapFrom(src => src.TankGroupIn))
                .ForMember(x => x.TankGroupOut, x => x.MapFrom(src => src.TankGroupOut))
                ;

            CreateMap<InsideModelFuelTankModelDTO, InsideModelFuelTank>()
                .ForMember(x => x.Id, x => x.MapFrom(src => src.Id))
                .ForMember(x => x.Name, x => x.MapFrom(src => src.Name))
                .ForMember(x => x.GeometryFilePath, x => x.MapFrom(src => src.GeometryFilePath))
                ;

            CreateMap<ConfigurationModelDTO, ConfigurationModel>()
                .ForMember(x => x.Id, x => x.MapFrom(src => src.Id))
                .ForMember(x => x.Name, x => x.MapFrom(src => src.Name))
                .ForMember(x => x.FuelTanks, x => x.MapFrom(src => src.FuelTanks))
                .ForMember(x => x.Branches, x => x.MapFrom(src => src.Branches))
                .ForMember(x => x.InsideModelFuelTanks, x => x.MapFrom(src => src.InsideModelFuelTanks))
                .ForMember(x => x.Type, x => x.MapFrom(src => src.Type))
                ;

            CreateMap<BranchConfigurationModelDTO, BranchConfigurationModel>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Name, x => x.Ignore())
                .ForMember(x => x.MinPitch, x => x.MapFrom(src => src.MinPitch))
                .ForMember(x => x.MaxPitch, x => x.MapFrom(src => src.MaxPitch))
                .ForMember(x => x.MinRoll, x => x.MapFrom(src => src.MinRoll))
                .ForMember(x => x.MaxRoll, x => x.MapFrom(src => src.MaxRoll))
                .ForMember(x => x.PitchStep, x => x.MapFrom(src => src.PitchStep))
                .ForMember(x => x.RollStep, x => x.MapFrom(src => src.RollStep))
                .ForMember(x => x.NodesQuantity, x => x.MapFrom(src => src.NodesQuantity))
                .ForMember(x => x.ReferencedPitch, x => x.MapFrom(src => src.ReferencedPitch))
                .ForMember(x => x.ReferencedRoll, x => x.MapFrom(src => src.ReferencedRoll))
                .ForMember(x => x.Coefficient, x => x.MapFrom(src => src.Coefficient))
                .ForMember(x => x.LengthCoef, x => x.MapFrom(src => src.LengthCoef))
                .ForMember(x => x.DefaultMinIndent, x => x.MapFrom(src => src.DefaultMinIndent))
                .ForMember(x => x.DefaultUpIndent, x => x.MapFrom(src => src.DefaultUpIndent))
                .ForMember(x => x.DefaultDownIndent, x => x.MapFrom(src => src.DefaultDownIndent))
                .ForMember(x => x.VisibleSensorDiametr, x => x.MapFrom(src => src.VisibleSensorDiametr))
                .ForMember(x => x.SensorVisualDiametrType, x => x.MapFrom(src => src.SensorVisualDiametrType))
                ;

            CreateMap<BranchModelDTO, BranchModel>()
                .ForMember(x => x.Id, x => x.MapFrom(src => src.Id))
                .ForMember(x => x.Name, x => x.MapFrom(src => src.Name))
                .ForMember(x => x.Creation, x => x.MapFrom(src => src.Creation))
                .ForMember(x => x.Updated, x => x.Ignore())
                .ForMember(x => x.FuelTanks, x => x.MapFrom(src => src.FuelTanks))
                .ForMember(x => x.TankInGroups, x => x.MapFrom(src => src.TankInGroups))
                .ForMember(x => x.TankOutGroups, x => x.MapFrom(src => src.TankOutGroups))
                .ForMember(x => x.Configuration, x => x.MapFrom(src => src.Configuration))
                .ForMember(x => x.Type, x => x.MapFrom(src => src.Type))
                ;

            CreateMap<ProjectModelDTO, ProjectModel>()
                .ForMember(x => x.Id, x => x.MapFrom(src => src.Id))
                .ForMember(x => x.Name, x => x.MapFrom(src => src.Name))
                .ForMember(x => x.Configurations, x => x.MapFrom(src => src.Configurations))
                .ForMember(x => x.Creation, x => x.MapFrom(src => src.Creation))
                .ForMember(x => x.ProjectAuthor, x => x.MapFrom(src => src.ProjectAuthor))
                ;

            CreateMap<BranchConfigurationModelDTO, FuelMeasurement.Tools.CalculationData.Models.ConfigurationModel>();

            CreateMap<BranchModelDTO, FuelMeasurement.Tools.CalculationData.Models.BranchModel>()
                .ForMember(dest => dest.AnglesModel, act => act.MapFrom(src => src.Configuration));
        }
    }
}
