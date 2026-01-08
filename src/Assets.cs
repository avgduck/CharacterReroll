using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CharacterReroll;

internal static class Assets
{
    internal static Sprite SpriteRandom { get; private set; }
    
    internal static void Init()
    {
        string pathAssets = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), "assets");
        DirectoryInfo assetsDirectory = new DirectoryInfo(pathAssets);

        Dictionary<string, FileInfo> assetFiles = assetsDirectory.GetFiles().ToDictionary(file => file.Name);
        Texture2D texRandom = LoadImageFile(assetFiles["random.png"]);
        SpriteRandom = Sprite.Create(texRandom, new Rect(0, 0, texRandom.width, texRandom.height), new Vector2(0.5f, 0.5f));
    }
    
    private static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[8 * 1024];
        int len;
        while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, len);
        }
    }
    
    internal static Texture2D LoadImageFile(FileInfo file)
    {
        using FileStream fileStream = file.OpenRead();
        using MemoryStream memoryStream = new MemoryStream();
        
        CopyStream(fileStream, memoryStream);
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(memoryStream.ToArray());
        return tex;
    }
}