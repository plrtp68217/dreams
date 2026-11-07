using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Applies only the FastFood scene. Run once; later Inspector adjustments are preserved.
public static class ConfigureFastFoodHud
{
    static TMP_FontAsset font;
    static readonly Color Ink = new(.055f,.050f,.070f,1), Cream = new(.89f,.85f,.74f), Gold = new(.87f,.67f,.27f);
    static T Read<T>(UnityEngine.Object o, string field) where T:UnityEngine.Object => (T)new SerializedObject(o).FindProperty(field).objectReferenceValue;
    static void Set(UnityEngine.Object o,string field,UnityEngine.Object value) { var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Value(UnityEngine.Object o,string field,string value) { var s=new SerializedObject(o);s.FindProperty(field).stringValue=value;s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Flag(UnityEngine.Object o,string field,bool value) { var s=new SerializedObject(o);s.FindProperty(field).boolValue=value;s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Number(UnityEngine.Object o,string field,float value) { var s=new SerializedObject(o);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo(); }
    static void Array(UnityEngine.Object o,string field,UnityEngine.Object[] values) { var s=new SerializedObject(o);var p=s.FindProperty(field);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo(); }
    static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos) { var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=(RectTransform)g.transform;r.sizeDelta=size;r.anchoredPosition=pos;return r; }
    static void Place(RectTransform r,Vector2 anchor,Vector2 pivot,Vector2 size,Vector2 pos) {r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.sizeDelta=size;r.anchoredPosition=pos;r.localScale=Vector3.one;r.localRotation=Quaternion.identity;}
    static void Stretch(RectTransform r,float inset=0) {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*inset;r.offsetMax=-Vector2.one*inset;}
    static UnityEngine.UI.Image Image(Transform parent,string name,Color color,Vector2 size,Vector2 pos) {var i=Rect(parent,name,size,pos).gameObject.AddComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=false;return i;}
    static TextMeshProUGUI Text(Transform parent,string name,float size) {var t=Rect(parent,name,new Vector2(200,32),Vector2.zero).gameObject.AddComponent<TextMeshProUGUI>();Style(t,size);return t;}
    static void Style(TMP_Text t,float size) {t.font=font;t.fontSharedMaterial=font.material;t.fontSize=size;t.enableAutoSizing=false;t.color=Cream;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Truncate;t.alignment=TextAlignmentOptions.Left;}
    static void Frame(UnityEngine.UI.Image border,Color edge,Color inside,float inset) {border.sprite=null;border.type=UnityEngine.UI.Image.Type.Simple;border.color=edge;var fill=Image(border.transform,"Inset",inside,Vector2.zero,Vector2.zero);Stretch(fill.rectTransform,inset);fill.transform.SetAsFirstSibling();}
    static FastFoodKeyPrompt Prompt(Transform host,TMP_Text source,UnityEngine.UI.Image visibility,string key="E",string caption=null)
    {
        var component=host.gameObject.AddComponent<FastFoodKeyPrompt>();
        var panel=Image(host,"KeyPrompt",new Color(.38f,.33f,.27f),new Vector2(300,44),Vector2.zero);
        Frame(panel,new Color(.38f,.33f,.27f),Ink,2);
        var group=panel.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.interactable=group.blocksRaycasts=false;
        var cap=Image(panel.transform,"Keycap",Cream,new Vector2(34,30),Vector2.zero);
        Place(cap.rectTransform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(34,30),new Vector2(8,0));
        Frame(cap,Cream,new Color(.14f,.13f,.16f),2);
        var letter=Text(cap.transform,"Key",18);Stretch(letter.rectTransform);letter.alignment=TextAlignmentOptions.Center;letter.text=key;
        var label=Text(panel.transform,"Action",18);Stretch(label.rectTransform,4);label.rectTransform.offsetMin=new Vector2(60,4);label.rectTransform.offsetMax=new Vector2(-12,-4);
        Set(component,"_source",source);Set(component,"_visibility",visibility);Set(component,"_caption",label);Set(component,"_key",letter);Set(component,"_panel",panel.rectTransform);Set(component,"_keycap",cap.rectTransform);Set(component,"_group",group);Value(component,"_fallbackKey",key);Value(component,"_captionId",caption??"");
        if(source!=null)source.enabled=false;if(visibility!=null)visibility.enabled=false;
        return component;
    }
    public static string Apply()
    {
        var scene=SceneManager.GetActiveScene();
        if(Application.isPlaying||scene.path!="Assets/Scenes/FastFood.unity")throw new InvalidOperationException("Open FastFood in Edit Mode");
        var view=UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        if(view.GetComponent<FastFoodNotificationHub>()!=null)throw new InvalidOperationException("Already configured. Edit the existing UI.");
        Directory.CreateDirectory("Logs/InventoryUI");EditorSceneManager.SaveScene(scene,"Logs/InventoryUI/FastFood.before-apply.unity",true);
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        var canvas=Read<GameObject>(view,"_hud").transform.parent;
        var inventory=Read<FastFoodInventory>(view,"_inventory");
        var hud=Read<GameObject>(view,"_hud").transform;
        var tray=hud.Find("Tray").GetComponent<UnityEngine.UI.Image>();
        Place(tray.rectTransform,Vector2.zero,Vector2.zero,new Vector2(328,80),new Vector2(24,24));
        Frame(tray,new Color(.55f,.18f,.16f),new Color(.27f,.055f,.067f,.97f),4);
        var title=Read<TextMeshProUGUI>(view,"_trayTitle");Style(title,14);Place(title.rectTransform,new Vector2(0,1),new Vector2(0,0),new Vector2(328,24),new Vector2(0,4));
        for(int i=0;i<4;i++)
        {
            var slot=tray.transform.Find("Slot"+(i+1));
            Place((RectTransform)slot,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(74,64),new Vector2(10+i*78,0));
            var button=slot.GetComponent<UnityEngine.UI.Button>();
            Frame(slot.GetComponent<UnityEngine.UI.Image>(),new Color(.38f,.16f,.17f),new Color(.11f,.065f,.08f),2);
            var colors=button.colors;colors.normalColor=colors.selectedColor=Color.white;colors.disabledColor=Color.white;colors.highlightedColor=new Color(1.3f,1.2f,1.1f);colors.pressedColor=new Color(.8f,.7f,.6f);button.colors=colors;
            var icon=slot.Find("Icon").GetComponent<UnityEngine.UI.Image>();icon.preserveAspect=true;Place(icon.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(58,48),new Vector2(0,-3));
            var number=slot.Find("SlotLabel").GetComponent<TMP_Text>();Style(number,12);Place(number.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(20,18),new Vector2(5,-2));
        }
        var pocket=hud.Find("Pocket").GetComponent<UnityEngine.UI.Image>();Place(pocket.rectTransform,new Vector2(1,0),new Vector2(1,0),new Vector2(184,64),new Vector2(-24,24));Frame(pocket,new Color(.29f,.28f,.30f),Ink,3);
        var pocketTitle=Read<TextMeshProUGUI>(view,"_pocketTitle");Style(pocketTitle,14);Place(pocketTitle.rectTransform,new Vector2(1,1),new Vector2(1,0),new Vector2(184,24),new Vector2(0,4));pocketTitle.alignment=TextAlignmentOptions.Right;
        var record=pocket.transform.Find("MedicalRecordSlot");Place((RectTransform)record,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(54,52),new Vector2(8,0));Frame(record.GetComponent<UnityEngine.UI.Image>(),Gold,new Color(.10f,.10f,.13f),2);
        var recordIcon=Read<UnityEngine.UI.Image>(view,"_pocketIcon");recordIcon.preserveAspect=true;Place(recordIcon.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(40,38),Vector2.zero);
        for(int i=1;i<3;i++){var slot=Image(pocket.transform,"EmptySlot"+i,new Color(.25f,.24f,.28f),new Vector2(54,52),Vector2.zero);Place(slot.rectTransform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(54,52),new Vector2(8+58*i,0));Frame(slot,slot.color,new Color(.10f,.10f,.13f),2);}
        var ready=Read<TextMeshProUGUI>(view,"_ready");ready.gameObject.SetActive(false);

        var hub=view.gameObject.AddComponent<FastFoodNotificationHub>();
        var card=Image(canvas,"NotificationCard",Gold,new Vector2(368,82),Vector2.zero);Place(card.rectTransform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(368,82),new Vector2(0,-32));Frame(card,Gold,Ink,2);
        var shadow=card.gameObject.AddComponent<UnityEngine.UI.Shadow>();shadow.effectColor=new Color(0,0,0,.5f);shadow.effectDistance=new Vector2(0,-6);shadow.useGraphicAlpha=true;
        var cardGroup=card.gameObject.AddComponent<CanvasGroup>();cardGroup.alpha=0;cardGroup.interactable=cardGroup.blocksRaycasts=false;
        var symbol=Image(card.transform,"Symbol",Gold,new Vector2(46,46),new Vector2(-142,-1));Frame(symbol,Gold,new Color(.48f,.10f,.11f),2);
        var symbolText=Text(symbol.transform,"Mark",30);Stretch(symbolText.rectTransform);symbolText.alignment=TextAlignmentOptions.Center;symbolText.text="+";
        var tick=Rect(symbol.transform,"Success",new Vector2(36,36),Vector2.zero);
        var shortStroke=Image(tick,"ShortStroke",Cream,new Vector2(13,4),new Vector2(-8,-3));shortStroke.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
        var longStroke=Image(tick,"LongStroke",Cream,new Vector2(25,4),new Vector2(4,2));longStroke.rectTransform.localRotation=Quaternion.Euler(0,0,45);tick.gameObject.SetActive(false);Set(hub,"_successIcon",tick.gameObject);
        var heading=Text(card.transform,"Title",18);Place(heading.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(264,26),new Vector2(80,-12));heading.color=Gold;
        var body=Text(card.transform,"Body",14);Place(body.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(264,28),new Vector2(80,-39));body.textWrappingMode=TextWrappingModes.Normal;
        var track=Image(card.transform,"LifetimeTrack",new Color(.26f,.22f,.16f),Vector2.zero,Vector2.zero);Place(track.rectTransform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(356,3),new Vector2(0,5));
        var progress=Image(track.transform,"Remaining",Gold,Vector2.zero,Vector2.zero);Stretch(progress.rectTransform);progress.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");progress.type=UnityEngine.UI.Image.Type.Filled;progress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;progress.fillOrigin=0;
        Set(hub,"_service",Read<FacelessOrderService>(view,"_service"));Set(hub,"_inventory",inventory);Set(hub,"_group",cardGroup);Set(hub,"_card",card.rectTransform);Set(hub,"_symbol",symbol.rectTransform);Set(hub,"_title",heading);Set(hub,"_body",body);Set(hub,"_symbolText",symbolText);Set(hub,"_accent",symbol);Set(hub,"_progress",progress);
        foreach(var notice in UnityEngine.Object.FindObjectsByType<SmoothServiceNotice>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            bool toast=notice.name=="Message"||notice.name=="RecipeError"||notice.name=="WrongCode";
            if(toast){Set(notice,"_notification",hub);Number(notice,"_notificationDuration",notice.name=="WrongCode"?2:notice.name=="RecipeError"?1.8f:3.5f);continue;}
            var label=Read<TMP_Text>(notice,"_label");
            var closeup=notice.transform;while(closeup.parent!=null&&!closeup.name.EndsWith("CloseUp"))closeup=closeup.parent;
            if(closeup==notice.transform)throw new Exception("Cannot find close-up for "+notice.name);
            notice.transform.SetParent(closeup,false);Place((RectTransform)notice.transform,new Vector2(.5f,0),new Vector2(.5f,.5f),new Vector2(660,44),new Vector2(0,42));
            var oldBg=notice.GetComponent<UnityEngine.UI.Image>();if(oldBg!=null)oldBg.enabled=false;
            Prompt(notice.transform,label,null,"");
        }
        var fryerBg=canvas.Find("Interaction/FryerCloseUp/HintBackground");if(fryerBg!=null)fryerBg.gameObject.SetActive(false);
        var prompt=Read<TextMeshProUGUI>(view,"_prompt");Place(prompt.rectTransform,new Vector2(.5f,0),new Vector2(.5f,.5f),new Vector2(660,44),new Vector2(0,148));Prompt(prompt.transform,prompt,null);prompt.gameObject.SetActive(false);
        var route=UnityEngine.Object.FindFirstObjectByType<FastFoodKitchenRoute>();var routeHint=Read<CanvasGroup>(route,"_hint");
        var routeScaler=routeHint.GetComponentInParent<UnityEngine.UI.CanvasScaler>();routeScaler.referenceResolution=new Vector2(1280,720);
        Place((RectTransform)routeHint.transform,new Vector2(.5f,0),new Vector2(.5f,.5f),new Vector2(660,44),new Vector2(0,148));routeHint.GetComponent<UnityEngine.UI.Image>().enabled=false;
        var routePrompt=Prompt(routeHint.transform,Read<TextMeshProUGUI>(route,"_hintText"),null,"A");Set(routePrompt,"_view",view);Set(routePrompt,"_input",Read<InputService>(view,"_input"));Flag(routePrompt,"_routeHint",true);

        var dialogCanvas=GameObject.Find("CAMERA/Main Camera/CanvasDialog").GetComponent<Canvas>();
        dialogCanvas.renderMode=RenderMode.ScreenSpaceOverlay;dialogCanvas.sortingOrder=50;
        var scaler=dialogCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        var arts=new System.Collections.Generic.List<RectTransform>();var keys=new System.Collections.Generic.List<RectTransform>();
        foreach(Transform dialog in dialogCanvas.transform)
        {
            Stretch((RectTransform)dialog);dialog.localScale=Vector3.one;
            var art=dialog.Find("Image_Subtitles") as RectTransform;
            if(art!=null)
            {
                Place(art,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(640,234),new Vector2(72,24));
                var t=art.GetComponentInChildren<TMP_Text>(true);t.enableAutoSizing=false;t.fontSize=22;
                var key=dialog.Find("ImageButtonE").GetComponent<UnityEngine.UI.Image>();Place(key.rectTransform,new Vector2(.5f,0),new Vector2(.5f,.5f),new Vector2(90,44),new Vector2(308,42));
                var animator=key.GetComponent<Animator>();if(animator!=null)animator.enabled=false;
                Prompt(key.transform,null,key,"E","service_continue");arts.Add(art);keys.Add(key.rectTransform);
            }
        }
        var order=dialogCanvas.transform.Find("Dialog_Order/Image_Order").GetComponent<UnityEngine.UI.Image>();Place(order.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(216,366),new Vector2(24,-16));
        Number(order.GetComponent<ReceiptHover>(),"_collapsedScale",.78f);
        var receiptText=order.GetComponentInChildren<TMP_Text>(true);receiptText.enableAutoSizing=false;receiptText.fontSize=14;receiptText.textWrappingMode=TextWrappingModes.Normal;
        var note=dialogCanvas.transform.Find("Dialog_Note/Image_Note") as RectTransform;Place(note,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(358,480),Vector2.zero);
        var noteKey=dialogCanvas.transform.Find("Dialog_Note/ImageButtonE").GetComponent<UnityEngine.UI.Image>();Place(noteKey.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(90,44),new Vector2(130,-215));var noteAnimator=noteKey.GetComponent<Animator>();if(noteAnimator!=null)noteAnimator.enabled=false;Prompt(noteKey.transform,null,noteKey,"E","service_close");
        var layout=view.gameObject.AddComponent<FastFoodHudLayout>();Set(layout,"_canvas",dialogCanvas.GetComponent<RectTransform>());Set(layout,"_inventory",inventory);Set(layout,"_trayBorder",tray);Array(layout,"_dialogueArt",arts.Cast<UnityEngine.Object>().ToArray());Array(layout,"_dialogueKeys",keys.Cast<UnityEngine.Object>().ToArray());
        ConfigureModalLayer(view,layout,dialogCanvas);
        var playerDialog=GameObject.Find("Player/Canvas/DialogUI_Player_E").GetComponent<DialogUI>();
        var image=playerDialog.GetComponentInChildren<UnityEngine.UI.Image>(true);
        var worldHint=Rect(canvas,"PlayerInteractionPrompt",new Vector2(660,44),new Vector2(0,148));Place(worldHint,new Vector2(.5f,0),new Vector2(.5f,.5f),new Vector2(660,44),new Vector2(0,148));Set(Prompt(worldHint,null,image,"E","service_interact"),"_view",view);
        card.transform.SetAsLastSibling();
        foreach(var c in dialogCanvas.GetComponentsInChildren<Component>(true))
        {EditorUtility.SetDirty(c);if(PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        PrefabUtility.RecordPrefabInstancePropertyModifications(image);
        PrefabUtility.RecordPrefabInstancePropertyModifications(dialogCanvas);PrefabUtility.RecordPrefabInstancePropertyModifications(scaler);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Configured compact HUD, left receipt, four visible items, shared key prompts, adaptive dialogue and notification card. Saved current FastFood including prior manual edits.";
    }
    static void ConfigureModalLayer(FastFoodServiceView view,FastFoodHudLayout layout,Canvas dialogCanvas)
    {
        var group=dialogCanvas.GetComponent<CanvasGroup>();if(group==null)group=dialogCanvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha=1;group.blocksRaycasts=group.interactable=true;
        Set(layout,"_view",view);Set(layout,"_dialogueGroup",group);Number(layout,"_dialogueScale",.75f);
        var receipt=dialogCanvas.transform.Find("Dialog_Order/Image_Order").GetComponent<UnityEngine.UI.Image>();
        foreach(var graphic in dialogCanvas.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
        {
            if(graphic==receipt)continue;
            graphic.raycastTarget=false;
            if(PrefabUtility.IsPartOfPrefabInstance(graphic))PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
        }
    }
    public static string FixModalInput()
    {
        var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/FastFood.unity")throw new Exception("FastFood Edit Mode required");
        var view=UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();var layout=view.GetComponent<FastFoodHudLayout>();
        var dialogCanvas=GameObject.Find("CAMERA/Main Camera/CanvasDialog").GetComponent<Canvas>();
        ConfigureModalLayer(view,layout,dialogCanvas);
        foreach(Transform dialog in dialogCanvas.transform)
        {
            var art=dialog.Find("Image_Subtitles");if(art==null)continue;art.localScale=Vector3.one*.75f;
            var key=(RectTransform)dialog.Find("ImageButtonE");key.anchoredPosition=new Vector2(72+236*.75f,46);
            if(PrefabUtility.IsPartOfPrefabInstance(art))PrefabUtility.RecordPrefabInstancePropertyModifications(art);
            if(PrefabUtility.IsPartOfPrefabInstance(key))PrefabUtility.RecordPrefabInstancePropertyModifications(key);
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Hidden non-interactive graphics no longer intercept input; dialogue layer hides during close-ups; dialogue art scaled to 75%.";
    }
    public static string Refine()
    {
        var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/FastFood.unity")throw new Exception("FastFood Edit Mode required");
        var hub=UnityEngine.Object.FindFirstObjectByType<FastFoodNotificationHub>();if(hub==null)throw new Exception("Apply first");
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        var card=Read<RectTransform>(hub,"_card");card.Find("Inset").GetComponent<UnityEngine.UI.Image>().color=Ink;
        var symbol=Read<RectTransform>(hub,"_symbol");var tick=symbol.Find("Success") as RectTransform;
        if(tick==null)
        {
            tick=Rect(symbol,"Success",new Vector2(36,36),Vector2.zero);
            var a=Image(tick,"ShortStroke",Cream,new Vector2(13,4),new Vector2(-8,-3));a.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
            var b=Image(tick,"LongStroke",Cream,new Vector2(25,4),new Vector2(4,2));b.rectTransform.localRotation=Quaternion.Euler(0,0,45);tick.gameObject.SetActive(false);
        }
        Set(hub,"_successIcon",tick.gameObject);
        var view=hub.GetComponent<FastFoodServiceView>();
        foreach(var prompt in UnityEngine.Object.FindObjectsByType<FastFoodKeyPrompt>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            Read<RectTransform>(prompt,"_panel").Find("Inset").GetComponent<UnityEngine.UI.Image>().color=Ink;
            if(prompt.transform.parent.name=="RouteCanvas")Set(prompt,"_input",Read<InputService>(view,"_input"));
            if(prompt.name=="PlayerInteractionPrompt")Set(prompt,"_view",view);
            if(PrefabUtility.IsPartOfPrefabInstance(prompt))PrefabUtility.RecordPrefabInstancePropertyModifications(prompt);
        }
        var receipt=GameObject.Find("CAMERA/Main Camera/CanvasDialog/Dialog_Order/Image_Order").GetComponent<ReceiptHover>();Number(receipt,"_collapsedScale",.78f);receipt.GetComponentInChildren<TMP_Text>(true).fontSize=14;
        foreach(var c in receipt.GetComponentsInChildren<Component>(true))if(PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.ImportAsset("Assets/Resources/dialogues.json",ImportAssetOptions.ForceUpdate);return "Refined receipt readability, opaque dark cards and success icon.";
    }
}
