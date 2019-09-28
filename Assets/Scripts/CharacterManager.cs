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
    public string mainJson;
    public GameObject characterContent,characterPrefab,danceContent,dancePrefab;
    

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
            var test = JSON.Parse(www.downloadHandler.text);
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
    
    AssetBundle bundle;
    AssetBundle textureBundle;
    AssetBundle danceBundle;
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

        if (danceBundle != null)
        {
            danceBundle.Unload(false);
        }

        var uwr = UnityWebRequestAssetBundle.GetAssetBundle(jSON["prefaburl"]);
        yield return uwr.SendWebRequest();
        bundle = DownloadHandlerAssetBundle.GetContent(uwr);

        var ast = bundle.LoadAsset<GameObject>(bundle.name);

        if (selectedObj != null)
            Destroy(selectedObj);

        selectedObj = Instantiate(ast, new Vector3(0, 0, 0), Quaternion.identity);
        selectedObj.GetComponent<Animator>().runtimeAnimatorController = Ani;
        var uwrTexture = UnityWebRequestAssetBundle.GetAssetBundle(jSON["textureurl"]);
        yield return uwrTexture.SendWebRequest();
        textureBundle = DownloadHandlerAssetBundle.GetContent(uwrTexture);

        
        var modelRenderer = selectedObj.transform.Find(jSON["prefabobj"]).gameObject.GetComponent<SkinnedMeshRenderer>();
        var materialList = modelRenderer.materials.ToList();
        
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
                var tempText = textureBundle.LoadAssetAsync<Texture>(texture.Value["textureaddress"]);
                yield return tempText;
                tempmat.SetTexture("_MainTex", tempText.asset as Texture);
            }
            else if (texture.Value["texturetype"] == "normal")
            {
                var tempmat = materialList.Find(item => item.name == texture.Value["texturename"] + " (Instance)");
                foreach (var mm in materialList)
                {
                    print(texture.Value["texturename"] + " (Instance)" + "  ---  " + mm + (texture.Value["texturename"] + " (Instance)").Equals(mm));
                }
                var tempText = textureBundle.LoadAssetAsync<Texture>(texture.Value["textureaddress"]);
                yield return tempText;
                tempmat.SetTexture("_NORMALMAP", tempText.asset as Texture);
            }
        }

        var uwrDance = UnityWebRequestAssetBundle.GetAssetBundle(jSON["animationurl"]);
        yield return uwrDance.SendWebRequest();
        danceBundle = DownloadHandlerAssetBundle.GetContent(uwrDance);
        int indexer = 0;

        foreach (var child in GetAllChilds(danceContent))
        {
            Destroy(child);
        }
        foreach(var dance in jSON["animatons"])
        {
            var danceAnim = danceBundle.LoadAssetAsync<RuntimeAnimatorController>(dance.Value["address"]);
            yield return danceAnim;
            
            var danceNew = Instantiate(dancePrefab);
            danceNew.transform.SetParent(danceContent.transform);
            danceNew.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
            danceNew.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(indexer * 220 + 110));
            danceNew.transform.Find("text").GetComponent<TextMeshProUGUI>().text = dance.Value["name"];
            danceNew.name = dance.Value["name"];
            danceNew.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedObj.GetComponent<Animator>().runtimeAnimatorController = danceAnim.asset as RuntimeAnimatorController;
            });
            
            indexer++;
        }
        
    }

    public List<GameObject> GetAllChilds(GameObject Go)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < Go.transform.childCount; i++)
        {
            list.Add(Go.transform.GetChild(i).gameObject);
        }
        return list;
    }
}
