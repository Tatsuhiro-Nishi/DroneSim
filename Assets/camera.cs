using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Threading;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

/// <summary>
/// 指定されたカメラの内容をキャプチャするサンプル
/// </summary>
public class camera: MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private float next_time=0.0f;
    [SerializeField] private float interval_time = 0f;
    private string pyExePath = @"C:/Users/Tatsuhiro Nishi/anaconda3/python.exe";
    [SerializeField] private string myPythonApp = @"C:/Users/Public/Documents/test.py";

    private void Update()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        if (Time.time >= next_time)
        {
            Vector3 pos = GameObject.Find("Cube").transform.position;
            Vector3 rot = GameObject.Find("Cube").transform.localEulerAngles;
            var direction = transform.forward;

            Vector3 buoy_pos = GameObject.Find("camera").transform.position;

            //Debug.Log(direction);
            next_time = Time.time + interval_time;

            sw.Start();
            Thread.Sleep(30);
            //byte[] data = shoot_SS();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;

            double buoy2pos = Math.Sqrt(Math.Pow(pos.x - buoy_pos.x, 2) 
                                     + Math.Pow(pos.y - buoy_pos.y, 2) 
                                     + Math.Pow(pos.z - buoy_pos.z, 2));
            Debug.Log("buoy2pos : " + buoy2pos);

            System.Random myRand = new System.Random();
            double[] ErrorRate = new double[] { myRand.NextDouble(), myRand.NextDouble(), myRand.NextDouble() };
            double[] Acoustic_pos = new double[3];
            Acoustic_pos[0] = pos.x + (buoy2pos * ErrorRate[1] * 0.01);
            Acoustic_pos[1] = pos.y + (buoy2pos * ErrorRate[2] * 0.01);
            Acoustic_pos[2] = pos.z + (buoy2pos * ErrorRate[0] * 0.01);

            Debug.Log("Now pos" + Acoustic_pos[0] + ", " + Acoustic_pos[1] + ", " + Acoustic_pos[2] + ", ");

            string message = Acoustic_pos[0] + "," + Acoustic_pos[1] + "," + Acoustic_pos[2] + "," + rot.y;
            socket_send(message);
            //File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", data);
        }
    }

    public byte[] shoot_SS()
    {
        var rt = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 24);
        var prev = _camera.targetTexture;
        _camera.targetTexture = rt;
        _camera.Render();
        _camera.targetTexture = prev;
        RenderTexture.active = rt;

        var screenShot = new Texture2D(
            _camera.pixelWidth,
            _camera.pixelHeight,
            TextureFormat.RGB24,
            false);
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        var bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);
        return bytes;
    }

    private void socket_send(string message)
    {
        //Debug.Log("delaytime send : " + delaytime);
        //Thread.Sleep(delaytime);
        var Client = new UdpClient(1900);                           // UdpClient作成（ポート番号は適当に割当）
        var RequestData = Encoding.UTF8.GetBytes(message);   // 適当なリクエストデータ
        //var ServerEp = new IPEndPoint(IPAddress.Any, 0);        // サーバ（通信相手）のエンドポイントServerEp作成（IP/Port未指定）

        //Client.EnableBroadcast = true;                          // ブロードキャスト有効化

        // ポート8080に送信
        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse("192.168.120.3"), 8080));
        //Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
        Client.Close();
    }
}
