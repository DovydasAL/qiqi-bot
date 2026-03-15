using QiQiBot.Models;

namespace QiQiBot.Services
{
    public interface ICitadelActivityService
    {
        Task ProcessCitadelActivitiesAsync(List<RuneMetricsProfileDTO> profiles, CancellationToken cancellationToken);
    }
}
