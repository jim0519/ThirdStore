using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreBusiness.Order;
using ThirdStoreCommon;

namespace ThirdStoreBusiness.ScheduleTask
{
    
    public class UpdateOrderDeliveryInstructionTask : ITask
    {
        private readonly IOrderService _orderService;
        private readonly IScheduleTaskService _scheduleTaskService;

        public UpdateOrderDeliveryInstructionTask(IOrderService orderService,
            IScheduleTaskService scheduleTaskService)
        {
            _orderService = orderService;
            _scheduleTaskService = scheduleTaskService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            try
            { 
                var updateOrderDeliveryInstructionTask = _scheduleTaskService.GetTaskByType("ThirdStoreBusiness.ScheduleTask.UpdateOrderDeliveryInstructionTask, ThirdStoreBusiness");
                if (updateOrderDeliveryInstructionTask == null)
                    throw new Exception("No task exists.");
                var intervalMin= updateOrderDeliveryInstructionTask.Seconds/60*2;
                var getOrderRequestDatetimeNow = DateTime.Now.AddMinutes(-2);
                _orderService.UpdateOrderDeliveryInstruction(getOrderRequestDatetimeNow.AddMinutes(0- intervalMin), getOrderRequestDatetimeNow);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }
    }
}
