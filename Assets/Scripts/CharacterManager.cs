using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public RuntimeAnimatorController Ani;
    public string mainJson,url, texture;
    public GameObject characterContent,characterPrefab;

    IEnumerator Start()
    {
        UnityWebRequest www = UnityWebRequest.Get(mainJson);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            var test = JSON.Parse(www.downloadHandler.text);
            print(test.Count);
            print(test["characters"].Count);
            int indexer = 0;
            foreach (var to in test["characters"])
            {
                var characterNew = Instantiate(characterPrefab);
                characterNew.transform.SetParent(characterContent.transform);
                characterNew.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
                characterNew.GetComponent<RectTransform>().anchoredPosition = new Vector2(indexer * 220 + 180, 0);
                characterNew.transform.Find("text").GetComponent<TextMeshProUGUI>().text = to.Key;
                characterNew.name = to.Key;
                characterNew.GetComponent<Button>().onClick.AddListener(() =>
                {
                    StartCoroutine(DownloadCor(to.Value));
                });
                print(to.Value);
                indexer++;
            }
        }

       
    }
    public void DownloadCharacter()
    {
        StartCoroutine(DownCor(url, texture));
    }
    AssetBundle bundle;
    AssetBundle textureBundle;
    GameObject selectedObj;
    IEnumerator DownloadCor(JSONNode jSON)
    {
        if (bundle != null)
        {
            bundle.Unload(false);
        }
        if (textureBundle != null)
        {
            textureBundle.Unload(false);
        }

        var uwr = UnityWebRequestAssetBundle.GetAssetBundle(jSON["prefaburl"]);
        yield return uwr.SendWebRequest();

        // Get an asset from the bundle and instantiate it.
        bundle = DownloadHandlerAssetBundle.GetContent(uwr);
        print(bundle.name);
        var ast = bundle.LoadAsset<GameObject>(bundle.name);
        if (selectedObj != null)
            Destroy(selectedObj);

        selectedObj = Instantiate(ast, new Vector3(0, 0, 0), Quaternion.identity);
        selectedObj.GetComponent<Animator>().runtimeAnimatorController = Ani;
        print(selectedObj.name);

        var uwrTexture = UnityWebRequestAssetBundle.GetAssetBundle(jSON["textureurl"]);
        yield return uwrTexture.SendWebRequest();

        textureBundle = DownloadHandlerAssetBundle.GetContent(uwrTexture);


        //var modelRenderer = gameobj.transform.Find(jSON["prefabobj"] + " (Instance)").gameObject.GetComponent<SkinnedMeshRenderer>();
        var modelRenderer = selectedObj.transform.Find(jSON["prefabobj"]).gameObject.GetComponent<SkinnedMeshRenderer>();
        var materialList = modelRenderer.materials.ToList();

       // var testmat = materialList.Find(item => item.name == "asd");
        foreach (var matt in modelRenderer.materials)
        {
            matt.EnableKeyword("_MainTex");
            matt.EnableKeyword("_NORMALMAP");
        }

        foreach (var texture in jSON["textures"])
        {
            if (texture.Value["texturetype"] == "diffuse")
            {
                var tempmat = materialList.Find(item => item.name == texture.Value["texturename"] + " (Instance)");
                foreach (var mm in materialList)
                {
                    print(texture.Value["texturename"] + "  ---  " + mm);
                }
                print(tempmat);
                var tempText = textureBundle.LoadAssetAsync<Texture>(texture.Value["textureaddress"]);
                yield return tempText;
                print(tempText);
                tempmat.SetTexture("_MainTex", tempText.asset as Texture);
            }
            else if (texture.Value["texturetype"] == "normal")
            {
                var tempmat = materialList.Find(item => item.name == texture.Value["texturename"] + " (Instance)");
                foreach (var mm in materialList)
                {
                    print(texture.Value["texturename"] + " (Instance)" + "  ---  " + mm + (texture.Value["texturename"] + " (Instance)").Equals(mm));
                }
                print(tempmat);
                //var tempmat = materialList.Find(item => item.name == texture.Value["texturename"]);
                var tempText = textureBundle.LoadAssetAsync<Texture>(texture.Value["textureaddress"]);
                yield return tempText;
                tempmat.SetTexture("_NORMALMAP", tempText.asset as Texture);
            }
        }
       /* var mainTex = textureBundle.LoadAssetAsync<Texture>("Cooper_Body_Diff");
        yield return mainTex;
        modelRenderer.materials[1].SetTexture("_MainTex", mainTex.asset as Texture);

        var normTex = textureBundle.LoadAssetAsync<Texture>("Cooper_Body_Normal");
        yield return normTex;
        modelRenderer.materials[2].SetTexture("_NORMALMAP", normTex.asset as Texture);*/
    }

    IEnumerator DownCor(string url, string urlTexture)
    {
        var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return uwr.SendWebRequest();

        // Get an asset from the bundle and instantiate it.
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
        print(bundle.name);
        var ast = bundle.LoadAsset<GameObject>(bundle.name);

        var gameobj = Instantiate(ast, new Vector3(0, 0, 0), Quaternion.identity);
        gameobj.GetComponent<Animator>().runtimeAnimatorController = Ani;
        print(gameobj.name);
        
        var uwrTexture = UnityWebRequestAssetBundle.GetAssetBundle(urlTexture);
        yield return uwrTexture.SendWebRequest();

        AssetBundle textureBundle = DownloadHandlerAssetBundle.GetContent(uwrTexture);

        
        
        var modelRenderer = gameobj.transform.Find("CHAR_Cooper").gameObject.GetComponent<SkinnedMeshRenderer>();
        var materialList = modelRenderer.materials.ToList();

        var testmat = materialList.Find(item => item.name == "asd");
        foreach (var matt in modelRenderer.materials)
        {
            matt.EnableKeyword("_MainTex");
            matt.EnableKeyword("_NORMALMAP");
        }
        var mainTex = textureBundle.LoadAssetAsync<Texture>("Cooper_Body_Diff");
        yield return mainTex;
        modelRenderer.materials[1].SetTexture("_MainTex", mainTex.asset as Texture);

        var normTex = textureBundle.LoadAssetAsync<Texture>("Cooper_Body_Normal");
        yield return normTex;
        modelRenderer.materials[2].SetTexture("_NORMALMAP", normTex.asset as Texture);
    }
}
