using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera desktopCamera;  // Camera cho desktop
    public Camera mobileCamera;   // Camera cho mobile

    void Start()
    {
        // Kiểm tra xem game có đang chạy trên di động hay không
        if (Application.isMobilePlatform)
        {
            // Nếu đang trên mobile, bật camera mobile và tắt camera desktop
            mobileCamera.gameObject.SetActive(true);
            desktopCamera.gameObject.SetActive(false);
        }
        else
        {
            // Nếu không phải trên mobile (chạy trên PC), bật camera desktop và tắt camera mobile
            mobileCamera.gameObject.SetActive(false);
            desktopCamera.gameObject.SetActive(true);
        }
    }
}
