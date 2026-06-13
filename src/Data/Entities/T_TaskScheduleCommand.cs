using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    /// <summary>
    /// Represents a command scheduled to be executed as part of a task.
    /// </summary>
    public class T_TaskScheduleCommand
    {
        /// <summary>
        /// Gets or sets the ID of the task schedule.
        /// </summary>
        [PrimaryKey]
        public int TaskScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the command.
        /// </summary>
        [PrimaryKey]
        public int CommandId { get; set; }
    }
}
