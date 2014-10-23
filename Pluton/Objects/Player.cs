﻿using System;
using UnityEngine;

namespace Pluton
{
    public class Player
    {     
        public readonly BasePlayer basePlayer;

        public Player(BasePlayer player)
        {        
            basePlayer = player;
            try {
                Stats = new PlayerStats(SteamID);
            } catch (Exception ex) {
                Logger.LogDebug("[Player] Couldn't load stats!");
                Logger.LogException(ex);
            }
        }

        public static Player Find(string nameOrSteamidOrIP)
        {
            BasePlayer player = BasePlayer.Find(nameOrSteamidOrIP);
            if (player != null)
                return new Player(player);
            Logger.LogDebug("[Player] Couldn't find player!");
            return null;
        }

        public static Player FindByGameID(ulong steamID)
        {
            BasePlayer player = BasePlayer.FindByID(steamID);
            if (player != null)
                return new Player(player);
            Logger.LogDebug("[Player] Couldn't find player!");
            return null;
        }

        public static Player FindBySteamID(string steamID)
        {
            return FindByGameID(UInt64.Parse(steamID));
        }

        public void Ban(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.Banned, Name, reason);
            ServerUsers.Save();
            Kick("Banned!");
        }

        public void Kick(string reason = "no reason")
        {
            Network.Net.sv.Kick(basePlayer.net.connection, reason);
        }

        public void Reject(string reason = "no reason")
        {
            ConnectionAuth.Reject(basePlayer.net.connection, reason);
        }

        public Vector3 GetLookPoint(float maxDist = 500f)
        {
            RaycastHit hit;
            Ray orig = basePlayer.eyes.Ray();
            if (Physics.Raycast(orig, out hit, maxDist, Physics.AllLayers)) {
                return hit.point;
            }
            return Vector3.zero;
        }

        public void Kill()
        {
            var info = new HitInfo();
            info.damageType = Rust.DamageType.Suicide;
            info.Initiator = basePlayer as BaseEntity;
            basePlayer.Die(info);
        }

        public void MakeNone(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.None, Name, reason);
            basePlayer.net.connection.authLevel = 0;
            ServerUsers.Save();
        }

        public void MakeModerator(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.Moderator, Name, reason);
            basePlayer.net.connection.authLevel = 1;
            ServerUsers.Save();
        }

        public void MakeOwner(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.Owner, Name, reason);
            basePlayer.net.connection.authLevel = 2;
            ServerUsers.Save();
        }

        public void Message(string msg)
        {
            basePlayer.SendConsoleCommand("chat.add " + StringExtensions.QuoteSafe(Server.server_message_name) + " " + StringExtensions.QuoteSafe(msg));
        }

        public void MessageFrom(string from, string msg)
        {
            basePlayer.SendConsoleCommand("chat.add " + StringExtensions.QuoteSafe(from) + " " + StringExtensions.QuoteSafe(msg));
        }

        public void ConsoleMessage(string msg)
        {
            SendConsoleCommand("echo " + msg);
        }

        public void SendConsoleCommand(string cmd)
        {
            basePlayer.SendConsoleCommand(StringExtensions.QuoteSafe(cmd));
        }

        public void GroundTeleport(float x, float y, float z)
        {
            Teleport(x, World.GetWorld().GetGround(x, z), z);
        }
        
        public void GroundTeleport(Vector3 v3)
        {
            Teleport(v3.x, World.GetWorld().GetGround(v3.x, v3.z), v3.z);
        }

        public void Teleport(Vector3 v3)
        {
            Teleport(v3.x, v3.y, v3.z);
        }

        public static Vector3[] firstLocations = new Vector3[]{
            new Vector3(2000, 0, 2000),
            new Vector3(-2000, 0, 2000),
            new Vector3(2000, 0, -2000),
            new Vector3(-2000, 0, -2000)
        };

        public void Teleport(float x, float y, float z)
        {  
            Vector3 firstloc = Vector3.zero;
            foreach (Vector3 v3 in firstLocations) {
                if (Vector3.Distance(Location, v3) > 1000f && Vector3.Distance(new Vector3(x, y, z), v3) > 1000f) {
                    firstloc = v3;
                }
            }

            basePlayer.supressSnapshots = true;
            basePlayer.transform.position = firstloc;
            basePlayer.UpdateNetworkGroup();

            basePlayer.transform.position = new UnityEngine.Vector3(x, y, z);
            basePlayer.UpdateNetworkGroup();
            basePlayer.UpdatePlayerCollider(true, false);
            basePlayer.SendFullSnapshot();
            basePlayer.inventory.SendSnapshot();
        }

        public bool Admin {
            get {
                return basePlayer.IsAdmin();
            }
        }

        public string AuthStatus {
            get {
                return basePlayer.net.connection.authStatus;
            }
        }

        public ulong GameID {
            get {
                return basePlayer.userID;
            }
        }

        public float Health {
            get {
                return basePlayer.metabolism.health.value;
            }
            set {
                basePlayer.metabolism.health.Add(value);
            }
        }

        public Inv Inventory {
            get {
                return new Inv(basePlayer.inventory);
            }
        }

        public string IP {
            get {
                return basePlayer.net.connection.ipaddress;
            }
        }

        public Vector3 Location {
            get {
                return basePlayer.transform.position;
            }
            set {
                basePlayer.transform.position.Set(value.x, value.y, value.z);
            }
        }

        public bool Moderator {
            get {
                return ServerUsers.Is(GameID, ServerUsers.UserGroup.Moderator);
            }
        }

        public string Name {
            get {
                return basePlayer.displayName;
            }
        }

        public bool Owner {
            get {
                return ServerUsers.Is(GameID, ServerUsers.UserGroup.Owner);
            }
        }

        public string OS {
            get {
                return basePlayer.net.connection.os;
            }
        }

        public int Ping {
            get {
                return basePlayer.net.connection.ping;
            }
        }

        public PlayerStats Stats {
            get {
                return Server.GetServer().serverData.Get("PlayerStats", SteamID) as PlayerStats;
            }
            set {
                Server.GetServer().serverData.Add("PlayerStats", SteamID, value);
            }
        }

        public string SteamID {
            get {
                return basePlayer.userID.ToString();
            }
        }

        public float TimeOnline {
            get {
                return basePlayer.net.connection.connectionTime;
            }
        }

        public float X {
            get {
                return basePlayer.transform.position.x;
            }
            set {
                basePlayer.transform.position.Set(value, Y, Z);
            }
        }

        public float Y {
            get {
                return basePlayer.transform.position.y;
            }
            set {
                basePlayer.transform.position.Set(X, value, Z);
            }
        }

        public float Z {
            get {
                return basePlayer.transform.position.z;
            }
            set {
                basePlayer.transform.position.Set(X, Y, value);
            }
        }
    }
}

