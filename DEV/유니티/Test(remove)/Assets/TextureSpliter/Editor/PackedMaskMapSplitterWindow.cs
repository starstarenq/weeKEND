using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Splits an AO/Roughness/Metal packed texture into maps used by Lit shaders:
/// MetallicSmoothness (R = Metallic, A = Smoothness) and AO (grayscale).
/// </summary>
public sealed class PackedMaskMapSplitterWindow : EditorWindow
{
    private enum SourceChannel
    {
        Red,
        Green,
        Blue,
        Alpha,
    }

    [SerializeField] private Texture2D sourceTexture;
    [SerializeField] private SourceChannel aoChannel = SourceChannel.Red;
    [SerializeField] private SourceChannel roughnessChannel = SourceChannel.Green;
    [SerializeField] private SourceChannel metalChannel = SourceChannel.Blue;
    [SerializeField] private bool invertRoughness = true;
    [SerializeField] private string metallicSuffix = "_MetallicSmoothness";
    [SerializeField] private string aoSuffix = "_AO";

    [MenuItem("Tools/Textures/Split Packed AO Roughness Metal")]
    private static void Open()
    {
        var window = GetWindow<PackedMaskMapSplitterWindow>();
        window.titleContent = new GUIContent("Packed Map Splitter");
        window.minSize = new Vector2(430f, 360f);

        if (Selection.activeObject is Texture2D selected)
            window.sourceTexture = selected;

        window.Show();
    }

    [MenuItem("Assets/Textures/Split Packed AO Roughness Metal", true)]
    private static bool ValidateOpenFromAsset()
        => Selection.activeObject is Texture2D;

    [MenuItem("Assets/Textures/Split Packed AO Roughness Metal")]
    private static void OpenFromAsset()
        => Open();

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Packed Mask Map Splitter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "AO + Roughness + Metal 패킹 텍스처를 Lit용 Metallic 맵(R=Metal, A=Smoothness)과 AO 맵으로 분리합니다.",
            MessageType.Info);

        EditorGUILayout.Space(6f);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField(
            "Source Packed Texture", sourceTexture, typeof(Texture2D), false);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Source Channel Mapping", EditorStyles.boldLabel);
        aoChannel = (SourceChannel)EditorGUILayout.EnumPopup("AO", aoChannel);
        roughnessChannel = (SourceChannel)EditorGUILayout.EnumPopup("Roughness", roughnessChannel);
        metalChannel = (SourceChannel)EditorGUILayout.EnumPopup("Metal", metalChannel);
        invertRoughness = EditorGUILayout.Toggle(
            new GUIContent("Roughness → Smoothness", "Smoothness = 1 - Roughness"),
            invertRoughness);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Output File Suffix", EditorStyles.boldLabel);
        metallicSuffix = EditorGUILayout.TextField("Metallic + Smoothness", metallicSuffix);
        aoSuffix = EditorGUILayout.TextField("AO", aoSuffix);

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "출력 PNG는 원본 텍스처와 같은 폴더에 저장됩니다. 마스크 데이터가 변형되지 않도록 sRGB는 자동으로 꺼집니다.",
            MessageType.None);

        GUILayout.FlexibleSpace();

        using (new EditorGUI.DisabledScope(sourceTexture == null))
        {
            if (GUILayout.Button("Split Texture", GUILayout.Height(34f)))
                SplitTexture();
        }

        EditorGUILayout.Space(8f);
    }

    private void SplitTexture()
    {
        string sourceAssetPath = AssetDatabase.GetAssetPath(sourceTexture);
        if (string.IsNullOrEmpty(sourceAssetPath) || !sourceAssetPath.StartsWith("Assets/", StringComparison.Ordinal))
        {
            EditorUtility.DisplayDialog("Packed Map Splitter", "Assets 폴더 안의 텍스처를 선택해 주세요.", "확인");
            return;
        }

        if (!TryBuildOutputPaths(sourceAssetPath, out string metallicPath, out string aoPath))
            return;

        bool metallicExists = File.Exists(ToAbsolutePath(metallicPath));
        bool aoExists = File.Exists(ToAbsolutePath(aoPath));
        if ((metallicExists || aoExists) && !EditorUtility.DisplayDialog(
                "Overwrite output textures?",
                "같은 이름의 출력 텍스처가 이미 있습니다. 덮어쓸까요?",
                "덮어쓰기",
                "취소"))
        {
            return;
        }

        Texture2D readableTexture = null;
        bool ownsReadableTexture = false;

        try
        {
            EditorUtility.DisplayProgressBar("Packed Map Splitter", "Reading source texture...", 0.15f);
            readableTexture = LoadReadableTexture(sourceAssetPath, out ownsReadableTexture);
            if (readableTexture == null)
                throw new InvalidOperationException("원본 텍스처를 읽을 수 없습니다.");

            Color32[] sourcePixels = readableTexture.GetPixels32();
            var metallicPixels = new Color32[sourcePixels.Length];
            var aoPixels = new Color32[sourcePixels.Length];

            EditorUtility.DisplayProgressBar("Packed Map Splitter", "Splitting channels...", 0.45f);
            for (int i = 0; i < sourcePixels.Length; i++)
            {
                Color32 pixel = sourcePixels[i];
                byte ao = ReadChannel(pixel, aoChannel);
                byte metal = ReadChannel(pixel, metalChannel);
                byte roughness = ReadChannel(pixel, roughnessChannel);
                byte smoothness = invertRoughness ? (byte)(255 - roughness) : roughness;

                metallicPixels[i] = new Color32(metal, 0, 0, smoothness);
                aoPixels[i] = new Color32(ao, ao, ao, 255);
            }

            EditorUtility.DisplayProgressBar("Packed Map Splitter", "Writing output textures...", 0.7f);
            WritePng(metallicPath, readableTexture.width, readableTexture.height, metallicPixels);
            WritePng(aoPath, readableTexture.width, readableTexture.height, aoPixels);

            AssetDatabase.Refresh();
            ConfigureMaskTexture(metallicPath, true);
            ConfigureMaskTexture(aoPath, false);
            AssetDatabase.SaveAssets();

            var metallicAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
            Selection.activeObject = metallicAsset;
            EditorGUIUtility.PingObject(metallicAsset);

            Debug.Log($"[PackedMapSplitter] Created '{metallicPath}' and '{aoPath}'.", metallicAsset);
            EditorUtility.DisplayDialog(
                "Packed Map Splitter",
                $"분할이 완료되었습니다.\n\nMetallic + Smoothness:\n{metallicPath}\n\nAO:\n{aoPath}",
                "확인");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Packed Map Splitter", $"텍스처 분할에 실패했습니다.\n\n{exception.Message}", "확인");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (ownsReadableTexture && readableTexture != null)
                DestroyImmediate(readableTexture);
        }
    }

    private bool TryBuildOutputPaths(string sourcePath, out string metallicPath, out string aoPath)
    {
        metallicPath = null;
        aoPath = null;

        string cleanMetallicSuffix = SanitizeSuffix(metallicSuffix);
        string cleanAoSuffix = SanitizeSuffix(aoSuffix);
        if (string.IsNullOrWhiteSpace(cleanMetallicSuffix) || string.IsNullOrWhiteSpace(cleanAoSuffix))
        {
            EditorUtility.DisplayDialog("Packed Map Splitter", "출력 파일 suffix를 입력해 주세요.", "확인");
            return false;
        }

        string directory = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        metallicPath = $"{directory}/{fileName}{cleanMetallicSuffix}.png";
        aoPath = $"{directory}/{fileName}{cleanAoSuffix}.png";

        if (string.Equals(metallicPath, aoPath, StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Packed Map Splitter", "두 출력 파일의 suffix는 서로 달라야 합니다.", "확인");
            return false;
        }

        return true;
    }

    private static Texture2D LoadReadableTexture(string assetPath, out bool ownsTexture)
    {
        ownsTexture = false;
        string extension = Path.GetExtension(assetPath).ToLowerInvariant();

        // Decode common source formats directly so compression and Read/Write import settings
        // do not alter source mask values or require a source reimport.
        if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
        {
            byte[] bytes = File.ReadAllBytes(ToAbsolutePath(assetPath));
            var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true)
            {
                name = Path.GetFileNameWithoutExtension(assetPath) + "_ReadableCopy"
            };

            if (ImageConversion.LoadImage(decoded, bytes, false))
            {
                ownsTexture = true;
                return decoded;
            }

            DestroyImmediate(decoded);
        }

        var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (imported != null && imported.isReadable)
            return imported;

        throw new InvalidOperationException(
            "PNG/JPG 이외의 포맷은 Texture Import Settings에서 Read/Write를 활성화한 뒤 다시 시도해 주세요.");
    }

    private static void WritePng(string assetPath, int width, int height, Color32[] pixels)
    {
        var output = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        try
        {
            output.SetPixels32(pixels);
            output.Apply(false, false);
            File.WriteAllBytes(ToAbsolutePath(assetPath), output.EncodeToPNG());
        }
        finally
        {
            DestroyImmediate(output);
        }
    }

    private static void ConfigureMaskTexture(string assetPath, bool preserveAlpha)
    {
        if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
            return;

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = false;
        importer.alphaSource = preserveAlpha
            ? TextureImporterAlphaSource.FromInput
            : TextureImporterAlphaSource.None;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.SaveAndReimport();
    }

    private static byte ReadChannel(Color32 pixel, SourceChannel channel)
    {
        return channel switch
        {
            SourceChannel.Red => pixel.r,
            SourceChannel.Green => pixel.g,
            SourceChannel.Blue => pixel.b,
            SourceChannel.Alpha => pixel.a,
            _ => 0,
        };
    }

    private static string SanitizeSuffix(string suffix)
    {
        if (suffix == null)
            return string.Empty;

        string sanitized = suffix.Trim();
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            sanitized = sanitized.Replace(invalidCharacter.ToString(), string.Empty);
        return sanitized;
    }

    private static string ToAbsolutePath(string assetPath)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
    }
}
