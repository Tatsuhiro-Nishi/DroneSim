using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using UnityEngine;
using Debug = UnityEngine.Debug;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

/// <summary>
/// �w�肳�ꂽ�J�����̓��e���L���v�`������T���v��
/// </summary>
public class camera: MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private float next_time=0.0f;
    [SerializeField] private float interval_time = 0.5f;
    private string pyExePath = @"C:/Users/Tatsuhiro Nishi/anaconda3/python.exe";
    [SerializeField] private string myPythonApp = @"C:/Users/Public/Documents/test.py";

    private void Update()
    {
        if (Time.time >= next_time)
        {
            Vector3 pos = GameObject.Find("Cube").transform.position;
            Vector3 rot = GameObject.Find("Cube").transform.localEulerAngles;

            Debug.Log(pos + ":" + rot);
            next_time = Time.time + interval_time;
            byte[] data = shoot_SS();

            //sendMsg_py();
            socket_send();
            //Debug.Log("saving" + Application.dataPath + "/SavedScreen.png");
            //File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", data);
        }
    }

    public void sendMsg_py()
    {
        int x = 2;
        int y = 5;

        //�O���v���Z�X�̐ݒ�
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

    private void socket_send()
    {
        /*IPEndPoint remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1900);
        var message = Encoding.UTF8.GetBytes("Hello world !");

        UdpClient client = new UdpClient(5353);
        client.Connect(remote);
        client.Send(message, message.Length);
        client.Close();*/
        Debug.Log("sended");
        Task.Delay(50000);
        var Client = new UdpClient(1900);                           // UdpClient�쐬�i�|�[�g�ԍ��͓K���Ɋ����j
        var RequestData = Encoding.UTF8.GetBytes("Request");   // �K���ȃ��N�G�X�g�f�[�^
        var ServerEp = new IPEndPoint(IPAddress.Any, 0);        // �T�[�o�i�ʐM����j�̃G���h�|�C���gServerEp�쐬�iIP/Port���w��j

        Client.EnableBroadcast = true;                          // �u���[�h�L���X�g�L����
        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8080)); // �|�[�g8888�Ƀu���[�h�L���X�g���M
        Client.Close();
    }
}
