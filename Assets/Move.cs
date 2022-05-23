using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
			transform.Rotate(0f, 5f, 0f, Space.Self);
		}
		//これは左。
		if (Input.GetKey("left"))
		{
			//transform.position -= transform.right * speed * Time.deltaTime;
			transform.Rotate(0f, -5f, 0f, Space.Self);
		}
		ROVmove();
		//Error_Measure();
	}

	private void ROVmove()
	{

		string[] message_arr = message.Split(',');
		float theta = float.Parse(message_arr[0]);
		Debug.Log("theta " + message_arr[0]);
		Debug.Log("recv_time " + DateTimeOffset.Now.ToUnixTimeSeconds());
		if (Math.Abs(theta) <= 360)
		{
			if (Math.Abs(theta) < 5)
			{
				transform.Translate(0f, 0f, 0.015f);
				Debug.Log("go");
			}

			else if (theta < 10 && theta >= 5)
			{
				transform.Rotate(0f, -0.1f, 0f, Space.Self);
				Debug.Log("left turn");
			}
			else if (theta > -10 && theta <= -5)
			{
				transform.Rotate(0f, 0.1f, 0f, Space.Self);
				Debug.Log("right turn");
			}

			else if (theta <= 180 && theta >= 10)
			{
				transform.Rotate(0f, -0.4f, 0f, Space.Self);
				Debug.Log("left turn");
			}
			else if (theta > -180 && theta <= -10)
			{
				transform.Rotate(0f, 0.4f, 0f, Space.Self);
				Debug.Log("right turn");
			}
			
		}
		if (theta == -999) { EditorApplication.Beep(); }
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