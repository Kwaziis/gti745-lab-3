using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class Connection : MonoBehaviour
{
    public Rigidbody ballRB;
    WebSocket websocket;

    private bool isGrounded = true; // Variable pour suivre l'état de la balle (au sol ou non)

    // Start is called before the first frame update
    async void Start()
    {
        websocket = new WebSocket("ws://localhost:8080");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Debug.Log("OnMessage!");
            // Debug.Log(bytes);

            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);

            if (!string.IsNullOrEmpty(message))
            {
                switch (message)
                {
                    case "HAUT":
                        ballRB.AddForce(Vector3.forward * 10);
                        break;
                    case "BAS":
                        ballRB.AddForce(Vector3.back * 10);
                        break;
                    case "GAUCHE":
                        ballRB.AddForce(Vector3.left * 10);
                        break;
                    case "DROITE":
                        ballRB.AddForce(Vector3.right * 10);
                        break;
                    case "STOP":
                        ballRB.velocity *= 0.5f;
                        break;
                    case "JUMP":
                        Jump();
                        break;
                    default:
                        Debug.Log("VIDE dectected");
                        break;
                }
            }
        };

        // Keep sending messages at every 0.3s
        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
    }

    // Méthode pour faire sauter la balle si elle est au sol
    void Jump()
    {
        if (isGrounded)
        {
            ballRB.AddForce(Vector3.up * 3, ForceMode.Impulse); // Applique une force vers le haut à la balle
            isGrounded = false; // Met à jour l'état de la balle (sautant)
        }
    }

    // Méthode pour détecter lorsque la balle touche le sol
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // Met à jour l'état de la balle (au sol)
        }
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
        #endif
    }

    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending bytes
            await websocket.Send(new byte[] { 10, 20, 30 });

            // Sending plain text
            await websocket.SendText("plain text message");
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
