using System.Collections;
using System.Collections.Generic;
//����̓��using�͎g��Ȃ�����{���͂���Ȃ���B
using UnityEngine;
//Move�ƌ����N���X�ŏ����Ă���
using Debug = UnityEngine.Debug;

using System;
using System.Diagnostics;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

public class Move : MonoBehaviour
{
	//Object�𓮂����X�s�[�h��4�ɂ���B
	//���̃X�s�[�h��public�Œ�`���Ă��邩��Inspector�ŕύX�ł����B
	public float speed = 4.0f;
	Rigidbody rb;
	string waterTag = "water";
	public float waterDrag = 5000.0f;
	public float defaultDrag = 5.0f;

	void Start()
	{
		rb = gameObject.GetComponent<Rigidbody>();
		defaultDrag = rb.drag;
	}

	// Update is called once per frame
	void Update()
	{
		//Unity���ɃC���v�b�g����Ă���up���g���Ė��̏�������Ƃ��̕����ɓ�����B
		if (Input.GetKey("up"))
		{
			//�ꏊ�͉������������O�ɐi��Řb���Ǝ~�܂��B
			transform.position += transform.forward * speed * Time.deltaTime;
		}
		//����͉��B
		if (Input.GetKey("down"))
		{
			transform.position -= transform.forward * speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.Space))
        {
			transform.position += transform.up * speed * Time.deltaTime;
		}

		//����͉E�B
		if (Input.GetKey("right"))
		{
			//transform.position += transform.right * speed * Time.deltaTime;
			transform.Rotate(0f, 0.5f, 0f);
		}
		//����͍��B
		if (Input.GetKey("left"))
		{
			//transform.position -= transform.right * speed * Time.deltaTime;
			transform.Rotate(0f, -0.5f, 0f);
		}
		//ListenMsg_py();
		ListenMessage();
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == waterTag)
		{
			// ���̒��̒�R���Z�b�g
			rb.drag = waterDrag;
			//speed = 2.0f;
			GetComponent<Renderer>().material.color = Color.red;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.name == waterTag)
		{
			// �ʏ�̒�R���Z�b�g
			rb.drag = defaultDrag;
			//speed = 4.0f;
			GetComponent<Renderer>().material.color = Color.green;
		}
	}

	public void ListenMsg_py()
    {
		string myPythonApp = "test.py";
		int x = 2;
		int y = 5;

		var myProcess = new Process
		{
			StartInfo = new ProcessStartInfo("python.exe")
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				Arguments = myPythonApp + " " + x + " " + y
			}
		};

		myProcess.Start();
		StreamReader myStreamReader = myProcess.StandardOutput;
		string myString = myStreamReader.ReadLine();
		Debug.Log("Msg" + myString);
		myProcess.WaitForExit();
		myProcess.Close();
	}


	public void ListenMessage()
    {
        byte[] bytes = new byte[256];
		// �ڑ��\�P�b�g�̏���
		//IPEndPoint local = new IPEndPoint(IPAddress.Any, 1900);
		//IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
		//UdpClient Server = new UdpClient(8888);

		//var data = new byte[4];
		//client.Receive(data, 4, SocketFlags.None);

		//var ClientEp = new IPEndPoint(IPAddress.Any, 0);                    // �N���C�A���g�i�ʐM����j�̃G���h�|�C���gClientEp�쐬�iIP/Port���w��j
		//var ClientRequestData = Server.Receive(ref ClientEp);               // �N���C�A���g����̃p�P�b�g��M�AClientEp�ɃN���C�A���g�̃G���h�|�C���g��񂪓���
		//var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);
		//var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		//server.Bind(new IPEndPoint(IPAddress.Any, 10408));

		// �f�[�^��M�ҋ@
		//var result = client.Receive(bytes);
		//byte[] receiveBytes = client.Receive(ref remote);

		// ��M�����f�[�^��ϊ�
		//var data = Encoding.UTF8.GetString(result.Buffer);

		//Debug.Log(receiveBytes);
		// Receive �C�x���g �����s
		//this.OnRecieve(data);

		var Server = new UdpClient(8000);                                       // �҂��󂯃|�[�g���w�肵��UdpClient����
		var ResponseData = Encoding.ASCII.GetBytes("SomeResponseData");         // �K���ȃ��X�|���X�f�[�^
		var ClientEp = new IPEndPoint(IPAddress.Any, 0);                    // �N���C�A���g�i�ʐM����j�̃G���h�|�C���gClientEp�쐬�iIP/Port���w��j
		Server.Client.ReceiveTimeout = 1000;
		var ClientRequestData = Server.Receive(ref ClientEp);               // �N���C�A���g����̃p�P�b�g��M�AClientEp�ɃN���C�A���g�̃G���h�|�C���g��񂪓���
		var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

		Debug.Log("Recived " + ClientRequest+ "from " + ClientEp.Address.ToString()+", sending response");    // ClientEp.Address�F�N���C�A���gIP
	}
}