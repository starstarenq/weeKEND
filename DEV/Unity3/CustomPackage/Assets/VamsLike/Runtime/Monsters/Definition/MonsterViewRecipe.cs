using System;
using System.Collections.Generic;
using SysKill.SkillIndicators;
using UnityEngine;
using VamSurvivalLike.Monsters.View;

namespace VamSurvivalLike.Monsters.Definition
{
    [Serializable]
    public sealed class MonsterViewRecipe
    {
        [SerializeReference] private List<MonsterViewModuleDefinition> modules = new();

        public IReadOnlyList<MonsterViewModuleDefinition> Modules => modules;
        public void Add(MonsterViewModuleDefinition module) => modules.Add(module);
        public void RemoveAt(int index) => modules.RemoveAt(index);
        public void Clear() => modules.Clear();
    }

    [Serializable]
    public abstract class MonsterViewModuleDefinition
    {
        [SerializeField] private string moduleId = "view";
        [SerializeField] private string anchorPath = "ViewRoot";
        [SerializeField] private string attackKey;

        public string ModuleId => moduleId;
        protected string AttackKey => attackKey;

        protected void SetIdentity(string id, string path, string key)
        {
            moduleId = id;
            anchorPath = path;
            attackKey = key;
        }
        protected abstract void Build(GameObject monsterRoot, Transform anchor, MonsterDefinitionOverride.ViewOverride viewOverride);

        public void Assemble(GameObject monsterRoot, MonsterDefinitionOverride dataOverride)
        {
            var anchor = MonsterAssembler.GetOrCreateAnchor(monsterRoot.transform, anchorPath);
            Build(monsterRoot, anchor, dataOverride?.FindView(moduleId));
        }
    }

    [Serializable]
    public sealed class SkillIndicatorViewModuleDefinition : MonsterViewModuleDefinition
    {
        [SerializeField] private SkillIndicator indicatorPrefab;
        [SerializeField] private SkillIndicator.Shape shape = SkillIndicator.Shape.Sector;
        [SerializeField, Min(0.05f)] private float radius = 2f;
        [SerializeField, Min(0.05f)] private float width = 2f;
        [SerializeField, Min(0.05f)] private float length = 4f;
        [SerializeField, Range(1f, 359f)] private float angle = 100f;
        [SerializeField, Range(0.02f, 0.95f)] private float innerRadiusRatio = 0.55f;
        [SerializeField, Range(0.005f, 0.15f)] private float border = 0.025f;
        [SerializeField, Range(0f, 1f)] private float opacity = 1f;
        [SerializeField, ColorUsage(true, true)] private Color tint = new(1f, 0.2f, 0.36f, 1f);
        [SerializeField] private bool followDuringWindup = true;
        [SerializeField] private bool syncRangeFromAttack = true;
        [SerializeField] private bool pinToWindupWorldPosition = true;

        public void Configure(
            SkillIndicator prefab,
            SkillIndicator.Shape indicatorShape,
            string id,
            string key,
            float indicatorRadius,
            float indicatorWidth,
            float indicatorLength,
            float indicatorAngle,
            Color indicatorTint,
            bool syncRange,
            bool pinToWorld)
        {
            indicatorPrefab = prefab;
            shape = indicatorShape;
            radius = indicatorRadius;
            width = indicatorWidth;
            length = indicatorLength;
            angle = indicatorAngle;
            tint = indicatorTint;
            syncRangeFromAttack = syncRange;
            pinToWindupWorldPosition = pinToWorld;
            SetIdentity(id, "ViewRoot/Ground", key);
        }

        protected override void Build(GameObject monsterRoot, Transform anchor, MonsterDefinitionOverride.ViewOverride viewOverride)
        {
            if (indicatorPrefab == null)
                throw new InvalidOperationException($"Skill Indicator module '{ModuleId}' has no prefab.");

            var indicator = UnityEngine.Object.Instantiate(indicatorPrefab, anchor);
            indicator.name = $"{ModuleId}_Indicator";
            indicator.owner = monsterRoot.transform;
            indicator.shape = viewOverride != null && viewOverride.shape >= 0
                ? (SkillIndicator.Shape)viewOverride.shape : shape;
            indicator.radius = Positive(viewOverride?.radius ?? -1f, radius);
            indicator.width = Positive(viewOverride?.width ?? -1f, width);
            indicator.length = Positive(viewOverride?.length ?? -1f, length);
            indicator.angle = Positive(viewOverride?.angle ?? -1f, angle);
            indicator.innerRadiusRatio = Positive(viewOverride?.innerRadiusRatio ?? -1f, innerRadiusRatio);
            indicator.border = Positive(viewOverride?.border ?? -1f, border);
            indicator.opacity = viewOverride != null && viewOverride.opacity >= 0f ? viewOverride.opacity : opacity;
            indicator.tint = ParseColor(viewOverride?.tint, tint);
            indicator.targetLayers = 0;
            indicator.Apply();
            indicator.gameObject.SetActive(false);

            var view = monsterRoot.AddComponent<MonsterSkillIndicatorView>();
            view.Configure(indicator, AttackKey, followDuringWindup,
                syncRangeFromAttack, pinToWindupWorldPosition);
        }

        private static float Positive(float candidate, float fallback) => candidate >= 0f ? candidate : fallback;

        private static Color ParseColor(string html, Color fallback) =>
            !string.IsNullOrWhiteSpace(html) && ColorUtility.TryParseHtmlString(html, out var parsed) ? parsed : fallback;
    }

    [Serializable]
    public sealed class PrefabViewModuleDefinition : MonsterViewModuleDefinition
    {
        [SerializeField] private GameObject viewPrefab;

        protected override void Build(GameObject monsterRoot, Transform anchor, MonsterDefinitionOverride.ViewOverride viewOverride)
        {
            if (viewPrefab == null)
                throw new InvalidOperationException($"Prefab View module '{ModuleId}' has no prefab.");
            var instance = UnityEngine.Object.Instantiate(viewPrefab, anchor);
            instance.name = $"{ModuleId}_{viewPrefab.name}";
        }
    }
}
