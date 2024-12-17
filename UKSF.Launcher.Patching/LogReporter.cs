using System;

namespace UKSF.Launcher.Patching {
    public class LogReporter {
        protected LogReporter(Action<string> logAction) => LogReporterAction = logAction;
        protected readonly Action<string> LogReporterAction;
    }
}