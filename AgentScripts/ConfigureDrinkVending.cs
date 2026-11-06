using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// One-time migration. Refuses to rebuild an already configured view.
public static class ConfigureDrinkVending
{
    const string Folder = "Assets/Sprites/FastFoodService/DrinkVending/";
    static TMP_FontAsset font;
    // Populated from the exported source manifest.
#pragma warning disable CS0649
    [Serializable] class Manifest { public Atlas[] atlases; public Part[] buttons; public int[] bayBounds; public int[] cupBounds; }
    [Serializable] class Atlas { public string name; public int count; public int columns; public int[] cell; public int[] bounds; }
    [Serializable] class Part { public string name; public int layer; public int[] bounds; }
#pragma warning restore CS0649
    static UnityEngine.Object Read(UnityEngine.Object obj, string name) => new SerializedObject(obj).FindProperty(name).objectReferenceValue;
    static void Set(UnityEngine.Object obj, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(obj); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Array(UnityEngine.Object obj, string name, UnityEngine.Object[] values)
    {
        var so = new SerializedObject(obj); var field = so.FindProperty(name); field.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name + ".png");
    static Sprite[] Frames(string name) => AssetDatabase.LoadAllAssetsAtPath(Folder + name + ".png").OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
    static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }
    static Vector2 Position(float x, float y) => new Vector2(x * 4 - 640, 360 - y * 4);
    static UnityEngine.UI.Image Image(Transform parent, string name, Sprite sprite, int[] bounds)
    {
        var rect = Rect(parent, name, new Vector2((bounds[2]-bounds[0])*4,(bounds[3]-bounds[1])*4), Position((bounds[0]+bounds[2])*.5f,(bounds[1]+bounds[3])*.5f));
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = sprite; image.raycastTarget = false;
        return image;
    }
    static TextMeshProUGUI Text(Transform parent, string name, float x, float y, float w, float h, float size)
    {
        var rect = Rect(parent,name,new Vector2(w*4,h*4),Position(x,y));
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.fontSharedMaterial = font.material;
        text.fontSize = size; text.enableAutoSizing = false; text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1,.95f,.86f); text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }
    static UnityEngine.UI.Button Button(UnityEngine.UI.Image image)
    {
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white; colors.highlightedColor = new Color(1.8f,1.8f,1.8f);
        colors.pressedColor = new Color(.7f,.7f,.7f); colors.selectedColor = Color.white;
        colors.disabledColor = new Color(.65f,.65f,.65f); colors.fadeDuration = .08f;
        button.colors = colors;
        button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        return button;
    }
    static void Import(string path, Atlas atlas)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = atlas == null ? SpriteImportMode.Single : SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100; importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.isReadable = true; importer.alphaIsTransparency = true;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center; settings.spritePivot = Vector2.one*.5f;
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings); importer.SaveAndReimport();
        if (atlas == null) return;
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
        int rows = (atlas.count+atlas.columns-1)/atlas.columns;
        int w = atlas.bounds[2]-atlas.bounds[0], h = atlas.bounds[3]-atlas.bounds[1];
        var rects = Enumerable.Range(0,atlas.count).Select(i =>
        {
            string name = atlas.name+"_"+i.ToString("000");
            return new SpriteRect { name=name, spriteID=old.TryGetValue(name,out var id)?id:GUID.Generate(),
                rect=new Rect(i%atlas.columns*atlas.cell[0],rows*atlas.cell[1]-(i/atlas.columns)*atlas.cell[1]-h,w,h),
                alignment=SpriteAlignment.Center,pivot=Vector2.one*.5f };
        }).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply(); importer.SaveAndReimport();
    }
    public static string RefineButtons()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required");
        var controller=UnityEngine.Object.FindFirstObjectByType<FastFoodDrinkCloseUp>();
        var field=new SerializedObject(controller).FindProperty("_buttons");
        for(int i=0;i<field.arraySize;i++)
        {
            var button=(UnityEngine.UI.Button)field.GetArrayElementAtIndex(i).objectReferenceValue;
            var effect=button.GetComponent<UnityEngine.UI.Outline>()??button.gameObject.AddComponent<UnityEngine.UI.Outline>();
            effect.effectDistance=new Vector2(4,-4); effect.effectColor=new Color(1,.88f,.58f,1); effect.enabled=false;
            var feedback=button.GetComponent<VendingButtonHold>()??button.gameObject.AddComponent<VendingButtonHold>();
            Set(feedback,"_outline",effect);
            var so=new SerializedObject(feedback); so.FindProperty("_repeat").boolValue=i<6; so.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        EditorSceneManager.SaveScene(controller.gameObject.scene);
        return "Seven button outlines configured";
    }
    public static string Apply()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/FastFood.unity") throw new InvalidOperationException("Open FastFood first");
        var view = UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        if (view == null || view.GetComponent<FastFoodDrinkCloseUp>() != null) throw new InvalidOperationException("Missing view or already configured");
        Directory.CreateDirectory("Logs/DrinkVending");
        EditorSceneManager.SaveScene(scene,"Logs/DrinkVending/FastFood.before-configure.unity",true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Folder+"source-layers.json"));
        foreach (string file in Directory.GetFiles(Folder,"*.png"))
            Import(file.Replace('\\','/'),manifest.atlases.FirstOrDefault(a=>a.name==Path.GetFileNameWithoutExtension(file)));
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        if (font == null || font.material.shader.name != "TextMeshPro/Bitmap") throw new InvalidOperationException("GNF bitmap font required");
        var modal=(GameObject)Read(view,"_modal");
        var root=Rect(modal.transform,"DrinkCloseUp",new Vector2(1280,720),Vector2.zero);
        root.anchorMin=Vector2.zero; root.anchorMax=Vector2.one; root.offsetMin=root.offsetMax=Vector2.zero;
        var backdrop=root.gameObject.AddComponent<UnityEngine.UI.Image>(); backdrop.color=Color.black; backdrop.raycastTarget=true;
        var fit=Rect(root,"ArtCanvas",new Vector2(1280,720),Vector2.zero);
        var fitter=fit.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        fitter.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio=16f/9;
        var content=Rect(fit,"SourceCoordinates",new Vector2(1280,720),Vector2.zero);
        content.gameObject.AddComponent<FryerArtScale>();
        var full=new[]{0,0,320,180};
        Image(content,"Background",Sprite("Background"),full);
        var flasks=new UnityEngine.UI.Image[3];
        var atlasNames=new[]{"RedLevels","AmberLevels","GreenLevels"};
        for(int i=0;i<3;i++) flasks[i]=Image(content,atlasNames[i],Frames(atlasNames[i])[0],manifest.atlases.Single(a=>a.name==atlasNames[i]).bounds);
        Image(content,"Machine",Sprite("Machine"),full);
        var bay=Image(content,"DrinkBay",Frames("DrinkAnimation")[0],manifest.bayBounds);
        var controls=Rect(content,"Controls",new Vector2(1280,720),Vector2.zero).gameObject.AddComponent<CanvasGroup>();
        var buttons=new UnityEngine.UI.Button[7];
        for(int i=0;i<7;i++)
        {
            var part=manifest.buttons[i]; buttons[i]=Button(Image(controls.transform,part.name,Sprite(part.name),part.bounds));
            var outlineEffect=buttons[i].gameObject.AddComponent<UnityEngine.UI.Outline>();
            outlineEffect.effectDistance=new Vector2(4,-4); outlineEffect.effectColor=new Color(1,.88f,.58f,1); outlineEffect.enabled=false;
            var feedback=buttons[i].gameObject.AddComponent<VendingButtonHold>(); Set(feedback,"_outline",outlineEffect);
            var feedbackFields=new SerializedObject(feedback); feedbackFields.FindProperty("_repeat").boolValue=i<6; feedbackFields.ApplyModifiedPropertiesWithoutUndo();
        }
        var prepare=Text(buttons[6].transform,"Label",160,90,85,20,40);
        prepare.rectTransform.anchoredPosition=Vector2.zero;
        var values=new TextMeshProUGUI[3];
        float[] xs={104,142.5f,181};
        for(int i=0;i<3;i++) values[i]=Text(content,"Selected"+i,xs[i],105.5f,32,23,36);
        var recipeTitle=Text(content,"RecipeTitle",54,28.5f,43,13,36);
        var recipeValues=new TextMeshProUGUI[3];
        for(int i=0;i<3;i++) recipeValues[i]=Text(content,"Recipe"+i,51,52+i*14.5f,40,12,32);
        var total=Text(content,"TotalVolume",42,106,58,24,18);
        total.alignment=TextAlignmentOptions.Left; total.lineSpacing=-3;
        total.overflowMode=TextOverflowModes.Truncate;
        var drink=Image(content,"DrinkPickup",Sprite("ReadyDrink"),manifest.cupBounds); drink.enabled=false;
        var outline=Image(content,"DrinkOutline",Sprite("DrinkOutline"),full); outline.enabled=false;
        var hint=Text(content,"TakeHint",273,161,91,18,27); hint.gameObject.SetActive(false);
        var warningRect=Rect(content,"RecipeError",new Vector2(520,62),Position(160,92));
        var warning=warningRect.gameObject.AddComponent<CanvasGroup>(); warning.alpha=0; warning.blocksRaycasts=false; warning.interactable=false;
        var warningBg=warningRect.gameObject.AddComponent<UnityEngine.UI.Image>(); warningBg.color=new Color(.07f,.025f,.025f,.96f); warningBg.raycastTarget=false;
        var warningText=Text(warningRect,"Text",160,90,125,15,36); warningText.rectTransform.anchoredPosition=Vector2.zero;
        var closeRect=Rect(root,"CloseDrink",new Vector2(36,36),Vector2.zero);
        closeRect.anchorMin=closeRect.anchorMax=Vector2.one; closeRect.pivot=Vector2.one; closeRect.anchoredPosition=new Vector2(-8,-8);
        var closeImage=closeRect.gameObject.AddComponent<UnityEngine.UI.Image>(); closeImage.color=new Color(.1f,.07f,.08f,.85f);
        var close=Button(closeImage);
        var closeText=Text(closeRect,"Label",160,90,9,9,28); closeText.text="X"; closeText.rectTransform.anchoredPosition=Vector2.zero;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(close.onClick,view.Close);
        var controller=view.gameObject.AddComponent<FastFoodDrinkCloseUp>();
        Set(controller,"_service",Read(view,"_service")); Set(controller,"_player",Read(view,"_player"));
        Set(controller,"_root",root.gameObject); Set(controller,"_canvas",content); Set(controller,"_controls",controls);
        Array(controller,"_buttons",buttons); Array(controller,"_flasks",flasks);
        Array(controller,"_redLevels",Frames("RedLevels")); Array(controller,"_amberLevels",Frames("AmberLevels")); Array(controller,"_greenLevels",Frames("GreenLevels"));
        Set(controller,"_bay",bay); Array(controller,"_drinkFrames",Frames("DrinkAnimation")); Set(controller,"_emptyBay",Sprite("EmptyBay"));
        Set(controller,"_drink",drink); Set(controller,"_outline",outline); Set(controller,"_recipeTitle",recipeTitle);
        Array(controller,"_recipeValues",recipeValues); Array(controller,"_values",values);
        Set(controller,"_total",total); Set(controller,"_prepare",prepare); Set(controller,"_hint",hint);
        Set(controller,"_warning",warning); Set(controller,"_warningText",warningText);
        Set(controller,"_audio",Read(view,"_audio")); Set(controller,"_pourSound",Read(view,"_pourSound")); Set(controller,"_pickupSound",Read(view,"_pickupSound"));
        Set(view,"_drinkCloseUp",controller);
        var inv=new SerializedObject(Read(view,"_inventory")); inv.FindProperty("_orderIcons").GetArrayElementAtIndex(2).objectReferenceValue=Sprite("ReadyDrink"); inv.ApplyModifiedPropertiesWithoutUndo();
        root.gameObject.SetActive(false);
        EditorUtility.SetDirty(view); EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed");
        return "Drink vending configured: 7 buttons, 303 syrup levels, 22 authored drink frames, localized labels and animated pickup";
    }
}
