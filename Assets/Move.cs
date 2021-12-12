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
	public string message;

	private UdpClient Server;
	private byte[] ResponseData;         // 適当なレスポンスデータ
	private IPEndPoint ClientEp;                    // クライアント（通信相手）のエンドポイントClientEp作成（IP/Port未指定）
	private Subject<string> subject = new Subject<string>();

	void Start()
	{
		rb = gameObject.GetComponent<Rigidbody>();
		defaultDrag = rb.drag;
		//ListenMessage();

		Server = new UdpClient(1901);
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

		ROVmove();
		
	}

	private void ROVmove()
	{
		float theta = float.Parse(message);

		if (Math.Abs(theta) <= 360)
		{
			if (Math.Abs(theta) < 15)
			{
				transform.Translate(0f, 0f, 0.01f);
			}

			else if (theta > 15)
			{
				transform.Rotate(0f, -0.5f, 0f);
				Debug.Log("theta > 15");
			}
			else if (theta < -15)
			{
				transform.Rotate(0f, 0.5f, 0f);
				Debug.Log("theta < -15");
			}
			//message = "-999";
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

	private void OnReceived(System.IAsyncResult result)
	{
		UdpClient getUdp = (UdpClient)result.AsyncState;
		IPEndPoint ipEnd = null;

		byte[] getByte = getUdp.EndReceive(result, ref ipEnd);
		message = Encoding.UTF8.GetString(getByte);
		Debug.Log("message " + message);
		subject.OnNext(message);
		getUdp.BeginReceive(OnReceived, getUdp);
	}

	private void OnDestroy()
	{
		Server.Close();
	}
}