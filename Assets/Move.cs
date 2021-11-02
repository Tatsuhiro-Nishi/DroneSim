using System.Collections;
using System.Collections.Generic;
//↑上の二つのusingは使わないから本当はいらないよ。
using UnityEngine;
//Moveと言うクラスで書いてくよ
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
	//Objectを動かすスピードを4にする。
	//このスピードはpublicで定義してあるからInspectorで変更できるよ。
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
		//Unity内にインプットされているupを使って矢印の上を押すとその方向に動くよ。
		if (Input.GetKey("up"))
		{
			//場所は押した分だけ前に進んで話すと止まるよ。
			transform.position += transform.forward * speed * Time.deltaTime;
		}
		//これは下。
		if (Input.GetKey("down"))
		{
			transform.position -= transform.forward * speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.Space))
        {
			transform.position += transform.up * speed * Time.deltaTime;
		}

		//これは右。
		if (Input.GetKey("right"))
		{
			//transform.position += transform.right * speed * Time.deltaTime;
			transform.Rotate(0f, 0.5f, 0f);
		}
		//これは左。
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
			// 水の中の抵抗をセット
			rb.drag = waterDrag;
			//speed = 2.0f;
			GetComponent<Renderer>().material.color = Color.red;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.name == waterTag)
		{
			// 通常の抵抗をセット
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
		// 接続ソケットの準備
		//IPEndPoint local = new IPEndPoint(IPAddress.Any, 1900);
		//IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
		//UdpClient Server = new UdpClient(8888);

		//var data = new byte[4];
		//client.Receive(data, 4, SocketFlags.None);

		//var ClientEp = new IPEndPoint(IPAddress.Any, 0);                    // クライアント（通信相手）のエンドポイントClientEp作成（IP/Port未指定）
		//var ClientRequestData = Server.Receive(ref ClientEp);               // クライアントからのパケット受信、ClientEpにクライアントのエンドポイント情報が入る
		//var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);
		//var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		//server.Bind(new IPEndPoint(IPAddress.Any, 10408));

		// データ受信待機
		//var result = client.Receive(bytes);
		//byte[] receiveBytes = client.Receive(ref remote);

		// 受信したデータを変換
		//var data = Encoding.UTF8.GetString(result.Buffer);

		//Debug.Log(receiveBytes);
		// Receive イベント を実行
		//this.OnRecieve(data);

		var Server = new UdpClient(8000);                                       // 待ち受けポートを指定してUdpClient生成
		var ResponseData = Encoding.ASCII.GetBytes("SomeResponseData");         // 適当なレスポンスデータ
		var ClientEp = new IPEndPoint(IPAddress.Any, 0);                    // クライアント（通信相手）のエンドポイントClientEp作成（IP/Port未指定）
		Server.Client.ReceiveTimeout = 1000;
		var ClientRequestData = Server.Receive(ref ClientEp);               // クライアントからのパケット受信、ClientEpにクライアントのエンドポイント情報が入る
		var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

		Debug.Log("Recived " + ClientRequest+ "from " + ClientEp.Address.ToString()+", sending response");    // ClientEp.Address：クライアントIP
	}
}