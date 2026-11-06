using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time migration of the open scene, preserving its current manual settings.
public static class ConfigureFridgeCloseUp
{
    const string Folder="Assets/Sprites/FastFoodService/FridgeCloseUp/";
    static TMP_FontAsset font;
#pragma warning disable CS0649
    [Serializable] class Manifest { public Part[] buttons; public int[] potatoBounds; }
    [Serializable] class Part { public string name; public string label; public int[] bounds; }
#pragma warning restore CS0649
    static UnityEngine.Object Read(UnityEngine.Object o,string n)=>new SerializedObject(o).FindProperty(n).objectReferenceValue;
    static void Set(UnityEngine.Object o,string n,UnityEngine.Object v)
    {var so=new SerializedObject(o);so.FindProperty(n).objectReferenceValue=v;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Array(UnityEngine.Object o,string n,UnityEngine.Object[] values)
    {
        var so=new SerializedObject(o);var p=so.FindProperty(n);p.arraySize=values.Length;
        for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Folder+name+".png");
    static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 position)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.sizeDelta=size;r.anchoredPosition=position;return r;
    }
    static Vector2 Position(float x,float y)=>new Vector2(x*4-588,386-y*4);
    static UnityEngine.UI.Image Image(Transform parent,string name,Sprite sprite,int[] b)
    {
        var r=Rect(parent,name,new Vector2((b[2]-b[0])*4,(b[3]-b[1])*4),Position((b[0]+b[2])*.5f,(b[1]+b[3])*.5f));
        var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.raycastTarget=false;return image;
    }
    static TextMeshProUGUI Text(Transform parent,string name,Vector2 size,Vector2 position,float fontSize)
    {
        var text=Rect(parent,name,size,position).gameObject.AddComponent<TextMeshProUGUI>();
        text.font=font;text.fontSharedMaterial=font.material;text.fontSize=fontSize;text.enableAutoSizing=false;
        text.color=new Color(1,.95f,.86f);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
        text.textWrappingMode=TextWrappingModes.NoWrap;text.overflowMode=TextOverflowModes.Truncate;return text;
    }
    static UnityEngine.UI.Button Button(UnityEngine.UI.Image image)
    {
        image.raycastTarget=true;var b=image.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;
        b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
        var colors=b.colors;colors.normalColor=colors.selectedColor=Color.white;
        colors.highlightedColor=new Color(1.4f,1.4f,1.4f);colors.pressedColor=new Color(.7f,.7f,.7f);
        colors.disabledColor=new Color(.65f,.65f,.65f);colors.fadeDuration=.08f;b.colors=colors;
        var outline=image.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=new Color(1,.88f,.58f,1);outline.effectDistance=new Vector2(4,-4);outline.enabled=false;
        var hover=image.gameObject.AddComponent<VendingButtonHold>();Set(hover,"_outline",outline);
        var fields=new SerializedObject(hover);fields.FindProperty("_repeat").boolValue=false;fields.ApplyModifiedPropertiesWithoutUndo();return b;
    }
    static SmoothServiceNotice Notice(TMP_Text label,CanvasGroup group=null)
    {
        if(group==null)
        {
            group=label.GetComponent<CanvasGroup>();
            if(group==null)group=label.gameObject.AddComponent<CanvasGroup>();
        }
        group.alpha=0;group.interactable=false;group.blocksRaycasts=false;
        var notice=group.GetComponent<SmoothServiceNotice>();
        if(notice==null)notice=group.gameObject.AddComponent<SmoothServiceNotice>();
        Set(notice,"_label",label);Set(notice,"_group",group);return notice;
    }
    static SmoothServiceNotice NoticeBox(Transform parent,string name,Vector2 position,Vector2 size,float fontSize)
    {
        var r=Rect(parent,name,size,position);var bg=r.gameObject.AddComponent<UnityEngine.UI.Image>();
        bg.color=new Color(.045f,.025f,.035f,.88f);bg.raycastTarget=false;
        var label=Text(r,"Text",size-new Vector2(16,4),Vector2.zero,fontSize);
        return Notice(label,r.gameObject.AddComponent<CanvasGroup>());
    }
    public static string Apply()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/FastFood.unity")throw new InvalidOperationException("Open FastFood first");
        var view=UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        if(view.GetComponent<FastFoodFridgeCloseUp>()!=null)throw new InvalidOperationException("Already configured; edit existing settings");
        Directory.CreateDirectory("Logs/FridgeCloseUp");EditorSceneManager.SaveScene(scene,"Logs/FridgeCloseUp/FastFood.before-configure.unity",true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string file in Directory.GetFiles(Folder,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.isReadable=true;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteAlignment=(int)SpriteAlignment.Center;settings.spritePivot=Vector2.one*.5f;
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        var manifest=Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Folder+"source-layers.json"));
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        var modal=(GameObject)Read(view,"_modal");
        var root=Rect(modal.transform,"FridgeCloseUp",new Vector2(1280,720),Vector2.zero);
        root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
        var backdrop=root.gameObject.AddComponent<UnityEngine.UI.Image>();backdrop.color=Color.black;backdrop.raycastTarget=true;
        var fit=Rect(root,"ArtCanvas",new Vector2(1176,772),Vector2.zero);
        var fitter=fit.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        fitter.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;fitter.aspectRatio=294f/193;
        var content=Rect(fit,"SourceCoordinates",new Vector2(1176,772),Vector2.zero);content.gameObject.AddComponent<CloseUpArtScale>();
        var full=new[]{0,0,294,193};
        Image(content,"Background",Sprite("Background"),full);
        var fries=Image(content,"Fries",Sprite("Fries"),manifest.potatoBounds);
        var doors=Image(content,"Doors",Sprite("Doors_00"),full);
        var controls=Rect(content,"Keypad",new Vector2(1176,772),Vector2.zero).gameObject.AddComponent<CanvasGroup>();
        var digits=new UnityEngine.UI.Button[10];UnityEngine.UI.Button backspace=null;
        foreach(var part in manifest.buttons)
        {
            var button=Button(Image(controls.transform,part.name,Sprite(part.name),part.bounds));
            if(part.label=="backspace")backspace=button;else digits[int.Parse(part.label)]=button;
        }
        var display=Text(content,"CodeDisplay",new Vector2(232,60),Position(213.5f,48.5f),44);
        display.text="____";display.color=new Color(.9f,1,.79f);
        var pickup=Image(content,"FriesPickup",Sprite("Fries"),manifest.potatoBounds);pickup.enabled=false;
        var outlineImage=Image(content,"FriesOutline",Sprite("FriesOutline"),full);outlineImage.enabled=false;
        var hint=NoticeBox(content,"TakeHint",Position(86,102),new Vector2(640,76),28);
        var warning=NoticeBox(content,"WrongCode",Position(213,176),new Vector2(340,68),32);
        var closeR=Rect(root,"CloseFridge",new Vector2(36,36),Vector2.zero);
        closeR.anchorMin=closeR.anchorMax=Vector2.one;closeR.pivot=Vector2.one;closeR.anchoredPosition=new Vector2(-8,-8);
        var closeImage=closeR.gameObject.AddComponent<UnityEngine.UI.Image>();closeImage.color=new Color(.1f,.07f,.08f,.85f);
        var close=closeR.gameObject.AddComponent<UnityEngine.UI.Button>();close.targetGraphic=closeImage;
        close.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
        Text(closeR,"Label",new Vector2(36,36),Vector2.zero,28).text="X";
        UnityEditor.Events.UnityEventTools.AddPersistentListener(close.onClick,view.Close);
        var controller=view.gameObject.AddComponent<FastFoodFridgeCloseUp>();
        Set(controller,"_service",Read(view,"_service"));Set(controller,"_player",Read(view,"_player"));
        Set(controller,"_root",root.gameObject);Set(controller,"_controls",controls);Array(controller,"_digits",digits);Set(controller,"_backspace",backspace);
        Set(controller,"_display",display);Set(controller,"_doors",doors);
        Array(controller,"_doorFrames",Enumerable.Range(0,17).Select(i=>Sprite("Doors_"+i.ToString("00"))).ToArray());
        Set(controller,"_fries",fries);Set(controller,"_pickup",pickup);Set(controller,"_outline",outlineImage);
        Set(controller,"_hint",hint);Set(controller,"_warning",warning);
        Set(controller,"_audio",Read(view,"_audio"));Set(controller,"_openSound",Read(view,"_fridgeSound"));Set(controller,"_pickupSound",Read(view,"_pickupSound"));
        Set(view,"_fridgeCloseUp",controller);
        return FinishNotices();
    }
    public static string FinishNotices()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
        var view=UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        var controller=view.GetComponent<FastFoodFridgeCloseUp>();
        var scene=controller.gameObject.scene;
        Set(view,"_messageNotice",Notice((TMP_Text)Read(view,"_message")));
        var fryer=view.GetComponent<FastFoodFryerCloseUp>();Set(fryer,"_hintNotice",Notice((TMP_Text)Read(fryer,"_hint")));
        var drink=view.GetComponent<FastFoodDrinkCloseUp>();
        Set(drink,"_warningNotice",Notice((TMP_Text)Read(drink,"_warningText"),(CanvasGroup)Read(drink,"_warning")));
        var drinkHint=(TMP_Text)Read(drink,"_hint");drinkHint.gameObject.SetActive(true);Set(drink,"_hintNotice",Notice(drinkHint));
        var inventory=new SerializedObject(Read(view,"_inventory"));inventory.FindProperty("_orderIcons").GetArrayElementAtIndex(1).objectReferenceValue=Sprite("Fries");inventory.ApplyModifiedPropertiesWithoutUndo();
        ((GameObject)Read(controller,"_root")).SetActive(false);EditorUtility.SetDirty(view);EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene))throw new IOException("Save failed");
        return "Authored fridge configured: 17 door frames, 11 keys, hover pickup, shared smooth kitchen notices";
    }
}
