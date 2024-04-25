﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using SpookSuite.Util;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.UI;

namespace SpookSuite.Menu.Game
{
    public class MainMenuAddon : MonoBehaviour
    {
        public Button joinFriendBtn;
        public Button joinRandomBtn;
        public Button viewLobbiesBtn;

        private Transform page;
        private Canvas canvas;
        private MainMenuMainPage main;


        private void Awake()
        {
            canvas = FindObjectOfType<Canvas>();
            page = GameObject.Find("MainPage").transform;
            main = FindObjectOfType<MainMenuMainPage>();
            
            CreateButtons();
            joinFriendBtn.onClick.AddListener(new UnityAction(this.OnJoinFriendButtonClicked));
            joinRandomBtn.onClick.AddListener(new UnityAction(this.OnJoinRandomButtonClicked));
            viewLobbiesBtn.onClick.AddListener(new UnityAction(this.OnViewLobbiesButtonClicked));

        }

        private void Start()
        {
            joinFriendBtn.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Join Friend";
        }

        private void CreateButtons()
        {
            viewLobbiesBtn = CreateBtnCopy("viewLobbiesBtn", "View Lobbies", 450).GetComponent<Button>();
            joinFriendBtn = CreateBtnCopy("JoinFriendBtn", "Join Friend", 375).GetComponent<Button>();
            joinRandomBtn = CreateBtnCopy("joinRandomBtn", "Join Random Private", 300).GetComponent<Button>();
        }

        private GameObject CreateBtnCopy(string name, string btnText, float y)
        {
            GameObject go = Instantiate(main.quitButton.gameObject, page);
            Destroy(go.GetComponentInChildren<GameObjectLocalizer>());

            go.name = name;
            RectTransform rt = go.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(-325, y);
            rt.anchorMax = new Vector2(1, 0);
            rt.anchorMin = new Vector2(1, 0);
            rt.sizeDelta = new Vector2(500f, 50);
            rt.localScale = new Vector3(1, 1, 1);
            rt.rotation = Quaternion.identity;

            go.GetComponentInChildren<TextMeshProUGUI>().text = btnText;


            return go;
        }

        private void OnJoinFriendButtonClicked()
        {
            List<CSteamID> lobbies = new List<CSteamID>();

            for(int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++)
            {
                CSteamID friend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                SteamFriends.GetFriendGamePlayed(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll), out var info);

                if (info.m_gameID.m_GameID == 2881650 && info.m_steamIDLobby.IsValid())
                    lobbies.Add(info.m_steamIDLobby);
            }

            if(lobbies.Count == 0)
            {
                Modal.ShowError("No Friend Lobbies Found!", "Failed to find any valid joinable friend lobbies. Please try again.");
                return;
            }

            MainMenuHandler.SteamLobbyHandler.Reflect().Invoke("JoinLobby", lobbies[Random.Range(0, lobbies.Count)]);
        }

        private void OnJoinRandomButtonClicked() => SpookSuite.Instance.StartCoroutine(JoinRandomPrivateGame());
        private void OnViewLobbiesButtonClicked()
        {
            Modal.ShowError("Feature Coming Soon!", "This feature is currently in development and will be available in a future update.");
        }

        internal IEnumerator JoinRandomPrivateGame()
        {
            Debug.Log("Joining Random Private Photon Room");

            //Hosting a game, Allows me to kick myself from the photon room, the game then bugs out and places me in a random what seems to be in progress, full, or private lobbies or all 3
            MainMenuHandler.Instance.Host(1);

            //lets wait for the hosted lobby
            yield return new WaitForSeconds(4);

            Debug.Log("Self Kicking / going to private lobby");
            //enable kicking in the photon room (this is local even if you are master client thus why we host our own lobby)
            PhotonNetwork.EnableCloseConnection = true;

            //Send the kick notification to myself (with it enabled and being the master as it does check if the request is sent by the master)
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions()
            {
                TargetActors = new int[1] { PhotonNetwork.LocalPlayer.ActorNumber }
            };
            PhotonNetwork.NetworkingClient.OpRaiseEvent((byte)203, (object)null, raiseEventOptions, SendOptions.SendReliable);

            //Waiting for the kick to happen and be randomly sent to another lobby, this takes a few seconds
            yield return new WaitForSeconds(5);

            Debug.Log("Setting Nickname");
            //change the nicckname because this is sketch af
            PhotonNetwork.NickName = "uhhhh, Hello?";

            Debug.Log("Load Factory");
            //You gotta force load the factory to be able to move and not lag, this will spawn a bunch of clones everyone can see, but sometimes you need to do this and will be alone
            //in the diving bell because they are already in the factory.
            SurfaceNetworkHandler.Instance.photonView.RPC("RPC_LoadScene", RpcTarget.All, (object)"FactoryScene");
        }
    }
}
