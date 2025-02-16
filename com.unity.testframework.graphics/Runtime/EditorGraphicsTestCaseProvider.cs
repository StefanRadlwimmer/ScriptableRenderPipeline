#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.TestTools.Graphics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.TestTools.Graphics
{
    internal class EditorGraphicsTestCaseProvider : IGraphicsTestCaseProvider
    {
        string m_ReferenceImagePath = string.Empty;

        public EditorGraphicsTestCaseProvider()
        {
        }

        public EditorGraphicsTestCaseProvider(string referenceImagePath)
        {
            m_ReferenceImagePath = referenceImagePath;
        }

        public IEnumerable<GraphicsTestCase> GetTestCases()
        {
            var allImages = CollectReferenceImagePathsFor(string.IsNullOrEmpty(m_ReferenceImagePath) ? ReferenceImagesRoot : m_ReferenceImagePath, QualitySettings.activeColorSpace, Application.platform,
                SystemInfo.graphicsDeviceType);

            foreach (var scenePath in EditorBuildSettings.scenes.Where(s => s.enabled == true).Select(s => s.path).ToArray())
            {
                Texture2D referenceImage = null;

                string imagePath;
                if (allImages.TryGetValue(Path.GetFileNameWithoutExtension(scenePath), out imagePath))
                {
                    referenceImage = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
                }

                yield return new GraphicsTestCase(scenePath, referenceImage);
            }
        }

        public const string ReferenceImagesRoot = "Assets/ReferenceImages";

        public static Dictionary<string, string> CollectReferenceImagePathsFor(string referenceImageRoot, ColorSpace colorSpace, RuntimePlatform runtimePlatform,
            GraphicsDeviceType graphicsApi)
        {
            var result = new Dictionary<string, string>();

            if (!Directory.Exists(referenceImageRoot))
                return result;

            var fullPathPrefix = string.Format("{0}/{1}/{2}/{3}/", referenceImageRoot, colorSpace, runtimePlatform, graphicsApi);

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith(fullPathPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Count(ch => ch == '/')))
            {
                // Skip directories
                if (!File.Exists(assetPath))
                    continue;

                var fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (fileName == null)
                    continue;

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (!texture)
                    continue;

                result[fileName] = assetPath;
            }

            return result;
        }
    }
}
#endif
