using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Models
{
    public class TaskScheduleAdd
    {
        /// <summary>
        /// Gets or sets the name of the task schedule.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the cron expression that defines the schedule.
        /// </summary>
        public required string CronExpression { get; set; }

        /// <summary>
        /// Gets or sets the command to be executed.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the description of the task schedule.
        /// </summary>
        public string? Description { get; set; }
    }

    public class TaskScheduleUpdate : TaskScheduleAdd
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task schedule.
        /// </summary>
        public int Id { get; set; }
    }

    public class TaskScheduleModel : TaskScheduleUpdate
    {
        /// <summary>
        /// Gets or sets the date and time when the task schedule was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the task was last run.
        /// </summary>
        public DateTime? LastRunAt { get; set; }

        /// <summary>
        /// Gets or sets the expression description of the task schedule.
        /// </summary>
        public string? ExpressionDescription { get; set; }
    }
}
