using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Represents a task schedule.
    /// </summary>
    public class T_TaskSchedule
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task schedule.
        /// </summary>
        [PrimaryKey, IgnoreUpdate, IgnoreInsert]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the task schedule was created.
        /// </summary>
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; }

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
        /// Gets or sets the date and time when the task was last run.
        /// </summary>
        public DateTime? LastRunAt { get; set; }

        /// <summary>
        /// Gets or sets the description of the task schedule.
        /// </summary>
        public string? Description { get; set; }
    }
}