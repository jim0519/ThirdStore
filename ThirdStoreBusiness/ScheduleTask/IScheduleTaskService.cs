using ThirdStoreCommon.Models.ScheduleTask;
using System.Collections.Generic;

namespace ThirdStoreBusiness.ScheduleTask
{
    public interface IScheduleTaskService
    {
        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="task">Task</param>
        void DeleteTask(T_ScheduleTask task);

        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Task</returns>
        T_ScheduleTask GetTaskById(int taskId);

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        T_ScheduleTask GetTaskByType(string type);

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Tasks</returns>
        IList<T_ScheduleTask> GetAllTasks(bool showHidden = false);

        /// <summary>
        /// Inserts a task
        /// </summary>
        /// <param name="task">Task</param>
        void InsertTask(T_ScheduleTask task);

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        void UpdateTask(T_ScheduleTask task);
    }
}
