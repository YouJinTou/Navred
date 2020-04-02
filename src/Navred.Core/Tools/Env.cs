using System;
using System.Collections.Generic;

namespace Navred.Core.Tools
{
    public static class Env
    {
        public static string ToStage(string value)
        {
            var stage = Environment.GetEnvironmentVariable(Constants.StageUpper);
            stage = string.IsNullOrWhiteSpace(stage) ? "qa" : stage;
            var result = $"{stage}_{value}";

            return result;
        }

        public static string GetVar(string name, string fallback = null, bool isRequired = false)
        {
            var result = Environment.GetEnvironmentVariable(name) ?? fallback;

            if (isRequired && string.IsNullOrWhiteSpace(result))
            {
                throw new KeyNotFoundException($"Missing environment variable: {name}");
            }

            return result;
        }

    }
}
