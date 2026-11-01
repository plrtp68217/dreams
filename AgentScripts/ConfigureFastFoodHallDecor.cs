using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class ConfigureFastFoodHallDecor
{
    const string Art = "Assets/Sprites/Backgrounds/FastFood/Props/HallDecor/";
    const float PosterWidth = .36f;

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        var till = scene.GetRootGameObjects().Single(g => g.name == "TILL");
        if (till.transform.Find("HallDecor") != null)
            throw new InvalidOperationException("HallDecor already exists; adjust it directly.");
        Directory.CreateDirectory("Logs/FastFoodHallDecor");
        EditorSceneManager.SaveScene(scene,"Logs/FastFoodHallDecor/FastFood.before-configure.unity",true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string path in Directory.GetFiles(Art,"*.png")) ImportSprite(path.Replace('\\','/'));
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        var entries = Database();
        var missing = entries.dialogues.Where(d => d.id.StartsWith("fastfood_poster_",StringComparison.Ordinal))
            .SelectMany(d => d.russian + d.english).Where(c => !char.IsWhiteSpace(c) && !font.HasCharacter(c)).Distinct().ToArray();
        if (missing.Length > 0) throw new InvalidOperationException("Poster font lacks: " + new string(missing));
        var material = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true))
            .First(r=>r.name=="FastFoodHall - 6").sharedMaterial;
        var root = new GameObject("HallDecor");
        Undo.RegisterCreatedObjectUndo(root,"Decorate FastFood hall");
        root.transform.SetParent(till.transform,false);
        Prop(root.transform,"PottedPlant",new Vector3(4.55f,-.74f,0),.62f,material);
        Prop(root.transform,"TrashBin",new Vector3(2.95f,-.74f,0),.43f,material);
        Prop(root.transform,"MopBucket",new Vector3(-3.55f,-.74f,0),.6f,material);
        Prop(root.transform,"WetFloorSign",new Vector3(-.9f,-.77f,0),.35f,material);
        Poster(root.transform,"PosterSmile","smile",-3.36f,62,80,material,font,entries);
        Poster(root.transform,"PosterFreeTill","till",-1.76f,51,79,material,font,entries);
        Poster(root.transform,"PosterOrder","order",-.16f,53,87,material,font,entries);
        Poster(root.transform,"PosterSauce","sauce",1.44f,57,92,material,font,entries);
        Poster(root.transform,"PosterHappiness","happiness",3.04f,78,94,material,font,entries);
        Poster(root.transform,"PosterMenu","menu",4.64f,64,91,material,font,entries);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Placed 4 unique floor props and 6 posters with 12 localized TMP labels, on the hall columns.";
    }

    static LocalizedDialogueDatabase Database()=>JsonUtility.FromJson<LocalizedDialogueDatabase>(File.ReadAllText("Assets/Resources/dialogues.json"));
    static string Phrase(LocalizedDialogueDatabase database,string id,Language language)
    {
        var entry=database.dialogues.Single(d=>d.id==id);
        return language==Language.Russian ? entry.russian : entry.english;
    }
    static void ImportSprite(string path)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=100;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteAlignment=(int)SpriteAlignment.BottomCenter;
        settings.spritePivot=new Vector2(.5f,0);
        settings.spriteMeshType=SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;
        importer.filterMode=FilterMode.Point;
        importer.wrapMode=TextureWrapMode.Clamp;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
    static SpriteRenderer Sprite(Transform parent,string name,Vector3 position,Material material)
    {
        var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.position=position;
        var renderer=obj.AddComponent<SpriteRenderer>();
        renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");
        renderer.sharedMaterial=material;renderer.sortingLayerName="Objects";
        return renderer;
    }
    static void Prop(Transform parent,string name,Vector3 position,float height,Material material)
    {
        var renderer=Sprite(parent,name,position,material);
        renderer.transform.localScale=Vector3.one*(height/renderer.sprite.bounds.size.y);
        renderer.sortingOrder=7;
    }
    static void Poster(Transform parent,string name,string key,float x,float titleTop,float bodyTop,
        Material material,TMP_FontAsset font,LocalizedDialogueDatabase database)
    {
        var renderer=Sprite(parent,name,new Vector3(x,-.10f,0),material);
        renderer.sortingOrder=0;
        renderer.transform.localScale=Vector3.one*(PosterWidth/renderer.sprite.bounds.size.x);
        float width=renderer.sprite.rect.width;
        float height=renderer.sprite.rect.height*81/width;
        var canvasObject=new GameObject("Lettering",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
        var rect=(RectTransform)canvasObject.transform;rect.SetParent(renderer.transform,false);
        rect.pivot=new Vector2(.5f,0);rect.sizeDelta=new Vector2(81,height);
        rect.localScale=Vector3.one*(width/8100f);rect.localPosition=new Vector3(0,0,-.01f);
        var canvas=canvasObject.GetComponent<Canvas>();
        canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=Camera.main;
        canvas.overrideSorting=true;canvas.sortingLayerName="Objects";canvas.sortingOrder=1;
        var scaler=canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.dynamicPixelsPerUnit=1;scaler.referencePixelsPerUnit=100;
        Color ink=key=="order" ? new Color(.11f,.07f,.035f) : new Color(.77f,.53f,.26f);
        var title=Label(rect,"Title",titleTop,bodyTop-titleTop-3,8,font,ink);
        var body=Label(rect,"Body",bodyTop,height-bodyTop-7,6,font,ink);
        string titleId="fastfood_poster_"+key+"_title", bodyId="fastfood_poster_"+key+"_body";
        title.text=Phrase(database,titleId,Language.Russian);body.text=Phrase(database,bodyId,Language.Russian);
        var localized=renderer.gameObject.AddComponent<LocalizedPosterText>();
        var fields=new SerializedObject(localized);
        fields.FindProperty("_title").objectReferenceValue=title;fields.FindProperty("_body").objectReferenceValue=body;
        fields.FindProperty("_titleId").stringValue=titleId;fields.FindProperty("_bodyId").stringValue=bodyId;
        fields.ApplyModifiedPropertiesWithoutUndo();
        title.ForceMeshUpdate(true);body.ForceMeshUpdate(true);
    }
    static TextMeshProUGUI Label(RectTransform parent,string name,float top,float height,float size,TMP_FontAsset font,Color color)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
        var rect=(RectTransform)obj.transform;rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);
        rect.anchoredPosition=new Vector2(0,-top);rect.sizeDelta=new Vector2(69,height);
        var text=obj.GetComponent<TextMeshProUGUI>();text.font=font;text.fontSharedMaterial=font.material;
        text.fontSize=size;text.enableAutoSizing=false;text.textWrappingMode=TextWrappingModes.Normal;
        text.alignment=TextAlignmentOptions.TopLeft;text.overflowMode=TextOverflowModes.Overflow;
        text.color=color;text.raycastTarget=false;text.richText=false;
        return text;
    }

    public static string PreviewLanguage(string language)
    {
        var selected=(Language)Enum.Parse(typeof(Language),language);
        var data=Database();int count=0;
        foreach(var label in UnityEngine.Object.FindObjectsByType<LocalizedPosterText>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var fields=new SerializedObject(label);
            foreach(string part in new[]{"title","body"})
            {
                var text=(TMP_Text)fields.FindProperty("_"+part).objectReferenceValue;
                text.text=Phrase(data,fields.FindProperty("_"+part+"Id").stringValue,selected);
                text.ForceMeshUpdate(true);count++;
            }
        }
        return "Previewed "+count+" labels in "+language;
    }

    public static string Capture(string name,float x)
    {
        var original=Camera.main;
        var obj=new GameObject("Temporary hall decor camera");
        var target=new RenderTexture(1600,900,24);var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try
        {
            var camera=obj.AddComponent<Camera>();camera.CopyFrom(original);camera.enabled=false;
            camera.transform.SetPositionAndRotation(new Vector3(x,original.transform.position.y,original.transform.position.z),original.transform.rotation);
            camera.ResetWorldToCameraMatrix();camera.ResetCullingMatrix();camera.ResetProjectionMatrix();
            EditorUtility.CopySerialized(original.GetComponent<UniversalAdditionalCameraData>(),obj.AddComponent<UniversalAdditionalCameraData>());
            camera.targetTexture=target;camera.aspect=1600f/900;camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();
            string path="Logs/FastFoodHallDecor/"+name+".png";File.WriteAllBytes(path,pixels.EncodeToPNG());return path;
        }
        finally
        {
            RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(pixels);target.Release();
            UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
