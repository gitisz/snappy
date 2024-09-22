using System.Threading.Tasks;

namespace Snappy.Hubs
{
    using System.Collections.Generic;
    using Snappy.Models;
    using Snappy.Models.Yahama;

    public interface ISnappyHubClient
    {
        Task GlobalMessageAsync(GlobalMessage globalMessage);
        Task ServerStatusAsync(Snapcast snapcast);
        Task GroupChangedAsync(string id, Group group);
        Task GetGroupsAsync(List<Group> groups);
        Task NotifyYamahaMainZoneBasicStatusChangedAsync(YamahaAvMainZone yamahaAvMainZone);
        Task NotifyYamahaZone2BasicStatusChangedAsync(YamahaAvZone2 yamahaAvZone2);
        Task NotifyYamahaZone3BasicStatusChangedAsync(YamahaAvZone3 yamahaAvZone3);

    }
}