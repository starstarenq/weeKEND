using System;
using UnityEngine;

namespace VamSurvivalLike.Monsters.Definition
{
    /// <summary>Optional JSON payload. Negative numeric values mean "use the SO value".</summary>
    [Serializable]
    public sealed class MonsterDefinitionOverride
    {
        public int schemaVersion = 1;
        public string monsterId;
        public StatsOverride stats = new();
        public BrainOverride brain = new();
        public ViewOverride[] views = Array.Empty<ViewOverride>();

        [Serializable]
        public sealed class StatsOverride
        {
            public float maxHealth = -1f;
            public float moveSpeed = -1f;
            public float contactDamage = -1f;
            public float contactInterval = -1f;
        }

        [Serializable]
        public sealed class BrainOverride
        {
            public float stoppingDistance = -1f;
            public float turnSpeed = -1f;
            public float triggerDistance = -1f;
            public float windupDuration = -1f;
            public float chargeDuration = -1f;
            public float recoveryDuration = -1f;
            public float chargeSpeedMultiplier = -1f;
            public float orbitRadius = -1f;
            public int clockwise = -1;
            public float desiredDistance = -1f;
            public float distanceTolerance = -1f;
        }

        [Serializable]
        public sealed class ViewOverride
        {
            public string moduleId;
            public int shape = -1;
            public float radius = -1f;
            public float width = -1f;
            public float length = -1f;
            public float angle = -1f;
            public float innerRadiusRatio = -1f;
            public float border = -1f;
            public float opacity = -1f;
            public string tint;
        }

        public static MonsterDefinitionOverride Parse(TextAsset json, string expectedMonsterId)
        {
            if (json == null || string.IsNullOrWhiteSpace(json.text))
                return null;
            var data = JsonUtility.FromJson<MonsterDefinitionOverride>(json.text);
            if (data == null)
                throw new FormatException($"Could not parse monster override '{json.name}'.");
            if (data.schemaVersion != 1)
                throw new NotSupportedException($"Monster override schema {data.schemaVersion} is not supported.");
            if (!string.IsNullOrEmpty(data.monsterId) && data.monsterId != expectedMonsterId)
                throw new InvalidOperationException(
                    $"Override monsterId '{data.monsterId}' does not match MonsterDefinition ID '{expectedMonsterId}'. " +
                    "Set both IDs to the same value, or leave the JSON monsterId empty to use it as a reusable override.");
            return data;
        }

        public ViewOverride FindView(string id)
        {
            if (views == null)
                return null;
            foreach (var view in views)
                if (view != null && view.moduleId == id)
                    return view;
            return null;
        }
    }
}
