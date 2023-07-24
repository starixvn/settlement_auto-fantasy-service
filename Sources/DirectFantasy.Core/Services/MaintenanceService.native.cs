using ONEbook.UM;
using ONEbook.UM.Services;

namespace DirectFantasy.Core.Services;

public interface IMaintenanceService
{
    bool IsMaintenance { get; }
}

public class MaintenanceServiceImp : IMaintenanceService
{
    public bool IsMaintenance
    {
        get
        {
            var maintenanceService = new MaintenanceService(
                UMClientManager.RestClient,
                UMClientManager.CacheService,
                UMClientManager.UMClientConfigs);

            var downTime = maintenanceService.GetUnderMaintenanceTime();

            return downTime.IsUnderMaintenanceTime;
        }
    }
}