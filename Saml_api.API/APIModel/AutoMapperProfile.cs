using AutoMapper;
using System;
using Saml.API.Helper;
using Saml.API.Models;

namespace Saml.API.APIModel
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<UserSettings, APIUserSetting>()
            //      .ReverseMap();

            CreateMap<APIUserMaster, UserMaster>()
                  .ReverseMap();

            //CreateMap<APIUserMaster, APIUserSignUp>().ReverseMap();
            //CreateMap<APIVFSUserSignUp, UserMaster>()
            //   .ReverseMap();
            //CreateMap<APIUserMaster, APIVFSUserSignUp>().ReverseMap();
            CreateMap<APIUserMaster, UserMasterDetails>()
                .ForMember(x => x.UserMasterId, y => y.Ignore())
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(details => details.ConfigurationColumn1, api => api.MapFrom(u => u.ConfigurationColumn1Id))
                .ForMember(details => details.ConfigurationColumn2, api => api.MapFrom(u => u.ConfigurationColumn2Id))
                .ForMember(details => details.ConfigurationColumn3, api => api.MapFrom(u => u.ConfigurationColumn3Id))
                .ForMember(details => details.ConfigurationColumn4, api => api.MapFrom(u => u.ConfigurationColumn4Id))
                .ForMember(details => details.ConfigurationColumn5, api => api.MapFrom(u => u.ConfigurationColumn5Id))
                .ForMember(details => details.ConfigurationColumn6, api => api.MapFrom(u => u.ConfigurationColumn6Id))
                .ForMember(details => details.ConfigurationColumn7, api => api.MapFrom(u => u.ConfigurationColumn7Id))
                .ForMember(details => details.ConfigurationColumn8, api => api.MapFrom(u => u.ConfigurationColumn8Id))
                .ForMember(details => details.ConfigurationColumn9, api => api.MapFrom(u => u.ConfigurationColumn9Id))
                .ForMember(details => details.ConfigurationColumn10, api => api.MapFrom(u => u.ConfigurationColumn10Id))
                .ForMember(details => details.ConfigurationColumn11, api => api.MapFrom(u => u.ConfigurationColumn11Id))
                .ForMember(details => details.ConfigurationColumn12, api => api.MapFrom(u => u.ConfigurationColumn12Id))
                .ForMember(details => details.ConfigurationColumn13, api => api.MapFrom(u => u.ConfigurationColumn13Id))
                .ForMember(details => details.ConfigurationColumn14, api => api.MapFrom(u => u.ConfigurationColumn14Id))
                .ForMember(details => details.ConfigurationColumn15, api => api.MapFrom(u => u.ConfigurationColumn15Id))

                 .ReverseMap();
            CreateMap<APIUserSignUp, UserMasterDetails>()
                .ForMember(x => x.UserMasterId, y => y.Ignore())
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(details => details.ConfigurationColumn1, api => api.MapFrom(u => u.ConfigurationColumn1Id))
                .ForMember(details => details.ConfigurationColumn2, api => api.MapFrom(u => u.ConfigurationColumn2Id))
                .ForMember(details => details.ConfigurationColumn3, api => api.MapFrom(u => u.ConfigurationColumn3Id))
                .ForMember(details => details.ConfigurationColumn4, api => api.MapFrom(u => u.ConfigurationColumn4Id))
                .ForMember(details => details.ConfigurationColumn5, api => api.MapFrom(u => u.ConfigurationColumn5Id))
                .ForMember(details => details.ConfigurationColumn6, api => api.MapFrom(u => u.ConfigurationColumn6Id))
                .ForMember(details => details.ConfigurationColumn7, api => api.MapFrom(u => u.ConfigurationColumn7Id))
                .ForMember(details => details.ConfigurationColumn8, api => api.MapFrom(u => u.ConfigurationColumn8Id))
                .ForMember(details => details.ConfigurationColumn9, api => api.MapFrom(u => u.ConfigurationColumn9Id))
                .ForMember(details => details.ConfigurationColumn10, api => api.MapFrom(u => u.ConfigurationColumn10Id))
                .ForMember(details => details.ConfigurationColumn11, api => api.MapFrom(u => u.ConfigurationColumn11Id))
                .ForMember(details => details.ConfigurationColumn12, api => api.MapFrom(u => u.ConfigurationColumn12Id))

                 .ReverseMap();

            //CreateMap<APIVFSUserSignUp, UserMasterDetails>()
            //     .ForMember(x => x.UserMasterId, y => y.Ignore())
            //     .ForMember(x => x.Id, y => y.Ignore())
            //     .ForMember(details => details.ConfigurationColumn1, api => api.MapFrom(u => u.ConfigurationColumn1Id))
            //     .ForMember(details => details.ConfigurationColumn2, api => api.MapFrom(u => u.ConfigurationColumn2Id))
            //     .ForMember(details => details.ConfigurationColumn3, api => api.MapFrom(u => u.ConfigurationColumn3Id))
            //     .ForMember(details => details.ConfigurationColumn4, api => api.MapFrom(u => u.ConfigurationColumn4Id))
            //     .ForMember(details => details.ConfigurationColumn5, api => api.MapFrom(u => u.ConfigurationColumn5Id))
            //     .ForMember(details => details.ConfigurationColumn6, api => api.MapFrom(u => u.ConfigurationColumn6Id))
            //     .ForMember(details => details.ConfigurationColumn7, api => api.MapFrom(u => u.ConfigurationColumn7Id))
            //     .ForMember(details => details.ConfigurationColumn8, api => api.MapFrom(u => u.ConfigurationColumn8Id))
            //     .ForMember(details => details.ConfigurationColumn9, api => api.MapFrom(u => u.ConfigurationColumn9Id))
            //     .ForMember(details => details.ConfigurationColumn10, api => api.MapFrom(u => u.ConfigurationColumn10Id))
            //     .ForMember(details => details.ConfigurationColumn11, api => api.MapFrom(u => u.ConfigurationColumn11Id))
            //     .ForMember(details => details.ConfigurationColumn12, api => api.MapFrom(u => u.ConfigurationColumn12Id))

            //      .ReverseMap();
            //CreateMap<APINodalUserSignUp, APINodalUser>()
            //   .ForMember(dest => dest.ConfigurationColumn12Id, api => api.MapFrom(u => u.AirPortId))
            //   .ForMember(dest => dest.UserId, api => api.MapFrom(u => u.MobileNumber))
            //   .ForMember(dest => dest.OrganizationCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
            //   .ForMember(dest => dest.CustomerCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
            //   .ReverseMap();
            //CreateMap<APITTUserSignUp, APITTUser>()
            //    .ForMember(dest => dest.UserName, api => api.MapFrom(u => u.Firstname + " " + u.Lastname))
            //    .ForMember(dest => dest.UserId, api => api.MapFrom(u => u.MobileNumber))
            //    .ForMember(dest => dest.OrganizationCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
            //    .ForMember(dest => dest.CustomerCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
            //    .ReverseMap();
            CreateMap<APITTUser, UserMasterDetails>()
               .ForMember(x => x.UserMasterId, y => y.Ignore())
               .ForMember(x => x.Id, y => y.Ignore())
               .ForMember(details => details.ConfigurationColumn1, api => api.MapFrom(u => u.ConfigurationColumn1Id))
               .ForMember(details => details.ConfigurationColumn11, api => api.MapFrom(u => u.ConfigurationColumn11Id))
               .ForMember(details => details.ConfigurationColumn12, api => api.MapFrom(u => u.ConfigurationColumn12Id))
               .ForMember(details => details.AadharNumber, api => api.MapFrom(u => Security.Encrypt(u.AadharNumber)))
               .ReverseMap();
            CreateMap<APINodalUser, UserMasterDetails>()
                .ForMember(x => x.UserMasterId, y => y.Ignore())
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(details => details.ConfigurationColumn7, api => api.MapFrom(u => u.ConfigurationColumn7Id))
                .ForMember(details => details.ConfigurationColumn10, api => api.MapFrom(u => u.ConfigurationColumn10Id))
                .ForMember(details => details.ConfigurationColumn11, api => api.MapFrom(u => u.ConfigurationColumn11Id))
                .ForMember(details => details.ConfigurationColumn12, api => api.MapFrom(u => u.ConfigurationColumn12Id))
                .ForMember(details => details.AadharNumber, api => api.MapFrom(u => Security.Encrypt(u.AadharNumber)))
                .ReverseMap();
            CreateMap<APINodalUserInfo, APINodalUser>()
                .ForMember(dest => dest.ConfigurationColumn12Id, api => api.MapFrom(u => u.AirPortId))
                .ForMember(dest => dest.UserId, api => api.MapFrom(u => u.MobileNumber))
                .ForMember(dest => dest.DateOfBirth, api => api.MapFrom(u => u.DateOfBirth1))
                .ForMember(dest => dest.UserName, api => api.MapFrom(u => u.FullName))
                .ForMember(dest => dest.FHName, api => api.MapFrom(u => u.FatherHusbandName))
                .ReverseMap();
            //CreateMap<PayproceesResponseClass, PaymentResponse>()
            //    .ForMember(dest => dest.udf1, api => api.MapFrom(u => Security.Decrypt(u.udf1)))
            //    .ForMember(dest => dest.udf2, api => api.MapFrom(u => Security.Decrypt(u.udf2)))
            //    .ForMember(dest => dest.udf3, api => api.MapFrom(u => Security.Decrypt(u.udf3)))
            //    .ForMember(dest => dest.udf4, api => api.MapFrom(u => Security.Decrypt(u.udf4)))
            //    .ForMember(dest => dest.udf5, api => api.MapFrom(u => Security.Decrypt(u.udf5)))
            //    .ReverseMap();
            //CreateMap<APIGroupAdminSignUp, APINodalUser>()
            //    .ForMember(dest => dest.ConfigurationColumn12Id, api => api.MapFrom(u => u.AirPortId))
            //    .ForMember(dest => dest.OrganizationCode, api => api.MapFrom(u => u.OrgCode))
            //    .ReverseMap();
            CreateMap<APIDhangyanUserSignUp, APIDhangyanUser>()
                .ForMember(dest => dest.UserName, api => api.MapFrom(u => !string.IsNullOrEmpty(u.MiddleName) ? u.FirstName + " " + u.MiddleName + " " + u.LastName : u.FirstName + " " + u.LastName))
                .ForMember(dest => dest.ConfigurationColumn1Id, api => api.MapFrom(u => u.StateId))
                .ForMember(dest => dest.ConfigurationColumn2, api => api.MapFrom(u => !string.IsNullOrEmpty(u.Organization) ? u.OrganizationType + "-" + u.Organization : u.OrganizationType))
                .ForMember(dest => dest.OrganizationCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
                .ForMember(dest => dest.CustomerCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
                .ReverseMap();
            CreateMap<APIDhangyanUserSignUp, APIDhangyanUserSignUpResponse>()
                .ReverseMap();
            CreateMap<APISchoolDhangyanSignUp, APIDhangyanUser>()
               .ForMember(dest => dest.UserName, api => api.MapFrom(u => !string.IsNullOrEmpty(u.MiddleName) ? u.FirstName + " " + u.MiddleName + " " + u.LastName : u.FirstName + " " + u.LastName))
               .ForMember(dest => dest.ConfigurationColumn10, api => api.MapFrom(u => !string.IsNullOrEmpty(u.school_name) ? u.school_name : u.school_name))
               .ForMember(dest => dest.ConfigurationColumn11, api => api.MapFrom(u => !string.IsNullOrEmpty(u.class_name) ? u.class_name : u.class_name))
               .ForMember(dest => dest.ConfigurationColumn12, api => api.MapFrom(u => !string.IsNullOrEmpty(u.registration) ? u.registration : u.registration))
               .ForMember(dest => dest.OrganizationCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
               .ForMember(dest => dest.CustomerCode, api => api.MapFrom(u => Security.DecryptForUI(u.OrgCode)))
               .ReverseMap();
            CreateMap<APISchoolDhangyanSignUp, APIDhangyanUserSignUpResponse>()
               .ReverseMap();
        }

    }
}
