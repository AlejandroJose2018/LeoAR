using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterManager : MonoBehaviour
{
    public RuntimeAnimatorController Ani;
    public string url, texture;

    public void DownloadCharacter()
    {
        StartCoroutine(DownCor(url, texture));
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
