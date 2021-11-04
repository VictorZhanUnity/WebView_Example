using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebViewManager : MonoBehaviour
{
    [SerializeField]
    private int Margin_Left, Margin_Top, Margin_Right, Margin_Bottom;
    [SerializeField]
    private string url;

    private WebViewObject webViewObject;

    void Start()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();

        webViewObject.Init(
            cb: (msg) => { Debug.Log(string.Format("CallFromJS[{0}]", msg)); },
            err: (msg) => { Debug.Log(string.Format("CallOnError[{0}]", msg)); },
            httpErr: (msg) => { Debug.Log(string.Format("CallOnHttpError[{0}]", msg)); },
            started: (msg) => { Debug.Log(string.Format("CallOnStarted[{0}]", msg)); },
            hooked: (msg) => { Debug.Log(string.Format("CallOnHooked[{0}]", msg)); },
            ld: (msg) => {
                Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
                webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
            });
        webViewObject.SetMargins(Margin_Left, Margin_Top, Margin_Right, Margin_Bottom);
        webViewObject.SetTextZoom(100);
        webViewObject.SetVisibility(true);

        LoadWebPage(url);
    }
    public void LoadWebPage(string webUrl)
    {
        url = webUrl;
        webViewObject.LoadURL(url.Replace(" ", "%20"));
    }

    public void Reload() => webViewObject.Reload();
    public void GetCookies(string url) => webViewObject.GetCookies(url);
    public void ClearCookies() => webViewObject.ClearCookies();
    public bool CanGoBack() => webViewObject.CanGoBack();
    public bool CanGoForward() => webViewObject.CanGoForward();
    public void GoBack() => webViewObject.GoBack();
    public void GoForward() => webViewObject.GoForward();
}
