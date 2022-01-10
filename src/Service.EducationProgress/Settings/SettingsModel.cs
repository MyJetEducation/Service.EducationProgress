using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.EducationProgress.Settings
{
    public class SettingsModel
    {
        [YamlProperty("EducationProgress.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("EducationProgress.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("EducationProgress.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("EducationProgress.ServerKeyValueServiceUrl")]
        public string ServerKeyValueServiceUrl { get; set; }

        [YamlProperty("EducationProgress.ServiceBusWriter")]
        public string ServiceBusWriter { get; set; }

        [YamlProperty("EducationProgress.UserHabitServiceUrl")]
        public string UserHabitServiceUrl { get; set; }

        [YamlProperty("EducationProgress.KeyEducationProgress")]
        public string KeyEducationProgress { get; set; }
    }
}
