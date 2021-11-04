/*
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Collections;
using System.IO;
using UnityEngine;
#if UNITY_2018_4_OR_NEWER
using UnityEngine.Networking;
#endif
using UnityEngine.UI;

public class WebViewHandler : MonoBehaviour
{
    private enum enumWebPage
    {
        medical_1, medical_2, industrial_1, industrial_2
    }
    [SerializeField]
    private enumWebPage CurrentPage;
    private const string BASE_URL = "https://victorzhanit.neocities.org/ARSOP";
    private string Url;


    [SerializeField]
    private int Margin_Left, Margin_Top, Margin_Right, Margin_Bottom;

    [SerializeField, Space(10), Header("Not Necessary¡õ")]
    private Text status;
    [SerializeField]
    private Button btnBack, btnForward, btnRefresh, btnSwitch, btnGetCookie, btnClearCookie;
    [SerializeField]
    private Button btnMedical_1, btnMedical_2, btnIndustrial_1, btnIndustrial_2;

    private WebViewObject webViewObject;

    IEnumerator Start()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        BindingButtonOnClick();
         
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log(string.Format("CallFromJS[{0}]", msg));
                if(status) status.text = msg;
            },
            err: (msg) =>
            {
                Debug.Log(string.Format("CallOnError[{0}]", msg));
                if (status) status.text = msg;
            },
            httpErr: (msg) =>
            {
                Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                if (status) status.text = msg;
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
                // NOTE: depending on the situation, you might prefer
                // the 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
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
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
        webViewObject.SetMargins(Margin_Left, Margin_Top, Margin_Right, Margin_Bottom);
        webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
        webViewObject.SetVisibility(true);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        Url = Path.Combine(BASE_URL, CurrentPage.ToString() + ".html");
        Debug.Log($"Url: {Url}");

        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
            foreach (var ext in exts)
            {
                var url = Url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                byte[] result = null;
                if (src.Contains("://"))
                {  // for Android
                    var unityWebRequest = UnityWebRequest.Get(src);
                    yield return unityWebRequest.SendWebRequest();
                    result = unityWebRequest.downloadHandler.data;
                }
                else
                {
                    result = System.IO.File.ReadAllBytes(src);
                }
                System.IO.File.WriteAllBytes(dst, result);
                if (ext == ".html")
                {
                    webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                    break;
                }
            }
        }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
        yield break;
    }

    [ContextMenu("Clear All UI Reference")]
    private void ClearAllUI()
    {
        btnBack = btnForward = btnRefresh = btnSwitch = btnGetCookie = btnClearCookie = null;
        status = null;
    }
    private void BindingButtonOnClick()
    {
        btnRefresh?.onClick.AddListener(() => webViewObject.Reload());
        btnSwitch?.onClick.AddListener(delegate {
            ConnectURL();
        });
        btnGetCookie?.onClick.AddListener(() => webViewObject.GetCookies(Url));
        btnClearCookie?.onClick.AddListener(() => webViewObject.ClearCookies());
        
        if(btnBack != null)
        { 
            btnBack.enabled = webViewObject.CanGoBack();
            btnBack.onClick.AddListener(() => webViewObject.GoBack());
        }

        if (btnForward != null)
        {
            btnForward.enabled = webViewObject.CanGoForward();
            btnForward?.onClick.AddListener(() => webViewObject.GoForward());
        }

        btnMedical_1?.onClick.AddListener(delegate {
            CurrentPage = enumWebPage.medical_1;
            ConnectURL();
        });
        btnMedical_2?.onClick.AddListener(delegate {
            CurrentPage = enumWebPage.medical_2;
            ConnectURL();
        });
        btnIndustrial_1?.onClick.AddListener(delegate {
            CurrentPage = enumWebPage.industrial_1;
            ConnectURL();
        });
        btnIndustrial_2?.onClick.AddListener(delegate {
            CurrentPage = enumWebPage.industrial_2;
            ConnectURL();
        });
    }

    private void ConnectURL()
    {
        if (webViewObject != null) Destroy(webViewObject);
        StartCoroutine(Start());
    }

    /*
        void OnGUI()
        {
            var x = 10;

            GUI.enabled = webViewObject.CanGoBack();
            if (GUI.Button(new Rect(x, 10, 80, 80), "<"))
            {
                webViewObject.GoBack();
            }
            GUI.enabled = true;
            x += 90;

            GUI.enabled = webViewObject.CanGoForward();
            if (GUI.Button(new Rect(x, 10, 80, 80), ">"))
            {
                webViewObject.GoForward();
            }
            GUI.enabled = true;
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "R"))
            {
                webViewObject.Reload();
            }
            x += 90;

            GUI.TextField(new Rect(x, 10, 180, 80), "" + webViewObject.Progress());
            x += 190;

            if (GUI.Button(new Rect(x, 10, 80, 80), "S"))
            {
                if (webViewObject != null)
                {
                    Destroy(webViewObject);
                }
                else
                {
                    StartCoroutine(Start());
                }
            }
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "C"))
            {
                Debug.Log(webViewObject.GetCookies(Url));
            }
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "X"))
            {
                webViewObject.ClearCookies();
            }
            x += 90;
        }*/
}
