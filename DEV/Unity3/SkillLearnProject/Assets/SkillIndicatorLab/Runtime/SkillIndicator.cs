using System;
using System.Collections.Generic;
using UnityEngine;

namespace SysKill.SkillIndicators
{
    /// <summary>Ground-plane telegraph. Local +Z is attack direction; dimensions are world units at unit root scale.</summary>
    [ExecuteAlways]
    public sealed class SkillIndicator : MonoBehaviour
    {
        public enum Shape { Circle, Rectangle, Sector, Donut }
        public Shape shape;
        public Renderer surface;
        [Min(0.05f)] public float radius = 2f;
        [Min(0.05f)] public float width = 2f;
        [Min(0.05f)] public float length = 4f;
        [Range(1f, 359f)] public float angle = 100f;
        [Range(0.02f, 0.95f)] public float innerRadiusRatio = 0.55f;
        [Range(0.005f, 0.15f)] public float border = 0.025f;
        [Range(0f, 1f)] public float progress = 0.65f;
        [Range(0f, 1f)] public float opacity = 1f;
        [ColorUsage(true, true)] public Color tint = new Color(0.1f, 0.8f, 1f, 0.9f);
        [Range(0f, 1f)] public float impact;

        MaterialPropertyBlock properties;
        public event Action<SkillIndicator> Triggered;
        [Header("Hit detection (target pivot must be inside the shape)")]
        [Min(0)] public int damage = 25;
        [Min(0)] public float hitDuration = 0.65f;
        [Min(0.01f)] public float hitHeight = 2f;
        public LayerMask targetLayers = ~0;
        public Transform owner;
        public Camera damageCamera;
        public event Action<SkillDamageTarget, int> DamageApplied;
        readonly HashSet<SkillDamageTarget> hitTargets = new HashSet<SkillDamageTarget>();
        float hitUntil;
        bool hitActive;

        public void Place(Vector3 groundPoint, Vector3 forward)
        {
            transform.position = groundPoint;
            forward.y = 0;
            if (forward.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(forward);
            Apply();
        }

        public void SetProgress(float value) { progress = Mathf.Clamp01(value); Apply(); }
        public void Trigger()
        {
            impact = 1; Apply();
            if (Application.isPlaying)
            {
                hitTargets.Clear();
                hitUntil = Time.time + Mathf.Max(0, hitDuration);
                hitActive = true;
                Physics.SyncTransforms();
                DetectHits();
            }
            Triggered?.Invoke(this);
        }
        public void EndHitWindow() { hitActive = false; hitTargets.Clear(); }
        void FixedUpdate()
        {
            if (!Application.isPlaying || !hitActive) return;
            if (Time.time >= hitUntil) { EndHitWindow(); return; }
            DetectHits();
        }
        void DetectHits()
        {
            Vector3 center = transform.TransformPoint(new Vector3(0, hitHeight * 0.5f,
                shape == Shape.Rectangle ? length * 0.5f : 0));
            Vector3 extents = new Vector3(shape == Shape.Rectangle ? width * 0.5f : radius,
                hitHeight * 0.5f, shape == Shape.Rectangle ? length * 0.5f : radius);
            Vector3 scale = transform.lossyScale;
            extents = Vector3.Scale(extents, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            foreach (Collider candidate in Physics.OverlapBox(center, extents, transform.rotation, targetLayers, QueryTriggerInteraction.Collide))
            {
                var target = candidate.GetComponentInParent<SkillDamageTarget>();
                if (target == null || !target.isActiveAndEnabled || target.CurrentHealth <= 0 || hitTargets.Contains(target)) continue;
                if (owner != null && (target.transform == owner || target.transform.IsChildOf(owner))) continue;
                Vector3 local = transform.InverseTransformPoint(target.transform.position);
                if (local.y < 0 || local.y > hitHeight || !ContainsGroundPoint(target.transform.position)) continue;
                hitTargets.Add(target);
                Vector3 popupPosition = target.transform.position + target.popupOffset;
                int applied = target.ApplyDamage(damage);
                SkillDamagePopup.Show(applied, popupPosition, damageCamera);
                DamageApplied?.Invoke(target, applied);
            }
        }
        void OnEnable() => Apply();
        void LateUpdate() => Apply();
        void OnDisable() { EndHitWindow(); if (surface != null) surface.SetPropertyBlock(null); }

        public void Apply()
        {
            if (surface == null) return;
            float r = Mathf.Max(0.05f, radius);
            float w = Mathf.Max(0.05f, width);
            float l = Mathf.Max(0.05f, length);
            // Quad UV0.y increases along local +Z after this rotation.
            surface.transform.localRotation = Quaternion.Euler(90, 0, 0);
            surface.transform.localPosition = new Vector3(0, 0.035f, shape == Shape.Rectangle ? l * 0.5f : 0);
            surface.transform.localScale = shape == Shape.Rectangle
                ? new Vector3(w, l, 1) : new Vector3(r * 2, r * 2, 1);
            if (properties == null) properties = new MaterialPropertyBlock();
            properties.Clear();
            properties.SetFloat("_Shape", (float)shape);
            properties.SetFloat("_Progress", Mathf.Clamp01(progress));
            properties.SetFloat("_Inner", Mathf.Clamp(innerRadiusRatio, 0.02f, 0.95f));
            properties.SetFloat("_Angle", Mathf.Clamp(angle, 1, 359));
            properties.SetFloat("_Border", Mathf.Clamp(border, 0.005f, 0.15f));
            properties.SetFloat("_Impact", Mathf.Clamp01(impact));
            properties.SetFloat("_Opacity", Mathf.Clamp01(opacity));
            properties.SetVector("_Tint", (Vector4)tint);
            surface.SetPropertyBlock(properties);
        }

        /// <summary>Planar shape test using the same boundary as the graph; caller controls height and damage rules.</summary>
        public bool ContainsGroundPoint(Vector3 worldPoint)
        {
            Vector3 p = transform.InverseTransformPoint(worldPoint);
            if (shape == Shape.Rectangle)
                return Mathf.Abs(p.x) <= width * 0.5f && p.z >= 0 && p.z <= length;
            float d = new Vector2(p.x, p.z).magnitude;
            if (d > radius) return false;
            if (shape == Shape.Donut) return d >= radius * innerRadiusRatio;
            if (shape == Shape.Sector && d > 0.00001f)
                return Mathf.Abs(Mathf.Atan2(p.x, p.z) * Mathf.Rad2Deg) <= angle * 0.5f;
            return true;
        }
    }
}
