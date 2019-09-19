using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.ScheduleTask;

namespace ThirdStoreData.Mapping
{
    public class T_ScheduleTaskMap:EntityTypeConfiguration<T_ScheduleTask>
    {
        public T_ScheduleTaskMap()
        {
            this.ToTable("T_ScheduleTask");
            this.HasKey(t => t.ID);
            this.Property(t => t.Name).IsRequired();
            this.Property(t => t.Type).IsRequired();
        }
    }
}
