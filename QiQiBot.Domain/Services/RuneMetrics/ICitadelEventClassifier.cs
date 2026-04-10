using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services.RuneMetrics;

public interface ICitadelEventClassifier
{
    CitadelEventType? Classify(RuneMetricsActivityDTO activity);
}
