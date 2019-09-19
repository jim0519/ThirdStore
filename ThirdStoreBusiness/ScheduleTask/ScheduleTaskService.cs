using ThirdStoreData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.ScheduleTask;

namespace ThirdStoreBusiness.ScheduleTask
{
    public class ScheduleTaskService : IScheduleTaskService
    {
        private readonly IRepository<T_ScheduleTask> _taskRepository;

        public ScheduleTaskService(IRepository<T_ScheduleTask> taskRepository)
        {
            this._taskRepository = taskRepository;
        }

        #region Methods

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual void DeleteTask(T_ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Delete(task);
        }

        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Task</returns>
        public virtual T_ScheduleTask GetTaskById(int taskId)
        {
            if (taskId == 0)
                return null;

            return _taskRepository.GetById(taskId);
        }

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        public virtual T_ScheduleTask GetTaskByType(string type)
        {
            if (String.IsNullOrWhiteSpace(type))
                return null;

            var query = _taskRepository.Table;
            query = query.Where(st => st.Type == type);
            query = query.OrderByDescending(t => t.ID);

            var task = query.FirstOrDefault();
            return task;
        }

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Tasks</returns>
        public virtual IList<T_ScheduleTask> GetAllTasks(bool showHidden = false)
        {
            var query = _taskRepository.Table;
            if (!showHidden)
            {
                query = query.Where(t => t.Enabled);
            }
            query = query.OrderByDescending(t => t.Seconds);

            var tasks = query.ToList();
            return tasks;
        }

        /// <summary>
        /// Inserts a task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual void InsertTask(T_ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Insert(task);
        }

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual void UpdateTask(T_ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Update(task);
        }

        #endregion

    }
}
