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
using UniRx;
using UnityEngine.UI;

public class Move : MonoBehaviour
{
	//Objectを動かすスピードを4にする。
	//このスピードはpublicで定義してあるからInspectorで変更できるよ。
	public float speed = 4.0f;
	Rigidbody rb;
	string waterTag = "water";
	public float waterDrag = 5000.0f;
	public float defaultDrag = 5.0f;

	private UdpClient Server;
	private byte[] ResponseData;         // 適当なレスポンスデータ
	private IPEndPoint ClientEp;                    // クライアント（通信相手）のエンドポイントClientEp作成（IP/Port未指定）
	private Subject<string> subject = new Subject<string>();
	[SerializeField] Text message;

	void Start()
	{
		rb = gameObject.GetComponent<Rigidbody>();
		defaultDrag = rb.drag;
		//ListenMessage();

		Server = new UdpClient(8080);
		Server.BeginReceive(OnReceived, Server);
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
		var Server = new UdpClient(8080);                                       // 待ち受けポートを指定してUdpClient生成
		var ResponseData = Encoding.ASCII.GetBytes("SomeResponseData");         // 適当なレスポンスデータ
		var ClientEp = new IPEndPoint(IPAddress.Any, 0);                    // クライアント（通信相手）のエンドポイントClientEp作成（IP/Port未指定）
		Server.Client.ReceiveTimeout = 10000;
		var ClientRequestData = Server.Receive(ref ClientEp);               // クライアントからのパケット受信、ClientEpにクライアントのエンドポイント情報が入る
		var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

		Debug.Log("Recived " + ClientRequest+ "from " + ClientEp.Address.ToString()+", sending response");    // ClientEp.Address：クライアントIP
	}

	private void OnReceived(System.IAsyncResult result)
	{
		UdpClient getUdp = (UdpClient)result.AsyncState;
		IPEndPoint ipEnd = null;

		byte[] getByte = getUdp.EndReceive(result, ref ipEnd);
		var message = Encoding.UTF8.GetString(getByte);
		Control(message);
		//subject.OnNext(message);
		getUdp.BeginReceive(OnReceived, getUdp);
	}

	private void Control(String message)
    {
		Debug.Log("message " + message);

    }

	private void OnDestroy()
	{
		Server.Close();
	}
}