using System;

namespace Navred.Core.Tools
{
    public static class Env
    {
        public static string ToStage(string value)
        {
            var stage = Environment.GetEnvironmentVariable("STAGE");
            stage = string.IsNullOrWhiteSpace(stage) ? "dev" : stage;
            var result = $"{stage}_{value}";

            return result;
        }
    }
}
