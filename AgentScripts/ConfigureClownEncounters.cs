using System;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureClownEncounters
{
    const string Art="Assets/Sprites/FastFoodService/Clown/";
    const string Anim="Assets/Animations/FastFoodClown/";
    const string VoiceFolder="Assets/Audio/Voices/ClownGruffStranger";
    const string VoicePath="Assets/Audio/Voices/ClownGruffStranger.asset";
    static UnityEngine.Object Read(UnityEngine.Object obj,string name)=>new SerializedObject(obj).FindProperty(name).objectReferenceValue;
    static void Set(UnityEngine.Object obj,string name,UnityEngine.Object value)
    {var s=new SerializedObject(obj);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static void Number(UnityEngine.Object obj,string name,float value)
    {var s=new SerializedObject(obj);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");
    static AnimationClip Clip(string name,bool footsteps)
    {
        var clip=new AnimationClip {name="Clown"+name,frameRate=5};
        var frames=Enumerable.Range(0,8).Select(i=>new ObjectReferenceKeyframe{time=i*.2f,value=Sprite(name.ToLowerInvariant()+"_"+i.ToString("00"))}).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding{path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"},frames);
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
        if(footsteps)AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{time=.2f,functionName="PlayFootStepSound"},new AnimationEvent{time=1f,functionName="PlayFootStepSound"}});
        AssetDatabase.CreateAsset(clip,Anim+"Clown"+name+".anim");return clip;
    }
    public static string Apply()
    {
        var scene=SceneManager.GetActiveScene();
        if(Application.isPlaying||scene.path!="Assets/Scenes/FastFood.unity")throw new InvalidOperationException("Open FastFood in Edit Mode");
        if(GameObject.Find("FacelessService/ClownEncounterPoints")!=null||File.Exists(VoicePath))throw new InvalidOperationException("Already configured; do not overwrite existing settings");
        Directory.CreateDirectory("Logs/ClownEncounters");EditorSceneManager.SaveScene(scene,"Logs/ClownEncounters/FastFood.before-configure.unity",true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string path in Directory.GetFiles(Art,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteAlignment=(int)(path.EndsWith("SubtitlesClownFastFood.png")?SpriteAlignment.Center:SpriteAlignment.BottomCenter);
            settings.spritePivot=path.EndsWith("SubtitlesClownFastFood.png")?new Vector2(.5f,.5f):new Vector2(.5f,0);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;
            importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        Directory.CreateDirectory(Anim);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var idle=Clip("Idle",false);var walk=Clip("Walk",true);
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Anim+"Clown.controller");
        controller.AddParameter(nameof(AnimatorBool.IsWalking),AnimatorControllerParameterType.Bool);
        var machine=controller.layers[0].stateMachine;var idleState=machine.AddState("Idle");idleState.motion=idle;machine.defaultState=idleState;
        var walkState=machine.AddState("Walk");walkState.motion=walk;
        var toWalk=idleState.AddTransition(walkState);toWalk.hasExitTime=false;toWalk.duration=0;toWalk.AddCondition(AnimatorConditionMode.If,0,nameof(AnimatorBool.IsWalking));
        var toIdle=walkState.AddTransition(idleState);toIdle.hasExitTime=false;toIdle.duration=0;toIdle.AddCondition(AnimatorConditionMode.IfNot,0,nameof(AnimatorBool.IsWalking));
        var profile=ScriptableObject.CreateInstance<DialogueVoiceProfile>();AssetDatabase.CreateAsset(profile,VoicePath);
        var clips=AssetDatabase.FindAssets("t:AudioClip",new[]{VoiceFolder}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
        if(clips.Length!=29)throw new Exception("Expected 29 prepared voice clips");
        foreach(var path in clips)
        {
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);importer.forceToMono=true;importer.loadInBackground=false;
            var sample=importer.defaultSampleSettings;sample.loadType=AudioClipLoadType.DecompressOnLoad;sample.compressionFormat=AudioCompressionFormat.PCM;
            sample.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;sample.preloadAudioData=true;importer.defaultSampleSettings=sample;importer.SaveAndReimport();
        }
        var voice=new SerializedObject(profile);var audioClips=voice.FindProperty("_clips");audioClips.arraySize=clips.Length;
        for(int i=0;i<clips.Length;i++)audioClips.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(clips[i]);
        voice.FindProperty("_volume").floatValue=.16f;voice.FindProperty("_fragmentsPerSecond").floatValue=3;
        voice.FindProperty("_pauseRange").vector2Value=new Vector2(.09f,.2f);voice.FindProperty("_pitchRange").vector2Value=new Vector2(.95f,1.02f);voice.ApplyModifiedPropertiesWithoutUndo();
        var director=UnityEngine.Object.FindFirstObjectByType<FastFoodTension>();
        var clown=(SpriteRenderer)Read(director,"_clown");clown.transform.localScale=Vector3.one*.7f;clown.sprite=Sprite("idle_00");
        clown.sortingLayerName="Characters";clown.sortingOrder=9;clown.color=Color.white;clown.enabled=true;
        var animator=clown.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var actor=clown.gameObject.AddComponent<Npc>();Set(actor,"_spriteRenderer",clown);Set(actor,"_animator",animator);Number(actor,"_speed",.55f);
        var footAudio=clown.gameObject.AddComponent<AudioSource>();footAudio.playOnAwake=false;footAudio.loop=false;footAudio.spatialBlend=0;footAudio.volume=.045f;
        var foot=clown.gameObject.AddComponent<NpcFootstepAudio>();
        var sourceFoot=GameObject.Find("TILL/Admin_FastFood");
        if(sourceFoot==null)sourceFoot=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<NpcFootstepAudio>(true)).First(f=>f.name=="Admin_FastFood").gameObject;
        EditorUtility.CopySerialized(sourceFoot.GetComponent<NpcFootstepAudio>(),foot);Set(foot,"_audioSource",footAudio);Set(foot,"_animator",animator);Number(foot,"_volume",.045f);
        var footFields=new SerializedObject(foot);footFields.FindProperty("_pitchRange").vector2Value=new Vector2(.8f,.9f);footFields.ApplyModifiedPropertiesWithoutUndo();
        var original=GameObject.Find("CAMERA/Main Camera/CanvasDialog/Dialog_Subtitles_Admin");
        var dialogObject=UnityEngine.Object.Instantiate(original,original.transform.parent,false);dialogObject.name="Dialog_Subtitles_Clown";
        Undo.RegisterCreatedObjectUndo(dialogObject,"Create clown dialogue window");
        var dialog=dialogObject.GetComponent<DialogCanvas>();Set(dialog,"_voiceProfile",profile);
        var graphic=dialogObject.AddComponent<GraphicService>();Number(graphic,"_fadeTime",.18f);Set(dialog,"_graphicService",graphic);
        var image=dialogObject.transform.Find("Image_Subtitles").GetComponent<UnityEngine.UI.Image>();image.sprite=Sprite("SubtitlesClownFastFood");image.preserveAspect=true;
        foreach(var ui in dialogObject.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)){var color=ui.color;color.a=0;ui.color=color;ui.raycastTarget=false;}
        var writer=dialogObject.GetComponentInChildren<TypewriterText>(true);Number(writer,"_charactersPerSecond",16);
        var text=writer.GetComponent<TextMeshProUGUI>();text.text="";
        var voiceSource=(AudioSource)Read(writer,"_voiceAudio");voiceSource.clip=null;voiceSource.loop=false;voiceSource.playOnAwake=false;voiceSource.spatialBlend=0;
        var points=new GameObject("ClownEncounterPoints").transform;points.SetParent(director.transform,false);
        float[] xs={-4.8f,-3.3f,-1.8f,-.2f,1.5f,3.2f};var anchors=new Transform[xs.Length];
        for(int i=0;i<xs.Length;i++){anchors[i]=new GameObject("HallPoint"+i).transform;anchors[i].SetParent(points,false);anchors[i].position=new Vector3(xs[i],-.74f,-2);}
        var final=new GameObject("ByCounter").transform;final.SetParent(points,false);final.position=new Vector3(-5.6f,-.74f,-2);
        var fields=new SerializedObject(director);var locations=fields.FindProperty("_hallPoints");locations.arraySize=anchors.Length;
        for(int i=0;i<anchors.Length;i++)locations.GetArrayElementAtIndex(i).objectReferenceValue=anchors[i];fields.ApplyModifiedPropertiesWithoutUndo();
        Set(director,"_route",UnityEngine.Object.FindFirstObjectByType<FastFoodKitchenRoute>());Set(director,"_till",UnityEngine.Object.FindFirstObjectByType<TillTrigger>());
        Set(director,"_actor",actor);Set(director,"_animator",animator);Set(director,"_dialog",dialog);Set(director,"_voice",profile);Set(director,"_finalPoint",final);
        Set(director,"_cameraOffset",UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>().GetComponent<CinemachineCameraOffset>());Set(director,"_gameCamera",Camera.main);
        Set(UnityEngine.Object.FindFirstObjectByType<GuestOrderSequence>(),"_clownEncounters",director);
        ((TextMeshProUGUI)Read(director,"_caption")).gameObject.SetActive(false);((TextMeshProUGUI)Read(director,"_levelLabel")).gameObject.SetActive(false);
        clown.gameObject.SetActive(false);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Configured authored clown (8 idle + 8 walk, run excluded), 29 Gruff Stranger voice clips, original clown subtitles, six hall anchors + counter, staged visits and horizontal camera focus.";
    }
}
