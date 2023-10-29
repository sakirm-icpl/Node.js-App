using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;
using log4net;
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/TrainingPlace")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class TrainingPlaceController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TrainingPlaceController));
        ITrainingPlaceRepository _trainingPlaceRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public TrainingPlaceController(ITrainingPlaceRepository trainingPlaceRepository, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _trainingPlaceRepository = trainingPlaceRepository;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> Get()
        {
            try
            {
                if (_trainingPlaceRepository.Count() == 0)
                {
                    return NotFound();
                }
                return Ok(await _trainingPlaceRepository.GetAll(s => s.IsDeleted == false));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.ilttrainingplacelist + " " + Permissions.iltshedules)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                if (await _trainingPlaceRepository.Get(id) == null)
                {
                    return NotFound();
                }
                return Ok(await _trainingPlaceRepository.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetTrainingPlaceDetails")]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> Get([FromBody] APITrainingPlaceDetails objAPITrainingPlaceDetails)
        {
            try
            {
                List<TrainingPlace> trainingPlace = await _trainingPlaceRepository.Get(objAPITrainingPlaceDetails.page, objAPITrainingPlaceDetails.pageSize, objAPITrainingPlaceDetails.search, objAPITrainingPlaceDetails.columnName);
                return Ok(trainingPlace);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("getTotalRecords")]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> GetCount([FromBody] APITrainingPlaceDetails objAPITrainingPlaceDetails)
        {
            try
            {
                int count = await _trainingPlaceRepository.count(objAPITrainingPlaceDetails.search, objAPITrainingPlaceDetails.columnName);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TrainingPlaceTypeAhead/{search?}")]
        public async Task<IActionResult> GetTrainingPlaceTypeAhead(string search = null)
        {
            try
            {
                List<TrainingPlaceTypeAhead> TrainingPlaceTypeAhead = await _trainingPlaceRepository.GetTrainingPlaceTypeAhead(search);
                return Ok(TrainingPlaceTypeAhead);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> Post([FromBody] TrainingPlace trainingPlace)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    //get userid from token
                    string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                    if (identity == null)
                    {
                        return StatusCode(401, Record.InvalidUserID);
                    }

                    int tokenId = Convert.ToInt32(identity);
                    bool valid1 = false;

                    if (valid1 == true)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    if (await _trainingPlaceRepository.NameCodeExists(trainingPlace.PlaceName))
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                    if (Convert.ToDouble(trainingPlace.AccommodationCapacity) > 10000)
                        return StatusCode(416, "Seat capacity must be less than equal to 10000.");

                    if (String.Equals(OrganisationCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (trainingPlace.PlaceType == "Internal")
                        {
                            ApiResponse obj = await _trainingPlaceRepository.CheckForValidInfo(trainingPlace.UserID, trainingPlace.ContactNumber, trainingPlace.ContactPerson);
                            if (obj.StatusCode == 409)
                                return StatusCode(409, "Invalid Contact Information");
                        }
                        TrainingPlace trainingplaceRecord = new TrainingPlace();
                        trainingplaceRecord.PlaceCode = Convert.ToString(await this._trainingPlaceRepository.GetTrainingPlaceCount() + 1); ;
                        trainingplaceRecord.Cityname = trainingPlace.Cityname;
                        trainingplaceRecord.PlaceName = trainingPlace.PlaceName;
                        trainingplaceRecord.PostalAddress = trainingPlace.PostalAddress;
                        trainingplaceRecord.TimeZone = trainingPlace.TimeZone;
                        trainingplaceRecord.AccommodationCapacity = trainingPlace.AccommodationCapacity;
                        trainingplaceRecord.Facilities = trainingPlace.Facilities;
                        trainingplaceRecord.PlaceType = trainingPlace.PlaceType;
                        trainingplaceRecord.UserID = trainingPlace.UserID;
                        trainingplaceRecord.ContactNumber = trainingPlace.ContactNumber;
                        trainingplaceRecord.AlternateContact = trainingPlace.AlternateContact;
                        trainingplaceRecord.ContactPerson = trainingPlace.ContactPerson;
                        trainingplaceRecord.IsActive = trainingPlace.IsActive;
                        trainingplaceRecord.Status = trainingPlace.Status;
                        trainingplaceRecord.CreatedDate = DateTime.UtcNow;
                        trainingplaceRecord.ModifiedDate = DateTime.UtcNow;
                        trainingplaceRecord.CreatedBy = tokenId;
                        await _trainingPlaceRepository.Add(trainingplaceRecord);
                        return Ok();
                    }
                    else
                    {
                        TrainingPlace trainingplaceRecord = new TrainingPlace();
                        trainingplaceRecord.PlaceCode = Convert.ToString(await this._trainingPlaceRepository.GetTrainingPlaceCount() + 1); ;
                        trainingplaceRecord.Cityname = trainingPlace.Cityname;
                        trainingplaceRecord.PlaceName = trainingPlace.PlaceName;
                        trainingplaceRecord.PostalAddress = trainingPlace.PostalAddress;
                        trainingplaceRecord.TimeZone = trainingPlace.TimeZone;
                        trainingplaceRecord.AccommodationCapacity = trainingPlace.AccommodationCapacity;
                        trainingplaceRecord.Facilities = trainingPlace.Facilities;
                        trainingplaceRecord.PlaceType = trainingPlace.PlaceType;
                        trainingplaceRecord.UserID = trainingPlace.UserID;
                        trainingplaceRecord.ContactNumber = trainingPlace.ContactNumber;
                        trainingplaceRecord.AlternateContact = trainingPlace.AlternateContact;
                        trainingplaceRecord.ContactPerson = trainingPlace.ContactPerson;
                        trainingplaceRecord.IsActive = trainingPlace.IsActive;
                        trainingplaceRecord.Status = trainingPlace.Status;
                        trainingplaceRecord.CreatedDate = DateTime.UtcNow;
                        trainingplaceRecord.ModifiedDate = DateTime.UtcNow;
                        trainingplaceRecord.CreatedBy = tokenId;
                        await _trainingPlaceRepository.Add(trainingplaceRecord);
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingPlace trainingPlace)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                TrainingPlace trainingplaceRecord = await _trainingPlaceRepository.Get(id);
                if (trainingplaceRecord == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                if (trainingplaceRecord.PlaceName != trainingPlace.PlaceName)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                if (Convert.ToDouble(trainingPlace.AccommodationCapacity) > 10000)
                    return StatusCode(416, "Seat capacity must be less than equal to 10000.");

                bool valid1 = false;

                if (FileValidation.CheckForSQLInjection(trainingPlace.PlaceName))
                    valid1 = true;
                else if (FileValidation.CheckForSQLInjection(trainingPlace.Cityname))
                    valid1 = true;
                else if (FileValidation.CheckForSQLInjection(trainingPlace.PostalAddress))
                    valid1 = true;
                else if (FileValidation.CheckForSQLInjection(trainingPlace.AccommodationCapacity))
                    valid1 = true;
                else if (FileValidation.CheckForSQLInjection(trainingPlace.ContactPerson))
                    valid1 = true;
                else if (FileValidation.CheckForSQLInjection(trainingPlace.ContactNumber))
                    valid1 = true;
                else if (FileValidation.CheckForSQLInjection(trainingPlace.AlternateContact))
                    valid1 = true;

                if (valid1 == true)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (String.Equals(OrganisationCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (trainingPlace.PlaceType == "Internal")
                    {
                        ApiResponse obj = await _trainingPlaceRepository.CheckForValidInfo(trainingPlace.UserID, trainingPlace.ContactNumber, trainingPlace.ContactPerson);
                        if (obj.StatusCode == 409)
                            return StatusCode(409, "Invalid Contact Information");
                    }
                }

                int result = await _trainingPlaceRepository.UpdateTrainingPlace(id, trainingPlace, UserId);
                return Ok();
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                TrainingPlace trainingPlace = await _trainingPlaceRepository.Get(DecryptedId);
                if (trainingPlace == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                int result = await _trainingPlaceRepository.DeleteTrainingPlace(DecryptedId, UserId);

                if (result == 0)
                {
                    return StatusCode(409, "Dependency Exists");
                }

                trainingPlace.IsDeleted = true;
                trainingPlace.ModifiedBy = UserId;

                await this._trainingPlaceRepository.Update(trainingPlace);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Exists")]
        [PermissionRequired(Permissions.ilttrainingplacelist)]
        public async Task<IActionResult> Exists(string q)
        {
            try
            {
                bool code = await _trainingPlaceRepository.NameCodeExists(q);

                if (code == true)
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}