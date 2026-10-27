#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GeneradorSpritesUI
{
    const string Carpeta = "Assets/UI/Sprites";

    [MenuItem("Tools/EmergencyVR/Generar sprites UI")]
    public static void Generar()
    {
        Directory.CreateDirectory(Carpeta);

        // nombre, ancho, alto, radio, grosor del borde (0 = relleno)
        Crear("panel_relleno", 256, 256, 48, 0);
        Crear("panel_borde",   256, 256, 48, 3);
        Crear("boton_relleno", 256, 128, 64, 0);
        Crear("boton_borde",   256, 128, 64, 3);

        AssetDatabase.Refresh();

        Configurar("panel_relleno", 48);
        Configurar("panel_borde", 48);
        Configurar("boton_relleno", 64);
        Configurar("boton_borde", 64);

        Debug.Log("Sprites UI generados en " + Carpeta);
    }

    static void Crear(string nombre, int w, int h, float radio, float grosor)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pix = new Color32[w * h];
        Vector2 centro = new Vector2(w / 2f, h / 2f);
        Vector2 mitad = new Vector2(w / 2f, h / 2f) - new Vector2(radio, radio);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - centro;
            Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - mitad;
            float d = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude
                      + Mathf.Min(Mathf.Max(q.x, q.y), 0) - radio;

            float a = Mathf.Clamp01(0.5f - d);
            if (grosor > 0)
                a *= Mathf.Clamp01(0.5f + (d + grosor));

            pix[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255f));
        }

        tex.SetPixels32(pix);
        tex.Apply();
        File.WriteAllBytes($"{Carpeta}/{nombre}.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    static void Configurar(string nombre, float borde)
    {
        string ruta = $"{Carpeta}/{nombre}.png";
        var ti = (TextureImporter)AssetImporter.GetAtPath(ruta);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spriteBorder = new Vector4(borde, borde, borde, borde);
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
    }
}
#endif