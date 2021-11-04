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

public class WebViewHandler_Fixed : MonoBehaviour
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

    void Start()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();

        webViewObject.Init(
            cb: (msg) =>{       Debug.Log(string.Format("CallFromJS[{0}]", msg));},
            err: (msg) =>{      Debug.Log(string.Format("CallOnError[{0}]", msg));},
            httpErr: (msg) =>{  Debug.Log(string.Format("CallOnHttpError[{0}]", msg));},
            started: (msg) =>{  Debug.Log(string.Format("CallOnStarted[{0}]", msg));},
            hooked: (msg) =>{   Debug.Log(string.Format("CallOnHooked[{0}]", msg));},
            ld: (msg) =>{   Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
                webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
            });
        webViewObject.SetMargins(Margin_Left, Margin_Top, Margin_Right, Margin_Bottom);
        webViewObject.SetTextZoom(100); 
        webViewObject.SetVisibility(true);

        Url = "https://victorzhanit.neocities.org/ARSOP/medical_2.html";
        webViewObject.LoadURL(Url.Replace(" ", "%20"));
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

        if (btnBack != null)
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
