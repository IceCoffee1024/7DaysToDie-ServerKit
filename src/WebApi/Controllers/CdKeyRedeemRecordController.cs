using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.Dtos;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Mapster;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Cd Key Redeem Record
    /// </summary>
    [Authorize]
    [RoutePrefix("api/CdKeyRedeemRecords")]
    public class CdKeyRedeemRecordsController : ApiController
    {
        private readonly ICdKeyRedeemRecordRepository _cdKeyRedeemRecordRepository;

        public CdKeyRedeemRecordsController(ICdKeyRedeemRecordRepository cdKeyRedeemRecordRepository)
        {
            _cdKeyRedeemRecordRepository = cdKeyRedeemRecordRepository;
        }

        /// <summary>
        /// Get all Cd Key Redeem Records.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<CdKeyRedeemRecordDto>> GetCdKeyRedeemRecords()
        {
            return (await _cdKeyRedeemRecordRepository.GetAllAsync()).Adapt<IEnumerable<CdKeyRedeemRecordDto>>();
        }

        /// <summary>
        /// Get a Cd Key Redeem Record by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(CdKeyRedeemRecordDto))]
        public async Task<IHttpActionResult> GetCdKeyRedeemRecord([FromUri] int id)
        {
            var entity = await _cdKeyRedeemRecordRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity.Adapt<CdKeyRedeemRecord>());
        }

        /// <summary>
        /// Creates a new Cd Key Redeem Record.
        /// </summary>
        /// <param name="dto">The Cd Key Redeem Record dto to create.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateCdKeyRedeemRecord([FromBody] CdKeyRedeemRecordCreateDto dto)
        {
            var entity = dto.Adapt<CdKeyRedeemRecord>();
            entity.CreatedAt = DateTime.Now;
            int id = await _cdKeyRedeemRecordRepository.InsertAsync<int>(entity);
            var location = new Uri(Request.RequestUri, $"api/CdKeyRedeemRecord/{id}");

            return Created(location, entity);
        }

        /// <summary>
        /// Updates an existing Cd Key Redeem Record.
        /// </summary>
        /// <param name="id">The ID of the Cd Key Redeem Record to update.</param>
        /// <param name="dto">The updated Cd Key Redeem Record dto.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> UpdateCdKeyRedeemRecord([FromUri] int id, [FromBody] CdKeyRedeemRecordUpdateDto dto)
        {
            var entity = await _cdKeyRedeemRecordRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            dto.Adapt(entity);
            await _cdKeyRedeemRecordRepository.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// Deletes a Cd Key Redeem Record by ID.
        /// </summary>
        /// <param name="id">The ID of the Cd Key Redeem Record to delete.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> DeleteCdKeyRedeemRecord([FromUri] int id)
        {
            int count = await _cdKeyRedeemRecordRepository.DeleteByIdAsync(id);
            if (count == 0)
            {
                return NotFound();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes multiple Cd Key Redeem Record by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the Cd Key Redeem Record to delete.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteCdKeyRedeemRecords([FromUri, Required, MinLength(1)] int[] ids)
        {
            int count = await _cdKeyRedeemRecordRepository.DeleteByIdsAsync(ids, true);
            if (count == 0)
            {
                return NotFound();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
