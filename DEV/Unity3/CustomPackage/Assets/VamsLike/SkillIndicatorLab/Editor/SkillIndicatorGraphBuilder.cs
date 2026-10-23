using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SysKill.SkillIndicators.Editor
{
    // Shader Graph's authoring classes are internal. Use its installed object API and serializer,
    // never fabricate serialized graph JSON, YAML, object IDs, or asset GUIDs.
    internal static class SkillIndicatorGraphBuilder
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        static Type T(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name)).FirstOrDefault(t => t != null)
            ?? throw new InvalidOperationException("Shader Graph API unavailable: " + name);
        static object New(string name) => Activator.CreateInstance(T(name), true);
        static object Get(object obj, string name) => obj.GetType().GetProperty(name, Flags).GetValue(obj);
        static void Set(object obj, string name, object value) => obj.GetType().GetProperty(name, Flags).SetValue(obj, value);
        static object Call(object obj, string name, params object[] args)
        {
            Type type = obj as Type ?? obj.GetType();
            foreach (MethodInfo method in type.GetMethods(Flags).Where(m => m.Name == name && !m.ContainsGenericParameters))
            {
                ParameterInfo[] ps = method.GetParameters();
                if (ps.Length < args.Length || ps.Skip(args.Length).Any(p => !p.IsOptional)) continue;
                if (args.Where((a, i) => a != null && !ps[i].ParameterType.IsInstanceOfType(a)).Any()) continue;
                object[] full = ps.Select((p, i) => i < args.Length ? args[i] : p.DefaultValue).ToArray();
                return method.Invoke(obj is Type ? null : obj, full);
            }
            throw new MissingMethodException(type.FullName, name);
        }
        static void EnumValue(object obj, string property, string value)
        {
            PropertyInfo p = obj.GetType().GetProperty(property, Flags);
            p.SetValue(obj, Enum.Parse(p.PropertyType, value));
        }
        static Array ArrayOf(string type, params object[] objects)
        {
            Array a = Array.CreateInstance(T(type), objects.Length);
            for (int i = 0; i < objects.Length; i++) a.SetValue(objects[i], i);
            return a;
        }
        static void Position(object node, float x, float y)
        {
            object state = Get(node, "drawState");
            Set(state, "expanded", true);
            Set(state, "position", new Rect(x, y, 220, 150));
            Set(node, "drawState", state);
        }
        static object Slot(int id, string name, int dimensions, bool output)
        {
            Type type = T("UnityEditor.ShaderGraph.Vector" + dimensions + "MaterialSlot");
            ConstructorInfo ctor = type.GetConstructors(Flags).First(c => c.GetParameters().Length >= 5);
            ParameterInfo[] ps = ctor.GetParameters();
            object[] args = ps.Select(p => p.HasDefaultValue ? p.DefaultValue : null).ToArray();
            args[0] = id; args[1] = name; args[2] = name;
            args[3] = Enum.Parse(ps[3].ParameterType, output ? "Output" : "Input");
            args[4] = Activator.CreateInstance(ps[4].ParameterType);
            return ctor.Invoke(args);
        }
        static void Connect(object graph, object from, int fromId, object to, int toId) =>
            Call(graph, "Connect", Call(from, "GetSlotReference", fromId), Call(to, "GetSlotReference", toId));

        public static Shader Create(string path, int shape, Color tint)
        {
            object graph = New("UnityEditor.ShaderGraph.GraphData");
            Call(graph, "AddContexts");
            Set(graph, "path", "Skill Indicators");
            object target = New("UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget");
            Call(target, "TrySetActiveSubTarget", T("UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget"));
            EnumValue(target, "surfaceType", "Transparent");
            EnumValue(target, "renderFace", "Both");
            Type fields = T("UnityEditor.ShaderGraph.BlockFields+SurfaceDescription");
            object colorDescriptor = fields.GetField("BaseColor", Flags).GetValue(null);
            object alphaDescriptor = fields.GetField("Alpha", Flags).GetValue(null);
            Call(graph, "InitializeOutputs", ArrayOf("UnityEditor.ShaderGraph.Target", target),
                ArrayOf("UnityEditor.ShaderGraph.BlockFieldDescriptor"));
            object colorBlock = New("UnityEditor.ShaderGraph.BlockNode");
            object alphaBlock = New("UnityEditor.ShaderGraph.BlockNode");
            Call(colorBlock, "Init", colorDescriptor); Call(alphaBlock, "Init", alphaDescriptor);
            object context = Get(graph, "fragmentContext");
            Set(Get(graph, "vertexContext"), "position", new Vector2(400, -160));
            Set(context, "position", new Vector2(400, 60));
            Call(graph, "AddBlock", colorBlock, context, 0);
            Call(graph, "AddBlock", alphaBlock, context, 1);
            Call(graph, "AddCategory", Call(T("UnityEditor.ShaderGraph.CategoryData"), "DefaultCategory"));

            object function = New("UnityEditor.ShaderGraph.CustomFunctionNode");
            EnumValue(function, "sourceType", "String");
            Set(function, "functionName", "SkillTelegraph");
            Set(function, "functionBody", Body);
            Call(function, "AddSlot", Slot(0, "UV", 2, false));
            string[] names = { "Shape", "Progress", "Inner", "Angle", "Border", "Impact", "Opacity", "Tint" };
            float[] defaults = { shape, 0.65f, 0.55f, 100, 0.025f, 0, 1 };
            for (int i = 0; i < names.Length; i++) Call(function, "AddSlot", Slot(i + 1, names[i], i == 7 ? 4 : 1, false));
            Call(function, "AddSlot", Slot(9, "Color", 3, true));
            Call(function, "AddSlot", Slot(10, "Alpha", 1, true));
            Call(graph, "AddNode", function);
            Position(function, 0, 0);
            object uv = New("UnityEditor.ShaderGraph.UVNode");
            Call(graph, "AddNode", uv); Position(uv, -600, -220);
            Connect(graph, uv, 0, function, 0);
            for (int i = 0; i < names.Length; i++)
            {
                object property = New("UnityEditor.ShaderGraph.Internal.Vector" + (i == 7 ? "4" : "1") + "ShaderProperty");
                Set(property, "displayName", names[i]);
                Set(property, "overrideReferenceName", "_" + names[i]);
                Set(property, "value", i == 7 ? (object)(Vector4)tint : defaults[i]);
                Call(graph, "AddGraphInput", property);
                object node = New("UnityEditor.ShaderGraph.PropertyNode");
                Call(graph, "AddNode", node); Set(node, "property", property);
                Position(node, -340, -100 + i * 155);
                Connect(graph, node, 0, function, i + 1);
            }
            Connect(graph, function, 9, colorBlock, 0);
            Connect(graph, function, 10, alphaBlock, 0);
            Call(graph, "ValidateGraph");
            Call(T("UnityEditor.ShaderGraph.FileUtilities"), "WriteShaderGraphToDisk", path, graph);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null) throw new InvalidOperationException("Shader Graph import failed: " + path);
            return shader;
        }

        // Signed distances keep borders anti-aliased. All forms retain their final outline during charge.
        const string Body = @"
float2 p = UV * 2.0 - 1.0;
float r = length(p);
float a = abs(atan2(p.x, p.y));
float halfAngle = radians(clamp(Angle, 1.0, 359.0) * 0.5);
float sd = r - 1.0;
float travel = r;
if (Shape > 0.5 && Shape < 1.5) { sd = max(abs(p.x), abs(p.y)) - 1.0; travel = UV.y; }
if (Shape > 1.5 && Shape < 2.5) { sd = max(sd, (a - halfAngle) * max(r, 0.001)); }
if (Shape > 2.5) { sd = max(sd, Inner - r); travel = saturate((r - Inner) / max(1.0 - Inner, 0.01)); }
float aa = max(fwidth(sd), 0.002);
float mask = 1.0 - smoothstep(-aa, aa, sd);
float edge = mask * smoothstep(-Border - aa, -Border + aa, sd);
float fill = (1.0 - smoothstep(Progress - 0.012, Progress + 0.012, travel)) * step(0.001, Progress);
float front = (1.0 - smoothstep(0.0, 0.022, abs(travel - Progress))) * step(0.001, Progress);
float accents = 0.0;
if (Shape > 0.5 && Shape < 1.5) {
    float chevron = abs(frac(UV.y * 4.0 + abs(p.x) * 0.32) - 0.5);
    accents = (1.0 - smoothstep(0.02, 0.045, chevron)) * 0.25 * fill;
}
Color = lerp(Tint.rgb, float3(1,1,1), saturate(edge * 0.3 + front * 0.35 + Impact * 0.8));
Alpha = saturate(mask * (0.08 + fill * 0.3 + front * 0.4 + accents + Impact * 0.32) + edge * 0.8) * Tint.a * Opacity;
";
    }
}
