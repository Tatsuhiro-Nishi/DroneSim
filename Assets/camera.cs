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
/// �w�肳�ꂽ�J�����̓��e���L���v�`������T���v��
/// </summary>
public class camera: MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private float next_time=0.0f;
    [SerializeField] private float interval_time = 0f;

    private void Update()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();  //Stopwatch�̃I�u�W�F�N�g�쐬
        if (Time.time >= next_time)
        {
            next_time = Time.time + interval_time;

            Vector3 pos = GameObject.Find("Cube").transform.position; //GameObj Cube�̈ʒu���擾
            Vector3 rot = GameObject.Find("Cube").transform.localEulerAngles; //GameObj Cube�̌������擾

            // �J��������̉摜�̃o�C�g�z����擾���鎞�Ԃ��v��
            // �������̎g�p�ʂ�MAX�ɂȂ�\��������̂�30ms�̒x���ő�p
            sw.Start();�@
            Thread.Sleep(30);
            //byte[] data = shoot_SS();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            Debug.Log(ts);
            /////////////////////////////////////////////////////////////

            string message = pos.x + "," + pos.y + "," + pos.z + "," + rot.y;
            socket_send(message);
            //File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", data); //�J��������̉摜��ۑ�
        }
    }

    /// <summary>
    /// Python Script�̎��s��@(���g�p�A�����Ɏg���邩���Ȃ�ňꉞ�c���Ă���)
    /// </summary>

    private string pyExePath = @"C:/Users/Tatsuhiro Nishi/anaconda3/python.exe";
    [SerializeField] private string myPythonApp = @"C:/Users/Public/Documents/test.py";

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

/*---------Camera�̎B�e���Ă���摜�̎擾-----------*/
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
        var Client = new UdpClient(1900);                           // UdpClient�쐬�A�|�[�g�ԍ��͎��g�̃|�[�g
        var RequestData = Encoding.UTF8.GetBytes(message);   // �ʒu���܂��͉摜�̑��M

        // �|�[�g8080�ɑ��M
        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse("192.168.120.3"), 8080)); //�����[�g�ɑ��M
        //Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080)); //���[�J���ɑ��M
        Client.Close(); //socket�����
    }
}
