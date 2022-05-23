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

    private void Update()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();  //Stopwatchのオブジェクト作成
        if (Time.time >= next_time)
        {
            next_time = Time.time + interval_time;

            Vector3 pos = GameObject.Find("Cube").transform.position; //GameObj Cubeの位置を取得
            Vector3 rot = GameObject.Find("Cube").transform.localEulerAngles; //GameObj Cubeの向きを取得

            // カメラからの画像のバイト配列を取得する時間を計測
            // メモリの使用量がMAXになる可能性があるので30msの遅延で代用
            sw.Start();　
            Thread.Sleep(30);
            //byte[] data = shoot_SS();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            Debug.Log(ts);
            /////////////////////////////////////////////////////////////

            string message = pos.x + "," + pos.y + "," + pos.z + "," + rot.y;
            socket_send(message);
            //File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", data); //カメラからの画像を保存
        }
    }

    /// <summary>
    /// Python Scriptの実行手法(未使用、何かに使えるかもなんで一応残しておく)
    /// </summary>

    private string pyExePath = @"C:/Users/Tatsuhiro Nishi/anaconda3/python.exe";
    [SerializeField] private string myPythonApp = @"C:/Users/Public/Documents/test.py";

    public void sendMsg_py()
    {
        int x = 2;
        int y = 5;

        //外部プロセスの設定
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            FileName = pyExePath, 
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true, 
            Arguments = myPythonApp + " " + x + " " + y, 
        };
        Process process = Process.Start(processStartInfo);
        StreamReader streamReader = process.StandardOutput;
        process.WaitForExit();
        string str = streamReader.ReadLine();
        process.Close();
        Debug.Log("Msg" + str);
    }

/*---------Cameraの撮影している画像の取得-----------*/
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
        var Client = new UdpClient(1900);                           // UdpClient作成、ポート番号は自身のポート
        var RequestData = Encoding.UTF8.GetBytes(message);   // 位置情報または画像の送信

        // ポート8080に送信
        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse("192.168.120.3"), 8080)); //リモートに送信
        //Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080)); //ローカルに送信
        Client.Close(); //socketを閉じる
    }
}
