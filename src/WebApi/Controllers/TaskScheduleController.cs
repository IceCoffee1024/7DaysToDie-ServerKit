using CronExpressionDescriptor;
using IceCoffee.SimpleCRUD;
using SdtdServerKit.Constants;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Task Schedule
    /// </summary>
    [Authorize]
    [RoutePrefix("api/TaskSchedule")]
    public class TaskScheduleController : ApiController
    {
        private readonly ITaskScheduleRepository _taskScheduleRepository;
        private readonly ICommandListRepository _commandListRepository;
        private readonly Functions.TaskSchedule _taskSchedule;

        public TaskScheduleController(ITaskScheduleRepository taskScheduleRepository, ICommandListRepository commandListRepository, Functions.TaskSchedule taskSchedule)
        {
            _taskScheduleRepository = taskScheduleRepository;
            _commandListRepository = commandListRepository;
            _taskSchedule = taskSchedule;
        }

        /// <summary>
        /// Get locale by language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private static string GetLocale(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return "en";
                case Language.German:
                    return "de";
                case Language.Spanish:
                    return "es";
                case Language.French:
                    return "fr";
                case Language.Italian:
                    return "it";
                case Language.Japanese:
                    return "ja";
                case Language.Koreana:
                    return "ko";
                case Language.Polish:
                    return "pl";
                case Language.Brazilian:
                    return "pt-BR";
                case Language.Russian:
                    return "ru";
                case Language.Turkish:
                    return "tr";
                case Language.Schinese:
                    return "zh-hans";
                case Language.Tchinese:
                    return "zh-hant";
                default:
                    return "en";
            }
        }

        private static string GetExpressionDescription(string cronExpression, Language language)
        {
            return ExpressionDescriptor.GetDescription(cronExpression, new Options()
            {
                ThrowExceptionOnParseError = false,
                DayOfWeekStartIndexZero = true,
                Use24HourTimeFormat = true,
                Locale = GetLocale(language)
            });
        }

        /// <summary>
        /// Get all task schedules.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<TaskScheduleModel>> GetAll([FromUri] Language language)
        {
            var result = new List<TaskScheduleModel>();
            var entites = await _taskScheduleRepository.GetAllAsync();
            foreach (var item in entites)
            {
                result.Add(new TaskScheduleModel
                {
                    Id = item.Id,
                    CreatedAt = item.CreatedAt,
                    Name = item.Name,
                    CronExpression = item.CronExpression,
                    IsEnabled = item.IsEnabled,
                    LastRunAt = item.LastRunAt,
                    Description = item.Description,
                    ExpressionDescription = GetExpressionDescription(item.CronExpression, language)
                });
            }
            return result;
        }
        
        /// <summary>
        /// Creates a new task schedule.
        /// </summary>
        /// <param name="model">The task schedule model to create.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] TaskScheduleAdd model)
        {
            try
            {
                var entity = new T_TaskSchedule()
                {
                    Name = model.Name,
                    CreatedAt = DateTime.Now,
                    CronExpression = model.CronExpression,
                    IsEnabled = model.IsEnabled,
                    Description = model.Description,
                };
                await _taskScheduleRepository.InsertAsync(entity);

                if (entity.IsEnabled)
                {
                    _taskSchedule.AddTaskSchedule(entity);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Updates an existing task schedule.
        /// </summary>
        /// <param name="id">The ID of the task schedule to update.</param>
        /// <param name="model">The updated task schedule model.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Put([FromUri] int id, [FromBody] TaskScheduleUpdate model)
        {
            try
            {
                var entity = await _taskScheduleRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }

                entity.Name = model.Name;
                entity.CronExpression = model.CronExpression;
                entity.IsEnabled = model.IsEnabled;
                entity.Description = model.Description;
                await _taskScheduleRepository.UpdateAsync(entity);

                if (entity.IsEnabled)
                {
                    _taskSchedule.AddTaskSchedule(entity);
                }
                else
                {
                    _taskSchedule.RemoveTaskSchedule(entity.Id);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            return Ok();
        }

        /// <summary>
        /// Deletes a task schedule by ID.
        /// </summary>
        /// <param name="id">The ID of the task schedule to delete.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete([FromUri] int id)
        {
            _taskSchedule.RemoveTaskSchedule(id);

            int count = await _taskScheduleRepository.DeleteByIdAsync(id);
            if(count == 0)
            {
                return NotFound();
            }
            return Ok(count);
        }

        /// <summary>
        /// Deletes multiple task schedules by their IDs.
        /// </summary>
        /// <param name="ids">The IDs of the task schedules to delete.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Delete([FromUri, Required, MinLength(1)] int[] ids)
        {
            foreach (var id in ids)
            {
                _taskSchedule.RemoveTaskSchedule(id);
            }

            int count = await _taskScheduleRepository.DeleteByIdsAsync(ids, true);
            if (count == 0)
            {
                return NotFound();
            }
            return Ok(count);
        }

        /// <summary>
        /// Gets the commands associated with a task schedule.
        /// </summary>
        /// <param name="id">The ID of the task schedule.</param>
        /// <returns>A list of commands associated with the task schedule.</returns>
        [HttpGet]
        [Route("{id}/Commands")]
        public async Task<IEnumerable<T_CommandList>> GetCommands(int id)
        {
            var data = await _commandListRepository.GetListByTaskScheduleIdAsync(id);
            return data;
        }

        /// <summary>
        /// Updates the commands associated with a task schedule.
        /// </summary>
        /// <param name="id">The ID of the task schedule.</param>
        /// <param name="itemIds">The IDs of the commands to associate with the task schedule.</param>
        /// <returns>An IHttpActionResult indicating the result of the operation.</returns>
        [HttpPut]
        [Route("{id}/Commands")]
        public async Task<IHttpActionResult> PutCommands(int id, [FromBody, Required] int[] itemIds)
        {
            var entity = await _taskScheduleRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var entities = new List<T_TaskScheduleCommand>();
            foreach (var item in itemIds)
            {
                entities.Add(new T_TaskScheduleCommand()
                {
                    TaskScheduleId = id,
                    CommandId = item
                });
            }

            using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();
            var repository = unitOfWork.GetRepository<ITaskScheduleCommandRepository>();
            await repository.DeleteByTaskScheduleIdAsync(id);
            await repository.InsertAsync(entities);
            unitOfWork.Commit();

            return Ok();
        }
    }
}
