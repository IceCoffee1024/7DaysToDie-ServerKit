using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Tele Record
    /// </summary>
    [Authorize]
    [RoutePrefix("api/TeleRecord")]
    public class TeleRecordController : ApiController
    {
        private readonly ITeleRecordRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleRecordController"/> class.
        /// </summary>
        /// <param name="teleRecordRepository">The repository for records.</param>
        public TeleRecordController(ITeleRecordRepository teleRecordRepository)
        {
            _repository = teleRecordRepository;
        }

        /// <summary>
        /// Gets all records.
        /// </summary>
        /// <returns>A collection of records.</returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<T_TeleRecord>> Get()
        {
            var data = await _repository.GetAllAsync();
            return data;
        }

        /// <summary>
        /// Deletes a record by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the record to delete.</param>
        /// <returns>The number of records deleted.</returns>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            int count = await _repository.DeleteByIdAsync(id);
            return Ok(count);
        }

        /// <summary>
        /// Deletes multiple records by their identifiers.
        /// </summary>
        /// <param name="ids">The identifiers of the records to delete.</param>
        /// <returns>The number of records deleted..</returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri, Required, MinLength(1)] int[] ids)
        {
            int count = await _repository.DeleteByIdsAsync(ids, true);
            return Ok(count);
        }
    }
}