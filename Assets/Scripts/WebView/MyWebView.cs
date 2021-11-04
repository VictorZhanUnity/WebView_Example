using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyWebView : MonoBehaviour
{
    [SerializeField]
    private Button btnBack, btnForward, btnRefresh, btnSwitch, btnGetCookie, btnClearCookie;
    
    private WebViewObject webViewObject;

    private string url;

    private void Start()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log(string.Format("CallFromJS[{0}]", msg));
            },
            err: (msg) =>
            {
                Debug.Log(string.Format("CallOnError[{0}]", msg));
            },
            httpErr: (msg) =>
            {
                Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
            },
            started: (msg) =>
            {
                Debug.Log(string.Format("CallOnStarted[{0}]", msg));
            },
            hooked: (msg) =>
            {
                Debug.Log(string.Format("CallOnHooked[{0}]", msg));
            },
            ld: (msg) =>
            {
                Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || (!UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_WEBGL)
#if true
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                webViewObject.EvaluateJS(
                    "window.Unity = {" +
                    "   call:function(msg) {" +
                    "       parent.unityWebView.sendMessage('WebViewObject', msg)" +
                    "   }" +
                    "};");
#endif
                webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
            }
            );
        webViewObject.SetMargins(1300, 0, 200, 0);
        webViewObject.SetTextZoom(100);  
        webViewObject.SetVisibility(true);

        BindingUIButton();
    }

    public void Load_URL1()
    {
        LoadURL("https://www.youtube.com");
    }
    public void Load_URL2()
    {
        LoadURL("https://www.goole.com");
    }

    private void LoadURL(string value)
    {
        url = value;
        webViewObject.LoadURL(url.Replace(" ", "%20"));
    }

    private void BindingUIButton()
    {
        btnRefresh?.onClick.AddListener(() => webViewObject.Reload());
        btnSwitch?.onClick.AddListener(delegate {
            LoadURL(url);
        });
        btnGetCookie?.onClick.AddListener(() => webViewObject.GetCookies(url));
        btnClearCookie?.onClick.AddListener(() => webViewObject.ClearCookies());
        btnBack?.onClick.AddListener(() => webViewObject.GoBack());
        btnForward?.onClick.AddListener(() => webViewObject.GoForward());
    }

    private void Update()
    {
        if (btnBack != null)
        {
            Debug.Log("webViewObject: " + webViewObject.CanGoBack() + "/" + webViewObject.CanGoForward());
            btnBack.enabled = webViewObject.CanGoBack();
        }

        if (btnForward != null)
        {
            btnForward.enabled = webViewObject.CanGoForward();
        }
        btnBack.enabled = false;
    }
}
